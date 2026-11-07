using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.EditorFormats.Map;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class Interior : IPart
{
	public static class Enter
	{
		public const int SUCCESS = 0;

		public const int HOSTILE = 1;

		public const int INVALID = 2;

		public const int OUT_OF_PHASE = 4;

		public const int UNREACHABLE = 8;

		public const int GIGANTIC = 16;

		public const int PREVENTED = 30;

		public const int FORBIDDEN = 1;
	}

	public const int FLAG_UNIQUE = 1;

	public const int FLAG_IGNORE_WEIGHT = 2;

	public string Cell;

	public int WX = 40;

	public int WY = 12;

	public int X = 1;

	public int Y = 1;

	public int Z = 10;

	public int FallDistance;

	public int CarriedWeight;

	public int Flags = 1;

	[NonSerialized]
	private static bool ErrorMessage;

	[NonSerialized]
	private InteriorZone _Zone;

	[NonSerialized]
	private string _ZoneID;

	[NonSerialized]
	private MapBuilder _Builder;

	[NonSerialized]
	private long WeightCacheTurn = -1L;

	[NonSerialized]
	private bool Collapsed;

	public bool IsZoneLive => The.ZoneManager.IsZoneLive(ZoneID);

	public bool Unique
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	public bool IgnoreWeight
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	public InteriorZone Zone => GetZone();

	public string ZoneID
	{
		get
		{
			if (_ZoneID == null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder("Interior");
				if (!Cell.IsNullOrEmpty())
				{
					stringBuilder.Compound(Cell, '@');
				}
				if (Unique)
				{
					stringBuilder.Compound(ParentObject.ID, '@');
				}
				stringBuilder.Compound(WX, '.');
				stringBuilder.Compound(WY, '.');
				stringBuilder.Compound(X, '.');
				stringBuilder.Compound(Y, '.');
				stringBuilder.Compound(Z, '.');
				_ZoneID = stringBuilder.ToString();
			}
			return _ZoneID;
		}
	}

	public MapBuilder Builder
	{
		get
		{
			if (_Builder == null)
			{
				_Builder = The.ZoneManager.GetZoneBlueprint(ZoneID).Builders.FirstOrDefault((ZoneBuilderBlueprint x) => x.Class == "MapBuilder")?.Create() as MapBuilder;
			}
			return _Builder;
		}
	}

	public InteriorZone GetZone(bool LiveOnly = false, bool BuiltOnly = false)
	{
		if (!Collapsed)
		{
			InteriorZone zone = _Zone;
			if (zone == null || zone.Stale)
			{
				_Zone = null;
				string zoneID = ZoneID;
				ZoneManager zoneManager = The.ZoneManager;
				if (LiveOnly)
				{
					if (zoneManager.CachedZones.TryGetValue(zoneID, out var value))
					{
						_Zone = value as InteriorZone;
					}
				}
				else if (BuiltOnly)
				{
					if (zoneManager.IsZoneBuilt(zoneID))
					{
						_Zone = zoneManager.GetZone(zoneID) as InteriorZone;
					}
				}
				else
				{
					_Zone = zoneManager.GetZone(zoneID) as InteriorZone;
					if (_Zone != null && _Zone.Location.IsClear())
					{
						_Zone.ParentObject = ParentObject;
					}
				}
				return _Zone;
			}
		}
		return _Zone;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != InventoryActionEvent.ID && ID != PooledEvent<GetTransitiveLocationEvent>.ID && ID != GetZoneSuspendabilityEvent.ID && ID != GetZoneFreezabilityEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetCarriedWeightEvent.ID && ID != GetExtrinsicWeightEvent.ID && (ID != SingletonEvent<FlushWeightCacheEvent>.ID || WeightCacheTurn == -1) && ID != PooledEvent<JoinPartyLeaderPossibleEvent>.ID && ID != TookDamageEvent.ID && ID != AfterDieEvent.ID && ID != BeforeDestroyObjectEvent.ID && ID != PooledEvent<IsVehicleOperationalEvent>.ID && ID != PooledEvent<IsRepairableEvent>.ID && ID != PooledEvent<RepairedEvent>.ID && ID != EnteringZoneEvent.ID && ID != EnteringCellEvent.ID)
		{
			return ID == PooledEvent<AfterPlayerBodyChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.CurrentCell != null && E.Actor != ParentObject)
		{
			E.AddAction("Enter", "enter", "EnterInterior", null, 'e', FireOnActor: false, 15);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCarriedWeightEvent E)
	{
		E.Weight += GetCarriedWeight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		E.Weight += GetCarriedWeight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FlushWeightCacheEvent E)
	{
		FlushWeightCache();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "EnterInterior")
		{
			TryEnter(E.Actor);
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		if (E.TargetCell.ParentZone.ZoneID == ZoneID)
		{
			return E.Result = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsVehicleOperationalEvent E)
	{
		if (E.Object == ParentObject && !Validate())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		if (E.Cell.ParentZone is InteriorZone && XRL.World.ZoneID.Match(E.Cell.ParentZone.ZoneID, ZoneID) >= 0)
		{
			E.RequestInterfaceExit();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteringCellEvent E)
	{
		GetZone(LiveOnly: true)?.Location.SetCell(E.Cell);
		return base.HandleEvent(E);
	}

	public int GetCarriedWeight()
	{
		if (IgnoreWeight)
		{
			return CarriedWeight = 0;
		}
		if (WeightCacheTurn >= 0 || !IsZoneLive)
		{
			return CarriedWeight;
		}
		MapBuilder builder = Builder;
		if (Builder?.Map == null)
		{
			return CarriedWeight;
		}
		InteriorZone zone = Zone;
		int num = builder.X;
		int num2 = builder.Y;
		int width = builder.Width;
		int height = builder.Height;
		if (num == -1)
		{
			num = zone.Width / 2 - width / 2;
		}
		if (num2 == -1)
		{
			num2 = zone.Height / 2 - height / 2;
		}
		CarriedWeight = 0;
		WeightCacheTurn = The.Game.Turns;
		List<int> list = null;
		int num3 = ParentObject.GetPart<Vehicle>()?.Passengers ?? 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				Cell cell = zone.GetCell(num + j, num2 + i);
				if (cell == null)
				{
					continue;
				}
				foreach (GameObject @object in cell.Objects)
				{
					if (!@object.IsReal || @object.HasIntProperty("InteriorRequired") || @object.HasTag("Cosmetic"))
					{
						continue;
					}
					int num4 = @object.Weight;
					if (num3 > 0 && @object.IsCombatObject())
					{
						int num5 = @object.IntrinsicWeight + Math.Min(@object.GetCarriedWeight(), @object.GetMaxCarriedWeight());
						if (list == null)
						{
							list = new List<int>();
						}
						list.Add(num5);
						num4 -= num5;
					}
					CarriedWeight += num4;
				}
			}
		}
		if (!list.IsNullOrEmpty() && list.Count > num3)
		{
			list.Sort();
			int k = 0;
			for (int num6 = list.Count - num3; k < num6; k++)
			{
				CarriedWeight += list[k];
			}
		}
		return CarriedWeight;
	}

	public void FlushWeightCache()
	{
		WeightCacheTurn = -1L;
		ParentObject.FlushCarriedWeightCache();
		if (ParentObject.CurrentZone is InteriorZone interiorZone)
		{
			FlushWeightCacheEvent.Send(interiorZone.ParentObject);
		}
	}

	public bool TryGetMap(out MapFile Map, out int MX, out int MY, out int Width, out int Height)
	{
		MapBuilder builder = Builder;
		Map = builder?.Map;
		if (Map == null)
		{
			MX = -1;
			MY = -1;
			Width = 0;
			Height = 0;
			return false;
		}
		MX = builder.X;
		MY = builder.Y;
		Width = builder.Width;
		Height = builder.Height;
		if (MX == -1)
		{
			MX = 40 - Width / 2;
		}
		if (MY == -1)
		{
			MY = 12 - Height / 2;
		}
		return true;
	}

	public bool Validate()
	{
		if (!TryGetMap(out var Map, out var MX, out var MY, out var Width, out var Height))
		{
			return true;
		}
		InteriorZone zone = Zone;
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				Cell cell = zone.GetCell(MX + j, MY + i);
				if (cell == null)
				{
					continue;
				}
				foreach (MapFileObjectBlueprint @object in Map.Cells[j, i].Objects)
				{
					if (@object.IntProperties == null)
					{
						continue;
					}
					int value = @object.IntProperties.GetValue("InteriorRequired", 0);
					if (value > 0)
					{
						GameObject firstObject = cell.GetFirstObject(@object.Name);
						if (firstObject == null)
						{
							return false;
						}
						if (value == 2 && (firstObject.IsBroken() || firstObject.IsRusted()))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public bool HasRequired(string Blueprint)
	{
		if (!TryGetMap(out var Map, out var _, out var _, out var Width, out var Height))
		{
			return false;
		}
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				foreach (MapFileObjectBlueprint @object in Map.Cells[j, i].Objects)
				{
					if (@object.IntProperties != null && !(@object.Name != Blueprint) && @object.IntProperties.ContainsKey("InteriorRequired"))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public int GetRequired(GameObject Object)
	{
		Cell cell = Object.CurrentCell;
		if (cell == null || cell.ParentZone.ZoneID != ZoneID)
		{
			return -1;
		}
		if (!TryGetMap(out var Map, out var MX, out var MY, out var _, out var _))
		{
			return -1;
		}
		int x = cell.X - MX;
		int y = cell.Y - MY;
		foreach (MapFileObjectBlueprint @object in Map.Cells[x, y].Objects)
		{
			if (!(@object.Name != Object.Blueprint))
			{
				if (@object.IntProperties == null)
				{
					return -1;
				}
				return @object.IntProperties.GetValue("InteriorRequired", -1);
			}
		}
		return -1;
	}

	public void Repair(GameObject Actor = null, GameObject Tool = null, IEvent ParentEvent = null)
	{
		MapBuilder builder = Builder;
		MapFile mapFile = Builder?.Map;
		if (mapFile == null)
		{
			return;
		}
		if (Collapsed)
		{
			_Zone = null;
			Collapsed = false;
		}
		InteriorZone zone = Zone;
		int num = builder.X;
		int num2 = builder.Y;
		int width = builder.Width;
		int height = builder.Height;
		if (num == -1)
		{
			num = zone.Width / 2 - width / 2;
		}
		if (num2 == -1)
		{
			num2 = zone.Height / 2 - height / 2;
		}
		RepairedEvent repairedEvent = PooledEvent<RepairedEvent>.FromPool();
		repairedEvent.Actor = Actor;
		repairedEvent.Tool = Tool;
		List<GameObject> list = Event.NewGameObjectList();
		List<GameObject> list2 = Event.NewGameObjectList();
		List<(Cell, MapFileObjectBlueprint)> list3 = new List<(Cell, MapFileObjectBlueprint)>();
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				Cell cell = zone.GetCell(num + j, num2 + i);
				if (cell == null)
				{
					continue;
				}
				list2.Clear();
				list2.AddRange(cell.Objects);
				foreach (MapFileObjectBlueprint obj in mapFile.Cells[j, i].Objects)
				{
					if (obj.HasProperty("InteriorRequired"))
					{
						int num3 = list2.FindIndex((GameObject x) => x.Blueprint == obj.Name);
						if (num3 >= 0)
						{
							repairedEvent.Subject = list2[num3];
							list2[num3].HandleEvent(repairedEvent, ParentEvent);
							list2.RemoveAt(num3);
						}
						else
						{
							list3.Add((cell, obj));
						}
					}
				}
				foreach (GameObject item in list2)
				{
					if (item.HasProperty("InteriorRequired"))
					{
						list.Add(item);
					}
				}
			}
		}
		foreach (var tuple in list3)
		{
			int num4 = list.FindIndex((GameObject x) => x.Blueprint == tuple.Item2.Name);
			if (num4 >= 0)
			{
				repairedEvent.Subject = list[num4];
				list[num4].HandleEvent(repairedEvent, ParentEvent);
				list[num4].SystemMoveTo(tuple.Item1, 0, forced: true);
				list.RemoveAt(num4);
			}
			else
			{
				tuple.Item1.AddObject(tuple.Item2.Create());
			}
		}
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		string tagOrStringProperty = ParentObject.GetTagOrStringProperty("RepairTool");
		if (tagOrStringProperty.IsNullOrEmpty())
		{
			return false;
		}
		if (E.Tool != null && E.Tool.GetBlueprint().DescendsFrom(tagOrStringProperty))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		Repair(E.Actor, E.Tool, E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTransitiveLocationEvent E)
	{
		if (E.Zone == ParentObject.CurrentZone && E.Origin?.ParentZone?.ZoneID == ZoneID && E.IsEgress)
		{
			Cell placementCell = GetPlacementCell(E.Actor);
			int priority = 500;
			string text = E.Source?.GetTagOrStringProperty("PortalKey");
			if (!text.IsNullOrEmpty() && text == ParentObject.GetTagOrStringProperty("PortalKey"))
			{
				priority = 1000;
			}
			E.AddLocation(placementCell, ParentObject, priority);
		}
		return base.HandleEvent(E);
	}

	public Cell GetPlacementCell(GameObject For = null)
	{
		Cell cell = ParentObject.CurrentCell;
		if (!cell.IsPassable(For))
		{
			return cell.getClosestPassableCellFor(For);
		}
		return cell;
	}

	public override bool HandleEvent(GetZoneSuspendabilityEvent E)
	{
		if (InteriorZone.Active?.ParentObject == ParentObject)
		{
			E.Suspendability = Suspendability.InsideInterior;
			E.Zone.MarkActive();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetZoneFreezabilityEvent E)
	{
		if (InteriorZone.Active?.ParentObject == ParentObject)
		{
			E.Freezability = Freezability.InsideInterior;
			E.Zone.MarkActive();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (E.NewBody == ParentObject || E.OldBody == ParentObject)
		{
			Zone?.HandleEvent(E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object == ParentObject && InteriorZone.Active?.ParentObject == ParentObject)
		{
			CombatJuice.cameraShake(0.5f);
			IComponent<GameObject>.AddPlayerMessage(Event.NewStringBuilder().Append(ParentObject.T()).Append(' ')
				.Append(ParentObject.GetVerb("take", PrependSpace: false))
				.Append(' ')
				.Append(E.Damage.Amount)
				.Append(' ')
				.Append("damage!")
				.ToString(), "R");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterDieEvent E)
	{
		ParentObject.PullDown();
		if (!Collapse(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		ParentObject.PullDown();
		if (!Collapse(E) && !E.Obliterate)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool Collapse(IEvent ParentEvent = null)
	{
		if (Collapsed)
		{
			return true;
		}
		if (!The.ZoneManager.IsZoneBuilt(ZoneID))
		{
			Collapsed = true;
			return true;
		}
		BeforeInteriorCollapseEvent beforeInteriorCollapseEvent = PooledEvent<BeforeInteriorCollapseEvent>.FromPool();
		InteriorZone interiorZone = (beforeInteriorCollapseEvent.Zone = Zone);
		beforeInteriorCollapseEvent.Object = ParentObject;
		if (!interiorZone.HandleEvent(beforeInteriorCollapseEvent, ParentEvent))
		{
			return false;
		}
		for (int i = 0; i < interiorZone.Height; i++)
		{
			for (int j = 0; j < interiorZone.Width; j++)
			{
				for (int num = interiorZone.Map[j][i].Objects.Count - 1; num >= 0; num--)
				{
					GameObject gameObject = interiorZone.Map[j][i].Objects[num];
					if (gameObject.IsCombatObject())
					{
						Cell escapeCell = interiorZone.GetEscapeCell(gameObject);
						gameObject.SystemMoveTo(escapeCell, 0, forced: true);
						if (FallDistance > 1)
						{
							StairsDown.InflictFallDamage(gameObject, FallDistance);
						}
					}
				}
			}
		}
		The.ZoneManager.DeleteZone(interiorZone);
		Collapsed = true;
		return true;
	}

	public bool CanEnter(GameObject Actor, bool Action = false, bool ShowMessage = false)
	{
		int Status = 0;
		if (ParentObject.CurrentCell == null || Collapsed)
		{
			Status = 2;
		}
		else if (!ParentObject.PhaseMatches(Actor))
		{
			Status = 4;
		}
		else if (!Actor.FlightCanReach(ParentObject))
		{
			Status = 8;
		}
		else if (Actor.IsGiganticCreature || Actor.HasPart(typeof(Vehicle)))
		{
			Status = 16;
		}
		else if (ParentObject.IsHostileTowards(Actor))
		{
			Status = 1;
		}
		if (!CanEnterInteriorEvent.Check(Actor, ParentObject, this, ref Status, ref Action, ref ShowMessage))
		{
			if (ShowMessage)
			{
				this.ShowMessage(Actor, Status);
			}
			return false;
		}
		return true;
	}

	public bool ShowMessage(GameObject Actor, int Status)
	{
		switch (Status)
		{
		case 4:
			Actor.Physics.DidXToY("are", "out of phase with", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true, FromDialog: false, Actor.IsPlayerControlled());
			return true;
		case 1:
			DidXToY("refuse", Actor, "entry", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true, FromDialog: false, Actor.IsPlayerControlled());
			return true;
		case 8:
			Actor.Physics.DidXToY("cannot", "reach", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true, FromDialog: false, Actor.IsPlayerControlled());
			return true;
		case 16:
			Actor.Physics.DidXToY("are", "too large to enter", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true, FromDialog: false, Actor.IsPlayerControlled());
			return true;
		case 2:
			Actor.Physics.DidXToY("are", "unable to enter", ParentObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true, FromDialog: false, Actor.IsPlayerControlled());
			return true;
		default:
			return false;
		}
	}

	public bool TryEnter(GameObject Actor, bool Force = false, bool ShowMessage = true)
	{
		if (Force || CanEnter(Actor, Action: true, ShowMessage))
		{
			GetTransitiveLocationEvent.GetFor(Zone, "Ingress,Interior", Actor, ParentObject, ParentObject.CurrentCell, out var Destination, out var Portal);
			if (Destination == null)
			{
				Destination = Zone.GetPullDownLocation(Actor);
			}
			if (Destination != null && Actor.DirectMoveTo(Destination, 0, Force))
			{
				string propertyOrTag = ParentObject.GetPropertyOrTag("EnterSound");
				if (!propertyOrTag.IsNullOrEmpty())
				{
					PlayWorldSound(propertyOrTag);
				}
				else
				{
					string propertyOrTag2 = ParentObject.GetPropertyOrTag("OpenSound");
					string text = Portal?.GetPropertyOrTag("OpenSound");
					string text2 = propertyOrTag2.Coalesce(text);
					if (!text2.IsNullOrEmpty())
					{
						if (Actor.IsPlayer())
						{
							if (propertyOrTag2 != text)
							{
								SoundManager.PlayWorldSound(text2, 15, Occluded: true, 0.5f, Destination.Location);
							}
						}
						else
						{
							PlayWorldSound(text2);
						}
					}
				}
				FlushWeightCache();
				Portal?.FireEvent("PortalTransition");
				return true;
			}
		}
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (InteriorZone.Active?.ParentObject == ParentObject)
		{
			ParentObject.CurrentZone.MarkActive();
			if (ParentObject.Brain != null)
			{
				ParentObject.Brain.Hibernating = true;
			}
		}
		else if (ParentObject.Brain != null)
		{
			ParentObject.Brain.Hibernating = false;
		}
	}
}
