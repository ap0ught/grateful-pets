using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Polygel : IPart
{
	public const string REPLICATION_CONTEXT = "Polygel";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeReplicatedEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeReplicatedEvent E)
	{
		return false;
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
			Inventory inventory = E.Actor.Inventory;
			ParentObject.SplitFromStack();
			List<GameObject> list = Event.NewGameObjectList();
			inventory.GetObjects(list);
			list.Remove(ParentObject);
			GameObject gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.HasPart<Body>() || !CanBeReplicatedEvent.Check(gameObject, E.Actor, "Polygel"))
			{
				Popup.ShowFail("A loud buzz is emitted. The unauthorized glyph flashes on the side of the applicator.");
				return false;
			}
			GameObject gameObject2 = null;
			try
			{
				gameObject.SplitStack(1, The.Player);
				gameObject2 = gameObject.DeepCopy(CopyEffects: true);
				gameObject2.Inventory?.Clear();
				if (gameObject2.HasPart<EnergyCellSocket>())
				{
					gameObject2.GetPart<EnergyCellSocket>().Cell = null;
				}
				if (gameObject2.HasPart<MagazineAmmoLoader>())
				{
					gameObject2.GetPart<MagazineAmmoLoader>().Ammo = null;
				}
				Temporary.CarryOver(ParentObject, gameObject2, CanRemove: true);
				Phase.carryOver(ParentObject, gameObject2);
				if (!inventory.Objects.Contains(gameObject))
				{
					inventory.AddObject(gameObject, null, Silent: true, NoStack: true, FlushTransient: true, E);
				}
				if (!inventory.Objects.Contains(gameObject2))
				{
					inventory.AddObject(gameObject2, null, Silent: true, NoStack: true, FlushTransient: true, E);
				}
				try
				{
					if (E.Actor.IsPlayer() && !E.Item.Understood())
					{
						E.Item.MakeUnderstood();
						Popup.Show(ParentObject.Itis + " " + ParentObject.an() + "!");
					}
				}
				catch (Exception message)
				{
					MetricsManager.LogError(message);
				}
				try
				{
					ParentObject.Destroy();
				}
				catch (Exception message2)
				{
					MetricsManager.LogError(message2);
				}
				try
				{
					Popup.Show("The polygel morphs into another " + gameObject2.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true) + "!");
				}
				catch (Exception message3)
				{
					MetricsManager.LogError(message3);
				}
				try
				{
					WasReplicatedEvent.Send(gameObject, E.Actor, gameObject2, "Polygel");
				}
				catch (Exception message4)
				{
					MetricsManager.LogError(message4);
				}
				try
				{
					ReplicaCreatedEvent.Send(gameObject2, E.Actor, gameObject, "Polygel");
				}
				catch (Exception message5)
				{
					MetricsManager.LogError(message5);
				}
			}
			finally
			{
				gameObject2?.CheckStack();
			}
		}
		return base.HandleEvent(E);
	}
}
