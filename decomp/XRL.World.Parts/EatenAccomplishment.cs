using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class EatenAccomplishment : IPart
{
	public string Text = "You ate it!";

	public bool Triggered;

	public string Hagiograph;

	public string HagiographCategory;

	public string HagiographWeight;

	public string Gospel;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == AfterInventoryActionEvent.ID)
			{
				return !Triggered;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AfterInventoryActionEvent E)
	{
		if (E.Command == "Eat" && !Triggered && E.Actor.IsPlayer())
		{
			JournalAPI.AddAccomplishment(Text.StartReplace().AddObject(ParentObject, "this").ToString(), Hagiograph.StartReplace().AddObject(ParentObject, "this").ForceThirdPerson()
				.ToString(), muralCategory: MuralCategoryHelpers.parseCategory(HagiographCategory), muralWeight: MuralCategoryHelpers.parseWeight(HagiographWeight), gospelText: Gospel.StartReplace().AddObject(ParentObject, "this").ForceThirdPerson()
				.ToString(), aggregateWith: null, category: "general", secretId: null, time: -1L);
			Triggered = true;
		}
		return base.HandleEvent(E);
	}
}
