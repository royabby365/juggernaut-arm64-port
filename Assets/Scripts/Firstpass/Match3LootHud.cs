using UnityEngine;

public class Match3LootHud : MonoBehaviour
{
	public Sprite Icon;

	public AltButton AltButton;

	public void SetLoot(ServerData.Bonus.DropElement loot)
	{
		Icon.SpriteName_ = GetLootImage(loot);
		switch (loot.Item.ElixirType)
		{
		case ServerData.Item.ElixirTypeE.Key:
			AltButton.HintCode = ServerData.HintCodesE.vzlom;
			break;
		case ServerData.Item.ElixirTypeE.Skull:
			AltButton.HintCode = ServerData.HintCodesE.sculls;
			break;
		case ServerData.Item.ElixirTypeE.Scarab:
			AltButton.HintCode = ServerData.HintCodesE.scarab;
			break;
		case ServerData.Item.ElixirTypeE.Gold:
			AltButton.HintCode = ServerData.HintCodesE.money;
			break;
		case ServerData.Item.ElixirTypeE.Diamond:
			AltButton.HintCode = ServerData.HintCodesE.cristals;
			break;
		case ServerData.Item.ElixirTypeE.Star:
			AltButton.HintCode = ServerData.HintCodesE.legendarypoints;
			break;
		default:
			AltButton.HintCode = ServerData.HintCodesE.none;
			break;
		}
	}

	public static string GetLootImage(ServerData.Bonus.DropElement loot)
	{
		string result = "gold_42x50";
		switch (loot.Item.ElixirType)
		{
		case ServerData.Item.ElixirTypeE.Key:
			result = "key_42x50";
			break;
		case ServerData.Item.ElixirTypeE.Skull:
			result = "skull_42x50";
			break;
		case ServerData.Item.ElixirTypeE.Scarab:
			result = "scarab_42x50";
			break;
		case ServerData.Item.ElixirTypeE.Gold:
			result = "gold_42x50";
			break;
		case ServerData.Item.ElixirTypeE.Diamond:
			result = "diamond_42x50";
			break;
		case ServerData.Item.ElixirTypeE.Star:
			result = "star";
			break;
		}
		return result;
	}
}
