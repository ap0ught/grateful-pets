using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LightManipulation : BaseMutation
{
	public const int RANGE = 999;

	public const int COUNT = 1000;

	public const int BASE_RADIUS_REGROWTH_TURNS = 15;

	public const int WILLPOWER_BASELINE = 16;

	public const int WILLPOWER_FACTOR = 5;

	public const int WILLPOWER_CEILING_FACTOR = 5;

	public const int WILLPOWER_FLOOR_DIVISOR = 5;

	public Guid LaseActivatedAbilityID = Guid.Empty;

	public Guid LightActivatedAbilityID = Guid.Empty;

	public int RadiusPenalty;

	public int RadiusRegrowthTimer;

	[NonSerialized]
	private static GameObject _Projectile;

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified("ProjectileLightManipulation");
			}
			return _Projectile;
		}
	}

	public int MaxLightRadius => GetMaxLightRadius(Level);

	public LightManipulation()
	{
		Type = "Mental";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Radius", GetMaxLightRadius(Level), !stats.mode.Contains("ability"));
		stats.Set("RechargeRate", "1 per " + GetRadiusRegrowthTurns() + " rounds", !stats.mode.Contains("ability"));
		stats.Set("LaserDamageInc", GetDamage(Level), !stats.mode.Contains("ability"));
		stats.Set("LaserPen", GetLasePenetrationBonus(Level) + RuleSettings.VISUAL_PENETRATION_BONUS, !stats.mode.Contains("ability"));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != BeforeRenderEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(LaseActivatedAbilityID, CollectStats);
		DescribeMyActivatedAbility(LightActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 999 && RadiusPenalty < MaxLightRadius && IsMyActivatedAbilityAIUsable(LaseActivatedAbilityID) && GameObject.Validate(E.Target) && !E.Target.IsInStasis() && ParentObject.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add("CommandLase", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (RadiusPenalty < MaxLightRadius && IsMyActivatedAbilityToggledOn(LightActivatedAbilityID))
		{
			AddLight(MaxLightRadius - RadiusPenalty);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandLase")
		{
			if (!IsMyActivatedAbilityUsable(LaseActivatedAbilityID))
			{
				return false;
			}
			if (ParentObject.OnWorldMap())
			{
				ParentObject.Fail("You cannot do that on the world map.");
				return false;
			}
			if (RadiusPenalty >= MaxLightRadius)
			{
				ParentObject.Fail("Your capacity is too weak.");
				return false;
			}
			List<Cell> list = PickLine(999, AllowVis.Any, (GameObject o) => o.IsCombat && o.PhaseMatches(ParentObject), IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, BlackoutStops: false, ParentObject, Projectile, "Lase", Snap: true);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			if (list.Count > 1000)
			{
				list.RemoveRange(1000, list.Count - 1000);
			}
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_lightManipulation_laser_fire");
			UseEnergy(1000, "Mental Mutation LightManipulation Lase");
			Cell cell = list[0];
			Cell cell2 = list.Last();
			float num = (float)Math.Atan2(cell2.X - cell.X, cell2.Y - cell.Y).toDegrees();
			list.RemoveAt(0);
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				Cell cell3 = list[num2];
				if (cell3.IsBlackedOut())
				{
					if (cell3.IsVisible() || (num2 > 0 && list[num2 - 1].IsVisible()))
					{
						IComponent<GameObject>.AddPlayerMessage("The darkness absorbs the laser beam.");
					}
					list.RemoveRange(num2, list.Count - num2);
				}
				else
				{
					if (!cell3.HasObjectWithRegisteredEvent("RefractLight") && !cell3.HasObjectWithRegisteredEvent("ReflectProjectile"))
					{
						continue;
					}
					bool flag = true;
					GameObject Object = null;
					string clip = null;
					string verb = "refract";
					int num3 = -1;
					if (cell3.HasObjectWithRegisteredEvent("RefractLight"))
					{
						Event obj = Event.New("RefractLight");
						obj.SetParameter("Projectile", (object)null);
						obj.SetParameter("Attacker", ParentObject);
						obj.SetParameter("Cell", cell3);
						obj.SetParameter("Angle", num);
						obj.SetParameter("Direction", Stat.Random(0, 359));
						obj.SetParameter("Verb", null);
						obj.SetParameter("Sound", "sfx_light_refract");
						obj.SetParameter("By", (object)null);
						flag = cell3.FireEvent(obj);
						if (!flag)
						{
							Object = obj.GetGameObjectParameter("By");
							clip = obj.GetStringParameter("Sound");
							verb = obj.GetStringParameter("Verb") ?? "refract";
							num3 = obj.GetIntParameter("Direction").normalizeDegrees();
						}
					}
					if (flag && cell3.HasObjectWithRegisteredEvent("ReflectProjectile"))
					{
						Event obj2 = Event.New("ReflectProjectile");
						obj2.SetParameter("Projectile", (object)null);
						obj2.SetParameter("Attacker", ParentObject);
						obj2.SetParameter("Cell", cell3);
						obj2.SetParameter("Angle", num);
						obj2.SetParameter("Direction", Stat.Random(0, 359));
						obj2.SetParameter("Verb", null);
						obj2.SetParameter("Sound", "sfx_light_refract");
						obj2.SetParameter("By", (object)null);
						flag = cell3.FireEvent(obj2);
						if (!flag)
						{
							Object = obj2.GetGameObjectParameter("By");
							clip = obj2.GetStringParameter("Sound");
							verb = obj2.GetStringParameter("Verb") ?? "refract";
							num3 = obj2.GetIntParameter("Direction").normalizeDegrees();
						}
					}
					if (flag || !GameObject.Validate(ref Object))
					{
						continue;
					}
					PlayWorldSound(clip, 0.5f, 0f, Combat: true);
					IComponent<GameObject>.XDidY(Object, verb, "the laser beam", "!", null, null, Object);
					float num4 = cell3.X;
					float num5 = cell3.Y;
					float num6 = (float)Math.Sin((float)num3 * (MathF.PI / 180f));
					float num7 = (float)Math.Cos((float)num3 * (MathF.PI / 180f));
					list.RemoveRange(num2, list.Count - num2);
					Cell cell4 = cell3;
					do
					{
						num4 += num6;
						num5 += num7;
						Cell cell5 = cell3.ParentZone.GetCell((int)num4, (int)num5);
						if (cell5 == null)
						{
							break;
						}
						if (cell5 != cell4)
						{
							list.Add(cell5);
							cell4 = cell5;
							if (cell5.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true) != null || cell5.HasSolidObjectForMissile(ParentObject, null, Projectile))
							{
								break;
							}
						}
					}
					while (num4 > 0f && num4 < 79f && num5 > 0f && num5 < 24f && list.Count < 400);
				}
			}
			int num8 = 0;
			for (int count = list.Count; num8 < count && !Lase(list[num8], count); num8++)
			{
			}
			RadiusPenalty++;
			SyncAbilityName();
			if (RadiusPenalty <= MaxLightRadius)
			{
			}
		}
		else if (E.Command == "CommandAmbientLight")
		{
			if (IsMyActivatedAbilityToggledOn(LightActivatedAbilityID))
			{
				PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_lightManipulation_deactivate");
				ToggleMyActivatedAbility(LightActivatedAbilityID);
			}
			else
			{
				if (IsMyActivatedAbilityCoolingDown(LaseActivatedAbilityID))
				{
					if (ParentObject.IsPlayer())
					{
						if (Options.AbilityCooldownWarningAsMessage)
						{
							MessageQueue.AddPlayerMessage("You must wait {{C|" + GetMyActivatedAbilityCooldownDescription(LaseActivatedAbilityID) + "}} before you can enable ambient light.");
						}
						else
						{
							Popup.ShowFail("You must wait {{C|" + GetMyActivatedAbilityCooldownDescription(LaseActivatedAbilityID) + "}} before you can enable ambient light.");
						}
					}
					return false;
				}
				PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_lightManipulation_activate");
				ToggleMyActivatedAbility(LightActivatedAbilityID);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("stars", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterMassMind");
		Registrar.Register("EndTurn");
		Registrar.Register("RefractLight");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You manipulate light to your advantage.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + "You produce ambient light within a radius of {{rules|" + GetMaxLightRadius(Level) + "}}.\n", "You may focus the light into a laser beam, temporarily reducing the radius of your ambient light by 1.\n"), "Laser damage increment: {{rules|", GetDamage(Level), "}}\n"), "Laser penetration: {{rules|", (GetLasePenetrationBonus(Level) + RuleSettings.VISUAL_PENETRATION_BONUS).ToString(), "}}\n"), "Ambient light recharges at a rate of 1 unit every ", GetRadiusRegrowthTurns().ToString(), " rounds until it reaches its maximum value.\n"), "{{rules|", GetReflectChance(Level).ToString(), "%}} chance to reflect light-based damage");
	}

	public int GetMaxLightRadius(int Level)
	{
		return (int)(4.0 + Math.Floor((float)Level / 2f));
	}

	public string GetDamage(int Level)
	{
		if (Level <= 1)
		{
			return "1d3";
		}
		if (Level <= 2)
		{
			return "1d4";
		}
		if (Level <= 3)
		{
			return "1d5";
		}
		if (Level <= 4)
		{
			return "1d4+1";
		}
		if (Level <= 5)
		{
			return "1d5+1";
		}
		if (Level <= 6)
		{
			return "1d4+2";
		}
		if (Level <= 7)
		{
			return "1d5+2";
		}
		if (Level <= 8)
		{
			return "1d4+3";
		}
		if (Level <= 9)
		{
			return "1d5+3";
		}
		if (Level > 9)
		{
			return "1d5+" + (Level - 6);
		}
		return "1d4+4";
	}

	public int GetLasePenetrationBonus()
	{
		return GetLasePenetrationBonus(Level);
	}

	public int GetLasePenetrationBonus(int Level)
	{
		return 4 + (Level - 1) / 2;
	}

	public bool Lase(Cell C, int PathLength = 0)
	{
		_ = Look._TextConsole;
		ScreenBuffer screenBuffer = TextConsole.ScrapBuffer.WithMap();
		bool result = false;
		if (C != null)
		{
			GameObject combatTarget = C.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
			if (combatTarget != null)
			{
				int lasePenetrationBonus = GetLasePenetrationBonus();
				int num = Stat.RollDamagePenetrations(combatTarget.Stat("AV"), lasePenetrationBonus, lasePenetrationBonus);
				if (num > 0)
				{
					string resultColor = Stat.GetResultColor(num);
					int num2 = 0;
					string damage = GetDamage(Level);
					for (int i = 0; i < num; i++)
					{
						num2 += damage.RollCached();
					}
					combatTarget.TakeDamage(num2, Owner: ParentObject, Message: "from %t laser beam! {{" + resultColor + "|(x" + num + ")}}", Attributes: "Light Laser", DeathReason: null, ThirdPersonDeathReason: null, Attacker: null, Source: null, Perspective: null, DescribeAsFrom: null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: true);
				}
				else if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your laser beam doesn't penetrate " + combatTarget.poss("armor") + ".", 'r');
				}
				else if (combatTarget.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("laser beam") + " doesn't penetrate your armor.", 'g');
				}
				result = true;
			}
		}
		if (C.IsVisible() || ParentObject.IsPlayer())
		{
			switch (Stat.Random(1, 3))
			{
			case 1:
				screenBuffer.WriteAt(C, "&C\u000f");
				break;
			case 2:
				screenBuffer.WriteAt(C, "&Y\u000f");
				break;
			default:
				screenBuffer.WriteAt(C, "&B\u000f");
				break;
			}
			screenBuffer.Draw();
			int num3 = 10 - PathLength / 5;
			if (num3 > 0)
			{
				Thread.Sleep(num3);
			}
		}
		return result;
	}

	public int GetReflectChance()
	{
		return GetReflectChance(Level);
	}

	public int GetReflectChance(int Level)
	{
		return 10 + 3 * Level;
	}

	public void SyncAbilityName()
	{
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(LaseActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.DisplayName = "Lase (" + (MaxLightRadius - RadiusPenalty) + " charges)";
		}
	}

	public static int GetRadiusRegrowthTurns(int Willpower)
	{
		int num = 15;
		if (!GlobalConfig.GetBoolSetting("LightManipulationWillpowerRecharge"))
		{
			return num;
		}
		int num2 = num;
		int num3 = (Willpower - 16) * 5;
		if (num3 != 0)
		{
			num2 = num2 * (100 - num3) / 100;
		}
		return Math.Max(Math.Min(num2, num * 5), num / 5);
	}

	public int GetRadiusRegrowthTurns()
	{
		return GetRadiusRegrowthTurns(ParentObject?.Stat("Willpower") ?? 16);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterMassMind")
		{
			RadiusPenalty = 0;
			SyncAbilityName();
		}
		else if (E.ID == "EndTurn")
		{
			if (RadiusPenalty > 0)
			{
				RadiusRegrowthTimer++;
				if (RadiusRegrowthTimer >= GetRadiusRegrowthTurns())
				{
					RadiusRegrowthTimer = 0;
					RadiusPenalty--;
					SyncAbilityName();
				}
			}
		}
		else if (E.ID == "RefractLight" && GetReflectChance().in100())
		{
			E.SetParameter("By", ParentObject);
			E.SetParameter("Direction", (int)(float)E.GetParameter("Angle") + 180);
			E.SetParameter("Verb", "reflect");
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		LaseActivatedAbilityID = AddMyActivatedAbility("Lase", "CommandLase", "Mental Mutations", null, "\u000f");
		LightActivatedAbilityID = AddMyActivatedAbility("Ambient Light", "CommandAmbientLight", "Mental Mutations", null, "\a", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref LightActivatedAbilityID);
		RemoveMyActivatedAbility(ref LaseActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
