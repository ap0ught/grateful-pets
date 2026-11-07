using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;
using Occult.Engine.CodeGeneration;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
[HasGameBasedStaticCache]
[GenerateSerializationPartial]
public class Examiner : IPart
{
	public const string OWNER_ANGER_SUPPRESS = "DontWarnOnExamine";

	public const int EPISTEMIC_STATUS_UNINITIALIZED = -1;

	public const int EPISTEMIC_STATUS_UNKNOWN = 0;

	public const int EPISTEMIC_STATUS_PARTIAL = 1;

	public const int EPISTEMIC_STATUS_KNOWN = 2;

	public const int EXAMINER_KEEP_TILE = 1;

	public const int EXAMINER_INJECT_UNKNOWN_COLOR_STRING = 2;

	public const int EXAMINER_INJECT_UNKNOWN_TILE_COLOR = 4;

	public const int EXAMINER_INJECT_UNKNOWN_DETAIL_COLOR = 8;

	public const int EXAMINER_INJECT_ALTERNATE_COLOR_STRING = 16;

	public const int EXAMINER_INJECT_ALTERNATE_TILE_COLOR = 32;

	public const int EXAMINER_INJECT_ALTERNATE_DETAIL_COLOR = 64;

	public const int EXAMINER_INJECT_ANY_UNKNOWN = 14;

	public const int EXAMINER_INJECT_ANY_ALTERNATE = 112;

	public int Complexity;

	public int Difficulty;

	public string Alternate = "BaseUnknown";

	public string Unknown = "BaseUnknown";

	public int EpistemicStatus = -1;

	public int Flags;

	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	private static Dictionary<string, GameObject> Samples;

	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	private static Dictionary<string, GameObject> RandomSamples;

	[NonSerialized]
	[GameBasedStaticCache(true, false, CreateInstance = false)]
	public static Dictionary<string, int> UnderstandingTable;

	[JsonIgnore]
	public bool KeepTile
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	[JsonIgnore]
	public bool InjectUnknownColorString
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	[JsonIgnore]
	public bool InjectUnknownTileColor
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	[JsonIgnore]
	public bool InjectUnknownDetailColor
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	[JsonIgnore]
	public bool InjectAlternateColorString
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	[JsonIgnore]
	public bool InjectAlternateTileColor
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	[JsonIgnore]
	public bool InjectAlternateDetailColor
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	[JsonIgnore]
	public bool InjectAnyUnknown => (Flags & 0xE) != 0;

	[JsonIgnore]
	public bool InjectAnyAlternate => (Flags & 0x70) != 0;

	public int Understanding
	{
		get
		{
			return GetUnderstanding(ParentObject.GetTinkeringBlueprint());
		}
		set
		{
			SetUnderstanding(ParentObject.GetTinkeringBlueprint(), value);
		}
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(Complexity);
		Writer.WriteOptimized(Difficulty);
		Writer.WriteOptimized(Alternate);
		Writer.WriteOptimized(Unknown);
		Writer.WriteOptimized(EpistemicStatus);
		Writer.WriteOptimized(Flags);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Complexity = Reader.ReadOptimizedInt32();
		Difficulty = Reader.ReadOptimizedInt32();
		Alternate = Reader.ReadOptimizedString();
		Unknown = Reader.ReadOptimizedString();
		EpistemicStatus = Reader.ReadOptimizedInt32();
		Flags = Reader.ReadOptimizedInt32();
	}

	public static void LoadGlobals(SerializationReader Reader)
	{
		UnderstandingTable = Reader.ReadDictionary<string, int>();
	}

	public static void SaveGlobals(SerializationWriter Writer)
	{
		Writer.Write(UnderstandingTable);
	}

	public static void ResetGlobals()
	{
		UnderstandingTable = new Dictionary<string, int>();
	}

	public static int GetUnderstanding(string Blueprint)
	{
		if (Blueprint == null)
		{
			return 0;
		}
		if (UnderstandingTable == null)
		{
			ResetGlobals();
		}
		if (UnderstandingTable.TryGetValue(Blueprint, out var value))
		{
			return value;
		}
		return 0;
	}

	public static void SetUnderstanding(string Blueprint, int Level)
	{
		if (Blueprint == null)
		{
			return;
		}
		if (UnderstandingTable == null)
		{
			ResetGlobals();
		}
		if (UnderstandingTable.TryGetValue(Blueprint, out var value))
		{
			if (value < Level)
			{
				UnderstandingTable[Blueprint] = Level;
			}
		}
		else
		{
			UnderstandingTable.Add(Blueprint, Level);
		}
	}

	public override bool SameAs(IPart p)
	{
		Examiner examiner = p as Examiner;
		if (examiner.Complexity != Complexity)
		{
			return false;
		}
		if (examiner.Difficulty != Difficulty)
		{
			return false;
		}
		if (examiner.Alternate != Alternate)
		{
			return false;
		}
		if (examiner.Unknown != Unknown)
		{
			return false;
		}
		if (examiner.Flags != Flags)
		{
			return false;
		}
		if (examiner.EpistemicStatus != EpistemicStatus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void CopyFrom(Examiner source)
	{
		Complexity = source.Complexity;
		Difficulty = source.Difficulty;
		Alternate = source.Alternate;
		Unknown = source.Unknown;
		Flags = source.Flags;
		EpistemicStatus = source.EpistemicStatus;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != AfterObjectCreatedEvent.ID && ID != PooledEvent<AnimateEvent>.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != EnteredCellEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetGenderEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetInventoryCategoryEvent>.ID && ID != ObjectCreatedEvent.ID && ID != InventoryActionEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			if (Complexity > 0 && Options.AutogetArtifacts)
			{
				return ID == AutoexploreObjectEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (Complexity > 0 && E.Command == null && Options.AutogetArtifacts && ParentObject.CanAutoget())
		{
			E.Command = "Autoget";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Complexity", Complexity);
		E.AddEntry(this, "Difficulty", Difficulty);
		E.AddEntry(this, "Alternate", Alternate);
		E.AddEntry(this, "Unknown", Unknown);
		E.AddEntry(this, "EpistemicStatus", EpistemicStatus);
		E.AddEntry(this, "Flags", Flags);
		E.AddEntry(this, "KeepTile", KeepTile);
		E.AddEntry(this, "InjectUnknownColorString", InjectUnknownColorString);
		E.AddEntry(this, "InjectUnknownTileColor", InjectUnknownTileColor);
		E.AddEntry(this, "InjectUnknownDetailColor", InjectUnknownDetailColor);
		E.AddEntry(this, "InjectAlternateColorString", InjectAlternateColorString);
		E.AddEntry(this, "InjectAlternateTileColor", InjectAlternateTileColor);
		E.AddEntry(this, "InjectAlternateDetailColor", InjectAlternateDetailColor);
		E.AddEntry(this, "UnknownSample", GetUnknownSample());
		E.AddEntry(this, "AlternateSample", GetAlternateSample());
		E.AddEntry(this, "ActiveSample", GetActiveSample());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.AsIfKnown)
		{
			GetActiveSample()?.ReplaceDisplayName(E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		if (E.Object == ParentObject && GetEpistemicStatus() != 2)
		{
			SetEpistemicStatus(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		if (GetEpistemicStatus() != 2)
		{
			GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
			if (gameObject != null && gameObject.IsPlayer())
			{
				E.AdjustValue((GetEpistemicStatus() == 1) ? 0.2 : 0.1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetGenderEvent E)
	{
		if (!E.AsIfKnown)
		{
			GameObject activeSample = GetActiveSample();
			if (activeSample != null)
			{
				Gender gender = activeSample.GetGender();
				if (gender != null)
				{
					E.Name = gender.Name;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!ParentObject.Understood(this))
		{
			E.AddAction("Examine", "examine", "Examine", null, 'x');
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Examine")
		{
			if (IsBroken())
			{
				Popup.ShowFail("Whatever " + ParentObject.it + ParentObject.Is + ", " + ParentObject.itis + " broken...");
				return false;
			}
			if (!E.Actor.CanMoveExtremities("Examine", ShowMessage: true))
			{
				return false;
			}
			bool Interrupt = false;
			int num = E.Actor.Stat("Intelligence") - Difficulty;
			int totalConfusion = E.Actor.GetTotalConfusion();
			if (totalConfusion > 0)
			{
				num -= totalConfusion;
			}
			int intProperty = E.Actor.GetIntProperty("InspectorEquipped");
			intProperty = GetTinkeringBonusEvent.GetFor(E.Actor, ParentObject, "Inspect", num, intProperty, ref Interrupt);
			if (Interrupt)
			{
				return false;
			}
			num += intProperty;
			if (E.Actor.IsPlayer())
			{
				if (!string.IsNullOrEmpty(ParentObject.Owner) && !ParentObject.HasPropertyOrTag("DontWarnOnExamine") && Popup.ShowYesNoCancel(ParentObject.Does("are") + " not owned by you, and examining " + ParentObject.them + " risks damaging " + ParentObject.them + ". Are you sure you want to do so?") != DialogResult.Yes)
				{
					return false;
				}
				GameObject inInventory = ParentObject.InInventory;
				if (inInventory != null && inInventory != E.Actor && !string.IsNullOrEmpty(inInventory.Owner) && inInventory.Owner != ParentObject.Owner && !inInventory.HasPropertyOrTag("DontWarnOnExamine") && Popup.ShowYesNoCancel(inInventory.Does("are") + " not owned by you, and examining " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " inside " + inInventory.them + " risks causing damage. Are you sure you want to do so?") != DialogResult.Yes)
				{
					return false;
				}
			}
			if (Options.SifrahExamine && E.Actor.IsPlayer())
			{
				if (totalConfusion > 0)
				{
					Popup.ShowFail("You're too confused to do that.");
				}
				else
				{
					ExamineSifrah examineSifrah = new ExamineSifrah(ParentObject, Complexity, Difficulty, Understanding, num);
					examineSifrah.Play(ParentObject);
					if (examineSifrah.InterfaceExitRequested)
					{
						E.RequestInterfaceExit();
					}
				}
			}
			else
			{
				int result = Stat.RollResult(num);
				result = TutorialManager.AdjustExaminationRoll(result);
				if (result >= 10 || result > Complexity)
				{
					result = Complexity;
				}
				if (result <= 0 && !ParentObject.HasPropertyOrTag("CantBreakOnExamine"))
				{
					ResultCriticalFailure(E.Actor);
					E.RequestInterfaceExit();
					if (E.Actor.IsPlayer())
					{
						AutoAct.Interrupt();
					}
				}
				else if (totalConfusion > 0)
				{
					ResultFakeConfusionFailure(E.Actor);
					E.RequestInterfaceExit();
					if (E.Actor.IsPlayer())
					{
						AutoAct.Interrupt();
					}
				}
				else if (result > 0 && result > Understanding)
				{
					ResultPartialSuccess(E.Actor, result);
				}
				else
				{
					ResultFailure(E.Actor);
				}
			}
			E.Actor.UseEnergy(1000, "Examine");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryCategoryEvent E)
	{
		if (!ParentObject.Understood(this) && !E.AsIfKnown)
		{
			E.Category = "Artifacts";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && !ParentObject.Understood(this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (E.Actor.IsPlayer() && CheckEpistemicStatus() != 2)
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckEpistemicStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (InjectAnyUnknown)
		{
			GameObject unknownSample = GetUnknownSample();
			if (unknownSample != null)
			{
				if (InjectUnknownColorString)
				{
					ParentObject.Render.ColorString = unknownSample.Render.ColorString;
				}
				if (InjectUnknownTileColor)
				{
					ParentObject.Render.TileColor = unknownSample.Render.TileColor;
				}
				if (InjectUnknownDetailColor)
				{
					ParentObject.Render.DetailColor = unknownSample.Render.DetailColor;
				}
			}
		}
		if (InjectAnyAlternate)
		{
			GameObject alternateSample = GetAlternateSample();
			if (alternateSample != null)
			{
				if (InjectAlternateColorString)
				{
					ParentObject.Render.ColorString = alternateSample.Render.ColorString;
				}
				if (InjectAlternateTileColor)
				{
					ParentObject.Render.TileColor = alternateSample.Render.TileColor;
				}
				if (InjectAlternateDetailColor)
				{
					ParentObject.Render.DetailColor = alternateSample.Render.DetailColor;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		CheckEpistemicStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckEpistemicStatus();
		return base.HandleEvent(E);
	}

	public static int GetBlueprintComplexity(GameObjectBlueprint Blueprint)
	{
		return Blueprint?.GetPartParameter("Examiner", "Complexity", 0) ?? 0;
	}

	public static int GetBlueprintComplexity(string Blueprint)
	{
		return GetBlueprintComplexity(GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint));
	}

	public static int GetBlueprintEpistemicStatus(GameObjectBlueprint Blueprint)
	{
		if (Blueprint == null)
		{
			return 0;
		}
		int understanding = GetUnderstanding(Blueprint.Name);
		int blueprintComplexity = GetBlueprintComplexity(Blueprint);
		if (understanding >= blueprintComplexity)
		{
			return 2;
		}
		if (understanding > 0)
		{
			return 1;
		}
		return 0;
	}

	public static int GetBlueprintEpistemicStatus(string Blueprint)
	{
		return GetBlueprintEpistemicStatus(GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint));
	}

	public int GetBlueprintEpistemicStatus()
	{
		return GetBlueprintEpistemicStatus(ParentObject.GetTinkeringBlueprint());
	}

	public static int GetAppropriateEpistemicStatus(int Understanding, int Complexity, GameObject Object = null)
	{
		if (Understanding >= Complexity)
		{
			return 2;
		}
		if (GameObject.Validate(ref Object))
		{
			int scanEpistemicStatus = Scanning.GetScanEpistemicStatus(The.Player, Object);
			if (scanEpistemicStatus == 2 || scanEpistemicStatus == 1)
			{
				return scanEpistemicStatus;
			}
		}
		if (Understanding > 0)
		{
			return 1;
		}
		if (GameObject.Validate(ref Object) && Object.HasProperty("PartiallyUnderstood"))
		{
			return 1;
		}
		return 0;
	}

	public int GetAppropriateEpistemicStatus()
	{
		return GetAppropriateEpistemicStatus(Understanding, Complexity, ParentObject);
	}

	public override int GetEpistemicStatus()
	{
		if (EpistemicStatus == -1)
		{
			SetEpistemicStatus(GetAppropriateEpistemicStatus());
		}
		return EpistemicStatus;
	}

	public void SetEpistemicStatus(int status)
	{
		EpistemicStatus = status;
		if (status == 2 || KeepTile)
		{
			ParentObject.RemoveEffect<EpistemicDisguise>();
			return;
		}
		GameObject activeSample = GetActiveSample(status);
		if (activeSample == null)
		{
			ParentObject.RemoveEffect<EpistemicDisguise>();
		}
		else
		{
			EpistemicDisguise.ApplyTo(ParentObject, activeSample);
		}
	}

	public int CheckEpistemicStatus()
	{
		if (EpistemicStatus != 2)
		{
			SetEpistemicStatus(GetAppropriateEpistemicStatus());
		}
		return EpistemicStatus;
	}

	public static bool MakeBlueprintUnderstood(string Blueprint, int Understanding)
	{
		if (GetUnderstanding(Blueprint) > Understanding)
		{
			return false;
		}
		The.Game.BlueprintSeen(Blueprint);
		SetUnderstanding(Blueprint, Understanding);
		return true;
	}

	public bool MakeUnderstood(bool ShowMessage = false)
	{
		if (Understanding >= Complexity)
		{
			return false;
		}
		string priorDesc = (ShowMessage ? ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) : null);
		ParentObject.Seen();
		Understanding = Complexity;
		CheckEpistemicStatus();
		if (ShowMessage)
		{
			Popup.Show(GetIdentifyMessage(priorDesc), null, "sfx_artifact_examination_success_total");
		}
		return true;
	}

	public bool MakeUnderstood(out string Message)
	{
		Message = null;
		if (Understanding >= Complexity)
		{
			return false;
		}
		string priorDesc = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		ParentObject.Seen();
		Understanding = Complexity;
		CheckEpistemicStatus();
		Message = GetIdentifyMessage(priorDesc);
		return true;
	}

	public bool MakePartiallyUnderstood(bool ShowMessage = false)
	{
		if (Understanding >= Complexity)
		{
			return false;
		}
		string priorDesc = (ShowMessage ? ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) : null);
		ParentObject.Seen();
		Understanding = Complexity - 1;
		CheckEpistemicStatus();
		if (ShowMessage)
		{
			Popup.Show(GetIdentifyMessage(priorDesc), null, "sfx_artifact_examination_success_partial");
		}
		return true;
	}

	public bool MakePartiallyUnderstood(out string Message)
	{
		Message = null;
		if (Understanding >= Complexity)
		{
			return false;
		}
		string priorDesc = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		ParentObject.Seen();
		Understanding = Complexity - 1;
		CheckEpistemicStatus();
		Message = GetIdentifyMessage(priorDesc);
		return true;
	}

	public string GetIdentifyMessage(string PriorDesc)
	{
		string text = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		if (text == PriorDesc)
		{
			return "You commit the distinguishing characteristics of " + text + " to memory.";
		}
		return "You identify " + PriorDesc + " as " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".";
	}

	public void ResultSuccess(GameObject Actor)
	{
		Understanding = Complexity;
		if (Actor.IsPlayer())
		{
			Popup.Show("You now understand " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: false) + ".", null, "sfx_artifact_examination_success_total");
		}
		ExamineSuccessEvent.Send(ParentObject, Actor, Complete: true);
		CheckEpistemicStatus();
	}

	public void ResultExceptionalSuccess(GameObject Actor)
	{
		string text = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		bool flag = false;
		if (20.in100())
		{
			if (RelicGenerator.ApplyBasicBestowal(ParentObject, null, 1, null, Standard: false, ShowInShortDescription: true))
			{
				flag = true;
			}
			else if (ModificationFactory.ApplyModifications(ParentObject, ParentObject.GetBlueprint(), -999, 1, "Examination") > 0)
			{
				flag = true;
			}
		}
		else if (ModificationFactory.ApplyModifications(ParentObject, ParentObject.GetBlueprint(), -999, 1, "Examination") > 0)
		{
			flag = true;
		}
		else if (50.in100() && RelicGenerator.ApplyBasicBestowal(ParentObject, null, 1, null, Standard: false, ShowInShortDescription: true))
		{
			flag = true;
		}
		if (flag && Actor.IsPlayer())
		{
			Popup.Show("You discover something about " + text + " that was hidden!", null, "sfx_artifact_examination_success_total");
		}
		ResultSuccess(ParentObject);
		if (flag && Actor.IsPlayer())
		{
			InventoryActionEvent.Check(ParentObject, Actor, ParentObject, "Look");
		}
	}

	public void ResultPartialSuccess(GameObject Actor, int newLevel = -1)
	{
		if (newLevel == -1)
		{
			newLevel = Math.Min(Stat.Random(1, Complexity - 1), Complexity - 1);
		}
		int blueprintComplexity = GetBlueprintComplexity(ParentObject.GetBlueprint());
		bool flag = false;
		string text = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		string text2 = ParentObject.Does("seem", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true);
		if (newLevel < Complexity && newLevel >= blueprintComplexity && Understanding < blueprintComplexity)
		{
			GameObject gameObject = GameObject.CreateSample(ParentObject.GetTinkeringBlueprint());
			if (gameObject.TryGetPart<Examiner>(out var Part) && Part.Complexity > 0 && newLevel >= Part.Complexity && !gameObject.DisplayNameOnlyDirect.StartsWith("[") && !gameObject.HasTag("BaseObject"))
			{
				flag = true;
				if (newLevel > Understanding)
				{
					Understanding = newLevel;
				}
				if (Actor.IsPlayer())
				{
					string text3 = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
					string displayName = gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: false, BaseOnly: true);
					string text4 = gameObject.GetxTag("Grammar", "adjunctNoun");
					if (text4.IsNullOrEmpty())
					{
						text4 = (gameObject.IsPlural ? "set" : "one");
					}
					string message = ((!(text3 != text)) ? ("You make some progress understanding " + text + ". You think " + ParentObject.itis + " probably a variety of " + displayName + ", and you believe you would be able to recognize an ordinary " + text4 + " of " + ((gameObject.a == "some ") ? "that" : "those") + " now.") : ("You make some progress understanding " + text + ". " + text2 + " to be " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", and you think " + ParentObject.itis + " probably a variety of " + displayName + "; you believe you would be able to recognize an ordinary " + text4 + " of " + ((gameObject.a == "some ") ? "that" : "those") + " now."));
					Popup.Show(message, null, "sfx_artifact_examination_success_partial");
				}
			}
		}
		if (!flag)
		{
			if (newLevel > Understanding)
			{
				Understanding = newLevel;
			}
			if (Actor.IsPlayer())
			{
				string text5 = ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true);
				string message2;
				if (Understanding >= Complexity && text != text5)
				{
					message2 = GetIdentifyMessage(text);
				}
				else
				{
					message2 = ((!(text != text5)) ? ("You make some progress understanding " + text5 + ".") : ("You make some progress understanding " + text + ". " + text2 + " to be " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + "."));
				}
				Popup.Show(message2, null, (Understanding >= Complexity) ? "sfx_artifact_examination_success_total" : "sfx_artifact_examination_success_partial");
			}
		}
		if (Understanding >= Complexity)
		{
			ExamineSuccessEvent.Send(ParentObject, Actor, Complete: true);
			CheckEpistemicStatus();
		}
		else
		{
			ExamineSuccessEvent.Send(ParentObject, Actor);
		}
	}

	public void ResultFailure(GameObject Actor)
	{
		if (ExamineFailureEvent.Check(Actor, ParentObject))
		{
			if (Actor.IsPlayer())
			{
				Popup.Show("You are puzzled by " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: false) + ".", null, "sfx_artifact_examination_noProgress");
			}
			AfterExamineFailureEvent.Send(Actor, ParentObject);
		}
	}

	public void ResultFakeConfusionFailure(GameObject Actor)
	{
		if (ExamineFailureEvent.Check(Actor, ParentObject, ConfusionBased: true))
		{
			if (Actor.IsPlayer())
			{
				Popup.Show("You think you broke " + ParentObject.them + "...", null, "sfx_artifact_examination_fail");
			}
			AfterExamineFailureEvent.Send(Actor, ParentObject, ConfusionBased: true);
		}
	}

	public void ResultCriticalFailure(GameObject Actor)
	{
		if (!ExamineCriticalFailureEvent.Check(Actor, ParentObject))
		{
			return;
		}
		string message = "You think you broke " + ParentObject.them + "...";
		if (ParentObject.ApplyEffect(new Broken(FromDamage: false, FromExamine: true)) && ParentObject.IsBroken())
		{
			if (Actor.IsPlayer())
			{
				Popup.Show(message, null, "sfx_artifact_examination_fail");
			}
			if (ParentObject.IsValid())
			{
				ParentObject.PotentiallyAngerOwner(Actor, "DontWarnOnExamine");
			}
		}
		else if (Actor.IsPlayer())
		{
			Popup.Show("You are puzzled by " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: false) + ".", null, "sfx_artifact_examination_fail");
		}
		AfterExamineCriticalFailureEvent.Send(Actor, ParentObject);
	}

	[WishCommand(null, null)]
	public static void IDAllHere()
	{
		foreach (GameObject item in GetContentsEvent.GetFor(IComponent<GameObject>.ThePlayer.CurrentZone))
		{
			item.MakeUnderstood();
		}
	}

	[WishCommand(null, null)]
	public static void IDAllHerePartial()
	{
		foreach (GameObject item in GetContentsEvent.GetFor(IComponent<GameObject>.ThePlayer.CurrentZone))
		{
			item.MakePartiallyUnderstood();
		}
	}

	[WishCommand(null, null)]
	public static void IDAll()
	{
		foreach (GameObject item in GetContentsEvent.GetFor(IComponent<GameObject>.ThePlayer.CurrentZone))
		{
			item.MakeUnderstood();
		}
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (blueprint.HasPart("Examiner") && blueprint.GetPartParameter<string>("TinkerItem", "SubstituteBlueprint") == null)
			{
				UnderstandingTable[blueprint.Name] = blueprint.GetPartParameter("Examiner", "Complexity", 0) + 10;
			}
		}
	}

	[WishCommand(null, null)]
	public static void IDAllPartial()
	{
		foreach (GameObject item in GetContentsEvent.GetFor(IComponent<GameObject>.ThePlayer.CurrentZone))
		{
			item.MakePartiallyUnderstood();
		}
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (blueprint.HasPart("Examiner") && blueprint.GetPartParameter<string>("TinkerItem", "SubstituteBlueprint") == null)
			{
				UnderstandingTable[blueprint.Name] = blueprint.GetPartParameter("Examiner", "Complexity", 0) - 1;
			}
		}
	}

	private static GameObject GetSample(string SampleBlueprint)
	{
		if (SampleBlueprint == null)
		{
			return null;
		}
		if (Samples == null)
		{
			Samples = new Dictionary<string, GameObject>();
		}
		if (Samples.TryGetValue(SampleBlueprint, out var value))
		{
			return value;
		}
		GameObject gameObject = GameObject.CreateSample(SampleBlueprint);
		Samples[SampleBlueprint] = gameObject;
		return gameObject;
	}

	private GameObject GetFinalSample(string SampleBlueprint)
	{
		if (SampleBlueprint == null)
		{
			return null;
		}
		if (RandomSamples == null)
		{
			RandomSamples = new Dictionary<string, GameObject>();
		}
		if (RandomSamples.TryGetValue(ParentObject.Blueprint, out var value))
		{
			return value;
		}
		GameObject sample = GetSample(SampleBlueprint);
		if (sample != null)
		{
			string tag = sample.GetTag("ExaminerRandom");
			if (!tag.IsNullOrEmpty() && !ParentObject.GetBlueprint().IsBaseBlueprint())
			{
				string state = "ExaminerRandomSelectionFor" + ParentObject.Blueprint;
				string text = The.Game?.GetStringGameState(state);
				if (!text.IsNullOrEmpty())
				{
					sample = GetSample(text);
					RandomSamples[ParentObject.Blueprint] = sample;
				}
				else
				{
					List<string> list = new List<string>();
					foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
					{
						if (blueprint.HasTag(tag))
						{
							string usageKey = GetUsageKey(blueprint.Name, SampleBlueprint);
							XRLGame game = The.Game;
							if (game == null || !game.GetBooleanGameState(usageKey))
							{
								list.Add(blueprint.Name);
							}
						}
					}
					string randomElement = list.GetRandomElement();
					if (randomElement == null)
					{
						MetricsManager.LogError("unable to find an unused blueprint tagged " + tag + " for " + ParentObject.Blueprint + ", using " + SampleBlueprint);
					}
					else
					{
						The.Game?.SetStringGameState(state, randomElement);
						string usageKey2 = GetUsageKey(randomElement, SampleBlueprint);
						The.Game?.SetBooleanGameState(usageKey2, Value: true);
						sample = GetSample(randomElement);
						RandomSamples[ParentObject.Blueprint] = sample;
					}
				}
			}
		}
		return sample;
	}

	private static string GetUsageKey(string Blueprint, string SampleBlueprint)
	{
		return "ExaminerRandomUsed" + Blueprint + "For" + SampleBlueprint;
	}

	public GameObject GetUnknownSample()
	{
		return GetFinalSample(Unknown);
	}

	public GameObject GetAlternateSample()
	{
		return GetFinalSample(Alternate);
	}

	public GameObject GetActiveSample(int EpistemicStatus)
	{
		return EpistemicStatus switch
		{
			0 => GetUnknownSample(), 
			1 => GetAlternateSample(), 
			_ => null, 
		};
	}

	public GameObject GetActiveSample()
	{
		return GetActiveSample(CheckEpistemicStatus());
	}

	public string GetActiveShortDescription(int EpistemicStatus)
	{
		GameObject activeSample = GetActiveSample(EpistemicStatus);
		if (activeSample != null)
		{
			Description part = activeSample.GetPart<Description>();
			if (part != null)
			{
				return part._Short;
			}
		}
		return null;
	}

	public string GetActiveShortDescription()
	{
		return GetActiveShortDescription(CheckEpistemicStatus());
	}
}
