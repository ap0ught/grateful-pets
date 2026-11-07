using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Qud.UI;
using XRL.Language;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.UI;

[UIView("ConsoleTrade", false, true, false, "Trade", null, false, 0, false)]
public class TradeUI : IWantsTextConsoleInit
{
	public enum TradeScreenMode
	{
		Trade,
		Container
	}

	public enum OfferStatus
	{
		NEXT,
		REFRESH,
		TOP,
		CLOSE
	}

	public const double TRADING_FLOATING_POINT_FUDGE = 0.0001;

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public static GameObject _Trader = null;

	public static double Performance = 1.0;

	public static bool AssumeTradersHaveWater = true;

	public static int[] ScrollPosition = new int[2];

	public static double[] Totals = new double[2];

	public static int[] Weight = new int[2];

	public static List<TradeEntry>[] Objects = null;

	public static int[][] NumberSelected = new int[2][];

	public static int nTotalWeight = 0;

	public static int nMaxWeight = 0;

	private static int SideSelected = 0;

	private static int RowSelect = 0;

	public static string sReadout = "";

	public static float costMultiple = 1f;

	public static TradeScreenMode ScreenMode;

	public static string TradeScreenVerb
	{
		get
		{
			if (ScreenMode != TradeScreenMode.Trade || !(costMultiple > 0f))
			{
				return "transfer";
			}
			return "trade";
		}
	}

	void IWantsTextConsoleInit.Init(TextConsole console, ScreenBuffer buffer)
	{
		_TextConsole = console;
		_ScreenBuffer = buffer;
	}

	public static double GetMultiplier(GameObject GO)
	{
		if (GO == null || !GO.IsCurrency)
		{
			return Performance;
		}
		return 1.0;
	}

	public static bool ValidForTrade(GameObject Object, GameObject Trader, GameObject Other = null, float CostMultiple = 1f, bool AcceptRelevant = true)
	{
		if (Other != null && Object.MovingIntoWouldCreateContainmentLoop(Other))
		{
			return false;
		}
		if (AcceptRelevant && !CanAcceptObjectEvent.Check(Object, Trader, Other))
		{
			return false;
		}
		if (ScreenMode == TradeScreenMode.Container)
		{
			return true;
		}
		if (Object.IsNatural())
		{
			return false;
		}
		if (CostMultiple > 0f && Object.HasPropertyOrTag("WaterContainer") && !Object.HasTagOrProperty("QuestItem"))
		{
			LiquidVolume liquidVolume = Object.LiquidVolume;
			if (liquidVolume != null && liquidVolume.IsFreshWater() && !Object.HasPart<TinkerItem>())
			{
				return false;
			}
		}
		if (Trader.IsPlayer())
		{
			if (Object.HasPropertyOrTag("PlayerWontSell"))
			{
				return false;
			}
			if (CostMultiple > 0f && !CanBeTradedEvent.Check(Object, Trader, Other, CostMultiple))
			{
				return false;
			}
		}
		else
		{
			if (Object.HasPropertyOrTag("WontSell") && !Trader.IsPlayerLed())
			{
				return false;
			}
			if (Trader.HasPropertyOrTag("WontSell") && Trader.GetPropertyOrTag("WontSell").Contains(Object.Blueprint) && !Trader.IsPlayerLed())
			{
				return false;
			}
			if (Trader.HasPropertyOrTag("WontSellTag") && Object.HasTagOrProperty(Trader.GetPropertyOrTag("WontSellTag")) && !Trader.IsPlayerLed())
			{
				return false;
			}
		}
		return true;
	}

	public static void GetObjects(GameObject Trader, List<TradeEntry> ReturnObjects, GameObject Other, float CostMultiple = 1f)
	{
		List<GameObject> list = new List<GameObject>(64);
		bool acceptRelevant = CanAcceptObjectEvent.Relevant(Other);
		foreach (GameObject @object in Trader.Inventory.GetObjects())
		{
			if (ValidForTrade(@object, Trader, Other, CostMultiple, acceptRelevant))
			{
				list.Add(@object);
			}
		}
		list.Sort((GameObject a, GameObject b) => a.SortVs(b));
		string text = "";
		foreach (GameObject item in list)
		{
			item.Seen();
			string inventoryCategory = item.GetInventoryCategory();
			if (inventoryCategory != text)
			{
				text = inventoryCategory;
				ReturnObjects.Add(new TradeEntry(text));
			}
			ReturnObjects.Add(new TradeEntry(item));
		}
	}

	public static string FormatPrice(double Price, float Multiplier)
	{
		return $"{Price * (double)Multiplier:0.00}";
	}

	public static void Reset()
	{
		ScrollPosition = new int[2];
		Totals = new double[2];
		Weight = new int[2];
		if (Objects == null)
		{
			Objects = new List<TradeEntry>[2];
			Objects[0] = new List<TradeEntry>();
			Objects[1] = new List<TradeEntry>();
		}
		Objects[0].Clear();
		Objects[1].Clear();
		NumberSelected = new int[2][];
	}

	public static int GetSideOfObject(GameObject obj)
	{
		if (FindInTradeList(Objects[0], obj) > -1)
		{
			return 0;
		}
		return 1;
	}

	public static int FindInTradeList(List<TradeEntry> list, GameObject obj)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GO == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public static double ItemValueEach(GameObject obj, bool? TraderInventory = null)
	{
		double num = obj.ValueEach;
		if (_Trader != null && (TraderInventory == true || (!TraderInventory.HasValue && FindInTradeList(Objects[0], obj) != -1)))
		{
			int intProperty = _Trader.GetIntProperty("MinimumSellValue");
			if (intProperty > 0 && num < (double)intProperty)
			{
				num = intProperty;
			}
		}
		return num;
	}

	public static double GetValue(GameObject obj, bool? TraderInventory = null)
	{
		if (TraderInventory == true || (!TraderInventory.HasValue && FindInTradeList(Objects[0], obj) != -1))
		{
			return ItemValueEach(obj, true) / GetMultiplier(obj);
		}
		if (TraderInventory == false || (!TraderInventory.HasValue && FindInTradeList(Objects[1], obj) != -1))
		{
			return ItemValueEach(obj, false) * GetMultiplier(obj);
		}
		return 0.0;
	}

	public static int GetNumberSelected(GameObject obj)
	{
		int num = FindInTradeList(Objects[0], obj);
		if (num != -1)
		{
			return NumberSelected[0][num];
		}
		num = FindInTradeList(Objects[1], obj);
		if (num != -1)
		{
			return NumberSelected[1][num];
		}
		return -999;
	}

	public static void SetSelectedObject(GameObject obj)
	{
		int num = FindInTradeList(Objects[0], obj);
		if (num != -1)
		{
			SideSelected = 0;
			RowSelect = num;
		}
		num = FindInTradeList(Objects[1], obj);
		if (num != -1)
		{
			SideSelected = 1;
			RowSelect = num;
		}
	}

	public static void SetNumberSelected(GameObject obj, int amount)
	{
		int num = FindInTradeList(Objects[0], obj);
		if (num != -1)
		{
			NumberSelected[0][num] = amount;
		}
		num = FindInTradeList(Objects[1], obj);
		if (num != -1)
		{
			NumberSelected[1][num] = amount;
		}
		UpdateTotals(Totals, Weight, Objects, NumberSelected);
	}

	public static void PerformObjectDropped(GameObject Object, int DroppedOnSide)
	{
		if (FindInTradeList(Objects[DroppedOnSide], Object) != -1)
		{
			int num = FindInTradeList(Objects[DroppedOnSide], Object);
			NumberSelected[DroppedOnSide][num] = 0;
			UpdateTotals(Totals, Weight, Objects, NumberSelected);
			return;
		}
		int num2 = FindInTradeList(Objects[1 - DroppedOnSide], Object);
		if (num2 != -1)
		{
			NumberSelected[1 - DroppedOnSide][num2] = Objects[1 - DroppedOnSide][num2].GO.Count;
			UpdateTotals(Totals, Weight, Objects, NumberSelected);
		}
	}

	public static void UpdateTotals(double[] Totals, int[] Weight, List<TradeEntry>[] Objects, int[][] NumberSelected)
	{
		for (int i = 0; i <= 1; i++)
		{
			double num = 1.0;
			switch (i)
			{
			case 0:
				num = 1.0 / Performance;
				break;
			case 1:
				num = Performance;
				break;
			}
			Totals[i] = 0.0;
			Weight[i] = 0;
			for (int j = 0; j < Objects[i].Count; j++)
			{
				if (Objects[i][j].GO != null && NumberSelected[i][j] > 0)
				{
					Weight[i] += Objects[i][j].GO.WeightEach * NumberSelected[i][j];
					if (Objects[i][j].GO.IsCurrency)
					{
						Totals[i] += ItemValueEach(Objects[i][j].GO) * (double)NumberSelected[i][j];
					}
					else
					{
						Totals[i] += ItemValueEach(Objects[i][j].GO) * num * (double)NumberSelected[i][j];
					}
				}
			}
			Totals[i] *= costMultiple;
		}
		sReadout = " {{C|" + $"{Totals[0]:0.###}" + "}} drams <-> {{C|" + $"{Totals[1]:0.###}" + "}} drams ÄÄ {{W|$" + The.Player.GetFreeDrams() + "}} ";
	}

	public static void ShowTradeScreen(GameObject Trader, float _costMultiple = 1f, TradeScreenMode screenMode = TradeScreenMode.Trade)
	{
		bool flag = Trader.IsPlayerLed();
		if (flag)
		{
			_costMultiple = 0f;
		}
		costMultiple = _costMultiple;
		ScreenMode = screenMode;
		TextConsole.LoadScrapBuffers();
		Reset();
		while (true)
		{
			if (Trader == null)
			{
				Reset();
				break;
			}
			if (!Trader.HasPart<Inventory>())
			{
				Popup.ShowFail(Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " cannot carry things.");
				Reset();
				break;
			}
			if (Trader.IsEngagedInMelee() && Trader.Brain != null)
			{
				Popup.ShowFail(Trader.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " engaged in melee combat and" + Trader.Is + " too busy to trade with you.");
				Reset();
				break;
			}
			if (Trader.IsAflame() && Trader.Brain != null)
			{
				Popup.ShowFail(Trader.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " on fire and" + Trader.Is + " too busy to trade with you.");
				Reset();
				break;
			}
			_Trader = Trader;
			int intProperty = _Trader.GetIntProperty("TraderCreditExtended");
			if (intProperty > 0)
			{
				int freeDrams = The.Player.GetFreeDrams();
				if (freeDrams <= 0)
				{
					Popup.Show(_Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will not trade with you until you pay " + _Trader.them + " the {{C|" + intProperty + "}} " + ((intProperty == 1) ? "dram" : "drams") + " of {{B|fresh water}} you owe " + _Trader.them + ".");
					Reset();
					break;
				}
				if (freeDrams < intProperty)
				{
					if (Popup.ShowYesNo(_Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will not trade with you until you pay " + _Trader.them + " the {{C|" + intProperty + "}} " + ((intProperty == 1) ? "dram" : "drams") + " of {{B|fresh water}} you owe " + _Trader.them + ". Do you want to give " + _Trader.them + " your {{C|" + freeDrams + "}} " + ((freeDrams == 1) ? "dram" : "drams") + " now?") == DialogResult.Yes)
					{
						intProperty -= freeDrams;
						The.Player.UseDrams(freeDrams);
						_Trader.SetIntProperty("TraderCreditExtended", intProperty);
					}
					Reset();
					break;
				}
				if (Popup.ShowYesNo(_Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will not trade with you until you pay " + _Trader.them + " the {{C|" + intProperty + "}} " + ((intProperty == 1) ? "dram" : "drams") + " of {{B|fresh water}} you owe " + _Trader.them + ". Do you want to give it to " + _Trader.them + " now?") != DialogResult.Yes)
				{
					Reset();
					break;
				}
				The.Player.UseDrams(intProperty);
				_Trader.RemoveIntProperty("TraderCreditExtended");
			}
			_Trader.ModIntProperty("TradeCount", 1);
			Performance = GetTradePerformanceEvent.GetFor(The.Player, _Trader);
			Tinkering_Repair part = Trader.GetPart<Tinkering_Repair>();
			int identifyLevel = Tinkering.GetIdentifyLevel(Trader);
			bool identify = identifyLevel > 0;
			bool flag2 = part != null;
			bool flag3 = Trader.HasSkill("Tinkering_Tinker1");
			bool flag4 = Trader.GetIntProperty("Librarian") != 0;
			GameObject player = The.Player;
			bool companion = flag;
			StartTradeEvent.Send(player, Trader, identifyLevel, companion, identify, flag2, flag3, flag4);
			SideSelected = 0;
			RowSelect = 0;
			int num = 21;
			int num2 = 1;
			while (true)
			{
				Objects[0].Clear();
				Objects[1].Clear();
				ScrollPosition[0] = 0;
				ScrollPosition[1] = 0;
				Totals[0] = 0.0;
				Totals[1] = 0.0;
				GetObjects(Trader, Objects[0], The.Player, costMultiple);
				GetObjects(The.Player, Objects[1], Trader, costMultiple);
				NumberSelected[0] = new int[Objects[0].Count];
				NumberSelected[1] = new int[Objects[1].Count];
				if (Objects[0].Count <= 0 && costMultiple > 0f)
				{
					if (!AllowTradeWithNoInventoryEvent.Check(The.Player, Trader))
					{
						Popup.Show(Trader.Does("have", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " nothing to trade.");
						_Trader = null;
						Reset();
						return;
					}
				}
				nTotalWeight = The.Player.GetCarriedWeight();
				nMaxWeight = The.Player.GetMaxCarriedWeight();
				UpdateTotals(Totals, Weight, Objects, NumberSelected);
				bool flag5 = false;
				int num3 = 0;
				string text = "[{{W|" + ControlManager.getCommandInputDescription("CmdTradeAdd", mapGlyphs: false) + "}}/{{W|" + ControlManager.getCommandInputDescription("CmdTradeRemove", mapGlyphs: false) + "}} Add/Remove]";
				int length = ColorUtility.StripFormatting(text).Length;
				string text2 = "[{{W|" + ControlManager.getCommandInputDescription("CmdTradeOffer", mapGlyphs: false) + "}} Offer]";
				int length2 = ColorUtility.StripFormatting(text2).Length;
				if (Options.ModernUI)
				{
					OfferStatus result = TradeScreen.show(Trader, costMultiple, screenMode).Result;
					if (result == OfferStatus.REFRESH || result == OfferStatus.NEXT || result == OfferStatus.TOP)
					{
						break;
					}
					return;
				}
				GameManager.Instance.PushGameView("ConsoleTrade");
				string s = "[ {{W|" + StringFormat.ClipLine(Trader.IsCreature ? Trader.poss("inventory") : Trader.ShortDisplayName, 34) + "}} ]";
				bool flag6 = false;
				while (true)
				{
					Keys keys;
					if (!flag6)
					{
						if (Objects[SideSelected].Count == 0)
						{
							SideSelected = 1 - SideSelected;
						}
						Event.ResetPool(resetMinEventPools: false);
						_ScreenBuffer.Clear();
						string text3 = "";
						string text4 = "";
						IRenderable renderable = null;
						int num4 = 0;
						int num5 = ScrollPosition[0];
						while (num4 < num && num5 < Objects[0].Count)
						{
							_ScreenBuffer.Goto(2, num4 + num2);
							GameObject gO = Objects[0][num5].GO;
							if (gO != null)
							{
								if (NumberSelected[0][num5] > 0)
								{
									_ScreenBuffer.Write("{{&Y^g|" + NumberSelected[0][num5] + "}} ");
								}
								_ScreenBuffer.Write(gO.RenderForUI("Trade"));
								_ScreenBuffer.Write(" ");
								_ScreenBuffer.Write(gO.DisplayName, processMarkup: true, HFlip: false, VFlip: false, null, 35);
								if (Trader.IsOwned() && gO.OwnedByPlayer)
								{
									_ScreenBuffer.Write(" {{G|[owned by you]}}");
								}
								string text5 = "";
								if (SideSelected == 0 && RowSelect == num4)
								{
									text3 = gO.DisplayNameSingle;
									if (Trader.IsOwned() && gO.OwnedByPlayer)
									{
										text3 += " {{G|[owned by you]}}";
									}
									renderable = gO.RenderForUI("Trade");
									text4 = " {{K|" + gO.WeightEach + "#}}";
									if (screenMode == TradeScreenMode.Trade)
									{
										string text6 = (gO.IsCurrency ? "Y" : "B");
										text5 = "{{" + text6 + "|$}}{{C|" + FormatPrice(GetValue(gO, true), costMultiple) + "}}";
										text4 = text4 + " " + text5;
									}
								}
								else if (screenMode == TradeScreenMode.Trade)
								{
									string text7 = (gO.IsCurrency ? "W" : "b");
									text5 = "{{" + text7 + "|$}}{{c|" + FormatPrice(GetValue(gO, true), costMultiple) + "}}";
								}
								int x = 40 - ColorUtility.LengthExceptFormatting(text5);
								_ScreenBuffer.Goto(x, num4 + num2);
								_ScreenBuffer.Write(text5);
							}
							else
							{
								string text8 = "{{K|[{{y|" + Objects[0][num5].CategoryName + "}}]}}";
								_ScreenBuffer.Goto(40 - ColorUtility.LengthExceptFormatting(text8), num4 + num2);
								_ScreenBuffer.Write(text8);
							}
							num4++;
							num5++;
						}
						_ScreenBuffer.Fill(41, num2, 77, num2 + num, 32, 0);
						int num6 = 0;
						int num7 = ScrollPosition[1];
						while (num6 < num && num7 < Objects[1].Count)
						{
							_ScreenBuffer.Goto(42, num6 + num2);
							GameObject gO2 = Objects[1][num7].GO;
							if (gO2 != null)
							{
								if (NumberSelected[1][num7] > 0)
								{
									_ScreenBuffer.Write("{{&Y^g|" + NumberSelected[1][num7] + "}} ");
								}
								_ScreenBuffer.Write(gO2.RenderForUI("Trade"));
								_ScreenBuffer.Write(" ");
								_ScreenBuffer.Write(gO2.DisplayName);
								string text5 = "";
								if (SideSelected == 1 && RowSelect == num6)
								{
									text3 = gO2.DisplayNameSingle;
									renderable = gO2.RenderForUI("Trade");
									text4 = " {{K|" + gO2.WeightEach + "#}}";
									if (screenMode == TradeScreenMode.Trade)
									{
										string text9 = (gO2.IsCurrency ? "Y" : "B");
										text5 = "{{" + text9 + "|$}}{{C|" + FormatPrice(GetValue(gO2, false), costMultiple) + "}}";
										text4 = text4 + " " + text5;
									}
								}
								else if (screenMode == TradeScreenMode.Trade)
								{
									string text10 = (gO2.IsCurrency ? "W" : "b");
									text5 = "{{" + text10 + "|$}}{{c|" + FormatPrice(GetValue(gO2, false), costMultiple) + "}}";
								}
								int x2 = 79 - ColorUtility.LengthExceptFormatting(text5);
								_ScreenBuffer.Goto(x2, num6 + num2);
								_ScreenBuffer.Write(text5);
							}
							else
							{
								string text11 = "{{K|[{{y|" + Objects[1][num7].CategoryName + "}}]}}";
								_ScreenBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(text11), num6 + num2);
								_ScreenBuffer.Write(text11);
							}
							num6++;
							num7++;
						}
						_ScreenBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
						_ScreenBuffer.Goto(2, 0);
						_ScreenBuffer.Write(s);
						_ScreenBuffer.Goto(42, 0);
						_ScreenBuffer.Write("[ {{W|Your inventory}} ]");
						_ScreenBuffer.Goto(40, 0);
						_ScreenBuffer.Write(194);
						for (int i = 1; i < 22; i++)
						{
							_ScreenBuffer.Goto(40, i);
							_ScreenBuffer.Write(179);
						}
						for (int j = 1; j < 79; j++)
						{
							_ScreenBuffer.Goto(j, 22);
							_ScreenBuffer.Write(196);
						}
						if (!Trader.IsCreature && Trader.IsOwned())
						{
							_ScreenBuffer.WriteAt(2, 22, "{{R|[ owned by someone else ]}}");
						}
						if (SideSelected == 0)
						{
							_ScreenBuffer.Goto(1, RowSelect + num2);
						}
						else
						{
							_ScreenBuffer.Goto(41, RowSelect + num2);
						}
						_ScreenBuffer.Write("{{&k^Y|>}}");
						_ScreenBuffer.Goto(40, 22);
						_ScreenBuffer.Write(193);
						_ScreenBuffer.Goto(0, 22);
						_ScreenBuffer.Write(195);
						_ScreenBuffer.Goto(79, 22);
						_ScreenBuffer.Write(180);
						if (Objects[SideSelected].Count > 0 && Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]] != null)
						{
							_ScreenBuffer.Goto(2, 23);
							if (renderable != null)
							{
								_ScreenBuffer.Write(renderable);
								_ScreenBuffer.Goto(4, 23);
							}
							_ScreenBuffer.Write(text3);
							if (!string.IsNullOrEmpty(text4))
							{
								_ScreenBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(text4), 23);
								_ScreenBuffer.Write(text4);
							}
						}
						int num8 = 2;
						_ScreenBuffer.Goto(num8, 24);
						if (ControlManager.activeControllerType != ControlManager.InputDeviceType.Gamepad)
						{
							_ScreenBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("Cancel") + "}} Exit]");
							num8 += 11;
						}
						_ScreenBuffer.Goto(num8, 24);
						_ScreenBuffer.Write(text);
						num8 += length + 1;
						if (ControlManager.activeControllerType != ControlManager.InputDeviceType.Gamepad)
						{
							_ScreenBuffer.Goto(num8, 24);
							_ScreenBuffer.Write("[{{W|0-9}} Pick]");
							num8 += 11;
						}
						if (screenMode == TradeScreenMode.Trade)
						{
							_ScreenBuffer.Goto(num8, 24);
							_ScreenBuffer.Write(text2);
							num8 += length2 + 1;
							if (Options.SifrahHaggling)
							{
								_ScreenBuffer.Goto(num8, 24);
								_ScreenBuffer.Write("[" + ControlManager.getCommandInputFormatted("SifraHaggle", mapGlyphs: false) + " Haggle]");
								num8 += 11;
							}
						}
						else
						{
							_ScreenBuffer.Goto(num8, 24);
							_ScreenBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("CmdTradeOffer", mapGlyphs: false) + "}} Transfer]");
							num8 += length2 + 4;
						}
						_ScreenBuffer.Goto(num8, 24);
						_ScreenBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("CmdVendorActions", mapGlyphs: false) + "}} Actions]");
						num8 += 16;
						if (screenMode == TradeScreenMode.Trade)
						{
							string text12 = " {{C|" + $"{Totals[0]:0.###}" + "}} drams ";
							_ScreenBuffer.Goto(39 - ColorUtility.LengthExceptFormatting(text12), 22);
							_ScreenBuffer.Write(text12);
							_ScreenBuffer.Goto(42, 22);
							_ScreenBuffer.Write(" {{C|" + $"{Totals[1]:0.###}" + "}} drams ÄÄ {{W|$" + The.Player.GetFreeDrams() + "}} ");
						}
						for (int k = 0; k <= 1; k++)
						{
							if (Objects[k].Count <= num)
							{
								continue;
							}
							for (int l = 1; l < 22; l++)
							{
								if (k == 0)
								{
									_ScreenBuffer.Goto(0, l);
								}
								else
								{
									_ScreenBuffer.Goto(79, l);
								}
								_ScreenBuffer.Write(177, ColorUtility.Bright((ushort)0), 0);
							}
							_ = (int)Math.Ceiling((double)Objects[k].Count / (double)num);
							int num9 = (int)Math.Ceiling((double)(Objects[k].Count + num) / (double)num);
							_ = 0;
							if (num9 <= 0)
							{
								num9 = 1;
							}
							int num10 = 21 / num9;
							if (num10 <= 0)
							{
								num10 = 1;
							}
							int num11 = (int)((double)(21 - num10) * ((double)ScrollPosition[k] / (double)(Objects[k].Count - num)));
							num11++;
							for (int m = num11; m < num11 + num10; m++)
							{
								if (k == 0)
								{
									_ScreenBuffer.Goto(0, m);
								}
								else
								{
									_ScreenBuffer.Goto(79, m);
								}
								_ScreenBuffer.Write(219, ColorUtility.Bright(7), 0);
							}
						}
						int num12 = (int)(LiquidVolume.GetLiquid("water").Weight * (double)CalculateTrade(Totals[0], Totals[1]));
						int num13 = Math.Max(0, nTotalWeight + Weight[0] - Weight[1] - num12);
						string text13 = "K";
						if (num13 > nMaxWeight)
						{
							text13 = "R";
						}
						string text14 = " {{" + text13 + "|" + num13 + "/" + nMaxWeight + " lbs.}} ";
						_ScreenBuffer.Goto(77 - ColorUtility.LengthExceptFormatting(text14), 22);
						_ScreenBuffer.Write(text14);
						_TextConsole.DrawBuffer(_ScreenBuffer);
						keys = Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
						if (keys == Keys.Escape)
						{
							flag6 = true;
						}
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")
						{
							flag6 = true;
						}
						if (keys >= Keys.D0 && keys <= Keys.D9)
						{
							if (Objects[SideSelected].Count > 0)
							{
								GameObject gO3 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
								if (gO3 != null)
								{
									int num14 = (int)(keys - 48);
									num3 = ((num3 < gO3.Count) ? (num3 * 10 + num14) : num14);
									if (SideSelected == 1 && NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] == 0 && (ScreenMode != TradeScreenMode.Container || Trader.IsOwned()) && !gO3.ConfirmUseImportant(null, TradeScreenVerb, null, num3))
									{
										continue;
									}
									if (num3 > gO3.Count)
									{
										NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = gO3.Count;
									}
									else
									{
										NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = num3;
									}
								}
								UpdateTotals(Totals, Weight, Objects, NumberSelected);
								continue;
							}
						}
						else
						{
							num3 = 0;
						}
						if (keys == Keys.Oemtilde && Objects[SideSelected].Count > 0)
						{
							NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = 0;
							UpdateTotals(Totals, Weight, Objects, NumberSelected);
							continue;
						}
						if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorActions" && Objects[SideSelected].Count > 0)
						{
							GameObject gO4 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							if (gO4 == null)
							{
								continue;
							}
							ShowVendorActions(gO4, Trader);
						}
						if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorRepair")
						{
							if (flag2)
							{
								if (Objects[SideSelected].Count > 0)
								{
									GameObject gO5 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
									if (gO5 != null)
									{
										DoVendorRepair(gO5, Trader);
									}
								}
							}
							else
							{
								Popup.Show(Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + Trader.GetVerb("do") + " not have the skill to repair items.");
							}
							continue;
						}
						if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTradeToggleAll")
						{
							bool flag7 = false;
							int n = 0;
							for (int count = Objects[SideSelected].Count; n < count; n++)
							{
								GameObject gO6 = Objects[SideSelected][n].GO;
								if (gO6 != null)
								{
									int count2 = gO6.Count;
									if (NumberSelected[SideSelected][n] != count2 && (SideSelected != 1 || !gO6.IsImportant()))
									{
										NumberSelected[SideSelected][n] = count2;
										flag7 = true;
									}
								}
							}
							if (!flag7)
							{
								int num15 = 0;
								for (int num16 = NumberSelected[SideSelected].Length; num15 < num16; num15++)
								{
									NumberSelected[SideSelected][num15] = 0;
								}
							}
							UpdateTotals(Totals, Weight, Objects, NumberSelected);
							continue;
						}
						if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTradeAllItems" && Objects[SideSelected].Count > 0)
						{
							GameObject gO7 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							if (gO7 != null)
							{
								if (NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] == gO7.Count)
								{
									NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = 0;
								}
								else
								{
									if (SideSelected == 1 && (ScreenMode != TradeScreenMode.Container || Trader.IsOwned()) && !gO7.ConfirmUseImportant(null, TradeScreenVerb))
									{
										goto IL_18dc;
									}
									NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] = gO7.Count;
								}
								UpdateTotals(Totals, Weight, Objects, NumberSelected);
							}
						}
						goto IL_18dc;
					}
					ScreenBuffer.ClearImposterSuppression();
					if (flag5)
					{
						The.Player.UseEnergy(1000, "Trading");
						_Trader.UseEnergy(1000, "Trading");
					}
					GameManager.Instance.PopGameView();
					_TextConsole.DrawBuffer(TextConsole.ScrapBuffer2);
					_Trader = null;
					Reset();
					if (flag)
					{
						Trader.Brain.PerformReequip();
					}
					return;
					IL_18dc:
					if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorLook")
					{
						if (Objects[SideSelected].Count > 0)
						{
							GameObject gO8 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							InventoryActionEvent.Check(gO8, The.Player, gO8, "Look");
						}
						continue;
					}
					if ((Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorRead") & flag4)
					{
						if (Objects[SideSelected].Count > 0)
						{
							GameObject gO9 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							InventoryActionEvent.Check(gO9, The.Player, gO9, "Read");
						}
						continue;
					}
					if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorRecharge")
					{
						if (!flag3)
						{
							continue;
						}
						if (Objects[SideSelected].Count > 0)
						{
							GameObject gO10 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							if (gO10 != null && (gO10.Understood() || identifyLevel >= gO10.GetComplexity()) && DoVendorRecharge(gO10, Trader))
							{
								break;
							}
						}
						else
						{
							Popup.Show(Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + Trader.GetVerb("do") + " not have the skill to recharge items.");
						}
						continue;
					}
					if ((Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorRecharge") & flag4)
					{
						if (Objects[SideSelected].Count > 0)
						{
							GameObject gO11 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							InventoryActionEvent.Check(gO11, The.Player, gO11, "Read");
						}
						continue;
					}
					if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdVendorExamine")
					{
						if (Objects[SideSelected].Count > 0)
						{
							GameObject gO12 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
							if (gO12 != null)
							{
								DoVendorExamine(gO12, Trader);
							}
						}
						continue;
					}
					if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTradeAdd" && Objects[SideSelected].Count > 0)
					{
						GameObject gO13 = Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO;
						if (gO13 != null && (SideSelected != 1 || NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] != 0 || (ScreenMode == TradeScreenMode.Container && !Trader.IsOwned()) || gO13.ConfirmUseImportant(null, TradeScreenVerb, null, 1)))
						{
							if (NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] < Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO.Count)
							{
								NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]]++;
							}
							UpdateTotals(Totals, Weight, Objects, NumberSelected);
						}
					}
					if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTradeRemove" && Objects[SideSelected].Count > 0 && Objects[SideSelected][RowSelect + ScrollPosition[SideSelected]].GO != null)
					{
						if (NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]] > 0)
						{
							NumberSelected[SideSelected][RowSelect + ScrollPosition[SideSelected]]--;
						}
						UpdateTotals(Totals, Weight, Objects, NumberSelected);
					}
					if (keys == Keys.NumPad4 && SideSelected == 1 && Objects[0].Count > 0)
					{
						SideSelected = 0;
					}
					if (keys == Keys.NumPad6 && SideSelected == 0 && Objects[1].Count > 0)
					{
						SideSelected = 1;
					}
					if (keys == Keys.NumPad8)
					{
						if (RowSelect == 0)
						{
							if (ScrollPosition[SideSelected] > 0)
							{
								ScrollPosition[SideSelected]--;
							}
						}
						else
						{
							RowSelect--;
						}
					}
					if (keys == Keys.NumPad2)
					{
						if (RowSelect < num - 1 && RowSelect + ScrollPosition[SideSelected] < Objects[SideSelected].Count - 1)
						{
							RowSelect++;
						}
						else if (ScrollPosition[SideSelected] + num < Objects[SideSelected].Count)
						{
							ScrollPosition[SideSelected]++;
						}
					}
					if (keys == Keys.Prior || Keyboard.RawCode == Keys.Prior || Keyboard.RawCode == Keys.Back)
					{
						if (RowSelect > 0)
						{
							RowSelect = 0;
						}
						else
						{
							ScrollPosition[SideSelected] -= num - 1;
							if (ScrollPosition[SideSelected] < 0)
							{
								ScrollPosition[SideSelected] = 0;
							}
						}
					}
					if (keys == Keys.Next || keys == Keys.Next || Keyboard.RawCode == Keys.Next || Keyboard.RawCode == Keys.Next)
					{
						if (RowSelect < num - 1)
						{
							RowSelect = num - 1;
							if (RowSelect + ScrollPosition[SideSelected] >= Objects[SideSelected].Count - 1)
							{
								RowSelect = Objects[SideSelected].Count - 1 - ScrollPosition[SideSelected];
							}
						}
						else if (RowSelect == num - 1)
						{
							ScrollPosition[SideSelected] += num - 1;
							if (RowSelect + ScrollPosition[SideSelected] >= Objects[SideSelected].Count - 1)
							{
								ScrollPosition[SideSelected] = Objects[SideSelected].Count - 1 - RowSelect;
							}
						}
					}
					if (RowSelect + ScrollPosition[SideSelected] >= Objects[SideSelected].Count - 1)
					{
						RowSelect = Objects[SideSelected].Count - 1 - ScrollPosition[SideSelected];
					}
					bool flag8 = false;
					bool forceComplete = false;
					if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:SifrahHaggle" && Options.SifrahHaggling && screenMode == TradeScreenMode.Trade)
					{
						if (Totals[0] == 0.0 && Totals[1] == 0.0)
						{
							Popup.ShowFail("Set up a trade, then haggle for it.");
							continue;
						}
						int num17 = CalculateTrade(Totals[0], Totals[1]);
						if (num17 > 0 && num17 > The.Player.GetFreeDrams() * 2)
						{
							Popup.ShowFail("You must have at least half the fresh water you would need to pay for this trade available in order to attempt to haggle for it.");
							continue;
						}
						HagglingSifrah hagglingSifrah = new HagglingSifrah(Trader);
						hagglingSifrah.Play(Trader);
						if (hagglingSifrah.Abort)
						{
							continue;
						}
						if (hagglingSifrah.Performance > 0)
						{
							Performance += (1.0 - Performance) * (double)hagglingSifrah.Performance / 100.0;
						}
						else
						{
							Performance -= Performance * (double)(-hagglingSifrah.Performance) / 100.0;
						}
						UpdateTotals(Totals, Weight, Objects, NumberSelected);
						int num18 = CalculateTrade(Totals[0], Totals[1]);
						string text15 = null;
						if (num17 == num18)
						{
							text15 = "In the end, though, it makes no difference.";
						}
						else if (num17 >= 0 && num18 >= 0)
						{
							text15 = "As a result, the trade costs you " + num18 + " " + ((num18 == 1) ? "dram" : "drams") + " rather than " + num17 + ".";
						}
						else if (num17 < 0 && num18 < 0)
						{
							text15 = "As a result, the trade is worth " + -num18 + " " + ((-num18 == 1) ? "dram" : "drams") + " rather than " + -num17 + ".";
						}
						else if (num17 >= 0 && num18 < 0)
						{
							text15 = "As a result, the trade goes from costing you " + num17 + " " + ((num17 == 1) ? "dram" : "drams") + " to being worth " + -num18 + ".";
						}
						else if (num17 < 0 && num18 >= 0)
						{
							text15 = "As a result, the trade goes from being worth " + -num17 + " " + ((-num17 == 1) ? "dram" : "drams") + " to being worth " + num18 + ".";
						}
						StringBuilder stringBuilder = Event.NewStringBuilder();
						if (!string.IsNullOrEmpty(hagglingSifrah.Description))
						{
							stringBuilder.Compound(hagglingSifrah.Description);
						}
						if (!string.IsNullOrEmpty(text15))
						{
							stringBuilder.Compound(text15);
						}
						if (stringBuilder.Length > 0)
						{
							Popup.Show(stringBuilder.ToString());
						}
						flag8 = true;
						forceComplete = true;
					}
					if (Keyboard.vkCode == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTradeOffer")
					{
						flag8 = true;
					}
					int difference = CalculateTrade(Totals[0], Totals[1]);
					if (!flag8)
					{
						continue;
					}
					switch (PerformOffer(difference, forceComplete, Trader, screenMode, Objects, NumberSelected))
					{
					case OfferStatus.TOP:
						break;
					case OfferStatus.REFRESH:
						goto end_IL_243c;
					default:
						continue;
					}
					goto end_IL_04bf;
					continue;
					end_IL_243c:
					break;
				}
				continue;
				end_IL_04bf:
				break;
			}
		}
	}

	private static bool TryRemove(GameObject Object, GameObject Receiver, List<GameObject> TradeToPlayer, List<GameObject> TradeToTrader, bool Force = false)
	{
		Event e = Event.New("CommandRemoveObject", "Object", Object).SetSilent(Silent: true);
		if (Receiver.FireEvent(e) | Force)
		{
			return true;
		}
		string text = (Receiver.IsPlayer() ? "you" : Receiver.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true));
		Popup.ShowBlock("Trade could not be completed, " + text + " couldn't drop object: " + Object.DisplayName);
		ReturnItems(TradeToPlayer, TradeToTrader);
		if (GameObject.Validate(ref Object))
		{
			Object.CheckStack();
		}
		return false;
	}

	public static string ShowVendorActions(GameObject GO, GameObject Trader, bool IncludeModernTradeOptions = false)
	{
		if (GO == null || Trader == null)
		{
			return null;
		}
		Tinkering_Repair part = Trader.GetPart<Tinkering_Repair>();
		int identifyLevel = Tinkering.GetIdentifyLevel(Trader);
		bool flag = identifyLevel > 0;
		bool flag2 = part != null;
		bool flag3 = Trader.HasSkill("Tinkering_Tinker1");
		bool flag4 = Trader.GetIntProperty("Librarian") != 0;
		List<string> list = new List<string> { "Look" };
		List<char> list2 = new List<char> { 'l' };
		if (IncludeModernTradeOptions)
		{
			list.Add("Add to trade");
			list2.Add('t');
		}
		if (flag && !GO.Understood())
		{
			list.Add("Identify");
			list2.Add('i');
		}
		if (flag2 && IsRepairableEvent.Check(Trader, GO, null, part))
		{
			list.Add("Repair");
			list2.Add('r');
		}
		if (flag3 && (GO.Understood() || identifyLevel >= GO.GetComplexity()) && GO.NeedsRecharge())
		{
			list.Add("Recharge");
			list2.Add('c');
		}
		if (flag4 && GO.HasInventoryActionWithCommand("Read"))
		{
			list.Add("Read");
			list2.Add('b');
		}
		int num = Popup.PickOption("select an action", null, "", "Sounds/UI/ui_notification", list.ToArray(), list2.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
		if (num >= 0)
		{
			if (list[num] == "Identify")
			{
				DoVendorExamine(GO, Trader);
			}
			if (list[num] == "Read")
			{
				DoVendorRead(GO, Trader);
			}
			if (list[num] == "Repair")
			{
				DoVendorRepair(GO, Trader);
			}
			if (list[num] == "Look")
			{
				DoVendorLook(GO, Trader);
			}
			if (list[num] == "Recharge")
			{
				DoVendorRecharge(GO, Trader);
			}
			return list[num];
		}
		return null;
	}

	public static void DoVendorExamine(GameObject GO, GameObject Trader)
	{
		int identifyLevel = Tinkering.GetIdentifyLevel(Trader);
		if (identifyLevel > 0)
		{
			if (!GO.Understood())
			{
				int complexity = GO.GetComplexity();
				int examineDifficulty = GO.GetExamineDifficulty();
				if (The.Player.HasPart<Dystechnia>())
				{
					Popup.ShowFail("You can't understand " + Grammar.MakePossessive(Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)) + " explanation.");
					return;
				}
				if (identifyLevel < complexity)
				{
					Popup.ShowFail("This item is too complex for " + Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " to identify.");
					return;
				}
				float num = complexity + examineDifficulty;
				int num2 = (int)Math.Max(2.0, -0.0667 + 1.24 * (double)num + 0.0967 * Math.Pow(num, 2.0) + 0.0979 * Math.Pow(num, 3.0));
				if (The.Player.GetFreeDrams() < num2)
				{
					Popup.ShowFail("You do not have the required {{C|" + num2 + "}} " + ((num2 == 1) ? "dram" : "drams") + " to identify this item.");
				}
				else if (Popup.ShowYesNo("You may identify this for " + num2 + " " + ((num2 == 1) ? "dram" : "drams") + " of fresh water.") == DialogResult.Yes && The.Player.UseDrams(num2))
				{
					Trader.GiveDrams(num2);
					Popup.Show(Trader.does("identify") + " " + GO.the + " " + GO.GetDisplayName() + " as " + GO.an(int.MaxValue, null, null, AsIfKnown: true) + ".");
					GO.MakeUnderstood();
				}
			}
			else
			{
				Popup.ShowFail("You already understand this item.");
			}
		}
		else
		{
			Popup.Show(Trader.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " have the skill to identify artifacts.");
		}
	}

	public static void DoVendorRead(GameObject GO, GameObject Trader)
	{
		InventoryActionEvent.Check(GO, The.Player, GO, "Read");
	}

	public static void DoVendorRepair(GameObject GO, GameObject Trader)
	{
		if (GO == null || Trader == null)
		{
			return;
		}
		Tinkering_Repair part = Trader.GetPart<Tinkering_Repair>();
		if (part == null)
		{
			return;
		}
		bool flag = GO.IsPlural || GO.Count > 1;
		if (IsRepairableEvent.Check(Trader, GO, null, part))
		{
			if (!Tinkering_Repair.IsRepairableBy(GO, Trader, null, part))
			{
				Popup.Show((flag ? "These items are" : "This item is") + " too complex for " + Trader.t() + " to repair.");
				return;
			}
			int num = Math.Max(5 + (int)(GetValue(GO, false) / 25.0), 5) * GO.Count;
			if (The.Player.GetFreeDrams() < num)
			{
				Popup.Show("You need {{C|" + num + "}} " + ((num == 1) ? "dram" : "drams") + " of fresh water to repair " + (flag ? "those" : "that") + ".");
			}
			else if (Popup.ShowYesNo("You may repair " + (flag ? "those" : "this") + " for {{C|" + num + "}} " + ((num == 1) ? "dram" : "drams") + " of fresh water.") == DialogResult.Yes && The.Player.UseDrams(num))
			{
				Trader.GiveDrams(num);
				The.Player.PlayWorldOrUISound("Sounds/Misc/sfx_interact_artifact_repair");
				RepairedEvent.Send(Trader, GO, null, part);
			}
		}
		else
		{
			Popup.Show((flag ? "Those items aren't" : "That item isn't") + " broken!");
		}
	}

	public static void DoVendorLook(GameObject GO, GameObject Trader)
	{
		InventoryActionEvent.Check(GO, The.Player, GO, "Look");
	}

	public static OfferStatus PerformOffer(int Difference, bool forceComplete, GameObject Trader, TradeScreenMode screenMode, List<TradeEntry>[] Objects, int[][] NumberSelected)
	{
		if (Difference > 0)
		{
			int freeDrams = The.Player.GetFreeDrams();
			if (freeDrams >= Difference)
			{
				if (forceComplete)
				{
					Popup.Show("You pony up " + Difference + " " + ((Difference == 1) ? "dram" : "drams") + " of fresh water to even up the trade.");
				}
				else if (Popup.ShowYesNo("You'll have to pony up " + Difference + " " + ((Difference == 1) ? "dram" : "drams") + " of fresh water to even up the trade. Agreed?") == DialogResult.No)
				{
					return OfferStatus.NEXT;
				}
			}
			else
			{
				if (!forceComplete)
				{
					Popup.Show("You don't have " + Difference + " " + ((Difference == 1) ? "dram" : "drams") + " of fresh water to even up the trade!");
					return OfferStatus.NEXT;
				}
				int num = Difference - freeDrams;
				if (freeDrams > 0)
				{
					Popup.Show("You pony up " + freeDrams + " " + ((freeDrams == 1) ? "dram" : "drams") + " of fresh water, and now owe " + Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " " + num + " " + ((num == 1) ? "dram" : "drams") + ".");
				}
				else
				{
					Popup.Show("You now owe " + Trader.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " " + num + " " + ((num == 1) ? "dram" : "drams") + " of fresh water.");
				}
				Trader.ModIntProperty("TraderCreditExtended", num);
			}
		}
		if (Difference < 0)
		{
			int num2 = -Difference;
			if (AssumeTradersHaveWater || Trader.GetFreeDrams() >= num2)
			{
				if (forceComplete)
				{
					Popup.Show(Trader.Does("pony", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " up " + num2 + " " + ((num2 == 1) ? "dram" : "drams") + " of fresh water to even up the trade.");
				}
				else if (Popup.ShowYesNo(Trader.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " will have to pony up " + num2 + " " + ((num2 == 1) ? "dram" : "drams") + " of fresh water to even up the trade. Agreed?") == DialogResult.No)
				{
					return OfferStatus.NEXT;
				}
			}
			else if (forceComplete)
			{
				Popup.Show(Trader.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " have " + num2 + " " + ((num2 == 1) ? "dram" : "drams") + " of fresh water to even up the trade!");
			}
			else if (Popup.ShowYesNo(Trader.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " have " + num2 + " " + ((num2 == 1) ? "dram" : "drams") + " of fresh water to even up the trade! Do you want to complete the trade anyway?") != DialogResult.Yes)
			{
				return OfferStatus.NEXT;
			}
		}
		int num3 = -Difference;
		List<GameObject> list = null;
		for (int i = 0; i < Objects[1].Count; i++)
		{
			if (NumberSelected[1][i] > 0)
			{
				if (list == null)
				{
					list = new List<GameObject>();
				}
				list.Add(Objects[1][i].GO);
			}
		}
		int num4 = The.Player.GetStorableDrams("water", null, list);
		List<GameObject> countForStore = null;
		for (int j = 0; j < Objects[0].Count; j++)
		{
			if (NumberSelected[0][j] > 0 && NumberSelected[0][j] >= Objects[0][j].GO.Count)
			{
				if (countForStore == null)
				{
					countForStore = new List<GameObject>();
				}
				countForStore.Add(Objects[0][j].GO);
			}
		}
		if (countForStore != null)
		{
			num4 += Trader.GetStorableDrams("water", null, null, (GameObject o) => countForStore.Contains(o));
		}
		if (Difference < 0 && num4 < num3)
		{
			if (forceComplete)
			{
				Popup.Show("You don't have enough water containers to carry that many drams! You can store " + num4 + " " + ((num4 == 1) ? "dram" : "drams") + ".");
			}
			else if (Popup.ShowYesNo("You don't have enough water containers to carry that many drams! Do you want to complete the trade for the " + num4 + " " + ((num4 == 1) ? "dram" : "drams") + " you can store?") == DialogResult.No)
			{
				return OfferStatus.NEXT;
			}
		}
		if (screenMode == TradeScreenMode.Container && NumberSelected[0].Any((int num7) => num7 > 0) && !Trader.FireEvent(Event.New("BeforeContentsTaken", "Taker", The.Player)))
		{
			return OfferStatus.REFRESH;
		}
		if (Difference > 0)
		{
			The.Player.UseDrams(Difference);
		}
		List<GameObject> list2 = new List<GameObject>(16);
		List<GameObject> list3 = new List<GameObject>(16);
		for (int num5 = 0; num5 < Objects[0].Count; num5++)
		{
			if (NumberSelected[0][num5] > 0)
			{
				GameObject gO = Objects[0][num5].GO;
				gO.SplitStack(NumberSelected[0][num5], Trader);
				if (!TryRemove(gO, Trader, list3, list2, forceComplete))
				{
					return OfferStatus.REFRESH;
				}
				gO.RemoveIntProperty("_stock");
				list3.Add(gO);
			}
		}
		for (int num6 = 0; num6 < Objects[1].Count; num6++)
		{
			if (NumberSelected[1][num6] <= 0)
			{
				continue;
			}
			GameObject gO2 = Objects[1][num6].GO;
			gO2.SplitStack(NumberSelected[1][num6], The.Player);
			if (!TryRemove(gO2, The.Player, list3, list2, forceComplete))
			{
				return OfferStatus.REFRESH;
			}
			if (Trader.IsMerchant() && costMultiple > 0f)
			{
				gO2.SetIntProperty("_stock", 1);
				gO2.RemoveIntProperty("StoredByPlayer");
				gO2.RemoveIntProperty("FromStoredByPlayer");
			}
			else
			{
				gO2.RemoveIntProperty("_stock");
				if (Trader.HasPart<Container>())
				{
					gO2.SetIntProperty("StoredByPlayer", 1);
				}
			}
			list2.Add(gO2);
		}
		if (screenMode == TradeScreenMode.Container)
		{
			if (list3.Count > 0 && !Trader.FireEvent(Event.New("AfterContentsTaken", "Taker", The.Player)))
			{
				ReturnItems(list3, list2);
				return OfferStatus.REFRESH;
			}
			foreach (GameObject item in list2)
			{
				try
				{
					if (!Trader.FireEvent(Event.New("CommandTakeObject", "Object", item, "PutBy", The.Player, "EnergyCost", 0)))
					{
						The.Player.ReceiveObject(item);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("trade move to container", x);
				}
			}
		}
		else
		{
			foreach (GameObject item2 in list2)
			{
				try
				{
					if (item2?.Physics != null)
					{
						item2.Physics.Owner = null;
					}
					Trader.TakeObject(item2, NoStack: false, Silent: false, 0);
				}
				catch (Exception x2)
				{
					MetricsManager.LogException("trade move to trader", x2);
				}
			}
			if (list2.Count > 0 && Trader.HasPart<Container>())
			{
				The.Player.FireEvent(Event.New("PutSomethingIn", "Object", Trader));
			}
		}
		foreach (GameObject item3 in list3)
		{
			try
			{
				if (!The.Player.TakeObject(item3, NoStack: false, Silent: false, 0))
				{
					Trader.ReceiveObject(item3);
				}
			}
			catch (Exception x3)
			{
				MetricsManager.LogException("trade move to player", x3);
			}
		}
		try
		{
			if (Difference < 0)
			{
				The.Player.GiveDrams(num3);
				Trader.UseDrams(num3);
			}
			else if (Difference > 0)
			{
				Trader.GiveDrams(Difference);
			}
		}
		catch (Exception x4)
		{
			MetricsManager.LogException("trade water exchange", x4);
		}
		if (screenMode == TradeScreenMode.Trade)
		{
			if (list2.Count > 0 || list3.Count > 0)
			{
				Popup.Show("Trade complete!");
			}
			else
			{
				Popup.Show("Nothing to trade.");
			}
		}
		if (list2.Count > 0 && Trader.IsPlayerLed())
		{
			Trader.Brain.PerformReequip();
		}
		return OfferStatus.TOP;
	}

	private static void ReturnItems(List<GameObject> TradeToPlayer, List<GameObject> TradeToTrader)
	{
		foreach (GameObject item in TradeToPlayer)
		{
			_Trader.ReceiveObject(item);
		}
		foreach (GameObject item2 in TradeToTrader)
		{
			The.Player.ReceiveObject(item2);
		}
	}

	public static bool DoVendorRecharge(GameObject GO, GameObject Trader)
	{
		bool AnyRelevant = false;
		bool AnyRechargeable = false;
		bool AnyNotFullyCharged = false;
		bool AnyRecharged = false;
		Predicate<IRechargeable> proc = (IRechargeable P) =>
		{
			AnyRelevant = true;
			if (!P.CanBeRecharged())
			{
				return true;
			}
			AnyRechargeable = true;
			int rechargeAmount = P.GetRechargeAmount();
			if (rechargeAmount <= 0)
			{
				return true;
			}
			AnyNotFullyCharged = true;
			int num = Math.Max(rechargeAmount / 500, 1);
			string text = ((P.ParentObject.Count > 1) ? "one of those" : P.ParentObject.indicativeDistal);
			if (The.Player.GetFreeDrams() < num)
			{
				Popup.Show("You need {{C|" + Grammar.Cardinal(num) + "}} " + ((num == 1) ? "dram" : "drams") + " of fresh water to charge " + text + ".");
				return false;
			}
			if (Popup.ShowYesNo("You may recharge " + text + " for {{C|" + Grammar.Cardinal(num) + "}} " + ((num == 1) ? "dram" : "drams") + " of fresh water.") == DialogResult.Yes && The.Player.UseDrams(num))
			{
				P.ParentObject.SplitFromStack();
				P.AddCharge(rechargeAmount);
				P.ParentObject.CheckStack();
				Trader.GiveDrams(num);
				AnyRecharged = true;
			}
			return true;
		};
		GO.ForeachPartDescendedFrom(proc);
		EnergyCellSocket part = GO.GetPart<EnergyCellSocket>();
		if (part != null && part.Cell != null)
		{
			part.Cell.ForeachPartDescendedFrom(proc);
		}
		if (!AnyRelevant)
		{
			Popup.Show("That item has no cell or rechargeable capacitor in it.");
		}
		else if (!AnyRechargeable)
		{
			Popup.Show("That item cannot be recharged this way.");
		}
		else if (!AnyNotFullyCharged)
		{
			Popup.Show(GO.Does("are", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " fully charged!");
		}
		if (AnyRecharged && Options.Sound)
		{
			SoundManager.PlaySound("Sounds/Abilities/sfx_ability_energyCell_recharge");
		}
		return AnyRecharged;
	}

	public static int CalculateTrade(double Bought, double Sold)
	{
		double num = Bought - Sold;
		if (num > 0.0)
		{
			num += 0.0001;
		}
		else if (num < 0.0)
		{
			num -= 0.0001;
		}
		if (Math.Abs(num) < 10.0)
		{
			return (int)Math.Ceiling(num);
		}
		return (int)Math.Round(num, MidpointRounding.AwayFromZero);
	}
}
