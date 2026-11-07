using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class OthoWander1 : IPart
{
	public long startTurn;

	public static bool begin()
	{
		The.Player.AddPart(new PlayerOthoWander1Safeguard());
		The.Player.GetPart<PlayerOthoWander1Safeguard>().startTurn = XRLCore.Core.Game.Turns;
		GameObject gameObject = The.Player.Physics.CurrentCell.ParentZone.FindObject("Otho");
		gameObject.SetIntProperty("AllowGlobalTraversal", 1);
		gameObject.AddPart<OthoWander1>().startTurn = XRLCore.Core.Game.Turns;
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID)
		{
			return ID == GetZoneSuspendabilityEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (The.Game.Turns - startTurn > 150 && ParentObject.CurrentCell != null && ParentObject.CurrentZone.Z == 13)
		{
			ParentObject.RemovePart(this);
			return base.HandleEvent(E);
		}
		if (The.Game.Turns - startTurn < 150)
		{
			ParentObject.Brain.MoveToGlobal("JoppaWorld.22.14.1.0.14", 18, 8);
		}
		else if (The.Game.Turns - startTurn > 150)
		{
			The.Game.SetIntGameState("OmonporchReady", 1);
			ParentObject.Brain.MoveToGlobal("JoppaWorld.22.14.1.0.13", 32, 21);
		}
		else
		{
			ParentObject.UseEnergy(1000);
		}
		return false;
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		E.Suspendability = Suspendability.Pinned;
		return false;
	}
}
