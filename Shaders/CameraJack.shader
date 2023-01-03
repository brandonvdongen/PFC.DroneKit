Shader "Unlit/CameraJack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "QUEUE"="Overlay+1000" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			ZTest Always
			//Cull OFF
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			bool isVR() {
				// USING_STEREO_MATRICES
				#if UNITY_SINGLE_PASS_STEREO
					return true;
				#else
					return false;
				#endif
			}

			bool isDroneCamera() {
				return !isVR() && _ScreenParams.y == 1084;
			}

			//bool isPanorama() {
				// Crude method
				// FOV=90=camproj=[1][1]
				//return unity_CameraProjection[1][1] == 1 && _ScreenParams.x == 1075 && _ScreenParams.y == 1025;
			//}
			bool IsInMirror(){
				return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
				if (IsInMirror() || isDroneCamera()) { o.vertex=1; }
				else if (!isVR()) {
					o.vertex = float4(2*v.uv-1,1,1);
					o.vertex.y *= -1;
				}
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
