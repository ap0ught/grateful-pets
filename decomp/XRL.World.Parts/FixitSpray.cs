using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class FixitSpray : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (E.Item.IsBroken() || E.Item.IsRusted())
			{
				E.Actor.ShowFailure("The sprayer head won't move.");
				return false;
			}
			List<GameObject> inventoryAndEquipment = E.Actor.GetInventoryAndEquipment();
			ProcessCellForTargets(E.Actor, E.Actor.CurrentCell, inventoryAndEquipment);
			foreach (Cell localAdjacentCell in E.Actor.CurrentCell.GetLocalAdjacentCells())
			{
				ProcessCellForTargets(E.Actor, localAdjacentCell, inventoryAndEquipment);
			}
			GameObject gameObject = PickItem.ShowPicker(inventoryAndEquipment, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.HasTagOrProperty("NoRepair"))
			{
				E.Actor.ShowFailure("You can't repair that.");
				return false;
			}
			string Message = null;
			int matterPhase = gameObject.GetMatterPhase();
			if (matterPhase >= 3 || !gameObject.PhaseMatches(E.Actor))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Some sticky goop passes through " + gameObject.t() + ".");
				}
			}
			else if (matterPhase == 2)
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Some sticky goop mixes in with " + gameObject.t() + ".");
				}
				gameObject.LiquidVolume?.MixWith(new LiquidVolume("gel", 1));
			}
			else
			{
				E.Actor.PlayWorldOrUISound("Sounds/Misc/sfx_interact_artifact_repair");
				if (E.Actor.IsPlayer())
				{
					if (gameObject == E.Actor)
					{
						Popup.Show("You are covered in sticky goop!");
					}
					else
					{
						Popup.Show(gameObject.Does("are") + " covered in sticky goop!");
					}
					ParentObject.MakeUnderstood(out Message);
				}
				RepairedEvent.Send(E.Actor, gameObject, ParentObject);
			}
			ParentObject.Destroy();
			if (!Message.IsNullOrEmpty())
			{
				Popup.Show(Message);
			}
		}
		return base.HandleEvent(E);
	}

	private void ProcessCellForTargets(GameObject Actor, Cell C, List<GameObject> Objects)
	{
		if (C == null)
		{
			return;
		}
		IList<GameObject> list;
		if (!C.IsSolidFor(Actor))
		{
			IList<GameObject> objects = C.Objects;
			list = objects;
		}
		else
		{
			IList<GameObject> objects = C.GetCanInteractInCellWithSolidObjectsFor(Actor);
			list = objects;
		}
		IList<GameObject> list2 = list;
		int i = 0;
		for (int count = list2.Count; i < count; i++)
		{
			GameObject gameObject = list2[i];
			Physics physics = gameObject.Physics;
			if (physics != null && physics.IsReal && IComponent<GameObject>.Visible(gameObject) && gameObject.Render != null && gameObject.Render.RenderLayer > 0 && !Objects.Contains(gameObject) && gameObject != ParentObject)
			{
				Objects.Add(gameObject);
			}
		}
	}
}
