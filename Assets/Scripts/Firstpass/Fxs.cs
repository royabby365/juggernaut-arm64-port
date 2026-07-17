using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

internal class Fxs : SingletonT<Fxs>
{
	private class FxData
	{
		internal UnityEngine.Object Fx;

		internal string Tag;
	}

	internal static string CurrentFxTag = string.Empty;

	internal static List<GameObject> Fxses = new List<GameObject>();

	private Dictionary<string, FxData> _fxCache = new Dictionary<string, FxData>();

	private static FieldInfo _sfxReatcTextFieldInfoCache = null;

	internal GameObject NewFx(string prototype, Vector3 position, Quaternion rotation, Transform parent, bool forceDraw)
	{
		UnityEngine.Object obj = LoadFx(prototype);
		if (obj == null)
		{
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(obj, position, rotation);
		if (parent != null)
		{
			gameObject.transform.parent = parent;
			gameObject.transform.rotation = Quaternion.identity;
		}
		if (!forceDraw)
		{
			PostCreateFx(gameObject);
		}
		return gameObject;
	}

	private static GameObject NewFx(GameObject prototype, Vector3 position, Transform parent)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prototype, position, Quaternion.identity);
		gameObject.transform.parent = parent;
		PostCreateFx(gameObject);
		return gameObject;
	}

	internal static void DestroyAllInbattleFxs()
	{
		foreach (GameObject fxse in Fxses)
		{
			try
			{
				if (fxse != null && fxse.tag == Battle.InBattleFxTag)
				{
					UnityEngine.Object.Destroy(fxse);
				}
			}
			catch (Exception)
			{
			}
		}
		Fxses.Clear();
	}

	internal static void PostCreateFx(GameObject fx)
	{
		fx.transform.tag = CurrentFxTag;
		Light[] componentsInChildren = fx.GetComponentsInChildren<Light>();
		foreach (Light obj in componentsInChildren)
		{
			UnityEngine.Object.Destroy(obj);
		}
		ParticleRenderer[] componentsInChildren2 = fx.GetComponentsInChildren<ParticleRenderer>();
		foreach (ParticleRenderer particleRenderer in componentsInChildren2)
		{
			particleRenderer.uvAnimationXTile = 1;
			particleRenderer.uvAnimationYTile = 1;
		}
		ParticleEmitter[] componentsInChildren3 = fx.GetComponentsInChildren<ParticleEmitter>();
		foreach (ParticleEmitter particleEmitter in componentsInChildren3)
		{
			if (particleEmitter.maxEmission > 20f)
			{
				float maxEmission = particleEmitter.maxEmission;
				particleEmitter.maxEmission = (int)((double)maxEmission * 0.25);
			}
		}
		if (!Globals.DebugFxOptimizerOn || !(FxOptimizer.I != null) || CurrentFxTag.Length <= 0)
		{
			return;
		}
		Texture[] textures = FxOptimizer.I.Textures;
		ParticleRenderer[] componentsInChildren4 = fx.GetComponentsInChildren<ParticleRenderer>();
		foreach (ParticleRenderer particleRenderer2 in componentsInChildren4)
		{
			if (textures.IndexOf(particleRenderer2.material.mainTexture) >= 0)
			{
				particleRenderer2.particleEmitter.emit = false;
				particleRenderer2.enabled = false;
				particleRenderer2.particleEmitter.enabled = false;
				particleRenderer2.gameObject.SetActiveRecursivelyMk1(setActive: false);
				Fxses.Add(particleRenderer2.gameObject);
			}
		}
	}

	internal GameObject PlayPersonFx(GameObject fxPrototype, GameObject person)
	{
		Transform transform = person.transform.Find("bones");
		if (transform == null)
		{
			return null;
		}
		return NewFx(fxPrototype, transform.transform.position, transform);
	}

	internal GameObject PlayPersonFx(GameObject fxPrototype, GameObject person, string pos)
	{
		Transform transform = person.transform.FindChildByName(pos);
		if (transform == null)
		{
			Utils.Log("PlayPersonFx failed " + pos);
			return PlayPersonFx(fxPrototype, person);
		}
		return NewFx(fxPrototype, transform.transform.position, transform);
	}

	internal void PlayPersonHpChange(GameObject fxPrototype, GameObject person, int damage, bool isForce)
	{
		if (damage == 0)
		{
			return;
		}
		GameObject gameObject = PlayPersonFx(fxPrototype, person);
		if (!(gameObject == null))
		{
			MeshRenderer componentInChildren = gameObject.GetComponentInChildren<MeshRenderer>();
			if (componentInChildren != null)
			{
				componentInChildren.material.color = ((damage >= 0) ? Color.green : ((!isForce) ? Color.yellow : Color.red));
			}
			Component component = gameObject.GetComponent<sfx_react>();
			if (component != null)
			{
				Utils.SetValue(component, "text", ((damage <= 0) ? string.Empty : "+") + damage, ref _sfxReatcTextFieldInfoCache);
			}
		}
	}

	internal void SetSfxReactText(GameObject go, string text)
	{
		Component component = go.GetComponent<sfx_react>();
		if (component != null)
		{
			Utils.SetValue(component, "text", text, ref _sfxReatcTextFieldInfoCache);
		}
	}

	internal void CleanFxCache()
	{
		_fxCache.Clear();
	}

	internal void CleanFxCache(string tag)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, FxData> item in _fxCache)
		{
			if (tag == item.Value.Tag)
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			_fxCache.Remove(item2);
		}
	}

	internal UnityEngine.Object LoadFx(string path)
	{
		return LoadFx(path, null);
	}

	internal UnityEngine.Object LoadFx(string path, string tag)
	{
		FxData value = null;
		if (!_fxCache.TryGetValue(path, out value))
		{
			UnityEngine.Object obj = Util.Resource<UnityEngine.Object>("effects/" + path);
			if (obj != null)
			{
				Utils.Log("LOADFX", path);
			}
			FxData fxData = new FxData();
			fxData.Fx = obj;
			fxData.Tag = tag;
			value = fxData;
			_fxCache.Add(path, value);
		}
		return value.Fx;
	}
}
