using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElasticPopup : MonoBehaviour
{
	public Transform background;

	public Transform topleft;

	public int hPadding = 54;

	public int vPadding = 40;

	public int colGap = 16;

	public int colWidth = 300;

	public int rowHeight = 40;

	public int buttonsWidth = 197;

	public int buttonsHeight = 289;

	public int headerShift = 60;

	public Transform cancelButton;

	public Transform sellButton;

	public Transform putonButton;

	public GameObject compareRow;

	public GameObject compareArrowRow;

	public GameObject compareIcon;

	public Transform col0root;

	public Transform col1root;

	public Transform buttonPad;

	public GameObject panelDark;

	public GameObject panelDarkBottom;

	public GameObject panelLight;

	public GameObject panelLightTop;

	public GameObject panelLightBottom;

	public GameObject popupTop;

	public GameObject popupMiddle;

	public GameObject popupBottom;

	public GameObject headerText;

	public GameObject itemText;

	public string currentText = "<???>";

	public string selectedText = "<???>";

	private Vector3 _bgpos;

	private Vector3 _col0pos;

	private Vector3 _col1pos;

	public Rect Compare(ServerData.Item old, ServerData.Item fresh)
	{
		return Make2Col(old, fresh);
	}

	public Rect Compare(ServerData.Item fresh)
	{
		return Make1Col(fresh);
	}

	public Rect Compare(InventoryItemButton old, InventoryItemButton fresh)
	{
		return Compare(old.shopItem, fresh.shopItem);
	}

	public Rect Compare(InventoryItemButton fresh)
	{
		return Compare(fresh.shopItem);
	}

	public void Cleanup()
	{
		Object.Destroy(col0root.gameObject);
		Object.Destroy(col1root.gameObject);
		Object.Destroy(background.gameObject);
		background = null;
		col0root = null;
		col1root = null;
	}

	private void Awake()
	{
		_bgpos = background.localPosition;
		_col0pos = col0root.localPosition;
		_col1pos = col1root.localPosition;
	}

	private void RegenerateRoots()
	{
		GameObject gameObject = new GameObject();
		GameObject gameObject2 = new GameObject();
		GameObject gameObject3 = new GameObject();
		gameObject.layer = base.transform.gameObject.layer;
		gameObject2.layer = base.transform.gameObject.layer;
		gameObject3.layer = base.transform.gameObject.layer;
		gameObject.transform.parent = base.transform;
		background = gameObject.transform;
		background.localPosition = _bgpos;
		gameObject2.transform.parent = topleft;
		col0root = gameObject2.transform;
		col0root.localPosition = _col0pos;
		gameObject3.transform.parent = topleft;
		col1root = gameObject3.transform;
		col1root.localPosition = _col1pos;
	}

	private List<int> PrepareStats(ref List<KeyValuePair<string, int>> first, ref List<KeyValuePair<string, int>> second)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, int> item in first)
		{
			hashSet.Add(item.Key);
		}
		foreach (KeyValuePair<string, int> item2 in second)
		{
			hashSet.Add(item2.Key);
		}
		string skill;
		foreach (string item3 in hashSet)
		{
			skill = item3;
			if (!first.Exists((KeyValuePair<string, int> kv) => kv.Key == skill))
			{
				first.Add(new KeyValuePair<string, int>(skill, 0));
			}
			if (!second.Exists((KeyValuePair<string, int> kv) => kv.Key == skill))
			{
				second.Add(new KeyValuePair<string, int>(skill, 0));
			}
		}
		first = first.OrderBy((KeyValuePair<string, int> kv) => kv.Key).ToList();
		second = second.OrderBy((KeyValuePair<string, int> kv) => kv.Key).ToList();
		List<int> list = new List<int>();
		for (int num = 0; num < first.Count; num++)
		{
			list.Add(second[num].Value - first[num].Value);
		}
		return list;
	}

	private List<KeyValuePair<string, int>> ProcessItem(ServerData.Item item)
	{
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
		ServerData.SkillInfo[] skills = item.Skills;
		if (skills == null)
		{
			return list;
		}
		ServerData.SkillInfo[] array = skills;
		foreach (ServerData.SkillInfo skillInfo in array)
		{
			string key = skillInfo.Skill.AsString();
			int current = skillInfo.Current;
			list.Add(new KeyValuePair<string, int>(key, current));
		}
		return list;
	}

	private Rect Make2Col(ServerData.Item olditem, ServerData.Item newitem)
	{
		RegenerateRoots();
		List<KeyValuePair<string, int>> first = ProcessItem(olditem);
		List<KeyValuePair<string, int>> second = ProcessItem(newitem);
		List<int> deltas = PrepareStats(ref first, ref second);
		currentText = SingletonT<ServerData>.I.GetPhrase(ServerData.PhraseComparePuton);
		selectedText = SingletonT<ServerData>.I.GetPhrase(ServerData.PhraseCompareChosen);
		MakeHeader(col0root, currentText);
		MakeHeader(col1root, selectedText);
		Rect rect = MakeFirstCol(col0root, first, SingletonT<ResourcesManager>.I.LoadItemIcon(olditem), olditem.TitleString, olditem);
		MakeSecondCol(col1root, second, deltas, SingletonT<ResourcesManager>.I.LoadItemIcon(newitem), newitem.TitleString, newitem);
		int num = Mathf.Max(rect.height, buttonsHeight).RoundToInt() + headerShift;
		Rect result = MakeBg(background, hPadding + 2 * colWidth + colGap + buttonsWidth, num + vPadding);
		int num2 = ((result.width - (float)(2 * colWidth) - (float)colGap - (float)buttonsWidth) / 2f).RoundToInt();
		int num3 = -((result.height - (float)num) / 2f).RoundToInt() - headerShift;
		topleft.localPosition = new Vector3(num2, num3, 0f);
		col1root.localPosition = new Vector3(colWidth + colGap, 0f, 0f);
		buttonPad.localPosition = new Vector3(col1root.localPosition.x + (float)colWidth - 2f, 0f, 0f);
		return result;
	}

	private Rect Make1Col(ServerData.Item freshItem)
	{
		RegenerateRoots();
		List<KeyValuePair<string, int>> stats = ProcessItem(freshItem);
		int num = Mathf.Max(MakeFirstCol(col0root, stats, SingletonT<ResourcesManager>.I.LoadItemIcon(freshItem), freshItem.TitleString, freshItem).height, buttonsHeight).RoundToInt();
		Rect result = MakeBg(background, hPadding + colWidth + buttonsWidth, num + vPadding);
		int num2 = ((result.width - (float)colWidth - (float)buttonsWidth) / 2f).RoundToInt();
		int num3 = -((result.height - (float)num) / 2f).RoundToInt();
		topleft.localPosition = new Vector3(num2, num3, 0f);
		buttonPad.localPosition = new Vector3(colWidth - 2, 0f, 0f);
		return result;
	}

	private void MakeHeader(Transform root, string text)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(headerText);
		gameObject.transform.parent = root;
		gameObject.transform.SetLayerRecursively(root);
		gameObject.transform.localPosition = new Vector3(2f, 33f, -50f);
		gameObject.transform.GetComponent<SpriteText>().Text_ = text;
	}

	private void MakeIconAndTitle(Transform root, Texture2D icon, string itemName, ServerData.Item item)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(compareIcon);
		gameObject.transform.SetLayerRecursively(root);
		gameObject.transform.parent = root;
		gameObject.transform.localPosition = new Vector3(24f, -28f, -100f);
		gameObject.GetComponent<Renderer>().material.mainTexture = icon;
		GameObject gameObject2 = (GameObject)Object.Instantiate(itemText);
		gameObject2.transform.SetLayerRecursively(root);
		gameObject2.transform.GetComponent<SpriteText>().Text_ = itemName;
		gameObject2.transform.GetComponent<SpriteText>().SetColor(item.DecodeColor());
		gameObject2.transform.parent = root;
		gameObject2.transform.localPosition = new Vector3(92f, -14f, -100f);
	}

	private Rect MakeFirstCol(Transform root, List<KeyValuePair<string, int>> stats, Texture2D icon, string itemName, ServerData.Item item)
	{
		MakeIconAndTitle(root, icon, itemName, item);
		Rect rect;
		List<GameObject> list = MakeColumn(root, stats.Count, out rect);
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(compareRow);
			gameObject.transform.SetLayerRecursively(root);
			gameObject.transform.parent = list[i].transform;
			gameObject.transform.localPosition = new Vector3(20f, -10f, -50f);
			gameObject.transform.GetComponent<CompareRow>().Set(stats[i].Key, stats[i].Value);
		}
		return rect;
	}

	private Rect MakeSecondCol(Transform root, List<KeyValuePair<string, int>> stats, List<int> deltas, Texture2D icon, string itemName, ServerData.Item item)
	{
		MakeIconAndTitle(root, icon, itemName, item);
		Rect rect;
		List<GameObject> list = MakeColumn(root, stats.Count, out rect);
		for (int i = 0; i < list.Count; i++)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(compareArrowRow);
			gameObject.transform.SetLayerRecursively(root);
			gameObject.transform.parent = list[i].transform;
			gameObject.transform.localPosition = new Vector3(20f, -10f, -50f);
			gameObject.transform.GetComponent<CompareRow>().Set(stats[i].Key, stats[i].Value);
			gameObject.transform.GetComponent<VertArrow>().Compare(deltas[i]);
		}
		return rect;
	}

	private List<GameObject> MakeColumn(Transform root, int rows, out Rect rect)
	{
		List<GameObject> list = new List<GameObject>();
		KeyValuePair<GameObject, Rect> lineBg = panelLightTop.GetLineBg(root, colWidth, 0);
		Rect linesBg = panelLight.GetLinesBg(3, root, colWidth, lineBg.Value.height.RoundToInt());
		Rect rect2 = linesBg;
		for (int i = 0; i < rows; i++)
		{
			if (i % 2 == 0)
			{
				KeyValuePair<GameObject, Rect> lineBg2 = panelDark.GetLineBg(root, colWidth, rect2.height.RoundToInt());
				rect2 = lineBg2.Value;
				list.Add(lineBg2.Key);
			}
			else
			{
				KeyValuePair<GameObject, Rect> lineBg3 = panelLight.GetLineBg(root, colWidth, rect2.height.RoundToInt());
				rect2 = lineBg3.Value;
				list.Add(lineBg3.Key);
			}
		}
		rect2 = ((rows % 2 != 0) ? panelDarkBottom.GetLineBg(root, colWidth, rect2.height.RoundToInt()).Value : panelLightBottom.GetLineBg(root, colWidth, rect2.height.RoundToInt()).Value);
		rect = rect2;
		return list;
	}

	private Rect MakeBg(Transform root, int w, int h)
	{
		KeyValuePair<GameObject, Rect> lineBg = popupTop.GetLineBg(root, w, 0);
		KeyValuePair<GameObject, Rect> lineBg2 = popupBottom.GetLineBg(root, w, 0);
		int num = lineBg.Value.height.RoundToInt();
		int num2 = lineBg2.Value.height.RoundToInt();
		int num3 = ((float)(h - num2 - num) / (float)rowHeight).CeilToInt();
		popupMiddle.GetLinesBg(num3, root, w, num);
		lineBg2.Key.transform.localPosition = new Vector3(0f, -num - num3 * rowHeight, 0f);
		return new Rect(0f, 0f, lineBg.Value.width, num + num2 + num3 * rowHeight);
	}
}
