using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Hoshino17
{
	public interface IH17CubemapGeneratorEditor
	{
		EditorWindow window { get; }
		Rect mainViewRect { get; }
	}

	public interface IH17CubemapGeneratorEditorTabView
	{
		void Initialize(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor);
		void OnEnable();
		void OnDisable();
		void OnDestroy();
		void OnGUI();
		void OnUpdate(bool isTabActive);
	}

	public interface ICubemapGeneratorPreviewSceneRenderer
	{
		void OnGUIFirst();
	}
	
	public sealed class H17CubemapGeneratorEditor : EditorWindow, IH17CubemapGeneratorEditor
	{
		EditorWindow IH17CubemapGeneratorEditor.window => this;

		int _tabIndex;
		H17CubemapGeneratorEditorContext _context = null!;

		readonly List<string> _tabNameList = new List<string>();
		readonly List<IH17CubemapGeneratorEditorTabView> _tabViewList = new List<IH17CubemapGeneratorEditorTabView>();

		Rect _mainViewRect = new Rect();
		public Rect mainViewRect => _mainViewRect;

		[MenuItem("Tools/H17CubemapGenerator", false, 1)]
		static void Create()
		{
			var window = GetWindow<H17CubemapGeneratorEditor>();
		}

		void Initialize()
		{
			titleContent = new GUIContent("H17CubemapGenerator");
			_context = new H17CubemapGeneratorEditorContext();

			this.wantsMouseMove = true;

			BuildOptionStringList();
			_context.onLanguageChanged += (value) => BuildOptionStringList();

			_tabViewList.Add(new H17CubemapGeneratorEditorMainTab());
			_tabViewList.Add(new H17CubemapGeneratorEditorPreviewTab());
			_tabViewList.Add(new H17CubemapGeneratorEditorSettingsTab());

			foreach (var tab in _tabViewList) { tab.Initialize(_context, this); }
		}

		void BuildOptionStringList()
		{
			_tabNameList.Clear();
			_tabNameList.Add(_context.GetText(TextId.TabMain));
			_tabNameList.Add(_context.GetText(TextId.TabPreview));
			_tabNameList.Add(_context.GetText(TextId.TabSettings));
		}

		void OnEnable()
		{
			if (_context == null)
			{
				Initialize();
			}
			foreach (var tab in _tabViewList) { tab.OnEnable(); }
		}

		void OnDisable()
		{
			foreach (var tab in _tabViewList) { tab.OnDisable(); }
		}

		void OnDestroy()
		{
			foreach (var tab in _tabViewList) { tab.OnDestroy(); }
			_context?.Dispose();
		}

		void OnGUI()
		{
			if (_context == null || !_context.initialized)
			{
				return;
			}

			CalcurateViewSize();

			OnGUIManageMouseEvent();

			foreach (var tab in _tabViewList)
			{
				if (tab is ICubemapGeneratorPreviewSceneRenderer previewTab)
				{
					previewTab.OnGUIFirst();
				}
			}

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				_tabIndex = GUILayout.Toolbar(_tabIndex, _tabNameList.ToArray(), new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
			}

			OnGUICommon();

			_tabViewList[_tabIndex]?.OnGUI();
		}

		void Update()
		{
			if (_context == null || !_context.initialized)
			{
				return;
			}

			for (int i = 0; i < _tabViewList.Count; i++)
			{
				_tabViewList[i].OnUpdate(_tabIndex == i);
			}
		}

		void OnGUIManageMouseEvent()
		{
			if (Event.current == null)
			{
				return;
			}
			if (Event.current.type == EventType.MouseMove ||
				Event.current.type == EventType.MouseDrag)
			{
				_context.generatorInstance?.SetEditorMousePosition(Event.current.mousePosition);
			}
			if (Event.current.type == EventType.MouseDown)
			{
				_context.generatorInstance?.SetEditorMouseDown();
			}
			if (Event.current.type == EventType.MouseMove ||
				Event.current.type == EventType.MouseUp)
			{
				_context.generatorInstance?.SetEditorMouseUp();
			}
		}

		void OnGUICommon()
		{
			_context.hideOtherUI = EditorGUILayout.Toggle(_context.GetText(TextId.HideOtherUIs), _context.hideOtherUI);
			if (_context.hideOtherUI)
			{
				return;
			}
		}

		void CalcurateViewSize()
		{
			float toolbarHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

			_mainViewRect = new Rect(0f, toolbarHeight, this.position.width, this.position.height - toolbarHeight);
		}
	}
}
