using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Tinkering;

public class Disassembly : OngoingAction
{
	public GameObject Object;

	public int NumberWanted;

	public int NumberDone;

	public int TotalNumberWanted;

	public int TotalNumberDone;

	public int OriginalCount;

	public int TotalOriginalCount;

	public int BitChance = int.MinValue;

	public int EnergyCostPer = 1000;

	public bool Auto;

	public bool WasTemporary;

	public bool Abort;

	public bool DoBitMessage;

	public string DisassemblingWhat;

	public string DisassemblingWhere;

	public string DisassembledWhat;

	public string DisassembledWhere;

	public string ReverseEngineeringMessage;

	public string BitsDone = "";

	public string InterruptBecause;

	public List<string> DisassembledWhats;

	public Dictionary<string, string> DisassembledWhatsWhere;

	public List<Action<GameObject>> Alarms;

	public List<GameObject> Queue;

	public Dictionary<GameObject, int> QueueNumberWanted;

	public Dictionary<GameObject, List<Action<GameObject>>> QueueAlarms;

	public Disassembly(GameObject Object = null, int NumberWanted = 1, bool Auto = false, List<Action<GameObject>> Alarms = null, int EnergyCostPer = 1000)
	{
		this.Object = Object;
		this.NumberWanted = NumberWanted;
		TotalNumberWanted = this.NumberWanted;
		this.Auto = Auto;
		this.Alarms = Alarms;
		this.EnergyCostPer = EnergyCostPer;
		OriginalCount = this.Object.Count;
		TotalOriginalCount = OriginalCount;
		WasTemporary = true;
	}

	public override string GetDescription()
	{
		return "disassembling";
	}

	public override bool ShouldHostilesInterrupt()
	{
		return TotalNumberWanted > 1;
	}

	public override bool Continue()
	{
		GameObject player = The.Player;
		if (!GameObject.Validate(ref Object))
		{
			InterruptBecause = "the item you were working on disappeared";
			return false;
		}
		if (Object.IsInGraveyard())
		{
			InterruptBecause = "the item you were working on was destroyed";
			return false;
		}
		if (Object.IsNowhere())
		{
			InterruptBecause = "the item you were working on disappeared";
			return false;
		}
		if (Object.InInventory != player && !Object.InSameOrAdjacentCellTo(player))
		{
			InterruptBecause = Object.does("are") + " no longer within your reach";
			return false;
		}
		if (Object.IsInStasis())
		{
			InterruptBecause = "you can no longer interact with " + Object.t();
			return false;
		}
		if (!Object.TryGetPart<TinkerItem>(out var Part))
		{
			InterruptBecause = Object.t() + " can no longer be disassembled";
			return false;
		}
		if (!player.HasSkill("Tinkering_Disassemble"))
		{
			InterruptBecause = "you no longer know how to disassemble things";
			return false;
		}
		if (!Part.CanBeDisassembled(player))
		{
			InterruptBecause = Object.t() + " can no longer be disassembled";
			return false;
		}
		if (!player.CanMoveExtremities("Disassemble", ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
		{
			InterruptBecause = "you can no longer move your extremities";
			return false;
		}
		int num = 0;
		try
		{
			bool Interrupt = false;
			if (BitChance == int.MinValue)
			{
				if (Part.Bits.Length == 1)
				{
					BitChance = 0;
				}
				else
				{
					int intProperty = player.GetIntProperty("DisassembleBonus");
					BitChance = 50;
					intProperty = GetTinkeringBonusEvent.GetFor(player, Object, "Disassemble", BitChance, intProperty, ref Interrupt);
					if (Interrupt)
					{
						return false;
					}
					BitChance += intProperty;
				}
			}
			string activeBlueprint = Part.ActiveBlueprint;
			TinkerData tinkerData = null;
			List<TinkerData> list = null;
			if (player.HasSkill("Tinkering_ReverseEngineer"))
			{
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (!(tinkerRecipe.Type == "Build") || !(tinkerRecipe.Blueprint == activeBlueprint))
					{
						continue;
					}
					tinkerData = tinkerRecipe;
					foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
					{
						if (knownRecipe.Blueprint == activeBlueprint)
						{
							tinkerData = null;
							break;
						}
					}
					break;
				}
				foreach (TinkerData tinkerRecipe2 in TinkerData.TinkerRecipes)
				{
					if (!(tinkerRecipe2.Type == "Mod") || !Object.HasPart(tinkerRecipe2.PartName))
					{
						continue;
					}
					bool flag = false;
					foreach (TinkerData knownRecipe2 in TinkerData.KnownRecipes)
					{
						if (knownRecipe2.Blueprint == tinkerRecipe2.Blueprint)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (list == null)
						{
							list = new List<TinkerData>();
						}
						list.Add(tinkerRecipe2);
					}
				}
			}
			int chance = 0;
			int num2 = 0;
			if (tinkerData != null || (list != null && list.Count > 0))
			{
				chance = 15;
				num2 = GetTinkeringBonusEvent.GetFor(player, Object, "ReverseEngineer", chance, num2, ref Interrupt);
				if (Interrupt)
				{
					return false;
				}
				chance += num2;
			}
			bool flag2 = Options.SifrahReverseEngineer && (tinkerData != null || (list != null && list.Count > 0));
			bool flag3 = false;
			if (flag2)
			{
				flag3 = Popup.ShowYesNo("Do you want to try to reverse engineer " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + "?", "Sounds/UI/ui_notification", AllowEscape: false) == DialogResult.Yes;
			}
			ReverseEngineeringSifrah reverseEngineeringSifrah = null;
			int reverseEngineerRating = 0;
			int complexity = 0;
			int difficulty = 0;
			if (flag3)
			{
				reverseEngineerRating = player.Stat("Intelligence") + num2;
				Examiner part = Object.GetPart<Examiner>();
				complexity = part?.Complexity ?? Object.GetTier();
				difficulty = part?.Difficulty ?? 0;
			}
			try
			{
				InventoryActionEvent.Check(Object, player, Object, "EmptyForDisassemble");
			}
			catch (Exception x)
			{
				MetricsManager.LogError("EmptyForDisassemble", x);
			}
			bool flag4 = NumberWanted > 1 && OriginalCount > 1;
			if (!Object.IsTemporary)
			{
				WasTemporary = false;
			}
			if (DisassemblingWhat == null)
			{
				DisassemblingWhat = Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
				if (flag4 || TotalNumberWanted > 1)
				{
					MessageQueue.AddPlayerMessage("You start disassembling " + Object.t() + ".");
				}
			}
			if (DisassemblingWhere == null && Object.CurrentCell != null)
			{
				DisassemblingWhere = player.DescribeDirectionToward(Object);
			}
			bool flag5 = false;
			if (NumberDone < NumberWanted)
			{
				string text = "";
				bool flag6 = true;
				bool flag7 = false;
				bool flag8 = false;
				int num3 = 0;
				GameObject gameObject = null;
				if (!WasTemporary)
				{
					if (Part.Bits.Length == 1)
					{
						if (Part.NumberMade <= 1 || Stat.Random(1, Part.NumberMade + 1) == 1)
						{
							text += Part.Bits;
						}
					}
					else
					{
						int num4 = Part.Bits.Length - 1;
						for (int i = 0; i < Part.Bits.Length; i++)
						{
							if ((num4 == i || BitChance.in100()) && (Part.NumberMade <= 1 || Stat.Random(1, Part.NumberMade + 1) == 1))
							{
								text += Part.Bits[i];
							}
						}
					}
					if (flag2)
					{
						if (flag3)
						{
							reverseEngineeringSifrah = new ReverseEngineeringSifrah(Object, complexity, difficulty, reverseEngineerRating, tinkerData);
							reverseEngineeringSifrah.Play(Object);
							if (reverseEngineeringSifrah.Succeeded)
							{
								flag7 = true;
								if (list != null)
								{
									if (reverseEngineeringSifrah.Mods > 0)
									{
										if (list.Count > reverseEngineeringSifrah.Mods)
										{
											List<TinkerData> list2 = new List<TinkerData>();
											for (int j = 0; j < reverseEngineeringSifrah.Mods; j++)
											{
												list2.Add(list[j]);
											}
											list = list2;
										}
									}
									else
									{
										list = null;
									}
								}
								if (reverseEngineeringSifrah.Critical)
								{
									flag6 = false;
									flag8 = true;
								}
								num3 = reverseEngineeringSifrah.XP;
							}
							else
							{
								tinkerData = null;
								list = null;
								if (reverseEngineeringSifrah.Critical)
								{
									Abort = true;
									BitsDone = "";
								}
							}
						}
						else
						{
							tinkerData = null;
							list = null;
						}
					}
					else if (!chance.in100())
					{
						tinkerData = null;
						list = null;
					}
					if (tinkerData != null || (list != null && list.Count > 0))
					{
						bool flag9 = false;
						string text2 = null;
						if (tinkerData != null)
						{
							gameObject = GameObject.CreateSample(tinkerData.Blueprint);
							tinkerData.DisplayName = gameObject.DisplayNameOnlyDirect;
							text2 = "build " + (gameObject.IsPlural ? gameObject.DisplayNameOnlyDirect : gameObject.GetPluralName(AsIfKnown: true, NoConfusion: true, Stripped: false, BaseOnly: true));
							flag9 = true;
						}
						if (list != null)
						{
							List<string> list3 = new List<string>();
							foreach (TinkerData item in list)
							{
								list3.Add(item.DisplayName);
							}
							string text3 = "mod items with the " + Grammar.MakeAndList(list3) + " " + ((list3.Count == 1) ? "mod" : "mods");
							text2 = ((text2 != null) ? (text2 + " and " + text3) : text3);
						}
						if (text2 != null)
						{
							string text4 = "{{G|Eureka! You may now " + text2;
							text4 = ((!flag6) ? (text4 + "... and were able to work out how without needing to destroy " + ((!flag9) ? Object.t() : (Object.IsPlural ? "these" : "this one")) + "!") : (text4 + "."));
							text4 += "}}";
							if (ReverseEngineeringMessage.IsNullOrEmpty())
							{
								ReverseEngineeringMessage = text4;
							}
							else
							{
								ReverseEngineeringMessage = ReverseEngineeringMessage + "\n\n" + text4;
							}
						}
						if (tinkerData != null)
						{
							TinkerData.KnownRecipes.Add(tinkerData);
						}
						if (list != null)
						{
							TinkerData.KnownRecipes.AddRange(list);
						}
					}
					else if (flag7)
					{
						string text5 = "You are unable to make further progress reverse engineering " + Object.poss("modding") + ".";
						if (ReverseEngineeringMessage.IsNullOrEmpty())
						{
							ReverseEngineeringMessage = text5;
						}
						else
						{
							ReverseEngineeringMessage = ReverseEngineeringMessage + "\n\n" + text5;
						}
					}
					if (num3 > 0)
					{
						player.AwardXP(num3, -1, 0, int.MaxValue, null, Object);
					}
					if (flag8)
					{
						TinkeringSifrah.AwardInsight();
					}
				}
				NumberDone++;
				TotalNumberDone++;
				if (TotalNumberWanted > 1)
				{
					Loading.SetLoadingStatus("Disassembled " + TotalNumberDone.Things("item") + " of " + TotalNumberWanted + "...");
				}
				if (!Abort)
				{
					if (player.HasRegisteredEvent("ModifyBitsReceived"))
					{
						Event obj = Event.New("ModifyBitsReceived", "Item", Object, "Bits", text);
						player.FireEvent(obj);
						text = obj.GetStringParameter("Bits", "");
					}
					BitsDone += text;
				}
				num += EnergyCostPer;
				DoBitMessage = true;
				Object.PlayWorldOrUISound("Sounds/Misc/sfx_interact_artifact_disassemble");
				if (flag6)
				{
					if (Alarms != null)
					{
						foreach (Action<GameObject> alarm in Alarms)
						{
							alarm(player);
						}
						Alarms = null;
					}
					Object.Destroy();
					if (!GameObject.Validate(Object) || Object.IsNowhere())
					{
						flag5 = false;
					}
				}
			}
			if ((NumberDone >= NumberWanted) | flag5)
			{
				ProcessDisassemblingWhat();
				if (!Abort)
				{
					if (!Queue.IsNullOrEmpty())
					{
						Object = Queue[0];
						Queue.RemoveAt(0);
						NumberDone = 0;
						OriginalCount = Object.Count;
						if (QueueNumberWanted == null || !QueueNumberWanted.TryGetValue(Object, out NumberWanted))
						{
							NumberWanted = OriginalCount;
						}
						Alarms = null;
						QueueAlarms?.TryGetValue(Object, out Alarms);
					}
					else
					{
						Abort = true;
					}
				}
			}
		}
		finally
		{
			if (num > 0)
			{
				player.UseEnergy(num, "Skill Tinkering Disassemble");
			}
		}
		return true;
	}

	public override string GetInterruptBecause()
	{
		return InterruptBecause;
	}

	public override bool CanComplete()
	{
		if (!Abort)
		{
			return TotalNumberDone >= TotalNumberWanted;
		}
		return true;
	}

	public override void Interrupt()
	{
		if (TotalNumberWanted > 1)
		{
			Loading.SetLoadingStatus("Interrupted!");
		}
		base.Interrupt();
	}

	public override void Complete()
	{
		if (TotalNumberWanted > 1)
		{
			Loading.SetLoadingStatus("Finished disassembling.");
		}
		base.Complete();
	}

	public override void End()
	{
		GameObject player = The.Player;
		if (TotalNumberDone > 0)
		{
			ProcessDisassemblingWhat();
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("You disassemble ");
			if (DisassembledWhats.IsNullOrEmpty())
			{
				stringBuilder.Append(DisassembledWhat ?? "something");
				if (!DisassembledWhere.IsNullOrEmpty())
				{
					stringBuilder.Append(' ').Append(DisassembledWhere);
				}
			}
			else
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				string text = null;
				foreach (string disassembledWhat in DisassembledWhats)
				{
					DisassembledWhatsWhere.TryGetValue(disassembledWhat, out var value);
					if (value != text && list2.Count > 0)
					{
						string text2 = Grammar.MakeAndList(list2);
						if (!text.IsNullOrEmpty())
						{
							text2 = text2 + " " + text;
						}
						list.Add(text2);
						list2.Clear();
					}
					text = value;
					list2.Add(disassembledWhat);
				}
				string text3 = Grammar.MakeAndList(list2);
				if (!text.IsNullOrEmpty())
				{
					text3 = text3 + " " + text;
				}
				list.Add(text3);
				stringBuilder.Append(Grammar.MakeAndList(list));
			}
			stringBuilder.Append('.');
			if (!ReverseEngineeringMessage.IsNullOrEmpty())
			{
				if (ReverseEngineeringMessage.Contains('\n'))
				{
					stringBuilder.Append("\n\n").Append(ReverseEngineeringMessage).Append("\n\n");
				}
				else
				{
					stringBuilder.Compound(ReverseEngineeringMessage, ' ');
				}
			}
			string text4 = null;
			if (!BitsDone.IsNullOrEmpty())
			{
				player.RequirePart<BitLocker>().AddBits(BitsDone);
				if (DoBitMessage)
				{
					text4 = "You receive tinkering bits <{{|" + BitType.GetDisplayString(BitsDone) + "}}>.";
				}
			}
			else if (WasTemporary)
			{
				text4 = "The parts crumble into dust.";
			}
			if (!text4.IsNullOrEmpty())
			{
				stringBuilder.Compound(text4, ' ');
			}
			if (Auto && ReverseEngineeringMessage.IsNullOrEmpty())
			{
				MessageQueue.AddPlayerMessage(stringBuilder.ToString());
			}
			else
			{
				Popup.Show(stringBuilder.ToString());
			}
		}
		base.End();
	}

	public void Enqueue(GameObject Object, int? NumberWanted = null, bool Auto = false, List<Action<GameObject>> Alarms = null)
	{
		int num = NumberWanted ?? Object.Count;
		if (Queue == null)
		{
			Queue = new List<GameObject>();
		}
		if (Queue.Contains(Object))
		{
			if (QueueNumberWanted == null)
			{
				QueueNumberWanted = new Dictionary<GameObject, int>();
			}
			if (QueueNumberWanted.TryGetValue(Object, out var value))
			{
				QueueNumberWanted[Object] = value + num;
			}
			else
			{
				QueueNumberWanted[Object] = num;
			}
			if (!Alarms.IsNullOrEmpty())
			{
				if (QueueAlarms == null)
				{
					QueueAlarms = new Dictionary<GameObject, List<Action<GameObject>>>();
				}
				if (QueueAlarms.TryGetValue(Object, out var value2))
				{
					value2.AddRange(Alarms);
				}
				else
				{
					QueueAlarms[Object] = Alarms;
				}
			}
		}
		else
		{
			Queue.Add(Object);
			if (QueueNumberWanted == null)
			{
				QueueNumberWanted = new Dictionary<GameObject, int>();
			}
			QueueNumberWanted[Object] = num;
			if (!Alarms.IsNullOrEmpty())
			{
				if (QueueAlarms == null)
				{
					QueueAlarms = new Dictionary<GameObject, List<Action<GameObject>>>();
				}
				QueueAlarms[Object] = Alarms;
			}
		}
		TotalNumberWanted += num;
		if (!Auto && this.Auto)
		{
			this.Auto = false;
		}
	}

	private void ProcessDisassemblingWhat()
	{
		if (DisassemblingWhat.IsNullOrEmpty())
		{
			return;
		}
		string text = DisassemblingWhat;
		if (NumberDone > 1 || NumberWanted > 1)
		{
			text = text + " x" + NumberDone;
		}
		if (DisassembledWhat == null)
		{
			DisassembledWhat = text;
			DisassembledWhere = DisassemblingWhere;
		}
		else
		{
			if (DisassembledWhats.IsNullOrEmpty())
			{
				if (DisassembledWhats == null)
				{
					DisassembledWhats = new List<string>();
				}
				if (DisassembledWhatsWhere == null)
				{
					DisassembledWhatsWhere = new Dictionary<string, string>();
				}
				DisassembledWhats.Add(DisassembledWhat);
				DisassembledWhatsWhere[DisassembledWhat] = DisassembledWhere;
			}
			DisassembledWhats.Add(text);
			DisassembledWhatsWhere[text] = DisassemblingWhere;
		}
		DisassemblingWhat = null;
		DisassemblingWhere = null;
	}
}
