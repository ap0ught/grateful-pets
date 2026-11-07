using System;
using System.IO;
using ConsoleLib.Console;
using Platform.IO;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.World;

namespace XRL.UI;

public class GameSummaryUI
{
	public static void Show(int Score, string Details, string Cause, string Name, string Leaderboard, bool bReal)
	{
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		if (!string.IsNullOrEmpty(The.Game.GetStringGameState("EndgameCause")))
		{
			Cause = The.Game.GetStringGameState("EndgameCause");
		}
		while (true)
		{
			string text = Details;
			if ((Leaderboard != null) & bReal)
			{
				text = ((!LeaderboardManager.leaderboardresults.ContainsKey(Leaderboard)) ? Details.Replace("<%leaderboard%>", "<requesting leaderboard results...>") : Details.Replace("<%leaderboard%>", "&Cê &ySteam leaderboard results &Cê\n\n" + LeaderboardManager.leaderboardresults[Leaderboard]));
			}
			bool flag = false;
			if (Options.ModernUI)
			{
				GameManager.Instance.PushGameView("GameSummaryScreen");
				_ = GameSummaryScreen.ShowGameSummary(Name, Cause, text, bReal).Result;
				GameManager.Instance.PopGameView();
				if (bReal)
				{
					break;
				}
				UIManager.instance.currentWindow = null;
				GameManager.Instance.StaticEffecting = 4;
				flag = true;
			}
			GameManager.Instance.PushGameView("GameSummary");
			TextBlock textBlock = new TextBlock(text, 76, 99999);
			bool flag2 = false;
			int num = 0;
			int num2 = 21;
			int num3 = Math.Max(0, textBlock.Lines.Count - num2);
			while (true)
			{
				if (!flag2)
				{
					XRL.World.Event.ResetPool(resetMinEventPools: false);
					scrapBuffer.Clear();
					scrapBuffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					for (int i = 1; i < 24; i++)
					{
						scrapBuffer.Goto(79, i);
						scrapBuffer.Write(177, ConsoleLib.Console.ColorUtility.Bright((ushort)0), 0);
					}
					if (textBlock.Lines.Count > num2)
					{
						_ = (int)Math.Ceiling((double)textBlock.Lines.Count / (double)num2);
						int num4 = (int)Math.Ceiling((double)(textBlock.Lines.Count + num2) / (double)num2);
						_ = 0;
						if (num4 <= 0)
						{
							num4 = 1;
						}
						int num5 = 23 / num4;
						if (num5 <= 0)
						{
							num5 = 1;
						}
						int num6 = (int)((double)(23 - num5) * ((double)num / (double)(textBlock.Lines.Count - 23)));
						num6++;
						for (int j = num6; j < num6 + num5; j++)
						{
							scrapBuffer.Goto(79, j);
							scrapBuffer.Write(219, ConsoleLib.Console.ColorUtility.Bright(7), 0);
						}
					}
					scrapBuffer.Goto(13, 0);
					scrapBuffer.Write("[ &wGame Summary && Chronology&y ]");
					scrapBuffer.Goto(50, 0);
					scrapBuffer.Write(" &WESC&y - Exit ");
					for (int k = num; k < num + num2 && k < textBlock.Lines.Count; k++)
					{
						scrapBuffer.Goto(2, k - num + 2);
						scrapBuffer.Write(textBlock.Lines[k]);
					}
					scrapBuffer.Goto(2, 24);
					scrapBuffer.Write(" [&Ws or F1&y - save tombstone file]");
					Popup._TextConsole.DrawBuffer(scrapBuffer);
					if (!flag)
					{
						Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeaderboardResultsUpdated")
						{
							break;
						}
						if (keys == Keys.M)
						{
							XRLCore.Core.Game.Player.Messages.Show();
						}
						if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
						{
							flag2 = true;
						}
						if (keys == Keys.NumPad2)
						{
							num++;
						}
						if (keys == Keys.Next || keys == Keys.Next || Keyboard.RawCode == Keys.Next || Keyboard.RawCode == Keys.Next)
						{
							num += 20;
						}
						if (keys == Keys.Prior || keys == Keys.Back || Keyboard.RawCode == Keys.Prior || Keyboard.RawCode == Keys.Prior)
						{
							num -= 20;
							if (num < 0)
							{
								num = 0;
							}
						}
						if (keys == Keys.NumPad8 && num > 0)
						{
							num--;
						}
						if (num > num3)
						{
							num = num3;
						}
						if (keys == Keys.S || keys == Keys.F1)
						{
							if (!bReal)
							{
								flag2 = true;
							}
							else
							{
								string text2 = Name + "-" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString();
								string text3 = new string(Platform.IO.Path.GetInvalidFileNameChars()) + new string(Platform.IO.Path.GetInvalidPathChars());
								for (int l = 0; l < text3.Length; l++)
								{
									text2 = text2.Replace(text3[l].ToString(), "");
								}
								text2 = DataManager.SavePath(text2 + ".txt");
								try
								{
									Debug.LogWarning("This is not ported to platform service yet");
									using (Stream stream = Platform.IO.File.Open(text2, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
									{
										using TextWriter textWriter = new StreamWriter(stream);
										for (int m = 0; m < textBlock.Lines.Count; m++)
										{
											textWriter.WriteLine(ConsoleLib.Console.ColorUtility.StripFormatting(textBlock.Lines[m]));
										}
									}
									Popup.Show("Your tombstone file was saved:\n\n" + text2.ToString());
								}
								catch (Exception)
								{
									Popup.Show("There was an error saving: " + text2.ToString());
								}
							}
						}
						if (keys == Keys.Escape || keys == Keys.NumPad5)
						{
							flag2 = true;
						}
						continue;
					}
				}
				GameManager.Instance.PopGameView();
				return;
			}
		}
	}
}
