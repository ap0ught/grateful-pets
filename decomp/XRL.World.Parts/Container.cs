using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Container : IPart
{
	public string Preposition = "in";

	public string OpenSound = "Sounds/Interact/sfx_interact_open_genericContainer";

	public string CloseSound;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			AttemptOpen(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Open", "open", "Open", null, 'o');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Open")
		{
			AttemptOpen(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public void PlayOpenSound()
	{
		string text = OpenSound ?? (OpenSound = ParentObject.GetTagOrStringProperty("OpenSound"));
		if (!text.IsNullOrEmpty())
		{
			PlayWorldSound(text);
		}
	}

	public void PlayCloseSound()
	{
		string text = CloseSound ?? (CloseSound = ParentObject.GetTagOrStringProperty("CloseSound"));
		if (!text.IsNullOrEmpty())
		{
			PlayWorldSound(text);
		}
	}

	public void AttemptOpen(GameObject Actor, IEvent ParentEvent = null)
	{
		ParentObject?.Inventory?.VerifyContents();
		if (!ParentObject.IsValid() || !ParentObject.FireEvent("BeforeOpen"))
		{
			return;
		}
		ParentObject.FireEvent("Opening");
		ParentObject.SetIntProperty("Autoexplored", 1);
		bool flag = ParentObject.Inventory?.HasObjectDirect((GameObject x) => x.HasIntProperty("StoredByPlayer")) ?? false;
		if (ParentObject.IsCreature)
		{
			if (!Actor.IsPlayer())
			{
				return;
			}
			if (Actor.PhaseMatches(ParentObject) && !ParentObject.HasPropertyOrTag("NoTrade") && !ParentObject.HasPropertyOrTag("FugueCopy") && Actor.DistanceTo(ParentObject) <= 1)
			{
				PlayOpenSound();
				if (ParentObject.IsPlayerLed())
				{
					TradeUI.ShowTradeScreen(ParentObject, 0f);
				}
				else if (ParentObject.IsPlayer())
				{
					Screens.CurrentScreen = 2;
					Screens.Show(The.Player);
				}
				else
				{
					TradeUI.ShowTradeScreen(ParentObject);
				}
				PlayCloseSound();
			}
			else
			{
				Popup.ShowFail("You cannot trade with " + ParentObject.t() + ".");
			}
			return;
		}
		if (!ParentObject.HasTagOrProperty("DontWarnOnOpen") && Actor.IsPlayer() && !string.IsNullOrEmpty(ParentObject.Owner) && ParentObject.Equipped != IComponent<GameObject>.ThePlayer && ParentObject.InInventory != IComponent<GameObject>.ThePlayer)
		{
			if (Popup.ShowYesNoCancel("That is not owned by you. Are you sure you want to open it?") != DialogResult.Yes)
			{
				return;
			}
			AIHelpBroadcastEvent.Send(ParentObject, Actor, null, null, 20, 1f, HelpCause.Trespass);
		}
		if (!Actor.IsPlayer())
		{
			return;
		}
		Inventory inventory = ParentObject.Inventory;
		if (inventory == null || inventory.GetObjectCount() == 0)
		{
			if (Popup.ShowYesNo("There's nothing " + Preposition + " that. Would you like to store an item?") == DialogResult.Yes)
			{
				PlayOpenSound();
				TradeUI.ShowTradeScreen(ParentObject, 0f, TradeUI.TradeScreenMode.Container);
				PlayCloseSound();
				if (inventory.GetObjectCount() > 0)
				{
					Actor.FireEvent(Event.New("PutSomethingIn", "Object", ParentObject));
				}
			}
		}
		else
		{
			List<GameObject> list = new List<GameObject>(inventory.GetObjectsDirect());
			bool RequestInterfaceExit = false;
			string title = "{{W|" + ((Preposition == "in") ? "Opening" : "Examining") + " " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "}}";
			bool notePlayerOwned = false;
			PlayOpenSound();
			List<GameObject> objects = inventory.GetObjects();
			GameObject parentObject = ParentObject;
			Func<List<GameObject>> regenerate = inventory.GetObjects;
			PickItem.ShowPicker(objects, ref RequestInterfaceExit, null, PickItem.PickItemDialogStyle.GetItemDialog, Actor, parentObject, null, title, PreserveOrder: false, regenerate, ShowContext: false, ShowIcons: true, notePlayerOwned);
			PlayCloseSound();
			if (RequestInterfaceExit)
			{
				ParentEvent?.RequestInterfaceExit();
			}
			if (list.Count < inventory.GetObjectCountDirect() && inventory.GetObjectCount() > 0)
			{
				Actor.FireEvent(Event.New("PutSomethingIn", "Object", ParentObject));
			}
			GameObject gameObject = null;
			double num = 0.0;
			double num2 = -1.0;
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				GameObject gameObject2 = list[num3];
				if (!inventory.Objects.Contains(gameObject2) && ParentObject.IsOwned() && !gameObject2.OwnedByPlayer)
				{
					double valueEach = gameObject2.ValueEach;
					num += valueEach * (double)gameObject2.Count;
					if (valueEach > num2)
					{
						num2 = valueEach;
						gameObject = gameObject2;
					}
				}
			}
			if (gameObject != null)
			{
				float magnitude = Mathf.Max(1f, (float)(num / 20.0));
				AIHelpBroadcastEvent.Send(ParentObject, Actor, gameObject, null, 20, magnitude, HelpCause.Theft);
			}
		}
		bool flag2 = ParentObject.Inventory?.HasObjectDirect((GameObject x) => x.HasIntProperty("StoredByPlayer")) ?? false;
		if (flag | flag2)
		{
			ParentObject.Inventory?.TryStoreBackup();
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Open");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Open")
		{
			AttemptOpen(E.GetGameObjectParameter("Opener"), E);
		}
		return base.FireEvent(E);
	}
}
