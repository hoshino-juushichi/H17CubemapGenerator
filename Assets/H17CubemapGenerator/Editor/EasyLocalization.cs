using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Hoshino17
{
	public enum TextId
	{
		Language,
		TabMain,
		TabPreview,
		TabSettings,
		HideOtherUIs,
		PreviewObject,
		ResetPreviewRotation,
		PreviewSupersampling,
		PreviewSkybox,
		PreviewOjbectSphere,
		PreviewOjbectCube,
		SelectInputSource,
		InputSourceCurrentScene,
		InputSourceSixSided,
		InputSourceCubemap,
		SpecificCamera,
		SelectOutputLayout,
		OutputLayoutCrossHorizontal,
		OutputLayoutCrossVertical,
		OutputLayoutStraitHorizontal,
		OutputLayoutStraitVertical,
		OutputLayoutSixSided,
		OutputLayoutEquirectangular,
		OutputLayoutMatcap,
		InputSpecificCamera,
		InputTextureWidth,
		InputCubemap,
		InputLeft,
		InputRight,
		InputTop,
		InputBottom,
		InputFront,
		InputBack,
		Redraw,
		OutputFormatHDR,
		OutputSRGB,
		OutputCubemap,
		OutputGenerateMipmap,
		EquirectangularRotation,
		FillMatcapOutsideByEdgeColor,
		Export,
		AdviceSetCubemap,
		AdviceSetSixSidedTexture,
		AdviceTextureWidthAndHeightEqual,
		AdviceEachTextureFaceSameSize,
		AdviceEachTextureFacesSameFormat,
		TextureShape,
		TextureShape2D,
		TextureShapeCube,
		UsingCameraAngles,
		HorizontalRotation,
		ExposureOverride,
		FixedExposure,
		Compensation,
	}

	public class EasyLocalization
	{
		SystemLanguage _language = SystemLanguage.Unknown;
		public SystemLanguage language
		{
			get => _language;
			set
			{
				_language = value;
				_current = _dictionalies[_language];
			}
		}

		readonly Dictionary<SystemLanguage, Dictionary<TextId, string>> _dictionalies = new Dictionary<SystemLanguage, Dictionary<TextId, string>>();
		Dictionary<TextId, string>? _current;
		Dictionary<TextId, string>? _failSafe;

		readonly List<SystemLanguage> _supportedLanguages = new List<SystemLanguage>();
		public IReadOnlyList<SystemLanguage> supportedLanguages => _supportedLanguages;

		public EasyLocalization()
		{
			var files = System.IO.Directory.GetFiles(AssetPath.h17CubemapGenerator + "Editor/Assets/Languages/", "*.csv", System.IO.SearchOption.TopDirectoryOnly);
			foreach ( var file in files )
			{
				var languageName = Path.GetFileNameWithoutExtension(file);

				if (EnumUtils.ParseEnum<SystemLanguage>(languageName, out var language))
				{
					var dict = new Dictionary<TextId, string>();
					LoadCSV(dict, file);
					_dictionalies.Add(language, dict);
					_supportedLanguages.Add(language);
				}
			}

			if (_dictionalies.ContainsKey(Application.systemLanguage))
			{
				this.language = Application.systemLanguage;
			}
			if (_dictionalies.ContainsKey(SystemLanguage.English))
			{
				_failSafe = _dictionalies[SystemLanguage.English];
			}
		}

		public string Get(TextId id)
		{
			if (_current != null)
			{
				if (_current.ContainsKey(id))
				{
					return _current[id];
				}
			}
			if (_failSafe != null)
			{
				if (_failSafe.ContainsKey(id))
				{
					return _failSafe[id];
				}
			}
			return id.ToString();
		}

		void LoadCSV(Dictionary<TextId, string> dict, string assetPath)
		{
			var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
			if (textAsset == null)
			{
				return;
			}
			StringReader reader = new StringReader(textAsset.text);
			while (reader.Peek() != -1)
			{
				string line = reader.ReadLine();
				var words = line.Split(',');
				if (words.Length >= 2)
				{
					if (EnumUtils.ParseEnum<TextId>(words[0], out var loc))
					{
						if (!dict.ContainsKey(loc))
						{
							dict.Add(loc, words[1]);
						}
					}
				}
			}
		}
	}
}