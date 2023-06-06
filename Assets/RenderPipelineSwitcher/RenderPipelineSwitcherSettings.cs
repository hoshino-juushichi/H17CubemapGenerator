using UnityEngine;
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

#nullable enable

namespace Hoshino17
{
	[CreateAssetMenu(fileName = "RenderPipelineSwitcherAsset", menuName = "Hoshino17/H17CubemapGenerator/Create RenderPipelineSwitcherAsset")]
	public class RenderPipelineSwitcherSettings : ScriptableObject
	{
#if USING_HDRP
		public HDRenderPipelineAsset? hdrpAsset;
#endif
#if USING_URP
		public UniversalRenderPipelineAsset? urpAsset;
#endif
	}
}