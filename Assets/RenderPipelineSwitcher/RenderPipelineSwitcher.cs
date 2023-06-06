using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering;

#nullable enable

namespace Hoshino17
{
	[InitializeOnLoad]
	public static class RenderPipelineSwitcher
	{
#if UNITY_2019_1_OR_NEWER
		static RenderPipelineSwitcherSettings? _settings;

		static RenderPipelineSwitcher()
		{
			EditorApplication.delayCall += () => UpdateMenuCheck();
		}

		static void InitSettings()
		{
			if (_settings == null)
			{
				string[] guids = AssetDatabase.FindAssets("RenderPipeline SwitcherAsset", null);
				if (guids.Length > 0) 
				{
					var tmp = AssetDatabase.GUIDToAssetPath(guids[0]);
					_settings = AssetDatabase.LoadAssetAtPath<RenderPipelineSwitcherSettings>(tmp);
				}
			}
		}

		[MenuItem("Tools/Switch RenderPipeline/Builtin", false, 1)]
		static void SwitchToStandard()
		{
			GraphicsSettings.renderPipelineAsset = null;
		    QualitySettings.renderPipeline = null;
			EditorApplication.delayCall += () => UpdateMenuCheck();
		}

#if USING_HDRP
		[MenuItem("Tools/Switch RenderPipeline/HDRP", false, 1)]
		static void SwitchToHDRP()
		{
			InitSettings();
			if (_settings == null) { throw new InvalidOperationException(); }

			GraphicsSettings.renderPipelineAsset = _settings.hdrpAsset;
			QualitySettings.renderPipeline = _settings.hdrpAsset;
			EditorApplication.delayCall += () => UpdateMenuCheck();
		}
#endif

#if USING_URP
		[MenuItem("Tools/Switch RenderPipeline/URP", false, 1)]
		static void SwitchToURP()
		{
			InitSettings();
			if (_settings == null) { throw new InvalidOperationException(); }

			GraphicsSettings.renderPipelineAsset = _settings.urpAsset;
			QualitySettings.renderPipeline = _settings.urpAsset;
			EditorApplication.delayCall += () => UpdateMenuCheck();
		}
#endif

		static void UpdateMenuCheck()
		{
			var pipelineType = RenderPipelineUtils.DetectPipeline();
			Menu.SetChecked("Tools/Switch RenderPipeline/Builtin", pipelineType == RenderPipelineUtils.PipelineType.BuiltInPipeline);
#if USING_HDRP
			Menu.SetChecked("Tools/Switch RenderPipeline/URP", pipelineType == RenderPipelineUtils.PipelineType.UniversalPipeline);
#endif
#if USING_URP
			Menu.SetChecked("Tools/Switch RenderPipeline/HDRP", pipelineType == RenderPipelineUtils.PipelineType.HDPipeline);
#endif
		}


#endif
	}
}