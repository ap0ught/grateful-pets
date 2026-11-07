using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RecomingPlaque : IPart
{
	private const string RecomingWords = "\"Unwrap thyself of thy grave clothes, and go.\"";

	public string Inscription = "";

	public string Prefix = "";

	public string Postfix = "";

	public bool NeedsGeneration = true;

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
			GeneratePlaque();
		}
		E.Prefix.Append(Prefix);
		E.Base.Clear();
		E.Base.Append(Inscription);
		E.Postfix.Append(Postfix);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void GeneratePlaque()
	{
		List<string> list = new List<string>(16);
		int maxWidth = 28;
		list.Add("");
		list.Add("");
		list.Add("");
		string[] array = StringFormat.ClipText("\"Unwrap thyself of thy grave clothes, and go.\"", maxWidth).Split('\n');
		foreach (string item in array)
		{
			list.Add(item);
		}
		list.Add("");
		list.Add("");
		list.Add("");
		string text = "^k&m";
		string text2 = "&M^m";
		string text3 = "m";
		Inscription = Event.NewStringBuilder().Append('ÿ').Append('ÿ')
			.Append('ÿ')
			.Append(text2 + "\a" + text)
			.Append('Ä', 31)
			.Append(text2 + "\a^k&y")
			.ToString();
		for (int j = 0; j < list.Count; j++)
		{
			Inscription += "\nÿÿÿ";
			Inscription = Inscription + text + "³^k&y";
			int num = ColorUtility.LengthExceptFormatting(list[j]);
			Inscription += list[j].PadLeft(16 + num / 2, 'ÿ').PadRight(31, 'ÿ');
			Inscription = Inscription + text + "³^k&y";
		}
		Inscription += Event.NewStringBuilder().Append("\n").Append('ÿ')
			.Append('ÿ')
			.Append('ÿ')
			.Append(text2 + "\a" + text)
			.Append("&" + text3 + "^k")
			.Append('Ä', 31)
			.Append(text2 + "\a^k&y")
			.ToString();
		NeedsGeneration = false;
	}
}
