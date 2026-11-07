using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.Wish;
using XRL.World;

namespace XRL;

[Serializable]
[HasWishCommand]
public class CheckpointingSystem : IGameSystem
{
	public class SaveCommand : ISystemSaveCommand, IActionCommand, IComposite
	{
		private static SaveCommand Instance = new SaveCommand();

		public string Type => "Checkpoint";

		public static void Issue()
		{
			ActionManager actionManager = The.ActionManager;
			actionManager.DequeueActionsDescendedFrom<ISystemSaveCommand>();
			actionManager.EnqueueAction(Instance);
		}

		public void Execute(XRLGame Game, ActionManager Manager)
		{
			Game.Checkpoint();
		}
	}

	public class LoadCommand : IActionCommand, IComposite
	{
		private static LoadCommand Instance = new LoadCommand();

		public static void Issue()
		{
			XRLGame game = The.Game;
			if (game == null || !game.Running)
			{
				return;
			}
			ActionManager actionManager = game.ActionManager;
			if (!actionManager.HasAction(typeof(LoadCommand)))
			{
				if (actionManager.IsPlayerTurn())
				{
					actionManager.SkipPlayerTurn = true;
				}
				actionManager.SkipSegment = true;
				actionManager.DequeueActionsDescendedFrom<ISystemSaveCommand>();
				actionManager.EnqueueAction(Instance);
			}
		}

		public void Execute(XRLGame Game, ActionManager Manager)
		{
			XRLGame xRLGame = XRLGame.LoadCurrentGame("Checkpoint");
			if (xRLGame != null)
			{
				Task task = xRLGame.SaveCopy("Checkpoint", "Primary");
				Task task2 = xRLGame.CacheCopy("CheckpointCache", "Cache", Reset: true);
				task.Wait();
				task2.Wait();
			}
		}
	}

	[NonSerialized]
	public static Dictionary<string, Renderable> deathIcons = new Dictionary<string, Renderable> { 
	{
		"lased to death",
		new Renderable(null, "Mutations/flaming_ray.bmp", "f", "&W", "&W", 'R')
	} };

	public string lastZone;

	public string lastCheckpointKey;

	public static bool ShowDeathMessage(string message, string deathCategory = null)
	{
		Debug.Log("deathCategory: " + deathCategory);
		if (!IsCheckpointingEnabled())
		{
			if (!string.IsNullOrEmpty(deathCategory) && deathIcons.ContainsKey(deathCategory))
			{
				string title = "";
				if (message.Contains("."))
				{
					title = message.Substring(0, message.IndexOf('.') + 1);
					message = message.Substring(message.IndexOf('.') + 2);
				}
				Popup.ShowSpace(message.Replace("You died.", ""), title, "Sounds/UI/ui_notification_death", deathIcons[deathCategory], LogMessage: true, ShowContextFrame: false, "DeathMessage");
			}
			else
			{
				Popup.ShowSpace(message, null, "Sounds/UI/ui_notification_death", null, LogMessage: true, ShowContextFrame: true, "DeathMessage");
			}
			return false;
		}
		if (Options.AllowReallydie && Popup.ShowYesNo("DEBUG: Do you really want to die?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) != DialogResult.Yes)
		{
			The.Player.RestorePristineHealth();
			return true;
		}
		while (true)
		{
			bool flag = Options.GetOption("OptionDefaultDeathToViewFinalMessages") == "Yes";
			int num = Popup.PickOption("", message + "\n", "", "Sounds/UI/ui_notification", (!flag) ? new string[4] { "Reload from checkpoint", "View final messages", "Retire character", "Quit to main menu" } : new string[4] { "View final messages", "Reload from checkpoint", "Retire character", "Quit to main menu" });
			if (num == (flag ? 1 : 0))
			{
				break;
			}
			if (num == ((!flag) ? 1 : 0))
			{
				XRLCore.Core.Game.Player.Messages.Show();
				continue;
			}
			switch (num)
			{
			case 2:
			{
				string text = " Type 'RETIRE' to confirm.";
				string text2 = string.Empty;
				if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console)
				{
					text = string.Empty;
					text2 = null;
				}
				string text3 = Popup.AskString("If you retire this character, your score will be recorded and your character will be lost. Are you sure you want to RETIRE THIS CHARACTER FOREVER?" + text, text2, "Sounds/UI/ui_notification", null, "RETIRE", 7, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false);
				if (text3 != null && text3.ToUpper() == "RETIRE")
				{
					return false;
				}
				break;
			}
			case 3:
				The.Game.DeathReason = "<nodeath>";
				The.Game.DeathCategory = "exit";
				The.Game.forceNoDeath = true;
				The.Game.Running = false;
				return true;
			default:
				return true;
			}
		}
		QueueRestore();
		return true;
	}

	public static bool IsPlayerInCheckpoint()
	{
		if (The.Player == null)
		{
			return false;
		}
		return The.Player?.CurrentZone?.IsCheckpoint() == true;
	}

	public static bool IsCheckpointingEnabled()
	{
		XRLGame game = The.Game;
		if (game.GetStringGameState("Checkpointing") != "Enabled")
		{
			string stringGameState = game.GetStringGameState("GameMode");
			if (stringGameState != "Wander" && stringGameState != "Roleplay")
			{
				return false;
			}
		}
		return true;
	}

	[WishCommand(null, null)]
	public static bool CheckpointOn()
	{
		The.Game.SetStringGameState("Checkpointing", "Enabled");
		MessageQueue.AddPlayerMessage("Checkpointing enabled");
		return true;
	}

	[WishCommand(null, null)]
	public static bool CheckpointOff()
	{
		The.Game.RemoveStringGameState("Checkpointing");
		string stringGameState = The.Game.GetStringGameState("GameMode");
		if (stringGameState == "Wander" || stringGameState == "Roleplay")
		{
			The.Game.SetStringGameState("GameMode", stringGameState + "NoCheckpoint");
		}
		return true;
	}

	public static void DoCheckpoint()
	{
		The.Game.Checkpoint();
	}

	public static void ManualCheckpoint(Zone Z, string Key)
	{
		if (!IsCheckpointingEnabled() || !The.Game.Running)
		{
			return;
		}
		CheckpointingSystem system = The.Game.GetSystem<CheckpointingSystem>();
		if (system != null)
		{
			if (Z.ZoneID == system.lastZone || Key == system.lastCheckpointKey)
			{
				return;
			}
			system.lastZone = Z.ZoneID;
			system.lastCheckpointKey = Key;
		}
		QueueCheckpoint();
	}

	public static void ManualCheckpoint()
	{
		if (IsCheckpointingEnabled() && The.Game.Running)
		{
			QueueCheckpoint();
		}
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!IsCheckpointingEnabled() || !Game.Running)
		{
			return base.HandleEvent(E);
		}
		if (E.Zone.ZoneID == lastZone)
		{
			return base.HandleEvent(E);
		}
		string checkpointKey = E.Zone.GetCheckpointKey();
		if ((checkpointKey != null || lastCheckpointKey != null) && checkpointKey != lastCheckpointKey)
		{
			QueueCheckpoint();
		}
		lastZone = E.Zone.ZoneID;
		lastCheckpointKey = checkpointKey;
		return base.HandleEvent(E);
	}

	public static void QueueCheckpoint()
	{
		SaveCommand.Issue();
	}

	public static void QueueRestore()
	{
		LoadCommand.Issue();
	}
}
