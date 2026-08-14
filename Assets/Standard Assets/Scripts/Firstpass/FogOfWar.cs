using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarx;

public class FogOfWar : MonoBehaviour
{
	public Color DarkMost = new Color(0.5f, 0f, 0f, 0.8f);

	public string MasksStem = "main_map/fogofwar/MapMask_{0:00}";

	private CompositeDisposable _subscriptions;

	private readonly HashSet<int> _openedSoFar = new HashSet<int>();

	private Mesh _mesh;

	public void DarkAndRefresh()
	{
		FillDark();
		_openedSoFar.Clear();
		RefreshOpenedAreas();
	}

	private void FillDark()
	{
		_mesh.SetTint(DarkMost);
	}

	private void RefreshOpenedAreas()
	{
		if (Globals.IsDebugBuild)
		{
			Debug.Log("FOG OF WAR: RefreshOpenedAreas");
		}
		foreach (ServerData.Location item in SingletonT<ServerData>.I._locations.Values.Where(IsLoactionOpened))
		{
			AddMask(item.MapId);
		}
		AddMask(14);
	}

	private bool IsLoactionOpened(ServerData.Location loc)
	{
		return loc.IsOpened || loc.IsZachistkaOpened || loc.IsShowMoney || loc.IsShowMobs;
	}

	private void RefreshOpenedLocation(ServerData.Location location, int unused)
	{
		if (IsLoactionOpened(location))
		{
			AddMask(location.MapId);
		}
	}

	private string MapIdToPath(int id)
	{
		int num = 0;
		return string.Format(MasksStem, id switch
		{
			1 => 5, 
			2 => 4, 
			3 => 15, 
			4 => 6, 
			5 => 2, 
			6 => 1, 
			7 => 7, 
			8 => 10, 
			9 => 8, 
			10 => 9, 
			11 => 11, 
			12 => 3, 
			13 => 13, 
			14 => 12, 
			15 => 14, 
			16 => 4, 
			17 => 8, 
			998 => 2, 
			_ => 5, 
		});
	}

	private void AddMask(int n)
	{
		if (_openedSoFar.Contains(n))
		{
			return;
		}
		_openedSoFar.Add(n);
		string path = MapIdToPath(n);
		Texture2D texture2D = Util.Resource<Texture2D>(path, typeof(Texture2D));
		List<Color> list = new List<Color>(_mesh.colors);
		Color[] pixels = texture2D.GetPixels();
		if (pixels.Length != list.Count && Globals.IsDebugBuild)
		{
			Debug.LogError($"Fuck-up: pixels-colors count. {pixels.Length}/{list.Count}");
		}
		for (int i = 0; i < list.Count; i++)
		{
			Color color = list[i];
			Color color2 = pixels[i];
			float num = 1f - color2.grayscale;
			if (num < color.a)
			{
				list[i] = new Color(color.r, color.g, color.b, num);
			}
		}
		_mesh.colors = list.ToArray();
	}

	private void Awake()
	{
		_mesh = base.transform.GetComponent<MeshFilter>().mesh;
	}

	private void OnEnable()
	{
		_subscriptions = new CompositeDisposable();
		_subscriptions.Add(Messenger<ServerData.Location>.AddListener(Globals.MsgZachistkaProgressChanged, delegate
		{
			RefreshOpenedAreas();
		}));
		_subscriptions.Add(Messenger<ServerData.Location, int>.AddListener(Globals.MsgLocationMobsAdded, RefreshOpenedLocation));
		_subscriptions.Add(Messenger<ServerData.Location, int, int>.AddListener(Globals.MsgLocationMoneyChanged, delegate(ServerData.Location _1, int _2, int _3)
		{
			RefreshOpenedLocation(_1, _3);
		}));
		_subscriptions.Add(Messenger<ServerData.Location, int>.AddListener(Globals.MsgLocationPopulationChanged, RefreshOpenedLocation));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgNewPersInited, DarkAndRefresh));
		_subscriptions.Add(Messenger.AddListener(Globals.MsgLoadThenContinueGame, DarkAndRefresh));
	}

	private void OnDisable()
	{
		_subscriptions.Dispose();
		_openedSoFar.Clear();
	}

	private void Start()
	{
	}
}
