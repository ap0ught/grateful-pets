using System;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;
using XRL.Messages;
using XRL.Rules;
using XRL.Wish;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[HasWishCommand]
public class BaseLiquid
{
	public const int STANDARD_SAVE_CAP = 24;

	public static readonly List<string> PaintGroups = new List<string>();

	public string ID;

	public int FlameTemperature = 99999;

	public int VaporTemperature = 100;

	public int FreezeTemperature;

	public int BrittleTemperature = -100;

	public int Temperature = 25;

	public int PureElectricalConductivity = 80;

	public int MixedElectricalConductivity = 80;

	public int ThermalConductivity = 50;

	public int Combustibility;

	public int Adsorbence = 100;

	public int Fluidity = 50;

	public int Evaporativity;

	public int Staining;

	public int Cleansing;

	public int ShallowPaintGroup = -1;

	public int DeepPaintGroup = -1;

	public int FreezeObjectThreshold1;

	public int FreezeObjectThreshold2;

	public int FreezeObjectThreshold3;

	public int SlipperySaveTargetBase = 5;

	public int StickySaveTargetBase = 1;

	public int StickyDuration = 12;

	public double SlipperySaveTargetScale = 0.3;

	public double StickySaveTargetScale = 0.1;

	public bool StainOnlyWhenPure;

	public bool EnableCleaning;

	public bool InterruptAutowalk;

	public bool ConsiderDangerousToContact;

	public bool ConsiderDangerousToDrink;

	public bool Glows;

	public bool SlipperyWhenFrozen;

	public bool SlipperyWhenWet;

	public bool StickyWhenFrozen;

	public bool StickyWhenWet;

	public double Weight = 0.25;

	public string ConversionProduct;

	public string VaporObject;

	public string FreezeObject1;

	public string FreezeObject2;

	public string FreezeObject3;

	public string FreezeObjectVerb1;

	public string FreezeObjectVerb2;

	public string FreezeObjectVerb3;

	public string SlipperyMessage;

	public string SlipperyParticle;

	public string SlipperySaveStat = "Agility";

	public string SlipperySaveVs = "Slip Move";

	public string StickyMessage;

	public string StickyParticle;

	public string StickySaveStat = "Strength,Agility";

	public string StickySaveVs = "Stuck Restraint";

	public string CirculatoryLossTerm = "leaking";

	public string CirculatoryLossNoun = "leak";

	[NonSerialized]
	public static List<string> DefaultColors = new List<string>(2) { "b", "B" };

	[NonSerialized]
	public string _ColoredCirculatoryLossTerm;

	[NonSerialized]
	public string _ColoredCirculatoryLossNoun;

	public static readonly string[] Puddles = new string[4] { "Liquids/Water/puddle_1.png", "Liquids/Water/puddle_2.png", "Liquids/Water/puddle_3.png", "Liquids/Water/puddle_4.png" };

	public string Name => GetName();

	public int Viscosity => 100 - Fluidity;

	public string ColoredCirculatoryLossTerm
	{
		get
		{
			if (_ColoredCirculatoryLossTerm == null)
			{
				_ColoredCirculatoryLossTerm = "{{" + GetColor() + "|" + CirculatoryLossTerm + "}}";
			}
			return _ColoredCirculatoryLossTerm;
		}
	}

	public string ColoredCirculatoryLossNoun
	{
		get
		{
			if (_ColoredCirculatoryLossNoun == null)
			{
				_ColoredCirculatoryLossNoun = "{{" + GetColor() + "|" + CirculatoryLossNoun + "}}";
			}
			return _ColoredCirculatoryLossNoun;
		}
	}

	public BaseLiquid(string ID)
	{
		this.ID = ID;
	}

	public virtual float GetValuePerDram()
	{
		return 0f;
	}

	public float GetExtrinsicValuePerDram(bool Pure)
	{
		return GetValuePerDram() * (Pure ? GetPureLiquidValueMultipler() : 1f);
	}

	/// <summary>
	///     When the liquid is pure, multiple ValuePerDram by this additional factor for value calculations.
	/// </summary>
	public virtual float GetPureLiquidValueMultipler()
	{
		return 1f;
	}

	public virtual string GetPreparedCookingIngredient()
	{
		return "";
	}

	public virtual string GetName()
	{
		return GetName(null);
	}

	public virtual string GetName(LiquidVolume Liquid)
	{
		return "liquid";
	}

	public virtual string GetAdjective(LiquidVolume Liquid)
	{
		return "liquidy";
	}

	public virtual string GetWaterRitualName()
	{
		return "liquid";
	}

	public virtual string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "damp";
	}

	public virtual string GetSmearedName(LiquidVolume Liquid)
	{
		return "damp";
	}

	public virtual string GetStainedName(LiquidVolume Liquid)
	{
		return GetName(Liquid);
	}

	public virtual bool SafeContainer(GameObject GO)
	{
		return true;
	}

	[Obsolete("Replaced by Froze(LiquidVolume, GameObject), retained for mod compatibility")]
	public virtual bool Froze(LiquidVolume Liquid)
	{
		return true;
	}

	public virtual bool Froze(LiquidVolume Liquid, GameObject By)
	{
		if (SlipperyWhenFrozen || SlipperyWhenWet || StickyWhenFrozen || StickyWhenWet)
		{
			Liquid?.ParentObject?.CurrentCell?.FlushNavigationCache();
		}
		return true;
	}

	public virtual bool Thawed(LiquidVolume Liquid, GameObject By)
	{
		if (SlipperyWhenFrozen || SlipperyWhenWet || StickyWhenFrozen || StickyWhenWet)
		{
			Liquid?.ParentObject?.CurrentCell?.FlushNavigationCache();
		}
		return true;
	}

	[Obsolete("Replaced by Vaporized(LiquidVolume, GameObject), retained for mod compatibility")]
	public virtual bool Vaporized(LiquidVolume Liquid)
	{
		return true;
	}

	public virtual bool Vaporized(LiquidVolume Liquid, GameObject By)
	{
		if (!VaporObject.IsNullOrEmpty())
		{
			Cell currentCell = Liquid.ParentObject.GetCurrentCell();
			if (currentCell != null)
			{
				GameObject gameObject = GameObject.Create(VaporObject);
				Gas part = gameObject.GetPart<Gas>();
				if (part != null)
				{
					part.Density = Liquid.Amount(ID) / 20;
					part.Creator = By;
				}
				currentCell.AddObject(gameObject);
			}
		}
		return Vaporized(Liquid);
	}

	public virtual bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		return true;
	}

	public bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message)
	{
		bool ExitInterface = false;
		return Drank(Liquid, Volume, Target, Message, ref ExitInterface);
	}

	public bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, IEvent E)
	{
		bool ExitInterface = false;
		bool result = Drank(Liquid, Volume, Target, Message, ref ExitInterface);
		if (ExitInterface)
		{
			E.RequestInterfaceExit();
		}
		return result;
	}

	public virtual void SmearOnTick(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
	}

	public virtual void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
	}

	public void ApplyRelativeTemperature(LiquidVolume Liquid, GameObject Object, int Multiple = 1)
	{
		int temperature = Object.Temperature;
		if (Temperature != temperature)
		{
			int num = Temperature - temperature;
			int num2 = Liquid.MilliAmount(ID) / (20000 / num) * Multiple;
			if (num2 > 0)
			{
				int? max = Temperature;
				Object.TemperatureChange(num2, null, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, 5, null, max);
			}
			else if (num2 < 0 && (Multiple != 1 || !Glotrot.IsIck(Liquid)))
			{
				Object.TemperatureChange(num2, null, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: false, 5, Temperature);
			}
		}
	}

	public virtual void FillingContainer(GameObject Container, LiquidVolume Liquid)
	{
		ApplyRelativeTemperature(Liquid, Container);
	}

	public virtual void ProcessTurns(LiquidVolume Liquid, GameObject Container, int Turns)
	{
		if (!Liquid.IsOpenVolume())
		{
			ApplyRelativeTemperature(Liquid, Container, Turns);
		}
	}

	public virtual void BeforeRender(LiquidVolume Liquid)
	{
	}

	public virtual void BeforeRenderSecondary(LiquidVolume Liquid)
	{
	}

	public virtual bool MixingWith(LiquidVolume Liquid, LiquidVolume NewLiquid, int Amount, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy, ref bool ExitInterface)
	{
		return true;
	}

	public virtual void MixedWith(LiquidVolume Liquid, LiquidVolume NewLiquid, int Amount, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy, ref bool ExitInterface)
	{
	}

	public virtual void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void RenderBackgroundSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void BaseRenderPrimary(LiquidVolume Liquid)
	{
	}

	public virtual void BaseRenderSecondary(LiquidVolume Liquid)
	{
	}

	public virtual void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
	}

	public virtual void RenderSmearSecondary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
	}

	public virtual void ObjectGoingProne(LiquidVolume Liquid, GameObject GO, bool UsePopups)
	{
	}

	public virtual void ObjectEnteredCell(LiquidVolume Liquid, IObjectCellInteractionEvent E)
	{
		if ((!SlipperyWhenFrozen && !SlipperyWhenWet && !StickyWhenFrozen && !StickyWhenWet) || !Liquid.IsOpenVolume() || !E.Object.HasPart<Body>())
		{
			return;
		}
		if (((SlipperyWhenFrozen && Liquid.ParentObject.IsFrozen()) || (SlipperyWhenWet && !Liquid.IsWadingDepth() && !E.Object.Slimewalking && !Liquid.ParentObject.IsFrozen())) && !E.Object.MakeSave(SlipperySaveStat, GetSlipperySaveDifficulty(Liquid, E.Object), null, null, SlipperySaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
		{
			if (!SlipperyMessage.IsNullOrEmpty())
			{
				string text = GameText.VariableReplace(SlipperyMessage, E.Object, Liquid.ParentObject);
				if (!text.IsNullOrEmpty())
				{
					E.Object.Physics.EmitMessage(text);
				}
			}
			if (!SlipperyParticle.IsNullOrEmpty())
			{
				E.Object.ParticleText(SlipperyParticle);
			}
			E.Object.Move(Directions.GetRandomDirection(), Forced: true);
		}
		else
		{
			if ((!StickyWhenFrozen || E.Object.Slimewalking || !Liquid.ParentObject.IsFrozen()) && (!StickyWhenWet || E.Object.Slimewalking || Liquid.ParentObject.IsFrozen()))
			{
				return;
			}
			int stickySaveDifficulty = GetStickySaveDifficulty(Liquid, E.Object);
			if (E.Object.MakeSave(StickySaveStat, stickySaveDifficulty, null, null, StickySaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
			{
				return;
			}
			if (!SlipperyMessage.IsNullOrEmpty())
			{
				string text2 = GameText.VariableReplace(SlipperyMessage, E.Object, Liquid.ParentObject);
				if (!text2.IsNullOrEmpty())
				{
					E.Object.Physics.EmitMessage(text2);
				}
			}
			if (!StickyParticle.IsNullOrEmpty())
			{
				E.Object.ParticleText(StickyParticle);
			}
			Stuck e = new Stuck(StickyDuration, stickySaveDifficulty, StickySaveVs, null, "stuck", "in", Liquid.ParentObject.ID);
			E.Object.ApplyEffect(e);
		}
	}

	public virtual int GetSlipperySaveDifficulty(LiquidVolume Liquid, GameObject Subject = null)
	{
		int val = SlipperySaveTargetBase + Liquid.Amount(ID).DiminishingReturns(SlipperySaveTargetScale) - (Subject?.GetIntProperty("Stable") ?? 0);
		return Math.Min(24, val);
	}

	public virtual int GetStickySaveDifficulty(LiquidVolume Liquid, GameObject Subject = null)
	{
		int val = StickySaveTargetBase + Liquid.Amount(ID).DiminishingReturns(StickySaveTargetScale);
		return Math.Min(24, val);
	}

	[Obsolete("Replaced by ObjectEnteredCell(LiquidVolume, ObjectEnteredCellEvent), retained for mod compatibility")]
	public virtual void ObjectEnteredCell(LiquidVolume Liquid, GameObject GO)
	{
	}

	public virtual bool EnteredCell(LiquidVolume Liquid, EnteredCellEvent E)
	{
		return EnteredCell(Liquid, ref E.InterfaceExit);
	}

	[Obsolete("Replaced by EnteredCell(LiquidVolume, EnteredCellEvent), retained for mod compatibility")]
	public virtual bool EnteredCell(LiquidVolume Liquid, ref bool ExitInterface)
	{
		return true;
	}

	public virtual bool PourIntoCell(LiquidVolume Liquid, GameObject Pourer, Cell TargetCell, ref int PourAmount, bool CanPourOn, ref bool RequestInterfaceExit)
	{
		return true;
	}

	public virtual bool Douse(LiquidVolume Liquid, GameObject Actor, ref int PourAmount, ref bool RequestInterfaceExit)
	{
		return true;
	}

	public virtual string SplashSound(LiquidVolume Liquid)
	{
		if (Liquid != null && Liquid.IsSwimmingDepth())
		{
			return "Sounds/Foley/fly_tileMove_water_swim";
		}
		if (Liquid != null && Liquid.IsWadingDepth())
		{
			return "Sounds/Foley/fly_tileMove_water_wade";
		}
		return "Sounds/Foley/fly_tileMove_water_puddle";
	}

	public virtual void ObjectInCell(LiquidVolume Liquid, GameObject GO)
	{
	}

	public virtual List<string> GetColors()
	{
		return DefaultColors;
	}

	public virtual string GetColor()
	{
		return "b";
	}

	public virtual int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, bool FilthAffinity, ref bool Uncacheable)
	{
		if ((SlipperyWhenFrozen || SlipperyWhenWet || StickyWhenFrozen || StickyWhenWet) && Liquid.IsOpenVolume())
		{
			if (Smart && GO != null)
			{
				if ((SlipperyWhenFrozen && Liquid.ParentObject.IsFrozen()) || (SlipperyWhenWet && !Liquid.IsWadingDepth() && !Slimewalking && !Liquid.ParentObject.IsFrozen()))
				{
					return Math.Max(GetSlipperySaveDifficulty(Liquid, GO) / 3, 2);
				}
				if ((StickyWhenFrozen && !Slimewalking && Liquid.ParentObject.IsFrozen()) || (StickyWhenWet && !Slimewalking && !Liquid.ParentObject.IsFrozen()))
				{
					return Math.Max(GetStickySaveDifficulty(Liquid, GO) / 2, 2);
				}
			}
			if (!Slimewalking)
			{
				return 2;
			}
		}
		return 0;
	}

	public virtual int GetHealingLocationValue(LiquidVolume Liquid, GameObject Actor)
	{
		return 0;
	}

	public virtual void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
	}

	public virtual string GetPaint(LiquidVolume Liquid)
	{
		if (!Liquid.IsWadingDepth())
		{
			return Liquid.ParentObject.GetTag("PaintedShallowLiquid", "shallow");
		}
		return Liquid.ParentObject.GetTag("PaintedLiquid", "deep");
	}

	public virtual string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (!Liquid.IsWadingDepth())
		{
			return Liquid.ParentObject.GetTag("PaintedShallowLiquidAtlas", "Liquids/Water/");
		}
		return Liquid.ParentObject.GetTag("PaintedLiquidAtlas", "Liquids/Water/");
	}

	public virtual string GetPaintExtension(LiquidVolume Liquid)
	{
		return Liquid.ParentObject.GetTag("PaintedLiquidExtension", ".png");
	}

	public virtual int GetPaintGroup(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			if (DeepPaintGroup == -1)
			{
				return DeepPaintGroup = AllocatePaintGroup(Liquid);
			}
			return DeepPaintGroup;
		}
		if (ShallowPaintGroup == -1)
		{
			return ShallowPaintGroup = AllocatePaintGroup(Liquid);
		}
		return ShallowPaintGroup;
	}

	protected virtual int AllocatePaintGroup(LiquidVolume Liquid)
	{
		string item = GetPaintAtlas(Liquid) + GetPaint(Liquid);
		int num = PaintGroups.IndexOf(item);
		if (num == -1)
		{
			num = PaintGroups.Count;
			PaintGroups.Add(item);
		}
		return num;
	}

	public virtual string GetPuddle(LiquidVolume Liquid)
	{
		return Puddles.GetRandomElementCosmetic();
	}

	public static void AddPlayerMessage(string msg, string color = null, bool capitalize = true)
	{
		MessageQueue.AddPlayerMessage(msg, color, capitalize);
	}

	public static void AddPlayerMessage(string msg, char color, bool capitalize = true)
	{
		MessageQueue.AddPlayerMessage(msg, color, capitalize);
	}

	public string GetFreezeObjectForVolume(int Volume)
	{
		if (!FreezeObject3.IsNullOrEmpty() && Volume >= FreezeObjectThreshold3)
		{
			return FreezeObject3;
		}
		if (!FreezeObject2.IsNullOrEmpty() && Volume >= FreezeObjectThreshold2)
		{
			return FreezeObject2;
		}
		if (!FreezeObject1.IsNullOrEmpty() && Volume >= FreezeObjectThreshold1)
		{
			return FreezeObject1;
		}
		return null;
	}

	public string GetFreezeObjectForVolume(int Volume, out string Verb)
	{
		Verb = null;
		if (!FreezeObject3.IsNullOrEmpty() && Volume >= FreezeObjectThreshold3)
		{
			Verb = FreezeObjectVerb3;
			return FreezeObject3;
		}
		if (!FreezeObject2.IsNullOrEmpty() && Volume >= FreezeObjectThreshold2)
		{
			Verb = FreezeObjectVerb2;
			return FreezeObject2;
		}
		if (!FreezeObject1.IsNullOrEmpty() && Volume >= FreezeObjectThreshold1)
		{
			Verb = FreezeObjectVerb1;
			return FreezeObject1;
		}
		return null;
	}

	[WishCommand("tileliquids", null)]
	public static void TestLiquids()
	{
		Zone activeZone = The.ActiveZone;
		for (int i = 0; i < activeZone.Height; i++)
		{
			for (int j = 0; j < activeZone.Width; j++)
			{
				activeZone.Map[j][i].Clear(null, Important: true, Combat: true, (GameObject x) => x.IsPlayer());
			}
		}
		StringMap<BaseLiquid> liquids = LiquidVolume.Liquids;
		int num = activeZone.Width / liquids.Count;
		int num2 = activeZone.Width % liquids.Count;
		string text = "";
		foreach (KeyValuePair<string, BaseLiquid> liquid in liquids)
		{
			if (text != "")
			{
				text += "\n";
			}
			text += liquid.Key;
			Action<GameObject> beforeObjectCreated = (GameObject x) =>
			{
				x.LiquidVolume.InitialLiquid = liquid.Key + "-1000";
			};
			for (int num3 = 5; num3 < 20; num3++)
			{
				if (num3 != 12)
				{
					Cell cell = activeZone.Map[num2][num3];
					GameObject gameObject = GameObjectFactory.Factory.CreateObject("SlimePuddle", beforeObjectCreated);
					cell.Objects.Add(gameObject);
					gameObject.Physics.EnterCell(cell);
					gameObject.LiquidVolume.CheckImage();
				}
			}
			foreach (Cell localAdjacentCell in activeZone.Map[num2][12].GetLocalAdjacentCells())
			{
				if (localAdjacentCell.Objects.Count == 0)
				{
					GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("SlimePuddle", beforeObjectCreated);
					localAdjacentCell.Objects.Add(gameObject2);
					gameObject2.Physics.EnterCell(localAdjacentCell);
					gameObject2.LiquidVolume.CheckImage();
				}
			}
			num2 += num;
			if (num2 >= 80)
			{
				break;
			}
		}
		MetricsManager.LogError(text);
	}
}
