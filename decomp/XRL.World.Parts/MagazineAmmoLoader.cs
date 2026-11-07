using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MagazineAmmoLoader : IPoweredPart
{
	public int MaxAmmo = 6;

	public string ID = "";

	public GameObject Ammo;

	public int ReloadEnergy = 1000;

	public string AmmoPart = "";

	public string ProjectileObject;

	[NonSerialized]
	private static Dictionary<string, List<GameObjectBlueprint>> AmmoBlueprints = new Dictionary<string, List<GameObjectBlueprint>>();

	public new int ChargeUse
	{
		get
		{
			return 0;
		}
		set
		{
			throw new Exception("cannot set ChargeUse on a MagazineAmmoLoader");
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		MagazineAmmoLoader magazineAmmoLoader = new MagazineAmmoLoader();
		magazineAmmoLoader.MaxAmmo = MaxAmmo;
		magazineAmmoLoader.ID = ID;
		if (Ammo != null)
		{
			magazineAmmoLoader.Ammo = MapInv?.Invoke(Ammo) ?? Ammo.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (magazineAmmoLoader.Ammo != null)
			{
				magazineAmmoLoader.Ammo.ForeachPartDescendedFrom((IAmmo p) =>
				{
					p.LoadedIn = Parent;
				});
			}
		}
		magazineAmmoLoader.ReloadEnergy = ReloadEnergy;
		magazineAmmoLoader.AmmoPart = AmmoPart;
		magazineAmmoLoader.ProjectileObject = ProjectileObject;
		magazineAmmoLoader.ParentObject = Parent;
		return magazineAmmoLoader;
	}

	public MagazineAmmoLoader()
	{
		base.ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		IsPowerSwitchSensitive = false;
		NameForStatus = "FiringMechanism";
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		MagazineAmmoLoader magazineAmmoLoader = p as MagazineAmmoLoader;
		if (magazineAmmoLoader.MaxAmmo != MaxAmmo)
		{
			return false;
		}
		if (magazineAmmoLoader.ID != ID)
		{
			return false;
		}
		if (magazineAmmoLoader.Ammo != Ammo)
		{
			return false;
		}
		if (magazineAmmoLoader.ReloadEnergy != ReloadEnergy)
		{
			return false;
		}
		if (magazineAmmoLoader.AmmoPart != AmmoPart)
		{
			return false;
		}
		if (magazineAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void SetAmmo(GameObject Object)
	{
		if (Object == Ammo)
		{
			return;
		}
		if (Ammo != null)
		{
			Ammo.ForeachPartDescendedFrom((IAmmo p) =>
			{
				p.LoadedIn = null;
			});
		}
		Ammo = Object;
		Object?.ForeachPartDescendedFrom((IAmmo p) =>
		{
			p.LoadedIn = ParentObject;
		});
		if (Sidebar.CurrentTarget == Object)
		{
			Sidebar.CurrentTarget = null;
		}
		FlushTransientCaches();
	}

	private bool AmmoWantsEvent(int ID, int cascade)
	{
		if (!GameObject.Validate(ref Ammo))
		{
			return false;
		}
		if (!MinEvent.CascadeTo(cascade, 4))
		{
			return false;
		}
		return Ammo.WantEvent(ID, cascade);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID && ID != PooledEvent<CheckLoadAmmoEvent>.ID && ID != PooledEvent<CheckReadyToFireEvent>.ID && ID != SingletonEvent<CommandReloadEvent>.ID && ID != PooledEvent<GetAmmoCountAvailableEvent>.ID && ID != PooledEvent<GetContentsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<GetMissileWeaponProjectileEvent>.ID && ID != PooledEvent<GetMissileWeaponStatusEvent>.ID && ID != PooledEvent<GetProjectileBlueprintEvent>.ID && ID != InventoryActionEvent.ID && ID != PooledEvent<LoadAmmoEvent>.ID && ID != SingletonEvent<NeedsReloadEvent>.ID && ID != PooledEvent<ShotCompleteEvent>.ID && ID != PooledEvent<StripContentsEvent>.ID && ID != AutoexploreObjectEvent.ID)
		{
			return AmmoWantsEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (Ammo == null)
		{
			Inventory inventory = E.Actor?.Inventory;
			bool flag = false;
			foreach (GameObject item in inventory.GetObjectsDirect())
			{
				if (IsValidAmmo(item))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ShotCompleteEvent E)
	{
		if (ReloadEnergy == 0)
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
			{
				CommandReloadEvent.Execute(equipped, ParentObject, E.LoadedAmmo);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		if (Ammo != null)
		{
			E.Register(Ammo.Count);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckLoadAmmoEvent E)
	{
		if (Ammo == null)
		{
			CheckLoadAmmoEvent checkLoadAmmoEvent = E;
			if (checkLoadAmmoEvent.Message == null)
			{
				checkLoadAmmoEvent.Message = ParentObject.Does("have") + " no more ammo!";
			}
			return false;
		}
		if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.Operational)
		{
			CheckLoadAmmoEvent checkLoadAmmoEvent = E;
			if (checkLoadAmmoEvent.Message == null)
			{
				checkLoadAmmoEvent.Message = ParentObject.Does("are") + " jammed!";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LoadAmmoEvent E)
	{
		if (Ammo == null)
		{
			LoadAmmoEvent loadAmmoEvent = E;
			if (loadAmmoEvent.Message == null)
			{
				loadAmmoEvent.Message = ParentObject.Does("have") + " no more ammo!";
			}
			return false;
		}
		if (GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) != ActivePartStatus.Operational)
		{
			LoadAmmoEvent loadAmmoEvent = E;
			if (loadAmmoEvent.Message == null)
			{
				loadAmmoEvent.Message = ParentObject.Does("are") + " jammed!";
			}
			return false;
		}
		E.LoadedAmmo = Ammo.RemoveOne();
		if (E.LoadedAmmo == Ammo)
		{
			SetAmmo(null);
		}
		if (ProjectileObject.IsNullOrEmpty())
		{
			E.Projectile = GetProjectileObjectEvent.GetFor(E.LoadedAmmo, ParentObject);
		}
		else
		{
			E.Projectile = GameObject.Create(ProjectileObject, 0, 0, null, null, null, "Projectile");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckReadyToFireEvent E)
	{
		if (Ammo == null || IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (GameObject.Validate(ref Ammo))
		{
			Ammo.HandleEvent(E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		if (E.Override == null)
		{
			if (E.Status != null)
			{
				E.Status.ammoRemaining = ((Ammo != null) ? Ammo.Count : 0);
				E.Status.ammoTotal = MaxAmmo;
			}
			if (Ammo == null)
			{
				E.Items.Append(" [{{K|empty}}]");
			}
			else
			{
				int count = Ammo.Count;
				string value = GetMissileStatusColorEvent.GetFor(Ammo);
				E.Items.Append(" [");
				if (!value.IsNullOrEmpty())
				{
					E.Items.Append("{{").Append(value).Append('|');
				}
				E.Items.Append(count);
				if (!value.IsNullOrEmpty())
				{
					E.Items.Append("}}");
				}
				E.Items.Append("]");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetProjectileBlueprintEvent E)
	{
		if (!ProjectileObject.IsNullOrEmpty())
		{
			E.Blueprint = ProjectileObject;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject) && (Ammo == null || Ammo.Count < MaxAmmo) && ParentObject.IsEquippedProperly())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if (E.Pass >= 2 && (E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
			if ((part == null || part.FiresManually) && E.MinimumCharge <= 0 && (E.Weapon == ParentObject || ParentObject.IsEquippedProperly()))
			{
				bool flag = Ammo == null || Ammo.Count < MaxAmmo;
				if (flag)
				{
					E.NeededReload.Add(this);
				}
				if (flag || (E.NeededReload.Count <= 0 && !NeedsReloadEvent.Check(E.Actor, this)))
				{
					List<GameObject> list = Event.NewGameObjectList();
					foreach (GameObject item in E.Actor.GetInventory())
					{
						if (IsValidAmmo(item))
						{
							list.Add(item);
						}
					}
					if (list.Count == 0)
					{
						if (E.Actor.IsPlayer())
						{
							E.Actor.PlayWorldSound("sfx_missile_reloadFail_noAmmo");
							IComponent<GameObject>.EmitMessage(E.Actor, "You have no more ammo for " + ParentObject.t() + ".", ' ', E.FromDialog);
						}
						return true;
					}
					if (!flag && list.Count == 1 && Ammo != null && Ammo.Blueprint == list[0].Blueprint)
					{
						if (E.Actor.IsPlayer())
						{
							IComponent<GameObject>.EmitMessage(E.Actor, ParentObject.Does("are") + " already fully loaded.", ' ', E.FromDialog);
						}
						return true;
					}
					GameObject gameObject = null;
					if (GameObject.Validate(ref E.LastAmmo))
					{
						for (int i = 0; i < list.Count; i++)
						{
							if (list[i].Blueprint == E.LastAmmo.Blueprint)
							{
								gameObject = list[i];
								break;
							}
						}
						if (gameObject == null)
						{
							return false;
						}
					}
					if (gameObject == null)
					{
						if (list.Count > 1)
						{
							if (E.Actor.IsPlayer())
							{
								gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
								if (gameObject == null)
								{
									return false;
								}
							}
							else
							{
								List<GameObject> list2 = Event.NewGameObjectList();
								foreach (GameObject item2 in list)
								{
									if (!item2.IsImportant())
									{
										int j = 0;
										for (int num = Math.Min(item2.Count, 10); j < num; j++)
										{
											list2.Add(item2);
										}
									}
								}
								gameObject = list2.GetRandomElement();
							}
						}
						else
						{
							gameObject = list[0];
						}
					}
					if (gameObject.ConfirmUseImportant(E.Actor, "load"))
					{
						E.TriedToReload.Add(this);
						Unload(E.Actor);
						if (Load(E.Actor, gameObject, E.FromDialog))
						{
							E.Reloaded.Add(this);
							if (!E.ObjectsReloaded.Contains(ParentObject))
							{
								E.ObjectsReloaded.Add(ParentObject);
							}
							E.EnergyCost(ReloadEnergy);
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(4) && GameObject.Validate(ref Ammo) && !E.Dispatch(Ammo))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		if (!ProjectileObject.IsNullOrEmpty())
		{
			E.Blueprint = ProjectileObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (GameObject.Validate(ref Ammo))
		{
			E.Value += Ammo.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (GameObject.Validate(ref Ammo))
		{
			E.Weight += Ammo.Weight;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		if (GameObject.Validate(ref Ammo) && (!E.KeepNatural || !Ammo.IsNatural()))
		{
			Ammo.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		if (GameObject.Validate(ref Ammo))
		{
			E.Objects.Add(Ammo);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Load Ammo", "load", "LoadMagazineAmmo", null, 'o');
		if (GameObject.Validate(ref Ammo))
		{
			E.AddAction("Unload Ammo", "unload", "UnloadMagazineAmmo", null, 'u');
			GetSlottedInventoryActionsEvent.Send(Ammo, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LoadMagazineAmmo")
		{
			CommandReloadEvent.Execute(E.Actor, ParentObject, null, FreeAction: false, FromDialog: true);
		}
		else if ((E.Command == "UnloadMagazineAmmo" || E.Command == "EmptyForDisassemble") && GameObject.Validate(ref Ammo))
		{
			ParentObject.PlayWorldSoundTag("UnloadSound");
			ParentObject.GetContext(out var ObjectContext, out var CellContext);
			if (ObjectContext != null)
			{
				ObjectContext.ReceiveObject(Ammo);
			}
			else if (CellContext != null)
			{
				CellContext.AddObject(Ammo);
			}
			else
			{
				E.Actor.ReceiveObject(Ammo);
				E.Generated.Add(Ammo);
			}
			SetAmmo(null);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			if (E.Cutoff >= 1100)
			{
				E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject, (Ammo != null && ProjectileObject.IsNullOrEmpty()) ? GetProjectileObjectEvent.GetFor(Launcher: ParentObject, Ammo: Ammo) : null));
			}
			if (!E.Reference)
			{
				if (GameObject.Validate(ref Ammo))
				{
					E.AddTag("{{y|[" + Ammo.DisplayNameOnly + "]}}", -5);
				}
				else
				{
					E.AddTag("{{y|[{{K|empty}}]}}", -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GenerateIntegratedHostInitialAmmo");
		Registrar.Register("SupplyIntegratedHostWithAmmo");
		base.Register(Object, Registrar);
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (GameObject.Validate(ref Ammo))
		{
			Ammo.TurnTick(TimeTick, Amount);
		}
	}

	public void Unload(GameObject Loader)
	{
		try
		{
			ParentObject.SplitFromStack();
			MessageQueue.Suppress = true;
			InventoryActionEvent.Check(ParentObject, Loader, ParentObject, "UnloadMagazineAmmo");
			MessageQueue.Suppress = false;
		}
		catch
		{
			MessageQueue.Suppress = false;
		}
	}

	public bool Load(GameObject Loader, GameObject ChosenAmmo, bool FromDialog = false)
	{
		try
		{
			ParentObject.SplitFromStack();
			MessageQueue.Suppress = true;
			ChosenAmmo.SplitStack(MaxAmmo, Loader);
			Event obj = Event.New("CommandRemoveObject");
			obj.SetParameter("Object", ChosenAmmo);
			obj.SetFlag("ForEquip", State: true);
			if (Loader.FireEvent(obj))
			{
				SetAmmo(ChosenAmmo);
				ParentObject.FireEvent("MagazineAmmoLoaderReloaded");
				MessageQueue.Suppress = false;
				PlayWorldSound(ParentObject.GetPropertyOrTag("ReloadSound"));
				if (Loader.IsPlayer())
				{
					IComponent<GameObject>.EmitMessage(Loader, "You reload " + ParentObject.t() + " with " + ((Ammo.Count == 1) ? Ammo.an() : Ammo.ShortDisplayName) + ".", ' ', FromDialog);
				}
				return true;
			}
			return false;
		}
		catch (Exception message)
		{
			MetricsManager.LogError(message);
			return false;
		}
		finally
		{
			MessageQueue.Suppress = false;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (E.HasFlag("TrackSupply"))
			{
				E.SetFlag("AnySupplyHandler", State: true);
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Host");
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			if (gameObjectParameter != null && gameObject != null)
			{
				int desiredAmmoCount = GetDesiredAmmoCount();
				int num = GetAccessibleAmmoCount();
				Inventory inventory = gameObject.Inventory;
				if (inventory != null)
				{
					List<GameObject> list = Event.NewGameObjectList();
					inventory.GetObjects(list, IsValidAmmo);
					foreach (GameObject item in list)
					{
						int count = item.Count;
						int num2 = 0;
						if (gameObject.IsPlayer())
						{
							if (E.HasFlag("TrackSupply"))
							{
								E.SetFlag("AnySupplies", State: true);
							}
							Math.Min(desiredAmmoCount - num, count);
							int? num3 = Popup.AskNumber("Supply " + gameObjectParameter.t() + " with how many " + (item.HasProperName ? ("of " + item.DisplayNameSingle) : Grammar.Pluralize(item.DisplayNameSingle)) + "? (max=" + count + ")", "Sounds/UI/ui_notification", "", count, 0, count);
							int num4 = 0;
							try
							{
								num4 = Convert.ToInt32(num3);
							}
							catch
							{
								break;
							}
							if (num4 > count)
							{
								num4 = count;
							}
							if (num4 < 0)
							{
								num4 = 0;
							}
							num2 = num4;
						}
						else if (desiredAmmoCount > num)
						{
							num2 = Math.Min(desiredAmmoCount - num, count);
						}
						if (num2 > 0)
						{
							IComponent<GameObject>.XDidYToZ(gameObject, "transfer", item.HasProperName ? item.DisplayNameOnly : (((num2 == 1) ? item.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) : (Grammar.Cardinal(num2) + " " + Grammar.Pluralize(item.ShortDisplayNameSingle))) + " to"), gameObjectParameter, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
							if (num2 < count)
							{
								item.Split(num2);
							}
							gameObjectParameter.ReceiveObject(item);
							gameObject.UseEnergy(1000, "Ammo Magazine Transfer");
							num += num2;
						}
					}
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter2 != null && gameObjectParameter2.Inventory != null)
			{
				int desiredAmmoCount2 = GetDesiredAmmoCount();
				int num5 = GetAccessibleAmmoCount();
				if (num5 < desiredAmmoCount2)
				{
					List<GameObjectBlueprint> ammoBlueprints = GetAmmoBlueprints();
					if (ammoBlueprints.Count == 1)
					{
						num5 += gameObjectParameter2.ReceiveObject(ammoBlueprints[0].Name, desiredAmmoCount2 - num5);
					}
					else if (ammoBlueprints.Count > 1)
					{
						int num6 = (desiredAmmoCount2 - num5) / ammoBlueprints.Count;
						if (num6 > 0)
						{
							foreach (GameObjectBlueprint item2 in ammoBlueprints)
							{
								num5 += gameObjectParameter2.ReceiveObject(item2.Name, num6);
							}
						}
						if (num5 < desiredAmmoCount2)
						{
							int num7 = 0;
							int num8 = desiredAmmoCount2 - num5 + 100;
							while (num5 < desiredAmmoCount2 && ++num7 < num8)
							{
								if (gameObjectParameter2.ReceiveObject(ammoBlueprints.GetRandomElement().Name))
								{
									num5++;
								}
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	private int GetDesiredAmmoCount()
	{
		MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
		int num = ParentObject.GetIntProperty("IntegratedWeaponHostShots");
		if (num <= 0)
		{
			if (MaxAmmo == 1)
			{
				num = 50;
			}
			else
			{
				num = ((MaxAmmo != 2) ? 200 : 100);
			}
		}
		return part.AmmoPerAction * num;
	}

	private int GetAccessibleAmmoCount()
	{
		int num = ((Ammo != null) ? Ammo.Count : 0);
		Inventory inventory = ParentObject.Holder?.Inventory;
		if (inventory != null)
		{
			foreach (GameObject @object in inventory.Objects)
			{
				if (@object.HasPart(AmmoPart))
				{
					num += @object.Count;
				}
			}
		}
		return num;
	}

	private static List<GameObjectBlueprint> GetAmmoBlueprints(string ForAmmoPart)
	{
		if (!AmmoBlueprints.ContainsKey(ForAmmoPart))
		{
			bool flag = false;
			List<GameObjectBlueprint> list = new List<GameObjectBlueprint>();
			{
				foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
				{
					if (!blueprint.HasPart(ForAmmoPart) || blueprint.HasTag("ExcludeFromDynamicEncounters") || blueprint.HasTag("BaseObject") || blueprint.HasTag("ExcludeFromTurretStock"))
					{
						continue;
					}
					if (flag)
					{
						if (blueprint.HasTag("TurretStockExclusive"))
						{
							AddAmmoBlueprint(list, blueprint);
						}
					}
					else if (blueprint.HasTag("TurretStockExclusive"))
					{
						list.Clear();
						AddAmmoBlueprint(list, blueprint);
						flag = true;
					}
					else
					{
						AddAmmoBlueprint(list, blueprint);
					}
				}
				return list;
			}
		}
		return AmmoBlueprints[ForAmmoPart];
	}

	private static void AddAmmoBlueprint(List<GameObjectBlueprint> List, GameObjectBlueprint BP)
	{
		string tag = BP.GetTag("TurretStockWeight");
		if (tag.IsNullOrEmpty())
		{
			List.Add(BP);
			return;
		}
		try
		{
			int num = Convert.ToInt32(tag);
			for (int i = 0; i < num; i++)
			{
				List.Add(BP);
			}
		}
		catch
		{
		}
	}

	private List<GameObjectBlueprint> GetAmmoBlueprints()
	{
		return GetAmmoBlueprints(AmmoPart);
	}

	public bool IsValidAmmo(GameObject Object)
	{
		if (!Object.HasPart(AmmoPart))
		{
			return Object.HasTagOrProperty(AmmoPart);
		}
		return true;
	}

	public bool IsValidAmmo(GameObjectBlueprint Blueprint)
	{
		if (!Blueprint.HasPart(AmmoPart))
		{
			return Blueprint.HasTagOrProperty(AmmoPart);
		}
		return true;
	}
}
