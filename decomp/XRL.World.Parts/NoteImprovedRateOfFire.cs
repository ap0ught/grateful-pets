using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class NoteImprovedRateOfFire : IPart
{
	public int ShotsAmount;

	public int AmmoAmount;

	public int AmmoCapacityAmount;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		List<string> list = new List<string>();
		if (ShotsAmount != 0)
		{
			list.Add(ShotsAmount.Signed() + " to " + ParentObject.its + " shots per action");
		}
		if (AmmoAmount != 0)
		{
			list.Add(AmmoAmount.Signed() + " to " + ParentObject.its + " ammo per action");
		}
		if (AmmoCapacityAmount != 0)
		{
			list.Add(AmmoCapacityAmount.Signed() + " to " + ParentObject.its + " ammo capacity");
		}
		if (list.Count > 0)
		{
			E.Postfix.AppendRules(ParentObject.IndicativeProximal + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("have") + " " + Grammar.MakeAndList(list) + ".");
		}
		return base.HandleEvent(E);
	}
}
