Shader "ProCamera2D/TransitionsFX/Fade" 
{
    Properties 
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _Step ("Step", Range(0, 1)) = 0
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
    }

    SubShader 
    {
        ZTest Always Cull Off ZWrite Off Fog{ Mode Off }

        Pass 
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" 

            uniform sampler2D _MainTex;
            uniform float _Step;
            uniform float4 _BackgroundColor;

            struct v2f 
			{
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
				return o;
			}

            float4 frag(v2f i) : COLOR 
            {
                float4 colour = _BackgroundColor;
                colour = tex2D(_MainTex, i.uv);
				return (saturate(colour) * (1 - _Step)) + (_BackgroundColor * _Step);
            }

            ENDCG
        }
    }
}