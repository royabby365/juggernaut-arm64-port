using UnityEngine;

public class ChapterBonusHud : MonoBehaviour
{
	private GameObject _drop;

	public SpriteText Header;

	public Transform BonusFrame;

	public GameObject LootProto;

	public MeshRenderer BackgroundRenderer;

	private void Start()
	{
	}

	private void OnEnable()
	{
		base.transform.GetSpriteGui().Release += Gui_Release;
	}

	private void OnDisable()
	{
		base.transform.GetSpriteGui().Release -= Gui_Release;
	}

	private void Gui_Release(SpriteButton obj)
	{
		if (!(HudMk1.Instance == null) && HudMk1.Instance.CurrentGui.Type == GuiRoot.GuiType.ChapterInfo && obj.name == "button_chapter_continue")
		{
			Globals.MainMenu.OnMsgGuiExitAchievmentOrExtraChapter();
		}
	}

	public void Init(ActionD onLoad)
	{
		if (_drop != null)
		{
			Object.Destroy(_drop);
		}
		if (Header != null)
		{
			Header.Text_ = AreaData.Current.Location.Title;
		}
		string textureName = string.Empty + AreaData.Current.Location.Id;
		string text = "resources/chapter_screens/" + textureName;
		SingletonT<ResourcesManager>.I.GetAssetBundleAsync(Globals.MainMenu, ResourcesManager.GetAssetBundlePath(text), delegate(string _, ResourcesManager.AssetBundleData ab, float time)
		{
			BackgroundRenderer.material.mainTexture = (Texture)ab.Bundle.LoadAsset(textureName);
			if (AreaData.Current.Location.Bonus.Drop != null && AreaData.Current.Location.Bonus.Drop.Count > 0)
			{
				_drop = (GameObject)Object.Instantiate(LootProto);
				_drop.transform.parent = BonusFrame.transform;
				_drop.transform.localPosition = new Vector3(-84f, 84f, -50f);
				ExtraItemPreview componentInChildren = _drop.GetComponentInChildren<ExtraItemPreview>();
				componentInChildren.SetLoot(AreaData.Current.Location.Bonus.Drop[0]);
			}
			SingletonT<ResourcesManager>.I.RemoveAssetBundleNoActions(ab);
			onLoad();
		}, delegate(string _, string errorMessage)
		{
			Utils.LogForce("ChapterBonusHud.Init", errorMessage);
			onLoad();
		});
	}
}
