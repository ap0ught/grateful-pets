using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class PortableWall : IPart
{
	public string Blueprint = "Foamcrete";

	public int Size = 9;

	public string Message = "You open the box and expose the compressed foamcrete to air. It starts to expand.";

	public string ExamineFailureMessage = "=subject.T= suddenly =verb:erupt= in =object.name=!";

	public string DeployingWhat = "Wall";

	public string Sound = "Sounds/Interact/sfx_interact_portableWall_deploy";

	public override bool SameAs(IPart p)
	{
		PortableWall portableWall = p as PortableWall;
		if (portableWall.Blueprint != Blueprint)
		{
			return false;
		}
		if (portableWall.Size != Size)
		{
			return false;
		}
		if (portableWall.Message != Message)
		{
			return false;
		}
		if (portableWall.ExamineFailureMessage != ExamineFailureMessage)
		{
			return false;
		}
		if (portableWall.DeployingWhat != DeployingWhat)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivatePortableWall", null, 'a', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivatePortableWall" && E.Actor != null)
		{
			if (E.Actor.OnWorldMap())
			{
				E.Actor.ShowFailure("You cannot do that on the world map.");
				return base.HandleEvent(E);
			}
			List<Cell> list = null;
			ParentObject.SplitFromStack();
			if (E.Actor.IsPlayer())
			{
				list = PickFieldAdjacent(Size, E.Actor, DeployingWhat);
			}
			else
			{
				List<GameObject> list2 = E.Actor.CurrentCell.ParentZone.FastSquareVisibility(E.Actor.CurrentCell.X, E.Actor.CurrentCell.Y, Size, "ForceWallTarget", E.Actor);
				if (list2.Count > 0)
				{
					list = new List<Cell>(list2.Count);
					foreach (GameObject item in list2)
					{
						list.Add(item.CurrentCell);
						if (list.Count >= Size)
						{
							break;
						}
					}
				}
			}
			if (list != null && list.Count > 0 && !list[0].OnWorldMap())
			{
				list[0].PlayWorldSound(Sound);
				DeployToCells(list, E.Actor);
				if (E.Actor != null && E.Actor.IsPlayer())
				{
					Popup.Show(GameText.VariableReplace(Message, E.Actor, ParentObject, StripColors: true));
				}
				ParentObject.Destroy();
			}
			else
			{
				ParentObject.CheckStack();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100())
		{
			Cell startCell = ParentObject.CurrentCell ?? E.Actor.CurrentCell;
			Cell cell = startCell?.GetRandomLocalCardinalAdjacentCell();
			if (cell != null)
			{
				List<Cell> cells = Event.NewCellList();
				cells.Add(cell);
				while (cells.Count < Size)
				{
					cell = cells.Last().GetRandomLocalCardinalAdjacentCell((Cell c) => !cells.Contains(c) && c != startCell);
					if (cell == null)
					{
						break;
					}
					cells.Add(cell);
				}
				GameObject gameObject = DeployToCells(cells, E.Actor);
				if (gameObject != null)
				{
					if (E.Actor != null && E.Actor.IsPlayer())
					{
						Popup.Show(GameText.VariableReplace(ExamineFailureMessage, ParentObject, gameObject, StripColors: true));
					}
					E.IdentifyIfDestroyed = true;
					ParentObject.Destroy();
					return true;
				}
			}
		}
		return false;
	}

	private GameObject DeployToCells(List<Cell> Cells, GameObject Actor)
	{
		GameObject result = null;
		TextConsole textConsole = Look._TextConsole;
		The.Core.RenderBase();
		textConsole.WaitFrame();
		foreach (Cell Cell in Cells)
		{
			string directionFromCell = Actor.CurrentCell.GetDirectionFromCell(Cell);
			foreach (GameObject item in Cell.GetObjectsWithPart("Physics"))
			{
				if (item.Physics.Solid)
				{
					item.Physics.Push(directionFromCell, 7500, 4);
				}
			}
			foreach (GameObject item2 in Cell.GetObjectsWithPart("Combat"))
			{
				if (item2.Physics != null)
				{
					item2.Physics.Push(directionFromCell, 7500, 4);
				}
			}
			result = Cell.AddObject(Blueprint);
			The.Core.RenderBase();
			textConsole.WaitFrame();
		}
		return result;
	}
}
