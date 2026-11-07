using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PetGloaming : IPart
{
	public List<string> visitedZones = new List<string>();

	public int Gloaming;

	[NonSerialized]
	public GameObject gloamingTarget;

	public string gloamingBlueprint;

	public int GloamingChance = 10;

	public int StopGloamingChance = 30;

	public int SecretChance = 2;

	public float lastGloam;

	public int gloamingFrames;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("ApplyRealityStabilized");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Gloaming > 0 && (gloamingFrames > 0 || Stat.RandomCosmetic(1, 30) <= 1))
		{
			if (gloamingFrames > 0)
			{
				gloamingFrames--;
			}
			else
			{
				gloamingFrames = Stat.Random(2, 6);
			}
			if (gloamingTarget == null)
			{
				gloamingTarget = GameObject.Create(gloamingBlueprint);
			}
			if (gloamingTarget != null)
			{
				E.RenderString = gloamingTarget.Render.RenderString;
				E.Tile = gloamingTarget.Render.Tile;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyRealityStabilized")
		{
			if (ParentObject.CurrentCell != null && gloamingBlueprint != null && Gloaming > 0)
			{
				Gloaming = 0;
				Cell cell = ParentObject.CurrentCell;
				ParentObject.Brain.PartyLeader = null;
				ParentObject.Destroy();
				GameObject gameObject = cell.AddObject(gloamingBlueprint);
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("astral tether") + " snaps and " + ParentObject.its + " binal specter substantiates as " + gameObject.an() + ".");
				gameObject.SetActive();
				IComponent<GameObject>.ThePlayer.ApplyEffect(new ResummonGloaming());
			}
		}
		else if (E.ID == "EnteredCell" && ParentObject.CurrentZone != null && !visitedZones.Contains(ParentObject.CurrentZone.ZoneID))
		{
			visitedZones.Add(ParentObject.CurrentZone.ZoneID);
			if (SecretChance.in100())
			{
				IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
				JournalMapNote obj = randomUnrevealedNote as JournalMapNote;
				string text = "";
				text = ((obj == null) ? randomUnrevealedNote.Text : ("The location of " + Grammar.InitLowerIfArticle(randomUnrevealedNote.Text)));
				Popup.Show(ParentObject.Does("beat") + " " + ParentObject.its + " wings, and the shattered voices of a trillion worlds ride the current of air and harmonize into one, revealing the following wisdom:\n\n" + text);
				randomUnrevealedNote.Reveal(ParentObject.DisplayName);
			}
			if (Gloaming > 0 && StopGloamingChance.in100())
			{
				Gloaming = 0;
				ParentObject.RemoveEffect<Gleaming>();
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("stop") + " gleaming.");
			}
			else if (GloamingChance.in100())
			{
				Gloaming = 1;
				gloamingTarget = EncountersAPI.GetACreature();
				gloamingBlueprint = gloamingTarget.Blueprint;
				if (!ParentObject.HasEffect<Gleaming>())
				{
					ParentObject.ApplyEffect(new Gleaming());
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("start") + " to gleam with an {{K|unearthly light}}.");
				}
			}
		}
		return base.FireEvent(E);
	}
}
