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
		class SaveAs6Sided : ICubemapSave
		{
			H17CubemapGenerator _generator;

			public SaveAs6Sided(H17CubemapGenerator generator)
			{
				_generator = generator;
			}

			public void Dispose()
			{
			}

			public IEnumerator SaveAsPNGCoroutine(string assetPath)
			{
				var suffixes = new string[] { "_xplus", "_xminus", "_yplus", "_yminus", "_zplus", "_zminus" };
				string assetExt = Path.GetExtension(assetPath);
				for (int i = 0; i < 6; i++)
				{
					string assetPathTmp = Path.ChangeExtension(assetPath, null) + $"{suffixes[i]}{assetExt}";
					var bytes = _generator._cachedFaces[i].EncodeToPNG();
					File.WriteAllBytes(assetPathTmp, bytes);
					AssetDatabase.ImportAsset(assetPathTmp);
					H17CubemapGenerator.SetOutputSpecification(assetPathTmp,
						TextureImporterShape.Texture2D,
						_generator._isOutputGenerateMipmap,
						_generator._isOutputSRGB);
					AssetDatabase.Refresh();
				}
				AssetDatabase.Refresh();
				yield break;
			}
		}
#endif
	}
}
