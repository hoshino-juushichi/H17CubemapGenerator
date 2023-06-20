using UnityEngine;
using UnityEditor;

#nullable enable

namespace Hoshino17
{
	public sealed class H17CubemapGeneratorEditorPreviewTab : IH17CubemapGeneratorEditorTabView, ICubemapGeneratorPreviewSceneRenderer
	{
		H17CubemapGeneratorEditorContext _context = null!;
		IH17CubemapGeneratorEditor _editor = null!;

		string[] _previewObjectOptions = null!;

		void IH17CubemapGeneratorEditorTabView.Initialize(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor)
		{
			_context = context;
			_editor = editor;

			BuildOptionStringList();
			_context.onLanguageChanged += (value) => BuildOptionStringList();
		}

		void BuildOptionStringList()
		{
			//_previewObjectOptions = EnumUtils.StringListOfEnum<H17CubemapGeneratorEditorContext.PreviewObjectType>().ToArray();
			_previewObjectOptions = new string[]
			{
				_context.GetText(TextId.PreviewObjectSphere),
				_context.GetText(TextId.PreviewObjectCube),
			};
		}

		void IH17CubemapGeneratorEditorTabView.OnEnable() {}
		void IH17CubemapGeneratorEditorTabView.OnDisable() {}
		void IH17CubemapGeneratorEditorTabView.OnDestroy()
		{
			_context = null!;
			_editor = null!;
		}

		void ICubemapGeneratorPreviewSceneRenderer.OnGUIFirst()
		{
			var viewRect = _editor.mainViewRect;
			int width = (int)viewRect.width;
			int height = (int)viewRect.height;
			int superSize = _context.superSampling ? 2 : 1;
			_context.previewScene.renderTextureSize = new Vector2Int(width * superSize, height * superSize);
			_context.previewScene.Render(true);
			GUI.DrawTexture(viewRect, _context.previewScene.renderTexture);
		}

		void IH17CubemapGeneratorEditorTabView.OnUpdate(bool isTabActive)
		{
			_editor.window.Repaint();

			_context.OnUpdate(Time.unscaledDeltaTime);
			_context.previewScene.camera.clearFlags = _context.showSkyBox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
			ModifyPreviewSceneCamera();
		}

		void IH17CubemapGeneratorEditorTabView.OnGUI()
		{
			if (_context.hideOtherUI)
			{
				return;
			}
			EditorGUI.BeginChangeCheck();
			int previewObjectIndex = EnumUtils.IndexOfEnum(_context.previewObject);
			previewObjectIndex = EditorGUILayout.Popup(_context.GetText(TextId.PreviewObject), previewObjectIndex, _previewObjectOptions, GUILayout.Width(220));
			if (EditorGUI.EndChangeCheck())
			{
				_context.previewObject = EnumUtils.EnumByIndex<H17CubemapGeneratorEditorContext.PreviewObjectType>(previewObjectIndex);
			}

			if (GUILayout.Button(_context.GetText(TextId.ResetPreviewRotation), GUILayout.Width(160)))
			{
				_context.ResetPreviewRotation();
			}

			_context.superSampling = EditorGUILayout.Toggle(_context.GetText(TextId.PreviewSupersampling), _context.superSampling);
			_context.showSkyBox = EditorGUILayout.Toggle(_context.GetText(TextId.PreviewSkybox), _context.showSkyBox);
		}

		void ModifyPreviewSceneCamera()
		{
			if (SceneView.lastActiveSceneView == null ||
				_context.generatorInstance == null)
			{
				return;
			}
		}
	}
}
