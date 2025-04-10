Shader "Custom/DataStreamParticleShader" {
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (0,1,0.5,1)
        _SecondaryColor ("Secondary Color", Color) = (0,0.6,1,1)
        _GlowColor ("Glow Color", Color) = (0,1,0,1)
        
        // ȿ�� ���� ������Ƽ
        _StreamWidth ("Stream Width", Range(0.01, 0.5)) = 0.05
        _StreamSpeed ("Stream Speed", Range(0.5, 10)) = 4
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.0
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 5.0
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0.1
        _FadeDistance ("Fade Distance", Range(0, 1)) = 0.2
    }
    
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100
        
        Blend SrcAlpha One // ���� �������� �߱� ȿ��
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
                float4 velocity : TEXCOORD1; // �ӵ� ������ velocity�� ����
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 params : TEXCOORD1; // �Ķ���� �����
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _PrimaryColor;
            fixed4 _SecondaryColor;
            fixed4 _GlowColor;
            float _StreamWidth;
            float _StreamSpeed;
            float _GlowIntensity;
            float _FlickerSpeed;
            float _NoiseAmount;
            float _FadeDistance;
            
            // ���� �Լ�
            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            v2f vert (appdata v) {
                v2f o;
                
                // ��ƼŬ ���󿡼� ���� ����
                float direction = v.color.r;      // r ä��: ���� (0-1 ����)
                float streamID = v.color.g * 20;  // g ä��: ��Ʈ�� ID (0-20)
                float speedFactor = v.color.b;    // b ä��: �ӵ� ���
                
                // ���� ���� ��� (direction ���� ������ ��ȯ)
                float angle = direction * 6.28318; // 0-1 ���� 0-2��� ��ȯ
                float2 streamDir = float2(cos(angle), sin(angle));
                
                // ��ƼŬ ��ġ ��� (�̹� ��ũ��Ʈ���� ��ġ��)
                float4 worldPos = v.vertex;
                
                // �ð� �� ������ ���
                float time = _Time.y * _StreamSpeed * speedFactor;
                float noiseTime = time + streamID * 0.5;
                
                // ������ ���� (�ణ�� ��鸲)
                float2 noise = float2(
                    sin(noiseTime + v.vertex.x * 10.0),
                    cos(noiseTime * 1.2 + v.vertex.y * 10.0)
                ) * _NoiseAmount;
                
                // ���� ��ġ�� ������ �߰�
                worldPos.xy += noise;
                
                o.vertex = UnityObjectToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // ���⿡ ���� ���� ��ȭ
                float colorLerp = sin(time * 0.2 + streamID) * 0.5 + 0.5;
                o.color = lerp(_PrimaryColor, _SecondaryColor, colorLerp);
                
                // �Ķ���� ����
                o.params = float4(direction, streamID, speedFactor, time);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // �Ķ���� ����
                float direction = i.params.x;
                float streamID = i.params.y;
                float speedFactor = i.params.z;
                float time = i.params.w;
                
                // ������ ��Ʈ�� ȿ�� (UV ��ǥ�� ������� �� ��Ʈ�� ���)
                float streamCenter = abs(i.uv.y - 0.5) / 0.5; // 0: �߾�, 1: �����ڸ�
                float streamShape = 1.0 - streamCenter;
                streamShape = pow(streamShape, 2.0); // �׵θ��� �� �ε巴��
                
                // ������ ���� ȿ�� (�ұ�Ģ�� ��� ��ȭ)
                float dataNoise = random(floor(i.uv * 10.0 + time));
                float dataChunk = step(0.6, dataNoise) * 0.3 + 0.7; // 30% ��� ��ȭ
                
                // �� ������ ������ ���� (0�� 1�� �帣�� ����)
                float binaryPattern = step(0.5, random(floor(i.uv.x * 15.0 + time) / 15.0));
                
                // ������ ȿ��
                float flicker = sin(time * _FlickerSpeed + streamID) * 0.5 + 0.5;
                flicker = pow(flicker, 0.5) * 0.3 + 0.7; // �ε巯�� ������ (30% ��ȭ)
                
                // �߱� ȿ��
                float glow = _GlowIntensity * flicker;
                
                // �ؽ�ó ���ø�
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // ��Ʈ�� ���� ���
                float streamIntensity = streamShape * dataChunk * flicker;
                
                // ���� ����
                fixed4 finalColor = i.color * streamIntensity;
                finalColor.rgb += _GlowColor.rgb * glow * streamShape * binaryPattern * 0.5;
                finalColor.a = streamShape * texColor.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}