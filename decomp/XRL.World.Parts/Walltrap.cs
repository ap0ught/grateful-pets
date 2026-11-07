using System;
using ConsoleLib.Console;

namespace XRL.World.Parts;

[Serializable]
public class Walltrap : IPart
{
	public int TurnInterval = 3;

	public int CurrentTurn;

	public string ReadyColor = "^g&R";

	public string WarmColor = "^g&r";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ProcessWalltrapTrigger();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ProcessWalltrapTrigger();
	}

	public void ProcessWalltrapTrigger()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && currentZone.IsActive() && !currentZone.IsWorldMap())
		{
			Render render = ParentObject.Render;
			if (++CurrentTurn == TurnInterval - 1)
			{
				render.ColorString = ReadyColor;
			}
			else
			{
				render.ColorString = WarmColor;
			}
			char? foreground = 'y';
			char? background = 'k';
			ColorUtility.FindLastForegroundAndBackground(render.ColorString, ref foreground, ref background);
			char? c = foreground;
			render.TileColor = "&" + c;
			render.DetailColor = background.ToString();
			if (CurrentTurn >= TurnInterval)
			{
				ParentObject.FireEvent("WalltrapTrigger");
				CurrentTurn = 0;
			}
		}
	}
}
