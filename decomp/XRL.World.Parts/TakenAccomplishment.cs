using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class TakenAccomplishment : IPart
{
	public string Text = "You got it!";

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
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Trigger(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		Trigger(E.Actor);
		return base.HandleEvent(E);
	}

	public void Trigger(GameObject who)
	{
		if (!Triggered && who != null && who.IsPlayer())
		{
			JournalAPI.AddAccomplishment(Text.StartReplace().AddObject(ParentObject, "this").ToString(), Hagiograph.StartReplace().AddObject(ParentObject, "this").ForceThirdPerson()
				.ToString(), muralCategory: MuralCategoryHelpers.parseCategory(HagiographCategory), muralWeight: MuralCategoryHelpers.parseWeight(HagiographWeight), gospelText: Gospel.StartReplace().AddObject(ParentObject, "this").ForceThirdPerson()
				.ToString(), aggregateWith: null, category: "general", secretId: null, time: -1L);
			Triggered = true;
		}
	}
}
