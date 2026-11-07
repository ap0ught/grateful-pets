using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Collections;
using XRL.Language;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Text;
using XRL.World.Text.Delegates;

namespace XRL;

public static class GameText
{
	private class ProcessContext : IDisposable
	{
		private static RingDeque<ProcessContext> Repository = new RingDeque<ProcessContext>();

		public int BufferCapacity = 64;

		public char[] Buffer = new char[64];

		public List<string> ReplaceParameters = new List<string>();

		public List<ReplacerEntry> PostProcessors = new List<ReplacerEntry>();

		public StringBuilder ValueBuilder = new StringBuilder(128);

		public static ProcessContext Get()
		{
			if (Repository.TryEject(out var Value))
			{
				return Value;
			}
			return new ProcessContext();
		}

		public void Dispose()
		{
			Repository.Enqueue(this);
		}
	}

	private static StringMap<int> Targets = new StringMap<int>
	{
		{ "player", -2 },
		{ "subject", 0 },
		{ "pronouns", 0 },
		{ "objpronouns", 1 },
		{ "object", 1 }
	};

	public static string VariableReplace(string Message, string ExplicitSubject, bool ExplicitSubjectPlural = false, string ExplicitObject = null, bool ExplicitObjectPlural = false, bool StripColors = false)
	{
		if (Message.IsNullOrEmpty())
		{
			return Message;
		}
		return Message.StartReplace().AddExplicit(ExplicitSubject, ExplicitSubjectPlural).AddExplicit(ExplicitObject, ExplicitObjectPlural)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(StringBuilder Message, string ExplicitSubject, bool ExplicitSubjectPlural = false, string ExplicitObject = null, bool ExplicitObjectPlural = false, bool StripColors = false)
	{
		return Message.StartReplace().AddExplicit(ExplicitSubject, ExplicitSubjectPlural).AddExplicit(ExplicitObject, ExplicitObjectPlural)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(string Message, TextArgument Subject, TextArgument Object = default(TextArgument), bool StripColors = false)
	{
		if (Message.IsNullOrEmpty())
		{
			return Message;
		}
		return Message.StartReplace().AddArgument(Subject).AddArgument(Object)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(StringBuilder Message, TextArgument Subject, TextArgument Object = default(TextArgument), bool StripColors = false)
	{
		return Message.StartReplace().AddArgument(Subject).AddArgument(Object)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(StringBuilder Message, TextArgument Object1, TextArgument Object2, TextArgument Object3, TextArgument Object4 = default(TextArgument), TextArgument Object5 = default(TextArgument), int DefaultObject = -1, bool StripColors = false)
	{
		return Message.StartReplace().SetDefaultArgument(DefaultObject).AddArgument(Object1)
			.AddArgument(Object2)
			.AddArgument(Object3)
			.AddArgument(Object4)
			.AddArgument(Object5)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(string Message, GameObject Subject, GameObject Object = null, bool StripColors = false)
	{
		if (Message.IsNullOrEmpty())
		{
			return Message;
		}
		return Message.StartReplace().AddObject(Subject).AddObject(Object)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(StringBuilder Message, GameObject Subject, GameObject Object = null, bool StripColors = false)
	{
		return Message.StartReplace().AddObject(Subject).AddObject(Object)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(StringBuilder Message, GameObject Object1, GameObject Object2, GameObject Object3, GameObject Object4 = null, GameObject Object5 = null, int DefaultObject = -1, bool StripColors = false)
	{
		return Message.StartReplace().SetDefaultArgument(DefaultObject).AddObject(Object1)
			.AddObject(Object2)
			.AddObject(Object3)
			.AddObject(Object4)
			.AddObject(Object5)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(StringBuilder Message, GameObject Subject, string ExplicitSubject, bool ExplicitSubjectPlural, GameObject Object, string ExplicitObject, bool ExplicitObjectPlural, bool StripColors = false)
	{
		return Message.StartReplace().AddArgument(new TextArgument(Subject, ExplicitSubject, ExplicitSubjectPlural)).AddArgument(new TextArgument(Object, ExplicitObject, ExplicitObjectPlural))
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(string Message, string ExplicitSubject, bool ExplicitSubjectPlural, GameObject Object, bool StripColors = false)
	{
		if (Message.IsNullOrEmpty())
		{
			return Message;
		}
		return Message.StartReplace().AddExplicit(ExplicitSubject, ExplicitSubjectPlural).AddObject(Object)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(string Message, GameObject Subject, string ExplicitObject, bool ExplicitObjectPlural, bool StripColors = false)
	{
		if (Message.IsNullOrEmpty())
		{
			return Message;
		}
		return Message.StartReplace().AddObject(Subject).AddExplicit(ExplicitObject, ExplicitObjectPlural)
			.StripColors(StripColors)
			.ToString();
	}

	public static string VariableReplace(string Message, bool StripColors = false)
	{
		if (Message.IsNullOrEmpty())
		{
			return Message;
		}
		return Message.StartReplace().StripColors(StripColors).ToString();
	}

	public static string VariableReplace(StringBuilder Message, bool StripColors = false)
	{
		Process(Message, null, null, null, -1, StripColors);
		return Message.ToString();
	}

	public static void Process(ref string Message, StringMap<ReplacerEntry> Replacers = null, StringMap<int> Aliases = null, IList<TextArgument> Arguments = null, int DefaultArgument = -1, bool StripColors = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder(Message);
		Process(stringBuilder, Replacers, Aliases, Arguments, DefaultArgument, StripColors);
		Message = stringBuilder.ToString();
	}

	public static void Process(StringBuilder Message, StringMap<ReplacerEntry> Replacers = null, StringMap<int> Aliases = null, IList<TextArgument> Arguments = null, int DefaultArgument = -1, bool StripColors = false)
	{
		if (!VariableReplacers.Initialized)
		{
			VariableReplacers.LoadReplacers();
		}
		using ProcessContext processContext = ProcessContext.Get();
		StringBuilder valueBuilder = processContext.ValueBuilder;
		List<string> replaceParameters = processContext.ReplaceParameters;
		List<ReplacerEntry> postProcessors = processContext.PostProcessors;
		StringMap<ReplacerEntry> map = VariableReplacers.Map;
		StringMap<ReplacerEntry> postMap = VariableReplacers.PostMap;
		char[] array = processContext.Buffer;
		int num = processContext.BufferCapacity;
		int num2 = Arguments?.Count ?? 0;
		TextArgument textArgument = ((DefaultArgument >= 0) ? Arguments[DefaultArgument] : default(TextArgument));
		char c = '\0';
		int num3 = -1;
		int num4 = 0;
		int num5 = -1;
		int num6 = -1;
		int num7 = -1;
		int num8 = -1;
		int num9 = -1;
		int i = 0;
		for (int length = Message.Length; i < length; i++)
		{
			c = Message[i];
			if (num5 == -1)
			{
				if (c == '=')
				{
					num6 = (num5 = i);
					num8 = (num7 = (num3 = (num9 = -1)));
					replaceParameters.Clear();
					postProcessors.Clear();
				}
				continue;
			}
			num4 = i - num5 - 1;
			if (num4 >= num)
			{
				char[] array2 = new char[num * 2];
				Array.Copy(array, 0, array2, 0, num);
				array = (processContext.Buffer = array2);
				num = (processContext.BufferCapacity = num * 2);
			}
			array[num4] = c;
			switch (c)
			{
			case '\n':
			case ' ':
				if (num7 == -1)
				{
					num5 = -1;
				}
				break;
			case ':':
				if (num7 == -1)
				{
					if (num9 == -1)
					{
						num9 = i;
					}
					num7 = i + 1;
				}
				else
				{
					replaceParameters.Add(new string(array, num7 - num5 - 1, i - num7));
					num7 = i + 1;
				}
				break;
			case '|':
				if (num8 == -1)
				{
					if (num7 != -1)
					{
						replaceParameters.Add(new string(array, num7 - num5 - 1, i - num7));
						num7 = -1;
					}
					else if (num9 == -1)
					{
						num9 = i;
					}
					num8 = i + 1;
				}
				else
				{
					Span<char> span2 = array.AsSpan(num8 - num5 - 1, i - num8);
					if (postMap.TryGetValue(span2, out var Value3))
					{
						postProcessors.Add(Value3);
					}
					else
					{
						MetricsManager.LogError("No variable post processor by key '" + new string(span2) + "' found.");
					}
					num8 = i + 1;
				}
				break;
			case '.':
				if (num3 == -1)
				{
					num3 = -3;
					if (TryParseTarget(array, Aliases, num4, out var Index))
					{
						num3 = Index;
						num6 = i;
					}
				}
				break;
			case '=':
			{
				if (num8 != -1)
				{
					Span<char> span = array.AsSpan(num8 - num5 - 1, i - num8);
					if (postMap.TryGetValue(span, out var Value))
					{
						postProcessors.Add(Value);
					}
					else
					{
						MetricsManager.LogError("No variable post processor by key '" + new string(span) + "' found.");
					}
					num8 = -1;
				}
				else if (num7 != -1)
				{
					replaceParameters.Add(new string(array, num7 - num5 - 1, i - num7));
					num7 = -1;
				}
				else if (num9 == -1)
				{
					num9 = i;
				}
				if (num6 >= num9 - 1)
				{
					num5 = -1;
					break;
				}
				ReadOnlySpan<char> readOnlySpan = array.AsSpan(num6 - num5, num9 - num6 - 1);
				TargetType type = TargetType.None;
				TextArgument textArgument2;
				if (num3 == -2)
				{
					textArgument2 = new TextArgument(The.Player);
					GameObject gameObject = textArgument2.Object;
					textArgument2.Pronouns = ((gameObject != null && gameObject.IsPlural) ? PronounSet.DefaultPlayerPlural : PronounSet.DefaultPlayer);
					type = TargetType.Player;
				}
				else if (num3 < 0)
				{
					textArgument2 = textArgument;
				}
				else if (num3 >= num2)
				{
					MetricsManager.LogWarning($"Object index {num3} for key '{new string(readOnlySpan)}' is out of bounds.");
					textArgument2 = default;
				}
				else
				{
					textArgument2 = Arguments[num3];
					type = TargetType.Object;
				}
				if ((Replacers != null && Replacers.TryGetValue(readOnlySpan, out var Value2)) || map.TryGetValue(readOnlySpan, out Value2))
				{
					valueBuilder.Clear();
					DelegateContext context = DelegateContext.Set(valueBuilder, replaceParameters, textArgument2.Object, textArgument2.Pronouns, type, textArgument2.Explicit, Value2.Default, Value2.Capitalize, Value2.Flags);
					string text = Value2.Delegate(context) ?? valueBuilder.ToString();
					int count = postProcessors.Count;
					if (count > 0)
					{
						valueBuilder.Clear();
						valueBuilder.Append(text);
						for (int j = 0; j < count; j++)
						{
							ReplacerEntry replacerEntry = postProcessors[j];
							context = DelegateContext.Set(valueBuilder, replaceParameters, textArgument2.Object, textArgument2.Pronouns, type, textArgument2.Explicit, replacerEntry.Default, replacerEntry.Capitalize, replacerEntry.Flags);
							replacerEntry.Delegate(context);
						}
						text = valueBuilder.ToString();
					}
					Message.Remove(num5, i - num5 + 1);
					Message.Insert(num5, text);
					i = num5 - 1 + text.Length;
					length = Message.Length;
				}
				else
				{
					MetricsManager.LogError("No variable replacer by key '" + new string(readOnlySpan) + "' found.");
				}
				num5 = -1;
				break;
			}
			}
		}
		if (StripColors)
		{
			ColorUtility.StripFormatting(Message);
		}
	}

	private static bool TryParseTarget(char[] Buffer, StringMap<int> Aliases, int Position, out int Index)
	{
		if (Buffer[Position - 1] == ']')
		{
			int num = Array.LastIndexOf(Buffer, '[', Position - 3);
			if (num != -1)
			{
				Span<char> span = Buffer.AsSpan(0, num);
				Span<char> span2 = Buffer.AsSpan(num, Position - num);
				if (Targets.ContainsKey(span) && int.TryParse(span2, out var result))
				{
					Index = result;
					return true;
				}
			}
		}
		else
		{
			Span<char> span3 = Buffer.AsSpan(0, Position);
			if ((Aliases != null && Aliases.TryGetValue(span3, out var Value)) || Targets.TryGetValue(span3, out Value))
			{
				Index = Value;
				return true;
			}
		}
		Index = -1;
		return false;
	}

	public static string GenerateMarkovMessageParagraph()
	{
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		return MarkovChain.GenerateParagraph(MarkovBook.CorpusData[text]);
	}

	public static string GenerateMarkovMessageSentence()
	{
		string text = "LibraryCorpus.json";
		MarkovBook.EnsureCorpusLoaded(text);
		return MarkovChain.GenerateSentence(MarkovBook.CorpusData[text]);
	}

	public static string RoughConvertSecondPersonToThirdPerson(string text, GameObject who)
	{
		if (text.IsNullOrEmpty())
		{
			return null;
		}
		if (!text.StartsWith("You were"))
		{
			text = ((!text.StartsWith("You")) ? (who.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " " + Grammar.InitLower(text)) : text.Replace("You", who.It));
		}
		else
		{
			string text2 = ((The.Player != null) ? ("by " + The.Player.ShortDisplayName + ".") : null);
			if (text2 != null && text.EndsWith(text2))
			{
				text = text.Substring(0, text.Length - text2.Length);
				text = ((!text.Contains(" to death ")) ? (text.Replace("You were", "You") + who.them + ".") : (text.Replace(" to death ", " ").Replace("You were", "You") + who.them + " to death."));
			}
			else
			{
				text = text.Replace("You were", who.Does("were", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true));
			}
		}
		if (text.Contains("yourself"))
		{
			text = text.Replace("yourself", who.itself);
		}
		if (text.Contains("yourselves"))
		{
			text = text.Replace("yourselves", who.itself);
		}
		if (text.Contains(" caused by "))
		{
			string text3 = ((The.Player != null) ? (" caused by " + The.Player.ShortDisplayName + ".") : null);
			if (text3 != null && text.EndsWith(text3))
			{
				text = text.Replace(text3, ", which you caused.");
			}
		}
		return text;
	}

	public static ReplaceBuilder StartReplace(this StringBuilder Text)
	{
		return ReplaceBuilder.Get().Start(Text);
	}

	public static ReplaceBuilder StartReplace(this string Text)
	{
		return ReplaceBuilder.Get().Start(Text);
	}
}
