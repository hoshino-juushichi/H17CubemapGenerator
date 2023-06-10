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
	public partial class H17CubemapGenerator : MonoBehaviour, IH17CubemapGenerator
	{
#if UNITY_EDITOR
		class SaveAsCubemap : ICubemapSave
		{
			H17CubemapGenerator _generator;

			public SaveAsCubemap(H17CubemapGenerator generator)
			{
				_generator = generator;
			}

			public void Dispose()
			{
			}

			public IEnumerator SaveAsPNGCoroutine(string assetPath)
			{
				int texWidth = _generator._cachedFaces[0]!.width;
				var blockSize = new Vector2Int(texWidth, texWidth);
				var tmpSize = blockSize;
				var destPos = new Vector2Int[6];

				switch (_generator._outputLayout)
				{
					case OutputLayout.CrossHorizontal:
						tmpSize.x *= 4;
						tmpSize.y *= 3;
						destPos[0] = new Vector2Int(2, 1); // +X
						destPos[1] = new Vector2Int(0, 1); // -X
						destPos[2] = new Vector2Int(1, 2); // +Y
						destPos[3] = new Vector2Int(1, 0); // -Y
						destPos[4] = new Vector2Int(1, 1); // +Z
						destPos[5] = new Vector2Int(3, 1); // -Z
						break;
					case OutputLayout.CrossVertical:
						tmpSize.x *= 3;
						tmpSize.y *= 4;
						destPos[0] = new Vector2Int(1, 2); // +X
						destPos[1] = new Vector2Int(1, 0); // -X
						destPos[2] = new Vector2Int(1, 3); // +Y
						destPos[3] = new Vector2Int(1, 1); // -Y
						destPos[4] = new Vector2Int(0, 2); // +Z
						destPos[5] = new Vector2Int(2, 2); // -Z
						break;
					case OutputLayout.StraitHorizontal:
						tmpSize.x *= 6;
						for (int i = 0; i < 6; i++)
						{
							destPos[i] = new Vector2Int(i, 0);
						}
						break;
					case OutputLayout.StraitVertical:
						tmpSize.y *= 6;
						for (int i = 0; i < 6; i++)
						{
							destPos[i] = new Vector2Int(0, 5 - i);
						}
						break;
					default:
						throw new InvalidOperationException("Unsupported OutputLayout type");
				}

				Texture2D tempTex = _generator.CreateTexture2DForOutputTemporary(tmpSize.x, tmpSize.y);
				TextureUtils.FillTexture2D(tempTex, Color.black);

				for (int i = 0; i < 6; i++)
				{
					tempTex.SetPixels(destPos[i].x * blockSize.x, destPos[i].y * blockSize.y, blockSize.x, blockSize.y, _generator._cachedFaces[i]!.GetPixels(), 0);
				}
				tempTex.Apply();

				var bytes = tempTex.EncodeToPNG();
				File.WriteAllBytes(assetPath, bytes);
				AssetDatabase.ImportAsset(assetPath);
				H17CubemapGenerator.SetOutputSpecification(assetPath,
					(_generator._isOutputCubemap ? TextureImporterShape.TextureCube : TextureImporterShape.Texture2D),
					_generator._isOutputGenerateMipmap,
					_generator._isOutputSRGB);
				AssetDatabase.Refresh();

				yield break;
			}

		}

#endif
	}
}
