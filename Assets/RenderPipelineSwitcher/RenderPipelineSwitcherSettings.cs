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
	[CreateAssetMenu(fileName = "RenderPipelineSwitcherSettingsAsset", menuName = "Hoshino17/H17CubemapGenerator/Create RenderPipelineSwitcherSettingsAsset")]
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