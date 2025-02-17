Shader "Custom/DeathReaperExplosionShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DataTex ("Data Pattern", 2D) = "white" {}
        _VoronoiTex ("Voronoi Pattern", 2D) = "white" {}
        
        _ExplosionRadius ("Explosion Radius", Range(0, 20)) = 5
        _Distortion ("Distortion Amount", Range(0, 1)) = 0.2
        _ExplosionForce ("Explosion Force", Range(0, 10)) = 5
        _TurbulenceFreq ("Turbulence Frequency", Range(0, 10)) = 2
        
        _CoreColor ("Core Color", Color) = (0.5,0,1,1)
        _RimColor ("Rim Color", Color) = (1,0,0.5,1)
        _DataColor ("Data Color", Color) = (0,0.7,1,1)
        _GlowColor ("Glow Color", Color) = (1,0,1,1)
        
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 5
        _DataFlowSpeed ("Data Flow Speed", Range(0, 10)) = 2
        _VortexStrength ("Vortex Strength", Range(0, 5)) = 1
        
        _DissolveEdge ("Dissolve Edge", Range(0, 1)) = 0.1
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeGlow ("Edge Glow", Range(0, 5)) = 2
        
        _TimeScale ("Time Scale", Range(0, 5)) = 1
        _DistortionScale ("Distortion Scale", Range(0, 10)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
        ZWrite On
        ZTest LEqual
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 grabPos : TEXCOORD4;
                float3 worldTangent : TEXCOORD5;
                float3 worldBinormal : TEXCOORD6;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex, _NoiseTex, _DataTex, _VoronoiTex, _GrabTexture;
            float4 _MainTex_ST, _GrabTexture_TexelSize;
            float _ExplosionRadius, _Distortion, _ExplosionForce, _TurbulenceFreq;
            float4 _CoreColor, _RimColor, _DataColor, _GlowColor;
            float _GlitchIntensity, _GlitchSpeed, _DataFlowSpeed, _VortexStrength;
            float _DissolveEdge, _DissolveAmount, _EdgeGlow;
            float _TimeScale, _DistortionScale;
            
            // ������ �Լ�
            float3 mod289(float3 x) 
            { 
                return x - floor(x * (1.0 / 289.0)) * 289.0; 
            }
            
            float2 mod289(float2 x) 
            { 
                return x - floor(x * (1.0 / 289.0)) * 289.0; 
            }
            
            float3 permute(float3 x) 
            { 
                return mod289(((x*34.0)+1.0)*x); 
            }
            
            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187,
                                      0.366025403784439,
                                     -0.577350269189626,
                                      0.024390243902439);
                
                float2 i  = floor(v + dot(v, C.yy));
                float2 x0 = v -   i + dot(i, C.xx);
                float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;
                
                i = mod289(i);
                float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
                
                float3 m = max(0.5 - float3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
                m = m*m;
                m = m*m;
                
                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;
                
                m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
                
                float3 g;
                g.x  = a0.x  * x0.x  + h.x  * x0.y;
                g.yz = a0.yz * x12.xz + h.yz * x12.yw;
                return 130.0 * dot(m, g);
            }
            
            // �͵�(Vortex) ���� �Լ�
            float2 vortex(float2 uv, float2 center, float strength)
            {
                float2 delta = uv - center;
                float angle = strength * length(delta);
                float x = cos(angle) * delta.x - sin(angle) * delta.y;
                float y = sin(angle) * delta.x + cos(angle) * delta.y;
                return float2(x + center.x, y + center.y);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // ��ü�� ���� �߰����� UV ���
                float3 normal = UnityObjectToWorldNormal(v.normal);
                float2 sphereUV = float2(
                    atan2(normal.x, normal.z) / (2 * 3.14159) + 0.5,
                    asin(normal.y) / 3.14159 + 0.5
                );
                
                // �ð� ��� ������
                float time = _Time.y * _TimeScale;
                float noise = snoise(sphereUV + time);
                
                // ���� ȿ��
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float dist = length(worldPos);
                float explosionFactor = saturate(1 - dist / _ExplosionRadius);
                
                // ��ü�� ���� ���� ���
                float turbulence = snoise(normal.xy * _TurbulenceFreq + time) * 
                                 snoise(normal.yz * _TurbulenceFreq + time) * 
                                 snoise(normal.xz * _TurbulenceFreq + time);
                
                // ���� ���
                float3 displacement = v.normal * (explosionFactor * _Distortion + 
                                               turbulence * _ExplosionForce);
                displacement *= saturate(1 - _DissolveAmount * 2);
                v.vertex.xyz += displacement;
                
                // �⺻ ��� ���
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = sphereUV;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = worldPos;
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                o.color = v.color;
                
                // ź��Ʈ ���� ���
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                
                // �׷� �н� ��ǥ
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _TimeScale;
                
                // �͵� ȿ���� ����� UV ��ǥ
                float2 vortexUV = vortex(i.uv, float2(0.5, 0.5), _VortexStrength * time * 0.5);
                
                // ������� ������ ����
                float2 noiseUV = vortexUV + time * 0.1;
                float noise = tex2D(_NoiseTex, noiseUV).r;
                float2 dataUV = vortexUV + float2(
                    snoise(float2(time * _DataFlowSpeed, 0)),
                    snoise(float2(0, time * _DataFlowSpeed))
                );
                float dataPattern = tex2D(_DataTex, dataUV).r;
                
                // Voronoi ������ �̿��� ���귯 ������
                float2 voronoiUV = vortexUV * _DistortionScale + time * 0.2;
                float voronoi = tex2D(_VoronoiTex, voronoiUV).r;
                
                // ������(Fresnel) ȿ�� ����
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), 5);
                fresnel = lerp(fresnel, fresnel * (1 + sin(time * 3)) * 0.5 + 0.5, 0.5);
                
                // �۸�ġ ȿ��
                float2 glitchUV = i.uv;
                float randomVal = snoise(float2(floor(time * _GlitchSpeed), floor(i.uv.y * 20)));
                if (randomVal > 1 - _GlitchIntensity) {
                    glitchUV.x += (randomVal - 0.5) * 0.1;
                    float blockNoise = snoise(float2(floor(i.uv.y * 20), time));
                    if (blockNoise > 0.8) {
                        glitchUV.y += blockNoise * 0.2;
                    }
                }
                
                // ������ ȿ��
                float dissolveNoise = noise + voronoi * 0.5;
                float dissolveEdge = step(dissolveNoise, _DissolveAmount) - 
                                   step(dissolveNoise, _DissolveAmount - _DissolveEdge);
                
                // ȭ�� �ְ� ȿ��
                float2 screenUV = i.grabPos.xy / i.grabPos.w;
                float2 distortion = (noise - 0.5) * _Distortion;
                float4 grabColor = tex2D(_GrabTexture, screenUV + distortion);
                
                // ���� ���� ���
                float3 finalColor = lerp(_CoreColor.rgb, _RimColor.rgb, fresnel);
                finalColor = lerp(finalColor, _DataColor.rgb, dataPattern * (1 - _DissolveAmount));
                finalColor = lerp(finalColor, grabColor.rgb, _DissolveAmount * 0.5);
                
                // �����ڸ� �߱� ȿ��
                float edgeGlow = dissolveEdge * _EdgeGlow;
                finalColor += _GlowColor.rgb * edgeGlow;
                
                // �ھ� �޽� ȿ��
                float corePulse = 0.5 + 0.5 * sin(time * 5);
                finalColor += _CoreColor.rgb * corePulse * (1 - _DissolveAmount) * (1 - fresnel);
                
                // �۸�ġ ���� ����
                float4 glitchColor = tex2D(_MainTex, glitchUV);
                finalColor = lerp(finalColor, glitchColor.rgb, _GlitchIntensity * randomVal);
                
                // ���� ���
                float alpha = (1 - _DissolveAmount) + edgeGlow;
                alpha = saturate(alpha * (1 - dissolveEdge * 0.5));
                alpha *= saturate(1 - length(i.worldPos) / (_ExplosionRadius * 2));
                alpha = saturate(alpha);
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
}