using System;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
#if USING_URP 
using UnityEngine.Rendering.Universal;
#endif

#nullable enable
#pragma warning disable 618 // obsolete
#pragma warning disable 414 // The field '' is assigned but its value is never used

namespace Hoshino17
{
	public partial class H17CubemapGenerator : MonoBehaviour, IH17CubemapGenerator
	{
		[SerializeField] ComputeShader _computeShader = null!;

#if UNITY_EDITOR

		class SaveAsMatcap : ICubemapSave
		{
			H17CubemapGenerator _generator;
			RenderTexture? _tempRT;
			RenderPipelineFook? _renderPipelineFook;
			bool _isDone;

			public SaveAsMatcap(H17CubemapGenerator generator)
			{
				_generator = generator;
				_generator.UpdatePreviewObjectVisivility(InternalPreviewObjectMode.Matcap);
			}

			public void Dispose()
			{
				if (_tempRT != null)
				{
					RenderTexture.ReleaseTemporary(_tempRT);
					_tempRT = null;
				}
				_renderPipelineFook?.Dispose();
				_renderPipelineFook = null;
				_generator._rendererCamera.gameObject.SetActive(false);
				_generator.UpdatePreviewObjectVisivility(InternalPreviewObjectMode.Default);
			}

			public IEnumerator SaveAsPNGCoroutine(string assetPath)
			{
				const int LayerNo_CubemapGeneratorMatcap = 31;

				var targetSourceCamera = _generator.GetTargetCamera();
				_generator._rendererCamera.CopyFrom(targetSourceCamera);
				_generator._rendererCamera.gameObject.SetActive(true);

				const float Adjust = 1.010f;
				float distance = 1f / Mathf.Atan(Adjust * targetSourceCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

				var position = targetSourceCamera.transform.position + -targetSourceCamera.transform.forward * distance;
				_generator._rendererCamera.transform.position = position;
				_generator._rendererCamera.orthographic = false;
				_generator._rendererCamera.usePhysicalProperties = false;
				_generator._rendererCamera.clearFlags = CameraClearFlags.SolidColor;
				_generator._rendererCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
				_generator._rendererCamera.cullingMask = (1 << LayerNo_CubemapGeneratorMatcap);
				_generator._previewSphereMatcap.transform.position = targetSourceCamera.transform.position;
				_generator._previewSphereMatcap.layer = LayerNo_CubemapGeneratorMatcap;

				var format = _generator._rendererCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32;
				var desc = new RenderTextureDescriptor(_generator._textureWidth, _generator._textureWidth, format, 32);
				desc.enableRandomWrite = true;
				_tempRT = RenderTexture.GetTemporary(desc);

				if (_generator._pipelineType == RenderPipelineUtils.PipelineType.BuiltInPipeline
#if USING_HDRP 
					|| _generator._pipelineType == RenderPipelineUtils.PipelineType.HDPipeline
#endif
					)
				{
					_generator._onUpdate -= OnRenderCamera;
					_generator._onUpdate += OnRenderCamera;

				}
				else
				{
					_renderPipelineFook = new RenderPipelineFook(
						onBeginFrameRendering: (context, cameras) => OnBeginFrameRendering(context, cameras)
					);
				}
				yield return new WaitUntil(() => _isDone);


				if (_generator._fillMatcapOutsideByEdgeColor)
				{
					RunComputeMatcapFillOutsideByEdgeColor(desc);
				}

				RenderTexture previous = RenderTexture.active;
				RenderTexture.active = _tempRT;

				TextureFormat textureFormat = _generator._isOutputHDR ? TextureFormat.RGBAHalf : TextureFormat.RGB24;
				var tempTex = new Texture2D(_tempRT.width, _tempRT.height, textureFormat, false);
				tempTex.ReadPixels(new Rect(0, 0, _tempRT.width, _tempRT.height), 0, 0);
				tempTex.Apply();

				var bytes = tempTex.EncodeToPNG();   
				File.WriteAllBytes(assetPath, bytes);

				RenderTexture.active = previous;

				RenderTexture.ReleaseTemporary(_tempRT);
				_tempRT = null;

				_renderPipelineFook?.Dispose();
				_renderPipelineFook = null;

				AssetDatabase.ImportAsset(assetPath);
				H17CubemapGenerator.SetOutputSpecification(assetPath,
					TextureImporterShape.Texture2D,
					_generator._isOutputGenerateMipmap,
					_generator._isOutputSRGB);
				AssetDatabase.Refresh();

				yield break;
			}

			void OnRenderCamera()
			{
				try
				{
					_generator._rendererCamera.targetTexture = _tempRT;
					_generator._rendererCamera.Render();
					_generator._rendererCamera.targetTexture = null;
				}
				finally
				{
					_isDone = true;
					_generator._rendererCamera.gameObject.SetActive(false);
					_generator._onUpdate -= OnRenderCamera;
				}
			}

			void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
			{
				for (int i = 0; i < cameras.Length; i++)
				{
					if (_generator._rendererCamera == cameras[i])
					{
						OnRender(context, cameras[i]);
						_isDone = true;
						break;
					}
				}
			}

			void OnRender(ScriptableRenderContext context, Camera camera)
			{
				camera.targetTexture = _tempRT;

#if USING_URP 
				if (_generator._pipelineType == RenderPipelineUtils.PipelineType.UniversalPipeline)
				{
					UniversalRenderPipeline.RenderSingleCamera(context, camera);
				}
				else
#endif
#if USING_HDRP
				if (_generator._pipelineType == RenderPipelineUtils.PipelineType.HDPipeline)
				{
				}
				else
#endif
				{
					throw new InvalidOperationException("Unsupported");
				}

				camera.targetTexture = null;
			}

			void RunComputeMatcapFillOutsideByEdgeColor(RenderTextureDescriptor desc)
			{
				if (_tempRT == null) throw new InvalidOperationException($"{nameof(_tempRT)} null");

				var tempRT2 = RenderTexture.GetTemporary(desc);
				Graphics.CopyTexture(_tempRT, tempRT2);

				int width = _tempRT.width;
				int kernelIndex = _generator._computeShader.FindKernel("MatcapFillOutsideByEdgeColor");
				_generator._computeShader.SetTexture(kernelIndex, "source", tempRT2);
				_generator._computeShader.SetTexture(kernelIndex, "result", _tempRT);
				_generator._computeShader.SetInt("width", width);
				_generator._computeShader.GetKernelThreadGroupSizes(kernelIndex, out var threadSizeX, out var threadSizeY, out var threadSizeZ);
				_generator._computeShader.Dispatch(kernelIndex, width / (int)threadSizeX, width / (int)threadSizeY, (int)threadSizeZ);

				RenderTexture.ReleaseTemporary(tempRT2);
			}
		}
#endif
	}
}