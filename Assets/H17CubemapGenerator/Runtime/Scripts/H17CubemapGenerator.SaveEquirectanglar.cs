using System;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#nullable enable
#pragma warning disable 1998

namespace Hoshino17
{
#if UNITY_EDITOR
	public partial class H17CubemapGenerator : MonoBehaviour, IH17CubemapGenerator
	{
		public void SetCubemapOutputEquirectanglarRotation(float angleEulerY)
		{
			if (this.isProcessing) { throw new InvalidOperationException("Now processing"); }
			_equirectanglarRotationEulerY = angleEulerY;
		}

		class SaveAsEquirectanglar : ICubemapSave
		{
			H17CubemapGenerator _generator;

			public SaveAsEquirectanglar(H17CubemapGenerator generator)
			{
				_generator = generator;
			}

			public void Dispose()
			{
			}

			public IEnumerator SaveAsPNGCoroutine(string assetPath)
			{
				if (_generator._materialBlitter == null) throw new InvalidOperationException($"{nameof(_generator._materialBlitter)} null");

				var targetSourceCamera = _generator.GetTargetCamera();
				_generator._rendererCamera.CopyFrom(targetSourceCamera);
				_generator._rendererCamera.gameObject.SetActive(true);
				Camera? camera = _generator._rendererCamera;

				Texture? texCubemap = null;
				if (_generator._cubemapRT != null)
				{
					texCubemap = _generator._cubemapRT;
				}
				else if (_generator._cubemapAlter != null)
				{
					texCubemap = _generator._cubemapAlter;
				}
				else
				{
					texCubemap = _generator._texCubemap;
				}
				_generator._materialBlitter.SetTexture(_idCubeTex, texCubemap);
				_generator._materialBlitter.SetFloat(_idRotationY, _generator._equirectanglarRotationEulerY * Mathf.Deg2Rad);

				var desc = _generator.GetRenderTextureDescriptorForOutoutTemporary(_generator._textureWidth * 2, _generator._textureWidth, false);
				var tempRT = RenderTexture.GetTemporary(desc);

				Graphics.SetRenderTarget(tempRT, 0, 0);
				Graphics.Blit(tempRT, _generator._materialBlitter, pass: 1);

				RenderTexture previous = RenderTexture.active;
				RenderTexture.active = tempRT;

				var tempTex = _generator.CreateTexture2DForOutputTemporary(tempRT.width, tempRT.height);
				tempTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
				tempTex.Apply();

				var bytes = tempTex.EncodeToPNG();   
				File.WriteAllBytes(assetPath, bytes);      

				RenderTexture.active = previous;
				RenderTexture.ReleaseTemporary(tempRT);
				AssetDatabase.ImportAsset(assetPath);
				H17CubemapGenerator.SetOutputSpecification(assetPath,
					(_generator._isOutputCubemap ? TextureImporterShape.TextureCube : TextureImporterShape.Texture2D),
					_generator._isOutputGenerateMipmap,
					_generator._isOutputSRGB);
				AssetDatabase.Refresh();

				yield break;
			}
		}
	}
#endif
}