using System.Collections.Generic;
using System.Text;

namespace XRL.UI;

public class BookPage
{
	public string Format = "Manual";

	public int LeftMargin = 2;

	public int RightMargin = 2;

	public int TopMargin;

	public int BottomMargin = 2;

	public string Title;

	public List<string> Lines = new List<string>();

	public string FullText
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string line in Lines)
			{
				stringBuilder.Append(line);
				stringBuilder.Append("\n");
			}
			return stringBuilder.ToString();
		}
	}

	public string RenderForModernUI
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string line in Lines)
			{
				stringBuilder.Append(line);
			}
			return stringBuilder.ToString();
		}
	}

	public BookPage(string Title, string Data)
	{
		string[] array = Data.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = StringFormat.ClipText(array[i], 77, KeepNewlines: true).Split('\n');
			foreach (string text in array2)
			{
				Lines.Add(text.Replace("\r", "") + "\n");
			}
		}
		this.Title = Title;
	}
}
