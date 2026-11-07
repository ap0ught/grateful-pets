using System.Diagnostics;
using ConsoleLib.Console;
using Qud.UI;
using XRL.World;

namespace XRL.UI;

public class GenericTerminal
{
	public static GenericTerminal instance;

	public int nSelected;

	public int Position;

	public int nTopLine;

	public GameObject terminal;

	public GameObject obj;

	public GenericTerminalScreen _currentScreen;

	public GenericTerminalScreen currentScreen
	{
		get
		{
			return _currentScreen;
		}
		set
		{
			_currentScreen = value;
			if (_currentScreen != null)
			{
				nTopLine = 0;
				nSelected = 0;
				Position = 0;
				_currentScreen.Update();
			}
		}
	}

	public string currentText => currentScreen.renderedText;

	public static string ntoa(int n)
	{
		return n switch
		{
			1 => "A", 
			2 => "B", 
			3 => "C", 
			4 => "D", 
			5 => "E", 
			6 => "F", 
			7 => "G", 
			8 => "H", 
			9 => "I", 
			10 => "J", 
			11 => "K", 
			12 => "L", 
			13 => "M", 
			14 => "N", 
			15 => "O", 
			_ => "?", 
		};
	}

	public static void ShowTerminal(GameObject Terminal, GameObject Object, GenericTerminalScreen startingScreen)
	{
		instance = new GenericTerminal();
		instance._ShowTerminal(Terminal, Object, startingScreen);
		instance = null;
	}

	public void _ShowTerminal(GameObject Terminal, GameObject obj, GenericTerminalScreen startingScreen)
	{
		terminal = Terminal;
		this.obj = obj;
		currentScreen = startingScreen;
		GameManager.Instance.PushGameView("CyberneticsTerminalScreen");
		if (Options.ModernUI)
		{
			_ = CyberneticsTerminalScreen.ShowGenericTerminal(this).Result;
			return;
		}
		try
		{
			TextConsole.LoadScrapBuffers();
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer2;
			ScreenBuffer scrapBuffer2 = TextConsole.ScrapBuffer2;
			Keys keys = Keys.None;
			bool flag = false;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Stopwatch stopwatch2 = new Stopwatch();
			stopwatch2.Start();
			int num = 3;
			int num2 = 76;
			while (!flag)
			{
				Event.ResetPool();
				scrapBuffer2.Copy(scrapBuffer);
				scrapBuffer2.Fill(num, 2, num2, 24, 32, ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
				scrapBuffer2.BeveledBox(num, 2, num2, 24, ColorUtility.Bright(TextColor.Black), 1);
				scrapBuffer2.Goto(20, 2);
				string FooterText = null;
				if (currentScreen != null)
				{
					currentScreen.BeforeRender(scrapBuffer2, ref FooterText);
				}
				if (stopwatch2.ElapsedMilliseconds > 15)
				{
					Position++;
					stopwatch2.Reset();
					stopwatch2.Start();
				}
				int num3 = num + 2;
				int num4 = 4;
				int num5 = 0;
				string text = "";
				int i;
				for (i = 0; i < Position && i < currentScreen.renderedText.Length; i++)
				{
					if (num3 > num2 - 2)
					{
						if (num5 >= nTopLine)
						{
							num4++;
						}
						num3 = num + 2;
						num5++;
						if (num5 >= nTopLine + 19)
						{
							scrapBuffer2.Goto(num + 2, num4);
							scrapBuffer2.Write("<more...>");
							break;
						}
					}
					if (currentText[i] == '\r')
					{
						continue;
					}
					if (currentText[i] == '&')
					{
						text = "";
						text += currentText.Substring(i, 1);
						i++;
						text += currentText.Substring(i, 1);
					}
					else if (currentText[i] == '\n')
					{
						if (num5 >= nTopLine)
						{
							num4++;
						}
						num3 = num + 2;
						num5++;
						if (num5 >= nTopLine + 19)
						{
							scrapBuffer2.Goto(num + 2, num4);
							scrapBuffer2.Write("<more...>");
							break;
						}
					}
					else
					{
						if (num5 >= nTopLine)
						{
							scrapBuffer2.Goto(num3, num4);
							scrapBuffer2.Write(text + currentText.Substring(i, 1).ToUpper());
						}
						num3++;
					}
				}
				if (i >= currentScreen.renderedText.Length)
				{
					currentScreen.TextComplete();
				}
				if (stopwatch.ElapsedMilliseconds % 1000 > 500)
				{
					scrapBuffer2.Write("_");
				}
				Popup._TextConsole.DrawBuffer(scrapBuffer2);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				currentScreen.TextComplete();
				keys = Keyboard.getvk(MapDirectionToArrows: true);
				if (Position < currentScreen.renderedText.Length + 1)
				{
					Position = currentScreen.renderedText.Length + 1;
					continue;
				}
				if (keys >= Keys.A && keys <= Keys.Z)
				{
					int num6 = (int)(keys - 65);
					if (num6 < currentScreen.Options.Count)
					{
						nSelected = num6;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 19 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 19;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					keys = Keys.Space;
				}
				if (keys == Keys.Escape)
				{
					currentScreen.Back();
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					currentScreen.Activate();
				}
				if (currentScreen == null)
				{
					flag = true;
				}
				switch (keys)
				{
				case Keys.Prior:
					if (nSelected < 10)
					{
						nSelected = 0;
					}
					else
					{
						nSelected -= 10;
					}
					if (nSelected < 0)
					{
						nSelected = currentScreen.Options.Count - 1;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				case Keys.Next:
					if (nSelected < currentScreen.Options.Count - 11)
					{
						nSelected += 10;
					}
					else
					{
						nSelected = currentScreen.Options.Count - 1;
					}
					if (nSelected >= currentScreen.Options.Count)
					{
						nSelected = 0;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				case Keys.NumPad8:
					nSelected--;
					if (nSelected < 0)
					{
						nSelected = currentScreen.Options.Count - 1;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				case Keys.NumPad2:
					nSelected++;
					if (nSelected >= currentScreen.Options.Count)
					{
						nSelected = 0;
					}
					currentScreen.Update();
					if (nTopLine < currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected];
					}
					if (nTopLine + 18 > currentScreen.OptionLines[nSelected])
					{
						nTopLine = currentScreen.OptionLines[nSelected] - 18;
					}
					if (nTopLine < 0)
					{
						nTopLine = 0;
					}
					break;
				}
			}
		}
		finally
		{
			GameManager.Instance.PopGameView();
		}
	}
}
