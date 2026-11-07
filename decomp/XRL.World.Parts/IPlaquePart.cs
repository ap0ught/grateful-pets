using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public abstract class IPlaquePart : IPart
{
	[NonSerialized]
	protected string _Inscription;

	public virtual string EdgeColor => "^k&y";

	public virtual string CornerColor => "&Y^y";

	public virtual string DarkColor => "&y^k";

	public virtual int MaxWidth => 26;

	public abstract string BaseInscription { get; }

	public virtual string Inscription
	{
		get
		{
			if (_Inscription != null)
			{
				return _Inscription;
			}
			List<string> list = new List<string>(16);
			list.Add("");
			string[] array = StringFormat.ClipText(BaseInscription, MaxWidth).Split('\n');
			foreach (string item in array)
			{
				list.Add(item);
			}
			list.Add("");
			string edgeColor = EdgeColor;
			string cornerColor = CornerColor;
			string darkColor = DarkColor;
			StringBuilder stringBuilder = Event.NewStringBuilder().Append('ÿ', 3).Append(cornerColor)
				.Append('\a')
				.Append(edgeColor)
				.Append('Ä', 31)
				.Append(cornerColor)
				.Append('\a')
				.Append(edgeColor);
			for (int j = 0; j < list.Count; j++)
			{
				stringBuilder.Append('\n').Append('ÿ', 3);
				stringBuilder.Append(edgeColor).Append('³').Append(edgeColor);
				int num = ColorUtility.LengthExceptFormatting(list[j]);
				stringBuilder.Append(list[j].PadLeft(16 + num / 2, 'ÿ').PadRight(31, 'ÿ'));
				stringBuilder.Append(edgeColor).Append('³').Append(edgeColor);
			}
			stringBuilder.Append('\n').Append('ÿ', 3).Append(cornerColor)
				.Append('\a')
				.Append(edgeColor)
				.Append(darkColor)
				.Append('Ä', 31)
				.Append(cornerColor)
				.Append('\a')
				.Append(edgeColor);
			return _Inscription = Event.FinalizeString(stringBuilder);
		}
	}
}
