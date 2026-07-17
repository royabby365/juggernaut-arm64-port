using System.Collections.Generic;
using UnityEngine;
using Yarx.Collections;

public class AtlasManager : SingletonT<AtlasManager>
{
	private readonly Dictionary<string, Atlas> _loaded = new Dictionary<string, Atlas>();

	private readonly Dictionary<string, Tuple<Atlas, int>> _spriteAtlas = new Dictionary<string, Tuple<Atlas, int>>();

	private readonly string[] _atlasNamesCommon = new string[10] { "a_assorted", "a_assorted_big", "a_assorted_big_2", "a_assorted_big_3", "a_assorted_big_4", "a_gui_alpha", "a_gui_alpha_2", "a_gui_alpha_3", "a_magic_book_bg", "a_advertising" };

	private readonly string[] _atlasNamesInternational = new string[1] { "a_international" };

	private readonly string[] _atlasNamesSets = new string[1] { "a_icons_set_" };

	public Tuple<Atlas, int> GetAtlasBySpriteName(string spriteName)
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
				_spriteAtlas[text] = Tuple.Create(atlas, i);
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
			return null;
		}
		gameObject.active = false;
		Atlas component = gameObject.transform.GetComponent<Atlas>();
		_loaded[atlasName] = component;
		return _loaded[atlasName];
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
