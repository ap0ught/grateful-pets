using ConsoleLib.Console;
using XRL.Core;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.ZoneParts;

namespace XRL.UI;

public class GritGateTerminalScreenRoot : GritGateTerminalScreen
{
	public bool ActionTaken;

	public GritGateTerminalScreenRoot()
	{
		MainText = "Sensor access granted. The Thin World is open to you. What do you wish?";
		ClearOptions();
	}

	public override void BeforeRender(ScreenBuffer buffer, ref string footerText)
	{
		if (Options.Count == 0)
		{
			if (!XRLCore.Core.Game.HasGameState("GritGateAttackScanned"))
			{
				Options.Add("Scan surroundings, Ereshkigal.");
				optionActions.Add(() =>
				{
					XRLCore.Core.Game.SetIntGameState("GritGateAttackScanned", 1);
					Terminal.Terminal.CurrentZone.GetPart<ScriptCallToArms>()?.scan();
					Terminal.CurrentScreen = new GritGateTerminalScreenMessage("{{y|Scanning..........................................\n..................................................\n..................................................\n[{{R|!!! {{y|Intruders detected}} !!!}}]\n..................................................\n[{{R|!!! {{y|I'm sounding alarms}} !!!}}]\n..................................................\n..................................................\n[{{R|!!! {{y|I'm calculating probable attack radii}} !!!}}]\n..................................................\n..................................................\n[{{R|!!! {{y|I'm displaying probable attack radii}} !!!}}]\n..................................................\n..................................................", this, () =>
					{
						UpdatePowerOptions();
					});
					ActionTaken = true;
				});
				Options.Add("Exit.");
				optionActions.Add(Exit);
			}
			else
			{
				UpdatePowerOptions();
			}
			Update();
		}
		base.BeforeRender(buffer, ref footerText);
	}

	public void UpdatePowerOptions()
	{
		ClearOptions();
		GameObject powerstation = Terminal.Terminal.CurrentCell.ParentZone.FindObject("GritGateBroadcastPowerStation");
		if (powerstation == null)
		{
			Options.Add("[{{R|!!! ERROR: RODANIS Y IS OFFLINE !!!}}]");
		}
		else if (!powerstation.GetPart<PowerSwitch>().Active)
		{
			Options.Add("Activate Rodanis Y. [{{R|-100 amps}}]");
			optionActions.Add(() =>
			{
				powerstation.FireEvent("PowerSwitchActivate");
				UpdatePowerOptions();
				ActionTaken = true;
			});
		}
		else
		{
			Options.Add("Deactivate Rodanis Y. [{{G|+100 amps}}]");
			optionActions.Add(() =>
			{
				powerstation.FireEvent("PowerSwitchDeactivate");
				UpdatePowerOptions();
				ActionTaken = true;
			});
		}
		GameObject chromebeacon = Terminal.Terminal.CurrentCell.ParentZone.FindObject("GritGateChromeBeacon");
		if (chromebeacon == null)
		{
			Options.Add("[{{R|!!! ERROR: CHROMELING BROADCAST BEACON IS OFFLINE !!!}}]");
			optionActions.Add(() =>
			{
			});
		}
		else
		{
			int currentlevel = chromebeacon.GetIntProperty("beaconlevel");
			if (currentlevel < 10)
			{
				Options.Add("Overclock chromelings (+10 QN, current level: {{C|" + currentlevel + "}}). [{{R|-22.5 amps}}]");
				optionActions.Add(() =>
				{
					chromebeacon.ModIntProperty("beaconlevel", 1);
					currentlevel++;
					ZoneAdjust part = chromebeacon.GetPart<ZoneAdjust>();
					part.AdjustSpec = chromebeacon.GetTag("beaconadjuststring").Replace("{0}", (currentlevel * 10).ToString());
					part.ChargeUse = currentlevel * 225;
					part.ParentObject.UseCharge(1, LiveOnly: false, 0L);
					UpdatePowerOptions();
					ActionTaken = true;
				});
			}
			if (currentlevel > 0)
			{
				Options.Add("Reduce clock on chromelings (-10 QN, current level: {{C|" + currentlevel + "}}). [{{G|+22.5 amps}}]");
				optionActions.Add(() =>
				{
					foreach (GameObject @object in Terminal.Terminal.CurrentCell.ParentZone.GetObjects("Chromeling"))
					{
						@object.RemoveEffect<Adjusted>();
					}
					chromebeacon.ModIntProperty("beaconlevel", -1);
					currentlevel--;
					ZoneAdjust part = chromebeacon.GetPart<ZoneAdjust>();
					part.ChargeUse = currentlevel * 225;
					if (currentlevel > 0)
					{
						part.AdjustSpec = chromebeacon.GetTag("beaconadjuststring").Replace("{0}", (currentlevel * 10).ToString());
						part.ParentObject.UseCharge(1, LiveOnly: false, 0L);
					}
					else
					{
						part.AdjustSpec = null;
					}
					ActionTaken = true;
					UpdatePowerOptions();
				});
			}
		}
		Options.Add("Activate a chain laser emplacement. [{{R|-50 amps}}]");
		optionActions.Add(() =>
		{
			Popup.Show("[{{R|!!! ERROR: REMOTE MANAGEMENT OFFLINE !!!}}]\n[{{R|!!! CHAIN LASER EMPLACEMENTS MUST BE ACTIVATED MANUALLY !!!}}]");
			ActionTaken = true;
			UpdatePowerOptions();
		});
		Options.Add("Activate a force projector. [{{R|-9 amps/square}}]");
		optionActions.Add(() =>
		{
			Popup.Show("[{{R|!!! ERROR: REMOTE MANAGEMENT OFFLINE !!!}}]\n[{{R|!!! FORCE PROJECTORS MUST BE ACTIVATED MANUALLY !!!}}]");
			UpdatePowerOptions();
		});
		Options.Add("Exit.");
		optionActions.Add(Exit);
		Update();
	}

	private void Exit()
	{
		Terminal.CurrentScreen = null;
		if (ActionTaken)
		{
			The.Player.UseEnergy(1000, "Terminal Interface");
		}
	}

	public override void Back()
	{
		Exit();
	}

	public override void Activate()
	{
		optionActions[Terminal.Selected]();
	}
}
