Shader "Custom/DigitalStripeDissolve" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (0,0.5,1,1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _StripeWidth ("Stripe Width", Range(1, 50)) = 10
        _StripeSpeed ("Stripe Speed", Range(0, 10)) = 1
        _StripeIntensity ("Stripe Intensity", Range(0, 1)) = 0.5
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.3
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
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
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _DissolveAmount;
            float _StripeWidth;
            float _StripeSpeed;
            float _StripeIntensity;
            float _GlitchIntensity;
            float _EdgeWidth;
            
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
                float noise : TEXCOORD2;
            };
            
            // ���� �Լ�
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // ������ �Լ�
            float sampleNoise(float2 uv) {
                return tex2Dlod(_NoiseTex, float4(uv, 0, 0)).r;
            }
            
            // �۸�ġ ȿ��
            float3 applyGlitch(float3 pos, float2 uv, float intensity) {
                float time = _Time.y;
                
                // ���� �۸�ġ - Ư�� Y�������� �߻�
                float glitchLineY = floor(uv.y * 20) / 20;
                float glitchNoise = rand(float2(glitchLineY, floor(time * 10)));
                float glitchThreshold = 0.75; // �۸�ġ�� �߻��� �Ӱ谪
                
                if (glitchNoise > glitchThreshold) {
                    // ���� �۸�ġ - X������ ������
                    float glitchAmount = (glitchNoise - glitchThreshold) * 4.0 * intensity;
                    pos.x += (rand(float2(glitchLineY, time)) * 2 - 1) * glitchAmount;
                }
                
                return pos;
            }
            
            v2f vert(appdata v) {
                v2f o;
                
                float3 localPos = v.vertex.xyz;
                float noise = sampleNoise(v.uv);
                o.noise = noise;
                
                // ������ ���൵�� ���� �۸�ġ ȿ�� ����
                if (_DissolveAmount > 0 && _GlitchIntensity > 0) {
                    localPos = applyGlitch(localPos, v.uv, _GlitchIntensity * _DissolveAmount);
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
                // �ð� ���
                float time = _Time.y * _StripeSpeed;
                
                // ���� ��Ʈ������ ���� ����
                float stripePattern = sin(i.worldPos.y * _StripeWidth + time * 5) * 0.5 + 0.5;
                
                // ������� ��Ʈ������ ������ �����Ͽ� ������ ����ũ ����
                float dissolveNoise = i.noise * 0.5 + stripePattern * 0.5;
                
                // �߰� �۸�ġ ȿ�� (����)
                float glitchLine = 0;
                if (_GlitchIntensity > 0.2) {
                    float linePos = floor(i.uv.y * 30) / 30;
                    float lineNoise = rand(float2(linePos, floor(time * 8)));
                    
                    // ���������� �۸�ġ ���� ����
                    if (lineNoise > 0.93) {
                        glitchLine = smoothstep(0.5, 0.51, rand(float2(i.uv.y, time)));
                    }
                }
                
                // �ؽ�ó ���ø�
                float2 uvOffset = float2(0, 0);
                
                // �۸�ġ ȿ���� ������ UV �ְ� �߰�
                if (_GlitchIntensity > 0 && _DissolveAmount > 0.3) {
                    float glitchAmount = _GlitchIntensity * _DissolveAmount;
                    
                    // ��ĵ���� ȿ��
                    float scanLine = frac(i.uv.y * 10 - time);
                    float scanGlitch = step(0.9, scanLine) * glitchAmount * stripePattern;
                    
                    // UV �ְ�
                    uvOffset.x += (rand(float2(floor(i.uv.y * 20) / 20, time)) * 2 - 1) * scanGlitch * 0.1;
                }
                
                fixed4 col = tex2D(_MainTex, i.uv + uvOffset) * _Color;
                
                // ������ ����ũ�� ������ ���൵ ��
                float dissolveMask = step(dissolveNoise, _DissolveAmount);
                
                // ���� �߱� ȿ�� (�������Ÿ��� ����)
                float edgeMask = step(dissolveNoise - _EdgeWidth, _DissolveAmount) - dissolveMask;
                
                // �߱� ���� ��� (��Ʈ������ ���Ͽ� ���� ���� ��ȭ)
                float3 glowColorMod = _GlowColor.rgb * (0.8 + stripePattern * 0.4);
                
                // �۸�ġ ������ ������ ���� ����
                glowColorMod = lerp(glowColorMod, float3(1, 1, 1), glitchLine * 0.7);
                
                // �߱� ȿ�� ����
                col.rgb = lerp(col.rgb, glowColorMod, edgeMask);
                
                // �۸�ġ ���� �߰�
                col.rgb = lerp(col.rgb, glowColorMod, glitchLine * _GlitchIntensity * 0.5);
                
                // ��Ʈ������ ���Ͽ� ���� ���� ��ȭ (�������Ÿ��� ����)
                float alpha = 1.0;
                
                if (_DissolveAmount > 0) {
                    // ������ ���� ����ȭ
                    alpha *= 1.0 - dissolveMask;
                    
                    // ������ ���� ���� �κп� ��Ʈ������ ȿ�� �߰�
                    if (_DissolveAmount > 0.1 && _DissolveAmount < 0.9) {
                        float dissolveProgress = smoothstep(0.0, 0.2, _DissolveAmount) * 
                                              smoothstep(1.0, 0.8, _DissolveAmount);
                        
                        // ��Ʈ������ ���Ͽ� ���� ������
                        float flicker = sin(time * 20) * 0.5 + 0.5;
                        float stripeFade = stripePattern * _StripeIntensity * dissolveProgress * flicker;
                        
                        // ������Ǵ� ���� ��ó������ ��Ʈ������ ȿ�� ����
                        float proximity = smoothstep(_DissolveAmount + 0.1, _DissolveAmount - 0.1, dissolveNoise);
                        alpha *= lerp(1.0, stripeFade, proximity * 0.5);
                    }
                }
                
                col.a *= alpha * _Color.a;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}