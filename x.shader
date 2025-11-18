Shader "Custom/NumberAtlasShader"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _Number ("Number", Int) = 0
        _DigitCount ("Digit Count", Int) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _Number;
            int _DigitCount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Her rakam 1/10 genişlikte
                float digitWidth = 1.0 / 10.0;

                // UV.x'e göre hangi basamakta olduğumuzu bul
                int digitIndex = int(floor(uv.x * _DigitCount));
                int number = _Number;

                // İlgili basamağı al
                for (int j = 0; j < (_DigitCount - digitIndex - 1); j++)
                    number /= 10;

                int digit = number % 10;

                // Atlas'tan doğru rakamı seç
                float2 atlasUV;
                atlasUV.x = (uv.x * _DigitCount - digitIndex) * digitWidth + digit * digitWidth;
                atlasUV.y = uv.y;

                return tex2D(_MainTex, atlasUV);
            }
            ENDCG
        }
    }
}