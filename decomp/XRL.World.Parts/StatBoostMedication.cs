using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StatBoostMedication : IPart
{
	public string Stat = "Agility";

	public string TrueManBoost = "2d6";

	public string TrueManDuration = "3d20";

	public string TrueManPermanent = "0";

	public string MutantBoost = "1d6";

	public string MutantDuration = "1d20";

	public string MutantPermanent = "0";

	public string Message = "The tube hisses against your skin, and your blood burns!";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply" && E.Actor.Statistics.ContainsKey(Stat))
		{
			string text = "";
			string text2 = "";
			if (E.Actor.IsTrueKin())
			{
				text = TrueManBoost;
				text2 = TrueManDuration;
				E.Actor.Statistics[Stat].BaseValue += XRL.Rules.Stat.Roll(TrueManPermanent);
			}
			else
			{
				text = MutantBoost;
				text2 = MutantDuration;
				E.Actor.Statistics[Stat].BaseValue += XRL.Rules.Stat.Roll(MutantPermanent);
			}
			int amount = XRL.Rules.Stat.Roll(text);
			int duration = XRL.Rules.Stat.Roll(text2);
			E.Actor.ApplyEffect(new BoostStatistic(duration, Stat, amount));
			if (E.Actor.IsPlayer())
			{
				Popup.Show(Message);
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
