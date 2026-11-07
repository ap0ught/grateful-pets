using System;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class BioAmmoLoader : IActivePart
{
	public int MaxCapacity = 12;

	public int Available = 12;

	public int TurnsToGenerate = 5;

	public int ReloadEnergy = 1000;

	public string ProjectileObject;

	public string StartAvailable;

	public string VariableTurnsToGenerate;

	public float TurnsToGenerateComputePowerFactor;

	public int TurnsGenerating;

	public BioAmmoLoader()
	{
		WorksOnSelf = true;
		IsBioScannable = true;
	}

	public override bool SameAs(IPart p)
	{
		BioAmmoLoader bioAmmoLoader = p as BioAmmoLoader;
		if (bioAmmoLoader.MaxCapacity != MaxCapacity)
		{
			return false;
		}
		if (bioAmmoLoader.Available != Available)
		{
			return false;
		}
		if (bioAmmoLoader.TurnsToGenerate != TurnsToGenerate)
		{
			return false;
		}
		if (bioAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (bioAmmoLoader.StartAvailable != StartAvailable)
		{
			return false;
		}
		if (bioAmmoLoader.VariableTurnsToGenerate != VariableTurnsToGenerate)
		{
			return false;
		}
		if (bioAmmoLoader.TurnsToGenerateComputePowerFactor != TurnsToGenerateComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID && (ID != AllowLiquidCollectionEvent.ID || ConsumesLiquid.IsNullOrEmpty()) && ID != PooledEvent<CheckLoadAmmoEvent>.ID && ID != PooledEvent<CheckReadyToFireEvent>.ID && ID != SingletonEvent<CommandReloadEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetAmmoCountAvailableEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMissileWeaponProjectileEvent>.ID && ID != PooledEvent<GetMissileWeaponStatusEvent>.ID && ID != PooledEvent<GetNotReadyToFireMessageEvent>.ID && ID != PooledEvent<GetProjectileBlueprintEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<LoadAmmoEvent>.ID && ID != SingletonEvent<NeedsReloadEvent>.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (!StartAvailable.IsNullOrEmpty())
		{
			Available = StartAvailable.RollCached();
		}
		if (!VariableTurnsToGenerate.IsNullOrEmpty())
		{
			TurnsToGenerate = VariableTurnsToGenerate.RollCached();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		if (Available <= 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		E.Register(Available);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckLoadAmmoEvent E)
	{
		if (Available <= 0)
		{
			if (E.Message == null)
			{
				E.Message = ParentObject.Does("are") + " exhausted!";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LoadAmmoEvent E)
	{
		if (Available > 0)
		{
			if (!ProjectileObject.IsNullOrEmpty())
			{
				E.Projectile = GameObject.Create(ProjectileObject, 0, 0, null, null, null, "Projectile");
			}
			Available--;
			return base.HandleEvent(E);
		}
		if (E.Message == null)
		{
			E.Message = ParentObject.Does("are") + " exhausted!";
		}
		return false;
	}

	public override bool HandleEvent(CheckReadyToFireEvent E)
	{
		if (Available <= 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		if (Available <= 0)
		{
			E.Message = ParentObject.Does("are") + " exhausted!";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		if (E.Override == null)
		{
			if (E.Status != null)
			{
				E.Status.ammoTotal = MaxCapacity;
				E.Status.ammoRemaining = Available;
			}
			if (Available <= 0)
			{
				E.Items.Append(" [{{K|empty}}]");
			}
			else
			{
				E.Items.Append(" [").Append(Available).Append(']');
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

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (TurnsToGenerateComputePowerFactor > 0f)
		{
			E.Postfix.AppendRules("Compute power on the local lattice decreases the time needed for this item to generate ammunition.");
		}
		else if (TurnsToGenerateComputePowerFactor < 0f)
		{
			E.Postfix.AppendRules("Compute power on the local lattice increases the time needed for this item to generate ammunition.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (!ConsumesLiquid.IsNullOrEmpty() && E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && (!CorrectLiquid(liquidVolume) || liquidVolume.Volume < liquidVolume.MaxVolume) && ParentObject.IsEquippedProperly())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if (!ConsumesLiquid.IsNullOrEmpty() && (E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			if (ParentObject.IsEquippedProperly() && E.MinimumCharge <= 0)
			{
				LiquidVolume liquidVolume = ParentObject.LiquidVolume;
				if (liquidVolume != null)
				{
					if (CorrectLiquid(liquidVolume) && liquidVolume.Volume >= liquidVolume.MaxVolume)
					{
						if (E.Actor.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " already full.");
						}
					}
					else
					{
						E.NeededReload.Add(this);
						int freeDrams = E.Actor.GetFreeDrams(ConsumesLiquid, ParentObject);
						if (freeDrams <= 0)
						{
							if (E.Actor.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You have no " + ConsumesLiquid + " for " + ParentObject.t() + ".", 'r');
							}
						}
						else
						{
							E.TriedToReload.Add(this);
							string text = ParentObject.t();
							if (liquidVolume.Volume > 0 && !CorrectLiquid(liquidVolume))
							{
								if (E.Actor.IsPlayer())
								{
									IComponent<GameObject>.AddPlayerMessage("You dump the " + liquidVolume.GetLiquidName() + " out of " + text + ".");
								}
								liquidVolume.EmptyIntoCell();
								text = ParentObject.t();
							}
							int val = liquidVolume.MaxVolume - liquidVolume.Volume;
							int num = Math.Min(freeDrams, val);
							E.Actor.UseDrams(num, ConsumesLiquid, ParentObject);
							liquidVolume.MixWith(new LiquidVolume(ConsumesLiquid, num));
							E.Reloaded.Add(this);
							if (!E.ObjectsReloaded.Contains(ParentObject))
							{
								E.ObjectsReloaded.Add(ParentObject);
							}
							E.EnergyCost(ReloadEnergy);
							PlayWorldSound(ParentObject.GetTag("ReloadSound"));
							if (E.Actor.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You " + ((liquidVolume.Volume < liquidVolume.MaxVolume) ? "partially " : "") + "fill " + text + " with " + liquidVolume.GetLiquidName() + ".");
							}
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
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

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Available < MaxCapacity && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			TurnsGenerating++;
			int num = GetAvailableComputePowerEvent.AdjustDown(this, TurnsToGenerate, TurnsToGenerateComputePowerFactor);
			if (TurnsGenerating >= num)
			{
				Available++;
				TurnsGenerating = 0;
				if (!VariableTurnsToGenerate.IsNullOrEmpty())
				{
					TurnsToGenerate = VariableTurnsToGenerate.RollCached();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));
			if (!E.Reference)
			{
				if (Available <= 0)
				{
					E.AddTag("{{y|[{{K|empty}}]}}", -5);
				}
				else
				{
					E.AddTag("{{y|[" + Available + "]}}", -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GenerateIntegratedHostInitialAmmo");
		Registrar.Register("PrepIntegratedHostToReceiveAmmo");
		Registrar.Register("SupplyIntegratedHostWithAmmo");
		base.Register(Object, Registrar);
	}

	private bool CorrectLiquid(LiquidVolume Volume)
	{
		if (!ConsumesLiquid.IsNullOrEmpty())
		{
			if (!Volume.ComponentLiquids.ContainsKey(ConsumesLiquid))
			{
				return false;
			}
			if (LiquidMustBePure && !Volume.IsPureLiquid(ConsumesLiquid))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		if (ConsumesLiquid.IsNullOrEmpty())
		{
			return true;
		}
		return WantsLiquidCollection(LiquidType);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PrepIntegratedHostToReceiveAmmo")
		{
			if (!ConsumesLiquid.IsNullOrEmpty())
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Host");
				if (gameObjectParameter != null)
				{
					Body body = gameObjectParameter.Body;
					if (body != null)
					{
						foreach (BodyPart item in body.GetPart("Arm"))
						{
							if (item.Equipped == null)
							{
								gameObjectParameter.ForceEquipObject(GameObject.Create("Gourd"), item, Silent: true, 0);
							}
						}
					}
				}
			}
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (!ConsumesLiquid.IsNullOrEmpty())
			{
				if (E.HasFlag("TrackSupply"))
				{
					E.SetFlag("AnySupplyHandler", State: true);
				}
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
				GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
				if (gameObjectParameter2 != null && gameObject != null)
				{
					int freeDrams = gameObject.GetFreeDrams(ConsumesLiquid, ParentObject);
					int storableDrams = gameObjectParameter2.GetStorableDrams(ConsumesLiquid);
					int num = Math.Min(freeDrams, storableDrams);
					if (gameObject.IsPlayer())
					{
						if (num > 0)
						{
							if (E.HasFlag("TrackSupply"))
							{
								E.SetFlag("AnySupplies", State: true);
							}
							num.ToString();
							int? num2 = Popup.AskNumber("Supply " + gameObjectParameter2.t() + " with how many drams of your " + ConsumesLiquid + "? (max=" + num + ")", "Sounds/UI/ui_notification", "", num, 0, num);
							int num3 = 0;
							try
							{
								num3 = Convert.ToInt32(num2);
							}
							catch
							{
								goto IL_0416;
							}
							if (num3 > num)
							{
								num3 = num;
							}
							if (num3 < 0)
							{
								num3 = 0;
							}
							num = num3;
						}
						else if (freeDrams <= 0)
						{
							Popup.Show("You have no " + ConsumesLiquid + " to supply " + gameObjectParameter2.t() + " with.");
						}
						else if (storableDrams <= 0)
						{
							Popup.Show(gameObjectParameter2.Does("have") + " no room for more " + ConsumesLiquid + ".");
						}
					}
					if (num > 0)
					{
						IComponent<GameObject>.XDidYToZ(gameObject, "transfer", Grammar.Cardinal(num) + " " + ((num == 1) ? "dram" : "drams") + " of " + ConsumesLiquid + " to", gameObjectParameter2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
						gameObject.UseDrams(num, ConsumesLiquid);
						gameObjectParameter2.GiveDrams(num, ConsumesLiquid);
						gameObject.UseEnergy(1000, "Ammo Liquid Transfer");
					}
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo" && !ConsumesLiquid.IsNullOrEmpty())
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter3 != null)
			{
				int storableDrams2 = gameObjectParameter3.GetStorableDrams(ConsumesLiquid);
				if (storableDrams2 > 0)
				{
					gameObjectParameter3.GiveDrams(storableDrams2, ConsumesLiquid);
				}
			}
		}
		goto IL_0416;
		IL_0416:
		return base.FireEvent(E);
	}
}
