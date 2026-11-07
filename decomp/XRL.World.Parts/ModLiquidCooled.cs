using System;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class ModLiquidCooled : IModification
{
	public const int LIQUID_CONSUMPTION_CHANCE_BASE = 6;

	public string PercentBonusRange = "30-60";

	public int PercentBonus;

	public int AmountBonus;

	public ModLiquidCooled()
	{
	}

	public ModLiquidCooled(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		IsTechScannable = true;
		NameForStatus = "CoolantSystem";
		ConsumesLiquid = "water";
		LiquidConsumptionChanceOneIn = 6;
	}

	public override void Attach()
	{
		if (ConsumesLiquid == null)
		{
			ConsumesLiquid = "water";
		}
		if (LiquidConsumptionChanceOneIn == 1)
		{
			LiquidConsumptionChanceOneIn = 6 + Tier;
		}
		base.Attach();
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		MissileWeapon part = Object.GetPart<MissileWeapon>();
		if (part == null)
		{
			return false;
		}
		if (part.ShotsPerAction <= 1)
		{
			return false;
		}
		if (part.ShotsPerAction != part.AmmoPerAction)
		{
			return false;
		}
		return true;
	}

	public override bool BeingAppliedBy(GameObject Object, GameObject Who)
	{
		if (Who.IsPlayer())
		{
			EnforceLiquidVolume(Object).Empty();
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		MissileWeapon part = Object.GetPart<MissileWeapon>();
		if (part != null)
		{
			if (AmountBonus == 0)
			{
				if (PercentBonus == 0)
				{
					PercentBonus = PercentBonusRange.RollCached();
				}
				AmountBonus = Math.Max(part.ShotsPerAction * PercentBonus / 100, 1);
			}
			if (part.ShotsPerAnimation == part.ShotsPerAction)
			{
				part.ShotsPerAnimation += AmountBonus;
			}
			part.ShotsPerAction += AmountBonus;
			part.AmmoPerAction += AmountBonus;
		}
		LiquidConsumptionChanceOneIn = 6 + Tier;
		EnforceLiquidVolume(Object);
		IncreaseDifficultyAndComplexity(2, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID && ID != PooledEvent<CheckLoadAmmoEvent>.ID && ID != PooledEvent<CheckReadyToFireEvent>.ID && ID != SingletonEvent<CommandReloadEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMissileWeaponStatusEvent>.ID && ID != PooledEvent<GetNotReadyToFireMessageEvent>.ID && ID != GetPreferredLiquidEvent.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<LoadAmmoEvent>.ID)
		{
			return ID == SingletonEvent<NeedsReloadEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus == ActivePartStatus.ProcessInputMissing || activePartStatus == ActivePartStatus.ProcessInputInvalid)
		{
			bool flag = false;
			Inventory inventory = E.Actor?.Inventory;
			if (inventory != null)
			{
				foreach (GameObject item in inventory.GetObjectsDirect())
				{
					LiquidVolume liquidVolume = item.LiquidVolume;
					if (liquidVolume != null && liquidVolume.Volume > 0 && (LiquidMustBePure ? liquidVolume.IsPureLiquid(ConsumesLiquid) : (liquidVolume.Amount(ConsumesLiquid) > 0)) && !liquidVolume.EffectivelySealed())
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

	public override bool HandleEvent(CheckLoadAmmoEvent E)
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			if (activePartStatus == ActivePartStatus.ProcessInputMissing || activePartStatus == ActivePartStatus.ProcessInputInvalid)
			{
				E.Message = GetStatusMessage(activePartStatus);
			}
			else if (E.Message == null)
			{
				E.Message = GetStatusMessage(activePartStatus);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LoadAmmoEvent E)
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			if (activePartStatus == ActivePartStatus.ProcessInputMissing || activePartStatus == ActivePartStatus.ProcessInputInvalid)
			{
				E.Message = GetStatusMessage(activePartStatus);
			}
			else if (E.Message == null)
			{
				E.Message = GetStatusMessage(activePartStatus);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckReadyToFireEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNotReadyToFireMessageEvent E)
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			E.Message = GetStatusMessage(activePartStatus);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponStatusEvent E)
	{
		if (E.Override == null)
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			string primaryLiquidColor = liquidVolume.GetPrimaryLiquidColor();
			if (liquidVolume.Volume == 0)
			{
				if (primaryLiquidColor != null)
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
			else if (!LiquidMustBePure || liquidVolume.IsPureLiquid(ConsumesLiquid))
			{
				if (primaryLiquidColor != null)
				{
					E.Items.Append(" [{{").Append(primaryLiquidColor).Append('|')
						.Append(liquidVolume.Volume)
						.Append("}}]");
				}
				else
				{
					E.Items.Append(" [").Append(liquidVolume.Volume).Append("]");
				}
			}
			else if (primaryLiquidColor != null)
			{
				E.Items.Append(" [{{").Append(primaryLiquidColor).Append("|?}}]");
			}
			else
			{
				E.Items.Append(" [?]");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedsReloadEvent E)
	{
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && (liquidVolume.Volume < liquidVolume.MaxVolume || (LiquidMustBePure && !liquidVolume.IsPureLiquid(ConsumesLiquid))) && ParentObject.IsEquippedProperly())
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
			if (!ParentObject.IsEquippedProperly() || E.MinimumCharge > 0)
			{
				return true;
			}
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume.Volume >= liquidVolume.MaxVolume && (!LiquidMustBePure || liquidVolume.IsPureLiquid(ConsumesLiquid)))
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " already full of " + liquidVolume.GetLiquidName() + ".");
				}
				return true;
			}
			E.NeededReload.Add(this);
			int freeDrams = E.Actor.GetFreeDrams(ConsumesLiquid, ParentObject, null, (GameObject o) => !o.HasPart<ModLiquidCooled>());
			if (freeDrams <= 0)
			{
				if (E.Actor.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You have no " + ConsumesLiquid + " for " + ParentObject.t() + ".", 'r');
				}
				return true;
			}
			E.TriedToReload.Add(this);
			string text = ParentObject.t();
			if (liquidVolume.Volume > 0 && LiquidMustBePure && !liquidVolume.IsPureLiquid(ConsumesLiquid))
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
			E.Actor.UseDrams(num, ConsumesLiquid, ParentObject, null, (GameObject o) => !o.HasPart<ModLiquidCooled>());
			liquidVolume.MixWith(new LiquidVolume(ConsumesLiquid, num));
			E.Reloaded.Add(this);
			if (!E.ObjectsReloaded.Contains(ParentObject))
			{
				E.ObjectsReloaded.Add(ParentObject);
			}
			E.EnergyCost(1000);
			if (E.Actor.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You " + ((liquidVolume.Volume < liquidVolume.MaxVolume) ? "partially " : "") + "fill " + text + " with " + liquidVolume.GetLiquidName() + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{" + (LiquidVolume.GetLiquid(ConsumesLiquid)?.GetColor() ?? "K") + "|liquid-cooled}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetInstanceDescription());
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
		else if (E.ID == "GenerateIntegratedHostInitialAmmo")
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
		goto IL_03e6;
		IL_03e6:
		return base.FireEvent(E);
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Liquid-cooled: This weapon's rate of fire is increased, but it requires pure water to function. When fired, there's a one in " + (6 + Tier) + " chance that 1 dram is consumed.";
	}

	public string GetInstanceDescription()
	{
		return "Liquid-cooled: This weapon's rate of fire is increased by " + AmountBonus + ", but " + ParentObject.it + " requires " + (LiquidMustBePure ? "pure " : "") + " " + ColorUtility.StripFormatting(LiquidVolume.GetLiquid(ConsumesLiquid).GetName()) + " to function. When fired, there's a one in " + LiquidConsumptionChanceOneIn + " chance that 1 dram is consumed.";
	}

	public LiquidVolume EnforceLiquidVolume(GameObject Object)
	{
		int liquidVolumeSize = GetLiquidVolumeSize(Object);
		if (liquidVolumeSize == 0)
		{
			return null;
		}
		LiquidVolume liquidVolume = Object.LiquidVolume;
		if (liquidVolume == null)
		{
			liquidVolume = new LiquidVolume();
			liquidVolume.MaxVolume = liquidVolumeSize;
			liquidVolume.Volume = liquidVolumeSize;
			liquidVolume.SetComponent(ConsumesLiquid, 1000);
			Object.AddPart(liquidVolume);
		}
		else if (liquidVolume.MaxVolume < liquidVolumeSize)
		{
			liquidVolume.MaxVolume = liquidVolumeSize;
		}
		return liquidVolume;
	}

	public LiquidVolume EnforceLiquidVolume()
	{
		return EnforceLiquidVolume(ParentObject);
	}

	public static int GetLiquidVolumeSizeForShotsPerAction(int Shots)
	{
		if (Shots <= 1)
		{
			return 0;
		}
		int num = Shots * 2;
		int num2 = num % 8;
		if (num2 != 0)
		{
			num += 8 - num2;
		}
		return num;
	}

	public static int GetLiquidVolumeSize(GameObject Object)
	{
		if (Object.TryGetPart<MissileWeapon>(out var Part))
		{
			return GetLiquidVolumeSizeForShotsPerAction(Part.ShotsPerAction);
		}
		return 0;
	}

	public int GetLiquidVolumeSize()
	{
		return GetLiquidVolumeSize(ParentObject);
	}

	public string GetStatusMessage(ActivePartStatus Status)
	{
		if (Status == ActivePartStatus.ProcessInputMissing || Status == ActivePartStatus.ProcessInputInvalid)
		{
			return ParentObject.Does("emit") + " a grinding noise.";
		}
		return ParentObject.Does("are") + " " + GetStatusPhrase(Status) + ".";
	}

	public string GetStatusMessage()
	{
		return GetStatusMessage(GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L));
	}
}
