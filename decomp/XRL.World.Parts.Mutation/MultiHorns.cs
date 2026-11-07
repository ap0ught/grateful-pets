using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MultiHorns : BaseMutation
{
	[NonSerialized]
	public List<GameObject> Horns = new List<GameObject>(2);

	public int ExtraHeads = 2;

	public bool bSetHeads;

	public string HornsName;

	public int Charging;

	[NonSerialized]
	private List<Cell> chargeCells;

	public string ManagerID => ParentObject.ID + "::MultiHorns";

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteGameObjectList(Horns);
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Horns = new List<GameObject>();
		Reader.ReadGameObjectList(Horns);
		base.Read(Basis, Reader);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveAbilityListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (Charging <= 0 && E.Distance <= GetChargeDistance(Level) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target))
		{
			E.Add("CommandMassiveCharge");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandMassiveCharge");
		base.Register(Object, Registrar);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Distance", GetChargeDistance(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetChargeCooldown(Level));
	}

	private bool ValidChargeTarget(GameObject obj)
	{
		return obj?.IsCombat ?? false;
	}

	public void PickChargeTarget()
	{
		if (Charging > 0)
		{
			return;
		}
		chargeCells = PickLine(GetChargeDistance(Level), AllowVis.OnlyVisible, ValidChargeTarget, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, BlackoutStops: false, null, null, "Wrecking Charge");
		if (chargeCells == null)
		{
			return;
		}
		if (chargeCells != null)
		{
			chargeCells = new List<Cell>(chargeCells);
		}
		if (chargeCells.FirstOrDefault() != ParentObject.CurrentCell)
		{
			chargeCells.Insert(0, ParentObject.CurrentCell);
		}
		if (chargeCells.Count <= 1 || chargeCells[0].ParentZone != chargeCells[1].ParentZone)
		{
			return;
		}
		while (chargeCells.Count < GetChargeDistance(Level) + 1)
		{
			for (int i = 0; i < chargeCells.Count - 1; i++)
			{
				if (chargeCells.Count >= GetChargeDistance(Level) + 1)
				{
					break;
				}
				if (chargeCells[i].ParentZone != chargeCells[i + 1].ParentZone)
				{
					break;
				}
				string directionFromCell = chargeCells[i].GetDirectionFromCell(chargeCells[i + 1]);
				chargeCells.Add(chargeCells[chargeCells.Count - 1].GetCellFromDirection(directionFromCell, BuiltOnly: false));
			}
		}
	}

	public string pathDirectionAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return ".";
		}
		n %= path.Count;
		return path[n].GetDirectionFromCell(path[(n + 1) % path.Count]);
	}

	public Cell cellAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return path[0];
		}
		if (n < path.Count)
		{
			return path[n];
		}
		Cell cell = path[path.Count - 1];
		for (int i = path.Count; i <= n; i++)
		{
			cell = cell.GetCellFromDirection(pathDirectionAtStep(n, path), BuiltOnly: false);
		}
		return cell;
	}

	public void PerformCharge(List<Cell> ChargePath, bool DoEffect = true)
	{
		Charging = 0;
		Dictionary<GameObject, int> dictionary = new Dictionary<GameObject, int>();
		List<GameObject> list = new List<GameObject>();
		if (!ChargePath.Any((Cell c) => c.IsVisible()))
		{
			DoEffect = false;
		}
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		DidX("charge", null, "!");
		int num = ParentObject.Stat("Strength") + Level * 2 - 2;
		int num2 = num;
		int num3 = (num - 5) * (num - 5);
		bool flag = ParentObject.HasPart<Robot>();
		for (int num4 = 0; num4 < ChargePath.Count; num4++)
		{
			int num5 = num4 + 1 + list.Count;
			while (num5 != -1)
			{
				int num6 = num5;
				num5 = -1;
				Cell cell = cellAtStep(num6, ChargePath);
				if (flag)
				{
					GameObject firstObjectWithPropertyOrTag = cell.GetFirstObjectWithPropertyOrTag("RobotStop");
					if (firstObjectWithPropertyOrTag != null)
					{
						DidXToY("are", "stopped in " + ParentObject.its + " tracks by", firstObjectWithPropertyOrTag, null, "!", null, null, null, ParentObject);
						goto end_IL_04a6;
					}
				}
				foreach (GameObject item in from go in cell.GetWalls()
					where ParentObject.PhaseAndFlightMatches(go)
					select go)
				{
					foreach (GameObject item2 in list)
					{
						if (!dictionary.ContainsKey(item2))
						{
							dictionary.Add(item2, 0);
						}
						dictionary[item2]++;
					}
					if (IComponent<GameObject>.Visible(item))
					{
						CombatJuice._cameraShake(0.25f);
					}
					if (num2 > Stats.GetCombatAV(item) + 20)
					{
						item.Destroy();
						continue;
					}
					DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item, null, "!", null, null, null, ParentObject);
					goto end_IL_04a6;
				}
				foreach (GameObject item3 in from go in cell.LoopObjectsWithPart("Combat")
					where ParentObject.PhaseAndFlightMatches(go)
					select go)
				{
					if (!list.Contains(item3) && item3 != ParentObject)
					{
						if (num <= item3.Stat("Strength") || !item3.CanBeInvoluntarilyMoved())
						{
							DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item3, null, "!", null, null, null, ParentObject);
							goto end_IL_04a6;
						}
						list.Add(item3);
						num5 = num6 + 1;
					}
					if (item3.IsPlayer())
					{
						CombatJuice._cameraShake(0.25f);
					}
				}
				foreach (GameObject item4 in from go in cell.LoopObjectsWithPart("Physics")
					where ParentObject.PhaseAndFlightMatches(go)
					select go)
				{
					if (item4.Physics.Solid && !item4.IsWall() && !item4.IsCombat && !list.Contains(item4) && item4 != ParentObject)
					{
						if (num3 <= item4.Weight)
						{
							DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item4, null, "!", null, null, null, ParentObject);
							goto end_IL_04a6;
						}
						list.Add(item4);
						num5 = num6 + 1;
					}
				}
			}
			list.RemoveAll((GameObject O) => O.IsInvalid() || O.IsNowhere());
			for (int num7 = list.Count - 1; num7 >= 0; num7--)
			{
				list[num7].DirectMoveTo(cellAtStep(num4 + num7 + 1, ChargePath));
			}
			ParentObject.DirectMoveTo(cellAtStep(num4, ChargePath));
			scrapBuffer.RenderBase();
			scrapBuffer.Draw();
			Thread.Sleep(10);
			continue;
			end_IL_04a6:
			break;
		}
		foreach (GameObject item5 in list)
		{
			if (item5 == ParentObject)
			{
				continue;
			}
			int num8 = 0;
			if (dictionary.ContainsKey(item5))
			{
				num8 = dictionary[item5];
			}
			int num9 = ExtraHeads + 1 + (ExtraHeads + 1) * Math.Min(3, num8);
			string damageIncrement = GetDamageIncrement(Level);
			int num10 = damageIncrement.RollCached();
			for (int num11 = 0; num11 < num9; num11++)
			{
				num10 += damageIncrement.RollCached();
			}
			if (num10 > 0)
			{
				string message = "from %t charge!";
				if (num8 == 1)
				{
					message = "from being slammed into a wall by %t charge!";
				}
				else if (num8 == 2)
				{
					message = "from being slammed into {{W|2}} walls by %t charge!";
				}
				else if (num8 >= 3)
				{
					message = "from being slammed into {{W|" + num8 + "}} walls by %t charge!";
				}
				int amount = num10;
				GameObject parentObject = ParentObject;
				item5.TakeDamage(amount, message, null, null, null, null, parentObject);
			}
			else
			{
				IComponent<GameObject>.XDidY(item5, "are", "shoved by " + Grammar.MakePossessive(ParentObject.the + ParentObject.DisplayNameOnly) + " charge!", null, null, null, null, item5);
			}
		}
		ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_charge_mega");
		CooldownMyActivatedAbility(ActivatedAbilityID, GetChargeCooldown(Level));
		ChargePath?.Clear();
	}

	public int GetChargeCooldown(int Level)
	{
		return Math.Max(42 - Level * 3, 5);
	}

	public int GetTurnsToCharge()
	{
		return 1;
	}

	public override bool Render(RenderEvent E)
	{
		if (chargeCells != null && chargeCells.Count > 0 && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = "&r^R";
			}
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (chargeCells != null)
		{
			int num = 1000;
			int val = num / Math.Max(chargeCells.Count, 1);
			int num2 = (int)(IComponent<GameObject>.frameTimerMS % num / Math.Max(val, 1));
			if (num2 > 0 && num2 < chargeCells.Count && chargeCells[num2].ParentZone == ParentObject.Physics.CurrentCell.ParentZone && chargeCells[num2].IsVisible())
			{
				buffer.Goto(chargeCells[num2].X, chargeCells[num2].Y);
				buffer.Write(ParentObject.Render.RenderString);
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].Tile = ParentObject.Render.Tile;
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].TileForeground = The.Color.DarkRed;
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].Detail = The.Color.Red;
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].SetForeground('r');
			}
			base.OnPaint(buffer);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Charging > 0)
			{
				if (!ParentObject.CanChangeMovementMode("Charging", ShowMessage: true))
				{
					Charging = 0;
				}
				else
				{
					if (chargeCells.IsNullOrEmpty())
					{
						PickChargeTarget();
					}
					if (chargeCells.IsNullOrEmpty())
					{
						Charging = 0;
					}
					else
					{
						Charging--;
						ParentObject.UseEnergy(1000, "Physical Mutation Massive Charge");
						if (Charging > 0)
						{
							if (ParentObject.IsPlayer())
							{
								The.Core.RenderDelay(500);
							}
							return false;
						}
						PerformCharge(chargeCells);
					}
				}
			}
		}
		else if (E.ID == "CommandMassiveCharge")
		{
			if (ParentObject.CurrentCell == null || ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You can't do that here.");
			}
			if (!ParentObject.CanChangeMovementMode("Charging", ShowMessage: true))
			{
				return false;
			}
			PickChargeTarget();
			if (chargeCells == null || chargeCells.Count <= 0)
			{
				return false;
			}
			Charging = GetTurnsToCharge();
			ParentObject.UseEnergy(1000, "Physical Mutation Massive Charge");
			PlayWorldSound("sfx_ability_longBeam_attack_chargeUp");
			DidX("stomp", "with bestial fury", "!", null, null, ParentObject);
			if (Visible() && AutoAct.IsInterruptable())
			{
				AutoAct.Interrupt(null, null, ParentObject, IsThreat: true);
			}
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "Several horns jut out of your head.";
	}

	public int GetChargeDistance(int Level)
	{
		return 8 + Level;
	}

	public string GetDamageIncrement(int Level)
	{
		return "2d" + (Level / 2 + 3);
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		int value = 0;
		if (Level == 1)
		{
			text = "2d3";
			value = 0;
		}
		if (Level == 2)
		{
			text = "2d4";
			value = 0;
		}
		if (Level == 3)
		{
			text = "2d4";
			value = 1;
		}
		if (Level == 4)
		{
			text = "2d5";
			value = 1;
		}
		if (Level == 5)
		{
			text = "2d5";
			value = 1;
		}
		if (Level == 6)
		{
			text = "2d6";
			value = 1;
		}
		if (Level == 7)
		{
			text = "2d6";
			value = 2;
		}
		if (Level == 8)
		{
			text = "2d7";
			value = 2;
		}
		if (Level == 9)
		{
			text = "2d7";
			value = 2;
		}
		if (Level >= 10)
		{
			text = "2d8";
			value = 2;
		}
		string text2 = "20% chance on melee attack to gore your opponent\n";
		text2 = text2 + "Damage increment: " + text + "\n";
		text2 = ((Level != base.Level) ? (text2 + "{{rules|Increased bleeding save difficulty and intensity}}\n") : (text2 + "Goring attacks may cause bleeding\n"));
		text2 = text2 + value.Signed() + " AV\n";
		text2 += "Cannot wear helmets\n";
		text2 += "Can launch into a destructive charge after a one round warm-up.\n";
		return text2 + "Charge distance: " + GetChargeDistance(Level);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		Body body = ParentObject.Body;
		BodyPart body2 = body.GetBody();
		if (!bSetHeads)
		{
			for (int i = 0; i < ExtraHeads; i++)
			{
				body2.AddPartAt("Head", 0, null, null, null, null, Category: body2.Category, Manager: ManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Dynamic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Head", OrInsertBefore: "Back").AddPart("Face", 0, null, null, null, null, Category: body2.Category, Manager: ManagerID);
			}
		}
		bSetHeads = true;
		foreach (GameObject horn in Horns)
		{
			horn.Destroy();
		}
		foreach (BodyPart item in body.GetPart("Head"))
		{
			item.ForceUnequip(Silent: true);
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("Horns Single");
			MeleeWeapon part = gameObject.GetPart<MeleeWeapon>();
			Armor part2 = gameObject.GetPart<Armor>();
			part.HitBonus = Math.Max(NewLevel - 4, 0);
			if (string.IsNullOrEmpty(HornsName))
			{
				HornsName = GetDisplayName(WithAnnotations: false).ToLower();
			}
			gameObject.Render.DisplayName = HornsName;
			part.Skill = "Cudgel";
			part.MaxStrengthBonus = 10;
			if (Level == 1)
			{
				part.BaseDamage = "2d3";
				part2.AV = 0;
			}
			if (Level == 2)
			{
				part.BaseDamage = "2d4";
				part2.AV = 0;
			}
			if (Level == 3)
			{
				part.BaseDamage = "2d5";
				part2.AV = 1;
			}
			if (Level == 4)
			{
				part.BaseDamage = "2d5";
				part2.AV = 1;
			}
			if (Level == 5)
			{
				part.BaseDamage = "2d5";
				part2.AV = 1;
			}
			if (Level == 6)
			{
				part.BaseDamage = "2d6";
				part2.AV = 1;
			}
			if (Level == 7)
			{
				part.BaseDamage = "2d6";
				part2.AV = 2;
			}
			if (Level == 8)
			{
				part.BaseDamage = "2d7";
				part2.AV = 2;
			}
			if (Level == 9)
			{
				part.BaseDamage = "2d7";
				part2.AV = 2;
			}
			if (Level >= 10)
			{
				part.BaseDamage = "2d8";
				part2.AV = 2;
			}
			ParentObject.ForceEquipObject(gameObject, item, Silent: true, 0);
			Horns.Add(gameObject);
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (string.IsNullOrEmpty(HornsName))
		{
			if (GO.HasTagOrProperty("HasHorns"))
			{
				SetDisplayName("Triple Horn");
				HornsName = "horn";
			}
			else if (GO.HasTagOrProperty("HasLittleHorns"))
			{
				SetDisplayName("Triple Horn");
				HornsName = "little horn";
			}
			else if (GO.Blueprint.Contains("Goat"))
			{
				SetDisplayName("Horns");
				HornsName = "horns";
			}
			else if (GO.Blueprint.Contains("Rhino"))
			{
				SetDisplayName("Horn");
				HornsName = "horn";
			}
			else
			{
				int num = Stat.Random(1, 100);
				if (num <= 35)
				{
					SetDisplayName("Horns");
					HornsName = "horns";
				}
				else if (num <= 65)
				{
					SetDisplayName("Antlers");
					HornsName = "antlers";
				}
				else
				{
					SetDisplayName("Horn");
					HornsName = "horn";
				}
			}
		}
		ActivatedAbilityID = AddMyActivatedAbility("Wrecking Charge", "CommandMassiveCharge", "Physical Mutations", null, "\u00af");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		if (Horns.Count > 0)
		{
			foreach (GameObject horn in Horns)
			{
				CleanUpMutationEquipment(GO, horn);
			}
			Horns.Clear();
		}
		GO.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		return base.Unmutate(GO);
	}
}
