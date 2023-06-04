using UnityEngine;
using UnityEngine.Rendering;

namespace Hoshino17
{ 
	public class RenderPipelineUtils : MonoBehaviour
	{
		public enum PipelineType
		{
			Unsupported,
			BuiltInPipeline,
			UniversalPipeline,
			HDPipeline
		}
 
		public static PipelineType DetectPipeline()
		{
#if UNITY_2019_1_OR_NEWER
			if (GraphicsSettings.renderPipelineAsset != null)
			{
				var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
				if (srpType.Contains("HDRenderPipelineAsset"))
				{
					return PipelineType.HDPipeline;
				}
				else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
				{
					return PipelineType.UniversalPipeline;
				}
				else
				{
					return PipelineType.Unsupported;
				}
			}
#elif UNITY_2017_1_OR_NEWER
			if (GraphicsSettings.renderPipelineAsset != null)
			{
				return PipelineType.Unsupported;
			}
#endif
			return PipelineType.BuiltInPipeline;
		}
	}
}
