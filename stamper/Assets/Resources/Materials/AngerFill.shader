Shader "Custom/AngerFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillAmount ("Fill", Range(0,1)) = 0
        _FillColor ("Color", Color) = (0.7,0,0,0.7)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FillAmount;
            float4 _FillColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 元のスプライトカラー
                fixed4 baseCol = tex2D(_MainTex, i.uv);

                // 透明ピクセルはそのまま（輪郭の灰色防止）
                if (baseCol.a <= 0.001)
                    return baseCol;

                // Fill処理
                if (i.uv.y <= _FillAmount)
                {
                    // 元の色を残しつつ赤をブレンド（調整可 0.75 → 0.5でもOK）
                    baseCol.rgb = lerp(baseCol.rgb, _FillColor.rgb, 0.6);
                }

                return baseCol;
            }
            ENDCG
        }
    }
}
