using System;
using XRL.Core;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Metamorphed : Effect
{
	public Guid AAID;

	public int XPAwarded;

	public GameObject OriginalBody;

	public Metamorphed()
	{
		DisplayName = "metamorphed";
	}

	public Metamorphed(GameObject OriginalBody, int Duration)
		: this()
	{
		this.OriginalBody = OriginalBody;
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 1024;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AwardedXPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardedXPEvent E)
	{
		XPAwarded += E.Amount;
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Assuming another creature's form.";
	}

	public override bool Apply(GameObject Object)
	{
		AAID = AddMyActivatedAbility("End Metamorphosis", "CommandEndMetamorphosis", "Physical Mutations", null, "\u0002");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		RemoveMyActivatedAbility(ref AAID);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandEndMetamorphosis");
		Registrar.Register("EndAction");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandEndMetamorphosis")
		{
			Cell cell = Object.Physics.CurrentCell;
			cell.RemoveObject(Object);
			cell.AddObject(OriginalBody);
			XRLCore.Core.Game.Player.Body = OriginalBody;
			Metamorphosis.TransferInventory(Object, OriginalBody, bTagLastEquipped: false);
			Object.MakeInactive();
			OriginalBody.MakeActive();
			OriginalBody.AwardXP(XPAwarded);
			IComponent<GameObject>.XDidY(OriginalBody, "revert", "to " + OriginalBody.its + " original form");
		}
		else if (E.ID == "EndAction" && Duration <= 0)
		{
			Object.FireEvent("CommandEndMetamorphosis");
		}
		return base.FireEvent(E);
	}
}
