using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Messages;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.UI;

[UIView("Popup", false, false, false, "Menu", null, false, 0, false)]
public class Popup : IWantsTextConsoleInit
{
	protected class PopupTextBuilder
	{
		public class Section
		{
			public int Selection;

			public int StartOffset;

			public int EndOffset;

			public int Count => 1 + EndOffset - StartOffset;
		}

		public const int IntroSelection = -1;

		public List<string> Lines = new List<string>(24);

		public int MaxClippedWidth;

		public int MaxWidth = 72;

		public int Padding;

		public bool RespectNewlines = true;

		public int Spacing;

		public string SpacingText = "";

		public List<Section> Sections = new List<Section>();

		public void AddSection(string text, int selection)
		{
			Section section = new Section();
			section.Selection = selection;
			section.StartOffset = Lines.Count;
			Lines.AddRange(StringFormat.ClipTextToArray(text, MaxWidth - Padding, out var MaxClippedWidth, RespectNewlines, KeepColorsAcrossNewlines: true, TransformMarkup: false, TransformMarkupIfMultipleLines: true));
			this.MaxClippedWidth = Math.Max(this.MaxClippedWidth, MaxClippedWidth + Padding);
			section.EndOffset = Lines.Count - 1;
			Sections.Add(section);
		}

		public void AddSpacing(int lines, string text = null)
		{
			for (int i = 0; i < lines; i++)
			{
				Lines.Add(Markup.Transform(text ?? SpacingText));
			}
		}

		public Section GetSection(int selection)
		{
			foreach (Section section in Sections)
			{
				if (section.Selection == selection)
				{
					return section;
				}
			}
			return null;
		}

		public string[] GetSelectionLines(int selection)
		{
			Section section = GetSection(selection);
			if (section == null)
			{
				return new string[0];
			}
			int count = section.Count + ((selection >= 0 && selection != Sections[Sections.Count - 1].Selection) ? Spacing : 0);
			return Lines.GetRange(section.StartOffset, count).ToArray();
		}

		public int GetSelectionStart(int selection)
		{
			return GetSection(selection)?.StartOffset ?? Lines.Count;
		}
	}

	public const string DEFAULT_SOUND = "Sounds/UI/ui_notification";

	public const string PROMPT_SOUND = "Sounds/UI/ui_notification_question";

	public const string WARN_SOUND = "Sounds/UI/ui_notification_warning";

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	[Obsolete("We don't use it. You shouldn't either.")]
	public static Thread ThinkingThread = null;

	[Obsolete("We don't use it. You shouldn't either.  See XRL.UI.Loading.SetLoadStatus(string) or LoadTask(string, Action)")]
	public static string ThinkingString = "";

	[Obsolete("We don't use it. You shouldn't either.  See XRL.UI.Loading.SetHideLoad(bool).")]
	public static bool DisableThinking = false;

	[Obsolete("Not used.")]
	public static string LastThinking = "";

	[Obsolete("Not used.")]
	public static bool bPaused = false;

	[NonSerialized]
	private static List<string> RenderDirectLines = new List<string>();

	public static TextBlock _RenderBlock = new TextBlock("", 78, 5000);

	public static bool Suppress = false;

	[NonSerialized]
	private static List<string> ColorPickerOptionStrings = new List<string>(32);

	[NonSerialized]
	private static List<string> ColorPickerOptions = new List<string>(32);

	[NonSerialized]
	private static List<char> ColorPickerKeymap = new List<char>(32);

	[NonSerialized]
	public static ScreenBuffer ScrapBuffer = ScreenBuffer.create(80, 25);

	[NonSerialized]
	public static ScreenBuffer ScrapBuffer2 = ScreenBuffer.create(80, 25);

	public static bool DisplayedLoadError = false;

	public static string SPACING_DARK_LINE = "{{K|================================================================================}}";

	public static string SPACING_BRIGHT_LINE = "{{Y|================================================================================}}";

	public static string SPACING_GREY_RAINBOW_LINE = "{{K-y-Y-y sequence|================================================================================}}";

	[Obsolete("Use Suppress field")]
	public static bool bSuppressPopups
	{
		get
		{
			return Suppress;
		}
		set
		{
			Suppress = value;
		}
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void StartThinkingThread(string String)
	{
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void StartThinkingThread(object oString)
	{
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void PauseThinking()
	{
		Loading.SetHideLoadStatus(hidden: true);
	}

	[Obsolete("Will be removed, no longer used.")]
	public static void ResumeThinking()
	{
		Loading.SetHideLoadStatus(hidden: false);
	}

	[Obsolete("Will be removed, no longer used.  See XRL.UI.Loading.SetLoadingStatus()")]
	public static void StartThinking(string DisplayString)
	{
		Loading.SetLoadingStatus(DisplayString);
	}

	[Obsolete("Will be removed, no longer used.  See XRL.UI.Loading.SetLoadingStatus()")]
	public static void EndThinking()
	{
		Loading.SetLoadingStatus(null);
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static void RenderDirect(int XPos, int YPos, ScreenBuffer Buffer, string Message, string BottomLineNoFormat, string BottomLine, int StartingLine)
	{
		Message = Markup.Transform(Message);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		string[] array = Message.Split('\n');
		RenderDirectLines.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			while (array[i].Length > 78)
			{
				string item = array[i].Substring(0, 78);
				string text = array[i].Substring(78, array[i].Length - 78);
				array[i] = text;
				RenderDirectLines.Add(item);
			}
			RenderDirectLines.Add(array[i]);
		}
		int num = BottomLineNoFormat.Length + 2;
		for (int j = 0; j < RenderDirectLines.Count; j++)
		{
			if (RenderDirectLines[j].Length + 2 >= num)
			{
				num = RenderDirectLines[j].Length + 2;
			}
		}
		int num2 = num / 2;
		int num3 = RenderDirectLines.Count / 2;
		if (num3 < 1)
		{
			num3 = 1;
		}
		num3++;
		num2++;
		int Top = 12 - num3;
		int Bottom = 12 + num3 + 1;
		int Left = 40 - num2;
		int Right = 40 + num2;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		for (int k = StartingLine; k < RenderDirectLines.Count && Top + 2 + k - StartingLine < 24; k++)
		{
			Buffer.Goto(XPos + 2, YPos + 2 + k - StartingLine);
			if (StartingLine > 0 && k == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + k - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(RenderDirectLines[k]);
			}
		}
		int num4 = num2 * 2;
		num4 -= BottomLineNoFormat.Length;
		num4 /= 2;
		num4++;
		Buffer.Goto(Left + num4, Bottom);
		Buffer.Write(BottomLine);
	}

	public static int Render(List<string> Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine)
	{
		int num = 22;
		int num2 = Message.Count;
		if (num2 > 20)
		{
			num2 = 20;
		}
		for (int i = 0; i < Message.Count; i++)
		{
			int num3 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Message[i]) + 2;
			if (num3 > num)
			{
				num = num3;
			}
		}
		int num4 = num / 2;
		int num5 = Message.Count / 2;
		if (num5 < 1)
		{
			num5 = 1;
		}
		num5++;
		num4++;
		int Top = 12 - num5;
		int Bottom = 12 + num5 + 1;
		int Left = 40 - num4;
		int Right = 40 + num4;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom + 1, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom + 1, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		for (int j = StartingLine; j < Message.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			Buffer.Goto(Left + 2, Top + 2 + j - StartingLine);
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(Message[j]);
			}
		}
		int num6 = num4 * 2;
		num6 -= BottomLineNoFormat.Length;
		num6 /= 2;
		num6++;
		Buffer.Goto(Left + num6, Bottom);
		Buffer.Write(BottomLine);
		return num2;
	}

	public static void Render(string Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine)
	{
		Message = Markup.Transform(Message);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		List<string> list = new List<string>(Message.Split('\n'));
		int num = 22;
		for (int i = 0; i < list.Count; i++)
		{
			int num2 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(list[i]) + 2;
			if (num2 > num)
			{
				num = num2;
			}
		}
		int num3 = num / 2;
		int num4 = list.Count / 2;
		if (num4 < 1)
		{
			num4 = 1;
		}
		num4++;
		num3++;
		int Top = 12 - num4;
		int Bottom = 12 + num4 + 1;
		int Left = 40 - num3;
		int Right = 40 + num3;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		for (int j = StartingLine; j < list.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			Buffer.Goto(Left + 2, Top + 2 + j - StartingLine);
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(list[j]);
			}
		}
		int num5 = num3 * 2;
		num5 -= BottomLineNoFormat.Length;
		num5 /= 2;
		num5++;
		Buffer.Goto(Left + num5, Bottom);
		Buffer.Write(BottomLine);
	}

	public static int RenderBlock(string Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine, int MinWidth = -1, int MinHeight = -1, string Title = null, bool BottomLineRegions = false, IRenderable Icon = null, bool centerIcon = true, bool rightIcon = false)
	{
		Message = Markup.Transform(Message);
		Title = Markup.Transform(Title);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		List<string> lines = new TextBlock(Message, 78, 5000, ReverseBlocks: false, rightIcon ? (-3) : 0).Lines;
		int num = 22;
		if ((Icon != null) & centerIcon)
		{
			lines.Insert(0, "");
			lines.Insert(0, "");
		}
		if (Title != null)
		{
			int num2 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Title) + 2;
			if (num2 > MinWidth)
			{
				MinWidth = num2;
			}
		}
		if (MinWidth > -1)
		{
			num = MinWidth;
		}
		for (int i = 0; i < lines.Count; i++)
		{
			int num3 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(lines[i]) + 2;
			if (i == 0 && Icon != null && !centerIcon)
			{
				num3 += 3;
			}
			if (num3 > num)
			{
				num = num3;
			}
		}
		int num4 = num / 2;
		int num5 = lines.Count / 2;
		num4++;
		if (MinHeight > -1)
		{
			num5 = MinHeight;
		}
		if (num5 < 2)
		{
			num5 = 2;
		}
		int Top = 10 - num5;
		int Bottom = Top + lines.Count + 3;
		int Left = 40 - num4;
		int Right = 40 + num4;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		if (Right < MinWidth - 1)
		{
			Right = MinHeight - 1;
		}
		if (Bottom < MinHeight - 1)
		{
			Bottom = MinHeight - 1;
		}
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		if (Title != null)
		{
			Buffer.Goto(Left + 1, Top);
			Buffer.Write(Title);
		}
		for (int j = StartingLine; j < lines.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			int num6 = Left + 2;
			int y = Top + 2 + j - StartingLine;
			if (j == 0 && Icon != null && !centerIcon && !rightIcon)
			{
				Buffer.Goto(num6 + 2, y);
			}
			else
			{
				Buffer.Goto(num6, y);
			}
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(lines[j]);
			}
			if (j == 0 && Icon != null)
			{
				if (centerIcon)
				{
					Buffer.Goto(num6 + (num - 1) / 2, y);
				}
				else if (rightIcon)
				{
					Buffer.Goto(Right - 3, y);
					Buffer.Write("   ");
					Buffer.Goto(Right - 2, y);
				}
				else
				{
					Buffer.Goto(num6, y);
				}
				Buffer.Write(Icon);
			}
		}
		int num7 = num4 * 2;
		num7 -= BottomLineNoFormat.Length;
		num7 /= 2;
		num7++;
		Buffer.Goto(Left + num7, Bottom);
		Buffer.Write(BottomLine);
		if (BottomLineRegions)
		{
			GameManager.Instance.ClearRegions();
			List<int> list = new List<int>();
			list.Add(0);
			for (int k = 0; k < BottomLineNoFormat.Length; k++)
			{
				if (BottomLineNoFormat[k] == ' ')
				{
					list.Add(k);
				}
			}
			list.Add(BottomLineNoFormat.Length);
			for (int l = 0; l < list.Count - 1; l++)
			{
				GameManager.Instance.AddRegion(Left + num7 + list[l], Bottom - 1, Left + num7 + list[l + 1], Bottom + 1, "LeftOption:" + (l + 1), "RightOption:" + (l + 1));
			}
		}
		return lines.Count;
	}

	public static int RenderBlock(StringBuilder Message, string BottomLineNoFormat, string BottomLine, ScreenBuffer Buffer, int StartingLine, int MinWidth, int MinHeight, string Title)
	{
		Markup.Transform(Message);
		Title = Markup.Transform(Title);
		if (Message.Contains("\r\n"))
		{
			Message = Message.Replace("\r\n", "\n");
		}
		if (Message.Contains("\r"))
		{
			Message = Message.Replace("\r", "\n");
		}
		_RenderBlock.Format(Message);
		List<string> lines = _RenderBlock.Lines;
		int num = 22;
		if (Title != null)
		{
			int num2 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Title) + 2;
			if (num2 > MinWidth)
			{
				MinWidth = num2;
			}
		}
		if (MinWidth > -1)
		{
			num = MinWidth;
		}
		for (int i = 0; i < lines.Count; i++)
		{
			int num3 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(lines[i]) + 2;
			if (num3 > num)
			{
				num = num3;
			}
		}
		int num4 = num / 2;
		int num5 = lines.Count / 2;
		num4++;
		if (MinHeight > -1)
		{
			num5 = MinHeight;
		}
		if (num5 < 2)
		{
			num5 = 2;
		}
		int Top = 10 - num5;
		int Bottom = Top + lines.Count + 3;
		int Left = 40 - num4;
		int Right = 40 + num4;
		TextConsole.Constrain(ref Left, ref Right, ref Top, ref Bottom);
		Buffer.Fill(Left, Top, Right, Bottom, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
		Buffer.ThickSingleBox(Left, Top, Right, Bottom, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		if (Title != null)
		{
			Buffer.Goto(Left + 1, Top);
			Buffer.Write(Title);
		}
		for (int j = StartingLine; j < lines.Count && Top + 2 + j - StartingLine < 24; j++)
		{
			Buffer.Goto(Left + 2, Top + 2 + j - StartingLine);
			if (StartingLine > 0 && j == StartingLine)
			{
				Buffer.Write("<up for more...>");
			}
			else if (Top + 2 + j - StartingLine == 23)
			{
				Buffer.Write("<down for more...>");
			}
			else
			{
				Buffer.Write(lines[j]);
			}
		}
		int num6 = num4 * 2;
		num6 -= BottomLineNoFormat.Length;
		num6 /= 2;
		num6++;
		Buffer.Goto(Left + num6, Bottom);
		Buffer.Write(BottomLine);
		return lines.Count;
	}

	public static async Task ShowFailAsync(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true)
	{
		await ShowAsync(Message, CopyScrap, Capitalize, DimBackground, LogMessage: false);
	}

	public static void ShowFail(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true)
	{
		Show(Message, null, "Sounds/UI/ui_notification", CopyScrap, Capitalize, DimBackground, LogMessage: false);
	}

	public static async Task ShowKeybindAsync(string Message, CancellationToken cancelToken)
	{
		await NewPopupMessageAsync(Message, PopupMessage.AnyKey, null, null, null, 0, null, null, null, showContextFrame: true, EscapeNonMarkupFormatting: true, pushView: false, cancelToken);
	}

	public static async Task ShowAsync(string Message, bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true, bool PushView = false)
	{
		if (Suppress)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
		}
		else if (!GameManager.IsOnGameContext() || Options.ModernUI)
		{
			await NewPopupMessageAsync(Message, PopupMessage.SingleButton, null, null, null, 0, null, null, null, showContextFrame: true, EscapeNonMarkupFormatting: true, PushView);
		}
		else
		{
			Show(Message);
		}
	}

	public static void Show(string Message, string Title = null, string Sound = "Sounds/UI/ui_notification", bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true, Location2D PopupLocation = null)
	{
		if (Suppress || _TextConsole == null)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
		}
		else
		{
			ShowBlock(Message, null, Sound, CopyScrap, Capitalize, DimBackground, LogMessage, PopupLocation);
		}
	}

	public static Keys ShowBlockWithCopy(string Message, string Prompt, string Title, string CopyInfo, bool LogMessage = true)
	{
		string message = "World seed copied to your clipboard.";
		if (UIManager.UseNewPopups)
		{
			bool copied = false;
			WaitNewPopupMessage(Message, PopupMessage.CopyButton, (QudMenuItem item) =>
			{
				if (item.command == "Copy")
				{
					ClipboardHelper.SetClipboardData(CopyInfo);
					copied = true;
				}
			}, null, Title);
			Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
			if (copied)
			{
				Show(message, null, "Sounds/UI/ui_notification", CopyScrap: false);
			}
			return Keys.Space;
		}
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		Title = Markup.Transform(Title);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		int num = 0;
		Keys keys = Keys.Pause;
		while (keys == Keys.Pause || keys == Keys.NumPad2 || keys == Keys.NumPad8 || keys == Keys.Next || keys == Keys.Prior || keys == Keys.Next)
		{
			int num2 = RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, num, -1, -1, Title);
			_TextConsole.DrawBuffer(ScrapBuffer);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (keys == Keys.C)
			{
				ClipboardHelper.SetClipboardData(CopyInfo);
				Show(message, null, "Sounds/UI/ui_notification", CopyScrap: false);
			}
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2);
		Loading.SetHideLoadStatus(hidden: false);
		return keys;
	}

	public static void ShowProgress(Action<Progress> a)
	{
		new Progress().start(a);
	}

	public static async Task<T> ShowProgressAsync<T>(Func<Progress, Task<T>> a)
	{
		return await new Progress().startAsync(a);
	}

	public static async Task<QudMenuItem> NewPopupMessageAsync(string message, List<QudMenuItem> buttons = null, List<QudMenuItem> options = null, string title = null, string inputDefault = null, int DefaultSelected = 0, string contextTitle = null, IRenderable contextRender = null, IRenderable afterRender = null, bool showContextFrame = true, bool EscapeNonMarkupFormatting = true, bool pushView = false, CancellationToken cancelToken = default(CancellationToken), bool askingNumber = false, string RestrictChars = "", string WantsSpecificPrompt = null, Location2D PopupLocation = null, string PopupID = null)
	{
		TaskCompletionSource<QudMenuItem> t = new TaskCompletionSource<QudMenuItem>();
		NavigationController instance = NavigationController.instance;
		GameManager.Instance.PushGameView("DynamicPopupMessage");
		ControlManager.ResetInput();
		return await instance.SuspendContextWhile(async () =>
		{
			await The.UiContext;
			PopupMessage messageWindow = UIManager.copyWindow("PopupMessage") as PopupMessage;
			inputDefault = Sidebar.FromCP437(inputDefault);
			try
			{
				messageWindow.ShowPopup(message, buttons ?? PopupMessage.SingleButton, (QudMenuItem item) =>
				{
					messageWindow.onHide = () =>
					{
						t.TrySetResult(item);
					};
				}, options ?? new List<QudMenuItem>(), (QudMenuItem item) =>
				{
					messageWindow.onHide = () =>
					{
						t.TrySetResult(item);
					};
				}, title, inputDefault != null, (!EscapeNonMarkupFormatting) ? inputDefault : inputDefault?.Replace("&&", "&").Replace("^^", "^"), DefaultSelected, () =>
				{
					t.TrySetCanceled();
				}, contextRender, contextTitle, afterRender, showContextFrame, pushView, cancelToken, askingNumber, RestrictChars, WantsSpecificPrompt, PopupLocation, PopupID);
				messageWindow.Show();
				QudMenuItem result = await t.Task;
				if (inputDefault != null || result.command == "CmdOptions")
				{
					result.text = Sidebar.ToCP437(messageWindow.inputBox.text);
					if (EscapeNonMarkupFormatting)
					{
						result.text = result.text.Replace("&", "&&").Replace("^", "^^");
					}
				}
				ControlManager.ResetInput();
				return result;
			}
			finally
			{
				UIManager.releaseCopy(messageWindow);
				GameManager.Instance.PopGameView(bHard: true);
			}
		});
	}

	public static async void WaitNewPopupMessage(string message, List<QudMenuItem> buttons = null, Action<QudMenuItem> callback = null, List<QudMenuItem> options = null, string title = null, string inputDefault = null, int DefaultSelected = 0, string contextTitle = null, IRenderable contextRender = null, IRenderable afterRender = null, bool showContextFrame = true, bool EscapeNonMarkupFormatting = true, Location2D PopupLocation = null, string PopupID = null, bool AllowRenderMapBehind = false)
	{
		ControlManager.ResetInput();
		if (Thread.CurrentThread == GameManager.Instance.uiQueue.threadContext)
		{
			string message2 = message;
			List<QudMenuItem> buttons2 = buttons;
			List<QudMenuItem> options2 = options;
			string title2 = title;
			string inputDefault2 = inputDefault;
			int defaultSelected = DefaultSelected;
			string contextTitle2 = contextTitle;
			IRenderable renderable = contextRender;
			bool flag = EscapeNonMarkupFormatting;
			IRenderable afterRender2 = afterRender;
			bool showContextFrame2 = showContextFrame;
			bool escapeNonMarkupFormatting = flag;
			Location2D popupLocation = PopupLocation;
			string popupID = PopupID;
			QudMenuItem obj = await NewPopupMessageAsync(message2, buttons2, options2, title2, inputDefault2, defaultSelected, contextTitle2, renderable, afterRender2, showContextFrame2, escapeNonMarkupFormatting, pushView: false, default, askingNumber: false, "", null, popupLocation, popupID);
			callback?.Invoke(obj);
			return;
		}
		TaskCompletionSource<QudMenuItem> complete = new TaskCompletionSource<QudMenuItem>();
		GameManager.Instance.uiQueue.awaitTask(() =>
		{
			PopupMessage messageWindow = UIManager.getWindow("PopupMessage") as PopupMessage;
			try
			{
				GameManager.Instance.PushGameView("PopupMessage");
				inputDefault = Sidebar.FromCP437(inputDefault);
				PopupMessage popupMessage = messageWindow;
				string message3 = message;
				List<QudMenuItem> buttons3 = buttons ?? PopupMessage.SingleButton;
				Action<QudMenuItem> commandCallback = (QudMenuItem i) =>
				{
					try
					{
						if (inputDefault != null)
						{
							i.text = Sidebar.ToCP437(messageWindow.inputBox.text);
						}
						if (EscapeNonMarkupFormatting)
						{
							i.text = i.text.Replace("&", "&&").Replace("^", "^^");
						}
						complete.SetResult(i);
					}
					catch (Exception exception)
					{
						complete.TrySetException(exception);
					}
				};
				List<QudMenuItem> items = options ?? new List<QudMenuItem>();
				Action<QudMenuItem> selectedItemCallback = (QudMenuItem i) =>
				{
					try
					{
						if (inputDefault != null)
						{
							i.text = messageWindow.inputBox.text;
						}
						complete.SetResult(i);
					}
					catch (Exception exception)
					{
						complete.TrySetException(exception);
					}
				};
				string title3 = title;
				bool includeInput = inputDefault != null;
				string inputDefault3 = ((!EscapeNonMarkupFormatting) ? inputDefault : inputDefault?.Replace("&&", "&").Replace("^^", "^"));
				int defaultSelected2 = DefaultSelected;
				Action onHide = () =>
				{
					complete.TrySetCanceled();
					Loading.SetHideLoadStatus(hidden: false);
				};
				string contextTitle3 = contextTitle;
				IRenderable renderable2 = contextRender;
				IRenderable afterRender3 = afterRender;
				bool showContextFrame3 = showContextFrame;
				Location2D popupLocation2 = PopupLocation;
				string popupID2 = PopupID;
				popupMessage.ShowPopup(message3, buttons3, commandCallback, items, selectedItemCallback, title3, includeInput, inputDefault3, defaultSelected2, onHide, renderable2, contextTitle3, afterRender3, showContextFrame3, pushView: true, default, askingNumber: false, "", null, popupLocation2, popupID2);
			}
			catch (Exception ex)
			{
				MetricsManager.LogError("Error showing popup", ex);
				complete.TrySetException(ex);
			}
			finally
			{
				ControlManager.ResetInput();
			}
		});
		if (AllowRenderMapBehind)
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			while (!complete.Task.IsCompleted && !complete.Task.IsCanceled)
			{
				complete.Task.Wait(33);
				The.Core?.RenderBaseToBuffer(scrapBuffer);
				scrapBuffer.Draw();
			}
		}
		else
		{
			complete.Task.Wait();
		}
		GameManager.Instance.PopGameView(bHard: true);
		if (complete.Task.IsCompleted)
		{
			callback?.Invoke(complete.Task.Result);
			return;
		}
		if (complete.Task.IsFaulted)
		{
			throw complete.Task.Exception ?? new Exception("Await new popup exception");
		}
		throw new Exception("Popup task was not completed some other way! " + complete?.Task);
	}

	public static void RenderBlock(string Message, string Prompt, bool Capitalize = true, bool MuteBackground = true, IRenderable Icon = null, bool centerIcon = true, bool rightIcon = false, bool LogMessage = true)
	{
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		Prompt = Markup.Transform(Prompt);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		int startingLine = 0;
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, startingLine, -1, -1, null, BottomLineRegions: false, Icon, centerIcon, rightIcon);
		_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
		if (Icon != null)
		{
			ScreenBuffer.ClearImposterSuppression();
		}
	}

	public static Keys ShowBlockPrompt(string Message, string Prompt, string Sound = "Sounds/UI/ui_notification", IRenderable Icon = null, bool Capitalize = true, bool MuteBackground = true, bool CenterIcon = true, bool RightIcon = false, bool LogMessage = true)
	{
		if (Suppress)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
			return Keys.Space;
		}
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		Prompt = Markup.Transform(Prompt);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		if (UIManager.UseNewPopups || ScrapBuffer == null)
		{
			WaitNewPopupMessage(Message, null, null, null, Prompt);
			return Keys.Space;
		}
		int num = 0;
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keys keys = Keys.Pause;
		while (keys == Keys.Pause || keys == Keys.NumPad2 || keys == Keys.NumPad8 || keys == Keys.Next || keys == Keys.Prior || keys == Keys.Next)
		{
			int num2 = RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, num, -1, -1, null, BottomLineRegions: false, Icon, CenterIcon, RightIcon);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (Icon != null)
			{
				ScreenBuffer.ClearImposterSuppression();
			}
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return keys;
	}

	public static Keys ShowBlockSpace(string Message, string Prompt, bool Capitalize = true, bool MuteBackground = true, IRenderable Icon = null, bool centerIcon = true, bool rightIcon = false, bool LogMessage = true)
	{
		if (Suppress)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
			return Keys.Space;
		}
		Loading.SetHideLoadStatus(hidden: true);
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		Prompt = Markup.Transform(Prompt);
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		int num = 0;
		new TextBlock(Message, 80, 5000);
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, null, null, null, "");
			return Keys.Space;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keys keys = Keys.Pause;
		while (keys != Keys.Escape && keys != Keys.Space && keys != Keys.Enter && keys != Keys.Enter)
		{
			int num2 = RenderBlock(Message, ConsoleLib.Console.ColorUtility.StripFormatting(Prompt), Prompt, ScrapBuffer, num, -1, -1, null, BottomLineRegions: false, Icon, centerIcon, rightIcon);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return keys;
	}

	public static void ShowSpace(string Message, string Title = null, string Sound = "Sounds/UI/ui_notification", Renderable AfterRender = null, bool LogMessage = true, bool ShowContextFrame = true, string PopupID = null)
	{
		if (Suppress)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message);
			}
			return;
		}
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		Message = Markup.Transform(Message);
		MessageQueue.AddPlayerMessage(Message);
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, null, null, null, null, null, 0, Title, null, AfterRender, ShowContextFrame, EscapeNonMarkupFormatting: true, null, PopupID);
			return;
		}
		new TextBlock(Message, 80, 5000);
		int startingLine = 0;
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		List<QudMenuItem> singleButton = PopupMessage.SingleButton;
		string bottomLineNoFormat = ConsoleLib.Console.ColorUtility.StripFormatting(singleButton[0].text);
		string text = singleButton[0].text;
		int num = 95;
		while (Keyboard.vkCode != Keys.Space && (num != 252 || !(Keyboard.CurrentMouseEvent.Event == "Accept")))
		{
			RenderBlock(Message, bottomLineNoFormat, text, ScrapBuffer, startingLine);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			num = Keyboard.getch();
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
	}

	public static Keys ShowBlock(string Message, string Title = null, string Sound = "Sounds/UI/ui_notification", bool CopyScrap = true, bool Capitalize = true, bool DimBackground = true, bool LogMessage = true, Location2D PopupLocation = null)
	{
		if (Suppress)
		{
			if (LogMessage)
			{
				MessageQueue.AddPlayerMessage(Message, null, Capitalize);
			}
			return Keys.Space;
		}
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		Message = Markup.Transform(Message);
		if (Capitalize)
		{
			Message = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Message);
		}
		if (LogMessage)
		{
			MessageQueue.AddPlayerMessage(Message);
		}
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, null, null, null, Title, null, 0, null, null, null, showContextFrame: true, EscapeNonMarkupFormatting: true, PopupLocation);
			return Keys.Space;
		}
		int num = 0;
		if (CopyScrap)
		{
			ScrapBuffer.Copy(TextConsole.CurrentBuffer);
			ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		}
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		Keys keys = Keys.Pause;
		while (keys != Keys.Space && keys != Keys.Enter && keys != Keys.Escape)
		{
			int num2 = RenderBlock(Message, "[press space]", "[press {{W|space}}]", ScrapBuffer, num, -1, -1, Title);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			try
			{
				keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("hm", x);
			}
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next || keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return keys;
	}

	public static int? AskNumber(string Message, string Sound = "Sounds/UI/ui_notification", string RestrictChars = "", int Start = 0, int Min = 0, int Max = int.MaxValue)
	{
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		if (RestrictChars == "")
		{
			RestrictChars = ((Min >= 0) ? "0123456789" : "0123456789-");
		}
		if (UIManager.UseNewPopups)
		{
			int? result = AskNumberAsync(Message, Start, Min, Max, RestrictChars, pushView: true).Result;
			if (result.HasValue)
			{
				result = Math.Max(Min, new int?(Math.Min(Max, result.Value)).Value);
			}
			return result;
		}
		Message = Markup.Transform(Message);
		string text = Start.ToString();
		int startingLine = 0;
		text = Start.ToString();
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:AskNumber");
		bool flag = false;
		int num = 95;
		while (true)
		{
			ScrapBuffer.Copy(ScrapBuffer2);
			string text2 = "";
			text2 = (flag ? (text + "_") : ("&y^K" + text + "^k"));
			RenderBlock(Message + "\n" + text2, "[\u0018\u0019\u001a\u001b adjust,Enter or space to confirm]", "[{{W|\u0018\u0019\u001a\u001b}} adjust, {{W|Enter}} or space to confirm]", ScrapBuffer, startingLine);
			_TextConsole.DrawBuffer(ScrapBuffer);
			num = Keyboard.getch();
			if (Keyboard.vkCode == Keys.Enter || Keyboard.vkCode == Keys.Space)
			{
				break;
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("SubmitString:"))
			{
				text = Keyboard.CurrentMouseEvent.Event.Substring(Keyboard.CurrentMouseEvent.Event.IndexOf(':') + 1);
				Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
				GameManager.Instance.PopGameView(bHard: true);
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Loading.SetHideLoadStatus(hidden: false);
				return Convert.ToInt32(text);
			}
			if (Keyboard.vkCode != Keys.MouseEvent)
			{
				if (Keyboard.vkCode == (Keys.V | Keys.Control))
				{
					string clipboardData = ClipboardHelper.GetClipboardData();
					text += clipboardData;
				}
				else
				{
					char c = (char)num;
					if ((char.IsDigit(c) || char.IsLetter(c) || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c) || num == 32 || (num == 45 && (!flag || text == "" || text == "0"))) && (RestrictChars == null || RestrictChars.IndexOf(c) != -1))
					{
						if (!flag)
						{
							text = "";
							flag = true;
						}
						text += c;
					}
				}
			}
			if ((Keyboard.vkCode == Keys.Back || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdDelete")) && text.Length > 0)
			{
				text = text.Substring(0, text.Length - 1);
			}
			if (Keyboard.vkCode == Keys.Escape || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
			{
				Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
				GameManager.Instance.PopGameView(bHard: true);
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Loading.SetHideLoadStatus(hidden: false);
				return null;
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick")
			{
				break;
			}
			if (Keyboard.vkCode == Keys.NumPad8)
			{
				int result2 = 0;
				int.TryParse(text, out result2);
				result2 += 10;
				text = Math.Min(Max, result2).ToString();
			}
			if (Keyboard.vkCode == Keys.NumPad2)
			{
				int result3 = 0;
				int.TryParse(text, out result3);
				result3 -= 10;
				text = Math.Max(Min, result3).ToString();
			}
			if (Keyboard.vkCode == Keys.NumPad6)
			{
				int result4 = 0;
				int.TryParse(text, out result4);
				result4++;
				text = Math.Min(Max, result4).ToString();
			}
			if (Keyboard.vkCode == Keys.NumPad4)
			{
				int result5 = 0;
				int.TryParse(text, out result5);
				result5--;
				text = Math.Max(Min, result5).ToString();
			}
			if (int.TryParse(text, out var result6))
			{
				result6 = Math.Min(Max, result6);
				result6 = Math.Max(Min, result6);
				text = result6.ToString();
			}
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		if (int.TryParse(text, out var result7))
		{
			result7 = Math.Min(Max, result7);
			return Math.Max(Min, result7);
		}
		return null;
	}

	public static async Task<int?> AskNumberAsync(string Message, int Start = 0, int Min = 0, int Max = int.MaxValue, string RestrictChars = "", bool pushView = false)
	{
		ControlManager.ResetInput();
		if (ControlManager.activeControllerType != ControlManager.InputDeviceType.Keyboard)
		{
			if (pushView)
			{
				GameManager.Instance.PushGameView("ModernPopupGamepadAskNumber");
			}
			try
			{
				return await AskNumberScreen.show(Message, Start, Min, Max);
			}
			finally
			{
				if (pushView)
				{
					GameManager.Instance.PopGameView(bHard: true);
				}
			}
		}
		List<QudMenuItem> submitCancelButton = PopupMessage.SubmitCancelButton;
		int num = Start;
		if (pushView)
		{
			GameManager.Instance.PushGameView("ModernPopup:AskNumber");
		}
		try
		{
			QudMenuItem qudMenuItem = await NewPopupMessageAsync(StringFormat.ClipText(Message, 60, KeepNewlines: true), submitCancelButton, null, null, num.ToString(), 0, null, null, null, showContextFrame: true, EscapeNonMarkupFormatting: true, pushView: false, default, askingNumber: true, RestrictChars);
			if (qudMenuItem.command == "Cancel")
			{
				return null;
			}
			return Convert.ToInt32(qudMenuItem.text);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("AskNumberAsync", x);
			return null;
		}
		finally
		{
			Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
			if (pushView)
			{
				GameManager.Instance.PopGameView(bHard: true);
			}
		}
	}

	public static async Task<string> AskStringAsync(string Message, string Default = "", int MaxLength = 80, int MinLength = 0, string RestrictChars = null, bool ReturnNullForEscape = false, bool EscapeNonMarkupFormatting = true, bool? AllowColorize = null, bool pushView = false, string WantsSpecificPrompt = null)
	{
		ControlManager.ResetInput();
		List<QudMenuItem> buttons = ((AllowColorize ?? Options.GetOption("OptionEnableColorTextInput").EqualsNoCase("Yes")) ? PopupMessage.SubmitCancelColorButton : PopupMessage.SubmitCancelButton);
		if (WantsSpecificPrompt != null)
		{
			if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.PC)
			{
				buttons = PopupMessage.SubmitCancelHoldButton;
			}
			else if (Default == null)
			{
				buttons = PopupMessage.CancelHoldButton;
			}
		}
		string result = Default;
		if (pushView)
		{
			GameManager.Instance.PushGameView("ModernPopup:AskString");
		}
		try
		{
			QudMenuItem qudMenuItem;
			while (true)
			{
				qudMenuItem = await NewPopupMessageAsync(StringFormat.ClipText(Message, 60, KeepNewlines: true), buttons, null, null, result, 0, null, null, null, showContextFrame: true, EscapeNonMarkupFormatting: true, pushView: false, default, askingNumber: false, "", WantsSpecificPrompt);
				if (!(qudMenuItem.command == "Color"))
				{
					break;
				}
				result = qudMenuItem.text;
				string text = await ShowColorPickerAsync("Choose color", 0, null, 60, RespectOptionNewlines: false, AllowEscape: true, null, "", includeNone: true, includePatterns: true, allowBackground: false, ConsoleLib.Console.ColorUtility.StripFormatting(result));
				if (!string.IsNullOrEmpty(text))
				{
					if (string.IsNullOrWhiteSpace(result))
					{
						result = " ";
					}
					result = "{{" + text + "|" + ConsoleLib.Console.ColorUtility.StripFormatting(result) + "}}";
				}
			}
			if (qudMenuItem.command == "Cancel")
			{
				return ReturnNullForEscape ? null : "";
			}
			return qudMenuItem.text;
		}
		catch (Exception x)
		{
			MetricsManager.LogError("AskStringAsync", x);
			return null;
		}
		finally
		{
			Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
			if (pushView)
			{
				GameManager.Instance.PopGameView(bHard: true);
			}
		}
	}

	public static string AskString(string Message, string Default = "", string Sound = "Sounds/UI/ui_notification", string RestrictChars = null, string WantsSpecificPrompt = null, int MaxLength = 80, int MinLength = 0, bool ReturnNullForEscape = false, bool EscapeNonMarkupFormatting = true, bool? AllowColorize = null)
	{
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		Message = Markup.Transform(Message);
		string text = "";
		int startingLine = 0;
		text = Default ?? "";
		if (UIManager.UseNewPopups)
		{
			return AskStringAsync(Message, Default, MaxLength, MinLength, RestrictChars, ReturnNullForEscape, EscapeNonMarkupFormatting, AllowColorize, pushView: true, WantsSpecificPrompt).Result;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:AskString");
		int num = 95;
		while (num != 13)
		{
			ScrapBuffer.Copy(ScrapBuffer2);
			RenderBlock(Message + "\n" + text + "_", "[Enter to confirm]", "[{{W|Enter}} to confirm]", ScrapBuffer, startingLine);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			num = Keyboard.getch();
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("SubmitString:"))
			{
				text = Keyboard.CurrentMouseEvent.Event.Substring(Keyboard.CurrentMouseEvent.Event.IndexOf(':') + 1);
				if (text.Length >= MinLength)
				{
					Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
					GameManager.Instance.PopGameView(bHard: true);
					_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
					Loading.SetHideLoadStatus(hidden: false);
					return text;
				}
			}
			if (Keyboard.vkCode != Keys.MouseEvent)
			{
				if (Keyboard.vkCode == (Keys.V | Keys.Control))
				{
					string clipboardData = ClipboardHelper.GetClipboardData();
					text += clipboardData;
				}
				else
				{
					char c = (char)num;
					if ((char.IsDigit(c) || char.IsLetter(c) || char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c) || num == 32) && text.Length < MaxLength && (RestrictChars == null || RestrictChars.IndexOf(c) != -1))
					{
						text += c;
						if (EscapeNonMarkupFormatting && (c == '&' || c == '^'))
						{
							text += c;
						}
					}
				}
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdDelete")
			{
				if (EscapeNonMarkupFormatting && text.Length > 1 && (text.EndsWith("&&") || text.EndsWith("^^")))
				{
					text = text.Substring(0, text.Length - 2);
				}
				else if (text.Length > 0)
				{
					text = text.Substring(0, text.Length - 1);
				}
			}
			if ((Keyboard.vkCode == Keys.Escape || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && MinLength <= 0)
			{
				Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
				GameManager.Instance.PopGameView(bHard: true);
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Loading.SetHideLoadStatus(hidden: false);
				if (!ReturnNullForEscape)
				{
					return "";
				}
				return null;
			}
			if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick" && text.Length >= MinLength)
			{
				break;
			}
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		return text;
	}

	public static XRL.World.GameObject PickGameObject(string Title, IList<XRL.World.GameObject> Objects, bool AllowEscape = false, bool ShowContext = false, bool UseYourself = true, Func<XRL.World.GameObject, string> ExtraLabels = null, int DefaultSelected = 0, bool ShortDisplayNames = false)
	{
		string[] array = new string[Objects.Count];
		char[] array2 = new char[Objects.Count];
		IRenderable[] array3 = new IRenderable[Objects.Count];
		char c = 'a';
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			string text;
			if (UseYourself && Objects[i] == The.Player)
			{
				text = "yourself";
			}
			else
			{
				text = Objects[i].GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, ShortDisplayNames);
				string text2 = ExtraLabels?.Invoke(Objects[i]);
				string text3 = (ShowContext ? Objects[i].GetListDisplayContext(The.Player) : null);
				if (!text2.IsNullOrEmpty())
				{
					text = (text3.IsNullOrEmpty() ? (text + " [" + text2 + "]") : (text + " [" + text2 + ", " + text3 + "]"));
				}
				else if (!text3.IsNullOrEmpty())
				{
					text = text + " [" + text3 + "]";
				}
			}
			array[i] = text;
			array2[i] = ((c <= 'z') ? c++ : ' ');
			array3[i] = contextRender(Objects[i]);
		}
		int num = PickOption(Title, null, "", "Sounds/UI/ui_notification", array, array2, array3, null, null, null, null, 0, 60, DefaultSelected, -1, AllowEscape);
		if (num < 0)
		{
			return null;
		}
		return Objects[num];
	}

	private static RenderEvent contextRender(XRL.World.GameObject context)
	{
		return context?.RenderForUI();
	}

	[Obsolete("Use ShowConversation(IRenderable)")]
	public static int ShowConversation(string Title, XRL.World.GameObject Context, string Intro = null, List<string> Options = null, bool AllowTrade = false, bool AllowEscape = true, bool AllowRenderMapBehind = false)
	{
		return ShowConversation(Title, Context.RenderForUI(), Intro, Options, AllowTrade, AllowEscape, AllowRenderMapBehind);
	}

	public static int ShowConversation(string Title, IRenderable Icon = null, string Intro = null, List<string> Options = null, bool AllowTrade = false, bool AllowEscape = true, bool AllowRenderMapBehind = false)
	{
		List<QudMenuItem> list = new List<QudMenuItem>();
		if (AllowEscape)
		{
			list.Add(PopupMessage.CancelButton[0]);
		}
		if (AllowTrade)
		{
			list.Insert(0, PopupMessage.AcceptCancelTradeButton[0]);
		}
		list.Insert(0, PopupMessage.LookButton);
		int SelectedOption = 0;
		List<QudMenuItem> list2 = new List<QudMenuItem>(Options.Count);
		for (int i = 0; i < Options.Count; i++)
		{
			list2.Add(new QudMenuItem
			{
				text = ((i < 9 && CapabilityManager.AllowKeyboardHotkeys) ? ("{{w|[" + (i + 1) + "]}} ") : "") + Options[i] + "\n\n",
				command = "option:" + i,
				hotkey = ((i < 9) ? ("Alpha" + (i + 1)) : null)
			});
		}
		WaitNewPopupMessage(Intro + "\n\n", list, (QudMenuItem item) =>
		{
			if (item.command == "Cancel")
			{
				SelectedOption = -1;
			}
			if (item.command == "trade")
			{
				SelectedOption = -2;
			}
			if (item.command == "Look")
			{
				SelectedOption = -3;
			}
			string command = item.command;
			if (command != null && command.StartsWith("option:"))
			{
				SelectedOption = Convert.ToInt32(item.command.Substring("option:".Length));
			}
		}, list2, "", null, 0, Title, Icon, null, showContextFrame: true, EscapeNonMarkupFormatting: true, null, "Conversation:" + Title, AllowRenderMapBehind);
		return SelectedOption;
	}

	[Obsolete("Use PickOptionAsync")]
	public static async Task<int> ShowOptionListAsync(string Title = "", IReadOnlyList<string> Options = null, IReadOnlyList<char> Hotkeys = null, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int DefaultSelected = 0, string SpacingText = "", XRL.World.GameObject context = null, IReadOnlyList<IRenderable> Icons = null, IRenderable IntroIcon = null, bool centerIntro = false, bool centerIntroIcon = true, int iconPosition = -1)
	{
		bool allowEscape = AllowEscape;
		return await PickOptionAsync(Title, Intro, SpacingText, Options, Hotkeys, Icons, context, IntroIcon, Spacing, MaxWidth, DefaultSelected, iconPosition, RespectOptionNewlines, allowEscape, centerIntro, centerIntroIcon);
	}

	public static async Task<int> PickOptionAsync(string Title = "", string Intro = null, string SpacingText = "", IReadOnlyList<string> Options = null, IReadOnlyList<char> Hotkeys = null, IReadOnlyList<IRenderable> Icons = null, XRL.World.GameObject Context = null, IRenderable IntroIcon = null, int Spacing = 0, int MaxWidth = 60, int DefaultSelected = 0, int IconPosition = -1, bool RespectOptionNewlines = false, bool AllowEscape = false, bool CenterIntro = false, bool CenterIntroIcon = true)
	{
		NavigationController framework = NavigationController.instance;
		NavigationContext oldContext = framework.activeContext;
		framework.activeContext = NavigationController.instance.suspensionContext;
		try
		{
			TaskCompletionSource<int> t = new TaskCompletionSource<int>();
			PickOption(Title, Intro, SpacingText, "Sounds/UI/ui_notification", Options, Hotkeys, Icons, null, Context, IntroIcon, (int choice) =>
			{
				t.TrySetResult(choice);
			}, Spacing, MaxWidth, DefaultSelected, IconPosition, AllowEscape, RespectOptionNewlines, CenterIntro, CenterIntroIcon, ForceNewPopup: true);
			return await t.Task;
		}
		finally
		{
			framework.activeContext = oldContext;
		}
	}

	[Obsolete("Use PickOption")]
	public static int ShowOptionList(string Title = "", IReadOnlyList<string> Options = null, IReadOnlyList<char> Hotkeys = null, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, int DefaultSelected = 0, string SpacingText = "", Action<int> onResult = null, XRL.World.GameObject context = null, IReadOnlyList<IRenderable> Icons = null, IRenderable IntroIcon = null, IReadOnlyList<QudMenuItem> Buttons = null, bool centerIntro = false, bool centerIntroIcon = true, int iconPosition = -1, bool forceNewPopup = false)
	{
		return PickOption(Title, Intro, SpacingText, "Sounds/UI/ui_notification", Options, Hotkeys, Icons, Buttons, context, IntroIcon, onResult, Spacing, MaxWidth, DefaultSelected, iconPosition, AllowEscape, RespectOptionNewlines, centerIntro, centerIntroIcon, forceNewPopup);
	}

	public static int PickOption(string Title = "", string Intro = null, string SpacingText = "", string Sound = "Sounds/UI/ui_notification", IReadOnlyList<string> Options = null, IReadOnlyList<char> Hotkeys = null, IReadOnlyList<IRenderable> Icons = null, IReadOnlyList<QudMenuItem> Buttons = null, XRL.World.GameObject Context = null, IRenderable IntroIcon = null, Action<int> OnResult = null, int Spacing = 0, int MaxWidth = 60, int DefaultSelected = 0, int IconPosition = -1, bool AllowEscape = false, bool RespectOptionNewlines = false, bool CenterIntro = false, bool CenterIntroIcon = true, bool ForceNewPopup = false, Location2D PopupLocation = null, string PopupID = null)
	{
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		if (Context != null && Intro == null && !UIManager.UseNewPopups)
		{
			Intro = Context.DisplayName;
		}
		Title = Markup.Transform(Title);
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		if (MaxWidth > 74)
		{
			MaxWidth = 74;
		}
		int num = 0;
		int SelectedOption = DefaultSelected;
		int num2 = 20;
		int num3 = 0;
		if (Hotkeys != null)
		{
			num3 += 3;
		}
		if (Icons != null && IconPosition == -1)
		{
			num3 += 2;
		}
		PopupTextBuilder popupTextBuilder = new PopupTextBuilder
		{
			SpacingText = SpacingText,
			Spacing = Spacing
		};
		popupTextBuilder.RespectNewlines = RespectOptionNewlines;
		popupTextBuilder.MaxWidth = MaxWidth - 4;
		if (!Intro.IsNullOrEmpty())
		{
			popupTextBuilder.AddSection(Intro, -1);
			popupTextBuilder.AddSpacing(1, "");
		}
		popupTextBuilder.Padding = 2 + num3;
		for (int i = 0; i < Options.Count; i++)
		{
			popupTextBuilder.AddSection(Options[i], i);
			if (i != Options.Count - 1)
			{
				popupTextBuilder.AddSpacing(Spacing);
			}
		}
		int num4 = Math.Min(MaxWidth, Math.Max(ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Title) + 2, popupTextBuilder.MaxClippedWidth + 4));
		if (popupTextBuilder.Lines.Count < num2)
		{
			num2 = popupTextBuilder.Lines.Count;
		}
		else
		{
			num = SelectedOption;
		}
		if (IntroIcon != null)
		{
			num2 += 2;
		}
		if (UIManager.UseNewPopups | ForceNewPopup)
		{
			List<QudMenuItem> list = new List<QudMenuItem>(Options.Count);
			for (int j = 0; j < Options.Count; j++)
			{
				list.Add(GetPopupOption(j, Options, Hotkeys, Icons));
			}
			IRenderable renderable;
			if (Context == null)
			{
				renderable = IntroIcon;
			}
			else
			{
				IRenderable renderable2 = contextRender(Context);
				renderable = renderable2;
			}
			IRenderable renderable3 = renderable;
			List<QudMenuItem> list2 = new List<QudMenuItem>();
			if (!Buttons.IsReadOnlyNullOrEmpty())
			{
				list2.AddRange(Buttons);
			}
			if (AllowEscape)
			{
				list2.AddRange(PopupMessage.CancelButton);
			}
			if (string.IsNullOrEmpty(Title) && Context != null)
			{
				Title = Context?.DisplayName;
			}
			WaitNewPopupMessage(Intro, list2, (QudMenuItem item) =>
			{
				if (item.command == "Cancel")
				{
					SelectedOption = -1;
				}
				else
				{
					string command = item.command;
					if (command != null && command.StartsWith("option:"))
					{
						SelectedOption = Convert.ToInt32(item.command.Substring("option:".Length));
					}
				}
				if (OnResult != null)
				{
					OnResult(SelectedOption);
				}
			}, list, (renderable3 == null) ? Title : null, null, DefaultSelected, (renderable3 == null) ? null : Title, renderable3, null, showContextFrame: true, EscapeNonMarkupFormatting: true, PopupLocation, PopupID);
			Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
			return SelectedOption;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Popup:Choice");
		Keys[] array = null;
		if (!Buttons.IsReadOnlyNullOrEmpty())
		{
			array = new Keys[Buttons.Count];
			for (int num5 = 0; num5 < Buttons.Count; num5++)
			{
				try
				{
					UnityEngine.KeyCode key = Keyboard.ParseUnityEngineKeyCode(Buttons[num5].hotkey);
					Keyboard.Keymap.TryGetValue(key, out var value);
					array[num5] = value;
				}
				catch
				{
				}
			}
		}
		Keys keys = Keys.Space;
		bool flag = true;
		int num6 = 0;
		int num7 = 0;
		while (flag || (Keyboard.RawCode != Keys.Space && Keyboard.RawCode != Keys.Enter && Keyboard.RawCode != Keys.Enter))
		{
			ScrapBuffer.Copy(ScrapBuffer2);
			int num8 = (80 - num4 - 4) / 2;
			int num9 = (25 - num2 - 2) / 2;
			int num10 = num8 + num4 + 4;
			int num11 = num9 + num2 + 3;
			ScrapBuffer.Fill(num8, num9, num10, num11, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			ScrapBuffer.ThickSingleBox(num8, num9, num10, num11, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			flag = false;
			int num12 = 0;
			if (IntroIcon != null)
			{
				if (CenterIntroIcon)
				{
					ScrapBuffer.Goto(num8 + 2 + (num4 - 1) / 2, num9 + num12 + 2);
				}
				else
				{
					ScrapBuffer.Goto(num8 + 2, num9 + num12 + 2);
				}
				ScrapBuffer.Write(IntroIcon);
				num12 += 2;
			}
			string[] selectionLines = popupTextBuilder.GetSelectionLines(-1);
			for (int num13 = 0; num13 < selectionLines.Length; num13++)
			{
				if (CenterIntro)
				{
					ScrapBuffer.Goto(num8 + 2 + (num4 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(selectionLines[num13])) / 2, num9 + num12 + 2 + num13);
				}
				else
				{
					ScrapBuffer.Goto(num8 + 2, num9 + num12 + 2 + num13);
				}
				ScrapBuffer.Write(selectionLines[num13]);
			}
			if (selectionLines.Length != 0)
			{
				num12 += selectionLines.Length + 1;
			}
			int selectionStart = popupTextBuilder.GetSelectionStart(0);
			int num14 = num;
			while (num12 <= num2 && num14 < Options.Count)
			{
				bool flag2 = SelectedOption == num14;
				int selectionStart2 = popupTextBuilder.GetSelectionStart(num14);
				string[] selectionLines2 = popupTextBuilder.GetSelectionLines(num14);
				int num15;
				for (num15 = 0; num15 < selectionLines2.Length; num15++)
				{
					if (num12 > num2)
					{
						break;
					}
					bool flag3 = num15 == 0;
					if (flag2 & flag3)
					{
						ScrapBuffer.Goto(num8 + 2, num9 + 2 + num12);
						ScrapBuffer.Write("{{Y|>}} ");
					}
					if (Hotkeys != null)
					{
						char c = Hotkeys[num14];
						if (flag3 && c != ' ')
						{
							ScrapBuffer.Goto(num8 + 4, num9 + 2 + num12);
							ScrapBuffer.Write((flag2 ? "{{W|" : "{{w|") + c + "}}) ");
						}
					}
					ScrapBuffer.Goto(num8 + num3 + 4, num9 + 2 + num12);
					ScrapBuffer.Write(StringFormat.SubstringNotCountingFormat(selectionLines2[num15], 0, num4 - num3 - 2));
					if (num15 == 0 && Icons != null && num14 < Icons.Count && Icons[num14] != null)
					{
						if (IconPosition == -1)
						{
							ScrapBuffer.Goto(num8 + num3 + 2, num9 + 2 + num12);
						}
						else
						{
							ScrapBuffer.Goto(num8 + num3 + 2 + IconPosition, num9 + 2 + num12);
						}
						ScrapBuffer.Write(Icons[num14]);
					}
					num6 = selectionStart2 + num15;
					num12++;
				}
				if (num15 == selectionLines2.Length)
				{
					num7 = num14;
				}
				num14++;
			}
			if (popupTextBuilder.Lines.Count > num2)
			{
				if (num6 + 1 < popupTextBuilder.Lines.Count)
				{
					ScrapBuffer.Goto(num8 + 2, num11);
					ScrapBuffer.Write("{{W|<More...>}}");
				}
				if (num > 0)
				{
					ScrapBuffer.Goto(num8 + 2, num9);
					ScrapBuffer.Write("{{W|<More...>}}");
				}
				int num16 = num11 - num9 - 2;
				if (num16 > 0)
				{
					ScreenBuffer scrapBuffer = ScrapBuffer;
					int top = num9 + 1;
					int selectionStart3 = popupTextBuilder.GetSelectionStart(num);
					int handleEnd = num6;
					ScrollbarHelper.Paint(scrapBuffer, top, num10, num16, ScrollbarHelper.Orientation.Vertical, selectionStart, popupTextBuilder.Lines.Count - 1, selectionStart3, handleEnd);
				}
			}
			if (!Buttons.IsReadOnlyNullOrEmpty())
			{
				ScrapBuffer.Goto(num8 + 14, num11);
				foreach (QudMenuItem Button in Buttons)
				{
					ScrapBuffer.Write(Button.text);
					ScrapBuffer.X++;
				}
			}
			if (!Title.IsNullOrEmpty())
			{
				ScrapBuffer.Goto(num8 + 2, num9);
				ScrapBuffer.Write(StringFormat.ClipLine(" " + Title, num4, AddEllipsis: false));
			}
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(XRL.UI.Options.MapDirectionsToKeypad);
			if (Icons != null || IntroIcon != null)
			{
				ScreenBuffer.ClearImposterSuppression();
			}
			if (Hotkeys != null)
			{
				for (int num17 = 0; num17 < Hotkeys.Count; num17++)
				{
					if (Keyboard.Char == Hotkeys[num17] && Hotkeys[num17] != ' ')
					{
						SelectedOption = num17;
						Keyboard.RawCode = Keys.Space;
						break;
					}
				}
			}
			if (array != null)
			{
				for (int num18 = 0; num18 < array.Length; num18++)
				{
					if (keys == array[num18] && Buttons[num18].command != null && Buttons[num18].command.StartsWith("option:"))
					{
						Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
						return Convert.ToInt32(Buttons[num18].command.Substring("option:".Length));
					}
				}
			}
			if (Buttons != null)
			{
				for (int num19 = 0; num19 < Buttons.Count; num19++)
				{
					if (Buttons[num19].command != null && keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Command:") && Keyboard.CurrentMouseEvent.Event == "Command:" + Buttons[num19].hotkey && Buttons[num19].command.StartsWith("option:"))
					{
						Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
						return Convert.ToInt32(Buttons[num19].command.Substring("option:".Length));
					}
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Choice:"))
			{
				SelectedOption = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
				break;
			}
			if (AllowEscape && (keys == Keys.Escape || Keyboard.vkCode == Keys.Escape))
			{
				_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
				Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
				GameManager.Instance.PopGameView(bHard: true);
				return -1;
			}
			if (keys == Keys.NumPad2)
			{
				SelectedOption++;
			}
			if (keys == Keys.NumPad8)
			{
				SelectedOption--;
			}
			if (Keyboard.IsCommandKey("Page Down"))
			{
				num = (SelectedOption = num7 + 1);
			}
			if (Keyboard.IsCommandKey("Page Up"))
			{
				int selectionStart4 = popupTextBuilder.GetSelectionStart(num);
				for (int num20 = num2 - selectionStart; SelectedOption > 0 && popupTextBuilder.GetSelectionStart(SelectedOption) + num20 > selectionStart4; SelectedOption--)
				{
				}
				num = SelectedOption;
			}
			SelectedOption = Math.Max(0, Math.Min(Options.Count - 1, SelectedOption));
			if (num > SelectedOption)
			{
				num = SelectedOption;
			}
			if (SelectedOption > num7)
			{
				int num21 = num2 - selectionStart;
				for (int selectionStart5 = popupTextBuilder.GetSelectionStart(SelectedOption + 1); num < Options.Count && popupTextBuilder.GetSelectionStart(num) + num21 < selectionStart5; num++)
				{
				}
			}
		}
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		Loading.SetHideLoadStatus(hidden: false);
		if (Keyboard.vkCode == Keys.Escape)
		{
			return -1;
		}
		return SelectedOption;
	}

	public static QudMenuItem GetPopupOption(int Index, IReadOnlyList<string> Options, IReadOnlyList<char> Hotkeys = null, IReadOnlyList<IRenderable> Icons = null)
	{
		if (!CapabilityManager.AllowKeyboardHotkeys)
		{
			Hotkeys = null;
		}
		if (Options == null || Index >= Options.Count)
		{
			throw new ArgumentOutOfRangeException("Index");
		}
		QudMenuItem result = default;
		bool flag = !Hotkeys.IsReadOnlyNullOrEmpty();
		char c = ((flag && Index < Hotkeys.Count) ? Hotkeys[Index] : '\0');
		if (c == ' ')
		{
			c = '\0';
		}
		result.icon = ((Icons != null && Index < Icons.Count) ? Icons[Index] : null);
		result.hotkey = ((c != '\0') ? ("char:" + c) : "");
		result.command = "option:" + Index;
		if (!flag)
		{
			result.text = string.Concat("{{y|" + Options[Index] + "}}");
		}
		else if (c == '\0')
		{
			result.text = string.Concat("    {{y|" + Options[Index] + "}}");
		}
		else
		{
			result.text = "{{W|[" + c + "]}} {{y|" + Options[Index] + "}}";
		}
		return result;
	}

	public static List<(int Selected, int Amount)> PickSeveral(string Title = "", string Intro = null, string SpacingText = "", string Sound = "Sounds/UI/ui_notification", IReadOnlyList<string> Options = null, IReadOnlyList<char> Hotkeys = null, IReadOnlyList<int> Stacks = null, IReadOnlyList<IRenderable> Icons = null, XRL.World.GameObject Context = null, IRenderable IntroIcon = null, Action<int> OnResult = null, int Amount = -1, int Spacing = 0, int MaxWidth = 60, int DefaultSelected = 0, int IconPosition = -1, bool RespectOptionNewlines = false, bool AllowEscape = false, bool CenterIntro = false, bool CenterIntroIcon = true, bool ForceNewPopup = false)
	{
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		List<(int, int)> list = new List<(int, int)>();
		string[] array = new string[Options.Count];
		QudMenuItem[] array2 = new QudMenuItem[2]
		{
			new QudMenuItem
			{
				text = "{{y|[" + ControlManager.getCommandInputFormatted("CmdDelete") + "] Accept}}",
				command = "option:-2",
				hotkey = "CmdDelete"
			},
			new QudMenuItem
			{
				command = "option:-3",
				hotkey = "Take All"
			}
		};
		while (true)
		{
			for (int i = 0; i < array.Length; i++)
			{
				int num = 0;
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					if (list[num2].Item1 == i)
					{
						num = list[num2].Item2;
						break;
					}
				}
				string text = "";
				if (num != 0)
				{
					text = ((Stacks == null || Stacks[i] <= num) ? "{{W|[þ]}} " : ("{{W|[" + num + "]}} "));
				}
				else
				{
					text = "{{y|[ ]}} ";
				}
				array[i] = text + Options[i];
			}
			array2[1].text = ((list.Count == array.Length) ? ("{{y|[" + ControlManager.getCommandInputFormatted("Take All") + "] Deselect All}}") : ("{{y|[" + ControlManager.getCommandInputFormatted("Take All") + "] Select All}}"));
			int num3 = PickOption(Title, Intro, SpacingText, null, array, Hotkeys, Icons, array2, Context, IntroIcon, OnResult, Spacing, MaxWidth, DefaultSelected, IconPosition, AllowEscape, RespectOptionNewlines, CenterIntro, CenterIntroIcon, ForceNewPopup);
			switch (num3)
			{
			case -1:
				return null;
			case -2:
				if (Amount >= 0 && list.Count > Amount)
				{
					Show("You cannot select more than " + Grammar.Cardinal(Amount) + " options!");
					continue;
				}
				Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
				return list;
			case -3:
				if (list.Count != array.Length)
				{
					list.Clear();
					for (int j = 0; j < Options.Count; j++)
					{
						list.Add((j, Stacks?[j] ?? 1));
					}
				}
				else
				{
					list.Clear();
				}
				continue;
			}
			int num4 = -1;
			for (int num5 = list.Count - 1; num5 >= 0; num5--)
			{
				if (list[num5].Item1 == num3)
				{
					num4 = num5;
					break;
				}
			}
			if (num4 >= 0)
			{
				list.RemoveAt(num4);
			}
			else if (Stacks != null && Stacks[num3] > 1)
			{
				int valueOrDefault = AskNumber("Select how many?", null, "", Stacks[num3], 0, Stacks[num3]).GetValueOrDefault();
				if (valueOrDefault != 0)
				{
					list.Add((num3, valueOrDefault));
				}
			}
			else
			{
				list.Add((num3, 1));
			}
			DefaultSelected = num3;
		}
	}

	private static void SetupColorPickers(string previewContent, bool includeNone, bool includePatterns, bool allowBackground)
	{
		ColorPickerOptionStrings.Clear();
		ColorPickerOptions.Clear();
		ColorPickerKeymap.Clear();
		char c = 'a';
		if (includeNone)
		{
			if (string.IsNullOrWhiteSpace(previewContent))
			{
				ColorPickerOptionStrings.Add("none");
			}
			else
			{
				ColorPickerOptionStrings.Add(previewContent + " (no coloring)");
			}
			ColorPickerOptions.Add("");
			ColorPickerKeymap.Add(c++);
		}
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		StringBuilder stringBuilder2 = XRL.World.Event.NewStringBuilder();
		foreach (SolidColor pickerColor in MarkupShaders.PickerColors)
		{
			if (allowBackground || !pickerColor.Background.HasValue || pickerColor.Background == 'k')
			{
				stringBuilder.Clear();
				stringBuilder2.Clear();
				if (allowBackground)
				{
					pickerColor.AssembleCode(stringBuilder2);
					stringBuilder.Append(stringBuilder2);
				}
				else
				{
					stringBuilder2.Append(pickerColor.Foreground);
					pickerColor.AssembleCode(stringBuilder);
				}
				if (string.IsNullOrWhiteSpace(previewContent))
				{
					stringBuilder.Append(pickerColor.GetDisplayName());
				}
				else
				{
					stringBuilder.Append(previewContent).Append("^k&y (").Append(pickerColor.GetDisplayName())
						.Append(")");
				}
				ColorPickerOptionStrings.Add(stringBuilder.ToString());
				ColorPickerOptions.Add(stringBuilder2.ToString());
				ColorPickerKeymap.Add(c++);
			}
		}
		if (!includePatterns)
		{
			return;
		}
		List<IMarkupShader> list = new List<IMarkupShader>(MarkupShaders.PickerShaders);
		list.Sort((IMarkupShader a, IMarkupShader b) => string.Compare(a.GetDisplayName(), b.GetDisplayName(), StringComparison.CurrentCultureIgnoreCase));
		foreach (IMarkupShader item in list)
		{
			if (string.IsNullOrWhiteSpace(previewContent))
			{
				ColorPickerOptionStrings.Add(stringBuilder.Clear().Append("{{").Append(item.GetName())
					.Append('|')
					.Append(item.GetDisplayName())
					.Append("}}")
					.ToString());
			}
			else
			{
				ColorPickerOptionStrings.Add(stringBuilder.Clear().Append("{{").Append(item.GetName())
					.Append('|')
					.Append(previewContent)
					.Append("}} (")
					.Append(item.GetDisplayName())
					.Append(")")
					.ToString());
			}
			ColorPickerOptions.Add(item.GetName());
			ColorPickerKeymap.Add(' ');
		}
	}

	public static string ShowColorPicker(string Title, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, string DefaultSelected = null, string SpacingText = "", bool includeNone = true, bool includePatterns = false, bool allowBackground = false, string previewContent = null)
	{
		SetupColorPickers(previewContent, includeNone, includePatterns, allowBackground);
		int num = PickOption(Title, Intro, SpacingText, "Sounds/UI/ui_notification", ColorPickerOptionStrings.ToArray(), ColorPickerKeymap.ToArray(), null, null, null, null, null, Spacing, MaxWidth, (!DefaultSelected.IsNullOrEmpty()) ? Math.Max(ColorPickerOptions.IndexOf(DefaultSelected), 0) : 0, -1, AllowEscape, RespectOptionNewlines);
		if (num < 0)
		{
			return null;
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		return ColorPickerOptions[num];
	}

	public static async Task<string> ShowColorPickerAsync(string Title, int Spacing = 0, string Intro = null, int MaxWidth = 60, bool RespectOptionNewlines = false, bool AllowEscape = false, string DefaultSelected = null, string SpacingText = "", bool includeNone = true, bool includePatterns = false, bool allowBackground = false, string previewContent = null)
	{
		SetupColorPickers(previewContent, includeNone, includePatterns, allowBackground);
		int num = await PickOptionAsync(Title, Intro, SpacingText, ColorPickerOptionStrings.ToArray(), ColorPickerKeymap.ToArray(), null, null, null, Spacing, MaxWidth, (!DefaultSelected.IsNullOrEmpty()) ? Math.Max(ColorPickerOptions.IndexOf(DefaultSelected), 0) : 0, -1, RespectOptionNewlines, AllowEscape);
		if (num < 0)
		{
			return null;
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		return ColorPickerOptions[num];
	}

	public static DialogResult WarnYesNo(string Message, string Sound = "Sounds/UI/ui_notification_warning", bool AllowEscape = true, DialogResult Default = DialogResult.Cancel)
	{
		return ShowYesNo(Message, Sound, AllowEscape, Default);
	}

	public static DialogResult ShowYesNo(string Message, string Sound = "Sounds/UI/ui_notification", bool AllowEscape = true, DialogResult defaultResult = DialogResult.Yes, Action<DialogResult> callback = null)
	{
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		Keyboard.ClearMouseEvents();
		int num = 0;
		DialogResult result = defaultResult;
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, PopupMessage.YesNoButton, (QudMenuItem i) =>
			{
				if (i.command == "No")
				{
					result = DialogResult.No;
				}
				if (i.command == "Yes")
				{
					result = DialogResult.Yes;
				}
				if (i.command == "Cancel")
				{
					result = DialogResult.No;
				}
				callback?.Invoke(result);
			});
			return result;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		Keys keys = Keys.Space;
		bool flag = true;
		while (flag || ((Keyboard.vkCode != Keys.MouseEvent || !Keyboard.CurrentMouseEvent.Event.StartsWith("LeftOption:")) && Keyboard.RawCode != Keys.Y && Keyboard.RawCode != Keys.N && !((Keyboard.vkCode == Keys.Escape) & AllowEscape) && Keyboard.vkCode != Keys.Space && Keyboard.vkCode != Keys.Enter))
		{
			flag = false;
			int num2 = RenderBlock(Message, "[Yes] [No]", "[{{W|Y}}es] [{{W|N}}o]", ScrapBuffer, num, -1, -1, null, BottomLineRegions: true);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior || keys == Keys.Next)
			{
				num -= 23;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		if (Keyboard.RawCode == Keys.Y || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:1"))
		{
			result = DialogResult.Yes;
		}
		if (Keyboard.RawCode == Keys.N || ((keys == Keys.Escape) & AllowEscape) || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:2"))
		{
			result = DialogResult.No;
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		return result;
	}

	public static async Task<DialogResult> ShowYesNoAsync(string Message)
	{
		DialogResult result = DialogResult.Cancel;
		QudMenuItem qudMenuItem = await NewPopupMessageAsync(Message, PopupMessage.YesNoButton);
		if (qudMenuItem.command == "No")
		{
			result = DialogResult.No;
		}
		if (qudMenuItem.command == "Yes")
		{
			result = DialogResult.Yes;
		}
		return result;
	}

	public static async Task<DialogResult> ShowYesNoCancelAsync(string Message)
	{
		DialogResult result = DialogResult.Cancel;
		QudMenuItem qudMenuItem = await NewPopupMessageAsync(Message, PopupMessage.YesNoCancelButton);
		if (qudMenuItem.command == "No")
		{
			result = DialogResult.No;
		}
		if (qudMenuItem.command == "Yes")
		{
			result = DialogResult.Yes;
		}
		if (qudMenuItem.command == "Cancel")
		{
			result = DialogResult.Cancel;
		}
		return result;
	}

	public static DialogResult WarnYesNoCancel(string Message, string Sound = "Sounds/UI/ui_notification_warning", bool AllowEscape = true, DialogResult Default = DialogResult.Cancel)
	{
		return ShowYesNoCancel(Message, Sound, AllowEscape, Default);
	}

	public static DialogResult ShowYesNoCancel(string Message, string Sound = "Sounds/UI/ui_notification", bool AllowEscape = true, DialogResult defaultResult = DialogResult.Cancel)
	{
		SoundManager.PlayUISound(Sound, 1f, Combat: false, Interface: true);
		Message = Markup.Transform(Options.OverlayUI ? ("{{white|" + Message + "}}") : Message);
		int num = 0;
		DialogResult result = defaultResult;
		if (UIManager.UseNewPopups)
		{
			WaitNewPopupMessage(Message, PopupMessage.YesNoCancelButton, (QudMenuItem i) =>
			{
				if (i.command == "No")
				{
					result = DialogResult.No;
				}
				if (i.command == "Yes")
				{
					result = DialogResult.Yes;
				}
				if (i.command == "Cancel")
				{
					result = DialogResult.Cancel;
				}
			});
			return result;
		}
		ScrapBuffer.Copy(TextConsole.CurrentBuffer);
		ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
		new TextBlock(Message, 80, 5000);
		GameManager.Instance.PushGameView("Popup:MessageBox");
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		Keys keys = Keys.Space;
		while (Keyboard.RawCode != Keys.Y && Keyboard.RawCode != Keys.Enter && Keyboard.RawCode != Keys.N && (Keyboard.vkCode != Keys.MouseEvent || !Keyboard.CurrentMouseEvent.Event.StartsWith("LeftOption:")))
		{
			GameManager.Instance.ClearRegions();
			string text;
			string bottomLineNoFormat;
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				text = "[" + ControlManager.getCommandInputFormatted("Accept") + "-Yes] [" + ControlManager.getCommandInputFormatted("V Negative") + "-No] [" + ControlManager.getCommandInputFormatted("Cancel") + "-Cancel]";
				bottomLineNoFormat = ConsoleLib.Console.ColorUtility.StripFormatting(text);
			}
			else
			{
				bottomLineNoFormat = "[Yes] [No] [ESC-Cancel]";
				text = "[{{W|Y}}es] [{{W|N}}o] [{{W|ESC}}-Cancel]";
			}
			int num2 = RenderBlock(Message, bottomLineNoFormat, text, ScrapBuffer, num, -1, -1, null, BottomLineRegions: true);
			_TextConsole.DrawBuffer(ScrapBuffer, null, bSkipIfOverlay: true);
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.Space)
			{
				Keyboard.RawCode = Keys.Y;
			}
			if (keys == Keys.Escape)
			{
				Keyboard.RawCode = Keys.Escape;
			}
			if (keys == Keys.MouseEvent)
			{
				if (Keyboard.CurrentMouseEvent.Event == "Command:Accept")
				{
					Keyboard.RawCode = Keys.Y;
				}
				if (Keyboard.CurrentMouseEvent.Event == "Command:V Negative")
				{
					Keyboard.RawCode = Keys.N;
				}
				if (Keyboard.CurrentMouseEvent.Event == "Command:Cancel")
				{
					Keyboard.RawCode = Keys.Escape;
				}
			}
			if (keys == Keys.NumPad2 && num2 - num > 20)
			{
				num++;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.Next)
			{
				num += 23;
			}
			if (keys == Keys.Prior || keys == Keys.Next)
			{
				num -= 23;
			}
			if ((keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && AllowEscape)
			{
				break;
			}
			if (num < 0)
			{
				num = 0;
			}
		}
		_TextConsole.DrawBuffer(ScrapBuffer2, null, bSkipIfOverlay: true);
		if (Keyboard.RawCode == Keys.Y || Keyboard.RawCode == Keys.Enter || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:1"))
		{
			result = DialogResult.Yes;
		}
		if (Keyboard.RawCode == Keys.N || (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftOption:2"))
		{
			result = DialogResult.No;
		}
		Keyboard.ClearInput(LeaveMovementEvents: false, clearFrameDown: true);
		GameManager.Instance.PopGameView(bHard: true);
		return result;
	}

	public static void DisplayLoadError(SerializationReader Reader, string Loadable, int Errors = 1)
	{
		bool flag = Errors == 1;
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		stringBuilder.Append("There").Compound(flag ? "was" : "were").Compound(Errors)
			.Compound(flag ? "error" : "errors")
			.Compound("while loading this")
			.Compound(Loadable)
			.Append('.');
		if (DisplayedLoadError)
		{
			MessageQueue.AddPlayerMessage(XRL.World.Event.FinalizeString(stringBuilder));
		}
		bool flag2 = false;
		foreach (string item in Reader.GetSavedMods().Except(ModManager.GetRunningMods()))
		{
			if (flag2)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag2 = true;
				stringBuilder.Append("\n\nMissing mods: ");
			}
			stringBuilder.Append(ModManager.GetModTitle(item));
		}
		if (flag2)
		{
			stringBuilder.Append('.');
		}
		DisplayedLoadError = true;
		stringBuilder.Append("\n\nDo you want to examine these errors in the Player.log?");
		if (ShowYesNoCancel(XRL.World.Event.FinalizeString(stringBuilder)) == DialogResult.Yes)
		{
			DataManager.Open(Path.Join(XRLCore.SavePath, "Player.log"));
		}
	}
}
