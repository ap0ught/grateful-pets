using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

/// <summary>
///   FlamingHands powers the "Flaming Ray" mutation.  You can now choose a variant from hands, feet or face.
/// </summary>
[Serializable]
public class FlamingRay : BaseDefaultEquipmentMutation
{
	public static readonly string COMMAND_NAME = "CommandFlamingRay";

	public static readonly string PROJECTILE_BLUEPRINT = "ProjectileFlamingRay";

	public static readonly int RANGE = 9;

	/// <summary>The <see cref="F:XRL.World.Anatomy.BodyPart.Type" /> we replace (chosen by variant selection.)</summary>
	public string BodyPartType;

	/// <summary>Do we still need to create the object? Setup as a public for serialization purposes.</summary>
	public bool CreateObject = true;

	/// <summary>Sound file to play when attacking.</summary>
	public string Sound = "Sounds/Abilities/sfx_ability_mutation_flamingRay_attack";

	[NonSerialized]
	private static GameObject _Projectile;

	/// <summary>Create or retrive the already created Projectile game object.</summary>
	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.Validate(ref _Projectile))
			{
				_Projectile = GameObject.CreateUnmodified(PROJECTILE_BLUEPRINT);
			}
			return _Projectile;
		}
	}

	public override bool UseVariantName => false;

	/// <summary>We are request to be re-mutated automatically when our body is rebuilt. Thanks slog.</summary>
	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("EmitText", GetDescription());
		stats.Set("Range", RANGE);
		stats.Set("Damage", ComputeDamage(), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 10);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (CheckObjectProperlyEquipped() && E.Distance <= RANGE && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (OnWorldMap)
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			if (!CheckObjectProperlyEquipped())
			{
				BodyPart firstBodyPart = ParentObject.GetFirstBodyPart(BodyPartType);
				if (firstBodyPart != null)
				{
					return ParentObject.Fail("Your " + firstBodyPart.GetOrdinalName() + " " + (firstBodyPart.Plural ? "are" : "is") + " too damaged to do that!");
				}
				return ParentObject.Fail("Your " + BodyPartType + " is too damaged to do that!");
			}
			if (!Cast(this))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		base.Register(Object, Registrar);
	}

	/// <summary>Show selected variant in character creation.</summary>
	public override string GetCreateCharacterDisplayName()
	{
		if (BodyPartType != null)
		{
			return GetDisplayName() + " (" + BodyPartType + ")";
		}
		return GetDisplayName();
	}

	public override string GetDescription()
	{
		BodyPart registeredSlot = GetRegisteredSlot(BodyPartType, evenIfDismembered: true);
		if (registeredSlot != null)
		{
			return "You emit a ray of flame from your " + registeredSlot.GetOrdinalName() + ".";
		}
		return "You emit a ray of flame.";
	}

	public override string GetLevelText(int level)
	{
		int rANGE = RANGE;
		return string.Concat(string.Concat(string.Concat("Emits a " + rANGE + "-square ray of flame in the direction of your choice.\n", "Damage: {{rules|", ComputeDamage(level), "}}\n"), "Cooldown: 10 rounds\n"), "Melee attacks heat opponents by {{rules|", GetHeatOnHitAmount(level), "}} degrees");
	}

	public string GetHeatOnHitAmount(int level)
	{
		return level * 2 + "d8";
	}

	public string ComputeDamage(int level)
	{
		string text = level + "d4";
		if (ParentObject != null)
		{
			int num = ParentObject.Body?.GetPartCount(BodyPartType) ?? 0;
			if (num > 0)
			{
				text += num.Signed();
			}
		}
		else
		{
			text += "+1";
		}
		return text;
	}

	public string ComputeDamage()
	{
		return ComputeDamage(Level);
	}

	public void Flame(Cell C, ScreenBuffer Buffer = null, bool DoEffect = true, bool UsePopups = false)
	{
		if (C != null)
		{
			List<GameObject> objectsInCell = C.GetObjectsInCell();
			bool flag = false;
			foreach (GameObject item in objectsInCell)
			{
				if (!item.PhaseMatches(ParentObject))
				{
					continue;
				}
				item.TemperatureChange(310 + 25 * Level, ParentObject);
				if (DoEffect && !flag)
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					flag = true;
				}
			}
			int phase = ParentObject.GetPhase();
			DieRoll cachedDieRoll = ComputeDamage().GetCachedDieRoll();
			foreach (GameObject item2 in C.GetObjectsWithPartReadonly("Combat"))
			{
				int amount = cachedDieRoll.Resolve();
				GameObject parentObject = ParentObject;
				int phase2 = phase;
				item2.TakeDamage(amount, "from %t flames!", "Fire", null, null, parentObject, null, null, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, UsePopups, phase2);
			}
		}
		if (DoEffect)
		{
			C.Flameburst(Buffer);
		}
	}

	public static bool Cast(FlamingRay mutation = null, string level = "5-6")
	{
		if (mutation == null)
		{
			mutation = new FlamingRay();
			mutation.Level = level.RollCached();
			mutation.ParentObject = The.Player;
		}
		List<Cell> list = mutation.PickLine(RANGE, AllowVis.Any, null, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, BlackoutStops: false, null, null, "Flaming Ray", Snap: true);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		if (list.Count == 1 && mutation.ParentObject.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + mutation.ParentObject.itself + "?") != DialogResult.Yes)
		{
			return false;
		}
		bool valueOrDefault = mutation?.ParentObject?.CurrentCell?.ParentZone?.IsActive() == true;
		ScreenBuffer buffer = null;
		if (valueOrDefault)
		{
			buffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
			XRLCore.Core.RenderMapToBuffer(buffer);
		}
		mutation.UseEnergy(1000, "Physical Mutation Flaming Hands");
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, 10);
		mutation.PlayWorldSound(mutation.Sound, 0.5f, 0f, Combat: true);
		int i = 0;
		for (int num = Math.Min(list.Count, 10); i < num; i++)
		{
			if (list.Count == 1 || list[i] != mutation.ParentObject.CurrentCell)
			{
				mutation.Flame(list[i], buffer, valueOrDefault);
			}
			if (i < num - 1 && list[i].IsSolidForProjectile(Projectile, mutation.ParentObject, null, mutation.ParentObject.Target))
			{
				break;
			}
		}
		BodyPart registeredSlot = mutation.GetRegisteredSlot(mutation.BodyPartType, evenIfDismembered: false);
		IComponent<GameObject>.XDidY(mutation.ParentObject, "emit", "a flaming ray" + ((registeredSlot != null) ? (" from " + mutation.ParentObject.its + " " + registeredSlot.GetOrdinalName()) : ""), "!", null, null, mutation.ParentObject);
		return true;
	}

	public bool CheckObjectProperlyEquipped()
	{
		if (!CreateObject)
		{
			return true;
		}
		if (HasRegisteredSlot(BodyPartType))
		{
			return GetRegisteredSlot(BodyPartType, evenIfDismembered: false) != null;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			if (!CheckObjectProperlyEquipped())
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null)
			{
				string heatOnHitAmount = GetHeatOnHitAmount(Level);
				int num = 400;
				int num2 = heatOnHitAmount.RollMaxCached();
				if ((num2 > 0 && gameObjectParameter.Physics.Temperature < num) || (num2 < 0 && gameObjectParameter.Physics.Temperature > num))
				{
					gameObjectParameter.TemperatureChange(heatOnHitAmount.RollCached(), E.GetGameObjectParameter("Attacker"), Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, ParentObject.GetPhase());
				}
			}
		}
		return base.FireEvent(E);
	}

	private void AddAbility()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Flaming Ray", COMMAND_NAME, "Physical Mutations", GetLevelText(Level), "\u00a8");
	}

	public override bool ChangeLevel(int NewLevel)
	{
		bool result = base.ChangeLevel(NewLevel);
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.Description = GetLevelText(Level);
		}
		return result;
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		BodyPartType = GameObjectFactory.Factory.Blueprints.GetValue(Variant)?.GetPartParameter("Armor", "WornOn", "Hands");
	}

	public override string GetVariantName(GameObjectBlueprint Blueprint)
	{
		return Blueprint?.GetPartParameter("Armor", "WornOn", "Hands");
	}

	public override IRenderable GetIcon()
	{
		if (!MutationFactory.TryGetMutationEntry(this, out var Entry))
		{
			return null;
		}
		return Entry.GetRenderable();
	}

	public override bool TryGetVariantValidity(GameObject Object, string Variant, out string Message)
	{
		string text = GameObjectFactory.Factory.Blueprints.GetValue(Variant)?.GetPartParameter("Armor", "WornOn", "Hands");
		if (!Object.Body.HasPart(text, EvenIfDismembered: true))
		{
			BodyPartType bodyPartTypeOrFail = Anatomies.GetBodyPartTypeOrFail(text);
			Message = ("Missing " + bodyPartTypeOrFail.Name).WithColor("R");
			return false;
		}
		return base.TryGetVariantValidity(Object, Variant, out Message);
	}

	public void MakeFlaming(BodyPart part)
	{
		if (part == null)
		{
			return;
		}
		if (part.DefaultBehavior != null && part.DefaultBehavior.Blueprint != Variant)
		{
			part.DefaultBehavior.RequirePart<Flaming>();
		}
		if (part.Parts != null)
		{
			for (int i = 0; i < part.Parts.Count; i++)
			{
				MakeFlaming(part.Parts[i]);
			}
		}
	}

	public override void OnDecorateDefaultEquipment(Body Body)
	{
		if (CreateObject)
		{
			GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(Variant);
			BodyPartType = blueprint.GetPartParameter("Armor", "WornOn", "Hands");
			if (!TryGetRegisteredSlot(Body, BodyPartType, out var Part, EvenIfDismembered: true))
			{
				Part = Body.GetFirstPart(BodyPartType);
				if (Part != null)
				{
					RegisterSlot(BodyPartType, Part);
				}
			}
			if (Part != null && Part.DefaultBehavior == null)
			{
				GameObject gameObject = GameObject.Create(blueprint);
				gameObject.GetPart<Armor>().WornOn = BodyPartType;
				Part.DefaultBehavior = gameObject;
				Part.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "FlamingRay");
			}
			MakeFlaming(Part);
			if (BodyPartType == "Hands")
			{
				foreach (BodyPart item in Body.GetPart("Hand"))
				{
					MakeFlaming(item);
				}
			}
		}
		base.OnDecorateDefaultEquipment(Body);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (Variant.IsNullOrEmpty())
		{
			List<string> variants = GetVariants();
			Variant = variants.GetRandomElement();
			if (!BodyPartType.IsNullOrEmpty())
			{
				foreach (string item in variants)
				{
					if (GameObjectFactory.Factory.GetBlueprint(item).GetPartParameter("Armor", "WornOn", "") == BodyPartType)
					{
						Variant = item;
						break;
					}
				}
			}
		}
		AddAbility();
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
