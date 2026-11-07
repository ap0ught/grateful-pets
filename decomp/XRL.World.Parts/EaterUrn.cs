using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HistoryKit;
using XRL.Names;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class EaterUrn : IPart
{
	public string Inscription;

	public string Introduction;

	public string Eulogy;

	public string Prefix;

	public string Postfix;

	public string Reason;

	public bool NeedsGeneration = true;

	public string Faction = "Eater";

	public string MemorializedName;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
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
			GenerateUrn();
		}
		if (!Prefix.IsNullOrEmpty())
		{
			E.Prefix.Append(Prefix);
		}
		if (!Inscription.IsNullOrEmpty())
		{
			E.Base.Append(Inscription);
		}
		if (!Postfix.IsNullOrEmpty())
		{
			E.Postfix.Append(Postfix);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void GenerateUrn()
	{
		NeedsGeneration = false;
		if (MemorializedName.IsNullOrEmpty())
		{
			if (Faction == "Eater")
			{
				MemorializedName = NameMaker.Eater();
			}
			else if (Faction == "YdFreehold")
			{
				MemorializedName = NameMaker.YdFreeholder();
			}
			else if (!Faction.IsNullOrEmpty())
			{
				MemorializedName = NameMaker.MakeName(null, null, null, null, null, Faction);
			}
			else
			{
				MemorializedName = MutantNameMaker.MakeMutantName();
			}
		}
		if (Introduction.IsNullOrEmpty())
		{
			if (Faction == "Eater")
			{
				Introduction = HistoricStringExpander.ExpandString("<spice.tombstones.eaterUrnIntro.!random>");
			}
			else if (Faction == "YdFreehold")
			{
				Introduction = HistoricStringExpander.ExpandString("<spice.tombstones.ydFreeholderUrnIntro.!random>");
			}
			else if (Faction == "Barathrumites")
			{
				Introduction = HistoricStringExpander.ExpandString("<spice.tombstones.barathrumiteUrnIntro.!random>");
			}
			else
			{
				Introduction = HistoricStringExpander.ExpandString("<spice.tombstones.genericUrnIntro.!random>");
			}
		}
		if (Eulogy.IsNullOrEmpty())
		{
			string text = "LibraryCorpus.json";
			MarkovBook.EnsureCorpusLoaded(text);
			MarkovChainData data = MarkovBook.CorpusData[text];
			Eulogy = "\"" + MarkovChain.GenerateShortSentence(data, null, 12).TrimEnd(' ') + "\"";
		}
		List<string> list = new List<string>(16);
		int maxWidth = 25;
		list.Add("");
		string[] array = StringFormat.ClipText(Introduction, maxWidth).Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		list.Add("");
		list.Add("");
		array = StringFormat.ClipText(MemorializedName, maxWidth).Split('\n');
		foreach (string item2 in array)
		{
			list.Add(item2);
		}
		list.Add("");
		list.Add("");
		if (!Reason.IsNullOrEmpty())
		{
			string text2 = Reason.Strip();
			int num = text2.IndexOf("@@");
			if (num != -1)
			{
				text2 = text2.Substring(num + 2).Capitalize();
			}
			if (text2.Contains("##"))
			{
				text2 = (80.in100() ? text2.Replace("##", "") : Regex.Replace(text2, "##.*?##", ""));
			}
			if (text2.Contains("by you ") || text2.Contains("by you."))
			{
				text2 = text2.Replace("by you", "by " + The.Player.GetReferenceDisplayName(int.MaxValue, null, "MemorialKiller", NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true));
			}
			array = StringFormat.ClipText(text2, maxWidth).Split('\n');
			foreach (string item3 in array)
			{
				list.Add(item3);
			}
			list.Add("");
			list.Add("");
		}
		array = StringFormat.ClipText(Eulogy, maxWidth).Split('\n');
		foreach (string item4 in array)
		{
			list.Add(item4);
		}
		list.Add("");
		Inscription = "";
		for (int j = 0; j < list.Count; j++)
		{
			Inscription += "\nÿÿÿ";
			if (j % 2 == 0)
			{
				Inscription += " ";
			}
			else
			{
				Inscription += " ";
			}
			Inscription += list[j].PadLeft(17 + (list[j].Length / 2 - 1), 'ÿ').PadRight(31, 'ÿ');
			if (j % 2 == 0)
			{
				Inscription += " ";
			}
			else
			{
				Inscription += " ";
			}
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('/');
		stringBuilder.Append('-', 27);
		stringBuilder.Append('\\');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append("\n");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append(" &K");
		stringBuilder.Append('_', 31);
		stringBuilder.Append(" &y");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append("\n");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append(" &K");
		stringBuilder.Append('_', 31);
		stringBuilder.Append(" &y");
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append('ÿ');
		stringBuilder.Append("\n");
		Prefix = stringBuilder.ToString();
		StringBuilder stringBuilder2 = Event.NewStringBuilder();
		for (int k = 0; k < 2; k++)
		{
			stringBuilder2.Append("\n");
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append(" &K");
			stringBuilder2.Append('_', 21);
			stringBuilder2.Append(" &y");
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
			stringBuilder2.Append('ÿ');
		}
		Inscription += "\n";
		Inscription += stringBuilder2.ToString();
	}
}
