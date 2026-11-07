using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

/// <summary>The base class for mutation parts to inherit from.</summary>
[Serializable]
public class BaseMutation : IPart
{
	/// <summary>A struct used while calculating the level of the mutation with all it's various bonues, and relaying the information to the UI.</summary>
	public struct LevelCalculation
	{
		public int bonus;

		public bool temporary;

		public string reason;

		public char sigil;
	}

	/// <summary>The variant chosen from the <see cref="M:XRL.World.Parts.Mutation.BaseMutation.GetVariants">GetVariants()</see>.</summary>
	public string Variant;

	[Obsolete("Preserves SaveCompat, use GetDisplayName() or Set DisplayName on XML Node.")]
	public string _DisplayName = "";

	/// <summary>The base level (without bonuses) of the mutation.</summary>
	public int BaseLevel = 1;

	/// <summary>The last value of BaseLevel, only differs during a call to <c><see cref="!:!">ChangeLevel</see></c>.</summary>
	public int LastLevel;

	/// <summary>Most mutations will have an activated ability, so the base defines one to use by default with its ability helper methods.</summary>
	public Guid ActivatedAbilityID;

	/// cache space saver - private
	[NonSerialized]
	private List<LevelCalculation> _levelCalcWorkspace = new List<LevelCalculation>();

	[NonSerialized]
	[Obsolete("Use GetMaxLevel()")]
	protected int MaxLevel = -1;

	[NonSerialized]
	[Obsolete("Cache Value Only, use GetMutationType()")]
	public string _Type;

	[NonSerialized]
	private MutationEntry _Entry;

	/// <summary>Additional parameters passed by default to <see cref="M:XRL.World.GameObject.UseEnergy(System.Int32,System.String,System.String,System.Nullable{System.Int32},System.Boolean)">UseEnergy</see> when calling the version without a <c>string Type</c> parameter.</summary>
	private string EnergyUseType;

	/// <summary>
	///   Overrides the level based mutation cap.  Is set by <see cref="M:XRL.World.GameObjectFactory.CreateObject(System.String)">the game object factory</see>
	///   when creating an object with a mutation set to a specific level.  Allows mosquitos to have Wings 10 at Level 1 for instance.
	/// </summary>
	public int CapOverride = -1;

	[NonSerialized]
	private string _RapidKey;

	/// <summary>The name shown in the mutations menu and character creation.</summary>
	public string DisplayName
	{
		[Obsolete("Use GetDisplayName()")]
		get
		{
			return GetDisplayName();
		}
		[Obsolete("All mutations should now have MutationEntries in XML. Set DisplayName on XML node.")]
		set
		{
			_DisplayName = value;
		}
	}

	public virtual int BaseElementWeight
	{
		get
		{
			if (BaseLevel > 0)
			{
				return 3 + BaseLevel;
			}
			return 0;
		}
	}

	/// <summary>Check whether mutation has more than one available variant.</summary>
	public virtual bool HasVariants
	{
		get
		{
			List<string> variants = GetVariants();
			if (variants != null)
			{
				return variants.Count > 1;
			}
			return false;
		}
	}

	/// <summary>Allow free selection of variant when gaining mutation.</summary>
	public virtual bool CanSelectVariant => true;

	/// <summary>Set the mutation's display name to the picked variant.</summary>
	public virtual bool UseVariantName => true;

	public int Level
	{
		get
		{
			return CalcLevel();
		}
		set
		{
			BaseLevel = value;
			if (ParentObject != null)
			{
				SyncMutationLevelsEvent.Send(ParentObject);
			}
		}
	}

	/// <summary>Example "Physical" or "MentalDefects" defaults to Mutation.xml category name</summary>
	public string Type
	{
		get
		{
			return GetMutationType();
		}
		[Obsolete("Should not need to set Type. Defaults to mutation Type in Mutations.xml, or the mutation category's Name.")]
		set
		{
			_Type = value;
		}
	}

	/// <summary>The <see cref="P:XRL.World.IPart.StatShifter" /> with the mutation's <see cref="P:XRL.World.Parts.Mutation.BaseMutation.DisplayName" />.</summary>
	public new StatShifter StatShifter
	{
		get
		{
			StatShifter statShifter = base.StatShifter;
			if (string.IsNullOrEmpty(statShifter.DefaultDisplayName))
			{
				statShifter.DefaultDisplayName = GetDisplayName(WithAnnotations: false);
			}
			return statShifter;
		}
	}

	protected virtual string GetBaseDisplayName()
	{
		return GetMutationEntry().GetDisplayName();
	}

	public string GetDisplayName(bool WithAnnotations = true)
	{
		if (!string.IsNullOrEmpty(_DisplayName))
		{
			return _DisplayName;
		}
		if (UseVariantName)
		{
			_DisplayName = GetVariantName(Variant);
		}
		if (string.IsNullOrEmpty(_DisplayName))
		{
			_DisplayName = GetBaseDisplayName() ?? "";
		}
		if (WithAnnotations && IsDefect())
		{
			return _DisplayName + " ({{r|D}})";
		}
		return _DisplayName;
	}

	public void SetDisplayName(string DisplayName)
	{
		_DisplayName = DisplayName;
	}

	public void ResetDisplayName()
	{
		_DisplayName = null;
	}

	/// <summary>Calculate and return the current level with bonuses.</summary>
	public int CalcLevel()
	{
		return CalcLevel(false);
	}

	public List<LevelCalculation> GetLevelCalculations()
	{
		CalcLevel(storeWork: true);
		return _levelCalcWorkspace;
	}

	public virtual int GetUIDisplayLevel()
	{
		return Level;
	}

	private int CalcLevel(bool storeWork = false)
	{
		if (storeWork)
		{
			_levelCalcWorkspace.Clear();
		}
		if (!CanLevel() || ParentObject == null)
		{
			return BaseLevel;
		}
		string text = "mutation";
		int num = BaseLevel;
		if (storeWork)
		{
			text = GetMutationTermEvent.GetFor(ParentObject, this);
			if (num <= 0)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = num,
					sigil = '\0',
					temporary = false,
					reason = "* You do not possess this " + text + " inherently, and so you cannot advance its rank."
				});
			}
			else
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = num,
					sigil = '\0',
					temporary = false,
					reason = "* This " + Grammar.MakePossessive(text) + " base rank is " + num + "."
				});
			}
		}
		MutationEntry mutationEntry = GetMutationEntry();
		Statistic value = null;
		Dictionary<string, Statistic> statistics = ParentObject.Statistics;
		if (statistics != null && statistics.TryGetValue(mutationEntry?.GetStat() ?? "", out value))
		{
			num += value.Modifier;
			if ((value.Modifier > 0) & storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = value.Modifier,
					sigil = '+',
					temporary = false,
					reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + value.Modifier + " due to your high " + value.Name + "."
				});
			}
			if ((value.Modifier < 0) & storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = value.Modifier,
					sigil = '-',
					temporary = false,
					reason = "- This " + Grammar.MakePossessive(text) + " rank is decreased by " + -value.Modifier + " due to your low " + value.Name + "."
				});
			}
		}
		int intProperty = ParentObject.GetIntProperty(mutationEntry?.Class);
		num += intProperty;
		if (intProperty != 0)
		{
			MetricsManager.LogWarning("Using old ClassName based IntProperty to adjust mutation level, use RequirePart<Mutations>().AddMutationMod(...):" + DebugName);
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.GetProperty());
		num += intProperty;
		if (intProperty != 0)
		{
			MetricsManager.LogWarning("Using old MutationEntry.Property based IntProperty to adjust mutation level, use RequirePart<Mutations>().AddMutationMod(...):" + DebugName);
		}
		intProperty = ParentObject.GetIntProperty("AllMutationLevelModifier");
		num += intProperty;
		if ((intProperty > 0) & storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ All your " + Grammar.Pluralize(text) + "' ranks are increased by " + intProperty + "."
			});
		}
		if ((intProperty < 0) & storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '-',
				reason = "- All your " + Grammar.Pluralize(text) + "' ranks are decreased by " + -intProperty + "."
			});
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.Category?.CategoryModifierName ?? "UnknownMutationLevelModifier");
		num += intProperty;
		if ((intProperty > 0) & storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ All your " + mutationEntry?.Category?.DisplayName + " " + Grammar.MakePossessive(Grammar.Pluralize(text)) + " ranks are increased by " + intProperty + "."
			});
		}
		if ((intProperty < 0) & storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '-',
				reason = "- All your " + mutationEntry?.Category?.DisplayName + " " + Grammar.MakePossessive(Grammar.Pluralize(text)) + " ranks are decreased by " + -intProperty + "."
			});
		}
		if (GetType() != typeof(AdrenalControl2) && IsPhysical())
		{
			intProperty = ParentObject.GetIntProperty("AdrenalLevelModifier");
			num += intProperty;
			if ((intProperty > 0) & storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					temporary = true,
					bonus = intProperty,
					sigil = '+',
					reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + intProperty + " due to your high adrenaline."
				});
			}
		}
		intProperty = GetRapidLevelAmount();
		num += intProperty;
		if ((intProperty > 0) & storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + intProperty + " due to being rapidly advanced " + intProperty / 3 + " time" + ((intProperty > 3) ? "s" : "") + "."
			});
		}
		Mutations part = ParentObject.GetPart<Mutations>();
		if (part != null)
		{
			for (int i = 0; i < part.MutationMods.Count; i++)
			{
				Mutations.MutationModifierTracker mutationModifierTracker = part.MutationMods[i];
				if (mutationModifierTracker.mutationName != Name)
				{
					continue;
				}
				num += mutationModifierTracker.bonus;
				if (storeWork)
				{
					switch (mutationModifierTracker.sourceType)
					{
					case Mutations.MutationModifierTracker.SourceType.Cooking:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to a metabolizing effect."
						});
						break;
					case Mutations.MutationModifierTracker.SourceType.Tonic:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to a tonic effect."
						});
						break;
					case Mutations.MutationModifierTracker.SourceType.Equipment:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to your equipped item, " + mutationModifierTracker.sourceName + "."
						});
						break;
					case Mutations.MutationModifierTracker.SourceType.External:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to " + mutationModifierTracker.sourceName + "."
						});
						break;
					default:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = ((mutationModifierTracker.bonus > 0) ? '+' : '-'),
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + Math.Abs(mutationModifierTracker.bonus) + " due to your " + mutationModifierTracker.sourceName + "."
						});
						break;
					}
				}
			}
		}
		if (num < 1)
		{
			if (storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = 1 - num,
					temporary = false,
					sigil = '+',
					reason = "+ " + ColorUtility.CapitalizeExceptFormatting(text) + " ranks cannot be reduced below 1."
				});
			}
			num = 1;
		}
		int mutationCap = GetMutationCap();
		if (mutationCap != -1 && mutationCap < num)
		{
			if (storeWork)
			{
				bool flag = false;
				int num2 = num - mutationCap;
				int num3 = _levelCalcWorkspace.Where((LevelCalculation c) => c.temporary).Aggregate(0, (int a, LevelCalculation b) => a + b.bonus);
				if (num3 > 0)
				{
					int num4 = Math.Min(num2, num3);
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						bonus = -num4,
						temporary = true,
						sigil = '-',
						reason = "- This " + Grammar.MakePossessive(text) + " rank is capped at " + mutationCap + " due to your level."
					});
					num2 -= num4;
					flag = true;
				}
				if (num2 > 0)
				{
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						bonus = -num2,
						temporary = false,
						sigil = ((!flag) ? '-' : '\0'),
						reason = (flag ? null : ("- This " + Grammar.MakePossessive(text) + " rank is capped at " + mutationCap + " due to your level."))
					});
				}
			}
			num = mutationCap;
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.GetCategoryForceProperty());
		intProperty += ParentObject.GetIntProperty(mutationEntry?.GetForceProperty());
		return num + intProperty;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != PooledEvent<GetPsychicGlimmerEvent>.ID || !(Type == "Mental")) && (ID != PooledEvent<IsSensableAsPsychicEvent>.ID || !(Type == "Mental")))
		{
			if (ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ActivatedAbilityID != Guid.Empty)
			{
				_ = ActivatedAbilityID;
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		if (Type == "Mental" && E.Subject == ParentObject && !IsDefect())
		{
			E.Level += Level;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		if (Type == "Mental")
		{
			E.Sensable = true;
		}
		return base.HandleEvent(E);
	}

	public void SyncLevel()
	{
		Mutations.SyncMutation(this, SyncGlimmer: true);
	}

	public virtual bool CompatibleWith(GameObject go)
	{
		List<MutationEntry> mutationEntries = MutationFactory.GetMutationEntries(this);
		if (mutationEntries == null)
		{
			return true;
		}
		foreach (MutationEntry item in mutationEntries)
		{
			string[] exclusions = item.GetExclusions();
			foreach (string name in exclusions)
			{
				if (MutationFactory.HasMutation(name))
				{
					string name2 = MutationFactory.GetMutationEntryByName(name).Class;
					if (go.HasPart(name2))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	/// <summary>
	/// For determining how many points of the mutation are "permanent" this will calculate the number of bonus levels
	/// added by Adrenal Control, temporary boosts from food and gear, and bonuses to physical and mental mutations.
	/// </summary>
	/// <returns>Number of levels attributable to a temporary boost.</returns>
	public int GetTemporaryLevels()
	{
		return (from v in GetLevelCalculations()
			where v.temporary
			select v).Aggregate(0, (int memo, LevelCalculation value) => memo + value.bonus);
	}

	/// <summary>Is the mutation in the physical category.</summary>
	public bool IsPhysical()
	{
		return GetMutationEntry()?.IsPhysical() ?? false;
	}

	/// <summary>Is the mutation in the mental category.</summary>
	public bool IsMental()
	{
		return GetMutationEntry()?.IsMental() ?? false;
	}

	/// <summary>Is the mutation in either defect category.</summary>
	public bool IsDefect()
	{
		return GetMutationEntry()?.IsDefect() ?? false;
	}

	/// <summary>Calculates the maximum level.  Defaults to 10 but will read MaxLevel from the mutations xml also.</summary>
	public virtual int GetMaxLevel()
	{
		if (MaxLevel == -1)
		{
			MaxLevel = 10;
			string name = GetType().Name;
			foreach (MutationEntry item in MutationFactory.AllMutationEntries())
			{
				if (item.Class.EqualsNoCase(name) && item.MaxLevel > -1)
				{
					MaxLevel = item.MaxLevel;
					break;
				}
			}
		}
		return MaxLevel;
	}

	/// <summary>Checks the mutation is levelable, base level is under max level, and total level is less than the current mutation cap</summary>
	public bool CanIncreaseLevel()
	{
		if (CanLevel() && BaseLevel < GetMaxLevel())
		{
			return Level < GetMutationCap();
		}
		return false;
	}

	/// <summary>The <see cref="F:XRL.MutationEntry.Type"><c>Type</c></see> of the mutation (from mutation entry or setting of <c><see cref="P:XRL.World.Parts.Mutation.BaseMutation.Type">Type</see></c>)</summary>
	public virtual string GetMutationType()
	{
		return _Type ?? (_Type = GetMutationEntry()?.Type ?? "Physical");
	}

	/// <summary>Is this mutation in the named <paramref name="category" /> in the Mutations.xml</summary>
	public bool isCategory(string category)
	{
		return GetMutationEntry()?.Category?.Name == category;
	}

	/// <summary>The <see cref="T:XRL.MutationEntry">MutationEntry</see> in <see cref="T:XRL.MutationFactory">MutationFactory</see></summary>
	public MutationEntry GetMutationEntry()
	{
		if (_Entry != null)
		{
			return _Entry;
		}
		List<MutationEntry> mutationEntries = MutationFactory.GetMutationEntries(this);
		if (mutationEntries.IsNullOrEmpty())
		{
			string text = (string.IsNullOrEmpty(_DisplayName) ? "..." : _DisplayName);
			MetricsManager.LogError("Mutation entry not found for '" + GetType().Name + "'. Please add `<mutation Name=\"" + text + "\" Class=\"" + GetType().Name + "\" />` node to a mutations xml file.");
			return _Entry = MutationFactory.CreateMutationEntryForMutation(this);
		}
		if (mutationEntries.Count == 1)
		{
			return _Entry = mutationEntries[0];
		}
		string variant = Variant;
		MutationEntry mutationEntry;
		if (variant == null)
		{
			foreach (MutationEntry item in mutationEntries)
			{
				if (item.Variant != null || item.HasVariants)
				{
					continue;
				}
				mutationEntry = (_Entry = item);
				mutationEntry = mutationEntry;
				goto IL_0182;
			}
		}
		else
		{
			foreach (MutationEntry item2 in mutationEntries)
			{
				if (!(item2.Variant == variant) && (!item2.HasVariants || !item2.GetVariants().Contains(variant)))
				{
					continue;
				}
				mutationEntry = (_Entry = item2);
				goto IL_0182;
			}
		}
		return _Entry = mutationEntries[0];
		IL_0182:
		return mutationEntry;
	}

	/// <summary>The <see cref="!:MutationEntry.Class">Class</see> from the mutation entry.</summary>
	public string GetMutationClass()
	{
		return GetMutationEntry()?.Class;
	}

	/// <summary>The <see cref="!:MutationEntry.Snippet">Snippet</see> from the mutation entry.</summary>
	public string GetBearerDescription()
	{
		return GetMutationEntry()?.Snippet ?? "";
	}

	/// <summary>Shortcut helper for calling <see cref="M:XRL.World.GameObject.UseEnergy(System.Int32,System.String,System.String,System.Nullable{System.Int32},System.Boolean)">ParentObject.UseEnergy</see>.</summary>
	public void UseEnergy(int Amount)
	{
		if (EnergyUseType == null)
		{
			EnergyUseType = Type + " Mutation";
		}
		ParentObject.UseEnergy(Amount, EnergyUseType);
	}

	/// <summary>Shortcut helper for calling <see cref="M:XRL.World.GameObject.UseEnergy(System.Int32,System.String,System.String,System.Nullable{System.Int32},System.Boolean)">ParentObject.UseEnergy</see>.</summary>
	public void UseEnergy(int Amount, string Type)
	{
		ParentObject.UseEnergy(Amount, Type);
	}

	public static int GetMutationCapForLevel(int level)
	{
		return level / 2 + 1;
	}

	/// <summary>Get the current cap for this mutation on this object.  Based on Level.  -1 means uncapped.</summary>
	public virtual int GetMutationCap()
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.HasStat("Level"))
		{
			return Math.Max(CapOverride, GetMutationCapForLevel(ParentObject.Stat("Level")));
		}
		return CapOverride;
	}

	/// <summary>The <see cref="F:XRL.MutationEntry.Stat">Stat</see> from the mutation entry, or its category.</summary>
	public string GetStat()
	{
		MutationEntry mutationEntry = GetMutationEntry();
		if (mutationEntry != null)
		{
			if (string.IsNullOrEmpty(mutationEntry.Stat) && mutationEntry.Category != null && !string.IsNullOrEmpty(mutationEntry.Category.Stat))
			{
				mutationEntry.Stat = mutationEntry.Category.Stat;
			}
			return mutationEntry.Stat;
		}
		return null;
	}

	/// <summary>Should the level of the mutation be shown in the character sheet. Defaults to <see cref="M:XRL.World.Parts.Mutation.BaseMutation.CanLevel" /></summary>
	public virtual bool ShouldShowLevel()
	{
		return CanLevel();
	}

	/// <summary>Can the player spend points in the mutation.  Defaults to true, override to disable.</summary>
	public virtual bool CanLevel()
	{
		return true;
	}

	/// <summary>Should we remutate the body when it changes (basically only to slog 💋)</summary>
	public virtual bool AffectsBodyParts()
	{
		return false;
	}

	/// <summary>Should we remutate the body when it changes (basically only to slog 💋)</summary>
	public virtual bool GeneratesEquipment()
	{
		return false;
	}

	public virtual string GetCreateCharacterDisplayName()
	{
		return GetDisplayName();
	}

	/// <summary>
	///     CollectStats using the current <see cref="P:XRL.World.Parts.Mutation.BaseMutation.Level" /> 
	/// </summary>
	/// <remarks>
	///     If overriding on a Mutation, you should override the version that is passed a Level
	/// </remarks>
	public virtual void CollectStats(Templates.StatCollector stats)
	{
		CollectStats(stats, Level);
	}

	/// <summary>
	///     CollectStats using <see cref="P:XRL.World.Parts.Mutation.BaseMutation.Level" /> 
	/// </summary>
	public virtual void CollectStats(Templates.StatCollector stats, int Level)
	{
	}

	/// <summary>The long description of the mutations powers.  Effects that change based on level should not be in this description method.</summary>
	public virtual string GetDescription()
	{
		if (Templates.TemplateByID.TryGetValue("Mutation." + GetMutationClass() + ".Description", out var value))
		{
			return value.Build(CollectStats, "mutation description");
		}
		return "<description>";
	}

	public virtual string GetLevelText(int Level)
	{
		if (Templates.TemplateByID.TryGetValue("Mutation." + GetMutationClass() + ".LevelText", out var value))
		{
			return value.Build((Templates.StatCollector stats) =>
			{
				CollectStats(stats, Level);
			}, "mutation leveltext").Trim('\n');
		}
		return "<Does level " + Level + " stuff>";
	}

	/// <summary>
	///   Add the mutation to the <paramref name="GO">game object</paramref>.
	///   Base method calls <see cref="M:XRL.World.Parts.Mutation.BaseMutation.ChangeLevel(System.Int32)">ChangeLevel</see>.
	///   Return false to cancel adding mutation.
	/// </summary>
	public virtual bool Mutate(GameObject GO, int Level = 1)
	{
		BaseLevel = Level;
		ChangeLevel(this.Level);
		return true;
	}

	/// <summary>Additional hook for after mutation is added.  <see cref="!:ParentObject" /> will be the new mutant.</summary>
	public virtual void AfterMutate()
	{
	}

	/// <summary>Removing the mutation from <paramref name="GO" />.</summary>
	public virtual bool Unmutate(GameObject GO)
	{
		LastLevel = 0;
		return true;
	}

	/// <summary>Hook for after mutation has been removed.  Note <see cref="!:ParentObject">ParentObject</see> will probably be null, so the old GameObject is passed in.</summary>
	public virtual void AfterUnmutate(GameObject GO)
	{
	}

	/// <summary>Number of levels from rapid mutation.</summary>
	public int GetRapidLevelAmount()
	{
		if (_RapidKey == null)
		{
			_RapidKey = "RapidLevel_" + Name;
		}
		return ParentObject.GetIntProperty(_RapidKey);
	}

	/// <summary>Set levels from rapid mutation and if enabled, Sync glimmer.</summary>
	public void SetRapidLevelAmount(int Amount, bool Sync = true)
	{
		if (_RapidKey == null)
		{
			_RapidKey = "RapidLevel_" + Name;
		}
		ParentObject.SetIntProperty(_RapidKey, Amount, RemoveIfZero: true);
		ChangeLevel(Level);
		if (Sync)
		{
			ParentObject.SyncMutationLevelAndGlimmer();
		}
	}

	/// <summary>Increase levels from rapid mutation and if enabled, Sync glimmer.</summary>
	public virtual void RapidLevel(int Amount, bool Sync = true)
	{
		if (_RapidKey == null)
		{
			_RapidKey = "RapidLevel_" + Name;
		}
		ParentObject.ModIntProperty(_RapidKey, Amount, RemoveIfZero: true);
		ChangeLevel(Level);
		if (Sync)
		{
			ParentObject.SyncMutationLevelAndGlimmer();
		}
	}

	/// <summary>This does not set the level, but is a hook to add logic for changing levels.  "LevelChanged" so to speak.  Base method sets <see cref="F:XRL.World.Parts.Mutation.BaseMutation.LastLevel" />=<paramref name="NewLevel" />.</summary>
	public virtual bool ChangeLevel(int NewLevel)
	{
		if (NewLevel >= 15 && LastLevel < 15 && ParentObject != null && ParentObject.IsPlayer() && !ParentObject.HasEffect<Dominated>())
		{
			Achievement.GET_MUTATION_LEVEL_15.Unlock();
		}
		LastLevel = NewLevel;
		return true;
	}

	/// <summary>Use to remove a equipment created by the mutation from the game.</summary>
	/// <param name="who">The mutant (ParentObject or what it used to be).</param>
	/// <param name="obj">The <c>ref</c> to the GameObject you are cleaning up - we will set it to null for you.</param>
	protected void CleanUpMutationEquipment(GameObject who, ref GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (who != null)
		{
			BodyPart bodyPart = obj.EquippedOn();
			if (bodyPart != null)
			{
				who.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart).SetSilent(Silent: true));
			}
			else
			{
				bodyPart = who.Body?.FindDefaultOrEquippedItem(obj);
				if (bodyPart != null && bodyPart.DefaultBehavior == obj)
				{
					bodyPart.DefaultBehavior = null;
				}
			}
		}
		obj.Obliterate();
		obj = null;
	}

	/// <summary>Fetches a static list of all possible variants.</summary>
	public virtual List<string> GetVariants()
	{
		return Mutations.GetVariants(Name);
	}

	/// <summary>Create a list of all possible variants for <see cref="M:XRL.World.Parts.Mutations.GetVariants(System.String)" /> using the "MutationEquipment" tag by default.</summary>
	/// <remarks>Invoked on stateless instance, result cached.</remarks>
	public virtual List<string> CreateVariants()
	{
		List<string> list = new List<string>();
		string name = Name;
		foreach (GameObjectBlueprint item in Mutations.GetAllMutationEquipment())
		{
			if (item.Tags.TryGetValue("MutationEquipment", out var value) && value == name && !item.IsBaseBlueprint())
			{
				list.Add(item.Name);
			}
		}
		return list;
	}

	/// <summary>Get the display name of the current variant.</summary>
	public virtual string GetVariantName()
	{
		return GetVariantName(Variant);
	}

	/// <summary>Get the display name of a variant's blueprint.</summary>
	public virtual string GetVariantName(string Blueprint)
	{
		return GetVariantName(GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint));
	}

	/// <summary>Get the display name of a variant's blueprint.</summary>
	public virtual string GetVariantName(GameObjectBlueprint Blueprint)
	{
		if (Blueprint == null)
		{
			return null;
		}
		if (!Blueprint.Tags.TryGetValue("VariantName", out var value))
		{
			return Blueprint.CachedDisplayNameStrippedTitleCase;
		}
		return value;
	}

	/// <summary>Choose variant #<paramref name="Index" /> from the <see cref="M:XRL.World.Parts.Mutation.BaseMutation.GetVariants" /> list.</summary>
	public virtual void SetVariant(string Variant)
	{
		this.Variant = Variant;
		if (UseVariantName)
		{
			ResetDisplayName();
		}
	}

	/// <summary>Get a renderable icon for a specific variant.</summary>
	public virtual IRenderable GetIcon(string Variant)
	{
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Variant);
		if (blueprintIfExists != null)
		{
			return new Renderable(blueprintIfExists);
		}
		return null;
	}

	/// <summary>Get a renderable icon for this mutation.</summary>
	public virtual IRenderable GetIcon()
	{
		if (Variant == null)
		{
			if (MutationFactory.TryGetMutationEntry(this, out var Entry))
			{
				return Entry.GetRenderable();
			}
			return null;
		}
		return GetIcon(Variant);
	}

	public virtual bool TryGetVariantValidity(GameObject Object, string Variant, out string Message)
	{
		Message = null;
		return true;
	}

	/// <summary>Display an option list of variants and set the selected variant to this instance.</summary>
	public bool SelectVariant(GameObject Object, bool AllowEscape = true)
	{
		List<string> variants = GetVariants();
		if (variants.IsNullOrEmpty())
		{
			return false;
		}
		string[] array = new string[variants.Count];
		IRenderable[] array2 = new IRenderable[variants.Count];
		bool[] array3 = new bool[variants.Count];
		for (int i = 0; i < variants.Count; i++)
		{
			string text = variants[i];
			array[i] = GetVariantName(text);
			array2[i] = GetIcon(text);
			array3[i] = TryGetVariantValidity(Object, text, out var Message);
			if (!array3[i])
			{
				array[i] = array[i].WithColor("K") + "\n" + Message;
			}
		}
		int num = 0;
		while (true)
		{
			num = Popup.PickOption("Choose variant", null, "", "Sounds/UI/ui_notification", array, null, array2, null, null, null, null, 0, 60, num, -1, AllowEscape);
			if (num < 0)
			{
				break;
			}
			if (array3[num])
			{
				SetVariant(variants[num]);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	///     Returns the term for this "mutation" on the object.
	/// </summary>
	/// <param name="who">Default to ParentObject</param>
	/// <returns />
	public string GetMutationTerm(GameObject who = null)
	{
		return GetMutationTermEvent.GetFor(who ?? ParentObject, this);
	}

	/// <summary>Use to remove a equipment created by the mutation from the game.</summary>
	/// <param name="who">The mutant (ParentObject or what it used to be).</param>
	/// <param name="obj">The GameObject you are cleaning up.</param>
	protected void CleanUpMutationEquipment(GameObject who, GameObject obj)
	{
		CleanUpMutationEquipment(who, ref obj);
	}

	public static BaseMutation Create(string Mutation, string Variant = null)
	{
		BaseMutation genericMutation = Mutations.GetGenericMutation(Mutation, Variant);
		if (genericMutation != null)
		{
			return Create(genericMutation.GetType(), Variant);
		}
		return null;
	}

	public static BaseMutation Create(Type Mutation, string Variant = null)
	{
		BaseMutation baseMutation = (BaseMutation)GamePartBlueprint.PartReflectionCache.Get(Mutation).GetNewInstance();
		if (!Variant.IsNullOrEmpty())
		{
			baseMutation.SetVariant(Variant);
		}
		return baseMutation;
	}

	public static BaseMutation Create(MutationEntry Mutation)
	{
		return Create(Mutation.Class, Mutation.Variant);
	}

	public static T Create<T>(string Variant = null) where T : BaseMutation
	{
		return (T)Create(typeof(T), Variant);
	}
}
