using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Hoshino17
{
    public interface IH17CubemapGeneratorEditorTabView
    {
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
	
    public abstract class H17CubemapGeneratorEditorTabBase : IH17CubemapGeneratorEditorTabView
    {
        H17CubemapGeneratorEditorContext _context = null!;
        protected H17CubemapGeneratorEditorContext context => _context;

        IH17CubemapGeneratorEditor _editor = null!;
        protected IH17CubemapGeneratorEditor editor => _editor;

        protected H17CubemapGeneratorEditorTabBase(H17CubemapGeneratorEditorContext context, IH17CubemapGeneratorEditor editor)
        {
            _context = context;
            _editor = editor;
        }

        public virtual void OnEnable() {}
        public virtual void OnDisable() {}
        public virtual void OnDestroy()
        {
            _context = null!;
            _editor = null!;
        }
        public virtual void OnUpdate(bool isTabActive) {}
        public virtual void OnGUI() {}
    }
}
