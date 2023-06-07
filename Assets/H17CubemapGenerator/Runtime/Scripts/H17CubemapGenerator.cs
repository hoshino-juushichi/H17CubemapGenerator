using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#nullable enable
#pragma warning disable 414 // The field '' is assigned but its value is never used

namespace Hoshino17
{
	public interface IH17CubemapGenerator
	{
		bool isAvailableInspector6Sided { get; }
		void SetPreviewObject(H17CubemapGenerator.PreviewObject previewObject);
		void SetSpecificCamera(Camera? specificCamera);
		void SetCubemapWidth(int textureWidth);
		void StartRender(H17CubemapGenerator.InputSource inputSource, Action? onCompleted = null);
		bool isProcessing { get; }
		bool isPipelingChanging { get; }
		void ResetPreviewCubeRotation();
		public void ClearCubemap();
		void ClearDragging();

#if UNITY_EDITOR
		GameObject previewSphere { get; }
		GameObject previewCube { get; }
		bool isSourceHDR { get; }
		void SetCubemapOutputEquirectanglarRotation(float angleEulerY);
		void SetCubemapOutputLayout(H17CubemapGenerator.OutputLayout cubemapOutputLayout);
		void SaveAsset(string path, Action<string>? onCompleted = null);
		void SetOutputDesirableHDR(bool value);
		void SetOutputDesirableSRGB(bool value);
		void SetOutputDesirableCubemap(bool value);
		void SetOutputDesirableGenerateMipmap(bool value);
		void SetFillMatcapOutsideByEdgeColor(bool value);
		void SetEditorDragControl(bool value);
		void SetEditorMousePosition(Vector2 position);
		void SetEditorMouseDown();
		void SetSourceCubamap(Cubemap cubamap);
		void SetSourceTexture(CubemapFace face, Texture2D texture);
		Action? onPipelineChanged { get; set; }
#endif
	}

	[ExecuteAlways]
	public partial class H17CubemapGenerator : MonoBehaviour, IH17CubemapGenerator
	{
		[SerializeField] Camera _rendererCamera = null!;
		[SerializeField] int _textureWidth = 1024;
		[SerializeField] float _equirectanglarRotationEulerY = 0;
		[SerializeField] GameObject _previewRoot = null!;
		[SerializeField] GameObject _previewSphere = null!;
		[SerializeField] GameObject _previewCube = null!;
		[SerializeField] GameObject _previewSphereMatcap = null!;

		public enum InputSource
		{
			CurrentScene,
			SixSided,
			Cubemap,
		}

		public enum OutputLayout
		{
			CrossHorizontal,
			CrossVertical,
			StraitHorizontal,
			StraitVertical,
			SixSided,
			Equirectanglar,
			Matcap,
		}

		public enum PreviewObject
		{
			None,
			Sphere,
			Cube,
		}

		public enum InternalPreviewObjectMode
		{
			Default,
			HideAll,
			Matcap,
		}

		RenderPipelineUtils.PipelineType _pipelineType = RenderPipelineUtils.PipelineType.Unsupported;
		Shader? _shaderBlitter = null!;
		Shader? _shaderPreview = null!;
		Material? _materialBlitter = null!;
		RenderTexture? _cubemapRT;
		Cubemap? _cubemapAlter;
		readonly Texture2D?[] _cachedFaces = new Texture2D[6];
		bool _isRenderProcessing;
		bool _isDone;
		bool _isSourceHDR;
		bool _isOutputDesirableHDR;
		bool _isOutputDesirableSRGB;
		bool _isOutputDesirableCubemap;
		bool _isOutputDesirableGenerateMipmap;
		bool _isOutputHDR;
		bool _isOutputSRGB;
		bool _isOutputCubemap;
		bool _isOutputGenerateMipmap;
		bool _fillMatcapOutsideByEdgeColor;
		bool _isPipelingChanging;
		bool _activeSceneChanged;
		readonly Texture2D[] _6sidedSources = new Texture2D[6];
		Camera?	_specificCamera;
		readonly List<Material> _previewMeshMaterials = new List<Material>();

		Matrix4x4 _previewRotationMatrix = Matrix4x4.identity;

		static readonly int _idMainTex = Shader.PropertyToID("_MainTex");
		static readonly int _idCubeTex = Shader.PropertyToID("_CubeTex");
		static readonly int _idPreviewRotationMatrix = Shader.PropertyToID("_PreviewRotationMatrix");
		static readonly int _idRotationY = Shader.PropertyToID("_RotationY");

		InputSource _inputSource = InputSource.CurrentScene;
		OutputLayout _outputLayout = OutputLayout.CrossHorizontal;
		PreviewObject _previewObject = PreviewObject.Sphere;

		bool _started;
		Action? _onStartExit;

		Action? _onUpdate;

		public bool isSourceHDR => _isSourceHDR;
		public RenderPipelineUtils.PipelineType pipelineType => _pipelineType;
		public Action? onPipelineChanged { get; set; }

#if UNITY_EDITOR
		public GameObject previewSphere => _previewSphere;
		public GameObject previewCube => _previewCube;
#endif
		public bool isPipelingChanging => _isPipelingChanging;
		public bool isProcessing => _isRenderProcessing
#if UNITY_EDITOR
			|| _isSaveProcessing
#endif
			;

		void Start()
		{
			Setup();
			_started = true;
			_onStartExit?.Invoke();

#if UNITY_EDITOR
			RenderPipelineManager.activeRenderPipelineTypeChanged -= OnPipelineChanged;
			RenderPipelineManager.activeRenderPipelineTypeChanged += OnPipelineChanged;
			EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged; 
			EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged; 
#endif
		}

		void OnDestroy()
		{
#if UNITY_EDITOR
			RenderPipelineManager.activeRenderPipelineTypeChanged -= OnPipelineChanged;
			EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
#endif
			CleanupRenderCurrentScene();
			DisposeRenderCache();
			DisposeMaterials();
		}
		void Update()
		{
			if (_isPipelingChanging)
			{
				return;
			}
			if (_activeSceneChanged)
			{
				_activeSceneChanged = false;
				DisposeRenderCache();
				DisposeMaterials();
				Setup();
				StartRender(_inputSource);
			}
			if (!isProcessing)
			{
				var pipelineType = RenderPipelineUtils.DetectPipeline();
				if (_pipelineType != pipelineType)
				{
					_pipelineType = pipelineType;
					OnPipelineChanging();
					return;
				}
				OnUpdateDragRotate();
			}
			_onUpdate?.Invoke();
		}

		public static void DestroyUnityObject(UnityEngine.Object obj) // Same as CoreUtils.Destroy
		{
			if (obj != null)
			{
#if UNITY_EDITOR
				if (Application.isPlaying && !UnityEditor.EditorApplication.isPaused)
					UnityEngine.Object.Destroy(obj);
				else
					UnityEngine.Object.DestroyImmediate(obj);
#else
				UnityEngine.Object.Destroy(obj);
#endif
			}
		}

		void Setup()
		{
			Shader? FindShader(string shaderName)
			{
				var shader = Shader.Find(shaderName);
				if (shader == null) { throw new InvalidOperationException($"{shaderName} load failed"); }
				return shader;
			}

			_pipelineType = RenderPipelineUtils.DetectPipeline();
			switch (_pipelineType)
			{
#if USING_URP 
				case RenderPipelineUtils.PipelineType.UniversalPipeline:
					_shaderBlitter = FindShader("Honshino17/H17CubemapGenerator/BlitterURP");
					_shaderPreview = FindShader("Honshino17/H17CubemapGenerator/PreviewURP");
					break;
#endif
#if USING_HDRP 
				case RenderPipelineUtils.PipelineType.HDPipeline:
					_shaderBlitter = FindShader("Honshino17/H17CubemapGenerator/BlitterHDRP");
					_shaderPreview = FindShader("Honshino17/H17CubemapGenerator/PreviewHDRP");
					break;
#endif
				case RenderPipelineUtils.PipelineType.BuiltInPipeline:
					_shaderBlitter = FindShader("Honshino17/H17CubemapGenerator/BlitterBuiltin");
					_shaderPreview = FindShader("Honshino17/H17CubemapGenerator/PreviewBuiltin");
					break;
				default: throw new InvalidOperationException("Unsupported");
			}

			// Preview Mesh
			var goes = new GameObject[] { _previewCube, _previewSphere, _previewSphereMatcap };
			for (int i = 0; i < goes.Length; i++)
			{
				var meshRenderer = goes[i].GetComponent<MeshRenderer>();
				var material = new Material(_shaderPreview);
				_previewMeshMaterials.Add(material);
				meshRenderer.material = material;
			}
			_previewCube.GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("SKYBOX_ON");

			UpdatePreviewMeshMatrix();
			UpdatePreviewMeshTexture(null);
#if UNITY_EDITOR
			GameObjectUtility.RemoveMonoBehavioursWithMissingScript(_rendererCamera.gameObject);
#endif
		}

		void OnActiveSceneChanged(Scene current, Scene next)
		{
			_onUpdate = null;
			_activeSceneChanged = true;
		}

		void OnPipelineChanging()
		{
			_onUpdate = null;
			DisposeRenderCache();
			DisposeMaterials();
			_isPipelingChanging = true;
		}

		void OnPipelineChanged()
		{
			DisposeRenderCache();
			DisposeMaterials();
			Setup();
			_isPipelingChanging = false;
			this.onPipelineChanged?.Invoke();
		}

		void DisposeMaterials()
		{
			if (_materialBlitter != null)
			{
				DestroyUnityObject(_materialBlitter);
			}

			for (int i = 0; i < _previewMeshMaterials.Count; i++)
			{
				DestroyUnityObject(_previewMeshMaterials[i]);
			}
			_previewMeshMaterials.Clear();
		}

		void DisposeRenderCache()
		{
			DisposeCachedFaces();

			if (_cubemapRT != null)
			{
				_cubemapRT.Release();
				_cubemapRT = null;
			}
			if (_cubemapAlter != null)
			{
				DestroyImmediate(_cubemapAlter);
				_cubemapAlter = null;
			}
		}

		public void ClearCubemap()
		{
			DisposeRenderCache();
		}

		void DisposeCachedFaces()
		{
			for (int i = 0; i < 6; i++)
			{
				if (_cachedFaces[i] != null)
				{
					DestroyImmediate(_cachedFaces[i]);
				}
			}
		}

		public void SetSpecificCamera(Camera? specificCamera)
		{
			_specificCamera = specificCamera;
		}

		public void SetCubemapWidth(int width)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_textureWidth = width;
		}

#if UNITY_EDITOR
		public void SetFillMatcapOutsideByEdgeColor(bool value)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_fillMatcapOutsideByEdgeColor = value;
		}

		public void SetOutputDesirableHDR(bool value)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_isOutputDesirableHDR = value;
		}

		public void SetOutputDesirableSRGB(bool value)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_isOutputDesirableSRGB = value;
		}

		public void SetOutputDesirableCubemap(bool value)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_isOutputDesirableCubemap = value;
		}

		public void SetOutputDesirableGenerateMipmap(bool value)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_isOutputDesirableGenerateMipmap = value;
		}

		public void SetCubemapOutputLayout(OutputLayout cubemapOutputLayout)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_outputLayout = cubemapOutputLayout;
		}
#endif

		public void StartRender(InputSource inputSource, Action? onCompleted = null)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_isRenderProcessing = true;
			_inputSource = inputSource;

			if (!_started)
			{
				_onStartExit += () => 
				{
					StartCoroutine(RenderProcessCoroutine(onCompleted));
				};
			}
			else
			{
				StartCoroutine(RenderProcessCoroutine(onCompleted));
			}
		}

		IEnumerator RenderProcessCoroutine(Action? onCompleted)
		{
			try
			{
				yield return StartCoroutine(RenderProcessMainCoroutine(onCompleted));
			}
			finally
			{
				CleanupRenderCurrentScene();
				_isRenderProcessing = false;
				if (_rendererCamera != null)
				{
					_rendererCamera.gameObject.SetActive(false);
				}
			}
		}

		IEnumerator RenderProcessMainCoroutine(Action? onCompleted)
		{
			int cubemapWidth = 0;
			switch (_inputSource)
			{
				case InputSource.CurrentScene:
					VetifyAndSetupRenderCurrentScene(out cubemapWidth);
					break;

				case InputSource.SixSided:
					VetifyAndSetupLoad6Sided(out cubemapWidth);
					break;

				case InputSource.Cubemap:
					VetifyAndSetupLoadCubemap();
					break;

				default:
					throw new InvalidOperationException("Unsupported InputSource");
			}

			if (_shaderBlitter == null) { throw new InvalidOperationException($"{nameof(_shaderBlitter)} null"); }

			if (_materialBlitter == null)
			{
				_materialBlitter = new Material(_shaderBlitter);
				if (_materialBlitter == null) { throw new InvalidOperationException($"{nameof(_materialBlitter)} null"); }
			}

			ClearCubemap();

			if (_inputSource == InputSource.Cubemap)
			{
				LoadCubemapFaces();
				UpdatePreviewMeshTexture(_texCubemap);
				_isDone = true;
				_isRenderProcessing = false;
				onCompleted?.Invoke();
				yield break;
			}

			// For HDRP-6Sided. Low Speedw.
			// I really want to make the process similar to the subsequent URP version.(2023/06/04)
#if USING_HDRP 
			if ((_pipelineType == RenderPipelineUtils.PipelineType.HDPipeline) &&
				(_inputSource == InputSource.SixSided))
			{
				LoadCubemapFacesFrom6Sided(cubemapWidth);
				UpdatePreviewMeshTexture(_cubemapAlter);
				_isDone = true;
				_isRenderProcessing = false;
				onCompleted?.Invoke();
				yield break;
			}
#endif

			// For URP
			var targetSourceCamera = GetTargetCamera();
			_rendererCamera.CopyFrom(targetSourceCamera);
			_rendererCamera.gameObject.SetActive(true);

			Camera? camera = _rendererCamera;

			_isSourceHDR = camera.allowHDR;

			var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32;
			var cubemapRTDesc = new RenderTextureDescriptor(cubemapWidth, cubemapWidth, format)
			{
				dimension = UnityEngine.Rendering.TextureDimension.Cube,
				enableRandomWrite = true
			};
			_cubemapRT = new RenderTexture(cubemapRTDesc);
			UpdatePreviewMeshTexture(null);

			_isDone = false;
			UpdatePreviewObjectVisivility(InternalPreviewObjectMode.HideAll);

			switch (_inputSource)
			{
				case InputSource.CurrentScene:
					RenderCurrentScene();
					break;
				case InputSource.SixSided:
					Load6Sided();
					break;
			}

			yield return new WaitUntil(() => _isDone);

			UpdatePreviewMeshTexture(_cubemapRT);
			_isRenderProcessing = false;
			onCompleted?.Invoke();
			UpdatePreviewObjectVisivility(InternalPreviewObjectMode.Default);
		}

		public void SetPreviewObject(PreviewObject previewObject)
		{
			_previewObject = previewObject;
			if (this.isProcessing)
			{
				return;
			}
			UpdatePreviewObjectVisivility(InternalPreviewObjectMode.Default);
		}

		void UpdatePreviewObjectVisivility(InternalPreviewObjectMode internalPreviewObjectMode)
		{
			switch (internalPreviewObjectMode)
			{
				case InternalPreviewObjectMode.Default:
					_previewRoot.SetActive(_previewObject != PreviewObject.None);
					_previewSphere.SetActive(_previewObject == PreviewObject.Sphere);
					_previewCube.SetActive(_previewObject == PreviewObject.Cube);
					_previewSphereMatcap.SetActive(false);
					break;
				case InternalPreviewObjectMode.HideAll:
					_previewRoot.SetActive(false);
					break;
				case InternalPreviewObjectMode.Matcap:
					_previewRoot.SetActive(true);
					_previewSphere.SetActive(false);
					_previewCube.SetActive(false);
					_previewSphereMatcap.SetActive(true);
					break;
			}
		}

		Camera GetTargetCamera()
		{ 
			if (_specificCamera != null)
			{ 
				return _specificCamera;	
			}
			if (Camera.main == null)
			{
				var scene = SceneManager.GetActiveScene();
				if (scene != null)
				{
					var rootObjects = scene.GetRootGameObjects();
					foreach (var obj in rootObjects)
					{
						var camera = obj.GetComponentInChildren<Camera>();
						if (camera != null)
						{
							return camera;
						}
					}
				}
				throw new InvalidOperationException("No Camera");
			}
			return Camera.main;
		}

		void UpdatePreviewMeshTexture(Texture? cubemapTex)
		{
			for (int i = 0; i < _previewMeshMaterials.Count; i++) 
			{
				_previewMeshMaterials[i].SetTexture(_idMainTex, cubemapTex);
			}
		}

		void UpdatePreviewMeshMatrix()
		{
			for (int i = 0; i < _previewMeshMaterials.Count; i++)
			{
				_previewMeshMaterials[i].SetMatrix(_idPreviewRotationMatrix, _previewRotationMatrix);
			}
		}

		public static bool IsHDRFormat(TextureFormat textureFormat)
		{
			switch (textureFormat)
			{
				case TextureFormat.RGBAFloat:
				case TextureFormat.RGBAHalf:
				case TextureFormat.RGB9e5Float:
				case TextureFormat.BC6H:
					return true;
			}
			return false;
		}

		public static bool IsOutputLayoutAvailableBeCubemap(OutputLayout outputLayout)
		{
			switch (outputLayout)
			{
				case OutputLayout.CrossHorizontal:
				case OutputLayout.CrossVertical:
				case OutputLayout.StraitHorizontal:
				case OutputLayout.StraitVertical:
				case OutputLayout.Equirectanglar:
					return true;
			}
			return false;
		}

	}
}
