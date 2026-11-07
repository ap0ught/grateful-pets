using System;

namespace XRL.World.Parts;

[Serializable]
public class CryptWiggleChest : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

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
		BroadcastHelp();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanSmartUse");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			CombatJuice.playPrefabAnimation(ParentObject, "Abilities/AbilityVFXWiggle", ParentObject.ID, ParentObject.Render.Tile + ";" + ParentObject.Render.GetTileForegroundColor() + ";" + ParentObject.Render.getDetailColor());
			BroadcastHelp();
			return false;
		}
		return base.FireEvent(E);
	}

	public void BroadcastHelp()
	{
		if (ParentObject.IsNowhere())
		{
			return;
		}
		foreach (GameObject item in ParentObject.CurrentCell.FastFloodVisibility("CryptSitterBehavior", 10))
		{
			item.FireEvent("AICryptHelpBroadcast");
		}
	}
}
