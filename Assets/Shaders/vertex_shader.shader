Shader "Unlit/vertex_shader"
{
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		Cull off

		CGPROGRAM
		#pragma surface surf SimpleSpecular vertex:vert   
		#pragma target 3.0

		half4 LightingSimpleSpecular(SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten) 
		{
			half3 h = normalize(lightDir+viewDir);

			half diff = max(0, dot(s.Normal, lightDir));

			float nh = max(0, dot(s.Normal, h));
			//float spec = pow(nh, 48.0);

			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb) * atten;
			c.a = s.Alpha;
			return c;
		}


		struct Input {
			float4 vertColor;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertColor = v.color;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb;			
		}
		ENDCG
	}
		FallBack "Diffuse"
}
