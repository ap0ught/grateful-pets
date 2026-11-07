using System;
using System.Collections.Generic;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Hok : BaseInitiatorySkill
{
	public const int BONUS_MOD_CHANCE = 50;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetTinkeringBonusEvent.ID)
		{
			return ID == PooledEvent<ModifyBitCostEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ModifyBitCostEvent E)
	{
		char key = '\0';
		int num = -1;
		int num2 = 0;
		foreach (KeyValuePair<char, int> bit in E.Bits)
		{
			int bitTier = BitType.GetBitTier(bit.Key);
			if (num == -1 || bitTier >= num)
			{
				num = bitTier;
				key = bit.Key;
			}
			num2 += bit.Value;
		}
		if (num2 > 1)
		{
			E.Bits[key]--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "BonusMod")
		{
			E.Bonus += 50;
		}
		else if (E.Type == "Repair")
		{
			E.Bonus++;
		}
		return base.HandleEvent(E);
	}
}
