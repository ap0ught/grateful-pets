using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ConsoleLib.Console;
using Qud.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class CyberneticsTerminal
{
	public static CyberneticsTerminal Instance;

	public List<CyberneticsCreditWedge> Wedges = new List<CyberneticsCreditWedge>();

	public List<GameObject> Implants = new List<GameObject>();

	public int Licenses;

	public int FreeLicenses;

	public int LicensesUsed;

	public int Credits;

	public int Selected;

	public int Position;

	public int TopLine;

	public GameObject Terminal;

	public GameObject Subject;

	public GameObject Actor;

	public TerminalScreen _CurrentScreen;

	public int LicensesRemaining => Licenses - LicensesUsed;

	public TerminalScreen CurrentScreen
	{
		get
		{
			return _CurrentScreen;
		}
		set
		{
			_CurrentScreen = value;
			if (_CurrentScreen == null)
			{
				return;
			}
			TopLine = 0;
			Selected = 0;
			Position = 0;
			_CurrentScreen.Update();
			Wedges.Clear();
			Credits = 0;
			Subject.ForeachInventoryAndEquipment((GameObject GO) =>
			{
				if (GO.TryGetPart<CyberneticsCreditWedge>(out var Part) && Part.Credits > 0)
				{
					Credits += Part.Credits * GO.Count;
					Wedges.Add(Part);
				}
			});
			Licenses = Subject.GetIntProperty("CyberneticsLicenses");
			FreeLicenses = Subject.GetIntProperty("FreeCyberneticsLicenses");
			Implants.Clear();
			Terminal.CurrentCell.ForeachAdjacentCell((Cell C) =>
			{
				C.ForeachObjectWithPart("Container", (GameObject GO) =>
				{
					GO.Inventory.ForeachObject((GameObject obj2) =>
					{
						if (obj2.IsImplant && obj2.Understood())
						{
							Implants.Add(obj2);
						}
					});
				});
			});
			Subject.Inventory.ForeachObject((GameObject obj2) =>
			{
				if (obj2.IsImplant && obj2.Understood())
				{
					Implants.Add(obj2);
				}
			});
			LicensesUsed = 0;
			Subject.Body.ForeachInstalledCybernetics((GameObject obj2) =>
			{
				if (obj2.IsImplant)
				{
					LicensesUsed += obj2.GetPart<CyberneticsBaseItem>().Cost;
				}
			});
			if (Implants.Count > 1)
			{
				Implants.Sort(SortImplants);
			}
		}
	}

	public string CurrentText => CurrentScreen.RenderedText;

	public bool Authorized => IsAuthorized(Subject);

	public int HackLevel => Terminal?.GetIntProperty("HackLevel") ?? 0;

	public int SecurityHardeningLevel
	{
		get
		{
			if (Terminal == null)
			{
				return 0;
			}
			if (!Terminal.HasIntProperty("SecurityHardeningLevel"))
			{
				Terminal.SetIntProperty("SecurityHardeningLevel", Stat.Random(0, 4));
			}
			return Terminal.GetIntProperty("SecurityHardeningLevel");
		}
	}

	public int SecurityAlertLevel => Terminal?.GetIntProperty("SecurityAlertLevel") ?? 0;

	public bool HackActive => HackLevel > SecurityAlertLevel;

	public bool LowLevelHack
	{
		get
		{
			GameObject terminal = Terminal;
			if (terminal == null)
			{
				return false;
			}
			return terminal.GetIntProperty("LowLevelHack") > 0;
		}
		set
		{
			if (value)
			{
				Terminal?.SetIntProperty("LowLevelHack", 1);
			}
			else
			{
				Terminal?.RemoveIntProperty("LowLevelHack");
			}
		}
	}

	public static void ShowTerminal(GameObject Terminal, GameObject Subject, GameObject Actor = null, TerminalScreen StartingScreen = null)
	{
		SoundManager.PlayUISound("Sounds/Interact/sfx_interact_computerterminal");
		Instance = new CyberneticsTerminal();
		Instance._ShowTerminal(Terminal, Subject, Actor, StartingScreen);
		Instance = null;
	}

	public int SortImplants(GameObject A, GameObject B)
	{
		int num = A.GetPart<CyberneticsBaseItem>()?.Cost ?? 0;
		int num2 = B.GetPart<CyberneticsBaseItem>()?.Cost ?? 0;
		bool num3 = A.HasTag("CyberneticsOneOnly") && Subject.HasInstalledCybernetics(A.Blueprint);
		bool flag = B.HasTag("CyberneticsOneOnly") && Subject.HasInstalledCybernetics(B.Blueprint);
		bool flag2 = num > LicensesRemaining;
		bool flag3 = num2 > LicensesRemaining;
		int num4 = (num3 | flag2).CompareTo(flag | flag3);
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = num.CompareTo(num2);
		if (num5 != 0)
		{
			return -num5;
		}
		return A.GetCachedDisplayNameForSort().CompareTo(B.GetCachedDisplayNameForSort());
	}

	public void _ShowTerminal(GameObject Terminal, GameObject Subject, GameObject Actor = null, TerminalScreen StartingScreen = null)
	{
		this.Terminal = Terminal;
		this.Subject = Subject;
		this.Actor = Actor ?? Subject;
		if (StartingScreen == null)
		{
			StartingScreen = (Authorized ? ((CyberneticsScreen)new CyberneticsScreenMainMenu()) : ((CyberneticsScreen)new CyberneticsScreenGoodbye()));
		}
		CurrentScreen = StartingScreen;
		GameManager.Instance.PushGameView("CyberneticsTerminalScreen");
		if (Options.ModernUI)
		{
			_ = CyberneticsTerminalScreen.ShowCyberneticsTerminal(this).Result;
			return;
		}
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
			if (CurrentScreen != null)
			{
				string footerText = null;
				CurrentScreen.BeforeRender(scrapBuffer2, ref footerText);
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
			for (i = 0; i < Position && i < CurrentScreen.RenderedText.Length; i++)
			{
				if (num3 > num2 - 2)
				{
					if (num5 >= TopLine)
					{
						num4++;
					}
					num3 = num + 2;
					num5++;
					if (num5 >= TopLine + 19)
					{
						scrapBuffer2.Goto(num + 2, num4);
						scrapBuffer2.Write("<more...>");
						break;
					}
				}
				if (CurrentText[i] == '\r')
				{
					continue;
				}
				if (CurrentText[i] == '&')
				{
					if (i < CurrentScreen.RenderedText.Length - 1 && CurrentText[i + 1] == '&')
					{
						i++;
						if (num5 >= TopLine)
						{
							scrapBuffer2.Goto(num3, num4);
							scrapBuffer2.Write(text + "&&");
						}
						num3++;
					}
					else
					{
						text = "";
						text += CurrentText.Substring(i, 1);
						i++;
						text += CurrentText.Substring(i, 1);
					}
				}
				else if (CurrentText[i] == '\n')
				{
					if (num5 >= TopLine)
					{
						num4++;
					}
					num3 = num + 2;
					num5++;
					if (num5 >= TopLine + 19)
					{
						scrapBuffer2.Goto(num + 2, num4);
						scrapBuffer2.Write("<more...>");
						break;
					}
				}
				else
				{
					if (num5 >= TopLine)
					{
						scrapBuffer2.Goto(num3, num4);
						scrapBuffer2.Write(text + CurrentText.Substring(i, 1).ToUpper());
					}
					num3++;
				}
			}
			if (i >= CurrentScreen.RenderedText.Length)
			{
				CurrentScreen.TextComplete();
			}
			if (stopwatch.ElapsedMilliseconds % 1000 > 500)
			{
				scrapBuffer2.Write("_");
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer2);
			if (Keyboard.kbhit())
			{
				CurrentScreen.TextComplete();
				keys = Keyboard.getvk(MapDirectionToArrows: true, pumpActions: false, wait: false);
				if (Position < CurrentScreen.RenderedText.Length + 1)
				{
					Position = CurrentScreen.RenderedText.Length + 1;
					Keyboard.ClearInput();
					continue;
				}
				bool flag2 = false;
				if (CurrentScreen.HackOption != -1 && keys == (Keys.Enter | Keys.Control))
				{
					Selected = CurrentScreen.HackOption;
					flag2 = true;
				}
				else if (keys >= Keys.A && keys <= Keys.Z)
				{
					int num6 = (int)(keys - 65);
					if (CurrentScreen.HackOption == -1)
					{
						if (num6 < CurrentScreen.Options.Count)
						{
							Selected = num6;
							flag2 = true;
						}
					}
					else if (num6 < CurrentScreen.Options.Count - 1)
					{
						Selected = ((num6 >= CurrentScreen.HackOption) ? (num6 - 1) : num6);
						flag2 = true;
					}
				}
				if (flag2)
				{
					CurrentScreen.Update();
					if (TopLine < CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected];
					}
					if (TopLine + 19 > CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected] - 19;
					}
					if (TopLine < 0)
					{
						TopLine = 0;
					}
					keys = Keys.Space;
				}
				if (keys == Keys.Escape)
				{
					CurrentScreen.Back();
				}
				if (keys == Keys.Space || keys == Keys.Enter)
				{
					CurrentScreen.Activate();
				}
				if (CurrentScreen == null)
				{
					flag = true;
				}
				switch (keys)
				{
				case Keys.Prior:
					if (Selected < 10)
					{
						Selected = 0;
					}
					else
					{
						Selected -= 10;
					}
					if (Selected < 0)
					{
						Selected = CurrentScreen.Options.Count - 1;
					}
					CurrentScreen.Update();
					if (TopLine < CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected];
					}
					if (TopLine + 18 > CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected] - 18;
					}
					if (TopLine < 0)
					{
						TopLine = 0;
					}
					break;
				case Keys.Next:
					if (Selected < CurrentScreen.Options.Count - 11)
					{
						Selected += 10;
					}
					else
					{
						Selected = CurrentScreen.Options.Count - 1;
					}
					if (Selected >= CurrentScreen.Options.Count)
					{
						Selected = 0;
					}
					CurrentScreen.Update();
					if (TopLine < CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected];
					}
					if (TopLine + 18 > CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected] - 18;
					}
					if (TopLine < 0)
					{
						TopLine = 0;
					}
					break;
				case Keys.NumPad8:
					Selected--;
					if (Selected < 0)
					{
						Selected = CurrentScreen.Options.Count - 1;
					}
					CurrentScreen.Update();
					if (TopLine < CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected];
					}
					if (TopLine + 18 > CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected] - 18;
					}
					if (TopLine < 0)
					{
						TopLine = 0;
					}
					break;
				case Keys.NumPad2:
					Selected++;
					if (Selected >= CurrentScreen.Options.Count)
					{
						Selected = 0;
					}
					CurrentScreen.Update();
					if (TopLine < CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected];
					}
					if (TopLine + 18 > CurrentScreen.OptionLines[Selected])
					{
						TopLine = CurrentScreen.OptionLines[Selected] - 18;
					}
					if (TopLine < 0)
					{
						TopLine = 0;
					}
					break;
				}
			}
			else
			{
				Thread.Sleep(7);
			}
		}
		GameManager.Instance.PopGameView();
	}

	public bool IsAuthorized(GameObject Object)
	{
		if (HackActive)
		{
			return true;
		}
		return Object?.IsTrueKin() ?? false;
	}

	public bool AttemptHack()
	{
		int num = SecurityHardeningLevel + SecurityAlertLevel;
		HackingSifrah hackingSifrah = new HackingSifrah(Terminal, 4 + num, 2 + num * 2, Subject?.Stat("Intelligence") ?? 0);
		hackingSifrah.HandlerID = Terminal.ID;
		hackingSifrah.HandlerPartName = "CyberneticsTerminal2";
		hackingSifrah.Play(Terminal);
		if (hackingSifrah.InterfaceExitRequested)
		{
			return false;
		}
		return HackActive;
	}

	public bool CheckSecurity(int AlertChance, TerminalScreen Screen, int Times = 1)
	{
		bool hackActive = HackActive;
		if (HackActive)
		{
			if (LowLevelHack)
			{
				AlertChance -= 5;
				if (AlertChance <= 0 && 50.in100())
				{
					AlertChance = 1;
				}
			}
			for (int i = 0; i < Times; i++)
			{
				if (AlertChance.in100())
				{
					Terminal.ModIntProperty("SecurityAlertLevel", 1);
					break;
				}
			}
		}
		if (Authorized)
		{
			if (hackActive && !HackActive)
			{
				Subject = Actor;
				CurrentScreen = new CyberneticsScreenMainMenu();
			}
			else
			{
				CurrentScreen = Screen;
			}
			return true;
		}
		CurrentScreen = new CyberneticsScreenGoodbye();
		return false;
	}

	public void GetPossibleSubjects(List<GameObject> Store)
	{
		if (Store != null)
		{
			Store.Add(Actor);
			GetPossibleSubjects(Store, Terminal.CurrentCell);
			Terminal.CurrentCell?.ForeachAdjacentCell((Cell cell) =>
			{
				GetPossibleSubjects(Store, cell);
			});
		}
	}

	public List<GameObject> GetPossibleSubjects()
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetPossibleSubjects(list);
		return list;
	}

	private void GetPossibleSubjects(List<GameObject> Store, Cell Cell)
	{
		if (Cell == null || Store == null)
		{
			return;
		}
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = Cell.Objects[i];
			if (gameObject.IsCreature && gameObject.Body != null && gameObject.IsAlliedTowards(Actor) && !gameObject.IsInStasis() && Terminal.PhaseMatches(gameObject) && !Store.Contains(gameObject))
			{
				Store.Add(gameObject);
			}
		}
	}

	public int GetPossibleSubjectCount()
	{
		return GetPossibleSubjects().Count;
	}

	public void GetAuthorizedSubjects(List<GameObject> Store)
	{
		if (Store != null)
		{
			Store.Add(Actor);
			GetAuthorizedSubjects(Store, Terminal.CurrentCell);
			Terminal.CurrentCell?.ForeachAdjacentCell((Cell cell) =>
			{
				GetAuthorizedSubjects(Store, cell);
			});
		}
	}

	public List<GameObject> GetAuthorizedSubjects()
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetAuthorizedSubjects(list);
		return list;
	}

	private void GetAuthorizedSubjects(List<GameObject> Store, Cell Cell)
	{
		if (Cell == null || Store == null)
		{
			return;
		}
		int i = 0;
		for (int count = Cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = Cell.Objects[i];
			if (gameObject.IsCreature && IsAuthorized(gameObject) && gameObject.Body != null && gameObject.IsAlliedTowards(Actor) && !gameObject.IsInStasis() && Terminal.PhaseMatches(gameObject) && !Store.Contains(gameObject))
			{
				Store.Add(gameObject);
			}
		}
	}

	public int GetAuthorizedSubjectCount()
	{
		return GetAuthorizedSubjects().Count;
	}
}
