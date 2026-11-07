using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Command : GoalHandler
{
	public string Cmd;

	public Command()
	{
	}

	public Command(string Cmd)
		: this()
	{
		this.Cmd = Cmd;
	}

	public override string GetDetails()
	{
		return Cmd;
	}

	public override bool Finished()
	{
		return false;
	}

	public override bool CanFight()
	{
		return false;
	}

	public override void TakeAction()
	{
		try
		{
			CommandEvent.Send(ParentObject, Cmd);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Command::Send", x);
		}
		Pop();
	}
}
