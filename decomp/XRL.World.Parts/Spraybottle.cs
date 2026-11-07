using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Spraybottle : IPart
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
			if (E.Item.IsBroken() || E.Item.IsRusted())
			{
				return E.Actor.Fail("The sprayer head won't move.");
			}
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume.Volume <= 0)
			{
				return E.Actor.Fail(ParentObject.Does("are") + " empty.");
			}
			List<GameObject> equippedObjects = E.Actor.Body.GetEquippedObjects();
			equippedObjects.AddRange(E.Actor.Inventory.GetObjects());
			equippedObjects.Remove(ParentObject);
			GameObject gameObject = PickItem.ShowPicker(equippedObjects, "Fungal Infection", PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor, null, null, null, PreserveOrder: false, null, ShowContext: true);
			if (gameObject == null)
			{
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				Popup.Show(gameObject.Does("are") + " covered in " + liquidVolume.GetLiquidName() + "!");
				ParentObject.MakeUnderstood();
			}
			gameObject.ApplyEffect(new LiquidCovered(liquidVolume, 1, Stat.Random(5, 10)));
		}
		return base.HandleEvent(E);
	}
}
