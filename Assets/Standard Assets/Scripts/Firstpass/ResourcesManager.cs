using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

internal class ResourcesManager : SingletonT<ResourcesManager>
{
	public class AssetBundleData
	{
		public enum StateE
		{
			Normal,
			InCallback,
			RemoveUnloadAll
		}

		public AssetBundle Bundle;

		public string Path;

		public StateE State;

		public List<ActionD<string, AssetBundleData, float>> LastOnLoad = new List<ActionD<string, AssetBundleData, float>>();

		public Dictionary<string, AudioClip> Clips;

		public ActionD<string, AssetBundleData, float> AddFakeLastOnLoad()
		{
			ActionD<string, AssetBundleData, float> actionD = delegate
			{
			};
			LastOnLoad.Add(actionD);
			return actionD;
		}

		public void Reset()
		{
			if (Bundle != null)
			{
				Bundle = null;
			}
		}

		public override string ToString()
		{
			return "{0} {1} {2} {3}".Fmt(Bundle, Path, State, LastOnLoad.Count);
		}
	}

	private class ArmorCacheInfo
	{
		internal string PersonId;

		internal string Set;

		internal ServerData.Slot.TypeE Slot;

		internal GameObject Object;
	}

	internal static bool RemoveAssetBundleAll;

	private int _assetBundleCount;

	internal int LastLoadedSceneIndex = -1;

	private Dictionary<string, Texture2D> _mobsTextures = new Dictionary<string, Texture2D>();

	private Dictionary<string, Texture2D> _armorsTextures = new Dictionary<string, Texture2D>();

	private Dictionary<string, Texture2D> _commonTextures = new Dictionary<string, Texture2D>();

	private Dictionary<string, AssetBundleData> _assetsBundles = new Dictionary<string, AssetBundleData>();

	private static readonly float WaitWWWLoadTime = 100f;

	public static int LoadingAssets;

	public static int UnloadingUnusedAssets;

	public Texture2D LoadItemIcon(ServerData.Item item)
	{
		try
		{
			string text = null;
			string text2 = null;
			text = SingletonT<ServerData>.I.GetItemImageName(item);
			text2 = ((item.Slot.SlotId != ServerData.Slot.TypeE.Weapon) ? ("icons/armor/" + item.Set + "/") : "icons/weapon/");
			Invs.Inv(!string.IsNullOrEmpty(text), "!string.IsNullOrEmpty(textureName)");
			Texture2D texture2D = LoadTextureNT(text2, text, _armorsTextures);
			if (texture2D == null)
			{
				texture2D = LoadTexture("icons/armor/0/", "default_shop_item", _armorsTextures);
			}
			return texture2D;
		}
		catch (Exception ex)
		{
			Utils.Log("LoadItemIcon failed", item.ToString(), ex.Message);
			throw new Exception("LoadItemIcon failed " + item.ToString(), ex);
		}
	}

	public Texture2D LoadItemIcon(ServerData.Bonus.DropElement item)
	{
		if (item.IsItem && item.Item != null)
		{
			return LoadItemIcon(item.Item);
		}
		if (item.IsBonus)
		{
			return LoadTexture("icons/other/", "chest_amun_2_1", _commonTextures);
		}
		if (item.IsExp)
		{
			return LoadTexture("icons/other/", "prosvet_moneta_green", _commonTextures);
		}
		return null;
	}

	public void LoadText(string path, ActionD<TextReader> action)
	{
		try
		{
			Utils.Log("LoadText start", path);
			UnityEngine.Object obj = Util.Resource<UnityEngine.Object>(path);
			TextAsset textAsset = (TextAsset)obj;
			using (StringReader v = new StringReader(textAsset.text))
			{
				action(v);
			}
			Utils.Log("LoadText start ok", path);
		}
		catch (Exception ex_)
		{
			Utils.HandleError(ex_, "Load text failed path=" + path);
		}
	}

	public void LoadText(string path, ActionD<string> action, ActionD<string> onError)
	{
		try
		{
			UnityEngine.Object obj = Util.Resource<UnityEngine.Object>(path);
			TextAsset textAsset = (TextAsset)obj;
			string v = string.Empty;
			using (StringReader stringReader = new StringReader(textAsset.text))
			{
				v = stringReader.ReadToEnd();
			}
			action(v);
		}
		catch (Exception ex)
		{
			Utils.LogFrom("ResourcesManager", "LoadText failed", path, "{", ex.MessageAndStacktraceWithInners(Environment.NewLine, string.Empty, string.Empty), "}");
			onError(ex.Message);
		}
	}

	public void LoadBytes(string path, ActionD<byte[]> action, ActionD<string> onError)
	{
		try
		{
			TextAsset textAsset = Util.Resource<TextAsset>(path);
			action(textAsset.bytes);
		}
		catch (Exception ex)
		{
			Utils.LogFrom("ResourcesManager", "LoadText failed", path, "{", ex.MessageAndStacktraceWithInners(Environment.NewLine, string.Empty, string.Empty), "}");
			onError(ex.Message);
		}
	}

	public static string GetAssetBundlePath(string name)
	{
		return GetBaseAssetBundlePath() + name + ".unity3d";
	}

	public void GetAssetBundleAsync(MonoBehaviour caller, string assetBundlePath, ActionD<string, AssetBundleData, float> onLoad, ActionD<string, string> onError)
	{
		AssetBundleData abd = null;
		if (_assetsBundles.TryGetValue(assetBundlePath, out abd))
		{
			abd.State = AssetBundleData.StateE.Normal;
			if (abd.Bundle != null)
			{
				onLoad(assetBundlePath, abd, 0f);
			}
			else
			{
				abd.LastOnLoad.Add(onLoad);
			}
			return;
		}
		abd = new AssetBundleData();
		_assetsBundles.Add(assetBundlePath, abd);
		abd.LastOnLoad.Add(onLoad);
		float startTime = Time.realtimeSinceStartup;
		caller.StartCoroutine(Utils.WWWLoadAssetBundle(assetBundlePath, WaitWWWLoadTime, delegate(string _, WWW www)
		{
			LoadingAssets--;
			float v = Time.realtimeSinceStartup - startTime;
			if (abd.Bundle != null)
			{
				onLoad(assetBundlePath, abd, v);
			}
			else
			{
				AssetBundle assetBundle = www.assetBundle;
				_assetBundleCount++;
				if (_assetBundleCount > 3)
				{
					Utils.LogForce("LOAD ASSET BUNDLE: too many concurrently loading asset bundles");
				}
				abd.Bundle = assetBundle;
				abd.Path = assetBundlePath;
				List<ActionD<string, AssetBundleData, float>> lastOnLoad = abd.LastOnLoad;
				abd.LastOnLoad = new List<ActionD<string, AssetBundleData, float>>();
				if (abd.State == AssetBundleData.StateE.Normal)
				{
					abd.State = AssetBundleData.StateE.InCallback;
				}
				foreach (ActionD<string, AssetBundleData, float> item in lastOnLoad)
				{
					item(assetBundlePath, abd, v);
					if (abd.Bundle == null)
					{
						abd.Bundle = assetBundle;
					}
				}
				abd.State = AssetBundleData.StateE.Normal;
				if (!_assetsBundles.ContainsKey(assetBundlePath))
				{
					if (!Globals.DebugDontUnloadAssetsBundles)
					{
						UnloadAssetBundle(abd.Bundle, abd.State == AssetBundleData.StateE.RemoveUnloadAll);
					}
					abd.Reset();
				}
				else
				{
					abd.State = AssetBundleData.StateE.Normal;
				}
			}
		}, delegate(string _, string errorMessage)
		{
			LoadingAssets--;
			_assetsBundles.Remove(assetBundlePath);
			onError(_, errorMessage);
		}));
	}

	public void LoadAsync(MonoBehaviour caller, AssetBundle bundle, object[] objs, FuncD<object, string> getPrefabName, ActionD<object, string, GameObject> action, ActionD onAllLoaded)
	{
		int loaded = 0;
		object[] array = objs;
		foreach (object obj in array)
		{
			object t = obj;
			caller.StartCoroutine(Utils.LoadAssetBundleAsync(bundle, getPrefabName(obj), delegate(string _name, GameObject _go)
			{
				LoadingAssets--;
				action(t, _name, _go);
				loaded++;
				if (loaded == objs.Length)
				{
					onAllLoaded();
				}
			}));
		}
	}

	internal void RemoveAssetBundle(AssetBundleData ab)
	{
		if (ab.Bundle != null)
		{
			if (!Globals.DebugDontUnloadAssetsBundles)
			{
				UnloadAssetBundle(ab.Bundle, destroyObjs: false);
			}
			ab.Reset();
		}
		_assetsBundles.Remove(ab.Path);
	}

	internal void RemoveAssetBundle(AssetBundleData ab, string path)
	{
		RemoveAssetBundle(ab, path, RemoveAssetBundleAll);
	}

	internal void RemoveAssetBundleNoActions(AssetBundleData ab)
	{
		if (ab != null)
		{
			_assetsBundles.Remove(ab.Path);
		}
	}

	internal void RemoveAssetBundleAndDestroyAll(string path)
	{
		AssetBundleData value = null;
		if (_assetsBundles.TryGetValue(path, out value) && value.LastOnLoad.Count == 0)
		{
			_assetsBundles.Remove(path);
			if (value.Bundle != null)
			{
				UnloadAssetBundle(value.Bundle, destroyObjs: true);
				value.Bundle = null;
			}
		}
	}

	internal void RemoveAssetBundle(AssetBundleData ab, string path, bool removeAll)
	{
		if (ab.Bundle != null)
		{
			if (ab.State == AssetBundleData.StateE.Normal)
			{
				if (!Globals.DebugDontUnloadAssetsBundles)
				{
					UnloadAssetBundle(ab.Bundle, removeAll);
				}
				ab.Reset();
			}
			else if (removeAll)
			{
				ab.State = AssetBundleData.StateE.RemoveUnloadAll;
			}
		}
		_assetsBundles.Remove(path);
	}

	internal void UnloadAssetBundle(AssetBundle b, bool destroyObjs)
	{
		if (b != null)
		{
			_assetBundleCount--;
			b.Unload(destroyObjs);
		}
	}

	public IEnumerator WaitUpdate(ActionD action)
	{
		yield return null;
		action?.Invoke();
	}

	public void CreatePerson(MonoBehaviour caller, string id, ActionD<string, GameObject> onLoad)
	{
		string path = GetAssetBundlePath(GetPathCharacter(id));
		GetFromAssetBundleAsync(caller, path, id, unloadAssetBundle: false, delegate(string _, AssetBundleData assetBundle, GameObject go, float time)
		{
			PersonData component = go.GetComponent<PersonData>();
			Utils.Log("GetPersonPrototype loaded AssetBundle", time, _);
			GameObject newgo = (GameObject)UnityEngine.Object.Instantiate(go);
			FromAssetBundle fromAssetBundle = newgo.AddComponent<FromAssetBundle>();
			fromAssetBundle.Path = path;
			caller.StartCoroutine(WaitUpdate(delegate
			{
				onLoad(id, newgo);
			}));
		}, delegate(string path_, string error)
		{
			if (Globals.DebugNoBundles)
			{
				Utils.Log("GetPersonPrototype failed (no-bundles fallback) creating placeholder:", id, error, path_);
				GameObject placeholder = new GameObject("PlayerPlaceholder_" + id);
				placeholder.AddComponent<PersonData>();
				FromAssetBundle fab = placeholder.AddComponent<FromAssetBundle>();
				fab.Path = path_;
				caller.StartCoroutine(WaitUpdate(delegate {
					onLoad(id, placeholder);
				}));
				return;
			}
			Invs.Inv(false, "GetPersonPrototype failed", id, error, path_);
		});
	}

	public void GetPersonPrototype(MonoBehaviour caller, string id, string fileSuffix, ActionD<string, GameObject> onLoad)
	{
		string assetBundlePath = GetAssetBundlePath(GetPathCharacter(id, fileSuffix));
		GetFromAssetBundleAsync(caller, assetBundlePath, id + fileSuffix, unloadAssetBundle: false, delegate(string _, AssetBundleData assetBundle, GameObject go, float time)
		{
			Utils.Log("GetPersonPrototype loaded AssetBundle", go, time, _);
			onLoad(id, go);
		}, delegate(string path_, string error)
		{
			Invs.Inv(false, "GetPersonPrototype failed", id, error, path_);
		});
	}

	public void GetPersonPrototypeAndRemoveAssetBundle(MonoBehaviour caller, string id, string fileSuffix, ActionD<string, GameObject> onLoad)
	{
		string assetBundlePath = GetAssetBundlePath(GetPathCharacter(id, fileSuffix));
		GetFromAssetBundleAsync(caller, assetBundlePath, id + fileSuffix, delegate(string _, AssetBundleData assetBundle, GameObject go, float time)
		{
			Utils.Log("GetPersonPrototype loaded AssetBundle", go, time, _);
			onLoad(id, go);
		}, delegate(string path_, string error)
		{
			Invs.Inv(false, "GetPersonPrototype failed", id, error, path_);
		});
	}

	public GameObject LoadSceneObject(string name)
	    {
	        UnityEngine.Object obj = Util.Resource<UnityEngine.Object>("_scene_objects/" + name);
	        if (obj == null || !(obj is GameObject))
	        {
	            Debug.LogWarning($"[ResourcesManager] Scene object '{name}' not found in Resources. Creating placeholder.");
	            var fallback = new GameObject(name);
	            if (name == "__battle")
	            {
	                var battle = fallback.AddComponent<Battle>();
	                var battleGui = fallback.AddComponent<BattleGui>();
	                battleGui.enabled = false; // disable until HudMk1 is ready
	                var selectGui = fallback.AddComponent<SelectGui>();
	                selectGui.enabled = false;
	                // Wire up fields the Battle Update methods expect
	                battle.BattleGui = battleGui;
	                battle.SelectGui = selectGui;
	                battle.BattleCameraController = fallback.AddComponent<BattleCameraController>();
	                battle.StartBattleCamera = fallback.AddComponent<StartBattleCamera>();
	                // arena_center: StartBattleCamera.Start() does GameObject.Find("arena_center")
	                var arenaCenter = new GameObject("arena_center");
	                arenaCenter.transform.SetParent(fallback.transform);
	                arenaCenter.transform.position = Vector3.zero;
	                // Add some visible elements
	                AddBattlePlaceholderElements(fallback);
	            }
	            if (name == "__battle_camera")
	                        {
	                            Debug.Log("[Placeholder] Creating __battle_camera with camera_upper child");
	                            var battleCam = new GameObject("camera_upper");
	                            battleCam.transform.SetParent(fallback.transform);
	                            var cam = battleCam.AddComponent<Camera>();
	                            cam.clearFlags = CameraClearFlags.SolidColor;
	                            cam.backgroundColor = new Color(0.3f, 0.4f, 0.6f);
	                            cam.nearClipPlane = 0.3f;
	                            cam.farClipPlane = 100f;
	                            cam.fieldOfView = 60f;
	                            battleCam.transform.position = new Vector3(0, 2, -5);
	                            battleCam.transform.LookAt(Vector3.zero);
	                            battleCam.AddComponent<AudioListener>();
	                        }
	            return fallback;
	        }
	        return (GameObject)obj;
	    }
    
	    private void AddBattlePlaceholderElements(GameObject battleGo)
	        {
	            // Build a simple visible arena using procedural meshes (CreatePrimitive/Materials
	            // get stripped by IL2CPP; these always work)
	            try
	            {
	                // Ground quad
	                var ground = CreateQuad("Ground", new Vector3(10, 1, 10));
	                ground.transform.SetParent(battleGo.transform);
	                ground.transform.position = new Vector3(0, -0.5f, 0);
	                ground.transform.Rotate(-90, 0, 0);
            
	                // Center marker cube
	                var marker = CreateCube("Center", new Vector3(0.5f, 2, 0.5f));
	                marker.transform.SetParent(battleGo.transform);
	                marker.transform.position = new Vector3(0, 1, 0);
            
	                // Four pillars
	                for (int i = -1; i <= 1; i += 2)
	                {
	                    for (int j = -1; j <= 1; j += 2)
	                    {
	                        var pillar = CreateCube("Pillar_" + i + "_" + j, new Vector3(0.3f, 1.5f, 0.3f));
	                        pillar.transform.SetParent(battleGo.transform);
	                        pillar.transform.position = new Vector3(i * 3, 1.5f, j * 3);
	                    }
	                }
            
	                // Light
	                var lightGo = new GameObject("Directional Light");
	                lightGo.transform.SetParent(battleGo.transform);
	                var light = lightGo.AddComponent<Light>();
	                light.type = LightType.Directional;
	                light.intensity = 0.8f;
	                light.transform.rotation = Quaternion.Euler(50, -30, 0);
	            }
	            catch (System.Exception e)
	            {
	                Debug.LogWarning($"[Placeholder] Arena build failed: {e.Message}");
	            }
	        }
    
	        private GameObject CreateQuad(string name, Vector3 size)
	        {
	            var go = new GameObject(name);
	            var mf = go.AddComponent<MeshFilter>();
	            var mesh = new Mesh();
	            mesh.vertices = new[]
	            {
	                new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f),
	                new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f)
	            };
	            mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
	            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
	            mesh.RecalculateNormals();
	                    mf.sharedMesh = mesh;
	                    var mr = go.AddComponent<MeshRenderer>();
	                    mr.sharedMaterial = MakeMaterial(new Color(0.4f, 0.45f, 0.5f));
	                    go.transform.localScale = size;
	                    return go;
	                }
    
	                private GameObject CreateCube(string name, Vector3 size)
	                {
	                    var go = new GameObject(name);
	                    var mf = go.AddComponent<MeshFilter>();
	                    mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
	                    var mr = go.AddComponent<MeshRenderer>();
	                    mr.sharedMaterial = MakeMaterial(new Color(0.6f, 0.6f, 0.65f));
	                    go.transform.localScale = size;
	                    return go;
	                }
    
	                private Material MakeMaterial(Color color)
	                    {
	                        var mat = new Material(Shader.Find("Standard"));
	                        if (mat == null || mat.shader == null)
	                            return null;
	                        mat.color = color;
	                        return mat;
	                    }

	public T CreateSceneObject<T>(string name) where T : Component
	{
		GameObject original = LoadSceneObject(name);
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(original);
		gameObject.name = name;
		T component = gameObject.GetComponent<T>();
		Invs.Inv(component != null, "CreateSceneObject", name);
		return component;
	}

	internal void LoadScene(MonoBehaviour caller, int index, ActionD<string, GameObject> onLoad)
	{
		Utils.Log("=======================LOADSCENE", index);
		string name = index + "_iOS";
		string path = GetAssetBundlePath("scenes/" + name);
		GetFromAssetBundleAsync(caller, path, name, unloadAssetBundle: false, delegate(string _, AssetBundleData __, GameObject go, float time)
		{
			LastLoadedSceneIndex = index;
			Utils.Log("LoadScene", name, time, go != null);
			go.name = Globals.LocationGameObjectSceneGeomName;
			onLoad(path, go);
		}, delegate(string path_, string error)
		{
			if (Globals.DebugNoBundles)
			{
				Utils.Log("LoadScene failed (no-bundles fallback) creating placeholder:", index, error);
				GameObject placeholder = new GameObject("ScenePlaceholder_" + index);
				LastLoadedSceneIndex = index;
				placeholder.name = Globals.LocationGameObjectSceneGeomName;
				onLoad(path_, placeholder);
				return;
			}
			Invs.Inv(false, "LoadScene failed", index, error, path_);
		});
	}

	internal void LoadHair(MonoBehaviour caller, string modelId, string hairId, ActionD<GameObject> onLoad, ActionD<string, string> onError)
	{
		Utils.Log("LOAD HAIR ", modelId, hairId);
		string path = GetAssetBundlePath($"characters/{modelId}/hairs/{hairId}");
		GetAssetBundleAsync(caller, path, delegate(string _, AssetBundleData ab, float time)
		{
			onLoad(ab.Bundle.mainAsset as GameObject);
			RemoveAssetBundle(ab, path);
		}, onError);
	}

	internal void LoadArmorSet(MonoBehaviour caller, string personId, string setName, ActionD<string, AssetBundleData> onLoad, ActionD<string, string> onError)
	{
		Invs.Inv(setName != null, "LoadArmorSet setName != null", personId);
		string path = string.Format("characters/{1}/armors/{0}m", setName, personId);
		path = GetAssetBundlePath(path);
		GetAssetBundleAsync(caller, path, delegate(string _, AssetBundleData ab, float time)
		{
			onLoad(path, ab);
		}, onError);
	}

	internal void LoadArmor(MonoBehaviour caller, string personId, string setName, ServerData.Slot.TypeE slot, ActionD<string, GameObject> onLoad, ActionD<string, string> onError)
	{
		LoadArmorAsync(caller, personId, setName, slot, onLoad, onError);
	}

	internal void LoadArmorAsync(MonoBehaviour caller, string personId, string setName, ServerData.Slot.TypeE slot, ActionD<string, GameObject> onLoad, ActionD<string, string> onError)
	{
		string text = slot.PrefabName(setName);
		string name = string.Format("characters/{2}/armors/{0}/{1}", setName, text, personId);
		if (slot == ServerData.Slot.TypeE.Weapon)
		{
			name = $"characters/weapons/{setName}";
		}
		if (slot == ServerData.Slot.TypeE.Eyes)
		{
			text = ((!(personId == "2")) ? "man_eyes_" : "woman_eyes_") + ((setName.Length != 1) ? string.Empty : "0") + setName;
			name = string.Format("characters/{1}/armors/eyes/{0}", setName, personId);
		}
		string abPath = GetAssetBundlePath(name);
		GetFromAssetBundleAsync(caller, abPath, text, unloadAssetBundle: false, delegate(string _, AssetBundleData assetBundle, GameObject go, float time)
		{
			onLoad(abPath, go);
		}, onError);
	}

	internal void UnloadUnusedAssetsFake(MonoBehaviour caller, ActionD action)
	{
		action();
	}

	internal void UnloadUnusedAssets(MonoBehaviour caller, ActionD action)
	{
		if (!(caller == null))
		{
			caller.StartCoroutine(UnloadAssets(action));
		}
	}

	internal void LoadAnimations(bool dontLoad, string model, MonoBehaviour go, PersonArmor personArmor, ActionD<string> onLoad)
	{
		if (dontLoad)
		{
			onLoad(null);
		}
		else if (personArmor.GetComponent<Animation>().GetClipCount() == 0)
		{
			string text = "none";
			if (personArmor.Weapon != null)
			{
				ArmorData component = personArmor.Weapon.GetComponent<ArmorData>();
				switch (component.WeaponAnimationType)
				{
				case AnimationTypes.Glaive:
					text = "glave";
					break;
				case AnimationTypes.Hammer:
					text = "hammer";
					break;
				case AnimationTypes.OneHanded:
					text = "onehanded";
					break;
				case AnimationTypes.TwoHanded:
					text = "twohanded";
					break;
				}
			}
			string path = GetAssetBundlePath(GetPathCharacter(model, "_animations_" + text));
			GetAssetBundleAsync(go, path, delegate(string abPath, AssetBundleData ab, float __)
			{
				AnimationsData component2 = ((GameObject)ab.Bundle.mainAsset).GetComponent<AnimationsData>();
				Animation animation = personArmor.gameObject.GetComponent<Animation>();
				AnimationClip[] animationClips = component2.AnimationClips;
				foreach (AnimationClip animationClip in animationClips)
				{
					if (!(animationClip == null))
					{
						if (animation[animationClip.name] != null)
						{
							animation.RemoveClip(animationClip.name);
						}
						animation.AddClip(animationClip, animationClip.name);
					}
				}
				if (component2.AnimationInBag != null)
				{
					animation.AddClip(component2.AnimationInBag, "idle_bag");
				}
				if (component2.AnimationInBag2 != null)
				{
					animation.AddClip(component2.AnimationInBag2, "idle_bag2");
				}
				RemoveAssetBundle(ab, abPath);
				onLoad(abPath);
			}, delegate(string _, string msg)
			{
				Utils.Log("LoadAnimations failed", go.name, path, _, msg);
			});
		}
		else
		{
			onLoad(null);
		}
	}

	private string Int2(int i)
	{
		string text = i.ToString();
		return (text.Length != 1) ? text : ("0" + text);
	}

	private string GetPathCharacter(string id)
	{
		return string.Format("characters/{0}/{0}", id);
	}

	private string GetPathCharacter(string id, string fileSuffix)
	{
		return string.Format("characters/{0}/{0}{1}", id, fileSuffix);
	}

	public static string GetBaseAssetBundlePath()
	{
		return "jar:file://" + ((!UnityApi.UseSingleApk()) ? (UnityApi.GetMainObbPath() + "!/assets/android/") : (Application.dataPath + "!/assets/android/"));
	}

	internal string AssetsBundlesInfo()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		stringBuilder.Append(_assetsBundles.Count + ":");
		foreach (KeyValuePair<string, AssetBundleData> assetsBundle in _assetsBundles)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("[{0}; bundle={1};{2}] ", assetsBundle.Key, assetsBundle.Value.Bundle != null, assetsBundle.Value.LastOnLoad.Count);
		}
		return stringBuilder.ToString();
	}

	internal void PrintInfo()
	{
		Utils.Log(AssetsBundlesInfo());
	}

	private void GetFromAssetBundleAsync<T>(MonoBehaviour caller, string assetBundlePath, string name, bool unloadAssetBundle, ActionD<string, AssetBundleData, T, float> onLoad, ActionD<string, string> onError) where T : UnityEngine.Object
	{
		Utils.LogForce("GetFromAssetBundleAsync2", name, assetBundlePath);
		float startTime = Time.realtimeSinceStartup;
		GetAssetBundleAsync(caller, assetBundlePath, delegate(string _, AssetBundleData ab, float abt)
		{
			ActionD<string, AssetBundleData, float> locker = ab.AddFakeLastOnLoad();
			caller.StartCoroutine(Utils.LoadAssetBundleAsync(ab.Bundle, name, delegate(string __, T go)
			{
				LoadingAssets--;
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				onLoad(assetBundlePath, ab, go, realtimeSinceStartup - startTime);
				RemoveAssetBundle(ab, assetBundlePath);
				ab.LastOnLoad.Remove(locker);
			}));
		}, onError);
	}

	private void GetFromAssetBundleAsync<T>(MonoBehaviour caller, string assetBundlePath, string name, ActionD<string, AssetBundleData, T, float> onLoad, ActionD<string, string> onError) where T : UnityEngine.Object
	{
		Utils.LogForce("GetFromAssetBundleAsync3", name, assetBundlePath);
		float startTime = Time.realtimeSinceStartup;
		GetAssetBundleAsync(caller, assetBundlePath, delegate(string _, AssetBundleData ab, float abt)
		{
			ActionD<string, AssetBundleData, float> locker = ab.AddFakeLastOnLoad();
			caller.StartCoroutine(Utils.LoadAssetBundleAsync(ab.Bundle, name, delegate(string __, T go)
			{
				LoadingAssets--;
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				onLoad(assetBundlePath, ab, go, realtimeSinceStartup - startTime);
				RemoveAssetBundle(ab);
				ab.LastOnLoad.Remove(locker);
			}));
		}, onError);
	}

	public static WWW CreateAssetBundleWWW(string path)
	{
		return new WWW(GetAssetBundlePath("resources/" + path));
	}

	public GameObject CreateSceneObject(string name)
	{
		GameObject original = LoadSceneObject(name);
		return (GameObject)UnityEngine.Object.Instantiate(original);
	}

	private IEnumerator UnloadAssets(ActionD action)
	{
		Utils.LogForce("**** UnloadUnusedAssets start");
		float t = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - t < 1f)
		{
			yield return null;
		}
		UnloadingUnusedAssets++;
		AsyncOperation async = Resources.UnloadUnusedAssets();
		while (!async.isDone)
		{
			yield return null;
		}
		UnloadingUnusedAssets--;
		Utils.Log("**** UnloadUnusedAssets finish");
		action?.Invoke();
	}

	private static Texture2D LoadTexture(string path, string name, Dictionary<string, Texture2D> cache)
	{
		Invs.Inv(name != null, path);
		path += name;
		if (!cache.TryGetValue(path, out var value))
		{
			value = (cache[path] = Util.Resource<Texture2D>(path));
		}
		Invs.Inv(value != null, name, path);
		return value;
	}

	private static Texture2D LoadTextureNT(string path, string name, Dictionary<string, Texture2D> cache)
	{
		Invs.Inv(name != null, path);
		path += name;
		if (!cache.TryGetValue(path, out var value))
		{
			value = (cache[path] = Util.Resource<Texture2D>(path));
		}
		return value;
	}

	private static Texture2D LoadTexture(string path, string name, string defaultName, Dictionary<string, Texture2D> cache)
	{
		string text = path;
		if (name != null)
		{
			path += name;
		}
		if (!cache.TryGetValue(path, out var value))
		{
			UnityEngine.Object obj = Util.Resource<UnityEngine.Object>(path);
			if (obj == null)
			{
				obj = Util.Resource<UnityEngine.Object>(text + defaultName);
			}
			value = (cache[path] = (Texture2D)obj);
		}
		Invs.Inv(value != null, name, path, defaultName);
		return value;
	}
}
