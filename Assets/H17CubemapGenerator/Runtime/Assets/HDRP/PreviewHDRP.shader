Shader "Honshino17/H17CubemapGenerator/PreviewHDRP"
{
	Properties
	{
		[MainTexture] _MainTex("MainTex", Cube) = "white" {}
	}

	HLSLINCLUDE
	#pragma target 4.5
	#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
	ENDHLSL

	SubShader
	{
		Tags{ "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDUnlitShader" }
		LOD 100

		Blend One Zero
		BlendOp Add
		ZWrite On
		ZTest LEqual
		Cull Back

		Pass
		{
			Name "PreviewCubeUnlit"
			Tags { "LightMode" = "ForwardOnly" }

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
	FallBack "Hidden/HDRP/FallbackError"
}