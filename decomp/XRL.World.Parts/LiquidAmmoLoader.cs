using System;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class LiquidAmmoLoader : IActivePart
{
	public string ID = "";

	public string Liquid = "water";

	public int ReloadEnergy = 1000;

	public string ProjectileObject;

	public bool ShowDamage;

	public int ShotsPerDram = 1;

	public int ShotsTakenOnCurrentDram;

	public LiquidAmmoLoader()
	{
		WorksOnEquipper = true;
		IsTechScannable = true;
		NameForStatus = "LiquidProjector";
	}

	public override bool SameAs(IPart p)
	{
		LiquidAmmoLoader liquidAmmoLoader = p as LiquidAmmoLoader;
		if (liquidAmmoLoader.ID != ID)
		{
			return false;
		}
		if (liquidAmmoLoader.Liquid != Liquid)
		{
			return false;
		}
		if (liquidAmmoLoader.ReloadEnergy != ReloadEnergy)
		{
			return false;
		}
		if (liquidAmmoLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (liquidAmmoLoader.ShowDamage != ShowDamage)
		{
			return false;
		}
		if (liquidAmmoLoader.ShotsPerDram != ShotsPerDram)
		{
			return false;
		}
		if (liquidAmmoLoader.ShotsTakenOnCurrentDram != ShotsTakenOnCurrentDram)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID && ID != AllowLiquidCollectionEvent.ID && ID != PooledEvent<CheckLoadAmmoEvent>.ID && ID != PooledEvent<CheckReadyToFireEvent>.ID && ID != SingletonEvent<CommandReloadEvent>.ID && ID != PooledEvent<GetAmmoCountAvailableEvent>.ID && (ID != PooledEvent<GetDisplayNameEvent>.ID || !ShowDamage) && ID != PooledEvent<GetMissileWeaponProjectileEvent>.ID && ID != PooledEvent<GetMissileWeaponStatusEvent>.ID && ID != PooledEvent<GetNotReadyToFireMessageEvent>.ID && ID != GetPreferredLiquidEvent.ID && ID != PooledEvent<GetProjectileBlueprintEvent>.ID && ID != PooledEvent<LoadAmmoEvent>.ID && ID != SingletonEvent<NeedsReloadEvent>.ID)
		{
			return ID == WantsLiquidCollectionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (!liquidVolume.IsPureLiquid(Liquid) || liquidVolume.Volume <= 0)
		{
			bool flag = false;
			Inventory inventory = E.Actor?.Inventory;
			if (inventory != null)
			{
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					LiquidVolume liquidVolume2 = item.LiquidVolume;
					if (liquidVolume2 != null && liquidVolume2.IsPureLiquid(Liquid) && liquidVolume2.Volume > 0 && !liquidVolume2.EffectivelySealed())
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		if (IsFuelGood())
		{
			E.Register(ParentObject.LiquidVolume.Volume * ShotsPerDram - ShotsTakenOnCurrentDram);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckLoadAmmoEvent E)
	{
		if (!IsFuelGood() || IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (E.Message == null)
			{
				E.Message = GetStatusMessage();
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LoadAmmoEvent E)
	{
		if (!IsFuelGood())
		{
			LoadAmmoEvent loadAmmoEvent = E;
			if (loadAmmoEvent.Message == null)
			{
				loadAmmoEvent.Message = GetStatusMessage();
			}
			return false;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			LoadAmmoEvent loadAmmoEvent = E;
			if (loadAmmoEvent.Message == null)
			{
				loadAmmoEvent.Message = GetStatusMessage(activePartStatus);
			}
			return false;
		}
		if (++ShotsTakenOnCurrentDram >= ShotsPerDram)
		{
			ParentObject.LiquidVolume?.UseDram();
			ShotsTakenOnCurrentDram = 0;
		}
		if (!ProjectileObject.IsNullOrEmpty())
		{
			E.Projectile = GameObject.Create(ProjectileObject, 0, 0, null, null, null, "Projectile");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckReadyToFireEvent E)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (!liquidVolume.IsPureLiquid(Liquid))
		{
			return false;
		}
		if (liquidVolume.Volume <= 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		if (!IsFuelGood() || IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Message = GetStatusMessage();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		if (E.Override == null)
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (E.Status != null)
			{
				if (!liquidVolume.IsPureLiquid(Liquid))
				{
					E.Status.ammoRemaining = 0;
					E.Status.ammoTotal = 0;
				}
				else
				{
					E.Status.ammoTotal = liquidVolume.MaxVolume * ShotsPerDram;
					E.Status.ammoRemaining = liquidVolume.Volume * ShotsPerDram;
				}
			}
			string primaryLiquidColor = liquidVolume.GetPrimaryLiquidColor();
			if (liquidVolume.Volume == 0)
			{
				if (!primaryLiquidColor.IsNullOrEmpty())
				{
					E.Items.Append(" [{{").Append(primaryLiquidColor).Append('|')
						.Append("empty")
						.Append("}}]");
				}
				else
				{
					E.Items.Append(" [empty]");
				}
			}
			else if (liquidVolume.IsPureLiquid(Liquid))
			{
				if (!primaryLiquidColor.IsNullOrEmpty())
				{
					E.Items.Append(" [{{").Append(primaryLiquidColor).Append('|')
						.Append(liquidVolume.Volume * ShotsPerDram)
						.Append("}}]");
				}
				else
				{
					E.Items.Append(" [").Append(liquidVolume.Volume * ShotsPerDram).Append("]");
				}
			}
			else if (!primaryLiquidColor.IsNullOrEmpty())
			{
				E.Items.Append(" [{{").Append(primaryLiquidColor).Append('|')
					.Append("?")
					.Append("}}]");
			}
			else
			{
				E.Items.Append(" [?]");
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
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && (!liquidVolume.IsPureLiquid(Liquid) || liquidVolume.Volume < liquidVolume.MaxVolume) && ParentObject.IsEquippedProperly())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if ((E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
			if ((part == null || part.FiresManually) && E.MinimumCharge <= 0 && (E.Weapon == ParentObject || ParentObject.IsEquippedProperly()))
			{
				LiquidVolume liquidVolume = ParentObject.LiquidVolume;
				if (liquidVolume.IsPureLiquid(Liquid) && liquidVolume.Volume >= liquidVolume.MaxVolume)
				{
					if (E.Actor.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " already full of " + liquidVolume.GetLiquidName() + ".");
					}
					return true;
				}
				E.NeededReload.Add(this);
				int freeDrams = E.Actor.GetFreeDrams(Liquid, ParentObject);
				if (freeDrams <= 0)
				{
					if (E.Actor.IsPlayer())
					{
						E.Actor.PlayWorldSound("sfx_missile_reloadFail_noAmmo");
						IComponent<GameObject>.AddPlayerMessage("You have no " + Liquid + " for " + ParentObject.t() + ".", 'r');
					}
					return true;
				}
				E.TriedToReload.Add(this);
				string text = ParentObject.t();
				if (liquidVolume.Volume > 0 && !liquidVolume.IsPureLiquid(Liquid))
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
				E.Actor.UseDrams(num, Liquid, ParentObject);
				liquidVolume.MixWith(new LiquidVolume(Liquid, num));
				E.Reloaded.Add(this);
				if (!E.ObjectsReloaded.Contains(ParentObject))
				{
					E.ObjectsReloaded.Add(ParentObject);
				}
				E.EnergyCost(ReloadEnergy);
				PlayWorldSound(ParentObject.GetTag("ReloadSound"));
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You " + ((liquidVolume.Volume < liquidVolume.MaxVolume) ? "partially " : "") + "fill " + ParentObject.t() + " with " + liquidVolume.GetLiquidName() + ".");
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
	{
		if (!string.IsNullOrEmpty(ProjectileObject))
		{
			E.Blueprint = ProjectileObject;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (ShowDamage && E.Understood())
		{
			E.AddMissileWeaponDamageTag(GetMissileWeaponPerformanceEvent.GetFor(ParentObject.Equipped, ParentObject));
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

	public override bool HandleEvent(GetPreferredLiquidEvent E)
	{
		if (E.Liquid == null)
		{
			E.Liquid = Liquid;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
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

	public bool IsFuelGood()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null)
		{
			return false;
		}
		if (!liquidVolume.IsPureLiquid(Liquid))
		{
			return false;
		}
		if (liquidVolume.Volume <= 0)
		{
			return false;
		}
		return true;
	}

	public string GetStatusMessage(ActivePartStatus Status)
	{
		return Status switch
		{
			ActivePartStatus.ProcessInputMissing => ParentObject.Does("are") + " empty.", 
			ActivePartStatus.ProcessInputInvalid => ParentObject.Does("are") + " not loaded with the correct liquid.", 
			_ => ParentObject.Does("are") + " " + GetStatusPhrase() + ".", 
		};
	}

	public string GetStatusMessage()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume.Volume <= 0)
		{
			return GetStatusMessage(ActivePartStatus.ProcessInputMissing);
		}
		if (!liquidVolume.IsPureLiquid(Liquid))
		{
			return GetStatusMessage(ActivePartStatus.ProcessInputInvalid);
		}
		return GetStatusMessage(GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L));
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		return Liquid == LiquidType;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PrepIntegratedHostToReceiveAmmo")
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
							gameObjectParameter.ForceEquipObject(GameObject.Create("StorageTank"), item, Silent: true, 0);
						}
					}
				}
			}
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (E.HasFlag("TrackSupply"))
			{
				E.SetFlag("AnySupplyHandler", State: true);
			}
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			if (gameObjectParameter2 != null && gameObject != null)
			{
				int freeDrams = gameObject.GetFreeDrams(Liquid, ParentObject);
				int storableDrams = gameObjectParameter2.GetStorableDrams(Liquid);
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
						int? num2 = Popup.AskNumber("Supply " + gameObjectParameter2.t() + " with how many drams of your " + Liquid + "? (max=" + num + ")", "Sounds/UI/ui_notification", "", num, 0, num);
						int num3 = 0;
						try
						{
							num3 = Convert.ToInt32(num2);
						}
						catch
						{
							goto IL_03e6;
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
						Popup.Show("You have no " + Liquid + " to supply " + gameObjectParameter2.t() + " with.");
					}
					else if (storableDrams <= 0)
					{
						Popup.Show(gameObjectParameter2.Does("have") + " no room for more " + Liquid + ".");
					}
				}
				if (num > 0)
				{
					IComponent<GameObject>.XDidYToZ(gameObject, "transfer", Grammar.Cardinal(num) + " " + ((num == 1) ? "dram" : "drams") + " of " + Liquid + " to", gameObjectParameter2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
					gameObject.UseDrams(num, Liquid);
					gameObjectParameter2.GiveDrams(num, Liquid);
					gameObject.UseEnergy(1000, "Ammo Liquid Transfer");
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo")
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter3 != null)
			{
				int storableDrams2 = gameObjectParameter3.GetStorableDrams(Liquid);
				if (storableDrams2 > 0)
				{
					gameObjectParameter3.GiveDrams(storableDrams2, Liquid);
				}
			}
		}
		goto IL_03e6;
		IL_03e6:
		return base.FireEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return !IsFuelGood();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		if (ParentObject.LiquidVolume.Volume > 0)
		{
			return "ProcessInputInvalid";
		}
		return "ProcessInputMissing";
	}
}
