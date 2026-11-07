using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Quills : BaseMutation
{
	public const int MINIMUM_QUILLS_TO_FLING = 80;

	public static readonly string COMMAND_NAME = "CommandQuillFling";

	public GameObject QuillsObject;

	public int oldLevel = 1;

	public int nMaxQuills = 300;

	public int _nQuills;

	public int nPenalty;

	public Guid QuillFlingActivatedAbilityID = Guid.Empty;

	public float QuillRegenerationCounter;

	[NonSerialized]
	protected GameObjectBlueprint _Blueprint;

	public int nQuills
	{
		get
		{
			return _nQuills;
		}
		set
		{
			if (value > nMaxQuills)
			{
				value = nMaxQuills;
			}
			if (_nQuills == value)
			{
				return;
			}
			_nQuills = value;
			if (_nQuills >= nMaxQuills / 2)
			{
				if (nPenalty > 0)
				{
					StatShifter.RemoveStatShift(ParentObject, "AV");
					nPenalty = 0;
				}
			}
			else if (nPenalty == 0)
			{
				nPenalty = GetAVPenalty(Level);
				StatShifter.SetStatShift(ParentObject, "AV", -nPenalty);
			}
			StringBuilder sB = Event.NewStringBuilder(AbilityName).Append(" [").Append(nQuills)
				.Append(' ')
				.Append((nQuills == 1) ? ObjectNameSingular : ObjectName)
				.Append(" left]");
			SetMyActivatedAbilityDisplayName(QuillFlingActivatedAbilityID, Event.FinalizeString(sB));
		}
	}

	public string BlueprintName => Variant.Coalesce("Quills");

	public string AbilityName
	{
		get
		{
			if (!Blueprint.Tags.TryGetValue("AbilityName", out var value))
			{
				return "Quill Fling";
			}
			return value;
		}
	}

	public string ObjectName => Blueprint.CachedDisplayNameStripped ?? "quills";

	public string ObjectNameSingular
	{
		get
		{
			if (!Blueprint.Tags.TryGetValue("DisplayNameSingular", out var value))
			{
				return "quill";
			}
			return value;
		}
	}

	public GameObjectBlueprint Blueprint
	{
		get
		{
			if (_Blueprint == null)
			{
				_Blueprint = GameObjectFactory.Factory.GetBlueprint(BlueprintName);
			}
			return _Blueprint;
		}
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Quills", ObjectName);
		stats.Set("RegenRate", GetRegenRate(Level).ToString());
		stats.Set("AVPenalty", GetAVPenalty(Level));
		stats.Set("QuillPen", Grammar.InitCap(ObjectNameSingular) + " penetration: " + GetQuillPenetration(Level));
		stats.Set("QuillDamage", Grammar.InitCap(ObjectNameSingular) + " damage: 1d3");
	}

	public float GetRegenRate(int Level)
	{
		return (float)Level / 4f;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != BeforeApplyDamageEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != TookDamageEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(QuillFlingActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (nQuills >= 80 && (double)nQuills > (double)nMaxQuills * 0.65 && (E.Distance <= 1 || ParentObject.HasEffect<Engulfed>()) && IsMyActivatedAbilityAIUsable(QuillFlingActivatedAbilityID) && E.Target != null && E.Target.IsCombatObject())
		{
			int num = 1;
			int num2 = 1;
			if (!ParentObject.HasEffect<Engulfed>())
			{
				foreach (Cell adjacentCell in E.Actor.CurrentCell.GetAdjacentCells())
				{
					GameObject combatTarget = adjacentCell.GetCombatTarget(E.Actor, IgnoreFlight: true);
					if (combatTarget == null || combatTarget == E.Target)
					{
						continue;
					}
					if (combatTarget.Brain != null)
					{
						if (E.Actor.IsHostileTowards(combatTarget))
						{
							num++;
							continue;
						}
						num2++;
						if (combatTarget.isDamaged(0.1))
						{
							num2++;
						}
					}
					else if (!OkayToDamageEvent.Check(combatTarget, E.Actor))
					{
						num2++;
					}
				}
			}
			if ((25 * num / num2).in100())
			{
				E.Add(COMMAND_NAME);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		Damage damage = E.Damage;
		if (damage != null && damage.HasAttribute("Quills"))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Actor != null && E.Actor != ParentObject && !ParentObject.OnWorldMap() && !E.Actor.HasPart<Quills>() && E.Damage.Amount > 0 && !E.Damage.HasAttribute("reflected") && E.Damage.HasAttribute("Unarmed"))
		{
			int num = (int)((double)nQuills * 0.01) + Stat.Random(1, 2) - 1;
			nQuills -= num;
			if (num > 0)
			{
				int num2 = (int)((float)E.Damage.Amount * ((float)(num * 3) / 100f));
				if (num2 == 0)
				{
					num2 = 1;
				}
				if (num2 > 0)
				{
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("impale") + " " + E.Actor.itself + " on " + ParentObject.poss(QuillsObject) + " and" + E.Actor.GetVerb("take") + " " + num2 + " damage!", 'G');
					}
					else if (E.Actor != null)
					{
						if (E.Actor.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You impale " + E.Actor.itself + " on " + ParentObject.poss(QuillsObject) + " and take " + num2 + " damage!", 'R');
						}
						else if (IComponent<GameObject>.Visible(E.Actor))
						{
							if (E.Actor.IsPlayerLed())
							{
								IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("impale") + " " + E.Actor.itself + " on " + ParentObject.poss(QuillsObject) + " and" + E.Actor.GetVerb("take") + " " + num2 + " damage!", 'r');
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("impale") + " " + E.Actor.itself + " on " + ParentObject.poss(QuillsObject) + " and" + E.Actor.GetVerb("take") + " " + num2 + " damage!", 'g');
							}
						}
					}
					Event obj = new Event("TakeDamage");
					Damage damage = new Damage(num2);
					damage.Attributes = new List<string>(E.Damage.Attributes);
					if (!damage.HasAttribute("reflected"))
					{
						damage.Attributes.Add("reflected");
					}
					obj.SetParameter("Damage", damage);
					obj.SetParameter("Owner", ParentObject);
					obj.SetParameter("Attacker", ParentObject);
					obj.SetParameter("Message", null);
					E.Actor.FireEvent(obj);
					ParentObject.FireEvent("ReflectedDamage");
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		float num = Level;
		float num2 = ((float)ParentObject.Stat("Willpower") - 16f) * 0.05f;
		float num3 = 1f - num2;
		if ((double)num3 <= 0.2)
		{
			num3 = 0.2f;
		}
		if (num2 < 1f)
		{
			num *= 1f / num3;
		}
		QuillRegenerationCounter += num;
		if (QuillRegenerationCounter >= 4f)
		{
			int num4 = (int)(QuillRegenerationCounter / 4f);
			nQuills += num4;
			QuillRegenerationCounter -= 4 * num4;
		}
		if (nQuills > nMaxQuills)
		{
			nQuills = nMaxQuills;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (!ParentObject.CheckFrozen())
			{
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			string objectName = ObjectName;
			if (nQuills < 80)
			{
				return ParentObject.Fail($"You don't have enough {objectName}! You need at least {80} {objectName} to {AbilityName}.");
			}
			GameObject gameObject = null;
			Engulfed effect = ParentObject.GetEffect((Engulfed sfx) => sfx.IsEngulfedByValid());
			ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_quills_expel");
			if (effect != null)
			{
				int num = (int)((double)nQuills * 0.1);
				if (num <= 0)
				{
					return false;
				}
				gameObject = effect.EngulfedBy;
				DidX("fling", ParentObject.its + " " + objectName, "!", null, null, ParentObject);
				QuillFling(gameObject.CurrentCell, num, UseQuills: true, Reactive: false, gameObject);
			}
			else
			{
				List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
				if (adjacentCells.Count <= 0)
				{
					return false;
				}
				int num2 = (int)((double)nQuills * 0.1) / adjacentCells.Count;
				if (num2 <= 0)
				{
					return false;
				}
				DidX("fling", ParentObject.its + " " + objectName + " everywhere", "!", null, null, ParentObject);
				foreach (Cell item in adjacentCells)
				{
					QuillFling(item, num2);
				}
			}
			UseEnergy(1000, "Physical Mutation Quills");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DefenderHit");
		base.Register(Object, Registrar);
	}

	public void QuillFling(Cell Cell, int Quills, bool UseQuills = true, bool Reactive = false, GameObject Target = null)
	{
		if (Cell == null || Cell.OnWorldMap())
		{
			return;
		}
		if (UseQuills)
		{
			if (Quills > nQuills)
			{
				return;
			}
			nQuills -= Quills;
			if (nQuills < 0)
			{
				nQuills = 0;
			}
		}
		bool flag = Cell.IsVisible();
		if (Target == null)
		{
			Target = Cell.GetCombatTarget(ParentObject, IgnoreFlight: true);
			if (Target == null)
			{
				return;
			}
		}
		int num = 0;
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		if (flag)
		{
			The.Core.RenderMapToBuffer(scrapBuffer);
		}
		int num2 = Math.Min((Level - 1) / 2, 6);
		for (int i = 0; i < Quills; i++)
		{
			int num3 = Stat.RollDamagePenetrations(Stats.GetCombatAV(Target), num2, num2);
			if (num3 <= 0)
			{
				continue;
			}
			if (flag)
			{
				scrapBuffer.Goto(Cell.X, Cell.Y);
				switch (Stat.Random(1, 4))
				{
				case 1:
					scrapBuffer.Write("&Y\\");
					break;
				case 2:
					scrapBuffer.Write("&Y-");
					break;
				case 3:
					scrapBuffer.Write("&Y/");
					break;
				case 4:
					scrapBuffer.Write("&Y|");
					break;
				}
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(10);
			}
			for (int j = 0; j < num3; j++)
			{
				num += Stat.Random(1, 3);
			}
		}
		Target.TakeDamage(num, Accidental: Reactive, Attacker: ParentObject, Message: "from %t " + ObjectName + "!", Attributes: "Stabbing Quills");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefenderHit" && 5.in100())
		{
			int num = Stat.Random(1, 4);
			if (num > nQuills)
			{
				num = nQuills;
			}
			if (num > 0)
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("The attack breaks " + Grammar.Cardinal(num) + " " + ((num == 1) ? ObjectNameSingular : ObjectName) + "!");
				}
				nQuills -= num;
			}
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return Blueprint.GetTag("VariantDescription").Coalesce("Hundreds of needle-pointed quills cover your body.");
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		_Blueprint = null;
	}

	public int GetAV(int Level)
	{
		if (Level <= 2)
		{
			return 2;
		}
		return Level / 3 + 2;
	}

	public int GetAVPenalty(int Level)
	{
		return GetAV(Level) / 2;
	}

	public int GetQuillPenetration(int Level)
	{
		return Math.Min(6, (Level - 1) / 2);
	}

	public override string GetLevelText(int Level)
	{
		string value = GetQuillPenetration(Level).ToString();
		int aVPenalty = GetAVPenalty(Level);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string objectName = ObjectName;
		if (Level == base.Level)
		{
			stringBuilder.Append("{{rules|").Append(nMaxQuills).Append("}} ")
				.Append(objectName)
				.Append('\n');
		}
		else
		{
			stringBuilder.Append("+{{rules|80-120}} ").Append(objectName).Append('\n');
		}
		stringBuilder.Append("May expel 10% of your ").Append(objectName).Append(" in a burst around yourself ({{c|\u001a}}{{rules|")
			.Append(value)
			.Append("}} {{r|\u0003}}1d3)\n")
			.Append("Regenerate ")
			.Append(objectName)
			.Append(" at the approximate rate of {{rules|")
			.Append((float)Level / 4f)
			.Append("}} per round\n")
			.Append("+{{rules|")
			.Append(GetAV(Level))
			.Append("}} AV as long as you retain half your ")
			.Append(objectName)
			.Append(" (+{{rules|")
			.Append(GetAV(Level) - aVPenalty)
			.Append("}} AV otherwise)\n")
			.Append("Creatures attacking you in melee may impale themselves on your ")
			.Append(objectName)
			.Append(", breaking roughly 1% of them and reflecting 3% damage per ")
			.Append(ObjectNameSingular)
			.Append(" broken.\n")
			.Append("Cannot wear body armor\n")
			.Append("Immune to other creatures' ")
			.Append(objectName);
		return Event.FinalizeString(stringBuilder);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		if (NewLevel != oldLevel)
		{
			int num = (NewLevel - oldLevel) * Stat.Random(80, 120);
			nMaxQuills = Math.Max(300, nMaxQuills + num);
			oldLevel = NewLevel;
		}
		nQuills = nMaxQuills;
		if (QuillsObject != null)
		{
			QuillsObject.GetPart<Armor>().AV = GetAV(Level);
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		BodyPart bodyPart = GO.Body?.GetBody();
		if (bodyPart != null)
		{
			bodyPart.ForceUnequip(Silent: true);
			QuillsObject = GameObject.Create(Blueprint);
			GO.ForceEquipObject(QuillsObject, bodyPart, Silent: true, 0);
			QuillFlingActivatedAbilityID = AddMyActivatedAbility(AbilityName, COMMAND_NAME, "Physical Mutations", null, "*");
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref QuillFlingActivatedAbilityID);
		CleanUpMutationEquipment(GO, ref QuillsObject);
		StatShifter.RemoveStatShift(GO, "AV");
		nPenalty = 0;
		return base.Unmutate(GO);
	}
}
