Shader "Custom/DataDissolution" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (0,0.5,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveScale ("Dissolve Scale", Range(0.1, 10)) = 1
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
        _VoxelSize ("Voxel Size", Range(0.001, 0.1)) = 0.01
        _VoxelHollow ("Voxel Hollow", Range(0, 0.5)) = 0.2
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1
    }
    
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite On
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma require samplelod
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _DissolveAmount;
            float _DissolveScale;
            float _GlitchIntensity;
            float _VoxelSize;
            float _VoxelHollow;
            float _EmissionStrength;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float dissolveNoise : TEXCOORD2;
            };
            
            // ���� �Լ� - ����ȭ
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // ������ �Լ� (���ؽ� ���̴��� ����)
            float noise(float2 co) {
                float2 scaledUV = co * _DissolveScale;
                float4 noiseColor = tex2Dlod(_NoiseTex, float4(scaledUV, 0, 0));
                return noiseColor.x; // .r ��� .x ���
            }
            
            // ����ȭ�� �۸�ġ ȿ��
            float3 applyGlitch(float3 pos, float2 uv, float intensity) {
                float time = _Time.y;
                float glitchX = sin(time * 20) * 0.02 * intensity;
                float glitchY = cos(time * 15) * 0.01 * intensity;
                
                pos.x += glitchX * sin(uv.y * 10);
                pos.y += glitchY * cos(uv.x * 8);
                
                return pos;
            }
            
            // ����ȭ�� ����ȭ �Լ�
            float3 voxelize(float3 pos) {
                return floor(pos / _VoxelSize + 0.5) * _VoxelSize;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                float3 localPos = v.vertex.xyz;
                float noiseVal = noise(v.uv);
                o.dissolveNoise = noiseVal;
                
                // ����ȭ ���� (���� ������ ����)
                if (_DissolveAmount > 0) {
                    localPos = voxelize(localPos);
                    
                    // ���� ȿ�� (��� �� �����)
                    float vertDissolve = saturate((_DissolveAmount * 2 - noiseVal) * 2);
                    
                    if (vertDissolve > 0) {
                        // ���� ���
                        localPos.y += vertDissolve * vertDissolve * 2;
                        
                        // x, z �������� �����
                        float dispX = (rand(v.uv + float2(0, _Time.y)) - 0.5) * 2;
                        float dispZ = (rand(v.uv + float2(_Time.y, 0)) - 0.5) * 2;
                        localPos.xz += float2(dispX, dispZ) * vertDissolve;
                    }
                    
                    // �۸�ġ ���� - �ܼ�ȭ�� ���� ���
                    if (_GlitchIntensity > 0) {
                        localPos = applyGlitch(localPos, v.uv, _GlitchIntensity * _DissolveAmount);
                    }
                }
                
                // ���ؽ� ��ġ ������Ʈ
                float4 modifiedVertex = float4(localPos, v.vertex.w);
                
                o.vertex = UnityObjectToClipPos(modifiedVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, modifiedVertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // �ؽ�ó ���ø�
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // ������ ����ũ ����
                float dissolveNoise = i.dissolveNoise;
                float dissolveMask = step(dissolveNoise, _DissolveAmount);
                
                // ���� �߱� ȿ��
                float edgeWidth = 0.05;
                float edgeMask = step(dissolveNoise - edgeWidth, _DissolveAmount) - dissolveMask;
                
                // �߱� ���� ����
                col.rgb = lerp(col.rgb, _GlowColor.rgb * _EmissionStrength, edgeMask);
                
                // ���� �κ� ���� ����
                col.a *= 1 - dissolveMask;
                
                // �����갡 50% �̻��� �� ���̾������� ȿ�� �߰�
                if (_DissolveAmount > 0.5) {
                    // ���� ���� ����
                    float3 voxPos = frac(i.worldPos / _VoxelSize);
                    float voxelEdge = 
                        step(voxPos.x, 0.05) + step(0.95, voxPos.x) + 
                        step(voxPos.y, 0.05) + step(0.95, voxPos.y) + 
                        step(voxPos.z, 0.05) + step(0.95, voxPos.z);
                    
                    // ���̾������� �߱� ����
                    if (voxelEdge > 0) {
                        col.rgb = lerp(col.rgb, _GlowColor.rgb * _EmissionStrength * 1.5, saturate(voxelEdge));
                    }
                }
                
                // �۸�ġ�� ���� �� ������ ���� �߰�
                if (_GlitchIntensity > 0.3) {
                    float timeOffset = _Time.y * 10;
                    float noiseLine = step(0.97, frac(i.uv.y * 30 + timeOffset * rand(floor(i.uv.y * 30))));
                    col.rgb = lerp(col.rgb, _GlowColor.rgb, noiseLine * _GlitchIntensity);
                }
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}