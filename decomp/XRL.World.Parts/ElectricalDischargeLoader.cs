using System;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the adjustment to charge used that is
/// implemented by IActivePart (adjusting by power load as a percentage)
/// applies to the charge-based damage/voltage calculations here,
/// multiplied by OverloadFactor; so, for example, if base charge draw
/// is 100, overloaded draw is 400, and OverloadFactor is the default
/// of 1/6, effective draw for purposes of damage and voltage calculation
/// is 100 + ((400 - 100) * (1 / 6)) = 150.
/// </remarks>
[Serializable]
public class ElectricalDischargeLoader : IPoweredPart
{
	public string ProjectileObject;

	public float ChargeFactor = 15f;

	public float OverloadFactor = 1f / 6f;

	[NonSerialized]
	private static List<GameObjectBlueprint> EnergyCellBlueprints;

	[NonSerialized]
	private static Dictionary<string, int> BlueprintMaxCharge = new Dictionary<string, int>(16);

	public ElectricalDischargeLoader()
	{
		ChargeUse = 300;
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "ElectricalDischargeSystem";
	}

	public override bool SameAs(IPart p)
	{
		ElectricalDischargeLoader electricalDischargeLoader = p as ElectricalDischargeLoader;
		if (electricalDischargeLoader.ProjectileObject != ProjectileObject)
		{
			return false;
		}
		if (electricalDischargeLoader.ChargeFactor != ChargeFactor)
		{
			return false;
		}
		if (electricalDischargeLoader.OverloadFactor != OverloadFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIWantUseWeaponEvent>.ID && ID != PooledEvent<CheckLoadAmmoEvent>.ID && ID != PooledEvent<CheckReadyToFireEvent>.ID && ID != SingletonEvent<CommandReloadEvent>.ID && ID != PooledEvent<GetAmmoCountAvailableEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMissileWeaponProjectileEvent>.ID && ID != PooledEvent<GetMissileWeaponStatusEvent>.ID && ID != PooledEvent<GetNotReadyToFireMessageEvent>.ID && ID != PooledEvent<GetProjectileBlueprintEvent>.ID && ID != PooledEvent<LoadAmmoEvent>.ID)
		{
			return ID == SingletonEvent<NeedsReloadEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIWantUseWeaponEvent E)
	{
		if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered && E.Object.TryGetPart<EnergyCellSocket>(out var socket))
		{
			bool result = false;
			E.Actor.Inventory.ForeachObject((GameObject obj) =>
			{
				if (!E.Actor.IsPlayer() || obj.Understood())
				{
					obj.ForeachPartDescendedFrom((IEnergyCell part) =>
					{
						if (part.SlotType == socket.SlotType && part.HasCharge(ChargeUse))
						{
							result = true;
							return false;
						}
						return true;
					});
				}
			});
			if (!result)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAmmoCountAvailableEvent E)
	{
		int activeChargeUse = GetActiveChargeUse();
		if (activeChargeUse > 0)
		{
			E.Register(ParentObject.QueryCharge(LiveOnly: false, 0L) / activeChargeUse);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckLoadAmmoEvent E)
	{
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			if (E.Message == null)
			{
				E.Message = GetStatusMessage(activePartStatus);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LoadAmmoEvent E)
	{
		int value = MyPowerLoadLevel();
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, PowerLoadLevel: value, IgnoreSubject: E.ActivePartsIgnoreSubject, IgnoreLocallyDefinedFailure: false, MultipleCharge: 1, ChargeUse: null, UseChargeIfUnpowered: false, GridMask: 0L);
		if (activePartStatus == ActivePartStatus.Operational)
		{
			if (!ProjectileObject.IsNullOrEmpty())
			{
				int? powerLoadLevel = value;
				int activeChargeUse = GetActiveChargeUse(null, powerLoadLevel);
				GameObject gameObject = GameObject.Create(ProjectileObject, 0, 0, null, null, null, "Projectile");
				DischargeOnHit dischargeOnHit = gameObject.RequirePart<DischargeOnHit>();
				dischargeOnHit.Voltage = GetVoltage(activeChargeUse, value).ToString();
				dischargeOnHit.DamageRange = GetDamageRoll(activeChargeUse, value);
				E.Projectile = gameObject;
			}
			return base.HandleEvent(E);
		}
		if (E.Message == null)
		{
			E.Message = GetStatusMessage(activePartStatus);
		}
		return false;
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
			if (E.Status != null)
			{
				E.Status.ammoTotal = 1;
				E.Status.ammoRemaining = (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) ? 1 : 0);
			}
			EnergyCellSocket part = ParentObject.GetPart<EnergyCellSocket>();
			if (part != null)
			{
				if (part.Cell == null)
				{
					E.Items.Append(" [{{K|empty}}]");
				}
				else if (!part.Cell.Understood())
				{
					E.Items.Append(" [?]");
				}
				else
				{
					part.Cell.ForeachPartDescendedFrom((IEnergyCell energyCell) =>
					{
						string text = energyCell.ChargeStatus();
						if (text != null)
						{
							E.Items.Append(" [").Append(text).Append("]");
						}
					});
				}
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
		if (E.Skip != this && (E.Weapon == null || E.Weapon == ParentObject) && GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered && ParentObject.IsEquippedProperly())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandReloadEvent E)
	{
		if (E.Pass >= 3 && (E.Weapon == null || E.Weapon == ParentObject) && !E.CheckedForReload.Contains(this))
		{
			E.CheckedForReload.Add(this);
			MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
			if ((part == null || part.FiresManually) && (E.Weapon == ParentObject || ParentObject.IsEquippedProperly()))
			{
				bool flag = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Unpowered;
				if (flag || (E.NeededReload.Count <= 0 && E.Reloaded.Count <= 0 && !NeedsReloadEvent.Check(E.Actor, this)))
				{
					if (flag)
					{
						E.NeededReload.Add(this);
					}
					if (ParentObject.WantEvent(InventoryActionEvent.ID, InventoryActionEvent.CascadeLevel))
					{
						E.TriedToReload.Add(this);
						if (InventoryActionEvent.Check(ParentObject, E.Actor, ParentObject, EnergyCellSocket.REPLACE_CELL_INTERACTION, Auto: false, OwnershipHandled: false, OverrideEnergyCost: true, Forced: false, Silent: false, 0, E.MinimumCharge))
						{
							E.Reloaded.Add(this);
							if (!E.ObjectsReloaded.Contains(ParentObject))
							{
								E.ObjectsReloaded.Add(ParentObject);
							}
							E.EnergyCost(1000);
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

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!ProjectileObject.IsNullOrEmpty() && E.Understood())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("{{W|").Append('\u0003').Append("}}")
				.Append(GetDamageRoll());
			E.AddTag(stringBuilder.ToString(), -20);
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

	public string GetStatusMessage(ActivePartStatus Status)
	{
		if (Status == ActivePartStatus.Unpowered)
		{
			return ParentObject.Does("click", int.MaxValue, null, null, "merely") + ".";
		}
		return ParentObject.Does("are") + " " + GetStatusPhrase(Status) + ".";
	}

	public string GetStatusMessage()
	{
		return GetStatusMessage(GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L));
	}

	public override int GetDraw(QueryDrawEvent E = null)
	{
		int num = base.GetDraw(E);
		if (ParentObject.TryGetPart<MissileWeapon>(out var Part))
		{
			num *= Part.AmmoPerAction;
		}
		return num;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PrepIntegratedHostToReceiveAmmo")
		{
			if (ChargeUse > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Host");
				if (gameObjectParameter != null && gameObjectParameter.HasPart<Robot>())
				{
					if (!gameObjectParameter.HasPart<ElectricalPowerTransmission>())
					{
						ElectricalPowerTransmission electricalPowerTransmission = new ElectricalPowerTransmission();
						electricalPowerTransmission.ChargeRate = ((GetChargePerAction() >= 1000) ? 10000 : 1000);
						electricalPowerTransmission.IsConsumer = true;
						gameObjectParameter.AddPart(electricalPowerTransmission);
					}
					if (!gameObjectParameter.HasPart<Capacitor>() && !gameObjectParameter.HasTagOrProperty("NoIntegratedHostCapacitor"))
					{
						Capacitor capacitor = new Capacitor();
						capacitor.MaxCharge = ChargeUse * 10;
						capacitor.ChargeRate = ChargeUse;
						capacitor.MinimumChargeToExplode = 0;
						capacitor.ChargeDisplayStyle = null;
						gameObjectParameter.AddPart(capacitor);
					}
				}
				if (!ParentObject.HasPart<IntegratedPowerSystems>() && ItemModding.ModificationApplicable("ModJacked", ParentObject))
				{
					ItemModding.ApplyModification(ParentObject, "ModJacked");
				}
			}
		}
		else if (E.ID == "SupplyIntegratedHostWithAmmo")
		{
			if (ChargeUse > 0)
			{
				if (E.HasFlag("TrackSupply"))
				{
					E.SetFlag("AnySupplyHandler", State: true);
				}
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Host");
				GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
				if (gameObjectParameter2 != null && gameObject != null)
				{
					Inventory inventory = gameObject.Inventory;
					if (inventory != null)
					{
						if (gameObject.IsPlayer())
						{
							int num = 0;
							List<GameObject> skip = Event.NewGameObjectList();
							while (true)
							{
								string SlotType = null;
								EnergyCellSocket part = ParentObject.GetPart<EnergyCellSocket>();
								if (part != null)
								{
									SlotType = part.SlotType;
								}
								List<string> OptionStrings = new List<string>(16);
								List<object> options = new List<object>(16);
								List<char> keymap = new List<char>(16);
								OptionStrings.Add("none");
								options.Add(null);
								keymap.Add('-');
								char c = 'a';
								inventory.ForeachObject((GameObject GO) =>
								{
									if (!skip.Contains(GO) && GO.Understood())
									{
										GO.ForeachPartDescendedFrom((IEnergyCell energyCell) =>
										{
											if (SlotType == null || energyCell.SlotType == SlotType)
											{
												OptionStrings.Add(GO.DisplayName);
												options.Add(GO);
												keymap.Add(c);
												char c2 = c;
												c = (char)(c2 + 1);
												return false;
											}
											return true;
										});
									}
								});
								if (options.Count <= 1)
								{
									break;
								}
								if (E.HasFlag("TrackSupply"))
								{
									E.SetFlag("AnySupplies", State: true);
								}
								int num2 = Popup.PickOption("Select " + ((num == 0) ? "one" : "another") + " of your cells to supply " + gameObjectParameter2.the + gameObjectParameter2.ShortDisplayName + " with, if desired.", null, "", "Sounds/UI/ui_notification", OptionStrings.ToArray(), keymap.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
								if (num2 < 0 || !(options[num2] is GameObject gameObject2))
								{
									break;
								}
								GameObject gameObject3 = gameObject2.RemoveOne();
								if (gameObjectParameter2.ReceiveObject(gameObject3))
								{
									IComponent<GameObject>.WDidXToYWithZ(gameObject, "transfer", gameObject3, "to", gameObjectParameter2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: true, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: true, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
									num++;
									continue;
								}
								Popup.Show(gameObjectParameter2.The + gameObjectParameter2.ShortDisplayName + " cannot take " + gameObject3.the + gameObject3.ShortDisplayName + ".");
								skip.Add(gameObject3);
							}
							if (num > 0)
							{
								gameObject.UseEnergy(1000, "Ammo Magazine Transfer");
							}
						}
						else
						{
							int desiredCharge = GetDesiredCharge();
							int num3 = GetAccessibleCharge();
							if (num3 < desiredCharge)
							{
								List<GameObject> list = Event.NewGameObjectList();
								inventory.GetObjects(list, FindEnergyCells);
								int num4 = 0;
								while (num3 < desiredCharge && list.Count > 0 && ++num4 < 100)
								{
									GameObject gameObject4 = null;
									int num5 = 0;
									foreach (GameObject item in list)
									{
										int num6 = item.QueryCharge(LiveOnly: false, 0L);
										if (num6 < ChargeUse)
										{
											continue;
										}
										int num7 = num6 - (desiredCharge - num3);
										if (gameObject4 == null)
										{
											gameObject4 = item;
											num5 = num7;
										}
										else if (num7 >= 0)
										{
											if (num7 < num5)
											{
												gameObject4 = item;
												num5 = num7;
											}
										}
										else if (num7 > num5)
										{
											gameObject4 = item;
											num5 = num7;
										}
									}
									if (gameObject4 != null)
									{
										gameObject4 = gameObject4.RemoveOne();
										IComponent<GameObject>.WDidXToYWithZ(gameObject, "transfer", gameObject4, "to", gameObjectParameter2, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: true, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: true, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
										if (gameObjectParameter2.ReceiveObject(gameObject4))
										{
											num3 += gameObject4.QueryCharge(LiveOnly: false, 0L);
										}
										else
										{
											gameObject4.CheckStack();
										}
										gameObject.UseEnergy(1000, "Ammo Magazine Transfer");
									}
								}
							}
						}
					}
				}
			}
		}
		else if (E.ID == "GenerateIntegratedHostInitialAmmo" && ChargeUse > 0)
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Host");
			if (gameObjectParameter3 != null && gameObjectParameter3.Inventory != null)
			{
				int desiredCharge2 = GetDesiredCharge();
				int num8 = GetAccessibleCharge();
				int num9 = 0;
				while (num8 < desiredCharge2 && ++num9 < 100)
				{
					string text = null;
					int num10 = 0;
					foreach (GameObjectBlueprint energyCellBlueprint in GetEnergyCellBlueprints())
					{
						int num11 = 0;
						if (!BlueprintMaxCharge.ContainsKey(energyCellBlueprint.Name))
						{
							try
							{
								num11 = energyCellBlueprint.GetPartParameter("EnergyCell", "MaxCharge", 0);
								BlueprintMaxCharge.Add(energyCellBlueprint.Name, num11);
							}
							catch
							{
								continue;
							}
						}
						else
						{
							num11 = BlueprintMaxCharge[energyCellBlueprint.Name];
						}
						if (num11 < ChargeUse)
						{
							continue;
						}
						int num12 = num11 - (desiredCharge2 - num8);
						if (text == null)
						{
							text = energyCellBlueprint.Name;
							num10 = num12;
						}
						else if (num12 >= 0)
						{
							if (num12 < num10)
							{
								text = energyCellBlueprint.Name;
								num10 = num12;
							}
						}
						else if (num12 > num10)
						{
							text = energyCellBlueprint.Name;
							num10 = num12;
						}
					}
					if (text == null)
					{
						continue;
					}
					GameObject gameObject5 = GameObject.Create(text);
					EnergyCell part2 = gameObject5.GetPart<EnergyCell>();
					if (part2 != null)
					{
						part2.Charge = part2.MaxCharge;
						if (gameObjectParameter3.ReceiveObject(gameObject5))
						{
							num8 += part2.Charge;
						}
						else
						{
							gameObject5.Obliterate();
						}
					}
					else
					{
						gameObject5.Obliterate();
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	private bool FindEnergyCells(GameObject obj)
	{
		return obj.HasPartDescendedFrom<IEnergyCell>();
	}

	private List<GameObjectBlueprint> GetEnergyCellBlueprints()
	{
		if (EnergyCellBlueprints == null)
		{
			EnergyCellBlueprints = new List<GameObjectBlueprint>();
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if (blueprint.HasPart("EnergyCell") && !blueprint.HasTag("ExcludeFromDynamicEncounters") && !blueprint.HasTag("BaseObject") && blueprint.GetPartParameter("EnergyCell", "MaxCharge", -1) != -1)
				{
					EnergyCellBlueprints.Add(blueprint);
				}
			}
		}
		return EnergyCellBlueprints;
	}

	private int GetChargePerAction()
	{
		MissileWeapon part = ParentObject.GetPart<MissileWeapon>();
		return ChargeUse * part.AmmoPerAction;
	}

	private int GetDesiredCharge()
	{
		return GetChargePerAction() * ParentObject.GetIntProperty("IntegratedWeaponHostShots", 100);
	}

	private int GetAccessibleCharge()
	{
		int num = ParentObject.QueryCharge(LiveOnly: false, 0L);
		Inventory inventory = ParentObject.Holder?.Inventory;
		if (inventory != null)
		{
			foreach (GameObject @object in inventory.Objects)
			{
				foreach (IEnergyCell item in @object.GetPartsDescendedFrom<IEnergyCell>())
				{
					num += item.GetCharge();
				}
			}
		}
		return num;
	}

	public string GetDamageRoll(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		string text = ElectricalGeneration.GetDischargeDamageRoll(GetEffectiveCharge(ChargeUse, PowerLoadLevel));
		if (text == null)
		{
			return null;
		}
		if (ParentObject.TryGetPart<MissilePerformance>(out var Part) && Part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (Part.DamageDieModifier != 0)
			{
				text = DieRoll.AdjustDieSize(text, Part.DamageDieModifier);
			}
			if (Part.DamageModifier != 0)
			{
				text = DieRoll.AdjustResult(text, Part.DamageModifier);
			}
		}
		if (ParentObject.HasPart<ModGigantic>())
		{
			text = DieRoll.AdjustResult(text, 3);
		}
		return text;
	}

	public int GetVoltage(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		return ElectricalGeneration.GetDischargeVoltage(GetEffectiveCharge(ChargeUse, PowerLoadLevel));
	}

	public int GetEffectiveCharge(int? ChargeUse = null, int? PowerLoadLevel = null)
	{
		int num = PowerLoadLevel ?? MyPowerLoadLevel();
		float num2 = ChargeUse ?? GetActiveChargeUse(ChargeUse, PowerLoadLevel);
		if (num != 100)
		{
			int? powerLoadLevel = 100;
			int activeChargeUse = GetActiveChargeUse(null, powerLoadLevel);
			num2 = (float)activeChargeUse + (num2 - (float)activeChargeUse) * OverloadFactor;
		}
		return (int)Math.Round(num2 * ChargeFactor);
	}
}
