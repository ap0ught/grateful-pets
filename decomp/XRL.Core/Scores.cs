using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using ConsoleLib.Console;
using UnityEngine;
using XRL.UI;

namespace XRL.Core;

[UIView("HighScores", false, false, false, "Menu", "HighScores", false, 0, false)]
public class Scores : IWantsTextConsoleInit
{
	public static TextConsole Console;

	public static ScreenBuffer Buffer;

	public static Scoreboard2 _Scores;

	public static Scoreboard2 Scoreboard
	{
		get
		{
			if (_Scores != null)
			{
				return _Scores;
			}
			Task<Scoreboard2> task = Scoreboard2.Load();
			task.Wait();
			_Scores = task.Result;
			if (_Scores == null)
			{
				_Scores = new Scoreboard2();
			}
			if (_Scores != null && _Scores.Scores != null)
			{
				foreach (ScoreEntry2 score in _Scores.Scores)
				{
					score?.CheckVersion();
				}
			}
			return _Scores;
		}
	}

	public void Init(TextConsole console, ScreenBuffer buffer)
	{
		Console = console;
		Buffer = buffer;
	}

	public static XRLGame Show()
	{
		GameManager.Instance.PushGameView("HighScores");
		int page = 0;
		int num = 0;
		int num2 = 0;
		int requestTop = 0;
		int num3 = 0;
		int playersPosition = -1;
		string currentBoard = null;
		string resultHandle = null;
		string text = null;
		string[] array = null;
		string[] array2 = null;
		if (array == null && page == 0 && array2 == null)
		{
			Scoreboard.Sort();
			List<string> list = new List<string>();
			int num4 = 0;
			foreach (ScoreEntry2 score in Scoreboard.Scores)
			{
				_ = score;
				num4++;
				if (num4 > 50)
				{
					break;
				}
			}
			array2 = list.ToArray();
			text = "Local Scores";
			playersPosition = 0;
		}
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<ScoreEntry2, List<string>> dictionary = new Dictionary<ScoreEntry2, List<string>>();
		for (int i = 0; i < Scoreboard.Scores.Count; i++)
		{
			stringBuilder.Length = 0;
			stringBuilder.Append(Scoreboard.Scores[i].Details);
			TextBlock textBlock = new TextBlock(stringBuilder, 75, 24);
			dictionary.Add(Scoreboard.Scores[i], textBlock.Lines);
		}
		Keys keys;
		do
		{
			Buffer.Clear();
			Buffer.Goto(0, 0);
			keys = Keys.None;
			if (GameManager.Instance.ModernUI)
			{
				if (page == 0)
				{
					array = array2;
				}
				if (array != null)
				{
					new List<string>(array);
					_ = playersPosition;
				}
			}
			Buffer.Goto(2, 0);
			if (page == 0)
			{
				Buffer.Write("&Y>Local Scores");
			}
			else
			{
				Buffer.Write(" &yLocal Scores");
			}
			Buffer.Goto(46, 0);
			if (page == 1)
			{
				Buffer.Write("&Y>Daily");
			}
			else
			{
				Buffer.Write(" &yDaily");
			}
			Buffer.Goto(54, 0);
			if (page == 2)
			{
				Buffer.Write("&Y>Daily (friends)");
			}
			else
			{
				Buffer.Write(" &yDaily (friends)");
			}
			if (page == 0)
			{
				if (Scoreboard.Scores.Count == 0)
				{
					Buffer.Goto(35, 12);
					Buffer.Write("No high scores!");
					Console.DrawBuffer(Buffer, null, bSkipIfOverlay: true);
					keys = Keyboard.getvk(MapDirectionToArrows: true);
				}
				else
				{
					Scoreboard.Sort();
					int num5 = 0;
					Buffer.Goto(0, 2);
					for (int j = num2; j < Scoreboard.Scores.Count && j < num2 + 3; j++)
					{
						stringBuilder.Length = 0;
						if (j == num)
						{
							stringBuilder.Append(">");
						}
						else
						{
							stringBuilder.Append(" ");
						}
						List<string> list2 = dictionary[Scoreboard.Scores[j]];
						stringBuilder.Append(" #&G").Append(j + 1).Append("&y) ")
							.Append(list2[0]);
						Buffer.Write(stringBuilder);
						Buffer.Write("\n");
						for (int k = 1; k < Math.Min(list2.Count, 7); k++)
						{
							if (Buffer.Y >= 23)
							{
								break;
							}
							Buffer.Write(list2[k]);
							Buffer.Write("\n");
						}
						if (Buffer.Y >= 23)
						{
							break;
						}
						string gameMode = Scoreboard.Scores[j].GameMode;
						if (string.IsNullOrEmpty(gameMode))
						{
							Buffer.Write("This game was played in Classic mode.");
						}
						else
						{
							Buffer.Write("This game was played in " + gameMode + " mode.");
						}
						if (Buffer.Y >= 23)
						{
							break;
						}
						Buffer.Write("\n");
						if (Buffer.Y >= 23)
						{
							break;
						}
						Buffer.Write("\n");
						num5 = j;
					}
					if (num2 < Scoreboard.Scores.Count - 2)
					{
						Buffer.Goto(0, 24);
						Buffer.Write("<more...>");
					}
					if (CapabilityManager.AllowKeyboardHotkeys)
					{
						if (num >= 0 && num < Scoreboard.Scores.Count && Scoreboard?.Scores[num]?.HasCoda() == true)
						{
							Buffer.Goto(43, 24);
							Buffer.Write("&Y[&WR&y - Revisit Epilogue&Y] &Y[&WD / Del&y - Delete&Y]");
						}
						else
						{
							Buffer.Goto(62, 24);
							Buffer.Write("&Y[&WD / Del&y - Delete&Y]");
						}
					}
					Console.DrawBuffer(Buffer, null, bSkipIfOverlay: true);
					keys = Keyboard.getvk(MapDirectionToArrows: true);
					if (keys == Keys.NumPad2 && num < Scoreboard.Scores.Count - 1)
					{
						num++;
						if (num > num5)
						{
							num2++;
						}
					}
					if (keys == Keys.Next)
					{
						num += 4;
						if (num > Scoreboard.Scores.Count - 1)
						{
							num = Scoreboard.Scores.Count - 1;
						}
						num2 = num - 2;
						if (num2 < 0)
						{
							num2 = 0;
						}
					}
					if (keys == Keys.NumPad8 && num > 0)
					{
						num--;
						if (num < num2)
						{
							num2 = num;
						}
					}
					if (keys == Keys.Prior)
					{
						num -= 4;
						if (num < 0)
						{
							num = 0;
						}
						num2 = num;
					}
					if (keys == Keys.R && num >= 0 && num < Scoreboard.Scores.Count && Scoreboard.Scores[num].HasCoda())
					{
						return Scoreboard.Scores[num].LoadCoda().Result;
					}
					if ((keys == Keys.Space || keys == Keys.Enter) && num >= 0 && num < Scoreboard.Scores.Count)
					{
						Popup.Show(Scoreboard.Scores[num].Details);
					}
					if (page == 0 && keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdDelete" && num >= 0 && num < Scoreboard.Scores.Count && Popup.ShowYesNo("Are you sure you want to delete this?\n\n" + dictionary[Scoreboard.Scores[num]][0]) == DialogResult.Yes)
					{
						Scoreboard.Scores.Remove(Scoreboard.Scores[num]);
						Scoreboard.Save().Wait();
						if (num >= Scoreboard.Scores.Count)
						{
							num--;
						}
						if (num < num2)
						{
							num2 = num;
						}
					}
				}
			}
			else
			{
				if (resultHandle == null || array == null)
				{
					if (!PlatformManager.AreLeaderboardsOffline())
					{
						Buffer.Goto(35, 12);
						Buffer.Write("loading scores...");
					}
					else
					{
						Buffer.Goto(28, 12);
						Buffer.Write("<not connected to provider>");
					}
				}
				else
				{
					for (int l = 0; l < 20; l++)
					{
						Buffer.Goto(2, 2 + l);
						int num6 = requestTop + l;
						if (num6 < array.Length)
						{
							Buffer.Write(array[num6]);
						}
					}
					Buffer.Goto(2, 23);
					Buffer.Write("Page " + (int)(Math.Ceiling((float)requestTop / 20f) + 1.0) + " of " + (int)Math.Ceiling((float)array.Length / 20f));
					Buffer.Goto(2, 24);
					Buffer.Write("&WDown&y-next page &WUp&y-previous page");
					if (text != null)
					{
						Buffer.Goto(50, 23);
						Buffer.Write(text);
					}
					Buffer.Goto(50, 24);
					Buffer.Write("&W7&y-previous board &W9&y-next board");
				}
				Console.DrawBuffer(Buffer);
				keys = Keyboard.getvk(MapDirectionToArrows: true);
			}
			bool flag = false;
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Page:"))
			{
				int num7 = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
				playersPosition = -1;
				page = num7;
				resultHandle = null;
				array = null;
				text = null;
				requestTop = 0;
				num3 = 0;
				if (page < 0)
				{
					page = 4;
				}
				flag = true;
			}
			if (keys == Keys.Left || keys == Keys.NumPad4)
			{
				playersPosition = -1;
				page--;
				resultHandle = null;
				array = null;
				text = null;
				requestTop = 0;
				num3 = 0;
				if (page < 0)
				{
					page = 4;
				}
				flag = true;
			}
			if (keys == Keys.Right || keys == Keys.NumPad6)
			{
				playersPosition = -1;
				page++;
				resultHandle = null;
				array = null;
				text = null;
				requestTop = 0;
				num3 = 0;
				if (page > 4)
				{
					page = 0;
				}
				flag = true;
			}
			if (keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
			{
				num3--;
				playersPosition = -1;
				requestTop = 0;
				resultHandle = null;
				array = null;
				flag = true;
			}
			if (keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
			{
				num3++;
				playersPosition = -1;
				requestTop = 0;
				resultHandle = null;
				array = null;
				flag = true;
			}
			if (keys == Keys.NumPad8 || keys == Keys.Up || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Up"))
			{
				requestTop -= 20;
				if (requestTop < 0)
				{
					requestTop = 0;
				}
			}
			if (keys == Keys.NumPad2 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Down"))
			{
				requestTop -= 20;
				if (requestTop < 0)
				{
					requestTop = 0;
				}
			}
			if ((keys == Keys.NumPad2 || keys == Keys.Down) && array != null)
			{
				requestTop += 20;
				if (requestTop > Math.Max(0, array.Length - 20))
				{
					requestTop = Math.Max(0, array.Length - 20);
				}
			}
			if (flag && (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console || !PlatformManager.AreLeaderboardsOffline()))
			{
				string boardID = null;
				if (page == 1)
				{
					CultureInfo cultureInfo = new CultureInfo("en-US");
					DateTime dateTime = DateTime.Now + new TimeSpan(num3, 0, 0, 0, 0);
					if (dateTime > DateTime.Now)
					{
						dateTime = DateTime.Now;
						num3--;
					}
					if (dateTime < new DateTime(2016, 11, 1))
					{
						dateTime = new DateTime(2016, 11, 1);
						num3++;
					}
					int dayOfYear = cultureInfo.Calendar.GetDayOfYear(dateTime);
					boardID = "daily:" + dateTime.Year + ":" + dayOfYear;
					text = "Day " + dayOfYear + " of " + dateTime.Year;
				}
				if (page == 2)
				{
					CultureInfo cultureInfo2 = new CultureInfo("en-US");
					DateTime dateTime2 = DateTime.Now + new TimeSpan(num3, 0, 0, 0, 0);
					if (dateTime2 > DateTime.Now)
					{
						dateTime2 = DateTime.Now;
						num3--;
					}
					if (dateTime2 < new DateTime(2016, 11, 1))
					{
						dateTime2 = new DateTime(2016, 11, 1);
						num3++;
					}
					int dayOfYear2 = cultureInfo2.Calendar.GetDayOfYear(dateTime2);
					boardID = "daily:" + dateTime2.Year + ":" + dayOfYear2;
					text = "Day " + dayOfYear2 + " of " + dateTime2.Year;
				}
				try
				{
					if (boardID != null)
					{
						currentBoard = boardID;
						Func<string> getLeaderboard = null;
						getLeaderboard = () => LeaderboardManager.GetLeaderboard(boardID, 0, 9999, page == 2 || page == 4, (LeaderboardScores result) =>
						{
							if (currentBoard == boardID)
							{
								switch (result.GetResult())
								{
								case LeaderboardScores.Result.Failure:
									Popup.ShowYesNo("An error has occurred, would you like to retry?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.Yes, (DialogResult dialogResult) =>
									{
										switch (dialogResult)
										{
										case DialogResult.Yes:
										case DialogResult.OK:
											getLeaderboard();
											break;
										default:
											PlatformManager.SetOfflineMode(isOffline: true);
											break;
										}
									});
									break;
								case LeaderboardScores.Result.Cancelled:
									break;
								default:
								{
									StringBuilder stringBuilder2 = new StringBuilder();
									int num8 = Math.Min(result.GetEntryCount(), 9999);
									for (int m = 0; m < num8; m++)
									{
										LeaderboardEntry leaderboardEntry = result.GetLeaderboardEntry(m);
										if (leaderboardEntry.isCurrentUser)
										{
											playersPosition = m;
											stringBuilder2.Append("&y" + leaderboardEntry.rank + ": &W" + leaderboardEntry.displayName + " &y(&w" + leaderboardEntry.score + "&y)\n");
										}
										else
										{
											stringBuilder2.Append("&y" + leaderboardEntry.rank + ": &y" + leaderboardEntry.displayName + " &y(&w" + leaderboardEntry.score + "&y)\n");
										}
									}
									if (playersPosition != -1)
									{
										requestTop = Math.Max(0, Math.Min(playersPosition - 9, num8 - 20));
									}
									if (!LeaderboardManager.leaderboardresults.ContainsKey(resultHandle))
									{
										LeaderboardManager.leaderboardresults.Add(resultHandle, "");
									}
									LeaderboardManager.leaderboardresults[resultHandle] = stringBuilder2.ToString();
									Debug.Log(stringBuilder2.ToString());
									Keyboard.PushMouseEvent("LeaderboardResultsUpdated");
									break;
								}
								}
							}
						});
						resultHandle = getLeaderboard();
					}
				}
				catch (Exception ex)
				{
					XRLCore.LogError("leaderboard", ex);
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeaderboardResultsUpdated")
			{
				array = LeaderboardManager.leaderboardresults[resultHandle].Split('\n');
			}
		}
		while (keys != Keys.Escape && keys != Keys.NumPad5);
		GameManager.Instance.PopGameView(bHard: true);
		return null;
	}
}
