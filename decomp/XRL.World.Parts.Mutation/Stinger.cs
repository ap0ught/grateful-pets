using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Stinger : BaseDefaultEquipmentMutation
{
	public const string COMMAND_NAME = "CommandSting";

	public GameObject StingerObject;

	public Guid StingActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private IStingerProperties _StingerProperties;

	public IStingerProperties StingerProperties
	{
		get
		{
			if (_StingerProperties == null)
			{
				_StingerProperties = StingerObject?.GetPartDescendedFrom<IStingerProperties>();
				if (_StingerProperties == null && !Variant.IsNullOrEmpty() && GameObjectFactory.Factory.Blueprints.TryGetValue(Variant, out var value))
				{
					foreach (KeyValuePair<string, GamePartBlueprint> part in value.Parts)
					{
						GamePartBlueprint value2 = part.Value;
						if (typeof(IStingerProperties).IsAssignableFrom(value2.T))
						{
							_StingerProperties = (IStingerProperties)value2.Reflector.GetNewInstance();
							value2.InitializePartInstance(_StingerProperties);
							break;
						}
					}
				}
				if (_StingerProperties == null)
				{
					_StingerProperties = new StingerPoisonProperties();
				}
			}
			return _StingerProperties;
		}
	}

	public override bool CanSelectVariant => false;

	public string ManagerID => ParentObject.ID + "::Stinger";

	public Stinger()
	{
	}

	[Obsolete("mod compat")]
	public Stinger(string VenomType)
		: this()
	{
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Stinger obj = base.DeepCopy(Parent, MapInv) as Stinger;
		obj.StingerObject = null;
		obj._StingerProperties = null;
		return obj;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(StingActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandSting")
		{
			if (StingerObject == null || !StingerObject.IsEquippedProperly())
			{
				return ParentObject.ShowFailure("You don't have a stinger.");
			}
			if (!ParentObject.CanMoveExtremities("Sting", ShowMessage: true))
			{
				return false;
			}
			Cell cell = PickDirection("Sting");
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(ParentObject);
			if (combatTarget == null || combatTarget == ParentObject)
			{
				if (ParentObject.IsPlayer())
				{
					if (cell.HasObjectWithPart("Combat"))
					{
						Popup.ShowFail("There is no one there you can sting.");
					}
					else
					{
						Popup.ShowFail("There is no one there to sting.");
					}
				}
				return false;
			}
			UseEnergy(1000, "Physical Mutation Stinger");
			CooldownMyActivatedAbility(StingActivatedAbilityID, 25);
			if (ParentObject.IsPlayer())
			{
				DidX("strike", combatTarget.the + combatTarget.ShortDisplayName + " with your stinger", "!", null, null, ParentObject);
			}
			else
			{
				DidX("strike", combatTarget.a + combatTarget.ShortDisplayName + " with " + ParentObject.its + " stinger", "!", null, null, ParentObject);
			}
			Strike(StingerObject, combatTarget, Auto: true);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && GameObject.Validate(E.Target) && !E.Target.IsWall() && IsMyActivatedAbilityAIUsable(StingActivatedAbilityID) && E.Actor.CanMoveExtremities("Sting"))
		{
			E.Add("CommandSting");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LungedTarget");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LungedTarget" && StingerObject != null && StingerObject.IsEquippedProperly() && !ParentObject.Body.IsPrimaryWeapon(StingerObject))
		{
			Strike(StingerObject, E.GetGameObjectParameter("Defender"));
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return StingerProperties.GetDescription();
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("StingerPen", GetPenetration(Level), !stats.mode.Contains("ability"));
		stats.Set("StingerDamage", GetDamage(Level), !stats.mode.Contains("ability"));
		stats.Set("Duration", StingerProperties.GetDuration(Level));
		stats.Set("VenomType", StingerProperties.GetAdjective());
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public void Strike(GameObject Stinger, GameObject Defender, bool Auto = false)
	{
		Combat.MeleeAttackWithWeapon(ParentObject, Defender, Stinger, ParentObject.GetBodyPartByManager(ManagerID), Auto ? "Autohit,Autopen,Stinging" : "Stinging");
	}

	public int GetSave(int Level)
	{
		return 14 + Level * 2;
	}

	public int GetCooldown(int Level)
	{
		return StingerProperties.GetCooldown(Level);
	}

	public string GetDamage(int Level)
	{
		return StingerProperties.GetDamage(Level);
	}

	public int GetPenetration(int Level)
	{
		return StingerProperties.GetPenetration(Level);
	}

	public override IRenderable GetIcon()
	{
		if (!MutationFactory.TryGetMutationEntry(this, out var Entry))
		{
			return null;
		}
		return Entry.GetRenderable();
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder("20% chance on melee attack to sting your opponent ({{c|\u001a}}{{rules|").Append(GetPenetration(Level) + RuleSettings.VISUAL_PENETRATION_BONUS).Append("}} {{r|\u0003}}{{rules|")
			.Append(GetDamage(Level))
			.Append("}})\n")
			.Append("Stinger is a long blade and can only penetrate once.\nAlways sting on charge or lunge.\nStinger applies venom on damage (only 20% chance if Stinger is your primary weapon).\nMay use Sting activated ability to strike with your stinger and automatically hit and penetrate.\nSting cooldown: ")
			.Append(GetCooldown(Level))
			.Append('\n');
		StingerProperties.AppendLevelText(stringBuilder, Level);
		stringBuilder.Append("+200 reputation with {{w|arachnids}}");
		return Event.FinalizeString(stringBuilder);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		BodyPart partByManager = body.GetPartByManager(ManagerID);
		if (partByManager != null)
		{
			AddStingerTo(partByManager);
		}
	}

	public static bool IsUnmanagedPart(BodyPart Part)
	{
		return Part.Manager.IsNullOrEmpty();
	}

	public static BodyPart AddTail(GameObject Object, string ManagerID, bool UseUnmanaged = false, bool DoUpdate = true)
	{
		BodyPart bodyPart = Object?.Body?.GetBody();
		if (bodyPart == null)
		{
			return null;
		}
		if (UseUnmanaged)
		{
			BodyPart firstPart = bodyPart.GetFirstPart("Tail", IsUnmanagedPart);
			if (firstPart != null)
			{
				firstPart.Manager = ManagerID;
				return firstPart;
			}
		}
		Object.WantToReequip();
		BodyPart firstAttachedPart = bodyPart.GetFirstAttachedPart("Tail", 0, Object.Body, EvenIfDismembered: true);
		int? num;
		bool doUpdate;
		if (firstAttachedPart != null)
		{
			firstAttachedPart.ChangeLaterality(2);
			num = bodyPart.Category;
			int? category = num;
			doUpdate = DoUpdate;
			return bodyPart.AddPartAt(firstAttachedPart, "Tail", 1, null, null, null, null, ManagerID, category, null, null, null, null, null, null, null, null, null, null, null, null, doUpdate);
		}
		num = bodyPart.Category;
		int? category2 = num;
		string[] orInsertBefore = new string[3] { "Roots", "Thrown Weapon", "Floating Nearby" };
		doUpdate = DoUpdate;
		return bodyPart.AddPartAt("Tail", 0, null, null, null, null, ManagerID, category2, null, null, null, null, null, null, null, null, null, null, null, null, "Feet", orInsertBefore, doUpdate);
	}

	public static void RemoveTail(GameObject Object, string ManagerID)
	{
		BodyPart bodyPartByManager = Object.GetBodyPartByManager(ManagerID);
		if (bodyPartByManager != null)
		{
			int laterality = bodyPartByManager.Laterality;
			int laterality2 = ((bodyPartByManager.Laterality != 1) ? 1 : 2);
			Object.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
			bodyPartByManager = Object.GetFirstBodyPart("Tail", laterality2);
			if (bodyPartByManager != null && bodyPartByManager.IsLateralityConsistent() && !Object.HasBodyPart("Tail", laterality))
			{
				bodyPartByManager.ChangeLaterality(0);
			}
			Object.WantToReequip();
		}
	}

	public void AddStingerTo(BodyPart Limb)
	{
		_StingerProperties = null;
		if (StingerObject == null)
		{
			StingerObject = GameObject.Create(Variant ?? "Stinger");
		}
		int level = Level;
		bool flag = StingerObject.EquipAsDefaultBehavior();
		MeleeWeapon part = StingerObject.GetPart<MeleeWeapon>();
		if (part != null)
		{
			part.BaseDamage = GetDamage(level);
			part.PenBonus = GetPenetration(level);
			part.Slot = Limb.Type;
		}
		if (flag && Limb.DefaultBehavior != null)
		{
			if (Limb.DefaultBehavior == StingerObject)
			{
				return;
			}
			Limb.DefaultBehavior = null;
		}
		if (!flag && Limb.Equipped != null)
		{
			if (Limb.Equipped == StingerObject)
			{
				return;
			}
			if (Limb.Equipped.CanBeUnequipped(null, null, Forced: false, SemiForced: true))
			{
				Limb.ForceUnequip(Silent: true);
			}
		}
		if (!Limb.Equip(StingerObject, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true))
		{
			CleanUpMutationEquipment(ParentObject, ref StingerObject);
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		BodyPart bodyPart = AddTail(GO, ManagerID, UseUnmanaged: true);
		if (bodyPart != null)
		{
			AddStingerTo(bodyPart);
			StingActivatedAbilityID = AddMyActivatedAbility("Sting", "CommandSting", "Physical Mutations", null, "\u009f");
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref StingActivatedAbilityID);
		CleanUpMutationEquipment(GO, ref StingerObject);
		RemoveTail(GO, ManagerID);
		_StingerProperties = null;
		return base.Unmutate(GO);
	}
}
