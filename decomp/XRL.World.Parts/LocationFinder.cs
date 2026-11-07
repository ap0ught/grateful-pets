using System;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class LocationFinder : IPart
{
	public string ID;

	[Obsolete("Unused, the text of the associated map note is always used.")]
	public string Text;

	public int Value;

	public string Trigger;

	[NonSerialized]
	private bool Activated;

	[NonSerialized]
	private bool WantBeforeRenderEvent;

	public LocationFinder()
	{
		Trigger = "Created";
	}

	public override void Attach()
	{
		WantBeforeRenderEvent = Trigger == "Created";
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (Trigger == "OnScreen")
		{
			Registrar.Register("EndTurn");
		}
		if (Trigger == "Taken")
		{
			Registrar.Register("Taken");
		}
		base.Register(Object, Registrar);
	}

	public void TriggerFind()
	{
		if (Activated)
		{
			return;
		}
		Activated = true;
		if (!The.Game.StringGameState.ContainsKey("LairFound_" + ID))
		{
			JournalMapNote mapNote = JournalAPI.GetMapNote(ID);
			string text;
			if (mapNote.Category == "Ruins")
			{
				text = ((mapNote.Text == "some forgotten ruins") ? Grammar.InitLower(mapNote.Text) : Grammar.MakeTitleCaseWithArticle(mapNote.Text));
			}
			else if (mapNote.Category == "Lairs")
			{
				text = Grammar.InitLower(mapNote.Text);
				Achievement.LAIRS_100.Progress.Increment();
				The.Game.StringGameState.Add("LairFound_" + ID, "1");
				The.Game.IncrementIntGameState("LairsFound", 1);
			}
			else
			{
				_ = mapNote.Category == "Settlements";
				text = Grammar.InitLowerIfArticle(mapNote.Text);
			}
			if (!mapNote.Revealed)
			{
				Popup.Show("You discover " + text + "!", null, "sfx_newLocation_discovered");
				JournalAPI.AddAccomplishment("You discovered " + text + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + text + ", once thought lost to the sands of time.", "In =year=, =name= won a decisive victory against the combined force of " + Factions.GetMostHatedFormattedName() + " from " + The.Player.CurrentZone.GetTerrainDisplayName() + " at the bloody Battle of " + text + ".", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
				if (Value > 0)
				{
					The.Player.AwardXP(Value, -1, 0, int.MaxValue, null, null, null, null, ParentObject.GetCurrentZone()?.ZoneID);
				}
				JournalAPI.RevealMapNote(mapNote);
			}
			else if (Value > 0)
			{
				Popup.Show("You traveled to " + text + "!");
				if (Value > 0)
				{
					The.Player.AwardXP(Value, -1, 0, int.MaxValue, null, null, null, null, ParentObject.GetCurrentZone()?.ZoneID);
				}
				JournalAPI.AddAccomplishment("You traveled to " + text + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + text + ", once thought lost to the sands of time.", "In =year=, =name= won a decisive victory against the combined force of " + Factions.GetMostHatedFormattedName() + " from " + The.Player.CurrentZone.GetTerrainDisplayName() + " at the bloody Battle of " + text + ".", null, "general", MuralCategory.VisitsLocation, MuralWeight.Low, null, -1L);
			}
		}
		if (ParentObject.HasTag("Non"))
		{
			ParentObject.Obliterate(null, Silent: true);
			return;
		}
		GameManager.Instance.gameQueue.queueTask(() =>
		{
			ParentObject.RemovePart(this);
		});
	}

	public override bool Render(RenderEvent E)
	{
		if (Trigger == "Seen" && ParentObject?.CurrentZone?.ZoneID == The.Player?.CurrentZone?.ZoneID && ParentObject?.CurrentZone?.ZoneID != null)
		{
			TriggerFind();
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == BeforeRenderEvent.ID)
			{
				return WantBeforeRenderEvent;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Trigger == "Created" && ParentObject?.CurrentZone?.ZoneID == The.Player?.CurrentZone?.ZoneID && ParentObject?.CurrentZone?.ZoneID != null)
		{
			TriggerFind();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (Trigger == "Taken" && E.ID == "Taken")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("TakingObject");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				TriggerFind();
			}
		}
		return base.FireEvent(E);
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(ID);
		Writer.WriteOptimized(Value);
		Writer.WriteOptimized(Trigger);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		ID = Reader.ReadOptimizedString();
		Value = Reader.ReadOptimizedInt32();
		Trigger = Reader.ReadOptimizedString();
	}
}
