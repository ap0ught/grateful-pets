using System;
using System.Collections.Generic;
using Genkit;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SpaceTimeVortex : IPart
{
	public string DestinationZoneID;

	public int Delay;

	[NonSerialized]
	private List<GameObject> Queue;

	[NonSerialized]
	private Dictionary<GameObject, Location2D> Locations;

	[NonSerialized]
	private bool InitialTurn;

	[NonSerialized]
	private static List<string> Summonable;

	public override void Initialize()
	{
		InitialTurn = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeingConsumedEvent>.ID && ID != PooledEvent<BlocksRadarEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != EnteredCellEvent.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<InterruptAutowalkEvent>.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == PooledEvent<RealityStabilizeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			ParentObject.ParticleBlip("&K-", 10, 0L);
			DidX("collapse", "under the pressure of normality", null, null, null, null, ParentObject);
			ParentObject.Destroy();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		ApplyVortex(E.Object);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		E.IndicateObject = ParentObject;
		return false;
	}

	public void RequireDestinationZone()
	{
		if (!DestinationZoneID.IsNullOrEmpty())
		{
			return;
		}
		Zone currentZone = ParentObject.CurrentZone;
		string world = currentZone?.GetZoneWorld();
		DestinationZoneID = GetRandomDestinationZoneID(world);
		if (Queue == null || Locations == null)
		{
			return;
		}
		if (!DestinationZoneID.IsNullOrEmpty())
		{
			foreach (GameObject item in Queue)
			{
				try
				{
					if (GameObject.Validate(item) && !item.HasContext())
					{
						Location2D location2D = Locations[item];
						Cell destinationCellFor = GetDestinationCellFor(DestinationZoneID, item, location2D);
						if (destinationCellFor != null)
						{
							Teleport(item, destinationCellFor, ParentObject, currentZone?.GetCell(location2D), VisualEffects: false);
							item.MakeActive();
						}
						else
						{
							item.Obliterate();
						}
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Spacetime vortex queue unload", x);
				}
			}
		}
		else
		{
			foreach (GameObject item2 in Queue)
			{
				if (GameObject.Validate(item2) && !item2.HasContext())
				{
					item2.Obliterate();
				}
			}
		}
		Queue = null;
		Locations = null;
	}

	public static Cell GetDestinationCellFor(string ZoneID, GameObject Target, Cell Origin = null)
	{
		if (ZoneID != null)
		{
			Zone zone = The.ZoneManager.GetZone(ZoneID);
			if (zone != null)
			{
				Cell cell = Origin ?? Target.CurrentCell;
				Cell cell2 = zone.GetCell(cell.X, cell.Y);
				if (cell2 != null && !cell2.IsPassable(Target))
				{
					cell2 = cell2.getClosestPassableCellFor(Target);
				}
				return cell2;
			}
		}
		return Target.GetRandomTeleportTargetCell();
	}

	public static Cell GetDestinationCellFor(string ZoneID, GameObject Target, Location2D Origin)
	{
		if (ZoneID != null && Origin != null && GameObject.Validate(ref Target))
		{
			Zone zone = The.ZoneManager.GetZone(ZoneID);
			if (zone != null)
			{
				Cell cell = zone.GetCell(Origin);
				Cell cell2 = zone.GetCell(cell.X, cell.Y);
				if (cell2 != null && !cell2.IsPassable(Target))
				{
					cell2 = cell2.getClosestPassableCellFor(Target);
				}
				return cell2;
			}
		}
		return Target.GetRandomTeleportTargetCell();
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		foreach (GameObject item in E.Cell.GetObjectsWithPartReadonly("Render"))
		{
			if (!ParentObject.IsValid() || ParentObject.CurrentCell != E.Cell)
			{
				break;
			}
			ApplyVortex(item);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeingConsumedEvent E)
	{
		ApplyVortex(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		SpaceTimeAnomalyPeriodicEvents();
		InitialTurn = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BlocksRadarEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DefendMeleeHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefendMeleeHit")
		{
			ApplyVortex(E.GetGameObjectParameter("Attacker"));
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Stat.RandomCosmetic(1, 60) < 3 * (Options.ThrottleAnimationRate / 20))
		{
			string text = "&C";
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&W";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&R";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&B";
			}
			Cell cell = ParentObject.CurrentCell;
			The.ParticleManager.AddRadial(text + "ù", cell.X, cell.Y, Stat.RandomCosmetic(0, 7), Stat.RandomCosmetic(5, 10), 0.01f * (float)Stat.RandomCosmetic(4, 6), -0.01f * (float)Stat.RandomCosmetic(3, 7));
		}
		switch (Stat.RandomCosmetic(0, 4))
		{
		case 0:
			E.ColorString = "&B^k";
			break;
		case 1:
			E.ColorString = "&R^k";
			break;
		case 2:
			E.ColorString = "&C^k";
			break;
		case 3:
			E.ColorString = "&W^k";
			break;
		case 4:
			E.ColorString = "&K^k";
			break;
		}
		switch (Stat.RandomCosmetic(0, 3))
		{
		case 0:
			E.RenderString = "\t";
			break;
		case 1:
			E.RenderString = "é";
			break;
		case 2:
			E.RenderString = "\u0015";
			break;
		case 3:
			E.RenderString = "\u000f";
			break;
		}
		return true;
	}

	public static string GetRandomDestinationZoneID(string World, bool Validate = true)
	{
		if (World != "JoppaWorld")
		{
			return null;
		}
		string text;
		while (true)
		{
			int parasangX = Stat.Random(0, 79);
			int parasangY = Stat.Random(0, 24);
			int zoneX = Stat.Random(0, 2);
			int zoneY = Stat.Random(0, 2);
			int zoneZ = (50.in100() ? Stat.Random(10, 40) : 10);
			text = ZoneID.Assemble(World, parasangX, parasangY, zoneX, zoneY, zoneZ);
			if (!Validate)
			{
				break;
			}
			Zone zone = The.ZoneManager.GetZone(text);
			if (zone.GetEmptyCellCount() < 100)
			{
				continue;
			}
			Cell cell = null;
			int num = 0;
			while (++num < 5)
			{
				Cell randomCell = zone.GetRandomCell(3 - num / 25);
				if (randomCell.IsReachable() && !randomCell.IsSolid())
				{
					cell = randomCell;
					break;
				}
			}
			if (cell != null)
			{
				break;
			}
		}
		return text;
	}

	public static bool IsValidTarget(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object?.Render != null && Object.IsReal && !Object.IsScenery && Object.GetPhase() != 4 && !Object.HasTagOrProperty("ExcavatoryTerrainFeature"))
		{
			return !Object.HasTagOrProperty("IgnoreSpaceTimeVortex");
		}
		return false;
	}

	public static bool Teleport(GameObject Object, Cell C, GameObject Device, Cell FromCell = null, bool VisualEffects = true)
	{
		if (Object.CurrentZone != C.ParentZone)
		{
			if (Object.Brain != null && !Object.IsPlayerLed())
			{
				if (Object.TryGetEffect<Lost>(out var Effect))
				{
					Effect.DisableUnlost = false;
				}
				else if (C.ParentZone.Z == 10)
				{
					Object.ApplyEffect(new Lost());
				}
			}
			if (!Object.IsPlayer() && !Object.WasPlayer())
			{
				if (Object.PartyLeader != null && !Object.HasEffect<Incommunicado>())
				{
					Object.ApplyEffect(new Incommunicado());
				}
				Object.Brain?.Goals.Clear();
			}
		}
		Object.PlayWorldSound("Sounds/Abilities/sfx_ability_spacetimeVortex_interact_in");
		bool result = Object.CellTeleport(C, null, Device, null, null, null, 1000, Forced: true, VisualEffects, !Object.IsPlayer(), SkipRealityDistortion: true, null, "appear", FromCell);
		Object.PlayWorldSound("Sounds/Abilities/sfx_ability_spacetimeVortex_interact_out");
		if (Object.IsPlayer())
		{
			Achievement.VORTICES_ENTERED.Progress.Increment();
		}
		return result;
	}

	public bool ApplyVortex(GameObject GO)
	{
		if (ParentObject == GO || !IsValidTarget(GO))
		{
			return false;
		}
		if (GO.HasPartDescendedFrom<SpaceTimeVortex>())
		{
			bool usePopup = IComponent<GameObject>.Visible(ParentObject) || IComponent<GameObject>.Visible(GO);
			string displayName = ParentObject.DisplayName;
			string displayName2 = GO.DisplayName;
			if (displayName == displayName2)
			{
				EmitMessage("Two " + displayName.Pluralize() + " come into contact and both explode!", ' ', FromDialog: false, usePopup);
			}
			else
			{
				DidXToY("come", "into contact with", GO, "and both explode", "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, usePopup);
			}
			GO.Obliterate();
			ParentObject.Explode(3000, null, "1d200", 1f, Neutron: true);
			return true;
		}
		if (DestinationZoneID == null && !ObjectCallsForExplicitTracking(GO))
		{
			IComponent<GameObject>.XDidYToZ(GO, "are", "sucked into", ParentObject, null, "!", null, null, null, GO, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: true);
			Location2D location2D = GO.CurrentCell?.Location;
			if (location2D != null)
			{
				if (Queue == null)
				{
					Queue = new List<GameObject>();
				}
				if (Locations == null)
				{
					Locations = new Dictionary<GameObject, Location2D>();
				}
				Queue.Add(GO);
				Locations[GO] = location2D;
				GO.RemoveFromContext();
				GO.MakeInactive();
			}
			else
			{
				GO.Obliterate();
			}
		}
		else
		{
			RequireDestinationZone();
			Cell destinationCellFor = GetDestinationCellFor(DestinationZoneID, GO, ParentObject.CurrentCell);
			if (destinationCellFor == null || !GO.FireEvent(Event.New("SpaceTimeVortexContact", "Object", ParentObject, "DestinationCell", destinationCellFor)))
			{
				return false;
			}
			if (GO.IsPlayerLed() && !GO.IsTrifling)
			{
				Popup.Show("Your companion, " + GO.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + "," + GO.GetVerb("have") + " been sucked into " + ParentObject.t() + " " + The.Player.DescribeDirectionToward(ParentObject) + "!");
			}
			else
			{
				IComponent<GameObject>.XDidYToZ(GO, "are", "sucked into", ParentObject, null, "!", null, null, null, GO, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: true);
			}
			Teleport(GO, destinationCellFor, ParentObject);
		}
		return true;
	}

	public static bool IsBlueprintSummonable(GameObjectBlueprint BP)
	{
		if (!EncountersAPI.IsEligibleForDynamicEncounters(BP))
		{
			return false;
		}
		if (!BP.HasPart("Brain"))
		{
			return false;
		}
		return true;
	}

	public static List<string> GetSummonableBlueprints()
	{
		if (Summonable == null)
		{
			Summonable = new List<string>(128);
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (IsBlueprintSummonable(blueprint))
				{
					Summonable.Add(blueprint.Name);
				}
			}
		}
		return Summonable;
	}

	public static string GetSummonableBlueprint()
	{
		return GetSummonableBlueprints().GetRandomElement();
	}

	public virtual int SpaceTimeAnomalyEmergencePermillageBaseChance()
	{
		return 5;
	}

	public virtual int SpaceTimeAnomalyEmergenceExplodePercentageBaseChance()
	{
		return 0;
	}

	public virtual bool SpaceTimeAnomalyStationary()
	{
		return false;
	}

	public int SpaceTimeAnomalyEmergencePermillageChance()
	{
		return GetSpaceTimeAnomalyEmergencePermillageChanceEvent.GetFor(ParentObject, SpaceTimeAnomalyEmergencePermillageBaseChance());
	}

	public int SpaceTimeAnomalyEmergenceExplodePercentageChance()
	{
		return GetSpaceTimeAnomalyEmergenceExplodePercentageChanceEvent.GetFor(ParentObject, SpaceTimeAnomalyEmergenceExplodePercentageBaseChance());
	}

	public void SpaceTimeAnomalyPeriodicEvents()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		if (Delay > 0)
		{
			Delay--;
			return;
		}
		bool flag = InitialTurn || SpaceTimeAnomalyStationary();
		List<Cell> list = (flag ? null : cell.GetAdjacentCells());
		if (!flag)
		{
			cell.RemoveObject(ParentObject);
		}
		if (SpaceTimeAnomalyEmergencePermillageChance().in1000())
		{
			if (list == null)
			{
				list = cell.GetLocalEmptyAdjacentCells();
			}
			Cell cell2 = (flag ? list.GetRandomElement() : cell);
			if (cell2 != null)
			{
				GameObject gameObject = GameObject.Create(GetSummonableBlueprint());
				if (gameObject != null)
				{
					cell2.AddObject(gameObject);
					gameObject.MakeActive();
					string verb = (gameObject.IsMobile() ? "climb" : "fall");
					IComponent<GameObject>.XDidYToZ(gameObject, verb, "through", ParentObject, IComponent<GameObject>.ThePlayer?.DescribeDirectionToward(ParentObject), "!", null, null, gameObject, null, UseFullNames: false, IndefiniteSubject: true);
					gameObject.PlayWorldSound("Sounds/Abilities/sfx_ability_spacetimeVortex_interact_out");
					if (SpaceTimeAnomalyEmergenceExplodePercentageChance().in100())
					{
						DidX("destabilize", null, "!", null, null, null, IComponent<GameObject>.ThePlayer);
						ParentObject.Explode(3000, null, "1d200", 1f, Neutron: true);
					}
				}
			}
		}
		if (!flag && list != null)
		{
			list.GetRandomElement()?.AddObject(ParentObject);
		}
	}

	public static bool ObjectCallsForExplicitTracking(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			return true;
		}
		if (Object.IsPlayerLed() && !Object.IsTrifling)
		{
			return true;
		}
		if (Object.IsImportant())
		{
			return true;
		}
		if (Object.IsInteresting())
		{
			return true;
		}
		return false;
	}
}
