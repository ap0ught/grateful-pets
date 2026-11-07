using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AnchorRoomTile : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (GameObject.Validate(E.Object) && E.Object.HasMarkOfDeath() && !E.Object.HasEffect<CorpseTethered>())
		{
			E.Object.ApplyEffect(new CorpseTethered());
			if (E.Object.IsPlayer())
			{
				E.Object.ParticleText("Tomb-tethered!", 'G');
			}
		}
		return base.HandleEvent(E);
	}
}
