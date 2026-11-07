using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatorSpray : IPart
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
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("The sprayer head won't move.");
				}
				return false;
			}
			if (E.Actor.IsPlayer() && !E.Item.Understood())
			{
				E.Item.MakeUnderstood();
				Popup.Show(E.Item.Itis + " " + E.Item.an() + "!");
			}
			Cell cell = null;
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				cell = E.Actor.Physics.PickDirection("Animate");
			}
			if (cell == null)
			{
				return false;
			}
			List<GameObject> list = new List<GameObject>();
			List<string> list2 = new List<string>();
			char c = 'a';
			List<char> list3 = new List<char>();
			foreach (GameObject @object in cell.Objects)
			{
				if (@object.HasTagOrProperty("Animatable"))
				{
					list.Add(@object);
					list2.Add(@object.DisplayNameOnlyStripped);
					list3.Add(c);
					c = (char)(c + 1);
				}
			}
			if (list.Count == 0)
			{
				Popup.ShowFail("There's nothing viable to animate here.");
				return false;
			}
			int num = Popup.PickOption("", "Choose a piece of furniture or other viable object to animate.", "", "Sounds/UI/ui_notification", list2.ToArray(), list3.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			GameObject gameObject = list[num];
			if (gameObject.HasPart<Brain>() && E.Actor.IsPlayer())
			{
				Popup.ShowFail("You can't animate an object that already has a brain.");
				return false;
			}
			gameObject.PlayWorldSound("Sounds/Interact/sfx_interact_sentience_imbue");
			Popup.Show("You imbue " + gameObject.t() + " with life.");
			JournalAPI.AddAccomplishment("You imbued " + gameObject.an() + " with life. Why?", "While traveling in " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= performed a sacred ritual with " + gameObject.an() + ", imbuing " + gameObject.them + " with life and arranging " + gameObject.them + " " + HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".babeTrait.!random>") + ". Many of the local denizens declared it a miracle. Some weren't so sure.", "While traveling in " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= performed a sacred ritual with " + gameObject.an() + ", imbuing " + gameObject.them + " with life and arranging " + gameObject.them + " " + HistoricStringExpander.ExpandString("<spice.elements." + The.Player.GetMythicDomain() + ".babeTrait.!random>") + ". Many of the local denizens declared it a miracle.", null, "general", MuralCategory.CommitsFolly, MuralWeight.Medium, null, -1L);
			AnimateObject.Animate(gameObject, E.Actor, ParentObject);
			CombatJuice.playPrefabAnimation(gameObject, "Abilities/AbilityVFXAnimated", gameObject.ID, gameObject.Render.Tile + ";" + gameObject.Render.GetTileForegroundColor() + ";" + gameObject.Render.getDetailColor());
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
