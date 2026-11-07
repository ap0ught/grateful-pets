using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;

namespace XRL.World.Parts;

public class IBookContents
{
	[Serializable]
	public class BookPageInfo : IComposite
	{
		public string Title;

		public string Text;

		public string Format;

		public string Margins = "2,2,2,2";
	}

	public void AppendCatalogLine(string Left, string Right, string Separator, int Width, StringBuilder SB)
	{
		int num = Width - ColorUtility.LengthExceptFormatting(Left) - ColorUtility.LengthExceptFormatting(Right);
		SB.Append(Left);
		int num2 = ColorUtility.LengthExceptFormatting(Separator);
		for (int i = 0; i < num; i += num2)
		{
			SB.Append(Separator);
		}
		SB.Append(Right);
	}

	public virtual string GetTitle()
	{
		throw new NotImplementedException();
	}

	public virtual List<BookPageInfo> GetContents()
	{
		throw new NotImplementedException();
	}
}
