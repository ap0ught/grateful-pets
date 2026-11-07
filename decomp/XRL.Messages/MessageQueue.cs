using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.UI;
using XRL.World;

namespace XRL.Messages;

[Serializable]
public class MessageQueue : IComposite
{
	public static bool Suppress = false;

	public List<string> Messages = new List<string>(8192);

	public int PreviousMessage;

	public int LastMessage;

	public int LastTurnMessages = -1;

	public bool Terse;

	[NonSerialized]
	private static char[] splitterNewline = new char[1] { '\n' };

	[NonSerialized]
	public static Queue<string> UnityMessages = new Queue<string>(32);

	public bool Cache_0_12Valid;

	private StringBuilder Cache_0_12 = new StringBuilder(1024);

	public static bool Enabled
	{
		get
		{
			if (Suppress)
			{
				return false;
			}
			if (XRLCore.Core == null)
			{
				return false;
			}
			if (XRLCore.Core.Game == null)
			{
				return false;
			}
			if (XRLCore.Core.Game.Player == null)
			{
				return false;
			}
			if (XRLCore.Core.Game.Player.Messages == null)
			{
				return false;
			}
			return true;
		}
	}

	public string LastLine
	{
		get
		{
			if (Messages.Count <= 0)
			{
				return null;
			}
			return Messages[Messages.Count - 1];
		}
	}

	public void Show()
	{
		if (Options.ModernUI)
		{
			_ = StatusScreensScreen.show(7, The.Game.Player.Body).Result;
		}
		else
		{
			BookUI.ShowBook(GetLines(0, Messages.Count).ToString(), "Message Log");
		}
	}

	public void EndPlayerTurn()
	{
	}

	public void BeginPlayerTurn()
	{
		LastMessage = PreviousMessage;
		PreviousMessage = Messages.Count;
		if (Messages.Count > 2000)
		{
			Messages.RemoveRange(0, 100);
			LastMessage -= 100;
			PreviousMessage -= 100;
		}
	}

	public static void AddPlayerMessage(string Message, string Color = null, bool Capitalize = true)
	{
		try
		{
			if (Enabled && Message != null)
			{
				if (Capitalize)
				{
					Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
				}
				if (!Color.IsNullOrEmpty())
				{
					StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
					stringBuilder.Append("{{").Append(Color).Append('|')
						.Append(Message)
						.Append("}}");
					Message = stringBuilder.ToString();
				}
				XRLCore.Core.Game.Player.Messages.Add(Message);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void AddPlayerMessage(string Message, char Color, bool Capitalize = true)
	{
		AddPlayerMessage(Message, (Color == ' ') ? null : (Color.ToString() ?? ""), Capitalize);
	}

	public void Add(string Message)
	{
		if (!Suppress)
		{
			Cache_0_12Valid = false;
			Message = Markup.Transform(Message);
			if (Message != "!clear")
			{
				Messages.Add(Message);
			}
			XRLCore.CallNewMessageLogEntryCallbacks(Message);
		}
	}

	public List<string> GetLinesList(int StartingLine, int nLines)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 1;
		if (StartingLine < 0)
		{
			StartingLine = 0;
		}
		int num2 = Messages.Count - 1 - StartingLine;
		while (num2 >= 0 && num < nLines)
		{
			if (num2 < LastMessage)
			{
				stringBuilder.Append("&K^k");
			}
			else
			{
				stringBuilder.Append("&k^Y");
			}
			if (Messages[num2].Length > 0 && Messages[num2][0] == '#')
			{
				stringBuilder.Append("#^k &y").Append(Messages[num2].Substring(1)).Append("\n");
			}
			else
			{
				stringBuilder.Append(">^k &y");
				stringBuilder.Append(Messages[num2]);
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
			}
			num2--;
			num++;
		}
		return list;
	}

	public StringBuilder GetLines(int StartingLine, int nLines)
	{
		if (Cache_0_12Valid && StartingLine == 0 && nLines == 12)
		{
			return Cache_0_12;
		}
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		int num = 1;
		if (StartingLine < 0)
		{
			StartingLine = 0;
		}
		int num2 = Messages.Count - 1 - StartingLine;
		while (num2 >= 0 && num < nLines)
		{
			if (num2 < LastMessage)
			{
				stringBuilder.Append("&K^k");
			}
			else
			{
				stringBuilder.Append("&k^Y");
			}
			if (Messages[num2].Length > 0 && Messages[num2][0] == '#')
			{
				stringBuilder.Append("#^k &y").Append(Messages[num2], 1, Messages[num2].Length - 1).Append('\n');
			}
			else
			{
				stringBuilder.Append(">^k &y").Append(Messages[num2]).Append('\n');
			}
			num2--;
			num++;
		}
		if (StartingLine == 0 && nLines == 12)
		{
			Cache_0_12Valid = true;
			Cache_0_12 = stringBuilder;
			return Cache_0_12;
		}
		return stringBuilder;
	}

	public override string ToString()
	{
		return GetLines(0, Messages.Count).ToString();
	}
}
