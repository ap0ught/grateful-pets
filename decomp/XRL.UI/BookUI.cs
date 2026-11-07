using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[HasGameBasedStaticCache]
[HasModSensitiveStaticCache]
[UIView("BookUI", false, false, false, null, null, false, 0, false)]
public class BookUI : IWantsTextConsoleInit
{
	public static int ScrollPosition;

	public static int IndexPosition;

	public static TextConsole Console;

	public static ScreenBuffer Buffer;

	public static Dictionary<string, string> BookCorpus = new Dictionary<string, string>();

	public static Dictionary<string, BookInfo> Books = new Dictionary<string, BookInfo>();

	public static List<string> DynamicBooks = new List<string>();

	public static readonly Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "books", HandleNodes },
		{ "book", HandleBookNode }
	};

	public static readonly Dictionary<string, Action<XmlDataHelper>> _BookNodes = new Dictionary<string, Action<XmlDataHelper>> { { "page", HandlePageNode } };

	private static bool IncludeCorpusData = false;

	private static StringBuilder CorpusBuilder = new StringBuilder();

	private static BookInfo CurrentBook;

	[GameBasedCacheInit]
	public static void Reset()
	{
		foreach (string dynamicBook in DynamicBooks)
		{
			Books.Remove(dynamicBook);
		}
		foreach (KeyValuePair<string, BookInfo> book in Books)
		{
			book.Value.Reset();
		}
		DynamicBooks.Clear();
	}

	[ModSensitiveCacheInit]
	public static void CacheInit()
	{
		Books.Clear();
		BookCorpus.Clear();
	}

	public void Init(TextConsole _Console, ScreenBuffer _Buffer)
	{
		Console = _Console;
		Buffer = _Buffer;
		InitBooks();
	}

	public static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(_Nodes);
	}

	public static void InitBooks(bool bIncludeCorpusData = false)
	{
		if (!Books.IsNullOrEmpty())
		{
			if (IncludeCorpusData == bIncludeCorpusData)
			{
				Reset();
				return;
			}
			Books.Clear();
			CorpusBuilder.Clear();
		}
		IncludeCorpusData = bIncludeCorpusData;
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("books"))
		{
			item.HandleNodes(_Nodes);
		}
	}

	public static void HandlePageNode(XmlDataHelper Reader)
	{
		string text = Markup.Transform(Reader.ReadString(), refreshAtNewline: true);
		if (IncludeCorpusData)
		{
			CorpusBuilder.Append(text).Append('\n');
		}
		text = text.Replace("[[", "").Replace("]]", "");
		CurrentBook.Texts.Add(text);
		Reader.DoneWithElement();
	}

	public static void ParseMargins(string Margins, ref int Top, ref int Right, ref int Bottom, ref int Left)
	{
		if (!Margins.IsNullOrEmpty())
		{
			Margins.AsDelimitedSpans(',', out var First, out var Second, out var Third, out var Fourth);
			if (int.TryParse(First, out var result))
			{
				Top = result;
			}
			if (int.TryParse(Second, out result))
			{
				Right = result;
			}
			if (int.TryParse(Third, out result))
			{
				Bottom = result;
			}
			if (int.TryParse(Fourth, out result))
			{
				Left = result;
			}
		}
	}

	public static void HandleBookNode(XmlDataHelper Reader)
	{
		string attribute = Reader.GetAttribute("ID");
		if (!Books.TryGetValue(attribute, out var value))
		{
			value = (Books[attribute] = new BookInfo());
			value.Texts = new List<string>();
		}
		else
		{
			value.Texts.Clear();
		}
		CurrentBook = value;
		value.Title = Reader.GetAttribute("Title");
		value.Format = Reader.GetAttribute("Format");
		ParseMargins(Reader.GetAttribute("Margins"), ref value.TopMargin, ref value.RightMargin, ref value.BottomMargin, ref value.LeftMargin);
		CorpusBuilder.Clear();
		Reader.HandleNodes(_BookNodes);
		if (IncludeCorpusData)
		{
			Match match = Regex.Match(CorpusBuilder.ToString(), "\\[\\[.*?\\]\\]");
			while (match != null && !string.IsNullOrEmpty(match.Value))
			{
				CorpusBuilder.Replace("[[" + match.Groups[0].Value + "]]", "");
				Debug.Log("Removing book header from corpus: " + match.Groups[0]?.ToString() + "...");
				match = match.NextMatch();
			}
			BookCorpus[attribute] = GameText.VariableReplace(CorpusBuilder);
		}
	}

	public static int NextWordLength(string Text, int Pos, StringBuilder Line, int LineWidth)
	{
		int num = 0;
		while (Pos < Text.Length)
		{
			if (Text[Pos] == ' ')
			{
				return num;
			}
			if (Text[Pos] == '\n')
			{
				return num;
			}
			if (Text[Pos] == '&' || Text[Pos] == '^')
			{
				Line.Append(Text[Pos]);
				Pos++;
				Line.Append(Text[Pos]);
			}
			else
			{
				num++;
				Line.Append(Text[Pos]);
			}
			Pos++;
		}
		return num;
	}

	public static bool NextLine(string Text, ref int Pos, StringBuilder Line, int LineWidth)
	{
		int num = 0;
		while (Pos < Text.Length)
		{
			if (Text[Pos] == '&' || Text[Pos] == '^')
			{
				Line.Append(Text[Pos]);
				Pos++;
				Line.Append(Text[Pos]);
			}
			else
			{
				num++;
				Line.Append(Text[Pos]);
			}
			Pos++;
			if (Pos < Text.Length)
			{
				if (Text[Pos] == '\n')
				{
					Line.Append('\n');
					Pos++;
					break;
				}
				if (num + NextWordLength(Text, Pos, Line, LineWidth) >= LineWidth)
				{
					Line.Append('\n');
					break;
				}
			}
		}
		if (Pos >= Text.Length)
		{
			return false;
		}
		return true;
	}

	public static List<BookPage> AutoformatPages(string Title, string Text, string Format, string Margins)
	{
		int Top = 2;
		int Right = 2;
		int Bottom = 2;
		int Left = 2;
		ParseMargins(Margins, ref Top, ref Right, ref Bottom, ref Left);
		return AutoformatPages(Title, Text, Format, Left, Right, Top, Bottom);
	}

	public static List<BookPage> AutoformatPages(string Title, string Text, string Format, int LeftMargin = 2, int RightMargin = 2, int TopMargin = 2, int BottomMargin = 2)
	{
		List<BookPage> list = new List<BookPage>();
		int maxWidth = 80 - LeftMargin - RightMargin;
		int num = 24 - TopMargin - BottomMargin;
		StringBuilder stringBuilder = new StringBuilder(1024);
		int MaxClippedWidth = 0;
		List<string> list2 = StringFormat.ClipTextToArray(GameText.VariableReplace(Text), maxWidth, out MaxClippedWidth, KeepNewlines: true);
		int num2 = 0;
		for (int i = 0; i < list2.Count; i++)
		{
			stringBuilder.Append(list2[i]);
			num2++;
			if (num2 >= num || i == list2.Count - 1)
			{
				num2 = 0;
				BookPage bookPage = new BookPage(Title, stringBuilder.ToString());
				bookPage.Format = Format;
				bookPage.LeftMargin = LeftMargin;
				bookPage.RightMargin = RightMargin;
				bookPage.TopMargin = TopMargin;
				bookPage.BottomMargin = BottomMargin;
				list.Add(bookPage);
				stringBuilder.Length = 0;
			}
			else
			{
				stringBuilder.Append("\n");
			}
		}
		return list;
	}

	public static void RenderDynamicBook(string BookID)
	{
		string text = BookID.Substring(1);
		Type type = ModManager.ResolveType("XRL.World.Parts." + text);
		object obj = Activator.CreateInstance(type);
		List<IBookContents.BookPageInfo> list = (List<IBookContents.BookPageInfo>)type.GetMethod("GetContents").Invoke(obj, new object[0]);
		List<BookPage> list2 = new List<BookPage>();
		foreach (IBookContents.BookPageInfo item in list)
		{
			if (item.Format == "Auto")
			{
				list2.AddRange(AutoformatPages(item.Title, GameText.VariableReplace(item.Text), item.Format, item.Margins));
			}
			else
			{
				list2.Add(new BookPage(item.Title, GameText.VariableReplace(item.Text)));
			}
		}
		BookInfo bookInfo = new BookInfo
		{
			Dynamic = true
		};
		try
		{
			MethodInfo method = type.GetMethod("GetTitle");
			bookInfo.Title = (string)method.Invoke(obj, new object[0]);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Exception executing GetTitle on dynamic book", x);
		}
		bookInfo.Pages = list2;
		Books.Add(BookID, bookInfo);
		DynamicBooks.Add(BookID);
	}

	public static void RenderPage(string BookID, int nPage)
	{
		BookPage bookPage;
		if (BookID[0] == '@')
		{
			if (!Books.ContainsKey(BookID))
			{
				RenderDynamicBook(BookID);
			}
			bookPage = Books[BookID].Pages[nPage];
		}
		else
		{
			bookPage = Books[BookID].Pages[nPage];
		}
		int num = 1 + bookPage.TopMargin;
		int num2 = 0;
		for (int i = num2; i < bookPage.Lines.Count; i++)
		{
			if (num >= 24)
			{
				break;
			}
			Buffer.Goto(bookPage.LeftMargin, num);
			Buffer.Write(GameText.VariableReplace(bookPage.Lines[i]));
			num++;
		}
		Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		Buffer.Goto(2, 0);
		Buffer.Write("[ {{Y|" + bookPage.Title + "}} ]");
		Buffer.Goto(2, 24);
		Buffer.Write("[ Page {{C|" + (nPage + 1) + "}} of {{C|" + Books[BookID].Count + "}} ]");
		Buffer.Write(" [" + ControlManager.getCommandInputFormatted("UI:Navigate/left", mapGlyphs: false) + "]-Previous Page [" + ControlManager.getCommandInputFormatted("UI:Navigate/right", mapGlyphs: false) + "]-Next Page [" + ControlManager.getCommandInputFormatted("Cancel", mapGlyphs: false) + "]-Exit]");
		if (bookPage.Lines.Count > 23)
		{
			for (int j = 1; j < 24; j++)
			{
				Buffer.Goto(79, j);
				Buffer.Write(177, ConsoleLib.Console.ColorUtility.Bright((ushort)0), 0);
			}
			_ = (int)Math.Ceiling((double)bookPage.Lines.Count / 23.0);
			int num3 = (int)((double)(int)Math.Ceiling((double)bookPage.Lines.Count + 23.0) / 23.0);
			_ = 0;
			if (num3 <= 0)
			{
				num3 = 1;
			}
			int num4 = 23 / num3;
			if (num4 <= 0)
			{
				num4 = 1;
			}
			int num5 = (int)((double)(23 - num4) * ((double)num2 / (double)(bookPage.Lines.Count - 23)));
			num5++;
			for (int k = num5; k < num5 + num4; k++)
			{
				Buffer.Goto(79, k);
				Buffer.Write(219, ConsoleLib.Console.ColorUtility.Bright(7), 0);
			}
		}
	}

	public static void RenderPage(MarkovBook Book, int nPage)
	{
		BookPage bookPage = Book.Pages[nPage];
		int num = 1 + bookPage.TopMargin;
		int num2 = 0;
		for (int i = num2; i < bookPage.Lines.Count; i++)
		{
			if (num >= 24)
			{
				break;
			}
			Buffer.Goto(bookPage.LeftMargin, num);
			Buffer.Write(GameText.VariableReplace(bookPage.Lines[i]));
			num++;
		}
		Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		Buffer.Goto(2, 0);
		Buffer.Write("[ {{Y|" + bookPage.Title + "}} ]");
		Buffer.Goto(2, 24);
		Buffer.Write("[ Page {{C|" + (nPage + 1) + "}} of {{C|" + Book.Pages.Count + "}} ]");
		Buffer.Write(" [" + ControlManager.getCommandInputFormatted("UI:Navigate/left", mapGlyphs: false) + "]-Previous Page [" + ControlManager.getCommandInputFormatted("UI:Navigate/right", mapGlyphs: false) + "]-Next Page [" + ControlManager.getCommandInputFormatted("Cancel", mapGlyphs: false) + "]-Exit]");
		if (bookPage.Lines.Count > 23)
		{
			for (int j = 1; j < 24; j++)
			{
				Buffer.Goto(79, j);
				Buffer.Write(177, ConsoleLib.Console.ColorUtility.Bright((ushort)0), 0);
			}
			_ = (int)Math.Ceiling((double)bookPage.Lines.Count / 23.0);
			int num3 = (int)((double)(int)Math.Ceiling((double)bookPage.Lines.Count + 23.0) / 23.0);
			_ = 0;
			if (num3 <= 0)
			{
				num3 = 1;
			}
			int num4 = 23 / num3;
			if (num4 <= 0)
			{
				num4 = 1;
			}
			int num5 = (int)((double)(23 - num4) * ((double)num2 / (double)(bookPage.Lines.Count - 23)));
			num5++;
			for (int k = num5; k < num5 + num4; k++)
			{
				Buffer.Goto(79, k);
				Buffer.Write(219, ConsoleLib.Console.ColorUtility.Bright(7), 0);
			}
		}
	}

	public static void ShowBookByID(string BookID, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> OnShowPage = null)
	{
		if (Options.ModernUI)
		{
			_ = BookScreen.show(BookID, Sound, OnShowPage).Result;
			return;
		}
		GameManager.Instance.PushGameView("Book");
		SoundManager.PlaySound(Sound);
		int num = 0;
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer2(bLoadFromCurrent: true);
		Keys keys;
		do
		{
			XRL.World.Event.ResetPool(resetMinEventPools: false);
			Buffer.Clear();
			Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			RenderPage(BookID, num);
			Console.DrawBuffer(Buffer);
			OnShowPage?.Invoke(num);
			keys = Keyboard.getvk(MapDirectionToArrows: true);
			if ((keys == Keys.NumPad6 || keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right")) && num < Books[BookID].Count - 1)
			{
				SoundManager.PlaySound("Sounds/Interact/sfx_interact_book_pageTurn", 0.2f);
				num++;
			}
			if ((keys == Keys.NumPad4 || keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left")) && num > 0)
			{
				SoundManager.PlaySound("Sounds/Interact/sfx_interact_book_pageTurn", 0.2f);
				num--;
			}
		}
		while (keys != Keys.Escape);
		GameManager.Instance.PopGameView(bHard: true);
		scrapBuffer.Draw();
	}

	public static void ShowBook(MarkovBook Book, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> OnShowPage = null, Action<int> AfterShowPage = null)
	{
		if (Options.ModernUI)
		{
			_ = BookScreen.show(Book, Sound, OnShowPage, AfterShowPage).Result;
			return;
		}
		GameManager.Instance.PushGameView("Book");
		SoundManager.PlaySound(Sound);
		int num = 0;
		Keys keys;
		do
		{
			XRL.World.Event.ResetPool(resetMinEventPools: false);
			Buffer.Clear();
			Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			RenderPage(Book, num);
			Console.DrawBuffer(Buffer);
			OnShowPage?.Invoke(num);
			keys = Keyboard.getvk(MapDirectionToArrows: true);
			if (keys == Keys.NumPad6 && num < Book.Pages.Count - 1)
			{
				SoundManager.PlaySound("Sounds/Interact/sfx_interact_book_pageTurn", 0.2f);
				AfterShowPage?.Invoke(num);
				num++;
			}
			if (keys == Keys.NumPad4 && num > 0)
			{
				SoundManager.PlaySound("Sounds/Interact/sfx_interact_book_pageTurn", 0.2f);
				AfterShowPage?.Invoke(num);
				num--;
			}
		}
		while (keys != Keys.Escape);
		AfterShowPage?.Invoke(num);
		GameManager.Instance.PopGameView(bHard: true);
	}

	public static void ShowBook(string PageText, string BookTitle, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> OnShowPage = null, Action<int> AfterShowPage = null)
	{
		PageText = Markup.Transform(PageText);
		BookTitle = Markup.Transform(BookTitle);
		MarkovBook markovBook = new MarkovBook();
		markovBook.Title = BookTitle;
		markovBook.Pages = new List<BookPage>();
		string format = "Auto";
		string margins = "1,2,2,2";
		markovBook.Pages.AddRange(AutoformatPages(BookTitle, PageText, format, margins));
		ShowBook(markovBook, Sound, OnShowPage, AfterShowPage);
	}

	public static void ShowBook(List<string> PageText, string BookTitle, string Sound = "Sounds/Interact/sfx_interact_book_read", Action<int> OnShowPage = null, Action<int> AfterShowPage = null)
	{
		BookTitle = Markup.Transform(BookTitle);
		MarkovBook markovBook = new MarkovBook();
		markovBook.Title = BookTitle;
		markovBook.Pages = new List<BookPage>();
		string format = "Auto";
		string margins = "1,2,2,2";
		foreach (string item in PageText)
		{
			markovBook.Pages.AddRange(AutoformatPages(BookTitle, Markup.Transform(item), format, margins));
		}
		ShowBook(markovBook, Sound, OnShowPage, AfterShowPage);
	}
}
