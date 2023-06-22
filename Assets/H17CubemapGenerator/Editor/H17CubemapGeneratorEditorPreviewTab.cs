using UnityEngine;
using UnityEditor;

#nullable enable

namespace Hoshino17
{
	public sealed class H17CubemapGeneratorEditorPreviewTab : H17CubemapGeneratorEditorTabBase, ICubemapGeneratorPreviewSceneRenderer
	{
		string[] _previewObjectOptions = null!;

		public H17CubemapGeneratorEditorPreviewTab(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor)
			: base(context, editor)
		{
			BuildOptionStringList();
			context.onLanguageChanged += (value) => BuildOptionStringList();
		}

		void BuildOptionStringList()
		{
			//_previewObjectOptions = EnumUtils.StringListOfEnum<H17CubemapGeneratorEditorContext.PreviewObjectType>().ToArray();
			_previewObjectOptions = new string[]
			{
				context.GetText(TextId.PreviewObjectSphere),
				context.GetText(TextId.PreviewObjectCube),
			};
		}

		void ICubemapGeneratorPreviewSceneRenderer.OnGUIFirst()
		{
			var viewRect = editor.mainViewRect;
			int width = (int)viewRect.width;
			int height = (int)viewRect.height;
			int superSize = context.superSampling ? 2 : 1;
			context.previewScene.renderTextureSize = new Vector2Int(width * superSize, height * superSize);
			context.previewScene.Render(true);
			GUI.DrawTexture(viewRect, context.previewScene.renderTexture);
		}

		public override void OnUpdate(bool isTabActive)
		{
			editor.window.Repaint();

			context.OnUpdate(Time.unscaledDeltaTime);
			context.previewScene.camera.clearFlags = context.showSkyBox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
		}

		public override void OnGUI()
		{
			if (context.hideOtherUI)
			{
				return;
			}
			EditorGUI.BeginChangeCheck();
			int previewObjectIndex = EnumUtils.IndexOfEnum(context.previewObject);
			previewObjectIndex = EditorGUILayout.Popup(context.GetText(TextId.PreviewObject), previewObjectIndex, _previewObjectOptions, GUILayout.Width(220));
			if (EditorGUI.EndChangeCheck())
			{
				context.previewObject = EnumUtils.EnumByIndex<H17CubemapGeneratorEditorContext.PreviewObjectType>(previewObjectIndex);
			}

			if (GUILayout.Button(context.GetText(TextId.ResetPreviewRotation), GUILayout.Width(160)))
			{
				context.ResetPreviewRotation();
			}

			context.superSampling = EditorGUILayout.Toggle(context.GetText(TextId.PreviewSupersampling), context.superSampling);
			context.showSkyBox = EditorGUILayout.Toggle(context.GetText(TextId.PreviewSkybox), context.showSkyBox);
		}
	}
}
