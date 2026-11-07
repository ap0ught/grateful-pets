using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ModCounterweighted : IMeleeModification
{
	public ModCounterweighted()
	{
	}

	public ModCounterweighted(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "BalanceSystems";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModCounterweighted).Tier != Tier)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public static int GetModificationLevel(int Tier)
	{
		return (int)Math.Ceiling((float)(Tier + 1) / 3f);
	}

	public int GetModificationLevel()
	{
		return GetModificationLevel(Tier);
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MeleeWeapon>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<MeleeWeapon>().HitBonus += GetModificationLevel();
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			int modificationLevel = GetModificationLevel();
			stringBuilder.Append("counterweighted");
			if (modificationLevel > 1)
			{
				stringBuilder.Append('(').Append(modificationLevel).Append(')');
			}
			E.AddAdjective(stringBuilder.ToString());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Counterweighted: Adds " + ((Tier > 0) ? GetModificationLevel(Tier).Signed() : "a bonus") + " to hit.";
	}
}
