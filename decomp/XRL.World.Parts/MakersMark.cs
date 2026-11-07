using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
[WantLoadBlueprint]
public class MakersMark : IPart
{
	public string Mark;

	public string CrafterName;

	public string Color;

	[NonSerialized]
	private string MarkText;

	public static void LoadBlueprint(GameObjectBlueprint Blueprint)
	{
		if (Blueprint.TryGetPartParameter<string>("MakersMark", "Mark", out var Result) && !Result.IsNullOrEmpty())
		{
			RecordUsage(Result);
		}
	}

	public override bool SameAs(IPart p)
	{
		MakersMark makersMark = p as MakersMark;
		if (makersMark.Mark != Mark)
		{
			return false;
		}
		if (makersMark.CrafterName != CrafterName)
		{
			return false;
		}
		if (makersMark.Color != Color)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == GetUnknownShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Mark", Mark);
		E.AddEntry(this, "Mark.Length", Mark.Length);
		E.AddEntry(this, "CrafterName", CrafterName);
		E.AddEntry(this, "Color", Color);
		E.AddEntry(this, "MarkText", GetMarkText());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddMark(GetMarkText());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Compound(GetDescText(), '\n');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		E.Postfix.Compound(GetDescText(), '\n');
		return base.HandleEvent(E);
	}

	private string GetMarkText()
	{
		return MarkText ?? (MarkText = BuildMarkText());
	}

	private string BuildMarkText()
	{
		if (Mark.Length <= 1)
		{
			return Mark.Color(Color);
		}
		List<string> list = Color.CachedDoubleSemicolonExpansion();
		if (list.Count != Mark.Length)
		{
			return Mark;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int i = 0;
		for (int length = Mark.Length; i < length; i++)
		{
			stringBuilder.Append("{{").Append(list[i]).Append('|')
				.Append(Mark[i])
				.Append("}}");
		}
		return stringBuilder.ToString();
	}

	private string GetDescText()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(GetMarkText()).Append("{{C|: ").Append(GetDescriptionFor(ParentObject, CrafterName, Mark))
			.Append("}}");
		return stringBuilder.ToString();
	}

	public static string GetDescriptionFor(GameObject Object, string CrafterName, string Mark = null)
	{
		string text = CrafterName ?? "a crafter";
		if (text.Contains(";;"))
		{
			text = Grammar.MakeAndList(text.CachedDoubleSemicolonExpansion());
		}
		return Object.IndicativeProximal + " " + (Object.IsPlural ? Grammar.Pluralize(Object.GetDescriptiveCategory()) : Object.GetDescriptiveCategory()) + Object.GetVerb("bear") + " the " + ((Mark != null && Mark.Length > 1) ? "marks" : "mark") + " of " + text + ".";
	}

	public static string GetDescriptionFor(GameObject Object, GameObject Actor)
	{
		return GetDescriptionFor(Object, NameForMark(Actor));
	}

	public static string NameForMark(GameObject Object)
	{
		return Object?.GetReferenceDisplayName(int.MaxValue, null, "MakersMark", NoColor: false, Stripped: true);
	}

	public bool AddCrafter(GameObject Crafter, string CrafterMark, string MarkColor)
	{
		if (!GameObject.Validate(ref Crafter) || CrafterMark.IsNullOrEmpty() || MarkColor.IsNullOrEmpty())
		{
			return false;
		}
		if (Mark.IsNullOrEmpty())
		{
			Mark = CrafterMark;
		}
		else
		{
			if (Mark.Contains(CrafterMark))
			{
				return false;
			}
			Mark += CrafterMark;
		}
		if (Color.IsNullOrEmpty())
		{
			Color = MarkColor;
		}
		else
		{
			Color = Color + ";;" + MarkColor;
		}
		string text = NameForMark(Crafter);
		if (CrafterName.IsNullOrEmpty())
		{
			CrafterName = text;
		}
		else
		{
			CrafterName = CrafterName + ";;" + text;
		}
		MarkText = null;
		return true;
	}

	public static string Generate(bool RecordUse = true)
	{
		int num = 0;
		int num2;
		do
		{
			num2 = Stat.Random(1, 254);
		}
		while (num2 == 94 || num2 == 38 || num2 == 124 || num2 == 44 || num2 == 123 || num2 == 125 || num2 == 32 || num2 == 10 || char.IsLetter((char)num2) || char.IsDigit((char)num2) || (The.Game.GetBooleanGameState("MakersMarkUsed_" + (char)num2) && ++num < 32));
		string text = ((char)num2).ToString() ?? "";
		if (RecordUse)
		{
			RecordUsage(text);
		}
		return text;
	}

	public static List<string> GetUsable(bool SkipUsed = true, bool IncludeUsedIfNoneLeft = true)
	{
		List<string> list = new List<string>(256);
		while (true)
		{
			for (int i = 1; i < 255; i++)
			{
				if (i != 94 && i != 38 && i != 124 && i != 44 && i != 123 && i != 125 && i != 32 && i != 10 && !char.IsLetter((char)i) && !char.IsDigit((char)i) && (!SkipUsed || !The.Game.GetBooleanGameState("MakersMarkUsed_" + (char)i)))
				{
					list.Add(((char)i).ToString() ?? "");
				}
			}
			if (!(SkipUsed & IncludeUsedIfNoneLeft) || list.Count != 0)
			{
				break;
			}
			SkipUsed = false;
		}
		return list;
	}

	public static void RecordUsage(string Mark)
	{
		The.Game.SetBooleanGameState("MakersMarkUsed_" + Mark, Value: true);
	}
}
