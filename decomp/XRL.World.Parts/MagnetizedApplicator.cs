using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class MagnetizedApplicator : IPart
{
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
			if (E.Item.IsBroken() || E.Item.IsRusted() || E.Item.IsEMPed())
			{
				E.Actor.Fail(ParentObject.Does("do", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " nothing.");
				return false;
			}
			List<GameObject> objects = E.Actor.Inventory.GetObjects((GameObject o) => CanMagnetize(o, E.Actor));
			if (objects.Count == 0)
			{
				if (ParentObject.Understood())
				{
					E.Actor.Fail(ParentObject.Does("do", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " nothing.");
				}
				else
				{
					E.Actor.Fail("You have no items that can be magnetized.");
				}
				return false;
			}
			GameObject gameObject = PickItem.ShowPicker(objects, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
			if (gameObject == null)
			{
				return false;
			}
			gameObject.SplitFromStack();
			if (E.Actor.IsPlayer())
			{
				ParentObject.MakeUnderstood();
			}
			string message = gameObject.Does("become") + " magnetized!";
			bool flag = gameObject.Understood();
			if (!ItemModding.ApplyModification(gameObject, new ModMagnetized(), DoRegistration: true, E.Actor))
			{
				E.Actor.Fail("Nothing happens.");
				gameObject.CheckStack();
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				Popup.Show(message);
				Popup.Show(ParentObject.Does("lose", int.MaxValue, null, null, null, AsIfKnown: false, Single: true) + " its magnetic charge and crumbles to powder.");
			}
			if (flag && !gameObject.Understood())
			{
				gameObject.MakeUnderstood();
			}
			ParentObject.Destroy();
			gameObject.CheckStack();
		}
		return base.HandleEvent(E);
	}

	private bool CanMagnetize(GameObject obj, GameObject by)
	{
		return ItemModding.ModificationApplicable("ModMagnetized", obj, by);
	}
}
