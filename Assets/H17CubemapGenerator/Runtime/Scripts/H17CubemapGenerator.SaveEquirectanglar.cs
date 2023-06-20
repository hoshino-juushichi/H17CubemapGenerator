﻿using System;
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

		class SaveAsEquirectanglar : CubemapSaveBase
		{
			public SaveAsEquirectanglar(H17CubemapGenerator generator) : base(generator) { }

			public override IEnumerator SaveAsPNGCoroutine(string assetPath)
			{
				if (generator._materialBlitter == null) throw new InvalidOperationException($"{nameof(generator._materialBlitter)} null");

				var targetSourceCamera = generator.GetTargetCamera();
				generator._rendererCamera.CopyFrom(targetSourceCamera);
				generator._rendererCamera.gameObject.SetActive(true);
				Camera? camera = generator._rendererCamera;

				Texture? texCubemap = null;
				if (generator._cubemapRT != null)
				{
					texCubemap = generator._cubemapRT;
				}
				else if (generator._cubemapAlter != null)
				{
					texCubemap = generator._cubemapAlter;
				}
				else
				{
					texCubemap = generator._texCubemap;
				}
				generator._materialBlitter.SetTexture(_idCubeTex, texCubemap);
				generator._materialBlitter.SetFloat(_idRotationY, generator._equirectanglarRotationEulerY * Mathf.Deg2Rad);

				var desc = generator.GetRenderTextureDescriptorForOutoutTemporary(generator._textureWidth * 2, generator._textureWidth, false);
				var tempRT = RenderTexture.GetTemporary(desc);

				Graphics.SetRenderTarget(tempRT, 0, 0);
				Graphics.Blit(tempRT, generator._materialBlitter, pass: 1);

				RenderTexture previous = RenderTexture.active;
				RenderTexture.active = tempRT;

				var tempTex = generator.CreateTexture2DForOutputTemporary(tempRT.width, tempRT.height);
				tempTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
				tempTex.Apply();

				var bytes = tempTex.EncodeToPNG();   
				File.WriteAllBytes(assetPath, bytes);      

				RenderTexture.active = previous;
				RenderTexture.ReleaseTemporary(tempRT);
				AssetDatabase.ImportAsset(assetPath);
				H17CubemapGenerator.SetOutputSpecification(assetPath,
					(generator._isOutputCubemap ? TextureImporterShape.TextureCube : TextureImporterShape.Texture2D),
					generator._isOutputGenerateMipmap,
					generator._isOutputSRGB);
				AssetDatabase.Refresh();

				yield break;
			}
		}
	}
#endif
}