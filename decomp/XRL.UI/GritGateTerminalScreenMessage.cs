using System;
using XRL.Core;
using XRL.Messages;

namespace XRL.UI;

public class GritGateTerminalScreenMessage : GritGateTerminalScreen
{
	private GritGateTerminalScreen LastScreen;

	private Action After;

	public GritGateTerminalScreenMessage(string message, GritGateTerminalScreen LastScreen, Action After = null)
	{
		this.LastScreen = LastScreen;
		this.After = After;
		MainText = message;
		ClearOptions();
		Options.Add("Exit.");
		optionActions.Add(() =>
		{
			MessageQueue.AddPlayerMessage("Alarms blare across the enclave.");
			XRLCore.Core.Game.FinishQuestStep("Grave Thoughts", "Investigate the Rumbling");
			Terminal.CurrentScreen = null;
		});
	}

	public override void Back()
	{
		Terminal.CurrentScreen = LastScreen;
		if (After != null)
		{
			After();
		}
	}

	public override void Activate()
	{
		optionActions[Terminal.Selected]();
	}
}
