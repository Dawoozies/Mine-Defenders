Shader "Unlit/SpriteRecolor"
{
    Properties
    {
        //the full texture
        _MainTex ("Texture", 2D) = "white" {}
        [MaterialToggle] _RecolorActive("Recolor Active", Float) = 0
        _Target1 ("Target 1", Color) = (0,0,0,0)
        _Color1 ("Color 1", Color) = (0,0,0,0)
        _Target2 ("Target 2", Color) = (0,0,0,0)
        _Color2 ("Color 2", Color) = (0,0,0,0)
        _Target3 ("Target 3", Color) = (0,0,0,0)
        _Color3 ("Color 3", Color) = (0,0,0,0)
        _Target4 ("Target 4", Color) = (0,0,0,0)
        _Color4 ("Color 4", Color) = (0,0,0,0)
        _Target5 ("Target 5", Color) = (0,0,0,0)
        _Color5 ("Color 5", Color) = (0,0,0,0)
        _Target6 ("Target 6", Color) = (0,0,0,0)
        _Color6 ("Color 6", Color) = (0,0,0,0)
        _Target7 ("Target 7", Color) = (0,0,0,0)
        _Color7 ("Color 7", Color) = (0,0,0,0)
        _Target8 ("Target 8", Color) = (0,0,0,0)
        _Color8 ("Color 8", Color) = (0,0,0,0)
        _Target9 ("Target 9", Color) = (0,0,0,0)
        _Color9 ("Color 9", Color) = (0,0,0,0)
        _Target10 ("Target 10", Color) = (0,0,0,0)
        _Color10 ("Color 10", Color) = (0,0,0,0)
        _Target11 ("Target 11", Color) = (0,0,0,0)
        _Color11 ("Color 11", Color) = (0,0,0,0)
        _Target12 ("Target 12", Color) = (0,0,0,0)
        _Color12 ("Color 12", Color) = (0,0,0,0)
        _Target13 ("Target 13", Color) = (0,0,0,0)
        _Color13 ("Color 13", Color) = (0,0,0,0)
        _Target14 ("Target 14", Color) = (0,0,0,0)
        _Color14 ("Color 14", Color) = (0,0,0,0)
        _Target15 ("Target 15", Color) = (0,0,0,0)
        _Color15 ("Color 15", Color) = (0,0,0,0)
        _Target16 ("Target 16", Color) = (0,0,0,0)
        _Color16 ("Color 16", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha

        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RecolorActive;
            fixed4 _Target1;
            fixed4 _Target2;
            fixed4 _Target3;
            fixed4 _Target4;
            fixed4 _Target5;
            fixed4 _Target6;
            fixed4 _Target7;
            fixed4 _Target8;
            fixed4 _Target9;
            fixed4 _Target10;
            fixed4 _Target11;
            fixed4 _Target12;
            fixed4 _Target13;
            fixed4 _Target14;
            fixed4 _Target15;
            fixed4 _Target16;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _Color4;
            fixed4 _Color5;
            fixed4 _Color6;
            fixed4 _Color7;
            fixed4 _Color8;
            fixed4 _Color9;
            fixed4 _Color10;
            fixed4 _Color11;
            fixed4 _Color12;
            fixed4 _Color13;
            fixed4 _Color14;
            fixed4 _Color15;
            fixed4 _Color16;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                if(_RecolorActive == 1)
                {
                col = all(col == _Target1) ? _Color1 : col;
                col = all(col == _Target2) ? _Color2 : col;
                col = all(col == _Target3) ? _Color3 : col;
                col = all(col == _Target4) ? _Color4 : col;
                col = all(col == _Target5) ? _Color5 : col;
                col = all(col == _Target6) ? _Color6 : col;
                col = all(col == _Target7) ? _Color7 : col;
                col = all(col == _Target8) ? _Color8 : col;
                col = all(col == _Target9) ? _Color9 : col;
                col = all(col == _Target10) ? _Color10 : col;
                col = all(col == _Target11) ? _Color11 : col;
                col = all(col == _Target12) ? _Color12 : col;
                col = all(col == _Target13) ? _Color13 : col;
                col = all(col == _Target14) ? _Color14 : col;
                col = all(col == _Target15) ? _Color15 : col;
                col = all(col == _Target16) ? _Color16 : col;
                }
                return col * i.color;
            }
            ENDCG
        }
    }
}
