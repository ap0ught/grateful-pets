using System;
using ConsoleLib.Console;
using UnityEngine;

namespace Qud.UI;

[Serializable]
public struct QudMenuItem
{
	public string text;

	public string command;

	public string hotkey;

	public IRenderable icon;

	public string simpleText
	{
		get
		{
			string text = this.text;
			if (text == null)
			{
				return text;
			}
			text = text.Strip();
			if (text.Contains(" ") && text.StartsWith("["))
			{
				text = text.Substring(text.IndexOf(" ") + 1);
			}
			return text.ToLower();
		}
	}

	public override string ToString()
	{
		return JsonUtility.ToJson(this);
	}

	public override int GetHashCode()
	{
		return text.GetHashCode() & command.GetHashCode() & hotkey.GetHashCode();
	}
}
