﻿using System;
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

	public sealed class H17CubemapGeneratorEditor : EditorWindow, IH17CubemapGeneratorEditor
	{
		EditorWindow IH17CubemapGeneratorEditor.window => this;

		int _tabIndex;
		H17CubemapGeneratorEditorContext? _context = null!;

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
			_context.onLanguageChanged += (_) => BuildOptionStringList();

			_tabViewList.Add(new H17CubemapGeneratorEditorMainTab(_context, this));
			_tabViewList.Add(new H17CubemapGeneratorEditorPreviewTab(_context, this));
			_tabViewList.Add(new H17CubemapGeneratorEditorSettingsTab(_context, this));
		}

		void BuildOptionStringList()
		{
			if (_context == null) { throw new InvalidOperationException(); }
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
			_tabViewList.Clear();
			_context?.Dispose();
			_context = null!;
		}

		void OnGUI()
		{
			if (_context == null)
			{
				return;
			}

			CalculateViewSize();

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
			if (_context == null)
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
			if (_context == null) { throw new InvalidOperationException(); }
			if (_context.generatorInstance == null) { throw new InvalidOperationException(); }

			if (Event.current.type == EventType.MouseMove ||
				Event.current.type == EventType.MouseDrag)
			{
				_context.generatorInstance.SetEditorMousePosition(Event.current.mousePosition);
			}
			if (Event.current.type == EventType.MouseDown)
			{
				_context.generatorInstance.SetEditorMouseDown();
			}
			if (Event.current.type == EventType.MouseMove ||
				Event.current.type == EventType.MouseUp)
			{
				_context.generatorInstance.SetEditorMouseUp();
			}
		}

		void OnGUICommon()
		{
			if (_context == null) { throw new InvalidOperationException(); }
			
			_context.hideOtherUI = EditorGUILayout.Toggle(_context.GetText(TextId.HideOtherUIs), _context.hideOtherUI);
			if (_context.hideOtherUI)
			{
				return;
			}
		}

		void CalculateViewSize()
		{
			float toolbarHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

			_mainViewRect = new Rect(0f, toolbarHeight, this.position.width, this.position.height - toolbarHeight);
		}
	}
}
