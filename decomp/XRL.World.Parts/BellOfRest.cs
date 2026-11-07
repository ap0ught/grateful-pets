using System;

namespace XRL.World.Parts;

[Serializable]
public class BellOfRest : IPart
{
	public long lastDamageTurn = -1L;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDestroyObjectEvent.ID;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("TookDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage" && Calendar.TotalTimeTicks != lastDamageTurn)
		{
			lastDamageTurn = Calendar.TotalTimeTicks;
			ParentObject.ParticleSpray("&c\r", "&C\u000e", "&B\r", "&b\u000e", 6);
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		The.Game.SetIntGameState("BellOfRestDestroyed", 1);
		ParentObject.ParticleSpray("&c\r", "&C\u000e", "&B\r", "&b\u000e", 6);
		SoundManager.PlaySound("sfx_bellOfRest_toll");
		return base.HandleEvent(E);
	}
}
