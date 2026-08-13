using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class TexturePacker : MonoBehaviour
{
	public static readonly string doNotPutInAtlasTag = "DoNotPutInAtlas";

	public Texture2D packedTexture;

	public Shader shader;

	public int textureSize = 2048;

	private void Start()
	{
	}

	public void RegenerateAtlas()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Material material = new Material(shader);
		Component[] componentsInChildren = GetComponentsInChildren<MeshFilter>(includeInactive: true);
		componentsInChildren = componentsInChildren.Where((Component filter) => filter.tag != doNotPutInAtlasTag).ToArray();
		stopwatch.Stop();
		TimeSpan elapsed = stopwatch.Elapsed;
		stopwatch.Reset();
		stopwatch.Start();
		Texture2D[] array = new Texture2D[componentsInChildren.Length];
		for (int num = 0; num < componentsInChildren.Length; num++)
		{
			Texture2D texture2D = componentsInChildren[num].gameObject.GetComponent<Renderer>().material.mainTexture as Texture2D;
			if (texture2D == null && Globals.IsDebugBuild)
			{
				UnityEngine.Debug.LogError(componentsInChildren[num].gameObject.name + " has no texture ...");
			}
			texture2D.GetPixel(0, 0);
			array[num] = texture2D;
		}
		stopwatch.Stop();
		TimeSpan elapsed2 = stopwatch.Elapsed;
		stopwatch.Reset();
		stopwatch.Start();
		packedTexture = new Texture2D(1, 1);
		Rect[] array2 = packedTexture.PackTextures(array, 1, 2048, makeNoLongerReadable: true);
		packedTexture.filterMode = FilterMode.Point;
		packedTexture.wrapMode = TextureWrapMode.Clamp;
		material.mainTexture = packedTexture;
		stopwatch.Stop();
		TimeSpan elapsed3 = stopwatch.Elapsed;
		stopwatch.Reset();
		stopwatch.Start();
		for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
		{
			componentsInChildren[num2].gameObject.GetComponent<Renderer>().material = material;
			Vector2[] uv = ((MeshFilter)componentsInChildren[num2]).mesh.uv;
			Vector2[] array3 = new Vector2[uv.Length];
			for (int num3 = 0; num3 < uv.Length; num3++)
			{
				ref Vector2 reference = ref array3[num3];
				reference = new Vector2(uv[num3].x * array2[num2].width + array2[num2].x, uv[num3].y * array2[num2].height + array2[num2].y);
				((MeshFilter)componentsInChildren[num2]).mesh.uv = array3;
			}
		}
		stopwatch.Stop();
		TimeSpan elapsed4 = stopwatch.Elapsed;
		if (Globals.IsDebugBuild)
		{
			UnityEngine.Debug.Log($"![filters:{elapsed.TotalMilliseconds} textures:{elapsed2.TotalMilliseconds} packing:{elapsed3.TotalMilliseconds} setUVs:{elapsed4.TotalMilliseconds}]");
		}
	}
}
