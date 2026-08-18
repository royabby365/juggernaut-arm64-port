using System.Collections.Generic;
using UnityEngine;
using Yarx.Collections;

public class AtlasManager : SingletonT<AtlasManager>
{
	private readonly Dictionary<string, Atlas> _loaded = new Dictionary<string, Atlas>();

	private readonly Dictionary<string, System.Tuple<Atlas, int>> _spriteAtlas = new Dictionary<string, System.Tuple<Atlas, int>>();

	private readonly string[] _atlasNamesCommon = new string[10] { "a_assorted", "a_assorted_big", "a_assorted_big_2", "a_assorted_big_3", "a_assorted_big_4", "a_gui_alpha", "a_gui_alpha_2", "a_gui_alpha_3", "a_magic_book_bg", "a_advertising" };

	private readonly string[] _atlasNamesInternational = new string[1] { "a_international" };

	private readonly string[] _atlasNamesSets = new string[1] { "a_icons_set_" };

	public System.Tuple<Atlas, int> GetAtlasBySpriteName(string spriteName)
	{
		if (_spriteAtlas.ContainsKey(spriteName))
		{
			return _spriteAtlas[spriteName];
		}
		return null;
	}

	private void PreloadCommonAtlases(AssetBundle assetBundle)
	{
		if (!Globals.DebugDoNotLoadAtlases)
		{
			string[] atlasNamesCommon = _atlasNamesCommon;
			foreach (string mangledName in atlasNamesCommon)
			{
				AddAtlas(assetBundle, mangledName);
			}
		}
	}

	private void PreloadInternationalAtlases(AssetBundle assetBundle, string locale)
	{
		if (!Globals.DebugDoNotLoadAtlases)
		{
			string[] atlasNamesInternational = _atlasNamesInternational;
			foreach (string text in atlasNamesInternational)
			{
				string mangledName = text + "_" + locale;
				AddAtlas(assetBundle, mangledName);
			}
		}
	}

	private void AddAtlas(AssetBundle assetBundle, string mangledName)
	{
		Atlas atlas = LoadAtlas(assetBundle, mangledName);
		if (atlas == null)
		{
			Debug.LogError("=== Atlas: {0} CANNOT BE LOADED ===".Fmt(mangledName));
			return;
		}
		for (int i = 0; i < atlas.Names.Length; i++)
		{
			string text = atlas.Names[i];
			if (_spriteAtlas.ContainsKey(text))
			{
				if (Globals.IsDebugBuild)
				{
					Debug.LogError("=== Sprite Name Collision: {0} ===".Fmt(text));
				}
			}
			else
			{
				_spriteAtlas[text] = System.Tuple.Create(atlas, i);
			}
		}
	}

	public void PreloadSetAtlases(int setNumber)
	{
		PreloadSetAtlases(null, setNumber);
	}

	public void PreloadSetAtlases(AssetBundle assetBundle, int setNumber)
	{
		if (!Globals.DebugDoNotLoadAtlases)
		{
			string[] atlasNamesSets = _atlasNamesSets;
			foreach (string text in atlasNamesSets)
			{
				string mangledName = text + setNumber;
				AddAtlas(assetBundle, mangledName);
			}
		}
	}

	private Atlas LoadAtlas(AssetBundle assetBundle, string atlasName)
	{
		if (_loaded.ContainsKey(atlasName))
		{
			return _loaded[atlasName];
		}
		GameObject gameObject = Util.Resource<GameObject>("__atlases/" + atlasName);
		if (gameObject == null)
		{
			if (Globals.IsDebugBuild)
			{
				Debug.LogWarning($"Cannot load atlas: {atlasName}");
			}
			// Fallback: create a runtime Atlas from exported textures + JSON data
			Debug.Log($"[AtlasManager] Creating runtime atlas for '{atlasName}' from texture exports");
			if (CreateRuntimeAtlas(atlasName, out Atlas runtimeAtlas))
			{
				_loaded[atlasName] = runtimeAtlas;
				return _loaded[atlasName];
			}
			return null;
		}
		gameObject.SetActive(false);
		Atlas component = gameObject.transform.GetComponent<Atlas>();
		_loaded[atlasName] = component;
		return _loaded[atlasName];

		bool CreateRuntimeAtlas(string name, out Atlas atlas)
		{
			atlas = null;
			
			if (LoadAtlasFromJson(name, out atlas))
				return true;
			
			string texName = name + "_tex";
			Texture2D tex = Resources.Load<Texture2D>("__atlases/" + texName) 
			              ?? Resources.Load<Texture2D>("__atlases/" + name)
			              ?? Resources.Load<Texture2D>("__textures/" + texName);
			if (tex == null) return false;
			
			var go = new GameObject("RuntimeAtlas_" + name);
			var atl = go.AddComponent<Atlas>();
			atl.Width = tex.width;
			atl.Height = tex.height;
			atl.TexturePath = texName;
			atl.Names = new[] { "full" };
			atl.Uvs = new[] { new Rect(0, 0, 1, 1) };
			atl.Dims = new[] { new Vector2(tex.width, tex.height) };
			atlas = atl;
			return true;
		}
		
		bool LoadAtlasFromJson(string atlasName, out Atlas atlas)
		{
			atlas = null;
			TextAsset jsonAsset = Resources.Load<TextAsset>("__atlases/atlas_complete");
			if (jsonAsset == null) return false;
			
			try
			{
				var wrapper = JsonUtility.FromJson<AtlasDataWrapper>("{\"items\":" + jsonAsset.text + "}");
				if (wrapper == null || wrapper.items == null) return false;
				
				foreach (var entry in wrapper.items)
				{
					string entryBaseName = entry.texture_path;
					if (string.IsNullOrEmpty(entryBaseName)) continue;
					
					string shortName = entryBaseName.Replace("__atlases/", "").Replace("_tex", "");
					
					if (shortName == atlasName || entryBaseName.Contains(atlasName))
					{
						string texName = entryBaseName.Contains("_tex") 
							? entryBaseName.Replace("__atlases/", "") 
							: atlasName + "_tex";
						
						Texture2D tex = Resources.Load<Texture2D>("__atlases/" + texName)
						              ?? Resources.Load<Texture2D>("__textures/" + texName);
						if (tex == null) 
						{
							Debug.LogWarning($"[AtlasManager] Texture '{texName}' not found for atlas '{atlasName}'");
							return false;
						}
						
						var go = new GameObject("Atlas_" + atlasName);
						var atl = go.AddComponent<Atlas>();
						atl.Width = entry.width;
						atl.Height = entry.height;
						atl.TexturePath = texName;
						atl.Names = entry.names;
						atl.Uvs = entry.GetUvArray();
						atl.Dims = entry.GetDimArray();
						
						Debug.Log($"[AtlasManager] Loaded atlas '{atlasName}' with {atl.Names.Length} sprites from JSON");
						atlas = atl;
						return true;
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError($"[AtlasManager] Error loading atlas JSON: {e.Message}");
			}
			return false;
		}
	}

	[System.Serializable]
	public class AtlasDataWrapper
	{
		public AtlasEntry[] items;
	}
	
	[System.Serializable]
	public class AtlasEntry
	{
		public string name;
		public int width;
		public int height;
		public string[] names;
		public float[][] uvs;
		public float[][] dims;
		public string texture_path;
		
		public Rect[] GetUvArray()
		{
			if (uvs == null) return new Rect[0];
			var result = new Rect[uvs.Length];
			for (int i = 0; i < uvs.Length; i++)
			{
				if (uvs[i] != null && uvs[i].Length >= 4)
					result[i] = new Rect(uvs[i][0], uvs[i][1], uvs[i][2], uvs[i][3]);
			}
			return result;
		}
		
		public Vector2[] GetDimArray()
		{
			if (dims == null) return new Vector2[0];
			var result = new Vector2[dims.Length];
			for (int i = 0; i < dims.Length; i++)
			{
				if (dims[i] != null && dims[i].Length >= 2)
					result[i] = new Vector2(dims[i][0], dims[i][1]);
			}
			return result;
		}
	}

	public void Clear()
	{
		_loaded.Clear();
		_spriteAtlas.Clear();
	}

	public void PreloadAtlases()
	{
		PreloadCommonAtlases(null);
		PreloadInternationalAtlases(null, UnityApi.GetLanguage());
		RefreshAllSprites();
	}

	public Texture2D LoadTexture(string textureName)
	{
		string path = "__atlases/" + textureName;
		return Util.Resource<Texture2D>(path);
	}

	private void RefreshAllSprites()
	{
		Object[] array = Object.FindObjectsOfType(typeof(Sprite));
		Object[] array2 = array;
		foreach (Object obj in array2)
		{
			((Sprite)obj).Refresh();
		}
	}
}