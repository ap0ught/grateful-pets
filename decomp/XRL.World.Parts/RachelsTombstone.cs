using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Qud.API;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RachelsTombstone : IPart
{
	public string Inscription = "";

	public string Prefix = "";

	public string Postfix = "";

	public bool NeedsGeneration = true;

	public string faction;

	public string name;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
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
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Look.ShowLooker(0, cell.X, cell.Y);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (NeedsGeneration)
		{
			GenerateTombstone();
		}
		E.Prefix.Append(Prefix);
		E.Base.Append(Inscription);
		E.Postfix.Append(Postfix);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.Render.Tile = "terrain/sw_tombstone_" + Stat.Random(1, 4) + ".bmp";
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt")
		{
			JournalAPI.RevealObservation("MarkOfDeathSecret", onlyIfNotRevealed: true);
			The.Game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
		}
		return base.FireEvent(E);
	}

	public void GenerateTombstone()
	{
		name = "Rebekah";
		string stringGameState = The.Game.GetStringGameState("MarkOfDeath");
		string input = HistoricStringExpander.ExpandString("<spice.tombstones.intro.!random>");
		string input2 = "Succumbed to glotrot.";
		List<string> list = new List<string> { "" };
		string[] array = StringFormat.ClipText(input, 28).Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		list.Add("");
		list.Add("");
		array = StringFormat.ClipText(name, 28).Split('\n');
		foreach (string item2 in array)
		{
			list.Add(item2);
		}
		list.Add("");
		list.Add("");
		array = StringFormat.ClipText(input2, 28).Split('\n');
		foreach (string item3 in array)
		{
			list.Add(item3);
		}
		list.Add("");
		array = StringFormat.ClipText(stringGameState, 28).Split('\n');
		foreach (string item4 in array)
		{
			list.Add(item4);
		}
		list.Add("");
		for (int j = 0; j < list.Count; j++)
		{
			Inscription += "\nÿÿÿ";
			if (j % 2 == 0)
			{
				Inscription += "º";
			}
			else
			{
				Inscription += "¶";
			}
			Inscription += list[j].PadLeft(17 + (list[j].Length / 2 - 1), 'ÿ').PadRight(31, 'ÿ');
			if (j % 2 == 0)
			{
				Inscription += "º";
			}
			else
			{
				Inscription += "Ç";
			}
		}
		NeedsGeneration = false;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('É');
		stringBuilder.Append('Í', 31);
		stringBuilder.Append('»');
		Prefix = stringBuilder.ToString();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append('ÿ');
		stringBuilder2.Append('ÿ');
		stringBuilder2.Append('ÿ');
		stringBuilder2.Append('Ó');
		stringBuilder2.Append('Ä', 31);
		stringBuilder2.Append('½');
		Inscription += "\n";
		Inscription += stringBuilder2.ToString();
	}
}
