using System;
using XRL.World.Parts;

namespace XRL.World.Units;

[Serializable]
public class GameObjectBaetylUnit : GameObjectUnit
{
	public class RewardOnPlace : IPart
	{
		public int Amount;

		public int Tier;

		public override bool WantEvent(int ID, int cascade)
		{
			return ID == EnteredCellEvent.ID;
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			GiveRewards(ParentObject, Amount, Tier);
			ParentObject.RemovePart(this);
			return base.HandleEvent(E);
		}
	}

	public string Amount;

	public string Tier;

	public bool Delay;

	public override void Apply(GameObject Object)
	{
		int amount = Amount.RollCached();
		int tier = Tier.RollCached();
		if (Delay)
		{
			Object.AddPart(new RewardOnPlace
			{
				Amount = amount,
				Tier = tier
			});
		}
		else
		{
			GiveRewards(Object, amount, tier);
		}
	}

	public static void GiveRewards(GameObject Object, int Amount, int Tier)
	{
		for (int i = 0; i < Amount; i++)
		{
			RandomAltarBaetyl randomAltarBaetyl = new RandomAltarBaetyl();
			randomAltarBaetyl.RewardTier = Tier;
			randomAltarBaetyl.GenerateDemand();
			randomAltarBaetyl.GenerateReward();
			if (!randomAltarBaetyl.RewardID.IsNullOrEmpty())
			{
				The.ZoneManager.peekCachedObject(randomAltarBaetyl.RewardID)?.ApplyModification("ModGigantic");
			}
			string text = randomAltarBaetyl.GiveReward(Object, IgnorePlayerControl: true, SkipItemMessage: true, SkipAttributePointsMessage: false, SkipMutationPointsMessage: false, SkipSkillPointsMessage: false, SkipExperiencePointsMessage: true, SkipLicensePointsMessage: false, SkipReputationMessage: true);
			if (!text.IsNullOrEmpty())
			{
				Object.Physics.DidX("gain", text);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Amount = null;
		Tier = null;
	}

	public override string GetDescription(bool Inscription = false)
	{
		return "Spawns with " + Amount + " random baetyl reward" + ((Amount == "1") ? "" : "s");
	}
}
