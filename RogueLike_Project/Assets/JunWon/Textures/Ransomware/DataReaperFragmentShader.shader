Shader "Custom/DataReaperFragmentShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _GridTex ("Digital Grid", 2D) = "black" {}
        
        _Color ("Main Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)
        
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.1)) = 0.02
        _GridStrength ("Grid Strength", Range(0, 1)) = 0.5
        
        _FragmentSize ("Fragment Size", Range(0.001, 0.1)) = 0.02
        _FragmentSpread ("Fragment Spread", Range(0, 1)) = 0.5
        _FragmentShift ("Fragment Shift", Range(0, 1)) = 0.2
        
        _GlitchFrequency ("Glitch Frequency", Range(0, 20)) = 5
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float2 gridUV : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD3;
                float3 worldNormal : TEXCOORD4;
                float4 color : COLOR;
                UNITY_FOG_COORDS(5)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            sampler2D _GridTex;
            float4 _GridTex_ST;
            
            fixed4 _Color;
            fixed4 _EmissionColor;
            
            float _DissolveAmount;
            float _EdgeWidth;
            float _GridStrength;
            
            float _FragmentSize;
            float _FragmentSpread;
            float _FragmentShift;
            
            float _GlitchFrequency;
            float _GlitchIntensity;
            
            // ���� �Լ�
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // �ȼ�ȭ �Լ�
            float2 pixelate(float2 uv, float size)
            {
                return floor(uv / size) * size;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // ������ ���࿡ ���� ���ؽ� ������ ���
                float3 vertexOffset = float3(0, 0, 0);
                
                if (_DissolveAmount > 0)
                {
                    // ������ �� ��� ���� ������
                    float2 noiseUV = v.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                    float noise = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).r;
                    
                    // ������ �Ӱ谪�� ������ �� ���Ͽ� ������ ���
                    float threshold = _DissolveAmount - 0.3; // ���� ������Ǵ� �κ�
                    
                    if (noise < threshold)
                    {
                        // ������ ������ ���� ���
                        float3 direction = normalize(v.normal + float3(
                            random(v.uv + _Time.x) * 2.0 - 1.0, 
                            random(v.uv + _Time.y) * 2.0 - 1.0, 
                            random(v.uv + _Time.z) * 2.0 - 1.0
                        ));
                        
                        // ������ ���൵�� ���� ������ ���� ���
                        float offsetStrength = saturate((threshold - noise) / 0.3) * _FragmentSpread;
                        
                        // ���� ���ؽ� ������
                        vertexOffset = direction * offsetStrength * _DissolveAmount * 0.5;
                        
                        // ���� ȸ�� �߰�
                        float rotAngle = _DissolveAmount * random(v.uv) * 6.28318;
                        float sinR = sin(rotAngle);
                        float cosR = cos(rotAngle);
                        float3 rotOffset = float3(
                            vertexOffset.x * cosR - vertexOffset.z * sinR,
                            vertexOffset.y,
                            vertexOffset.x * sinR + vertexOffset.z * cosR
                        );
                        
                        vertexOffset = rotOffset;
                    }
                }
                
                // �۸�ġ ȿ�� - ���� ������
                float glitchTime = _Time.y * _GlitchFrequency;
                float2 blockPos = floor(v.uv * 10) / 10;
                float blockNoise = random(blockPos + floor(glitchTime));
                
                if (blockNoise > 0.95 && _GlitchIntensity > 0)
                {
                    float glitchOffset = (random(blockPos + glitchTime) * 2 - 1) * 0.1 * _GlitchIntensity;
                    vertexOffset.x += glitchOffset;
                }
                
                // ���� ���ؽ� ��ġ ���
                o.vertex = UnityObjectToClipPos(v.vertex + float4(vertexOffset, 0));
                
                // UV ��ǥ ���
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.gridUV = TRANSFORM_TEX(v.uv, _GridTex) + float2(_Time.y * 0.1, _Time.y * 0.05); // �׸��� �ִϸ��̼�
                
                // ���� ��ġ �� �븻 ���
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // ���� �÷� ����
                o.color = v.color * _Color;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // �⺻ �ؽ�ó ���ø�
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
    
                // ������ ���ø�
                float noise = tex2D(_NoiseTex, i.noiseUV).r;
    
                // ������ ȿ�� ��� (���� ������ ���� �����ڸ� ȿ��)
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float fresnel = 1.0 - saturate(dot(normalize(i.worldNormal), viewDir));
                fresnel = pow(fresnel, 3) * 0.8;
    
                // �����ڸ� ȿ�� ��� (������ + ������ ���)
                float edgeFactor = smoothstep(0.4, 0.6, fresnel + noise * 0.2);
    
                // �⺻ ���� ���������� �������ϵ�, �����ڸ����� ȿ�� ����
                if (edgeFactor > 0)
                {
                    // �۸�ġ ȿ�� (�����ڸ�����)
                    float2 glitchUV = i.uv;
                    if (_GlitchIntensity > 0 && edgeFactor > 0.2)
                    {
                        float glitchTime = _Time.y * _GlitchFrequency;
                        float rowNoise = random(float2(floor(i.uv.y * 20) / 20, floor(glitchTime)));
            
                        if (rowNoise > 0.8)
                        {
                            float glitchAmount = (random(float2(i.uv.y, glitchTime)) * 2 - 1) * 0.03 * _GlitchIntensity * edgeFactor;
                            glitchUV.x += glitchAmount;
                        }
                    }
        
                    // �����ڸ��� ���� ���� �߰�
                    float grid = tex2D(_GridTex, i.gridUV).r;
        
                    // �����ڸ� �߱� ȿ��
                    col.rgb = lerp(col.rgb, _EmissionColor.rgb * (1.5 + grid), edgeFactor * 0.7);
        
                    // �����ڸ����� ������ ������ ���� �߰�
                    col.rgb += grid * _EmissionColor.rgb * edgeFactor * _GridStrength;
                }
    
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}