using System;
using System.Collections.Generic;
using XRL;
using XRL.World.Parts;

namespace Qud.API;

public static class AbilityAPI
{
	public static int abilityCount
	{
		get
		{
			if (The.Player == null)
			{
				return 0;
			}
			return The.Player.GetPart<ActivatedAbilities>()?.AbilityByGuid.Keys.Count ?? 0;
		}
	}

	public static ActivatedAbilityEntry GetAbility(int nAbility)
	{
		if (The.Player == null)
		{
			return null;
		}
		ActivatedAbilities part = The.Player.GetPart<ActivatedAbilities>();
		if (part == null)
		{
			return null;
		}
		int num = 0;
		foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in part.AbilityByGuid)
		{
			if (num == nAbility)
			{
				return item.Value;
			}
			num++;
		}
		return null;
	}
}
