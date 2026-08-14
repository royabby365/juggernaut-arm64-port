internal static class EnumsExt
{
	internal static bool HasFlag(this ReactE react, ReactE v)
	{
		return (react & v) != 0;
	}

	internal static bool IsMagic(this DamageTypeE type)
	{
		return type == DamageTypeE.LightingMagic || type == DamageTypeE.IceMagic || type == DamageTypeE.FireMagic || type == DamageTypeE.DarkMagic;
	}

	internal static MagicTypeE AsMagic(this DamageTypeE type)
	{
		return type switch
		{
			DamageTypeE.DarkMagic => MagicTypeE.Darkness, 
			DamageTypeE.FireMagic => MagicTypeE.Fire, 
			DamageTypeE.IceMagic => MagicTypeE.Ice, 
			DamageTypeE.LightingMagic => MagicTypeE.Lighting, 
			_ => MagicTypeE.None, 
		};
	}

	internal static string ReactionScenario(this ReactE react, string direction)
	{
		if (direction == string.Empty || direction == "0")
		{
			direction = "f";
		}
		string result = null;
		if (react.HasFlag(ReactE.Block))
		{
			result = Globals.ReactBlock;
		}
		else if (react.HasFlag(ReactE.Dodge))
		{
			result = Globals.ReactDodge;
		}
		else if (react.HasFlag(ReactE.Death))
		{
			result = ((!react.HasFlag(ReactE.Critical)) ? Globals.ReactDeath : (Globals.ReactDeath + direction));
		}
		else if (react.HasFlag(ReactE.Damage))
		{
			result = ((!react.HasFlag(ReactE.Critical)) ? Globals.ReactDamage : (Globals.ReactDamage + direction));
		}
		else if (react.HasFlag(ReactE.Heal))
		{
			result = Globals.ReactHeal;
		}
		return result;
	}
}
