using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using ConsoleLib.Console;
using HarmonyLib;
using ModelShark;
using Platform.IO;
using Qud.API;
using Qud.UI;
using TMPro;
using UnityEngine;
using XRL.Collections;
using XRL.Core;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.UI;

[HasOptionFlagUpdate]
[HasModSensitiveStaticCache]
public class Options
{
	public enum PlayAreaScaleTypes
	{
		Fit,
		Cover,
		PixelPerfect
	}

	[OptionFlag]
	public static bool ModernUI;

	[OptionFlag]
	public static bool ModernCharacterSheet;

	[OptionFlag]
	public static int MasterVolume;

	[OptionFlag]
	public static bool Sound;

	[OptionFlag]
	public static int SoundVolume;

	[OptionFlag]
	public static bool Music;

	[OptionFlag]
	public static bool Ambient;

	[OptionFlag]
	public static int AmbientVolume;

	[OptionFlag]
	public static int InterfaceVolume;

	[OptionFlag]
	public static int CombatVolume;

	[OptionFlag]
	public static int MusicVolume;

	[OptionFlag]
	public static bool MusicBackground;

	[OptionFlag]
	public static bool AutoexploreChests;

	[OptionFlag]
	public static bool AskForWorldmap;

	[OptionFlag]
	public static bool AskForOneItem;

	[OptionFlag]
	public static bool AskAutostair;

	[OptionFlag]
	public static bool ConfirmSwimming;

	[OptionFlag]
	public static bool ConfirmDangerousLiquid;

	[OptionFlag]
	public static bool DisplayLedLevelUp;

	[OptionFlag]
	public static bool PopupJournalNote;

	[OptionFlag("Option@AlwaysHPColor")]
	public static bool AlwaysHPColor;

	[OptionFlag("Option@HPColor")]
	public static bool HPColor;

	[OptionFlag("Option@MutationColor")]
	public static bool MutationColor;

	[OptionFlag]
	public static bool StripUIColorText;

	[OptionFlag]
	public static bool ShowSidebarAbilities;

	[OptionFlag]
	public static bool ShowCurrentCellPopup;

	[OptionFlag]
	public static bool ShowDetailedWeaponStats;

	[OptionFlag]
	public static bool ShowMonsterHPHearts;

	[OptionFlag]
	public static bool ShiftHidesSidebar;

	[OptionFlag]
	public static bool ShowNumberOfItems;

	[OptionFlag]
	public static bool DisableFloorTextures;

	[OptionFlag]
	public static bool HighlightStairs;

	[OptionFlag]
	public static bool LocationIntseadOfName;

	[OptionFlag]
	public static bool AlphanumericBits;

	[OptionFlag]
	public static bool AutogetSpecialItems;

	[OptionFlag]
	public static bool AutogetScrap;

	[OptionFlag]
	public static bool AutogetFood;

	[OptionFlag]
	public static bool AutogetBooks;

	[OptionFlag]
	public static bool AutogetZeroWeight;

	[OptionFlag]
	public static bool AutogetIfHostiles;

	[OptionFlag]
	public static bool AutogetFromNearby;

	[OptionFlag]
	public static bool AutogetDroppedLiquid;

	[OptionFlag]
	public static bool TakeallCorpses;

	[OptionFlag]
	public static bool AutogetNuggets;

	[OptionFlag]
	public static bool AutogetTradeGoods;

	[OptionFlag]
	public static bool AutogetFreshWater;

	[OptionFlag]
	public static bool DebugShowFullZoneDuringBuild;

	[OptionFlag]
	public static bool DebugDamagePenetrations;

	[OptionFlag]
	public static bool DebugSavingThrows;

	[OptionFlag]
	public static bool DebugGetLostChance;

	[OptionFlag]
	public static bool DebugStatShift;

	[OptionFlag]
	public static bool DebugEncounterChance;

	[OptionFlag]
	public static bool DebugTravelSpeed;

	[OptionFlag]
	public static bool DebugInternals;

	[OptionFlag]
	public static bool DebugAttitude;

	[OptionFlag]
	public static bool InventoryConsistencyCheck;

	[OptionFlag]
	public static bool ShowReachable;

	[OptionFlag]
	public static bool ShowOverlandEncounters;

	[OptionFlag]
	public static bool ShowOverlandRegions;

	[OptionFlag("OptionShowQuickstart")]
	public static bool ShowQuickstartOption;

	[OptionFlag]
	public static bool AllowReallydie;

	[OptionFlag]
	public static bool AllowSaveLoad;

	[OptionFlag]
	public static bool DisablePermadeath;

	[OptionFlag]
	public static bool EnablePrereleaseContent;

	[OptionFlag]
	public static bool EnableWishRegionNames;

	[OptionFlag]
	public static bool DisableTryLimit;

	[OptionFlag]
	public static bool DisableDefectLimit;

	[OptionFlag]
	public static bool GivesRepShowsCurrentRep;

	[OptionFlag]
	public static bool DisableImposters;

	[OptionFlag]
	public static bool DisableAchievements;

	[OptionFlag]
	public static bool CheckMemory;

	[OptionFlag]
	public static bool DrawPopulationHintMaps;

	[OptionFlag]
	public static bool DrawInfluenceMaps;

	[OptionFlag]
	public static bool DrawPathfinder;

	[OptionFlag]
	public static bool DrawPathfinderHalt;

	[OptionFlag]
	public static bool DrawNavigationWeightMaps;

	[OptionFlag]
	public static bool DrawCASystems;

	[OptionFlag]
	public static bool DrawFloodVis;

	[OptionFlag]
	public static bool DrawFloodAud;

	[OptionFlag]
	public static bool DrawFloodOlf;

	[OptionFlag]
	public static bool DrawFloodExplore;

	[OptionFlag]
	public static bool DrawArcs;

	[OptionFlag]
	public static bool DisablePlayerbrain;

	[OptionFlag]
	public static bool DisableZoneCaching2;

	[OptionFlag]
	public static bool DisableZoneThawRestrict;

	[OptionFlag]
	public static bool DebugShowConversationNode;

	[OptionFlag]
	public static bool AllowCSMods;

	[OptionFlag]
	public static bool HarmonyDebug;

	[OptionFlag]
	public static bool TextBuilderDebug;

	[OptionFlag]
	public static bool OutputModAssembly;

	public static bool DisableCacheCompression;

	[OptionFlag]
	public static bool CacheEarly;

	[OptionFlag]
	public static bool CollectEarly;

	[OptionFlag]
	public static bool DisableFloorTextureObjects;

	[OptionFlag]
	public static bool ThrottleAnimation;

	[OptionFlag]
	public static bool DrawStepImmediately;

	[OptionFlag]
	public static bool EnableSeed;

	[OptionFlag]
	public static bool Analytics;

	[OptionFlag]
	public static bool DisableBloodsplatter;

	[OptionFlag]
	public static bool DisableSmoke;

	[OptionFlag]
	public static bool MapDirectionsToKeypad;

	[OptionFlag]
	public static bool CapInputBuffer;

	[OptionFlag]
	public static bool LogTurnSeparator;

	[OptionFlag]
	public static bool IndentBodyParts;

	[OptionFlag]
	public static bool AbilityCooldownWarningAsMessage;

	[OptionFlag]
	public static bool PressingRightInInventoryEquips;

	[OptionFlag]
	public static bool AllowFramelessZoomOut;

	[OptionFlag]
	public static bool DropAll;

	[OptionFlag]
	public static bool OverlayNearbyObjectsLocal;

	[OptionFlag]
	public static bool OverlayNearbyObjectsTakeable;

	[OptionFlag]
	public static bool OverlayNearbyObjectsPools;

	[OptionFlag]
	public static bool OverlayNearbyObjectsPlants;

	[OptionFlag]
	public static bool UseOverlayCombatEffects;

	[OptionFlag]
	public static bool AutoSip;

	[OptionFlag]
	public static string AutoSipLevel;

	[OptionFlag]
	public static int AutosaveInterval;

	[OptionFlag]
	public static bool AutoTorch;

	[OptionFlag]
	public static bool AutoDisassembleScrap;

	[OptionFlag]
	public static bool ShowScavengeItemAsMessage;

	[OptionFlag]
	public static bool DismemberAsPopup;

	[OptionFlag]
	public static bool MouseMovement;

	[OptionFlag]
	public static bool MouseScrollWheel;

	public static int MinimapScale;

	[OptionFlag]
	public static int KeyRepeatDelay;

	[OptionFlag]
	public static int KeyRepeatRate;

	public static bool OverlayUI;

	[OptionFlag]
	public static bool DisplayVignette;

	[OptionFlag]
	public static bool DisplayScanlines;

	[OptionFlag]
	public static int DisplayBrightness;

	[OptionFlag]
	public static int DisplayContrast;

	[OptionFlag("OptionDisplayResolution")]
	public static string FullscreenResolution;

	[OptionFlag]
	public static bool DisplayFullscreen;

	[OptionFlag]
	public static string DisplayFramerate;

	[OptionFlag]
	public static bool ShowErrorPopups;

	[OptionFlag]
	public static bool UseCombatSounds;

	[OptionFlag]
	public static bool UseInterfaceSounds;

	[OptionFlag]
	public static bool MouseInput;

	[OptionFlag]
	public static bool OverlayMinimap;

	[OptionFlag]
	public static bool OverlayNearbyObjects;

	[OptionFlag]
	public static bool DigOnMove;

	[OptionFlag]
	public static int AutoexploreRate;

	[OptionFlag]
	public static bool AutogetAmmo;

	[OptionFlag]
	public static bool AutogetArtifacts;

	[OptionFlag]
	public static bool AutogetPrimitiveAmmo;

	[OptionFlag]
	public static bool DisableFullscreenColorEffects;

	[OptionFlag]
	public static bool DisableFullscreenWarpEffects;

	[OptionFlag("OptionPlayFireSounds")]
	public static bool UseFireSounds;

	[OptionFlag("OptionUseOverlayDamageText")]
	public static bool UseCombatText;

	[OptionFlag]
	public static bool UseTextParticleVFX;

	[OptionFlag]
	public static bool AutoexploreAttackIgnoredAdjacentEnemies;

	[OptionFlag]
	public static bool InterruptHeldMovement;

	[OptionFlag]
	public static bool DisplayMousableZoneTransitionBorder;

	[OptionFlag("PickTargetLocked", AllowMissing = true)]
	private static bool _PickTargetLocked;

	[OptionFlag("LookLocked", AllowMissing = true)]
	private static bool _LookLocked;

	[OptionFlag("OptionUseParticleVFX")]
	private static bool _UseParticleVFX;

	public static PlayAreaScaleTypes? PlayScaleOverride = null;

	private static PlayAreaScaleTypes _PlayScaleBind;

	public static int AbilityBarMode;

	public static int MouseCursor;

	public static int AutoexploreIgnoreEasyEnemies;

	public static int AutoexploreIgnoreDistantEnemies;

	[OptionFlag("OptionTileScale")]
	private static int _TileScale;

	public static int DockMovable;

	public static float DockOpacity = 1f;

	public static double StageScale = 1.0;

	public static string StageScaleRaw = "1.0";

	[OptionFlag]
	public static bool DisableTextAnimationEffects;

	[OptionFlag]
	public static bool DisableAllIdleTileAnimations;

	[OptionFlag("OptionUseTextAutowalkThreatIndicator")]
	public static bool UseTextAutoactInterruptionIndicator;

	[OptionFlag]
	public static bool EnableMods;

	public static NameValueBag Bag;

	public static GameOptions Map;

	public static Dictionary<string, List<GameOption>> OptionsByCategory;

	public static Dictionary<string, GameOption> OptionsByID;

	public static Dictionary<string, string> DefaultsByID;

	private static ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private static Dictionary<string, bool> lastOptionsRequirementState;

	public static int _MessageLogLineSizeAdjustment;

	private static Dictionary<string, Rack<MemberInfo>> OptionBindings = new Dictionary<string, Rack<MemberInfo>>();

	private static bool BindingsInitialized = false;

	private static readonly Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "options", HandleNodes },
		{ "option", HandleOptionNode }
	};

	public static int ThrottleAnimationRate
	{
		get
		{
			if (ThrottleAnimation)
			{
				return 100;
			}
			return 20;
		}
	}

	public static bool PickTargetLocked
	{
		get
		{
			return _PickTargetLocked;
		}
		set
		{
			SetOption("OptionPickTargetLocked", value);
		}
	}

	public static bool LookLocked
	{
		get
		{
			return _LookLocked;
		}
		set
		{
			SetOption("OptionLookLocked", value);
		}
	}

	public static bool UseParticleVFX
	{
		get
		{
			if (UseImposters)
			{
				return _UseParticleVFX;
			}
			return false;
		}
	}

	public static bool UseImposters => !DisableImposters;

	public static bool UseTiles => Globals.RenderMode == RenderModeType.Tiles;

	public static PlayAreaScaleTypes PlayScale => PlayScaleOverride ?? _PlayScaleBind;

	[OptionFlag("OptionPlayScale")]
	private static string PlayScaleBind
	{
		set
		{
			_PlayScaleBind = value switch
			{
				"Fit" => PlayAreaScaleTypes.Fit, 
				"Cover" => PlayAreaScaleTypes.Cover, 
				"Pixel Perfect" => PlayAreaScaleTypes.PixelPerfect, 
				_ => PlayAreaScaleTypes.Fit, 
			};
		}
	}

	[OptionFlag("AbilityBarMode")]
	private static string AbilityBarModeBind
	{
		set
		{
			int abilityBarMode = ((value == "Compact") ? 1 : 0);
			AbilityBarMode = abilityBarMode;
		}
	}

	[OptionFlag("OptionMouseCursor")]
	private static string MouseCursorBind
	{
		set
		{
			MouseCursor = value switch
			{
				"System" => 0, 
				"Default" => 1, 
				"Alternate" => 2, 
				_ => 1, 
			};
		}
	}

	[OptionFlag("OptionAutoexploreIgnoreEasyEnemies")]
	private static string AutoexploreIgnoreEasyEnemiesBind
	{
		set
		{
			AutoexploreIgnoreEasyEnemies = DifficultyEvaluation.GetDifficultyFromDescription(value);
		}
	}

	[OptionFlag("OptionAutoexploreIgnoreDistantEnemies")]
	private static string AutoexploreIgnoreDistantEnemiesBind
	{
		set
		{
			AutoexploreIgnoreDistantEnemies = (int.TryParse(value, out var result) ? result : 9999999);
		}
	}

	public static int TileScale
	{
		get
		{
			if (PlayScale == PlayAreaScaleTypes.PixelPerfect)
			{
				return _TileScale;
			}
			return 0;
		}
	}

	[OptionFlag("OptionDockMovable")]
	private static string DockMovableBind
	{
		set
		{
			DockMovable = value switch
			{
				"Flip" => 3, 
				"Right" => 2, 
				"Left" => 1, 
				_ => 0, 
			};
		}
	}

	[OptionFlag("OptionDockOpacity")]
	private static string DockOpacityBind
	{
		set
		{
			if (int.TryParse(value, out var result))
			{
				DockOpacity = (float)result / 100f;
			}
			else
			{
				DockOpacity = 1f;
			}
		}
	}

	[OptionFlag("OptionPrereleaseStageScale")]
	private static string StageScaleBind
	{
		set
		{
			StageScaleRaw = value;
			double result = 1.0;
			if (value.StartsWith("auto"))
			{
				if (value == "auto x1.25")
				{
					result = 1.25;
				}
				if (value == "auto x1.5")
				{
					result = 1.5;
				}
				StageScale = Math.Min((double)Screen.width * result / 1920.0, (double)Screen.height * result / 640.0);
			}
			else if (double.TryParse(value, out result))
			{
				StageScale = result;
			}
			else
			{
				StageScale = 1.0;
			}
		}
	}

	public static bool PrereleaseInputManager => true;

	public static bool SifrahExamine
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahExamine").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahExamineAuto
	{
		get
		{
			string text = GetOption("OptionSifrahExamineAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahRepair
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahRepair").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahRepairAuto
	{
		get
		{
			string text = GetOption("OptionSifrahRepairAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahReverseEngineer
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahReverseEngineer").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static bool SifrahDisarming
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahDisarming").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahDisarmingAuto
	{
		get
		{
			string text = GetOption("OptionSifrahDisarmingAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahHaggling
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahHaggling").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static bool SifrahRecruitment
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahRecruitment").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahRecruitmentAuto
	{
		get
		{
			string text = GetOption("OptionSifrahRecruitmentAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahHacking
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahHacking").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahHackingAuto
	{
		get
		{
			string text = GetOption("OptionSifrahHackingAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static string SifrahHackingLowLevel
	{
		get
		{
			string text = GetOption("OptionSifrahHackingLowLevel");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahItemNaming
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahItemNaming").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahItemModding
	{
		get
		{
			if (!SifrahGame.Installed)
			{
				return "Never";
			}
			string text = GetOption("OptionSifrahItemModding");
			if (text.IsNullOrEmpty())
			{
				text = "Never";
			}
			return text;
		}
	}

	public static bool SifrahRealityDistortion
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahRealityDistortion").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahRealityDistortionAuto
	{
		get
		{
			string text = GetOption("OptionSifrahRealityDistortionAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahPsychicCombat
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahPsychicCombat").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static string SifrahPsychicCombatAuto
	{
		get
		{
			string text = GetOption("OptionSifrahPsychicCombatAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || text.IsNullOrEmpty())
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static string SifrahWaterRitual
	{
		get
		{
			if (!SifrahGame.Installed)
			{
				return "Never";
			}
			string text = GetOption("OptionSifrahWaterRitual");
			if (text.IsNullOrEmpty())
			{
				text = "Never";
			}
			return text;
		}
	}

	public static bool SifrahBaetylOfferings
	{
		get
		{
			if (SifrahGame.Installed)
			{
				return GetOption("OptionSifrahBaetylOfferings").EqualsNoCase("Yes");
			}
			return false;
		}
	}

	public static bool AnySifrah
	{
		get
		{
			if (SifrahGame.Installed)
			{
				if (!SifrahExamine && !SifrahRepair && !SifrahReverseEngineer && !SifrahDisarming && !SifrahHaggling && !SifrahRecruitment && !SifrahHacking && !SifrahItemNaming && !(SifrahItemModding != "Never") && !SifrahRealityDistortion && !SifrahPsychicCombat && !(SifrahWaterRitual != "Never"))
				{
					return SifrahBaetylOfferings;
				}
				return true;
			}
			return false;
		}
	}

	public static int MessageLogLineSizeAdjustment => _MessageLogLineSizeAdjustment;

	public static string StageViewID => "Stage";

	public static void SetOption(string ID, bool Value)
	{
		SetOption(ID, Value ? "Yes" : "No");
	}

	public static void SetOption(string ID, string Value)
	{
		using (Lock.TakeWriteLock())
		{
			Bag.SetValue(ID, Value);
		}
		UpdateFlags(ID);
	}

	public static bool HasOption(string ID)
	{
		using (Lock.TakeReadLock())
		{
			return OptionsByID.ContainsKey(ID);
		}
	}

	public static bool HasOptionValue(string ID)
	{
		if (Bag == null)
		{
			Debug.LogWarning("Accessing options pre-init: " + ID);
			return false;
		}
		using (Lock.TakeReadLock())
		{
			return Bag.GetValue(ID) != null;
		}
	}

	public static string GetOption(string ID, string Default = "")
	{
		if (Bag == null)
		{
			Debug.LogWarning("Accessing options pre-init: " + ID);
			return Default;
		}
		using (Lock.TakeReadLock())
		{
			string value = Bag.GetValue(ID);
			if (value != null)
			{
				return value;
			}
			if (DefaultsByID.TryGetValue(ID, out value))
			{
				return value;
			}
			return Default;
		}
	}

	public static bool GetOptionBool(string ID)
	{
		return GetOption(ID).EqualsNoCase("Yes");
	}

	public static bool ShouldCheckRequirements()
	{
		if (lastOptionsRequirementState == null)
		{
			lastOptionsRequirementState = new Dictionary<string, bool>();
		}
		bool result = false;
		foreach (KeyValuePair<string, GameOption> item in OptionsByID)
		{
			bool flag = item.Value.Requires?.RequirementsMet ?? true;
			if (lastOptionsRequirementState.TryGetValue(item.Key, out var value))
			{
				if (value != flag)
				{
					result = true;
					lastOptionsRequirementState.Set(item.Key, flag);
				}
			}
			else
			{
				result = true;
				lastOptionsRequirementState.Set(item.Key, flag);
			}
		}
		return result;
	}

	public static void UpdateBindings(string ID = null, bool Refresh = false)
	{
		if (!BindingsInitialized | Refresh)
		{
			OptionBindings.Clear();
			foreach (Type item in ModManager.GetTypesWithAttribute(typeof(HasOptionFlagUpdate), Cache: false))
			{
				HasOptionFlagUpdate customAttribute = item.GetCustomAttribute<HasOptionFlagUpdate>();
				string prefix = customAttribute.Prefix ?? "Option";
				bool fieldFlags = customAttribute.FieldFlags;
				BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo[] fields = item.GetFields(bindingAttr);
				foreach (FieldInfo fieldInfo in fields)
				{
					bool requireAttribute = !fieldFlags || !fieldInfo.IsPublic;
					EvaluateMember(fieldInfo.FieldType, fieldInfo, prefix, requireAttribute);
				}
				PropertyInfo[] properties = item.GetProperties(bindingAttr);
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (propertyInfo.CanWrite)
					{
						bool requireAttribute2 = !fieldFlags || !propertyInfo.SetMethod.IsPublic;
						EvaluateMember(propertyInfo.PropertyType, propertyInfo, prefix, requireAttribute2);
					}
				}
			}
			BindingsInitialized = true;
		}
		if (!ID.IsNullOrEmpty())
		{
			if (OptionBindings.TryGetValue(ID, out var value))
			{
				Update(ID, value);
			}
			return;
		}
		foreach (var (iD, members) in OptionBindings)
		{
			Update(iD, members);
		}
		static void EvaluateMember(Type Type, MemberInfo Info, string Prefix, bool RequireAttribute)
		{
			OptionFlag customAttribute2 = Info.GetCustomAttribute<OptionFlag>();
			if ((!RequireAttribute || customAttribute2 != null) && ((object)Type == typeof(bool) || (object)Type == typeof(string) || (object)Type == typeof(int) || (object)Type == typeof(float)))
			{
				bool num = customAttribute2?.AllowMissing ?? false;
				string text2 = customAttribute2?.ID ?? Info.Name;
				string text3 = Prefix + text2;
				if (num || HasOption(text3))
				{
					text2 = text3;
				}
				else if (!HasOption(text2))
				{
					MetricsManager.LogAssemblyError(Info, "No option by ID '" + text3 + "' or '" + text2 + "' could be resolved for '" + Info.DeclaringType.FullName + "." + Info.Name + "'.");
					return;
				}
				if (!OptionBindings.TryGetValue(text2, out var value2))
				{
					value2 = (OptionBindings[text2] = new Rack<MemberInfo>());
				}
				value2.Add(Info);
			}
		}
		static void Update(string iD2, Rack<MemberInfo> Members)
		{
			string option = GetOption(iD2);
			foreach (MemberInfo Member in Members)
			{
				try
				{
					if (Member is FieldInfo { FieldType: var fieldType } fieldInfo2)
					{
						if ((object)fieldType == typeof(bool))
						{
							fieldInfo2.SetValue(null, option.EqualsNoCase("Yes"));
						}
						else if ((object)fieldType == typeof(string))
						{
							fieldInfo2.SetValue(null, option);
						}
						else if ((object)fieldType == typeof(int))
						{
							fieldInfo2.SetValue(null, int.TryParse(option, out var result) ? result : 0);
						}
						else if ((object)fieldType == typeof(float))
						{
							fieldInfo2.SetValue(null, float.TryParse(option, out var result2) ? result2 : 0f);
						}
					}
					else if (Member is PropertyInfo { PropertyType: var propertyType } propertyInfo2)
					{
						if ((object)propertyType == typeof(bool))
						{
							propertyInfo2.SetValue(null, option.EqualsNoCase("Yes"));
						}
						else if ((object)propertyType == typeof(string))
						{
							propertyInfo2.SetValue(null, option);
						}
						else if ((object)propertyType == typeof(int))
						{
							propertyInfo2.SetValue(null, int.TryParse(option, out var result3) ? result3 : 0);
						}
						else if ((object)propertyType == typeof(float))
						{
							propertyInfo2.SetValue(null, float.TryParse(option, out var result4) ? result4 : 0f);
						}
					}
				}
				catch (Exception message)
				{
					MetricsManager.LogAssemblyError(Member, message);
				}
			}
		}
	}

	public static void UpdateFlags(string ID = null, bool Refresh = false)
	{
		UpdateBindings(ID, Refresh);
		ObjectFinder.instance?.ReadOptions();
		SingletonWindowBase<MouseBlocker>.instance?.UpdateOptions();
		CursorManager.instance?.UpdateOptions();
		if (CapInputBuffer)
		{
			GameManager.bCapInputBuffer = true;
		}
		else
		{
			GameManager.bCapInputBuffer = false;
		}
		int.TryParse(GetOption("OptionMessageLineLogScale"), out _MessageLogLineSizeAdjustment);
		if (GetOption("OptionUseTiles").EqualsNoCase("Yes"))
		{
			Globals.RenderMode = RenderModeType.Tiles;
		}
		else
		{
			Globals.RenderMode = RenderModeType.Text;
		}
		if (ModernUI)
		{
			GameManager.Instance.ModernUI = true;
		}
		else
		{
			GameManager.Instance.ModernUI = false;
		}
		if (Analytics)
		{
			Globals.EnableMetrics = true;
		}
		else
		{
			Globals.EnableMetrics = false;
		}
		if (Sound)
		{
			Globals.EnableSound = true;
		}
		else
		{
			Globals.EnableSound = false;
		}
		if (Music)
		{
			Globals.EnableMusic = true;
		}
		else
		{
			Globals.EnableMusic = false;
		}
		if (Ambient)
		{
			Globals.EnableAmbient = true;
		}
		else
		{
			Globals.EnableAmbient = false;
		}
		Globals.AmbientVolume = (float)AmbientVolume / 100f * 0.5f;
		Globals.InterfaceVolume = (float)InterfaceVolume / 100f;
		Globals.CombatVolume = (float)CombatVolume / 100f;
		if (int.TryParse(GetOption("OptionDisplayHPWarning", "40%").TrimEnd('%'), out var result))
		{
			Globals.HPWarningThreshold = result;
		}
		else
		{
			Globals.HPWarningThreshold = int.MinValue;
		}
		if (MouseInput)
		{
			GameManager.Instance.MouseInput = true;
		}
		else
		{
			GameManager.Instance.MouseInput = false;
		}
		SoundManager.WriteSoundsToLog = GetOption("OptionWriteSoundsToLog").EqualsNoCase("Yes");
		AchievementManager.Enabled = !DisableAchievements;
		int masterVolume = MasterVolume;
		int musicVolume = MusicVolume;
		int soundVolume = SoundVolume;
		GameManager.Instance.compassScale = (float)Convert.ToInt32(GetOption("OptionOverlayCompassScale", "100")) / 100f;
		GameManager.Instance.nearbyObjectsListScale = (float)Convert.ToInt32(GetOption("OptionOverlayNearbyObjectsScale", "100")) / 100f;
		GameManager.Instance.minimapScale = (float)Convert.ToInt32(GetOption("OptionMinimapScale", "100")) / 100f;
		GameManager.Instance.TileScale = TileScale;
		GameManager.Instance.StageScale = StageScale;
		GameManager.Instance.DockMovable = DockMovable;
		GameManager.Instance.DisplayMinimap = OverlayMinimap;
		Harmony.DEBUG = HarmonyDebug;
		int result2 = 2000;
		int.TryParse(GetOption("OptionTooltipDelay"), out result2);
		TooltipManager.Instance.tooltipDelay = (float)result2 / 1000f;
		BaseLineWithTooltip.TOOLTIP_DELAY = (float)result2 / 1000f;
		lock (SoundManager.SoundRequests)
		{
			SoundManager.MasterVolume = (float)masterVolume / 100f;
			SoundManager.MusicVolume = (float)musicVolume / 100f;
			SoundManager.SoundVolume = (float)soundVolume / 100f;
			SoundManager.MusicSources.StopFadeAsync();
		}
		float num = (float)Convert.ToInt32(GetOption("OptionKeyRepeatDelay")) / 100f;
		float num2 = (float)Convert.ToInt32(GetOption("OptionKeyRepeatRate")) / 100f;
		ControlManager.delaytime = 0.1f + 2f * num;
		ControlManager.repeattime = 0f + 0.2f * (1f - num2);
		ControlManager.updateFont = true;
		Leveler.PlayerLedPrompt = DisplayLedLevelUp;
		IBaseJournalEntry.NotedPrompt = PopupJournalNote;
		if (ModManager.Initialized)
		{
			foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(OptionFlagUpdate), typeof(HasOptionFlagUpdate)))
			{
				try
				{
					item.Invoke(null, Array.Empty<object>());
				}
				catch (Exception arg)
				{
					MetricsManager.LogAssemblyError(item, $"Error invoking {item.DeclaringType.FullName}.{item.Name}: {arg}");
				}
			}
		}
		CursorManager.instance?.Sync();
		if (GetOption("OptionBugDeckKeyboardShift") == "Yes")
		{
			TMP_InputField.keycodeMapper = Keyboard.UcKeycodeMapper;
			TMP_InputField.enableHorribleDeckWorkaround = true;
		}
		else
		{
			TMP_InputField.enableHorribleDeckWorkaround = false;
		}
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			GameObject.Find("Main Camera").GetComponent<CC_AnalogTV>().enabled = DisplayScanlines;
			GameObject.Find("Main Camera").GetComponent<CC_FastVignette>().enabled = DisplayVignette;
			GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Math.Max(-70, DisplayBrightness);
			GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().contrast = Math.Max(-70, DisplayContrast);
			if (DisplayFullscreen)
			{
				string text = FullscreenResolution;
				if (text == "*Max")
				{
					Resolution resolution = GameManager.resolutions.Last();
					text = resolution.width + "x" + resolution.height;
				}
				if (text == "Screen")
				{
					text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
				}
				if (text == "Unset")
				{
					Screen.fullScreen = true;
				}
				else
				{
					string[] array = text.Split('x');
					int width = Convert.ToInt32(array[0]);
					int height = Convert.ToInt32(array[0]);
					Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
					Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
				}
			}
			else
			{
				Screen.fullScreen = false;
			}
			if (MusicBackground)
			{
				Application.runInBackground = true;
				SoundManager.MusicSources.SetMusicBackground(State: true);
			}
			else
			{
				Application.runInBackground = true;
				SoundManager.MusicSources.SetMusicBackground(State: false);
			}
			string displayFramerate = DisplayFramerate;
			if (displayFramerate == "Unlimited")
			{
				QualitySettings.vSyncCount = 0;
				Application.targetFrameRate = 0;
			}
			else
			{
				if (!(displayFramerate == "VSync"))
				{
					try
					{
						Application.targetFrameRate = Convert.ToInt16(displayFramerate);
						QualitySettings.vSyncCount = 0;
						return;
					}
					catch
					{
						Application.targetFrameRate = 60;
						QualitySettings.vSyncCount = 0;
						return;
					}
				}
				QualitySettings.vSyncCount = 1;
				Application.targetFrameRate = 60;
			}
		});
	}

	[ModSensitiveCacheInit]
	public static void LoadAllOptions()
	{
		if (GameManager.AwakeComplete)
		{
			LoadOptions();
			LoadModOptions();
			UpdateFlags();
		}
	}

	public static void LoadOptions()
	{
		OptionsByCategory = new Dictionary<string, List<GameOption>>();
		OptionsByID = new Dictionary<string, GameOption>();
		Bag = new NameValueBag(DataManager.LocalPath("PlayerOptions.json"));
		DefaultsByID = new Dictionary<string, string>();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Options", IncludeBase: true, IncludeMods: false))
		{
			try
			{
				HandleNodes(item);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Error loading base options", x);
			}
		}
		Bag.Load();
		if (Bag.GetValue("OptionAutogetDroppedLiquid") == null && Bag.GetValue("OptionAutogetNoDroppedLiquid").EqualsNoCase("Yes"))
		{
			Bag.SetValue("OptionAutogetDroppedLiquid", "No");
		}
	}

	private static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(_Nodes);
	}

	public static void LoadModOptions()
	{
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("Options", IncludeBase: false))
		{
			try
			{
				HandleNodes(item);
			}
			catch (Exception msg)
			{
				item.modInfo.Error(msg);
			}
		}
		SifrahGame.Installed = GlobalConfig.GetBoolSetting("EnableSifrah");
	}

	public static void LoadModOptionDefaults()
	{
		foreach (ModInfo mod in ModManager.Mods)
		{
			foreach (FileInfo item in mod.Directory.EnumerateFiles("*Option*.xml", SearchOption.AllDirectories))
			{
				CacheDefaults(item.FullName);
			}
		}
	}

	private static void CacheDefaults(string Path)
	{
		Lock.EnterWriteLock();
		try
		{
			using XmlReader xmlReader = XmlReader.Create(Path);
			while (xmlReader.Read())
			{
				if (!xmlReader.Name.IsNullOrEmpty() && !(xmlReader.Name == "xml"))
				{
					if (xmlReader.Name.EqualsNoCase("Options"))
					{
						break;
					}
					return;
				}
			}
			while (xmlReader.Read())
			{
				if (xmlReader.Name.EqualsNoCase("Option") && xmlReader.NodeType != XmlNodeType.EndElement)
				{
					string attribute = xmlReader.GetAttribute("ID");
					string attribute2 = xmlReader.GetAttribute("Default");
					if (attribute != null && attribute2 != null)
					{
						DefaultsByID[attribute] = attribute2;
					}
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			Lock.ExitWriteLock();
		}
	}

	private static void HandleOptionNode(XmlDataHelper xml)
	{
		GameOption gameOption = LoadOptionNode(xml);
		if (!OptionsByCategory.ContainsKey(gameOption.Category))
		{
			OptionsByCategory.Add(gameOption.Category, new List<GameOption>());
		}
		OptionsByCategory[gameOption.Category].Add(gameOption);
		OptionsByID.Add(gameOption.ID, gameOption);
	}

	private static GameOption LoadOptionNode(XmlDataHelper xml)
	{
		string text = xml.ParseAttribute<string>("ID", null, required: true);
		if (OptionsByID.TryGetValue(text, out var Option))
		{
			OptionsByCategory[Option.Category].Remove(Option);
			OptionsByID.Remove(text);
		}
		else
		{
			Option = new GameOption();
			Option.ID = text;
		}
		Option.DisplayText = xml.ParseAttribute("DisplayText", Option.DisplayText);
		Option.ConsoleDisplayText = xml.ParseAttribute("ConsoleDisplayText", Option.DisplayText);
		Option.SwitchDisplayText = xml.ParseAttribute("SwitchDisplayText", Option.ConsoleDisplayText);
		Option.Category = xml.ParseAttribute("Category", Option.Category ?? "No Category", required: true);
		Option.Requires = xml.ParseAttribute<GameOption.RequiresSpec>("Requires", null);
		Option.Type = xml.ParseAttribute("Type", Option.Type);
		Option.SearchKeywords = xml.ParseAttribute("SearchKeywords", Option.SearchKeywords);
		Option.RequiresCapability = xml.ParseAttribute("RequiresCapability", Option.RequiresCapability);
		Option.Default = CapabilityManager.instance.GetDefaultOptionOverrideForCapabilities(Option.ID, xml.ParseAttribute("Default", Option.Default));
		DefaultsByID[text] = Option.Default;
		if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console)
		{
			Option.DisplayText = Option.ConsoleDisplayText;
		}
		if (Option.Type == "Button")
		{
			Option.OnClick = xml.ParseAttribute<MethodInfo>("OnClick", null, required: true);
		}
		string attribute = xml.GetAttribute("Values");
		if (!attribute.IsNullOrEmpty())
		{
			if (xml.GetAttribute("Values") == "*Resolution")
			{
				HashSet<string> hashSet = new HashSet<string>(GameManager.resolutions.Count);
				foreach (Resolution resolution in GameManager.resolutions)
				{
					string item = resolution.width + "x" + resolution.height;
					hashSet.Add(item);
				}
				hashSet.Add("Screen");
				hashSet.Add("Unset");
				Option.DisplayValues = (Option.Values = hashSet.ToArray());
			}
			else
			{
				using ScopeDisposedList<string> scopeDisposedList = ScopeDisposedList<string>.GetFromPool();
				using ScopeDisposedList<string> scopeDisposedList2 = ScopeDisposedList<string>.GetFromPool();
				DelimitedEnumeratorChar enumerator2 = attribute.DelimitedBy(',').GetEnumerator();
				while (enumerator2.MoveNext())
				{
					enumerator2.Current.Split('|', out var First, out var Second);
					string item2 = new string(First);
					if (Second.Length != 0)
					{
						if (scopeDisposedList2.Count == 0)
						{
							scopeDisposedList2.AddRange(scopeDisposedList);
						}
						scopeDisposedList2.Add(new string(Second));
					}
					else if (scopeDisposedList2.Count > 0)
					{
						scopeDisposedList2.Add(item2);
					}
					scopeDisposedList.Add(item2);
				}
				Option.Values = scopeDisposedList.ToArray();
				Option.DisplayValues = ((scopeDisposedList2.Count == 0) ? Option.Values : scopeDisposedList2.ToArray());
			}
		}
		Option.Min = xml.GetAttributeInt("Min", Option.Min);
		Option.Max = xml.GetAttributeInt("Max", Option.Max);
		Option.Increment = xml.GetAttributeInt("Increment", Option.Increment);
		Option.Restart = xml.GetAttributeBool("Restart", defaultValue: false);
		xml.HandleNodes(new Dictionary<string, Action<XmlDataHelper>> { 
		{
			"helptext",
			(XmlDataHelper xmlDataHelper) =>
			{
				Option.HelpText = xmlDataHelper.GetTextNode();
			}
		} });
		if (Option.Restart)
		{
			string text2 = "This option requires a game restart to take effect.";
			if (Option.HelpText.IsNullOrEmpty())
			{
				Option.HelpText = text2;
			}
			else
			{
				GameOption gameOption = Option;
				gameOption.HelpText = gameOption.HelpText + "\n\n" + text2;
			}
		}
		return Option;
	}
}
