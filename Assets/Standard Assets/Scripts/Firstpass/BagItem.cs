using System;
using System.Collections.Generic;

[Serializable]
public class BagItem
{
	public int SellPrice = 100;

	public ServerData.MoneyType.TypeE SellCurrency = ServerData.MoneyType.TypeE.Gold;

	public int BuyPrice = 100;

	public ServerData.MoneyType.TypeE BuyCurrency = ServerData.MoneyType.TypeE.Gold;

	public int Life;

	public int Strength;

	public int Anger;

	public int Vamp;

	public int Mana;

	public string[] Description;

	internal ServerData.ShopGood ServerShopItem;

	public string Name => (ServerShopItem == null) ? string.Empty : ServerShopItem.Item.TitleString;

	public BagItem(ServerData.ShopGood data)
	{
		Invs.Inv(data.Item != null, data.Item != null);
		ServerShopItem = data;
		foreach (KeyValuePair<ServerData.MoneyType, int> item in (data.Price.Count <= 0) ? data.Item.SellPrice : data.Price)
		{
			SellCurrency = item.Key.Type;
			SellPrice = item.Value;
		}
		foreach (KeyValuePair<ServerData.MoneyType, int> item2 in (data.Price.Count <= 0) ? data.Item.SellPrice : data.Price)
		{
			BuyCurrency = item2.Key.Type;
			BuyPrice = item2.Value;
		}
		Life = data.Item.GetSkill(ServerData.Skill.TypeE.Vitality, 0);
		Strength = data.Item.GetSkill(ServerData.Skill.TypeE.Strength, 0);
		Anger = data.Item.GetSkill(ServerData.Skill.TypeE.Rage, 0);
		if (data.Item.ElixirType == ServerData.Item.ElixirTypeE.None)
		{
			string[] array = new string[(data.Item.Skills != null) ? Math.Min(3, data.Item.Skills.Length) : 0];
			for (int i = 0; i < array.Length; i++)
			{
				ServerData.SkillInfo skillInfo = data.Item.Skills[i];
				array[i] = string.Empty;
				if (skillInfo != null && skillInfo.Skill != null)
				{
					array[i] = skillInfo.Skill.Title + " " + skillInfo.Current;
				}
			}
			Description = array;
		}
		else
		{
			Description = new string[1] { SingletonT<ServerData>.I.GetPhrase(ServerData.PhraseShopItemsCount) + " " + data.Count };
		}
	}
}
