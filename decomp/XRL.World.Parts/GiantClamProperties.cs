using System.Linq;
using Qud.API;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

public class GiantClamProperties : IPart
{
	public ClamSystem ClamSystem => The.Game.RequireSystem(() => new ClamSystem());

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetNavigationWeightEvent.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == ObjectStoppedFlyingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart)
		{
			E.MinWeight(95);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (CheckTeleport(E.Object, E.Cell))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectStoppedFlyingEvent E)
	{
		if (CheckTeleport(E.Object, E.Cell))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool CheckTeleport(GameObject Object, Cell C = null)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (!IsValidTarget(Object))
		{
			return false;
		}
		if (C == null)
		{
			C = Object.GetCurrentCell();
		}
		if (C.ParentZone.Built)
		{
			if (Object.IsPlayer() && !ParentObject.HasIntProperty("EnteredByPlayer"))
			{
				ParentObject.SetIntProperty("EnteredByPlayer", 1);
				Achievement.CLAMS_ENTERED_100.Progress.Increment();
				if (ParentObject.HasIntProperty("MakClam"))
				{
					Achievement.ENTER_MAK_CLAM.Unlock();
				}
			}
			if (ParentObject.CurrentZone.ZoneID.StartsWith("JoppaWorld."))
			{
				if (Object.IsPlayer() && ReadyForClamWorld(Object))
				{
					TeleportToClamWorld(Object);
				}
				else
				{
					TeleportJoppaWorld(Object);
				}
			}
			else
			{
				TeleportFromClamWorld(Object);
			}
		}
		return true;
	}

	public bool IsValidTarget(GameObject Object)
	{
		if (Object != null && Object != ParentObject && Object.IsReal && Object.Brain != null && Object.GetLongProperty("ClamTeleportTurn", 0L) != The.Game.Turns && ParentObject.PhaseAndFlightMatches(Object))
		{
			return !Object.GetBlueprint().DescendsFrom("Giant Clam");
		}
		return false;
	}

	public static bool Teleport(GameObject Object, Cell TargetCell, char Color)
	{
		Object.SetLongProperty("ClamTeleportTurn", The.Game.Turns);
		string text = "&" + char.ToUpper(Color);
		string text2 = "&" + char.ToLower(Color);
		bool result = Object.TeleportTo(TargetCell, 0, ignoreCombat: true, ignoreGravity: false, forced: false, UsePopups: false, "disappear", null);
		if (Object.IsPlayer())
		{
			Object.TeleportSwirl(null, text);
		}
		else
		{
			Object.SmallTeleportSwirl(null, text);
		}
		Object.Physics.PlayWorldSound("Sounds/Foley/fly_tileMove_water_wade");
		Object.Heartspray(text, text2, "&C", "&Y", 'ù');
		Object.Heartspray(text, text2, "&C", text2, 'ú');
		Object.Heartspray(text, text2, "&C", "&Y", 'ø');
		Object.MakeActive();
		return result;
	}

	public GameObject GetLinkedClam(Zone Z)
	{
		if (ParentObject.TryGetIntProperty("ClamId", out var id))
		{
			GameObject gameObject = Z.FindObject((GameObject o) => o.GetIntProperty("ClamId", -1) == id);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return Z.GetObjects("Giant Clam").GetRandomElement();
	}

	public void TeleportToClamWorld(GameObject Object)
	{
		bool flag = The.ZoneManager.IsZoneBuilt(ClamSystem.ClamWorldId);
		Zone clamZone = ClamSystem.GetClamZone();
		Cell cell = GetLinkedClam(clamZone)?.CurrentCell;
		if (cell == null)
		{
			IComponent<GameObject>.AddPlayerMessage("You hear a shloop and then a hitch. Nothing happens.");
			if (clamZone.CountObjects((GameObject x) => x.IsReal) == 0)
			{
				MetricsManager.LogError("Tzimtzlum empty, zone " + (flag ? "was" : "was not") + " built previously.");
				if (Popup.ShowYesNo("This zone didn't build properly, do you wish to rebuild it?") == DialogResult.Yes)
				{
					The.ZoneManager.SuspendZone(clamZone);
					The.ZoneManager.DeleteZone(clamZone);
					TeleportToClamWorld(Object);
				}
			}
			return;
		}
		Object.Physics.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_spacetimeWeirdness", 1f);
		SoundManager.PlayMusic("Music/Clam Dimension", "music", Crossfade: true, 20f);
		Popup.Show("You hear a shloop, and the world around you warps and shifts violently.");
		Popup.Show("One dram of {{neutronic|neutron}} flux evaporates from your inventory.");
		Popup.Show("In the midst of your disorientation, you find a passageway to another dimension.");
		Object.UseDrams(1, "neutronflux");
		Teleport(Object, cell, 'O');
		if (!The.Game.HasBooleanGameState("VisitedTzimtzlum"))
		{
			The.Game.SetBooleanGameState("VisitedTzimtzlum", Value: true);
			JournalAPI.AddAccomplishment("While dimensionally lost, you discovered a passageway to the pocket dimension Tzimtzlum and traveled there.", "Via the power of an imaginative mind at cosmological scales, =name= created the pocket dimension Tzimtzlum and traveled there, immanentizing it for all astral wanderers.", "While lost in the warrens of space and time, =name= discovered the pocket dimension Tzimtzlum. There " + The.Player.GetPronounProvider().Subjective + " sat at the conjunction of leylines and befriended three hundred clams.", null, "general", MuralCategory.VisitsLocation, MuralWeight.High, null, -1L);
			Achievement.TRAVEL_TZIMTZLUM.Unlock();
			MetricsManager.SendTelemetry("game_event", "travel.clam_world", Object.DisplayNameOnlyDirect + ":" + Object.Level + ":" + The.Game.Turns);
		}
	}

	public void TeleportFromClamWorld(GameObject Object)
	{
		Zone joppaZone = ClamSystem.GetJoppaZone(ParentObject.GetIntProperty("ClamId"));
		Cell cell = GetLinkedClam(joppaZone)?.CurrentCell;
		if (cell == null)
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You hear a shloop and then a hitch. Nothing happens.");
			}
			return;
		}
		Teleport(Object, cell, 'B');
		if (Object.IsPlayer())
		{
			Popup.Show("You find a passageway back to your home dimension.");
		}
	}

	public void TeleportJoppaWorld(GameObject Object)
	{
		Zone joppaZone = ClamSystem.GetJoppaZone(ParentObject.CurrentZone.ZoneID);
		Cell cell = joppaZone.GetObjects("Giant Clam").GetRandomElement()?.CurrentCell;
		if (cell == null)
		{
			cell = joppaZone.GetEmptyCellsShuffled().FirstOrDefault();
			if (cell == null)
			{
				IComponent<GameObject>.AddPlayerMessage("You hear a shloop and then a hitch. Nothing happens.");
				return;
			}
			cell.AddObject("Giant Clam");
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You hear a shloop and the world around you shifts.");
		}
		Teleport(Object, cell, 'B');
	}

	public static bool ReadyForClamWorld(GameObject go)
	{
		if (!go.HasEffect<Lost>())
		{
			return false;
		}
		if (go.GetFreeDrams("neutronflux") < 1)
		{
			return false;
		}
		return true;
	}
}
