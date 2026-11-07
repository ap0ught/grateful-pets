using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using XRL.Collections;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.Wish;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
[HasModSensitiveStaticCache]
public class Mutations : IPart
{
	[Serializable]
	public class MutationModifierTracker : IComposite
	{
		[Serializable]
		public enum SourceType
		{
			Unknown,
			External,
			StatMod,
			Equipment,
			Cooking,
			Tonic
		}

		public Guid id;

		public int bonus;

		public string mutationName;

		public SourceType sourceType;

		public string sourceName;

		public bool WantFieldReflection => false;

		public MutationModifierTracker()
		{
		}

		public MutationModifierTracker(MutationModifierTracker Source, bool CopyID = false)
		{
			id = (CopyID ? Source.id : Guid.NewGuid());
			bonus = Source.bonus;
			mutationName = Source.mutationName;
			sourceType = Source.sourceType;
			sourceName = Source.sourceName;
		}

		public void Write(SerializationWriter Writer)
		{
			Writer.Write(id);
			Writer.WriteOptimized(bonus);
			Writer.WriteOptimized(mutationName);
			Writer.WriteOptimized((int)sourceType);
			Writer.WriteOptimized(sourceName);
		}

		public void Read(SerializationReader Reader)
		{
			id = Reader.ReadGuid();
			bonus = Reader.ReadOptimizedInt32();
			mutationName = Reader.ReadOptimizedString();
			sourceType = (SourceType)Reader.ReadOptimizedInt32();
			sourceName = Reader.ReadOptimizedString();
		}
	}

	[NonSerialized]
	private static StringMap<List<string>> MutationVariants = new StringMap<List<string>>(32);

	[NonSerialized]
	private static List<GameObjectBlueprint> MutationEquipment = new List<GameObjectBlueprint>(32);

	[NonSerialized]
	private static StringMap<BaseMutation> GenericMutations = new StringMap<BaseMutation>(32);

	[NonSerialized]
	public List<BaseMutation> MutationList = new List<BaseMutation>();

	[NonSerialized]
	public List<MutationModifierTracker> MutationMods = new List<MutationModifierTracker>();

	private List<string> FinalizeMutations;

	private int SyncAttempts;

	private bool RestartSync;

	[NonSerialized]
	private static List<MutationEntry> _pool = new List<MutationEntry>();

	public List<BaseMutation> ActiveMutationList => MutationList.Where((BaseMutation m) => m.Level > 0).ToList();

	[ModSensitiveCacheInit]
	public static void ResetCaches()
	{
		MutationEquipment.Clear();
	}

	public static List<string> GetVariants(string Mutation)
	{
		if (!MutationVariants.TryGetValue(Mutation, out var Value))
		{
			BaseMutation genericMutation = GetGenericMutation(Mutation);
			StringMap<List<string>> mutationVariants = MutationVariants;
			List<string> list = genericMutation.CreateVariants() ?? new List<string>();
			Value = list;
			mutationVariants[Mutation] = list;
		}
		return Value;
	}

	public static List<GameObjectBlueprint> GetAllMutationEquipment()
	{
		if (MutationEquipment.IsNullOrEmpty())
		{
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.Tags.ContainsKey("MutationEquipment"))
				{
					MutationEquipment.Add(blueprint);
				}
			}
		}
		return MutationEquipment;
	}

	public static BaseMutation GetGenericMutation(string Mutation, string Variant = null, GameObject Actor = null)
	{
		BaseMutation Value = Actor?.GetPart(Mutation) as BaseMutation;
		if (Value != null)
		{
			return Value;
		}
		int length = Mutation.Length;
		int num = ((!Variant.IsNullOrEmpty()) ? (Variant.Length + 1) : 0);
		Span<char> span = stackalloc char[length + num];
		Mutation.CopyTo(0, span, 0, length);
		if (num != 0)
		{
			span[length] = '.';
			Variant.CopyTo(0, span, length + 1, num - 1);
		}
		if (GenericMutations.TryGetValue(span, out Value))
		{
			return Value;
		}
		Type type = ModManager.ResolveType("XRL.World.Parts.Mutation." + Mutation);
		if ((object)type == null)
		{
			UnityEngine.Debug.LogWarning("Cannot resolve mutation type for " + Mutation);
			return null;
		}
		return GenericMutations[span] = BaseMutation.Create(type, Variant);
	}

	public override void FinalizeCopyEarly(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopyEarly(Source, CopyEffects, CopyID, MapInv);
		Mutations part = Source.GetPart<Mutations>();
		if (part == null)
		{
			return;
		}
		foreach (BaseMutation mutation in part.MutationList)
		{
			if (ParentObject.GetPart(mutation.Name) is BaseMutation item)
			{
				MutationList.Add(item);
			}
		}
		foreach (MutationModifierTracker mutationMod in part.MutationMods)
		{
			if (ParentObject.HasPart(mutationMod.mutationName))
			{
				MutationMods.Add(new MutationModifierTracker(mutationMod, CopyID | CopyEffects));
			}
		}
	}

	public override void FinalizeCopyLate(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopyLate(Source, CopyEffects, CopyID, MapInv);
		if (CopyID)
		{
			return;
		}
		if (CopyEffects)
		{
			foreach (MutationModifierTracker item in new List<MutationModifierTracker>(MutationMods))
			{
				if (item.sourceType != MutationModifierTracker.SourceType.StatMod && item.sourceType != MutationModifierTracker.SourceType.Equipment)
				{
					RemoveMutationMod(item.id);
				}
			}
			return;
		}
		foreach (MutationModifierTracker item2 in new List<MutationModifierTracker>(MutationMods))
		{
			RemoveMutationMod(item2.id);
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.Write(MutationList.Count);
		foreach (BaseMutation mutation in MutationList)
		{
			try
			{
				Writer.Write(mutation.Name);
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception serializing mutation " + mutation.Name + " : " + ex.ToString());
			}
		}
		Writer.Write(MutationMods);
		base.Write(Basis, Writer);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		MutationList.Clear();
		if (num > 0)
		{
			FinalizeMutations = new List<string>();
		}
		for (int i = 0; i < num; i++)
		{
			FinalizeMutations.Add(Reader.ReadString());
		}
		MutationMods = Reader.ReadList<MutationModifierTracker>();
		base.Read(Basis, Reader);
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		if (FinalizeMutations == null)
		{
			return;
		}
		if (MutationList == null)
		{
			MutationList = new List<BaseMutation>();
		}
		foreach (string finalizeMutation in FinalizeMutations)
		{
			BaseMutation baseMutation = (BaseMutation)ParentObject.GetPart(finalizeMutation);
			if (baseMutation != null)
			{
				MutationList.Add(baseMutation);
			}
		}
		FinalizeMutations = null;
		base.FinalizeRead(Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public BaseMutation GetMutation(string MutationName)
	{
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == MutationName)
				{
					return MutationList[i];
				}
			}
		}
		return null;
	}

	public BaseMutation GetMutationByName(string Name)
	{
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == Name)
				{
					return MutationList[i];
				}
			}
		}
		return null;
	}

	public bool HasMutation(string MutationName)
	{
		if (MutationName == "Chimera")
		{
			return ParentObject.IsChimera();
		}
		if (MutationName == "Esper")
		{
			return ParentObject.IsEsper();
		}
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == MutationName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMutation(BaseMutation CheckedMutation)
	{
		if (CheckedMutation == null)
		{
			return false;
		}
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == CheckedMutation.Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Guid AddMutationMod(string Mutation, string Variant = null, int Level = 1, MutationModifierTracker.SourceType SourceType = MutationModifierTracker.SourceType.Unknown, string SourceName = null)
	{
		BaseMutation genericMutation = GetGenericMutation(Mutation, Variant);
		if (genericMutation != null)
		{
			return AddMutationMod(genericMutation, Variant, Level, SourceType, SourceName);
		}
		return Guid.Empty;
	}

	public Guid AddMutationMod(MutationEntry Mutation, string Variant, int Level = 1, MutationModifierTracker.SourceType SourceType = MutationModifierTracker.SourceType.Unknown, string SourceName = null)
	{
		return AddMutationMod(Mutation.Class, Variant, Level, SourceType, SourceName);
	}

	public Guid AddMutationMod(MutationEntry Mutation, int Level = 1, MutationModifierTracker.SourceType SourceType = MutationModifierTracker.SourceType.Unknown, string SourceName = null)
	{
		return AddMutationMod(Mutation.Class, Mutation.Variant, Level, SourceType, SourceName);
	}

	public Guid AddMutationMod(Type Mutation, string Variant = null, int Level = 1, MutationModifierTracker.SourceType SourceType = MutationModifierTracker.SourceType.Unknown, string SourceName = null)
	{
		return AddMutationMod(Mutation.Name, Variant, Level, SourceType, SourceName);
	}

	public Guid AddMutationMod(BaseMutation Mutation, string Variant = null, int Level = 1, MutationModifierTracker.SourceType SourceType = MutationModifierTracker.SourceType.Unknown, string SourceName = null)
	{
		Guid empty = Guid.Empty;
		Type type = Mutation.GetType();
		if (!ParentObject.HasPart(type))
		{
			if (!Mutation.CompatibleWith(ParentObject))
			{
				return Guid.Empty;
			}
			empty = AddTracker(Mutation.Name, Level, SourceType, SourceName);
			BaseMutation baseMutation = BaseMutation.Create(type, Variant);
			baseMutation.ParentObject = ParentObject;
			if (baseMutation.Mutate(ParentObject, 0))
			{
				ParentObject.AddPart(baseMutation);
				baseMutation.AfterMutate();
			}
		}
		else
		{
			empty = AddTracker(Mutation.Name, Level, SourceType, SourceName);
		}
		ParentObject.SyncMutationLevelAndGlimmer();
		return empty;
	}

	private Guid AddTracker(string Name, int Level = 1, MutationModifierTracker.SourceType SourceType = MutationModifierTracker.SourceType.Unknown, string SourceName = null)
	{
		MutationModifierTracker mutationModifierTracker = new MutationModifierTracker
		{
			id = Guid.NewGuid(),
			mutationName = Name,
			sourceType = SourceType,
			sourceName = SourceName,
			bonus = Level
		};
		MutationMods.Add(mutationModifierTracker);
		return mutationModifierTracker.id;
	}

	public int GetLevelAdjustmentsForMutation(string className)
	{
		return MutationMods.Where((MutationModifierTracker m) => m.mutationName == className).Aggregate(0, (int a, MutationModifierTracker b) => a + b.bonus);
	}

	public void RemoveMutationMod(Guid id)
	{
		MutationModifierTracker tracker = MutationMods.Find((MutationModifierTracker m) => m.id == id);
		if (tracker != null)
		{
			MutationMods.Remove(tracker);
			if (!MutationList.Exists((BaseMutation m) => m.Name == tracker.mutationName) && !MutationMods.Exists((MutationModifierTracker m) => m.mutationName == tracker.mutationName))
			{
				BaseMutation baseMutation = ParentObject.GetPart(tracker.mutationName) as BaseMutation;
				if (baseMutation.Unmutate(ParentObject))
				{
					ParentObject.RemovePart(baseMutation);
					baseMutation.AfterUnmutate(ParentObject);
				}
			}
		}
		ParentObject.SyncMutationLevelAndGlimmer();
	}

	public int AddMutation(string NewMutationClass, int Level = 1)
	{
		return AddMutation(BaseMutation.Create(NewMutationClass), Level);
	}

	public int AddMutation(string Class, string Variant, int Level = 1)
	{
		return AddMutation(BaseMutation.Create(Class, Variant), Level);
	}

	public int AddMutation(MutationEntry NewMutation, int Level = 1)
	{
		return AddMutation(NewMutation.CreateInstance(), Level);
	}

	public int AddMutation(BaseMutation NewMutation, int Level = 1, bool Sync = true)
	{
		if (NewMutation == null)
		{
			return -1;
		}
		if (MutationList == null)
		{
			MutationList = new List<BaseMutation>();
		}
		if (NewMutation.Name != null)
		{
			if (HasMutation(NewMutation.Name))
			{
				MutationEntry mutationEntry = NewMutation.GetMutationEntry();
				if (mutationEntry != null && mutationEntry.Ranked)
				{
					BaseMutation mutation = GetMutation(NewMutation.Name);
					(mutation as IRankedMutation).AdjustRank(1);
					return MutationList.IndexOf(mutation);
				}
			}
			else
			{
				if (NewMutation.Variant.IsNullOrEmpty())
				{
					List<string> variants = NewMutation.GetVariants();
					if (!variants.IsNullOrEmpty())
					{
						NewMutation.SetVariant(variants.GetRandomElement());
					}
				}
				if (ParentObject.HasPart(NewMutation.Name))
				{
					if (ParentObject.GetPart(NewMutation.Name) is BaseMutation baseMutation)
					{
						baseMutation.Unmutate(ParentObject);
						ParentObject.RemovePart(baseMutation);
						baseMutation.AfterUnmutate(ParentObject);
					}
					else
					{
						MetricsManager.LogError("Could not retrieve " + NewMutation.Name + " part as BaseMutation from " + ParentObject.DebugName + ", using stopgap unmutate procedure");
						ParentObject.RemovePart(NewMutation.Name);
						NewMutation.AfterUnmutate(ParentObject);
					}
				}
				NewMutation.ParentObject = ParentObject;
				if (NewMutation.Mutate(ParentObject, Level))
				{
					if (ParentObject.HasRegisteredEvent("BeforeMutationAdded"))
					{
						ParentObject.FireEvent(Event.New("BeforeMutationAdded", "Object", ParentObject, "Mutation", NewMutation.GetMutationClass()));
					}
					ParentObject.AddPart(NewMutation);
					MutationList.Add(NewMutation);
					try
					{
						NewMutation.AfterMutate();
						if (ParentObject.HasRegisteredEvent("MutationAdded"))
						{
							ParentObject.FireEvent(Event.New("MutationAdded", "Object", ParentObject, "Mutation", NewMutation.GetMutationClass()));
						}
					}
					catch (Exception message)
					{
						MetricsManager.LogError(message);
					}
					if (Sync)
					{
						ParentObject.SyncMutationLevelAndGlimmer();
					}
					if (ParentObject != null && ParentObject.IsPlayer() && MutationList.Count >= 10)
					{
						Achievement.HAVE_10_MUTATIONS.Unlock();
					}
					return MutationList.Count - 1;
				}
			}
		}
		return -1;
	}

	public BodyPart CheckAddChimericBodyPart(bool Silent = false)
	{
		if (ParentObject != null && ParentObject.CurrentCell != null && ParentObject.IsChimera() && GlobalConfig.GetIntSetting("ChimericBodyPartChance").in100())
		{
			return AddChimericBodyPart(Silent);
		}
		return null;
	}

	public BodyPart AddChimericBodyPart(bool Silent = false, string Manager = "Chimera", BodyPart AttachAt = null)
	{
		BodyPartType randomBodyPartType = Anatomies.GetRandomBodyPartType(IncludeVariants: true, true, false, RequireLiveCategory: true, null, null, UseChimeraWeight: true);
		if (randomBodyPartType == null)
		{
			MetricsManager.LogWarning("could not generate a random body part type");
			return null;
		}
		bool flag = GlobalConfig.GetIntSetting("ChimericBodyPartStandardChance").in100();
		if (AttachAt == null)
		{
			AttachAt = GetChimericBodyPartAttachmentPoint(ParentObject, randomBodyPartType, flag);
		}
		if (AttachAt == null)
		{
			MetricsManager.LogWarning("could not find " + (flag ? "standard" : "random") + " attachment point for " + randomBodyPartType.FinalType);
			return null;
		}
		Body parentBody = AttachAt.ParentBody;
		bool? dynamic = true;
		BodyPart bodyPart = new BodyPart(randomBodyPartType, parentBody, null, null, null, null, null, null, null, null, null, null, null, null, null, null, dynamic)
		{
			Manager = Manager
		};
		List<BodyPartType> list = Anatomies.FindUsualChildBodyPartTypes(randomBodyPartType);
		if (list != null)
		{
			foreach (BodyPartType item in list)
			{
				bodyPart.AddPart(new BodyPart(item, bodyPart.ParentBody, null, null, null, null, null, Manager));
			}
		}
		PlayWorldSound("sfx_characterMod_limb_acquire");
		if (!Silent)
		{
			string text;
			if (bodyPart.Mass)
			{
				text = "Some " + bodyPart.Name;
			}
			else
			{
				text = ((!bodyPart.Plural) ? Grammar.A(bodyPart.Name, Capitalize: true) : ("A set of " + bodyPart.Name));
			}
			EmitMessage(text + " grows out of " + (ParentObject.IsPlayer() ? "your" : Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName)) + " " + AttachAt.GetOrdinalName() + "!", ' ', FromDialog: true);
		}
		if (bodyPart.Laterality == 0)
		{
			if (!string.IsNullOrEmpty(randomBodyPartType.UsuallyOn) && randomBodyPartType.UsuallyOn != AttachAt.Type)
			{
				BodyPartType bodyPartType = AttachAt.VariantTypeModel();
				bodyPart.ModifyNameAndDescriptionRecursively(bodyPartType.Name.Replace(" ", "-"), bodyPartType.Description.Replace(" ", "-"));
			}
			if (AttachAt.Laterality != 0)
			{
				bodyPart.ChangeLaterality(AttachAt.Laterality | bodyPart.Laterality, Recursive: true);
			}
		}
		AttachAt.AddPart(bodyPart, bodyPart.Type, new string[2] { "Thrown Weapon", "Floating Nearby" });
		GlobalConfig.GetIntSetting("ChimericBodyPartMirrorChance").in100();
		return bodyPart;
	}

	public static BodyPart GetChimericBodyPartAttachmentPoint(GameObject Subject, BodyPartType NewType, bool Standard = true)
	{
		if (Subject == null)
		{
			return null;
		}
		Body body = Subject.Body;
		if (body == null)
		{
			return null;
		}
		if (Standard)
		{
			if (string.IsNullOrEmpty(NewType.UsuallyOn))
			{
				return body.GetBody();
			}
		}
		else if (GlobalConfig.GetIntSetting("ChimericBodyPartRandomFromBodyChance").in100())
		{
			return body.GetBody();
		}
		List<BodyPart> list = new List<BodyPart>();
		List<BodyPart> list2 = new List<BodyPart>();
		foreach (BodyPart part in body.GetParts())
		{
			if (part.Abstract || !part.Contact || part.Extrinsic || !string.IsNullOrEmpty(part.DependsOn) || !string.IsNullOrEmpty(part.RequiresType) || !BodyPartCategory.IsLiveCategory(part.Category))
			{
				continue;
			}
			if (Standard)
			{
				if (NewType.UsuallyOn == part.Type)
				{
					list2.Add(part);
					if (string.IsNullOrEmpty(NewType.UsuallyOnVariant) || NewType.UsuallyOnVariant == part.VariantType)
					{
						list.Add(part);
					}
				}
			}
			else
			{
				list.Add(part);
			}
		}
		return list.GetRandomElement() ?? list2.GetRandomElement() ?? body.GetBody();
	}

	public void RemoveMutation(BaseMutation Mutation, bool Sync = true)
	{
		if (MutationMods.Any((MutationModifierTracker x) => x.mutationName == Mutation.Name))
		{
			MutationList.Remove(Mutation);
			Mutation.BaseLevel = 0;
			if (Sync)
			{
				ParentObject.SyncMutationLevelAndGlimmer();
			}
		}
		else if (Mutation.Unmutate(ParentObject))
		{
			ParentObject.RemovePart(Mutation);
			MutationList.Remove(Mutation);
			Mutation.AfterUnmutate(ParentObject);
		}
	}

	public void LevelMutation(BaseMutation Mutation, int Level)
	{
		Mutation.BaseLevel = Level;
		Mutation.ChangeLevel(Mutation.Level);
		ParentObject.SyncMutationLevelAndGlimmer();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (MutationList != null)
		{
			foreach (BaseMutation mutation in MutationList)
			{
				stringBuilder.Append(mutation.GetDisplayName() + " (" + mutation.Level + ")\n");
			}
		}
		return stringBuilder.ToString();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<StatChangeEvent>.ID)
		{
			return ID == PooledEvent<SyncMutationLevelsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (MutationList != null && MutationFactory.StatsUsedByMutations.Contains(E.Name))
		{
			bool flag = false;
			foreach (BaseMutation mutation in MutationList)
			{
				if (!(mutation.GetStat() == E.Name))
				{
					continue;
				}
				int level = mutation.Level;
				if (level != mutation.LastLevel)
				{
					if (level > 0 && mutation.LastLevel <= 0)
					{
						mutation.Mutate(mutation.ParentObject, mutation.BaseLevel);
					}
					else if (level <= 0 && mutation.LastLevel > 0)
					{
						mutation.Unmutate(mutation.ParentObject);
					}
					else
					{
						mutation.ChangeLevel(mutation.Level);
					}
					flag = true;
				}
			}
			if (flag)
			{
				ParentObject.SyncMutationLevelAndGlimmer();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SyncMutationLevelsEvent E)
	{
		if (MutationList != null)
		{
			if (SyncAttempts > 0)
			{
				if (SyncAttempts > 100)
				{
					MetricsManager.LogError("Too many mutation sync attempts", new StackTrace());
				}
				else
				{
					RestartSync = true;
				}
			}
			else
			{
				try
				{
					bool flag = false;
					while (true)
					{
						IL_0041:
						SyncAttempts++;
						RestartSync = false;
						foreach (BaseMutation mutation in MutationList)
						{
							if (SyncMutation(mutation))
							{
								flag = true;
							}
							if (RestartSync)
							{
								goto IL_0041;
							}
						}
						List<string> list = null;
						foreach (MutationModifierTracker tracker in MutationMods)
						{
							if (!MutationList.Exists((BaseMutation m) => m.Name == tracker.mutationName))
							{
								if (list == null)
								{
									list = new List<string>();
								}
								else if (list.Contains(tracker.mutationName))
								{
									continue;
								}
								list.Add(tracker.mutationName);
							}
						}
						if (list == null)
						{
							break;
						}
						foreach (string item in list)
						{
							if (SyncMutation(ParentObject.GetPart(item) as BaseMutation))
							{
								flag = true;
							}
							if (RestartSync)
							{
								goto IL_0041;
							}
						}
						break;
					}
					if (flag)
					{
						GlimmerChangeEvent.Send(ParentObject);
					}
				}
				finally
				{
					SyncAttempts = 0;
					RestartSync = false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool SyncMutation(BaseMutation mutation, bool SyncGlimmer = false)
	{
		if (mutation?.ParentObject == null)
		{
			return false;
		}
		int level = mutation.Level;
		if (level != mutation.LastLevel)
		{
			if (level <= 0 && mutation.LastLevel > 0)
			{
				mutation.Unmutate(mutation.ParentObject);
			}
			else
			{
				mutation.ChangeLevel(mutation.Level);
			}
			if (SyncGlimmer)
			{
				GlimmerChangeEvent.Send(mutation.ParentObject);
			}
			return true;
		}
		return false;
	}

	public bool IncludedInMutatePool(MutationEntry Entry, bool AllowMultipleDefects = false)
	{
		return IncludedInMutatePool(ParentObject, MutationList, Entry, AllowMultipleDefects);
	}

	private static bool IncludedInMutatePool(GameObject who, List<BaseMutation> CurrentMutations, MutationEntry entry, bool allowMultipleDefects = false)
	{
		if (entry != null && entry.ExcludeFromPool)
		{
			return false;
		}
		if (who.Property.TryGetValue("MutationLevel", out var value) && !string.IsNullOrEmpty(value) && !MutationFactory.GetMutationEntryByName(value).OkWith(entry, CheckOther: true, allowMultipleDefects))
		{
			return false;
		}
		if (CurrentMutations != null)
		{
			foreach (BaseMutation CurrentMutation in CurrentMutations)
			{
				if (CurrentMutation.GetMutationEntry() == entry && (entry == null || !entry.Ranked))
				{
					return false;
				}
				if (!entry.OkWith(CurrentMutation.GetMutationEntry(), CheckOther: true, allowMultipleDefects))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static List<MutationEntry> GetMutatePool(GameObject who, List<BaseMutation> CurrentMutations = null, Predicate<MutationEntry> filter = null, bool allowMultipleDefects = false)
	{
		_pool.Clear();
		if (CurrentMutations == null)
		{
			Mutations part = who.GetPart<Mutations>();
			if (part != null)
			{
				CurrentMutations = part.MutationList;
			}
		}
		if (filter == null)
		{
			filter = (MutationEntry x) => !x.Defect;
		}
		foreach (MutationCategory category in MutationFactory.GetCategories())
		{
			foreach (MutationEntry entry in category.Entries)
			{
				if (IncludedInMutatePool(who, CurrentMutations, entry, allowMultipleDefects) && filter(entry))
				{
					_pool.Add(entry);
				}
			}
		}
		return _pool;
	}

	public List<MutationEntry> GetMutatePool(Predicate<MutationEntry> filter = null, bool allowMultipleDefects = false)
	{
		return GetMutatePool(ParentObject, MutationList, filter, allowMultipleDefects);
	}

	[WishCommand("chimericpart", null)]
	public static bool HandleChimericPartWish()
	{
		IComponent<GameObject>.ThePlayer.RequirePart<Mutations>().AddChimericBodyPart();
		return true;
	}

	[WishCommand(null, null, Command = "mutation")]
	public static void WishMutation(string argument)
	{
		MutationEntry mutationEntry = null;
		int num = int.MaxValue;
		foreach (MutationEntry item in MutationFactory.AllMutationEntries())
		{
			if (item.Name.EqualsNoCase(argument))
			{
				WishMutationAdd(item.Class, item.Variant);
				return;
			}
			int num2 = Grammar.LevenshteinDistance(argument, item.Name);
			if (num2 < num)
			{
				mutationEntry = item;
				num = num2;
			}
		}
		if (mutationEntry != null)
		{
			int num3 = Math.Max(mutationEntry.Name.Length, argument.Length);
			if ((float)(num3 - num) * 1f / (float)num3 > 0.75f && Popup.ShowYesNo("Did you mean " + mutationEntry.Name + "?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) == DialogResult.Yes)
			{
				WishMutationAdd(mutationEntry.Class, mutationEntry.Variant);
				return;
			}
		}
		argument.Split(':', out var First, out var Second);
		foreach (MutationEntry item2 in MutationFactory.AllMutationEntries())
		{
			if (!item2.Class.EqualsNoCase(First))
			{
				continue;
			}
			if (Second.IsNullOrEmpty())
			{
				WishMutationAdd(item2.Class);
				return;
			}
			foreach (string variant in item2.GetVariants())
			{
				if (variant.EndsWith(Second, StringComparison.OrdinalIgnoreCase))
				{
					WishMutationAdd(item2.Class, variant);
					return;
				}
			}
		}
		foreach (Type item3 in ModManager.GetTypesAssignableFrom(typeof(BaseMutation), Cache: false))
		{
			if (item3.IsAbstract)
			{
				continue;
			}
			BaseMutation genericMutation = GetGenericMutation(item3.Name);
			if (!genericMutation.GetDisplayName().EqualsNoCase(First) && !genericMutation.Name.EqualsNoCase(First))
			{
				continue;
			}
			if (Second.IsNullOrEmpty())
			{
				WishMutationAdd(genericMutation.Name);
				return;
			}
			foreach (string variant2 in genericMutation.GetVariants())
			{
				if (variant2.EndsWith(Second, StringComparison.OrdinalIgnoreCase))
				{
					WishMutationAdd(genericMutation.Name, variant2);
					return;
				}
			}
		}
		Popup.Show(Second.IsNullOrEmpty() ? ("No mutation by the name '" + First + "' could be found.") : ("No mutation by the name '" + First + "' and variant '" + Second + "' could be found."));
	}

	public static void WishMutationAdd(string Class, string Variant = null)
	{
		BaseMutation baseMutation = BaseMutation.Create(Class, Variant);
		if (Variant.IsNullOrEmpty() && baseMutation.HasVariants)
		{
			baseMutation.SelectVariant(The.Player, AllowEscape: false);
		}
		The.Player.RequirePart<Mutations>().AddMutation(baseMutation);
		IComponent<GameObject>.XDidY(IComponent<GameObject>.ThePlayer, "gain", "the mutation " + baseMutation.GetDisplayName(WithAnnotations: false), "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
	}
}
