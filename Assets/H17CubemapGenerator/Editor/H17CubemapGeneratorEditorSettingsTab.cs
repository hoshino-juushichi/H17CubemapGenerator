using UnityEngine;
using UnityEditor;

#nullable enable

namespace Hoshino17
{
	public sealed class H17CubemapGeneratorEditorSettingsTab : IH17CubemapGeneratorEditorTabView
	{
		H17CubemapGeneratorEditorContext _context = null!;
		IH17CubemapGeneratorEditor _editor = null!;

		string[] _languageOptions = null!;

		void IH17CubemapGeneratorEditorTabView.Initialize(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor)
		{
			_context = context;
			_editor = editor;

			_languageOptions = EnumUtils.StringListOfEnum<SystemLanguage>(_context.supportedLanguages).ToArray();
		}

		void IH17CubemapGeneratorEditorTabView.OnEnable() {}
		void IH17CubemapGeneratorEditorTabView.OnDisable() {}
		void IH17CubemapGeneratorEditorTabView.OnDestroy() {}

		void IH17CubemapGeneratorEditorTabView.OnUpdate(bool isTabActive)
		{
		}

		void IH17CubemapGeneratorEditorTabView.OnGUI()
		{
			if (_context.hideOtherUI)
			{
				return;
			}

			EditorGUI.BeginChangeCheck();
			int languageIndex = _context.supportedLanguages.IndexOf(_context.language);
			languageIndex = EditorGUILayout.Popup(_context.GetText(TextId.Language), languageIndex, _languageOptions, GUILayout.Width(220));
			if (EditorGUI.EndChangeCheck())
			{
				_context.language = _context.supportedLanguages[languageIndex];
			}

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.LabelField(_context.pipelineType.ToString(), EditorStyles.helpBox, GUILayout.Width(220));
			EditorGUI.EndDisabledGroup();
		}
	}
}
