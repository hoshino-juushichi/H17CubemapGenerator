using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Hoshino17
{
	public static class AssetPath
	{
		static string? _assetPath = null;

		public static string h17CubemapGenerator
		{
			get
			{
				if (_assetPath == null)
				{
					string[] guids = AssetDatabase.FindAssets("H17CubemapGenerator.Editor", null);
					if (guids.Length > 0) 
					{
						var tmp = AssetDatabase.GUIDToAssetPath(guids[0]);
						tmp = tmp.Replace("Editor/H17CubemapGenerator.Editor.asmdef", string.Empty);
						_assetPath = tmp;
					}
				}
				if (_assetPath == null)
				{
					throw new InvalidOperationException("Asset path not found s");
				}
				return _assetPath;
			}
		}
	}
}