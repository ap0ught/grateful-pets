using System;
using Genkit;
using UnityEngine;
using XRL.UI;
using XRL.World.Conversations.Parts;

namespace XRL.World.Parts;

[Serializable]
public class ShevaStarshipControl : IPart
{
	public const int LAUNCH_DURATION = 200;

	public string Prompt = "LAUNCH";

	public int Timer = -1;

	public bool IsStarted => Timer > 0;

	public bool IsFinished => Timer == 0;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != GetInventoryActionsEvent.ID && ID != GetZoneSuspendabilityEvent.ID && ID != InventoryActionEvent.ID && ID != CanSmartUseEvent.ID)
		{
			return ID == CommandSmartUseEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Launch", "launch", "StarshipLaunch", null, 'c');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		if (Timer > 0)
		{
			E.Suspendability = Suspendability.Pinned;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "StarshipLaunch")
		{
			AttemptLaunch(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		AttemptLaunch(E.Actor);
		return false;
	}

	public bool AttemptLaunch(GameObject Actor)
	{
		if (IsStarted)
		{
			return Actor.ShowFailure("The spaceship's launch sequence has already begun.");
		}
		if (IsFinished)
		{
			return Actor.ShowFailure("The spaceship is already traversing the void.");
		}
		if (ParentObject.CurrentZone?.ResolveZoneWorld() != "NorthSheva")
		{
			return Actor.ShowFailure("The spaceship can't launch from here.");
		}
		if (Actor.IsPlayer())
		{
			string text = "";
			string text2 = null;
			if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
			{
				text = "(type " + Prompt + " to confirm)";
				text2 = "";
			}
			if (!Popup.AskString("Launch spaceship and end game?" + text, text2, "Sounds/UI/ui_notification", null, Prompt, Prompt.Length, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).EqualsNoCase(Prompt))
			{
				return false;
			}
		}
		StartLaunch();
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Timer > 0)
		{
			Timer = Math.Max(Timer - Amount, 0);
			CheckTimer();
			ParentObject.CurrentZone?.MarkActive();
		}
	}

	public void CheckTimer()
	{
		if (!(ParentObject.CurrentZone is InteriorZone interiorZone))
		{
			Timer = -1;
			return;
		}
		if (Timer > 10)
		{
			interiorZone.SetZoneProperty("ambient_bed_2", "sfx_endgame_spaceship_engine_started_lp");
		}
		else if (Timer == 10)
		{
			interiorZone.SetZoneProperty("ambient_bed_2", "sfx_endgame_spaceship_engine_preTakeOff_lp");
		}
		else if (Timer == 0)
		{
			interiorZone.RemoveZoneProperty("ambient_bed_2");
		}
		if (interiorZone.IsActive())
		{
			AmbientSoundsSystem.PlayAmbientBeds(interiorZone);
			if (Timer == 200)
			{
				EmitLaunchPopup("Exodus launch sequence initiated!");
				return;
			}
			if (Timer == 10)
			{
				EmitLaunchPopup("Exodus launch in 10...");
				return;
			}
			if (Timer >= 1 && Timer <= 9)
			{
				EmitMessage($"Exodus launch in {Timer}...", ' ', FromDialog: false, UsePopup: false, AlwaysVisible: true);
				return;
			}
			if (Timer == 0)
			{
				LaunchActiveEnd();
				return;
			}
			if (Timer <= 100 || Timer % 50 != 0)
			{
				int timer = Timer;
				if (timer <= 10 || timer > 100 || Timer % 25 != 0)
				{
					return;
				}
			}
			EmitLaunchPopup($"Launching in {Timer}...");
		}
		else if (Timer == 0)
		{
			The.Game.SetInt64GameState("StarshipLaunched", The.Game.Turns);
			if (interiorZone.HasObject("Barathrum"))
			{
				The.Game.TryAddDelimitedGameState("Barathrum", ',', "Launched");
				The.Game.TryAddDelimitedGameState("EndExtra", ',', "Starshiib");
			}
			GameObject parentObject = interiorZone.ParentObject;
			int num = parentObject.DistanceTo(The.Player);
			if (num < 200)
			{
				Cell cell = The.Player.CurrentCell;
				Point2D cell2 = cell.Pos2D + parentObject.CurrentCell.PathDifferenceTo(cell);
				int distance = Math.Clamp(10 + num / 3, 15, 25);
				SoundManager.PlayWorldSound("sfx_endgame_spaceship_takeOff", distance, Occluded: true, 1f, cell2);
				Popup.Show("The moors rattle and a marble brick crumbles as a starship takes off.");
			}
			parentObject.AddPart(new InteriorBlockEntrance
			{
				Message = "There is no starship to enter. The docking bay is empty."
			});
		}
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (Timer > 0)
		{
			CameraShake.shakeDuration = Math.Max(CameraShake.shakeDuration, 3f);
			float b = ((Timer < 10) ? 1.5f : 0.75f);
			float val = Mathf.Lerp(0.05f, b, (float)(200 - Timer) / 200f);
			CameraShake.currentShakeAmount = Math.Max(CameraShake.currentShakeAmount, val);
		}
		return base.FinalRender(E, bAlt);
	}

	public void EmitLaunchPopup(string Message)
	{
		EmitMessage(Message, 'R', FromDialog: false, UsePopup: true, AlwaysVisible: true);
	}

	public void StartLaunch()
	{
		Timer = 200;
		CheckTimer();
	}

	public void LaunchActiveEnd()
	{
		SoundManager.PlayUISound("sfx_endgame_spaceship_takeOff");
		The.Game.SetStringGameState("EndType", "Launch");
		if (NephalProperties.IsFoiled("Ehalcodon"))
		{
			The.Game.SetStringGameState("EndGrade", "Super");
		}
		if (ParentObject.CurrentZone.HasObject("Barathrum"))
		{
			The.Game.TryAddDelimitedGameState("EndExtra", ',', "Starshiib");
			The.Game.TryAddDelimitedGameState("EndExtra", ',', "WithBarathrum");
		}
		EndGame.Start();
	}
}
