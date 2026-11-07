using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CauseBleedingWhenDestroyed : IPart
{
	public string Damage = "1";

	public int SaveTarget = 20;

	public bool Stack = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		(E.Object.Equipped ?? E.Object.Implantee)?.ApplyEffect(new Bleeding(Damage, SaveTarget, null, Stack));
		return base.HandleEvent(E);
	}
}
