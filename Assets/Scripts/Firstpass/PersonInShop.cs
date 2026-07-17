using UnityEngine;

internal class PersonInShop : MonoBehaviour
{
	private class ArmorPartData
	{
		public readonly string SetName;

		public readonly ServerData.Slot Slot;

		public ArmorPartData(string setName, ServerData.Slot slot)
		{
			SetName = setName;
			Slot = slot;
		}
	}

	public Vector3 CharacterInitPosition;

	public Vector3 CharacterInitAngles;

	private GameObject _person;

	private void LoadPerson(ref GameObject person, string prototypeId)
	{
		if (!string.IsNullOrEmpty(prototypeId))
		{
			person = Globals.PlayerGameObject;
			if (person != null)
			{
				person.transform.position = CharacterInitPosition;
				person.transform.rotation = Quaternion.Euler(CharacterInitAngles.x, CharacterInitAngles.y, CharacterInitAngles.z);
				person.active = true;
				person.GetComponent<PersonArmor>().PutAllPlayerArmor();
			}
		}
	}

	private void SetupIdle()
	{
		PersonShopData component = _person.GetComponent<PersonShopData>();
		Utils.Log("personShopData", component != null);
		string text = "idle";
		if (component != null)
		{
			if (_person.animation[Globals.ShopIdleAnimationName] == null)
			{
				AnimationClip[] array = Utils.MakeArray((AnimationClip _) => _ != null, component.HammerInShopIdles);
				if (array.Length > 0)
				{
					AnimationClip clip = array[Random.Range(0, array.Length - 1)];
					text = Globals.ShopIdleAnimationName;
					if (_person.animation[text] != null)
					{
						_person.animation.RemoveClip(text);
					}
					_person.animation.AddClip(clip, text);
				}
			}
			else
			{
				text = Globals.ShopIdleAnimationName;
			}
		}
		if (_person != null && _person.animation[text] != null)
		{
			_person.animation[text].wrapMode = WrapMode.Loop;
			_person.animation.Play(text);
			_person.animation.cullingType = AnimationCullingType.BasedOnRenderers;
		}
	}

	private Rect ButttonPos(int i)
	{
		int num = 30;
		return new Rect(10f, 10 + 30 * i, 250f, num);
	}

	private void Awake()
	{
		if (SingletonT<ServerData>.I.PlayerServerPersData != null)
		{
			LoadPerson(ref _person, SingletonT<ServerData>.I.PlayerServerPersData.ModelId);
		}
	}
}
