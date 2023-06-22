using UnityEngine;
using UnityEditor;

#nullable enable

namespace Hoshino17
{
	public sealed class H17CubemapGeneratorEditorSettingsTab : H17CubemapGeneratorEditorTabBase
	{
		readonly string[] _languageOptions = null!;

		public H17CubemapGeneratorEditorSettingsTab(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor)
			: base(context, editor)
		{
			_languageOptions = EnumUtils.StringListOfEnum<SystemLanguage>(context.supportedLanguages).ToArray();
		}

		public override void OnGUI()
		{
			if (context.hideOtherUI)
			{
				return;
			}

			EditorGUI.BeginChangeCheck();
			int languageIndex = context.supportedLanguages.IndexOf(context.language);
			languageIndex = EditorGUILayout.Popup(context.GetText(TextId.Language), languageIndex, _languageOptions, GUILayout.Width(220));
			if (EditorGUI.EndChangeCheck())
			{
				context.language = context.supportedLanguages[languageIndex];
			}

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.LabelField(context.pipelineType.ToString(), EditorStyles.helpBox, GUILayout.Width(220));
			EditorGUI.EndDisabledGroup();
		}
	}
}
