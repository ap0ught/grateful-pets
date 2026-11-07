using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class NoteImprovedAccuracy : IPart
{
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
		E.Postfix.AppendRules(ParentObject.IndicativeProximal + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("have") + " improved accuracy.");
		return base.HandleEvent(E);
	}
}
