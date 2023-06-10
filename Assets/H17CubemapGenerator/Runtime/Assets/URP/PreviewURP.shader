Shader "Honshino17/H17CubemapGenerator/PreviewURP"
{
	Properties
	{
		[MainTexture] _MainTex("MainTex", Cube) = "white" {}
	}

	HLSLINCLUDE
	#pragma target 2.0
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	ENDHLSL

	SubShader
	{
		Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "Queue" = "Background"}
		LOD 100

		Blend One Zero
		BlendOp Add
		ZWrite On
		ZTest LEqual
		Cull Back

		Pass
		{
			Name "PreviewCubeUnlit"

			HLSLPROGRAM

			#pragma shader_feature _ SKYBOX_ON

			TEXTURECUBE(_MainTex);	SAMPLER(sampler_MainTex);
			uniform float4x4 _PreviewRotationMatrix;

			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				float4 positionOS	: POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 positionOS : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;

				output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
				output.positionOS = input.positionOS.xyz;
				output.positionCS = TransformWorldToHClip(output.positionWS);

				float3 normalOS = normalize(input.positionOS.xyz);
				output.normalWS = TransformObjectToWorldNormal(normalOS);

				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
#if defined(SKYBOX_ON)
				float3 vec = normalize(input.positionOS);
				float4 color = SAMPLE_TEXTURECUBE( _MainTex, sampler_MainTex, vec);
				return color;
#else
				float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
				float3 reflectDir = reflect(-viewDir, input.normalWS);
				reflectDir = mul(reflectDir, (float3x3)_PreviewRotationMatrix);
				float4 color = SAMPLE_TEXTURECUBE( _MainTex, sampler_MainTex, reflectDir);
				return color;
#endif
			}

			ENDHLSL
		}

	}
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
