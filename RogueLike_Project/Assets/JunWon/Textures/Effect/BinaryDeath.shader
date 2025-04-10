Shader "Custom/BinaryNumbersParticleShader" {
    Properties {
        _ZeroTex ("Zero Texture", 2D) = "white" {}
        _OneTex ("One Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0,1,0.5,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0.6,1,1)
        _GlowColor ("Glow Color", Color) = (0,1,0,1)
        
        // ȿ�� ���� ������Ƽ
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.0
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 5.0
        _RiseSpeed ("Rise Speed", Range(0, 10)) = 1.0
        _SpinSpeed ("Spin Speed", Range(-5, 5)) = 1.0
        _FadeDistance ("Fade Distance", Range(0, 5)) = 2.0
        _DigitalEffect ("Digital Effect", Range(0, 1)) = 0.5
    }
    
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 customData : TEXCOORD1; // x: ��ƼŬ ����, y: ���� �õ�, z: 0 �Ǵ� 1 ����
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 customData : TEXCOORD1;
            };
            
            sampler2D _ZeroTex;
            sampler2D _OneTex;
            float4 _ZeroTex_ST;
            float4 _OneTex_ST;
            fixed4 _PrimaryColor;
            fixed4 _SecondaryColor;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _FlickerSpeed;
            float _RiseSpeed;
            float _SpinSpeed;
            float _FadeDistance;
            float _DigitalEffect;
            
            // ���� �Լ�
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // ��ƼŬ�� ���� 0�� 1 �� ����
                float binaryType = v.customData.z;
                if (binaryType <= 0.0) {
                    // customData�� ���� ������ UV ��ǥ�� ����
                    binaryType = step(0.5, random(v.uv));
                }
                
                // ��ƼŬ �õ� (�� ��ƼŬ�� ������)
                float seed = v.customData.y;
                if (seed <= 0.0) {
                    // customData�� ���� ������ ���ؽ� ��ġ�� ����
                    seed = random(v.vertex.xy);
                }
                
                // ��� ȿ�� (y������ �̵�)
                float time = _Time.y;
                float rise = time * _RiseSpeed + seed * 10.0; // �� ��ƼŬ���� �ٸ� ����
                
                // ȸ�� ȿ��
                float spin = time * _SpinSpeed + seed * 6.28318; // �� ��ƼŬ���� �ٸ� ȸ�� ����
                float2x2 rotationMatrix = float2x2(
                    cos(spin), -sin(spin),
                    sin(spin), cos(spin)
                );
                float2 rotatedPos = mul(rotationMatrix, v.vertex.xy);
                
                // �¿� ��鸲 ȿ��
                float wiggle = sin(time * 3.0 + seed * 10.0) * 0.1 * seed;
                
                // ���� ��ġ ���
                float3 newPos = float3(
                    rotatedPos.x + wiggle,
                    v.vertex.y + rise,
                    v.vertex.z
                );
                
                // ���� ���������� ���ؽ� ��ġ ���
                float4 worldPos = mul(unity_ObjectToWorld, float4(newPos, 1.0));
                
                // ī�޶���� �Ÿ��� ���� ���̵� ���
                float viewDist = distance(_WorldSpaceCameraPos.xyz, worldPos.xyz);
                float fadeAlpha = saturate((_FadeDistance - viewDist) / _FadeDistance);
                
                o.vertex = UnityObjectToClipPos(float4(newPos, 1.0));
                o.uv = v.uv;
                
                // ���� ���̵�� ��ƼŬ ���� �ݿ�
                o.color = v.color;
                o.color.a *= fadeAlpha;
                
                // Ŀ���� ������ ����
                o.customData = float4(binaryType, seed, v.customData.x, 0);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // 0 �Ǵ� 1 ����
                float binaryType = i.customData.x;
                float seed = i.customData.y;
                float age = i.customData.z; // ��ƼŬ ���� (�ִ� ���)
                
                // �ð� ����
                float time = _Time.y;
                
                // ������ ȿ��
                float flicker = sin(time * _FlickerSpeed + seed * 10.0) * 0.5 + 0.5;
                flicker = pow(flicker, 0.5); // �� �ڿ������� ������
                
                // ������ ������ ȿ��
                float2 noiseUV = i.uv + time * 0.1;
                float digitalNoise = random(floor(noiseUV * 20.0) / 20.0);
                float glitchEffect = step(0.7, digitalNoise) * _DigitalEffect;
                
                // �ؽ�ó ���� �� ���ø�
                float2 offsetUV = i.uv;
                
                // �۸�ġ ȿ�� (���� UV �ְ�)
                if (glitchEffect > 0.0) {
                    offsetUV.x += (digitalNoise - 0.5) * 0.1 * _DigitalEffect;
                }
                
                // 0 �Ǵ� 1 �ؽ�ó ���ø�
                fixed4 texColor;
                if (binaryType < 0.5) {
                    texColor = tex2D(_ZeroTex, offsetUV);
                } else {
                    texColor = tex2D(_OneTex, offsetUV);
                }
                
                // ���� ���
                float colorLerp = sin(time * 0.5 + seed * 6.28318) * 0.5 + 0.5;
                fixed4 baseColor = lerp(_PrimaryColor, _SecondaryColor, colorLerp);
                
                // ������ ȿ�� (���� ���� ��ȭ)
                if (glitchEffect > 0.0) {
                    baseColor.rgb = lerp(baseColor.rgb, _GlowColor.rgb, glitchEffect * 0.5);
                }
                
                // ���� ����
                fixed4 finalColor = texColor * i.color;
                finalColor.rgb *= baseColor.rgb;
                
                // �߱� ȿ��
                finalColor.rgb += _GlowColor.rgb * flicker * _GlowIntensity * 0.3;
                
                // ���İ� ���
                finalColor.a *= texColor.a * i.color.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}