using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Grabber : IPart
{
	public int Chance = 15;

	public int StuckDuration = 10;

	public int SaveTarget = 20;

	public string StuckAdjective = "grabbed";

	public string StuckPreposition = "by";

	public string StuckSaveVs = "Grab Stuck Restraint";

	public string CheckEvent = "BeforeGrabbed";

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool SameAs(IPart Part)
	{
		Grabber grabber = Part as Grabber;
		if (grabber.Chance != Chance)
		{
			return false;
		}
		if (grabber.StuckDuration != StuckDuration)
		{
			return false;
		}
		if (grabber.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (grabber.StuckAdjective != StuckAdjective)
		{
			return false;
		}
		if (grabber.StuckPreposition != StuckPreposition)
		{
			return false;
		}
		if (grabber.StuckSaveVs != StuckSaveVs)
		{
			return false;
		}
		if (grabber.CheckEvent != CheckEvent)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" && ParentObject.hitpoints > 0 && ParentObject.Equipped?.CurrentCell != null && Chance.in100())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (CheckEvent.IsNullOrEmpty() || gameObjectParameter.FireEvent(CheckEvent))
			{
				Stuck e = new Stuck(StuckDuration, SaveTarget, StuckSaveVs, null, StuckAdjective, StuckPreposition, ParentObject.Equipped.ID);
				gameObjectParameter.ApplyEffect(e);
			}
		}
		return base.FireEvent(E);
	}
}
