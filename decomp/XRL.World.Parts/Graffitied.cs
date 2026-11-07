using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class Graffitied : IPart
{
	public string Text;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{graffitied|graffitied}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AddGraffiti(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		AddGraffiti(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (E.Context != "Sample")
		{
			Graffiti(ParentObject);
		}
		else
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public void Graffiti(GameObject wall)
	{
		wall.Render.SetForegroundColor(Crayons.GetRandomColorExcept((string c) => c == wall.Render.GetForegroundColor()));
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		MarkovChainData data = MarkovBook.CorpusData[text];
		Text = MarkovChain.GenerateShortSentence(data);
		Text = Grammar.Obfuscate(Text.TrimEnd(' '), 10);
		wall.SetIntProperty("HasGraffiti", 1);
	}

	public void AddGraffiti(IShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(Text))
		{
			if (E.Infix.Length > 0)
			{
				E.Infix.Append("\n");
			}
			E.Infix.Append("\n").Append("Graffiti is scrawled across the surface. It reads: \n\n\"").Append("{{")
				.Append(ParentObject.Render.GetForegroundColor())
				.Append("|")
				.Append(Text)
				.Append("}}")
				.Append("\"\n");
		}
	}
}
