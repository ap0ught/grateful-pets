using System;
using System.Collections.Generic;
using Qud.UI;
using XRL;
using XRL.CharacterBuilds;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;

public abstract class TutorialStep
{
	public int step;

	public TutorialManager manager;

	protected string CurrentGameView => GameManager.Instance.CurrentGameView;

	public bool ConstrainToCurrentZone(Cell nextCell)
	{
		if (nextCell?.ParentZone != The.PlayerCell?.ParentZone)
		{
			Popup.Show("We're not quite ready to leave yet.");
			return false;
		}
		return true;
	}

	public virtual bool StartingLineTooltip(GameObject go, GameObject compareGo)
	{
		return true;
	}

	public virtual void BeforeRenderEvent()
	{
		Zone zone = The.Player?.CurrentZone;
		Cell cell = The.Player?.CurrentCell;
		if (zone != null && cell != null && zone.Z > 10)
		{
			zone.AddLight(cell.X, cell.Y, 5);
		}
	}

	public virtual int AdjustExaminationRoll(int result)
	{
		return result;
	}

	public virtual bool AllowOverlandEncounters()
	{
		return false;
	}

	public virtual bool OnTradeOffer()
	{
		return true;
	}

	public virtual bool OnTradeComplete()
	{
		return true;
	}

	public virtual bool AllowTooltipUpdate()
	{
		return true;
	}

	public virtual void GameSync()
	{
	}

	public virtual bool IsEditableChargenPanel(string id)
	{
		switch (id)
		{
		default:
			return id == "Starting Location";
		case "Pregens":
		case "Genotypes":
		case "Game Modes":
		case "Summary":
		case "Customize":
			return true;
		}
	}

	public virtual bool AllowSelectPregen(string id)
	{
		return id == "Marsh Taur";
	}

	public virtual bool AllowSelectGenotype(string id)
	{
		return id == "Mutated Human";
	}

	public virtual bool AllowTargetPick(GameObject go, Type ability, List<Cell> target)
	{
		return true;
	}

	public virtual bool AllowSelectedPopupCommand(string id, QudMenuItem command)
	{
		return true;
	}

	public virtual bool AllowInventoryInteract(GameObject go)
	{
		return true;
	}

	public virtual bool AllowOnSelected(FrameworkDataElement element)
	{
		return true;
	}

	public virtual bool AllowCommand(string id)
	{
		return true;
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnBootGame(XRLGame game, EmbarkInfo info)
	{
	}

	public virtual void OnTrigger(string trigger)
	{
	}

	public virtual bool BeforePlayerEnterCell(Cell cell)
	{
		return true;
	}
}
