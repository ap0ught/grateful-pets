using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class NoteExpandedSlots : IPart
{
	public string Slots;

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
		if (!Slots.IsNullOrEmpty())
		{
			E.Postfix.AppendRules(ParentObject.IndicativeProximal + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("are") + ", unusually, able to function in " + Grammar.A(Grammar.MakeOrList(Slots.CachedCommaExpansion())).ToLower() + ".");
		}
		return base.HandleEvent(E);
	}

	public void AddSlot(string Slot)
	{
		Slots = Slots.AddDelimitedSubstring(',', Slot);
	}
}
