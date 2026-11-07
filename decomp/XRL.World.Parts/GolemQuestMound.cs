using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.Language;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Quests.GolemQuest;

namespace XRL.World.Parts;

[Serializable]
public class GolemQuestMound : IPart
{
	public string BuilderID;

	public string BodyID;

	public long CompleteTurn = -1L;

	public const int GolemQuestXPAward = 40000;

	[NonSerialized]
	private GolemQuestSystem _System;

	public int CompleteDays => Mathf.RoundToInt((float)(CompleteTurn - The.Game.TimeTicks) * 1f / 1200f);

	public GolemQuestSystem System => _System ?? (_System = GolemQuestSystem.Get());

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (TryDisplayOptionsFor(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Interface", "interface", "ActivateGolemMound", null, 'i');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateGolemMound" && TryDisplayOptionsFor(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckCompletion();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckCompletion();
	}

	public void CheckCompletion()
	{
		if (CompleteTurn > 0 && The.Game.TimeTicks >= CompleteTurn && ParentObject.CurrentZone == The.ActiveZone)
		{
			CompleteTurn = -1L;
			Popup.Show("{{C|Your golem is ready for use.}}");
			Place(GameObject.FindByID(BuilderID) ?? The.Player);
		}
	}

	public bool TryDisplayOptionsFor(GameObject Actor)
	{
		if (Actor != The.Player)
		{
			return false;
		}
		if (!Actor.CheckFrozen())
		{
			return false;
		}
		if (CompleteTurn > 0)
		{
			int completeDays = CompleteDays;
			string text = "soon";
			if (completeDays >= 1)
			{
				text = "in " + Grammar.Cardinal(completeDays) + ((completeDays == 1) ? " day" : " days");
			}
			return Actor.ShowFailure("The creature is being shaped. It will be finished " + text + ".");
		}
		if (Actor.AreHostilesNearby())
		{
			return Actor.ShowFailure("You can't do that with hostile creatures nearby.");
		}
		if (System == null)
		{
			_System = GolemQuestSystem.Require();
		}
		PlayWorldSound("Sounds/Interact/sfx_interact_scrapClayMound_activate");
		DisplayOptions(Actor);
		return true;
	}

	public void DisplayOptions(GameObject Actor)
	{
		System.Mound = this;
		GolemQuestSelection[] array = System.Selections.Values.ToArray();
		string[] array2 = new string[array.Length];
		char[] array3 = new char[array.Length];
		bool flag = The.Game.HasFinishedQuest("The Golem") || ParentObject.HasIntProperty("Wish");
		QudMenuItem[] array4 = ((!flag) ? null : new QudMenuItem[1]
		{
			new QudMenuItem
			{
				command = "option:-2",
				hotkey = "CmdDelete"
			}
		});
		int num = 0;
		while (true)
		{
			array2.Fill(array.Select((GolemQuestSelection x) => x.GetOptionText()));
			array3.Fill(array.Select((GolemQuestSelection x) => x.Key));
			bool flag2 = false;
			if (flag)
			{
				flag2 = array.All((GolemQuestSelection x) => x.IsValid());
				array4[0].text = (flag2 ? "{{W|[Backspace]}} {{y|Build}}" : "{{K|Build}}");
			}
			num = Popup.PickOption("", ParentObject.GetPart<Description>().Short + "\n\n", "", "Sounds/UI/ui_notification", array2, array3, null, array4, ParentObject, null, null, 0, 60, num, -1, AllowEscape: true, RespectOptionNewlines: true);
			if (num < 0 || num >= array.Length)
			{
				System.UpdateQuest();
				if (num != -2)
				{
					break;
				}
				if (flag2)
				{
					Build(Actor);
					break;
				}
			}
			else
			{
				array[num].Pick();
				Event.ResetPool();
			}
		}
		if (System != null)
		{
			System.Mound = null;
		}
	}

	public void Build(GameObject Actor, int DurationTicks = 0)
	{
		GameObject gameObject = Vehicle.CreateOwnedBy(System.Body.BodyBlueprint.Name, Actor, true, true);
		gameObject.SetGender(System.Body.Material.GetGender(AsIfKnown: true));
		foreach (KeyValuePair<string, GolemQuestSelection> selection in System.Selections)
		{
			try
			{
				selection.Value.Apply(gameObject);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("GolemMound", x);
			}
		}
		GolemQuestSelection.ProcessDescription(gameObject);
		The.Game.FinishQuest("The Golem");
		IComponent<GameObject>.ThePlayer.AwardXP(40000, -1, 0, int.MaxValue, null, gameObject);
		The.ZoneManager.CachedObjects[gameObject.ID] = gameObject;
		BuilderID = Actor.ID;
		BodyID = gameObject.ID;
		if (DurationTicks <= 0)
		{
			Place(Actor);
		}
		else
		{
			CompleteTurn = The.Game.TimeTicks + DurationTicks;
		}
	}

	public void Place(GameObject Actor)
	{
		Dictionary<string, GameObject> cachedObjects = The.ZoneManager.CachedObjects;
		if (!BodyID.IsNullOrEmpty() && cachedObjects.TryGetValue(BodyID, out var value))
		{
			cachedObjects.Remove(BodyID);
			value.SetAlliedLeader<AllyConstructed>(Actor);
			ParentObject.CurrentZone.GetObjectsWithTagOrProperty("GolemChassis", UseEventList: true).ForEach((GameObject x) =>
			{
				x.Obliterate(null, Silent: true);
			});
			ParentObject.ReplaceWith(value);
			BodyID = null;
			value.MakeActive();
			value.Brain.PerformReequip();
			The.Game.FlagSystemForRemoval(System);
			The.Game.RemoveBooleanGameState("GolemBuilding");
			_System = null;
			JournalAPI.AddAccomplishment("With the help of Pax Klanq and the Barathrumites, you constructed " + value.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: true, BaseOnly: true) + " to climb the Spindle.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
		}
	}
}
