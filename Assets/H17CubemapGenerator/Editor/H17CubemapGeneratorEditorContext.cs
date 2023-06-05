using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Hoshino17
{
	public sealed class H17CubemapGeneratorEditorContext
	{
		public PreviewScene previewScene { get; private set; }
		public H17CubemapGenerator? generatorInstance { get; private set; }

		static readonly string AssetPathCubemapGeneratorPrefab = AssetPath.h17CubemapGenerator + "Editor/Assets/h17cubemap_generator.prefab";

		static class SettingsKeys
		{
			public const string HideOtherUI = "H17CubemapGenerator.HideOtherUI";
			public const string ShowTreeView = "H17CubemapGenerator.ShowTreeView";
			public const string ShowSkyBox = "H17CubemapGenerator.ShowSkyBox";
			public const string SuperSampling = "H17CubemapGenerator.SuperSampling";
			public const string PreviewObject = "H17CubemapGenerator.PreviewObject";
			public const string InputSource = "H17CubemapGenerator.InputSource";
			public const string OutputLayout = "H17CubemapGenerator.OutputLayout";
			public const string TextureWidth = "H17CubemapGenerator.TextureWidth";
			public const string EquirectangularRotation = "H17CubemapGenerator.EquirectangularRotation";
			public const string OutputHDR = "H17CubemapGenerator.OutputHDR";
			public const string OutputSRGB = "H17CubemapGenerator.OutputSRGB";
			public const string OutputCubemap = "H17CubemapGenerator.OutputCubemap";
			public const string OutputGenerateMipmap = "H17CubemapGenerator.OutputGenerateMipmap";
			public const string FillMatcapOutsideByEdgeColor = "H17CubemapGenerator.FillMatcapOutsideByEdgeColor";
			public const string Language = "H17CubemapGenerator.Language";
			public const string LastAssetPath = "H17CubemapGenerator.LastAssetPath";
			public const string LastAssetCubemap = "H17CubemapGenerator.LastAssetCubemap";
			public const string LastAssetTextureLeft = "H17CubemapGenerator.LastAssetTextureLeft";
			public const string LastAssetTextureRight = "H17CubemapGenerator.LastAssetTextureRight";
			public const string LastAssetTextureTop = "H17CubemapGenerator.LastAssetTextureTop";
			public const string LastAssetTextureBottom = "H17CubemapGenerator.LastAssetTextureBottom";
			public const string LastAssetTextureFront = "H17CubemapGenerator.LastAssetTextureFront";
			public const string LastAssetTextureBack = "H17CubemapGenerator.LastAssetTextureBack";
		}

		readonly EasyLocalization _localization = new EasyLocalization();

		bool _initialized;
		bool _requestRedraw;
		string _adviceMessage = string.Empty;
		readonly PropertyBool _hideOtherUIProp = new PropertyBool(SettingsKeys.HideOtherUI, false);
		readonly PropertyBool _showSkyBoxProp = new PropertyBool(SettingsKeys.ShowSkyBox, false);
		readonly PropertyBool _superSamplingProp = new PropertyBool(SettingsKeys.SuperSampling, true);
		public enum PreviewObjectType { Sphere, Cube, }
		readonly PropertyEnum<PreviewObjectType> _previewObjectProp = new PropertyEnum<PreviewObjectType>(SettingsKeys.PreviewObject, PreviewObjectType.Sphere);
		readonly PropertyEnum<H17CubemapGenerator.InputSource> _inputSourceProp = new PropertyEnum<H17CubemapGenerator.InputSource>(SettingsKeys.InputSource, H17CubemapGenerator.InputSource.CurrentScene);
		readonly PropertyEnum<H17CubemapGenerator.OutputLayout> _outputLayoutProp = new PropertyEnum<H17CubemapGenerator.OutputLayout>(SettingsKeys.OutputLayout, H17CubemapGenerator.OutputLayout.CrossHorizontal);
		public enum TextureWidthType { _64, _128, _256, _512, _1024, _2048 }
		readonly int[] _textureWidthArray = new[] { 64, 128, 256, 512, 1024, 2048, };
		readonly PropertyEnum<TextureWidthType> _textureWidthProp = new PropertyEnum<TextureWidthType>(SettingsKeys.TextureWidth, TextureWidthType._1024);
		readonly PropertyFloat _equirectangularRotationProp = new PropertyFloat(SettingsKeys.EquirectangularRotation, 0f);
		readonly PropertyBool _outputHDRProp = new PropertyBool(SettingsKeys.OutputHDR, false);
		readonly PropertyBool _outputSRGBProp = new PropertyBool(SettingsKeys.OutputSRGB, true);
		readonly PropertyBool _outputCubemapProp = new PropertyBool(SettingsKeys.OutputCubemap, true);
		readonly PropertyBool _outputGenerateMipmapProp = new PropertyBool(SettingsKeys.OutputGenerateMipmap, true);
		readonly PropertyBool _fillMatcapOutsideByEdgeColorProp = new PropertyBool(SettingsKeys.FillMatcapOutsideByEdgeColor, false);
		readonly PropertyObject<Camera> _specificCameraProp = new PropertyObject<Camera>(null);
		string _lastAssetPath = string.Empty;
		readonly PropertyAsset<Cubemap> _cubemapProp = new PropertyAsset<Cubemap>(SettingsKeys.LastAssetCubemap, null);
		readonly PropertyAsset<Texture2D> _textureLeftProp = new PropertyAsset<Texture2D>(SettingsKeys.LastAssetTextureLeft, null);
		readonly PropertyAsset<Texture2D> _textureRightProp = new PropertyAsset<Texture2D>(SettingsKeys.LastAssetTextureRight, null);
		readonly PropertyAsset<Texture2D> _textureTopProp = new PropertyAsset<Texture2D>(SettingsKeys.LastAssetTextureTop, null);
		readonly PropertyAsset<Texture2D> _textureBottomProp = new PropertyAsset<Texture2D>(SettingsKeys.LastAssetTextureBottom, null);
		readonly PropertyAsset<Texture2D> _textureFrontProp = new PropertyAsset<Texture2D>(SettingsKeys.LastAssetTextureFront, null);
		readonly PropertyAsset<Texture2D> _textureBackProp = new PropertyAsset<Texture2D>(SettingsKeys.LastAssetTextureBack, null);
		readonly PropertyEnum<SystemLanguage> _languageProp;

		public bool initialized => _initialized;
		public string adviceMessage => _adviceMessage;
		public bool hideOtherUI { get => _hideOtherUIProp.value; set => _hideOtherUIProp.value = value; }
		public bool showSkyBox { get => _showSkyBoxProp.value; set => _showSkyBoxProp.value = value; }
		public bool superSampling { get => _superSamplingProp.value; set => _superSamplingProp.value = value; }
		public PreviewObjectType previewObject { get => _previewObjectProp.value; set => _previewObjectProp.value = value; }
		public H17CubemapGenerator.InputSource inputSource { get => _inputSourceProp.value; set => _inputSourceProp.value = value; }
		public H17CubemapGenerator.OutputLayout outputLayout { get => _outputLayoutProp.value; set => _outputLayoutProp.value = value; }
		public TextureWidthType textureWidth { get => _textureWidthProp.value; set => _textureWidthProp.value = value; }
		public float equirectangularRotation { get => _equirectangularRotationProp.value; set => _equirectangularRotationProp.value = value; }
		public bool outputHDR { get => _outputHDRProp.value; set => _outputHDRProp.value = value; }
		public bool outputSRGB { get => _outputSRGBProp.value; set => _outputSRGBProp.value = value; }
		public bool outputCubemap { get => _outputCubemapProp.value; set => _outputCubemapProp.value = value; }
		public bool outputGenerateMipmap { get => _outputGenerateMipmapProp.value; set => _outputGenerateMipmapProp.value = value; }
		public bool fillMatcapOutsideByEdgeColor { get => _fillMatcapOutsideByEdgeColorProp.value; set => _fillMatcapOutsideByEdgeColorProp.value = value; }
		public SystemLanguage language { get => _languageProp.value; set => _languageProp.value = value; }
		public Camera? specificCamera { get => _specificCameraProp.value; set => _specificCameraProp.value = value; }
		public Cubemap? cubemap { get => _cubemapProp.value; set => _cubemapProp.value = value; }
		public Texture2D? textureLeft { get => _textureLeftProp.value; set => _textureLeftProp.value = value; }
		public Texture2D? textureRight { get => _textureRightProp.value; set => _textureRightProp.value = value; }
		public Texture2D? textureTop { get => _textureTopProp.value; set => _textureTopProp.value = value; }
		public Texture2D? textureBottom { get => _textureBottomProp.value; set => _textureBottomProp.value = value; }
		public Texture2D? textureFront { get => _textureFrontProp.value; set => _textureFrontProp.value = value; }
		public Texture2D? textureBack { get => _textureBackProp.value; set => _textureBackProp.value = value; }
		public bool isSourceHDR { get => (this.generatorInstance != null) ? this.generatorInstance.isSourceHDR : false; }

		public Action<SystemLanguage>? onLanguageChanged;
		public IReadOnlyList<SystemLanguage> supportedLanguages => _localization.supportedLanguages;

		public RenderPipelineUtils.PipelineType pipelineType => (this.generatorInstance != null) ? this.generatorInstance.pipelineType : RenderPipelineUtils.PipelineType.Unsupported;

		public H17CubemapGeneratorEditorContext()
		{
			_lastAssetPath = PropertyKeyValue.LoadString(SettingsKeys.LastAssetPath, _lastAssetPath);
			this.previewScene = new PreviewScene();
			this.previewScene.camera.transform.position = new Vector3(0f, 0f, -2);

			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetPathCubemapGeneratorPrefab);
			if (prefab == null)
			{
				throw new InvalidOperationException($"{AssetPathCubemapGeneratorPrefab} not found");
			}
			var goGenerator = GameObject.Instantiate(prefab);
			goGenerator.hideFlags = HideFlags.HideAndDontSave;
			this.generatorInstance = goGenerator.GetComponent<H17CubemapGenerator>();
			this.generatorInstance.SetEditorDragControl(true);

			this.generatorInstance.previewSphere.transform.SetParent(null);
			this.previewScene.AddGameObject(this.generatorInstance.previewSphere);
			this.generatorInstance.previewCube.transform.SetParent(null);
			this.previewScene.AddGameObject(this.generatorInstance.previewCube);

			_languageProp = new PropertyEnum<SystemLanguage>(SettingsKeys.Language, _localization.language);
			_localization.language = _languageProp.value;
			_languageProp.onValueChanged += (value) =>
			{
				_localization.language = value;
				this.onLanguageChanged?.Invoke(value);
			};

			_specificCameraProp.onValueChanged += (value) => RequestRedraw();
			_previewObjectProp.onValueChanged += (value) => UpdatePreviewObject();
			_inputSourceProp.onValueChanged += (value) => RequestRedraw();
			_textureWidthProp.onValueChanged += (value) => RequestRedraw();
			_cubemapProp.onValueChanged += (value) => RequestRedraw();
			_textureLeftProp.onValueChanged += (value) => RequestRedraw();
			_textureRightProp.onValueChanged += (value) => RequestRedraw();
			_textureTopProp.onValueChanged += (value) => RequestRedraw();
			_textureBottomProp.onValueChanged += (value) => RequestRedraw();
			_textureFrontProp.onValueChanged += (value) => RequestRedraw();
			_textureBackProp.onValueChanged += (value) => RequestRedraw();

			this.generatorInstance.SetPreviewObject(H17CubemapGenerator.PreviewObject.None);
			RequestRedraw();
			// UpdatePreviewObject() is called when redraw is completed by RequestRdraw

			_initialized = true;
		}

		public void Dispose()
		{
			this.previewScene?.Dispose();
			if (this.generatorInstance != null)
			{
				GameObject.DestroyImmediate(this.generatorInstance);
				this.generatorInstance = null;
			}
		}

		public void OnUpdate(float deltaTime)
		{
			if (_requestRedraw)
			{
				_requestRedraw = false;
				StartRedraw();
			}

			UpdateAnimation(deltaTime);
		}

		public void RequestRedraw()
		{
			_requestRedraw = true;
		}

		public void UpdateAnimation(float deltaTime)
		{
		}

		void StartRedraw()
		{
			if (!_initialized)
			{
				return;
			}
			if (this.generatorInstance == null) throw new InvalidOperationException();

			if (!CanRender())
			{
				ClearCubemap();
				return;
			}

			switch (this.inputSource)
			{
				case H17CubemapGenerator.InputSource.CurrentScene:
					this.generatorInstance.SetSpecificCamera(this.specificCamera);
					this.generatorInstance.SetCubemapWidth(_textureWidthArray[(int)this.textureWidth]);
					break;

				case H17CubemapGenerator.InputSource.Cubemap:
					this.generatorInstance.SetSourceCubamap(this.cubemap!);
					break;

				case H17CubemapGenerator.InputSource.SixSided:
					var textures = GetSixSidedTextures();
					for (int i = 0; i < textures.Length; i++)
					{
						this.generatorInstance.SetSourceTexture((CubemapFace)i, textures[i]!);
					}
					break;
			}

			this.generatorInstance.StartRender(this.inputSource, onCompleted: () =>
			{
				UpdatePreviewObject();
			});
		}

		void UpdatePreviewObject()
		{
			if (this.generatorInstance == null) throw new InvalidOperationException();

			switch (_previewObjectProp.value)
			{
				case PreviewObjectType.Sphere:
					this.generatorInstance.SetPreviewObject(H17CubemapGenerator.PreviewObject.Sphere);
					break;
				case PreviewObjectType.Cube:
					this.generatorInstance.SetPreviewObject(H17CubemapGenerator.PreviewObject.Cube);
					break;
			}
		}

		public void ResetPreviewRotation()
		{
			if (this.generatorInstance == null) throw new InvalidOperationException();

			this.generatorInstance.ResetPreviewCubeRotation();
		}

		public void ExportSaveCubemap()
		{
			if (this.generatorInstance == null) throw new InvalidOperationException();

			this.generatorInstance.SetFillMatcapOutsideByEdgeColor(this.fillMatcapOutsideByEdgeColor);
			this.generatorInstance.SetOutputDesirableHDR(this.outputHDR);
			this.generatorInstance.SetOutputDesirableSRGB(this.outputSRGB);
			this.generatorInstance.SetOutputDesirableCubemap(this.outputCubemap);
			this.generatorInstance.SetOutputDesirableGenerateMipmap(this.outputGenerateMipmap);
			this.generatorInstance.SetCubemapOutputEquirectanglarRotation(this.equirectangularRotation);
			this.generatorInstance.SetCubemapOutputLayout(this.outputLayout);
			this.generatorInstance.SaveAsset(_lastAssetPath, onCompleted: (path) =>
			{
				_lastAssetPath = path;
				PropertyKeyValue.SaveString(SettingsKeys.LastAssetPath, _lastAssetPath);
			});
		}

		public void ClearCubemap()
		{
			if (this.generatorInstance == null) throw new InvalidOperationException();

			this.generatorInstance.ClearCubemap();
		}

		Texture2D?[] GetSixSidedTextures()
		{
			return new Texture2D?[] { this.textureLeft, this.textureRight, this.textureTop, this.textureBottom, this.textureFront, this.textureBack };
		}

		public bool CanRender()
		{
			switch (this.inputSource)
			{
				case H17CubemapGenerator.InputSource.Cubemap:
					if (this.cubemap == null)
					{
						_adviceMessage = GetText(TextId.AdviceSetCubemap);
						return false;
					}
					break;

				case H17CubemapGenerator.InputSource.SixSided:
					var textures = GetSixSidedTextures();
					int requiredWidth = 0;
					TextureFormat requiredFormat = TextureFormat.RGBA32;
					for (int i = 0; i < textures.Length; i++)
					{
						var tex = textures[i];
						if (tex == null)
						{
							_adviceMessage = GetText(TextId.AdviceSetSixSidedTexture);
							return false;
						}
						if (i == 0)
						{
							if (tex.width != tex.height)
							{
								_adviceMessage = GetText(TextId.AdviceTextureWidthAndHeightEqual);
								return false;
							}
							requiredWidth = tex.width;
							requiredFormat = tex.format;
						}
						else
						{
							if (tex.width != requiredWidth ||
								tex.height != requiredWidth)
							{
								_adviceMessage = GetText(TextId.AdviceEachTextureFaceSameSize);
								return false;
							}
							if (tex.format != requiredFormat)
							{
								_adviceMessage = GetText(TextId.AdviceEachTextureFacesSameFormat);
								return false;
							}
						}
					}
					return true;
				default:
					break;
			}
			_adviceMessage = string.Empty;
			return true;
		}

		public string GetText(TextId id)
		{
			return _localization.Get(id);
		}
	}
}
