using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AiUnity.NLog.Core.Config;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.LayoutRenderers.Wrappers;

[LayoutRenderer("replace", true)]
[ThreadAgnostic]
[Preserve]
public sealed class ReplaceLayoutRendererWrapper : WrapperLayoutRendererBase
{
	[ThreadAgnostic]
	public class Replacer
	{
		private readonly string text;

		private readonly string replaceGroupName;

		private readonly string replaceWith;

		internal Replacer(string text, string replaceGroupName, string replaceWith)
		{
			this.text = text;
			this.replaceGroupName = replaceGroupName;
			this.replaceWith = replaceWith;
		}

		internal string EvaluateMatch(Match match)
		{
			return ReplaceNamedGroup(text, replaceGroupName, replaceWith, match);
		}
	}

	private Regex regex;

	public string SearchFor { get; set; }

	public bool Regex { get; set; }

	public string ReplaceWith { get; set; }

	public string ReplaceGroupName { get; set; }

	public bool IgnoreCase { get; set; }

	public bool WholeWords { get; set; }

	protected override void InitializeLayoutRenderer()
	{
		base.InitializeLayoutRenderer();
		string text = SearchFor;
		if (!Regex)
		{
			text = System.Text.RegularExpressions.Regex.Escape(text);
		}
		RegexOptions regexOptions = RegexOptions.None;
		if (IgnoreCase)
		{
			regexOptions |= RegexOptions.IgnoreCase;
		}
		if (WholeWords)
		{
			text = "\\b" + text + "\\b";
		}
		regex = new Regex(text, regexOptions);
	}

	protected override string Transform(string text)
	{
		Replacer replacer = new Replacer(text, ReplaceGroupName, ReplaceWith);
		if (!string.IsNullOrEmpty(ReplaceGroupName))
		{
			return regex.Replace(text, replacer.EvaluateMatch);
		}
		return regex.Replace(text, ReplaceWith);
	}

	public static string ReplaceNamedGroup(string input, string groupName, string replacement, Match match)
	{
		StringBuilder stringBuilder = new StringBuilder(input);
		int index = match.Index;
		int num = match.Length;
		foreach (Capture item in from c in match.Groups[groupName].Captures.OfType<Capture>()
			orderby c.Index descending
			select c)
		{
			if (item != null)
			{
				num += replacement.Length - item.Length;
				stringBuilder.Remove(item.Index, item.Length);
				stringBuilder.Insert(item.Index, replacement);
			}
		}
		int num2 = index + num;
		stringBuilder.Remove(num2, stringBuilder.Length - num2);
		stringBuilder.Remove(0, index);
		return stringBuilder.ToString();
	}
}
