using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ReshephsCrypt : IPart
{
	public string Inscription = "";

	public string Prefix = "";

	public string Postfix = "";

	public bool bFirst = true;

	public string faction;

	public string name;

	public bool transitallowed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("BeforeClosing");
		Registrar.Register("SyncClosed");
		Registrar.Register("ObjectEntered");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && ParentObject.Physics.CurrentCell.ParentZone.FindObject("Biographer") == null && ParentObject.HasIntProperty("Sealed"))
		{
			IComponent<GameObject>.XDidY(ParentObject, "open");
			ParentObject.RemoveIntProperty("Sealed");
			ParentObject.FireEvent("SyncOpened");
			foreach (GameObject item in ParentObject.Physics.CurrentCell.ParentZone.GetObjectsWithPart("ReshephsCrypt"))
			{
				if (item != ParentObject)
				{
					item.RemoveIntProperty("Sealed");
					item.FireEvent("SyncOpened");
				}
			}
		}
		if (E.ID == "SyncClosed" && transitallowed)
		{
			transitallowed = false;
			GameObject crypt = null;
			foreach (GameObject item2 in ParentObject.Physics.currentCell.ParentZone.GetObjectsWithPart("ReshephsCrypt"))
			{
				item2.SetStringProperty("EntombTriggered", "1");
				item2.SetIntProperty("Sealed", 1);
				item2.FireEvent("SyncClosed");
				crypt = item2;
			}
			GameManager.Instance.gameQueue.queueTask(() =>
			{
				ThinWorld.TransitToThinWorld(crypt);
			});
		}
		if (E.ID == "BeforeClosing")
		{
			if (ParentObject.HasStringProperty("EntombTriggered"))
			{
				Popup.Show("The sarcophagus is inert.");
				transitallowed = false;
				return true;
			}
			if (!(Popup.AskString("The sealing mechanisms inside this sarcophagus will certainly kill you if you close " + IComponent<GameObject>.ThePlayer.itself + " inside. Are you sure you want to enter the sarcophagus?\n\nType 'ENTOMB' to confirm.", "", "Sounds/UI/ui_notification", null, "ENTOMB", 6, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).ToUpper() == "ENTOMB"))
			{
				transitallowed = false;
				return false;
			}
			if ((!IComponent<GameObject>.ThePlayer.IsOriginalPlayerBody() && IComponent<GameObject>.ThePlayer.HasEffect<Dominated>()) || IComponent<GameObject>.ThePlayer.HasEffect<WakingDream>())
			{
				Popup.Show("You mind swerves from afar, and a force of repulsion from inside the sarcophagus prevents your entry.");
				transitallowed = false;
				return false;
			}
			if (!IComponent<GameObject>.ThePlayer.HasMarkOfDeath())
			{
				Popup.Show("A force of repulsion from inside the sarcophagus prevents your entry.\n\nYou do not bear the Mark of Death.");
				transitallowed = false;
				return false;
			}
			Popup.Show("You climb into the sarcophagus.");
			if (!The.Game.HasQuest("Tomb of the Eaters"))
			{
				The.Game.StartQuest("Tomb of the Eaters");
			}
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			Cell cell = IComponent<GameObject>.ThePlayer.GetCurrentCell();
			cell.RemoveObject(IComponent<GameObject>.ThePlayer);
			cell.ParentZone.LightAll();
			cell.ParentZone.VisAll();
			cell.ParentZone.Render(scrapBuffer);
			scrapBuffer.Draw();
			cell.AddObject(IComponent<GameObject>.ThePlayer);
			GameManager.Instance.ForceGameView();
			Thread.Sleep(1000);
			transitallowed = true;
		}
		if (E.ID == "ObjectEntered")
		{
			_ = transitallowed;
		}
		return base.FireEvent(E);
	}

	public static void AddMarkOfDeath()
	{
		XRLCore.Core.Game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
	}
}
