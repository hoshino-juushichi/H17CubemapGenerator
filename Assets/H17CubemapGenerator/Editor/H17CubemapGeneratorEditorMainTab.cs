using UnityEngine;
using UnityEditor;

#nullable enable

namespace Hoshino17
{
	public sealed class H17CubemapGeneratorEditorMainTab : IH17CubemapGeneratorEditorTabView
	{
		H17CubemapGeneratorEditorContext _context = null!;

		string[] _inputSourceOptions = null!;
		string[] _outputLayoutOptions = null!;
		string[] _textureWidthOptions = null!;
		string[] _textureShapeOptions = null!;

		void IH17CubemapGeneratorEditorTabView.Initialize(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor)
		{
			_context = context;

			BuildOptionStringList();
			_context.onLanguageChanged += (value) => BuildOptionStringList();
		}

		void BuildOptionStringList()
		{
			//_inputSourceOptions = EnumUtils.StringListOfEnum<H17CubemapGenerator.InputSource>().ToArray();
			_inputSourceOptions = new string[] {
				_context.GetText(TextId.InputSourceCurrentScene),
				_context.GetText(TextId.InputSourceSixSided),
				_context.GetText(TextId.InputSourceCubemap),
			};

			//_outputLayoutOptions = EnumUtils.StringListOfEnum<H17CubemapGenerator.OutputLayout>().ToArray();
			_outputLayoutOptions = new string[]
			{
				_context.GetText(TextId.OutputLayoutCrossHorizontal),
				_context.GetText(TextId.OutputLayoutCrossVertical),
				_context.GetText(TextId.OutputLayoutStraitHorizontal),
				_context.GetText(TextId.OutputLayoutStraitVertical),
				_context.GetText(TextId.OutputLayoutSixSided),
				_context.GetText(TextId.OutputLayoutEquirectangular),
				_context.GetText(TextId.OutputLayoutMatcap),
			};

			_textureWidthOptions = EnumUtils.StringListOfEnum<H17CubemapGeneratorEditorContext.TextureWidthType>().ToArray();
			Replace(_textureWidthOptions, "_", string.Empty);

			_textureShapeOptions = new string[]
			{
				_context.GetText(TextId.TextureShape2D),
				_context.GetText(TextId.TextureShapeCube),
			};
		}

		void IH17CubemapGeneratorEditorTabView.OnEnable() {}
		void IH17CubemapGeneratorEditorTabView.OnDisable() {}
		void IH17CubemapGeneratorEditorTabView.OnDestroy() {}

		void IH17CubemapGeneratorEditorTabView.OnGUI()
		{
			if (_context.hideOtherUI)
			{
				return;
			}
			EditorGUI.BeginChangeCheck();
			int inputSourceIndex = EnumUtils.IndexOfEnum(_context.inputSource);
			inputSourceIndex = EditorGUILayout.Popup(_context.GetText(TextId.SelectInputSource), inputSourceIndex, _inputSourceOptions, GUILayout.Width(270));
			if (EditorGUI.EndChangeCheck())
			{
				_context.inputSource = EnumUtils.EnumByIndex<H17CubemapGenerator.InputSource>(inputSourceIndex);
			}

			if (!string.IsNullOrEmpty(_context.adviceMessage))
			{
				EditorGUILayout.LabelField(_context.adviceMessage, EditorStyles.helpBox, GUILayout.Width(420));
			}

			switch (_context.inputSource)
			{
				case H17CubemapGenerator.InputSource.CurrentScene:
					int textureWidthIndex = EnumUtils.IndexOfEnum(_context.textureWidth);
					textureWidthIndex = EditorGUILayout.Popup(_context.GetText(TextId.InputTextureWidth), textureWidthIndex, _textureWidthOptions, GUILayout.Width(270));
					if (EditorGUI.EndChangeCheck())
					{
						_context.textureWidth = EnumUtils.EnumByIndex<H17CubemapGeneratorEditorContext.TextureWidthType>(textureWidthIndex);
					}
					_context.specificCamera = EditorGUILayout.ObjectField(_context.GetText(TextId.SpecificCamera), _context.specificCamera, typeof(Camera), false, GUILayout.Width(220)) as Camera;

					break;

				case H17CubemapGenerator.InputSource.Cubemap:
					_context.cubemap = EditorGUILayout.ObjectField(_context.GetText(TextId.InputCubemap), _context.cubemap, typeof(Cubemap), false, GUILayout.Width(220)) as Cubemap;
					break;
				case H17CubemapGenerator.InputSource.SixSided:
					GUILayout.BeginHorizontal();
					_context.textureLeft = EditorGUILayout.ObjectField(_context.GetText(TextId.InputLeft), _context.textureLeft, typeof(Texture2D), false, GUILayout.Width(220)) as Texture2D;
					_context.textureRight = EditorGUILayout.ObjectField(_context.GetText(TextId.InputRight), _context.textureRight, typeof(Texture2D), false, GUILayout.Width(220)) as Texture2D;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					_context.textureTop = EditorGUILayout.ObjectField(_context.GetText(TextId.InputTop), _context.textureTop, typeof(Texture2D), false, GUILayout.Width(220)) as Texture2D;
					_context.textureBottom = EditorGUILayout.ObjectField(_context.GetText(TextId.InputBottom), _context.textureBottom, typeof(Texture2D), false, GUILayout.Width(220)) as Texture2D;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					_context.textureFront = EditorGUILayout.ObjectField(_context.GetText(TextId.InputFront), _context.textureFront, typeof(Texture2D), false, GUILayout.Width(220)) as Texture2D;
					_context.textureBack = EditorGUILayout.ObjectField(_context.GetText(TextId.InputBack), _context.textureBack, typeof(Texture2D), false, GUILayout.Width(220)) as Texture2D;
					GUILayout.EndHorizontal();
					break;
			}

			EditorGUI.BeginDisabledGroup(!_context.CanRender());
			if (GUILayout.Button(_context.GetText(TextId.Redraw), GUILayout.Width(120)))
			{
				_context.RequestRedraw();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Separator();

			EditorGUI.BeginChangeCheck();
			int outputLayoutIndex = EnumUtils.IndexOfEnum(_context.outputLayout);
			outputLayoutIndex = EditorGUILayout.Popup(_context.GetText(TextId.SelectOutputLayout), outputLayoutIndex, _outputLayoutOptions, GUILayout.Width(320));
			if (EditorGUI.EndChangeCheck())
			{
				_context.outputLayout = EnumUtils.EnumByIndex<H17CubemapGenerator.OutputLayout>(outputLayoutIndex);
			}

			EditorGUI.BeginDisabledGroup(!_context.isSourceHDR);
			_context.outputHDR = EditorGUILayout.Toggle(_context.GetText(TextId.OutputFormatHDR), _context.outputHDR, GUILayout.Width(220));
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!H17CubemapGenerator.IsOutputLayoutAvailableBeCubemap(_context.outputLayout));

			EditorGUI.BeginChangeCheck();
			int textureShapeIndex = _context.outputCubemap ? 1 : 0;
			textureShapeIndex = EditorGUILayout.Popup(_context.GetText(TextId.TextureShape), textureShapeIndex, _textureShapeOptions, GUILayout.Width(320));
			if (EditorGUI.EndChangeCheck())
			{
				_context.outputCubemap = (textureShapeIndex != 0);
			}
			EditorGUI.EndDisabledGroup();

			_context.outputSRGB = EditorGUILayout.Toggle(_context.GetText(TextId.OutputSRGB), _context.outputSRGB, GUILayout.Width(220));
			_context.outputGenerateMipmap = EditorGUILayout.Toggle(_context.GetText(TextId.OutputGenerateMipmap), _context.outputGenerateMipmap, GUILayout.Width(220));

			switch (_context.outputLayout)
			{
				case H17CubemapGenerator.OutputLayout.Equirectanglar:
					_context.equirectangularRotation = (float)EditorGUILayout.IntField(_context.GetText(TextId.EquirectangularRotation), (int)_context.equirectangularRotation, GUILayout.Width(220));
					break;
				case H17CubemapGenerator.OutputLayout.Matcap:
					_context.fillMatcapOutsideByEdgeColor = EditorGUILayout.Toggle(_context.GetText(TextId.FillMatcapOutsideByEdgeColor), _context.fillMatcapOutsideByEdgeColor, GUILayout.Width(220));
					break;
			}

			if (GUILayout.Button(_context.GetText(TextId.Export), GUILayout.Width(120)))
			{
				_context.ExportSaveCubemap();
			}
		}

		void IH17CubemapGeneratorEditorTabView.OnUpdate(bool isTabActive)
		{
		}

		static void Replace(string[] strings, string oldValue, string newValue)
		{
			for (int i = 0; i < strings.Length; i++)
			{
				strings[i] = strings[i].Replace(oldValue, newValue);
			}
		}
	}
}
