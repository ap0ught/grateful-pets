using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CherubimLock : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeContentsTaken");
		Registrar.Register("AfterContentsTaken");
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDestroyObjectEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneThawedEvent.ID)
		{
			return ID == AfterZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		Obliterate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		Obliterate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterZoneBuiltEvent E)
	{
		Obliterate();
		return base.HandleEvent(E);
	}

	public void Obliterate()
	{
		if (The.Game.GetIntGameState("RobberChimesTriggered") == 1 && !ParentObject.HasIntProperty("Raised"))
		{
			ParentObject.Obliterate();
		}
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		Chime();
		return base.HandleEvent(E);
	}

	public void Chime()
	{
		if (The.Game.HasIntGameState("RobberChimesTriggered"))
		{
			return;
		}
		The.Game.SetIntGameState("RobberChimesTriggered", 1);
		Popup.Show("Robber chimes ring through the vault, and you hear the grating of stone on stone as the other imperial reliquaries slide into the unreachable recesses of the Tomb.");
		ParentObject.SetIntProperty("Raised", 1);
		ParentObject.GetPart<DoubleContainer>().GetSibling()?.SetIntProperty("Raised", 1);
		AIHelpBroadcastEvent.Send(ParentObject, The.Player, null, "Cherubim", 80, 10f, HelpCause.Theft);
		foreach (Zone value in The.ZoneManager.CachedZones.Values)
		{
			Zone.ObjectEnumerator enumerator2 = value.IterateObjects().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				GameObject current = enumerator2.Current;
				if (IsLocked(current))
				{
					current.Obliterate();
				}
			}
		}
	}

	private bool IsLocked(GameObject Object)
	{
		if (!Object.HasIntProperty("Raised"))
		{
			return Object.HasPart(typeof(CherubimLock));
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeContentsTaken")
		{
			if (ParentObject.CurrentCell.ParentZone.GetObjectsWithTagOrProperty("CherubimLock").Count > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Taker");
				if (gameObjectParameter != null && gameObjectParameter.IsPlayerControlled())
				{
					EmitMessage("The protective force of the cherubim prevents " + gameObjectParameter.t() + " from taking anything from the reliquary.", ' ', FromDialog: false, gameObjectParameter.IsPlayer());
				}
				return false;
			}
		}
		else if (E.ID == "AfterContentsTaken")
		{
			Chime();
			return true;
		}
		return base.FireEvent(E);
	}
}
