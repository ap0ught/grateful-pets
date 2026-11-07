using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.UI;
using XRL.Rules;
using XRL.World;

namespace XRL.UI;

[UIView("WorldCreationProgress", false, true, false, "Menu", null, false, 0, false)]
public class WorldCreationProgress : IWantsTextConsoleInit
{
	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public static List<StepEntry> Steps;

	public static int CurrentStep = -1;

	public static int MaxSteps = 0;

	public static BookPage Page = null;

	public static void DrawProgressBar(ScreenBuffer Buffer, int xp, int yp, int Length, int Percentage)
	{
		Buffer.Goto(xp, yp);
		int num = (int)((float)Length * ((float)Percentage / 100f));
		for (int i = 0; i < num; i++)
		{
			Buffer.Write("&g^y±");
		}
		if (Percentage >= 100)
		{
			Buffer.Write("&g^y±");
			return;
		}
		Buffer.Write("&b^g°");
		for (int j = 0; j < Length - num; j++)
		{
			Buffer.Write("&K°");
		}
	}

	public static void Begin(int TotalSteps)
	{
		Steps = new List<StepEntry>();
		CurrentStep = -1;
		MaxSteps = TotalSteps;
		Page = BookUI.Books["Quotes"][Stat.Random(0, BookUI.Books["Quotes"].Count - 1)];
	}

	public static void Draw(bool Last = false)
	{
		_ScreenBuffer.Clear();
		_ScreenBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		_ScreenBuffer.Goto(31, 0);
		_ScreenBuffer.Write("[ Creating World ]");
		for (int i = 0; i < Steps.Count; i++)
		{
			int num = 5;
			int num2 = 2 + i * 3;
			_ScreenBuffer.Goto(num, num2);
			_ScreenBuffer.Write(Steps[i].Text);
			if (Steps[i].CurrentStep >= Steps[i].MaxSteps)
			{
				_ScreenBuffer.Write(" : &GComplete!");
			}
			else if (Steps[i].StepText != null && Steps[i].StepText != "")
			{
				_ScreenBuffer.Write(" : " + Steps[i].StepText);
			}
			DrawProgressBar(_ScreenBuffer, num, num2 + 1, 68, Steps[i].CurrentStep * 100 / Steps[i].MaxSteps);
		}
		_ScreenBuffer.Goto(3, 22);
		DrawProgressBar(_ScreenBuffer, 3, 24, 74, Last ? 100 : (Steps.Count * 100 / MaxSteps));
		if (Page != null)
		{
			int num3 = 22 - Page.Lines.Count;
			foreach (string line in Page.Lines)
			{
				_ScreenBuffer.Goto(5, num3);
				_ScreenBuffer.Write("&y" + line);
				num3++;
			}
		}
		_TextConsole.DrawBuffer(_ScreenBuffer);
	}

	public static void NextStep(string Text, int TotalSteps)
	{
		if (GameManager.IsOnGameContext())
		{
			Event.ResetPool();
		}
		WorldGenerationScreen.AddMessage(Text);
		if (CurrentStep > -1)
		{
			Steps[CurrentStep].CurrentStep = Steps[CurrentStep].MaxSteps;
		}
		StepEntry stepEntry = new StepEntry();
		stepEntry.CurrentStep = 0;
		stepEntry.MaxSteps = TotalSteps;
		stepEntry.StepText = "";
		stepEntry.Text = Text;
		Steps.Add(stepEntry);
		CurrentStep++;
		Draw();
	}

	public static void StepProgress(string StepText, bool Last = false)
	{
		WorldGenerationScreen.IncrementProgress();
		WorldGenerationScreen.AddMessage(StepText);
		Steps[CurrentStep].CurrentStep++;
		Steps[CurrentStep].StepText = StepText;
		Draw(Last);
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}
}
