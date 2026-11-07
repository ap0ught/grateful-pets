using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class NoteReducedWeight : IPart
{
	public int Amount;

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
		E.Postfix.AppendRules(ParentObject.IndicativeProximal + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("weigh") + " " + Grammar.Cardinal(Amount) + " " + ((Amount == 1) ? "pound" : "pounds") + " less than usual.");
		return base.HandleEvent(E);
	}
}
