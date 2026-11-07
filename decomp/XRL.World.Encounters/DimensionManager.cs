using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Skills;

namespace XRL.World.Encounters;

[Serializable]
public class DimensionManager : IComposite
{
	public static readonly int GLIMMER_FLOOR = 20;

	public static readonly int GLIMMER_EXTRADIMENSIONAL_FLOOR = 40;

	public static readonly int NUM_EXTRA_DIMENSIONS = 8;

	public List<PsychicFaction> PsychicFactions = new List<PsychicFaction>();

	public List<ExtraDimension> ExtraDimensions = new List<ExtraDimension>();

	public bool WantFieldReflection => false;

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(PsychicFactions.Count);
		foreach (PsychicFaction psychicFaction in PsychicFactions)
		{
			Writer.WriteComposite(psychicFaction);
		}
		Writer.WriteOptimized(ExtraDimensions.Count);
		foreach (ExtraDimension extraDimension in ExtraDimensions)
		{
			Writer.WriteComposite(extraDimension);
		}
	}

	public void Read(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		PsychicFactions.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			PsychicFactions.Add(Reader.ReadComposite<PsychicFaction>());
		}
		num = Reader.ReadOptimizedInt32();
		ExtraDimensions.EnsureCapacity(num);
		for (int j = 0; j < num; j++)
		{
			ExtraDimensions.Add(Reader.ReadComposite<ExtraDimension>());
		}
	}

	public static void Init()
	{
		DimensionManager dimensionManager = new DimensionManager();
		for (int i = 1; i < 4; i++)
		{
			dimensionManager.PsychicFactions.Add(dimensionManager.InitializeFaction());
		}
		dimensionManager.GenerateMoreDimensions();
		XRLCore.Core.Game.SetObjectGameState("DimensionManager", dimensionManager);
	}

	public PsychicFaction InitializeFaction()
	{
		PsychicFaction psychicFaction = new PsychicFaction();
		int num = 0;
		do
		{
			if (++num >= 1000)
			{
				throw new Exception("infinitely looping on psychic faction base choice");
			}
			psychicFaction.factionName = Factions.GetRandomPotentiallyExtradimensionalFaction().Name;
		}
		while (IsValueTaken_PsychicFactions("factionName", psychicFaction.factionName));
		if (80.in100())
		{
			List<MutationEntry> mutationsOfCategory = MutationFactory.GetMutationsOfCategory("Mental");
			mutationsOfCategory.ShuffleInPlace();
			MutationEntry mutationEntry = null;
			foreach (MutationEntry item in mutationsOfCategory)
			{
				if (item.Category != null && item.Category.Name == "Mental" && item.Cost > 1 && !IsValueTaken_PsychicFactions("preferredMutation", item.Class))
				{
					mutationEntry = item;
					break;
				}
			}
			psychicFaction.preferredMutation = mutationEntry.Class;
		}
		else
		{
			psychicFaction.preferredMutation = "none";
		}
		psychicFaction.mainColor = Crayons.GetRandomColorAll();
		num = 0;
		string randomColorAll;
		do
		{
			randomColorAll = Crayons.GetRandomColorAll();
		}
		while (randomColorAll == psychicFaction.mainColor && ++num < 100);
		psychicFaction.a = Grammar.weirdLowerAs.GetRandomElement().ToString();
		psychicFaction.A = Grammar.weirdUpperAs.GetRandomElement().ToString();
		psychicFaction.e = Grammar.weirdLowerEs.GetRandomElement().ToString();
		psychicFaction.E = Grammar.weirdUpperEs.GetRandomElement().ToString();
		psychicFaction.i = Grammar.weirdLowerIs.GetRandomElement().ToString();
		psychicFaction.I = Grammar.weirdUpperIs.GetRandomElement().ToString();
		psychicFaction.o = Grammar.weirdLowerOs.GetRandomElement().ToString();
		psychicFaction.O = Grammar.weirdUpperOs.GetRandomElement().ToString();
		psychicFaction.u = Grammar.weirdLowerUs.GetRandomElement().ToString();
		psychicFaction.U = Grammar.weirdUpperUs.GetRandomElement().ToString();
		psychicFaction.c = Grammar.weirdLowerCs.GetRandomElement().ToString();
		psychicFaction.f = Grammar.weirdLowerFs.GetRandomElement().ToString();
		psychicFaction.n = Grammar.weirdLowerNs.GetRandomElement().ToString();
		psychicFaction.t = Grammar.weirdLowerTs.GetRandomElement().ToString();
		psychicFaction.y = Grammar.weirdLowerYs.GetRandomElement().ToString();
		psychicFaction.B = Grammar.weirdUpperBs.GetRandomElement().ToString();
		psychicFaction.C = Grammar.weirdUpperCs.GetRandomElement().ToString();
		psychicFaction.Y = Grammar.weirdUpperYs.GetRandomElement().ToString();
		psychicFaction.L = Grammar.weirdUpperLs.GetRandomElement().ToString();
		psychicFaction.R = Grammar.weirdUpperRs.GetRandomElement().ToString();
		psychicFaction.N = Grammar.weirdUpperNs.GetRandomElement().ToString();
		num = 0;
		do
		{
			psychicFaction.cultSymbol = int.Parse(HistoricStringExpander.ExpandString("<spice.extradimensional.cultSymbols.!random>", null, null));
		}
		while ((IsValueTaken_PsychicFactions("cultSymbol", psychicFaction.cultSymbol) || IsValueTaken_PsychicFactions("dimensionSymbol", psychicFaction.cultSymbol)) && ++num < 100);
		if (psychicFaction.preferredMutation == "none")
		{
			num = 0;
			do
			{
				psychicFaction.cultForm = Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.extradimensional.cultForms.!random>", null, null));
			}
			while (IsValueTaken_PsychicFactions("cultForm", psychicFaction.cultForm) && ++num < 100);
		}
		else
		{
			string text = HistoricStringExpander.ExpandString("<spice.extradimensional." + psychicFaction.preferredMutation + ".!random>");
			num = 0;
			do
			{
				psychicFaction.cultForm = Grammar.MakeTitleCase(text.Replace("*cult*", HistoricStringExpander.ExpandString("<spice.extradimensional.cultForms.!random>")));
			}
			while (IsValueTaken_PsychicFactions("cultForm", psychicFaction.cultForm) && ++num < 100);
		}
		num = 0;
		do
		{
			psychicFaction.dimensionSymbol = int.Parse(HistoricStringExpander.ExpandString("<spice.extradimensional.cultSymbols.!random>", null, null));
		}
		while (IsValueTaken_PsychicFactions("dimensionSymbol", psychicFaction.dimensionSymbol) || (IsValueTaken_PsychicFactions("cultSymbol", psychicFaction.cultSymbol) && ++num < 100));
		num = 0;
		do
		{
			psychicFaction.dimensionName = "the " + Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.extradimensional.dimensionNames.!random>", null, null));
		}
		while (IsValueTaken_PsychicFactions("dimensionName", psychicFaction.dimensionName) && ++num < 100);
		num = 0;
		do
		{
			psychicFaction.dimensionalWeaponIndex = Stat.Random(0, 17);
		}
		while (IsValueTaken_PsychicFactions("dimensionalWeaponIndex", psychicFaction.dimensionalWeaponIndex) && ++num < 100);
		num = 0;
		do
		{
			psychicFaction.dimensionalMissileWeaponIndex = Stat.Random(0, 17);
		}
		while (IsValueTaken_PsychicFactions("dimensionalMissileWeaponIndex", psychicFaction.dimensionalMissileWeaponIndex) && ++num < 100);
		num = 0;
		do
		{
			psychicFaction.dimensionalArmorIndex = Stat.Random(0, 19);
		}
		while (IsValueTaken_PsychicFactions("dimensionalArmorIndex", psychicFaction.dimensionalArmorIndex) && ++num < 100);
		num = 0;
		do
		{
			psychicFaction.dimensionalShieldIndex = Stat.Random(0, 18);
		}
		while (IsValueTaken_PsychicFactions("dimensionalShieldIndex", psychicFaction.dimensionalShieldIndex) && ++num < 100);
		num = 0;
		do
		{
			psychicFaction.dimensionalMiscIndex = Stat.Random(0, 21);
		}
		while (IsValueTaken_PsychicFactions("dimensionalMiscIndex", psychicFaction.dimensionalMiscIndex) && ++num < 100);
		num = 0;
		do
		{
			psychicFaction.dimensionalTraining = GenerateRandomTraining();
		}
		while (psychicFaction.dimensionalTraining != null && IsValueTaken_PsychicFactions("dimensionalTraining", psychicFaction.dimensionalTraining) && ++num < 100);
		string text2 = Guid.NewGuid().ToString();
		JournalAPI.AddObservation("There exists a dimension known as {{O|" + psychicFaction.dimensionName.Replace("*DimensionSymbol*", ((char)psychicFaction.dimensionSymbol).ToString()) + "}}.", text2, "Dimensions", text2, new string[1] { "dimension" }, revealed: false, -1L);
		psychicFaction.dimensionSecretID = text2;
		return psychicFaction;
	}

	public static string GenerateRandomTraining()
	{
		if (70.in100())
		{
			PowerEntry randomElement = SkillFactory.GetPowers().GetRandomElement();
			if (randomElement.ParentSkill != null && (randomElement.Cost <= 0 || randomElement.IsSkillInitiatory))
			{
				return randomElement.ParentSkill.Class;
			}
			return randomElement.Class;
		}
		return Statistic.Attributes.GetRandomElement();
	}

	public void GenerateMoreDimensions()
	{
		for (int i = 1; i <= NUM_EXTRA_DIMENSIONS; i++)
		{
			ExtraDimension extraDimension = new ExtraDimension();
			extraDimension.mainColor = Crayons.GetRandomColorAll();
			int num = 0;
			string randomColorAll;
			do
			{
				randomColorAll = Crayons.GetRandomColorAll();
			}
			while (randomColorAll == extraDimension.mainColor && ++num < 100);
			extraDimension.a = Grammar.weirdLowerAs.GetRandomElement().ToString();
			extraDimension.A = Grammar.weirdUpperAs.GetRandomElement().ToString();
			extraDimension.e = Grammar.weirdLowerEs.GetRandomElement().ToString();
			extraDimension.E = Grammar.weirdUpperEs.GetRandomElement().ToString();
			extraDimension.i = Grammar.weirdLowerIs.GetRandomElement().ToString();
			extraDimension.I = Grammar.weirdUpperIs.GetRandomElement().ToString();
			extraDimension.o = Grammar.weirdLowerOs.GetRandomElement().ToString();
			extraDimension.O = Grammar.weirdUpperOs.GetRandomElement().ToString();
			extraDimension.u = Grammar.weirdLowerUs.GetRandomElement().ToString();
			extraDimension.U = Grammar.weirdUpperUs.GetRandomElement().ToString();
			extraDimension.c = Grammar.weirdLowerCs.GetRandomElement().ToString();
			extraDimension.f = Grammar.weirdLowerFs.GetRandomElement().ToString();
			extraDimension.n = Grammar.weirdLowerNs.GetRandomElement().ToString();
			extraDimension.t = Grammar.weirdLowerTs.GetRandomElement().ToString();
			extraDimension.y = Grammar.weirdLowerYs.GetRandomElement().ToString();
			extraDimension.B = Grammar.weirdUpperBs.GetRandomElement().ToString();
			extraDimension.C = Grammar.weirdUpperCs.GetRandomElement().ToString();
			extraDimension.Y = Grammar.weirdUpperYs.GetRandomElement().ToString();
			extraDimension.L = Grammar.weirdUpperLs.GetRandomElement().ToString();
			extraDimension.R = Grammar.weirdUpperRs.GetRandomElement().ToString();
			extraDimension.N = Grammar.weirdUpperNs.GetRandomElement().ToString();
			num = 0;
			do
			{
				extraDimension.Symbol = int.Parse(HistoricStringExpander.ExpandString("<spice.extradimensional.cultSymbols.!random>"));
			}
			while ((IsValueTaken_PsychicFactions("dimensionSymbol", extraDimension.Symbol) || IsValueTaken_PsychicFactions("cultSymbol", extraDimension.Symbol) || IsValueTaken_ExtraDimensions("Symbol", extraDimension.Symbol)) && ++num < 100);
			num = 0;
			do
			{
				extraDimension.Name = "the " + Grammar.MakeTitleCase(HistoricStringExpander.ExpandString("<spice.extradimensional.dimensionNames.!random>"));
			}
			while ((IsValueTaken_PsychicFactions("dimensionName", extraDimension.Name) || IsValueTaken_ExtraDimensions("Name", extraDimension.Name)) && ++num < 100);
			num = 0;
			do
			{
				extraDimension.WeaponIndex = Stat.Random(0, 15);
			}
			while ((IsValueTaken_PsychicFactions("dimensionalWeaponIndex", extraDimension.WeaponIndex) || IsValueTaken_ExtraDimensions("WeaponIndex", extraDimension.WeaponIndex)) && ++num < 40);
			num = 0;
			do
			{
				extraDimension.MissileWeaponIndex = Stat.Random(0, 13);
			}
			while ((IsValueTaken_PsychicFactions("dimensionalWeaponIndex", extraDimension.WeaponIndex) || IsValueTaken_ExtraDimensions("WeaponIndex", extraDimension.WeaponIndex)) && ++num < 40);
			num = 0;
			do
			{
				extraDimension.ArmorIndex = Stat.Random(0, 19);
			}
			while (IsValueTaken_PsychicFactions("dimensionalArmorIndex", extraDimension.ArmorIndex) || IsValueTaken_ExtraDimensions("ArmorIndex", extraDimension.ArmorIndex));
			num = 0;
			do
			{
				extraDimension.ShieldIndex = Stat.Random(0, 18);
			}
			while (IsValueTaken_PsychicFactions("dimensionalShieldIndex", extraDimension.ShieldIndex) || IsValueTaken_ExtraDimensions("ShieldIndex", extraDimension.ShieldIndex));
			num = 0;
			do
			{
				extraDimension.MiscIndex = Stat.Random(0, 21);
			}
			while (IsValueTaken_PsychicFactions("dimensionalMiscIndex", extraDimension.MiscIndex) || IsValueTaken_ExtraDimensions("MiscIndex", extraDimension.MiscIndex));
			num = 0;
			do
			{
				extraDimension.Training = GenerateRandomTraining();
			}
			while (extraDimension.Training != null && IsValueTaken_ExtraDimensions("Training", extraDimension.Training) && ++num < 100);
			string text = Guid.NewGuid().ToString();
			JournalAPI.AddObservation("There exists a dimension known as {{O|" + extraDimension.Name.Replace("*DimensionSymbol*", ((char)extraDimension.Symbol).ToString()) + "}}.", text, "Dimensions", text, new string[1] { "dimension" }, revealed: false, -1L);
			extraDimension.SecretID = text;
			ExtraDimensions.Add(extraDimension);
		}
		JournalAPI.AddObservation("There exists a pocket dimension known as {{O|Tzimtzlum}}.", "$Tzimtzlum", "Dimensions", "$Tzimtzlum", new string[1] { "dimension" }, revealed: false, -1L);
	}

	public bool IsValueTaken_PsychicFactions(string property, object value)
	{
		foreach (PsychicFaction psychicFaction in PsychicFactions)
		{
			if (psychicFaction.GetType().GetField(property).GetValue(psychicFaction)
				.ToString() == value.ToString())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValueTaken_ExtraDimensions(string property, object value)
	{
		foreach (ExtraDimension extraDimension in ExtraDimensions)
		{
			if (extraDimension.GetType().GetField(property).GetValue(extraDimension)
				.ToString() == value.ToString())
			{
				return true;
			}
		}
		return false;
	}
}
