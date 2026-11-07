using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.World.Text;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LiquidSpitter : BaseMutation
{
	public static readonly string BasePool = "BasePool";

	public static readonly Dictionary<string, string> LiquidAnimationColors = new Dictionary<string, string>
	{
		{ "lava", "&W" },
		{ "acid", "&G" },
		{ "honey", "&w" },
		{ "slime", "&g" },
		{ "water", "&B" },
		{ "salt", "&Y" },
		{ "base", "&B" },
		{ "wine", "&m" },
		{ "asphalt", "&K" },
		{ "oil", "&K" },
		{ "blood", "&r" },
		{ "sludge", "&w" },
		{ "goo", "&G" },
		{ "putrid", "&g" },
		{ "gel", "&Y" },
		{ "ooze", "&K" },
		{ "cider", "&w" },
		{ "convalessence", "&C" },
		{ "neutronflux", "&y" },
		{ "cloning", "&Y" },
		{ "proteangunk", "&c" },
		{ "wax", "&Y" },
		{ "ink", "&K" },
		{ "sap", "&W" },
		{ "brainbrine", "&g" },
		{ "algae", "&g" },
		{ "sunslag", "&Y" },
		{ "warmstatic", "&y" }
	};

	public static readonly string[] Exclude = new string[1] { "neutronflux" };

	public static readonly string[] Dilute = new string[4] { "cloning", "brainbrine", "sunslag", "warmstatic" };

	public const string ABL_CMD = "CommandSpitLiquid";

	[FieldSaveVersion(406)]
	public new string Pool;

	public List<string> Liquids = new List<string>();

	[FieldSaveVersion(406)]
	public List<string> Proportions = new List<string>();

	private string _LiquidName;

	public string LiquidName
	{
		get
		{
			if (_LiquidName == null)
			{
				GameObject gameObject = CreatePool(Sample: true);
				_LiquidName = gameObject.LiquidVolume.GetLiquidName();
				gameObject.Obliterate();
			}
			return _LiquidName;
		}
	}

	public string InitialLiquid
	{
		get
		{
			return GetLiquidSpec();
		}
		set
		{
			AddLiquidSpec(value, Replace: true);
		}
	}

	public string GetAnimationColor
	{
		get
		{
			if (Liquids.Count > 0 && LiquidAnimationColors.TryGetValue(Liquids.GetRandomElementCosmetic(), out var value))
			{
				return value;
			}
			return "&y";
		}
	}

	public LiquidSpitter()
	{
	}

	public LiquidSpitter(string Liquid)
		: this()
	{
		AddLiquid(Liquid);
	}

	public override bool CanLevel()
	{
		return false;
	}

	public int GetCooldown()
	{
		return 40;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		LiquidSpitter obj = base.DeepCopy(Parent) as LiquidSpitter;
		obj.Pool = Pool;
		obj.Liquids = new List<string>(Liquids);
		obj.Proportions = new List<string>(Proportions);
		return obj;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		if (Reader.FileVersion < 406)
		{
			while (Proportions.Count < Liquids.Count)
			{
				Proportions.Add(null);
			}
		}
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Liquid", ColorUtility.StripBackgroundFormatting(LiquidName));
		stats.Set("Range", 8);
		stats.Set("Area", "3x3");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown());
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.HasLOSTo(E.Target, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			E.Add("CommandSpitLiquid");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command != "CommandSpitLiquid")
		{
			return base.HandleEvent(E);
		}
		if (Liquids.Count == 0)
		{
			MetricsManager.LogError($"No liquids to spit (Creature: {ParentObject})");
			return ParentObject.ShowFailure("You lack a liquid to spit!");
		}
		List<Cell> list = PickBurst(1, 8, Locked: false, AllowVis.OnlyVisible, "Spit");
		if (list.IsNullOrEmpty())
		{
			return false;
		}
		if (list.Any((Cell C) => C.DistanceTo(ParentObject) > 9))
		{
			return ParentObject.ShowFailure("That is out of range! (8 squares)");
		}
		ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_creature_liquid_spit");
		SlimeGlands.SlimeAnimation(GetAnimationColor, ParentObject.CurrentCell, list[0]);
		UseEnergy(1000, "Physical Mutation Spit Liquid");
		CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown());
		DidX("spit", "a puddle of " + LiquidName, "!", null, null, ParentObject);
		int num = 0;
		for (int count = list.Count; num < count; num++)
		{
			if (80.in100() || num == 0)
			{
				list[num].AddObject(CreatePool());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			if (Liquids.Contains("salt"))
			{
				E.Add("salt", 1);
			}
			else if (Liquids.Contains("convalessence"))
			{
				E.Add("ice", 1);
			}
			else if (Liquids.Contains("sunslag"))
			{
				E.Add("stars", 1);
			}
			else if (Liquids.Contains("brainbrine"))
			{
				E.Add("scholarship", 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You spit a puddle of " + ColorUtility.StripBackgroundFormatting(LiquidName) + ".";
	}

	public override string GetLevelText(int Level)
	{
		return "Range: 8\nArea: 3x3\nCooldown: 10 rounds";
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spit Liquid", "CommandSpitLiquid", "Physical Mutations", null, "\u00ad");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public bool TryGetPoolBlueprint(out GameObjectBlueprint Blueprint)
	{
		if (Pool.IsNullOrEmpty())
		{
			Blueprint = null;
			return false;
		}
		if (!GameObjectFactory.Factory.Blueprints.TryGetValue(Pool, out Blueprint))
		{
			MetricsManager.LogError("LiquidSpitter: Pool blueprint '" + Pool + "' does not exist, falling back");
			Blueprint = null;
			return false;
		}
		if (!Blueprint.InheritsFrom("Water"))
		{
			MetricsManager.LogError("LiquidSpitter: Pool blueprint '" + Pool + "' does not inherit from 'Water', falling back");
			Blueprint = null;
			return false;
		}
		if (!Blueprint.TryGetPartParameter<string>("LiquidVolume", "StartVolume", out var Result) || Result.IsNullOrEmpty())
		{
			MetricsManager.LogError("LiquidSpitter: Pool blueprint " + Pool + " has an invalid starting volume, falling back");
			Blueprint = null;
			return false;
		}
		return true;
	}

	public GameObject CreatePool(bool Sample = false)
	{
		if (!TryGetPoolBlueprint(out var Blueprint))
		{
			Blueprint = GameObjectFactory.Factory.GetBlueprint(BasePool);
		}
		GameObject gameObject = GameObject.Create(Blueprint, 0, 0, null, null, null, Sample ? "Sample" : null);
		LiquidVolume liquidVolume = gameObject.LiquidVolume;
		liquidVolume.ComponentLiquids.Clear();
		string startVolume = liquidVolume.StartVolume;
		int i = 0;
		int count = Liquids.Count;
		int count2 = Proportions.Count;
		for (; i < count; i++)
		{
			string key = Liquids[i];
			int num = ((i < count2) ? Proportions[i].Coalesce(startVolume) : startVolume).RollCached();
			if (num > 0)
			{
				liquidVolume.ComponentLiquids[key] = num;
			}
		}
		liquidVolume.Update();
		if (liquidVolume.Primary.IsNullOrEmpty())
		{
			MetricsManager.LogError("Spitting liquid without primary " + string.Format("(Blueprint: {0}, Liquids: {1}, Proportions: {2}, Creature: {3}, Volume: {4}, ", Blueprint.Name, string.Join(", ", Liquids), string.Join(", ", Proportions), ParentObject, liquidVolume.Volume) + "Components: " + string.Join(", ", liquidVolume.ComponentLiquids) + ")");
		}
		return gameObject;
	}

	public void CheckDilute()
	{
		if (Liquids.Count == 1 && Dilute.Contains(Liquids[0]))
		{
			_LiquidName = null;
			AddLiquidInternal("water");
		}
	}

	private void AddLiquidInternal(string ID, string Proportion = null)
	{
		if (!Liquids.Contains(ID) && !Exclude.Contains(ID))
		{
			Liquids.Add(ID);
			Proportions.Add(Proportion);
		}
	}

	public void AddLiquid(string ID, string Proportion = null)
	{
		_LiquidName = null;
		AddLiquidInternal(ID);
		CheckDilute();
	}

	public string GetLiquidSpec()
	{
		int count = Liquids.Count;
		int count2 = Proportions.Count;
		switch (count)
		{
		case 0:
			return "";
		case 1:
			if (count2 == 0 || Proportions[0].IsNullOrEmpty())
			{
				return Liquids[0];
			}
			break;
		}
		using TextBuilder textBuilder = TextBuilder.Get();
		for (int i = 0; i < count; i++)
		{
			textBuilder.Compound(Liquids[i], ',');
			if (i < count2 && !Proportions[i].IsNullOrEmpty())
			{
				textBuilder.Compound(Proportions[i], ':');
			}
		}
		return textBuilder.ToString();
	}

	public void AddLiquidSpec(string Spec, bool Replace = false)
	{
		_LiquidName = null;
		if (Replace)
		{
			Liquids.Clear();
			Proportions.Clear();
		}
		if (Spec.IsNullOrEmpty())
		{
			return;
		}
		DelimitedEnumeratorChar enumerator = Spec.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			current.Split(':', out var First, out var Second);
			if (Second.IsEmpty)
			{
				current.Split('-', out First, out Second);
			}
			AddLiquidInternal(First.ToString(), Second.IsEmpty ? null : Second.ToString());
		}
		CheckDilute();
	}

	public void AddLiquids(IReadOnlyList<string> Liquids, IReadOnlyList<string> Proportions = null, bool Replace = false)
	{
		_LiquidName = null;
		if (Replace)
		{
			this.Liquids.Clear();
			this.Proportions.Clear();
		}
		if (!Liquids.IsNullOrEmpty())
		{
			int i = 0;
			int count = Liquids.Count;
			int num = Proportions?.Count ?? 0;
			for (; i < count; i++)
			{
				AddLiquidInternal(Liquids[i], (i < num) ? Proportions[i] : null);
			}
			CheckDilute();
		}
	}
}
