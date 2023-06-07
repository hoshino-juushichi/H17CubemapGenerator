using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

#nullable enable

namespace Hoshino17
{
	public static class TextureUtils
	{
		public static Texture2D CreateReadabeTexture2D(Texture2D texture2d)
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(
					texture2d.width,
					texture2d.height,
					0,
					RenderTextureFormat.Default,
					RenderTextureReadWrite.Linear);

			Graphics.Blit(texture2d, renderTexture);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTexture;
			Texture2D readableTextur2D = new Texture2D(texture2d.width, texture2d.height);
			readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			readableTextur2D.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTexture);
			return readableTextur2D;
		}

		public static void FillTexture2D(Texture2D texture2d, Color color)
		{
			int width = texture2d.width;
			int height = texture2d.height;
			int elements = width * height;
			Color[] pixels = new Color[elements];
			for (int i = 0; i < elements; i++)
			{
				pixels[i] = color;
			}
			texture2d.SetPixels(pixels);
		}

		public static Color[] VertialInvertPixels(Color[] pixels, int blockWidth, int blockHeight)
		{
			Color[] pixels2 = new Color[pixels.Length];

			for (int y = 0; y < blockHeight; y++)
			{
				for (int x = 0; x < blockWidth; x++)
				{
					pixels2[y * blockHeight + x] = pixels[(blockHeight - 1 - y) * blockHeight + x];
				}
			}
			return pixels2;
		}
	}
}
