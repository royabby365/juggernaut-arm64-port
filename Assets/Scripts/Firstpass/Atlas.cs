using UnityEngine;

public class Atlas : MonoBehaviour
{
	private Material _mat;

	public int Width;

	public int Height;

	public string[] Names;

	public Rect[] Uvs;

	public Vector2[] Dims;

	public string TexturePath;

	public Material Material
	{
		get
		{
			if (_mat == null)
			{
				Shader shader = Shader.Find("Transparent/UVCO UnlitVertexColoredOverbright");
				Material material = new Material(shader);
				material.name = base.name;
				Material mat = material;
				_mat = mat;
				LoadMaterialTexture();
			}
			return _mat;
		}
		private set
		{
			_mat = value;
		}
	}

	private void LoadMaterialTexture()
	{
		string textureName = ((!Globals.DebugDoNotLoadAtlasTextures) ? TexturePath : Globals.DebugFakeTexturePath);
		_mat.mainTexture = SingletonT<AtlasManager>.I.LoadTexture(textureName);
	}
}
