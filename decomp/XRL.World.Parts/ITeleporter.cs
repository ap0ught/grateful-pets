using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public abstract class ITeleporter : IPoweredPart
{
	public const int FAIL_NONE = 0;

	public const int FAIL_WORLD = 1;

	public const int FAIL_PLANE = 2;

	public const int FAIL_PROTOCOL = 4;

	public string DestinationZone = "";

	public string Sound;

	public int DestinationX = 40;

	public int DestinationY = 13;

	public bool UsableInCombat;

	public bool Intraplanar;

	public bool Interplanar;

	public bool Interprotocol;

	public long LastTurnUsed;

	public ITeleporter()
	{
		ChargeUse = 0;
		WorksOnCarrier = true;
		WorksOnHolder = true;
	}

	public override bool SameAs(IPart p)
	{
		ITeleporter teleporter = p as ITeleporter;
		if (teleporter.DestinationZone != DestinationZone)
		{
			return false;
		}
		if (teleporter.DestinationX != DestinationX)
		{
			return false;
		}
		if (teleporter.DestinationY != DestinationY)
		{
			return false;
		}
		if (teleporter.UsableInCombat != UsableInCombat)
		{
			return false;
		}
		if (teleporter.Intraplanar != Intraplanar)
		{
			return false;
		}
		if (teleporter.Interplanar != Interplanar)
		{
			return false;
		}
		if (teleporter.Interprotocol != Interprotocol)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AddedToInventoryEvent.ID || !WorksOnCarrier) && (ID != EquippedEvent.ID || (!WorksOnEquipper && !WorksOnHolder && !WorksOnWearer)) && ID != PooledEvent<ExamineSuccessEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<GetRecoilersEvent>.ID)
		{
			if (ID == ImplantedEvent.ID)
			{
				return WorksOnImplantee;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetRecoilersEvent E)
	{
		if (VisibleInRecoilerList())
		{
			E.Objects.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineSuccessEvent E)
	{
		if (E.Object == ParentObject)
		{
			RequireRecoilAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		if (E.Item == ParentObject)
		{
			RequireRecoilAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (E.Item == ParentObject)
		{
			RequireRecoilAbility(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (E.Item == ParentObject)
		{
			RequireRecoilAbility(E.Implantee);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 5);
			if (Interprotocol)
			{
				E.Add("circuitry", 5);
			}
		}
		return base.HandleEvent(E);
	}

	public int GetTeleportFailure()
	{
		if (The.ZoneManager == null || !The.ZoneManager.IsZoneLive(DestinationZone))
		{
			return 0;
		}
		WorldBlueprint worldBlueprint = GetAnyBasisZone()?.ResolveWorldBlueprint();
		WorldBlueprint worldBlueprint2 = The.ZoneManager.GetZone(DestinationZone)?.ResolveWorldBlueprint();
		if (!Intraplanar && worldBlueprint != worldBlueprint2)
		{
			return 1;
		}
		if (!Interplanar && worldBlueprint?.Plane != worldBlueprint2?.Plane)
		{
			return 2;
		}
		if (!Interprotocol && worldBlueprint?.Protocol != worldBlueprint2?.Protocol)
		{
			return 4;
		}
		return 0;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (GetTeleportFailure() != 0)
		{
			return true;
		}
		return base.GetActivePartLocallyDefinedFailure();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return GetTeleportFailure() switch
		{
			1 => "RelativityCompensationFailure", 
			2 => "QuantumPhaseMismatch", 
			4 => "ProtocolMismatch", 
			_ => base.GetActivePartLocallyDefinedFailureDescription(), 
		};
	}

	public virtual string GetCustomTeleportFailure(GameObject Actor)
	{
		return null;
	}

	public virtual bool VisibleInRecoilerList()
	{
		if (ParentObject.Understood())
		{
			return !ParentObject.HasPropertyOrTag("ExcludeFromRecoilerList");
		}
		return false;
	}

	public bool AttemptTeleport(GameObject Actor, IEvent FromEvent = null)
	{
		if (DestinationZone.IsNullOrEmpty())
		{
			return Actor.Fail("Nothing happens.");
		}
		if (!UsableInCombat && Actor.AreHostilesNearby())
		{
			if (Actor == ParentObject)
			{
				return Actor.Fail("You can't recoil with hostiles nearby!");
			}
			return Actor.Fail("You can't use " + ParentObject.t() + " with hostiles nearby!");
		}
		int num = ParentObject.QueryCharge(LiveOnly: false, 0L);
		The.ZoneManager.GetZone(DestinationZone);
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: true, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		string customTeleportFailure = GetCustomTeleportFailure(Actor);
		if (!customTeleportFailure.IsNullOrEmpty() || activePartStatus != ActivePartStatus.Operational || !IsObjectActivePartSubject(Actor))
		{
			if (Actor.IsPlayer())
			{
				switch (activePartStatus)
				{
				case ActivePartStatus.LocallyDefinedFailure:
					switch (GetTeleportFailure())
					{
					case 4:
					{
						string text2 = GetAnyBasisZone()?.ResolveWorldBlueprint()?.Protocol;
						string text3 = The.ZoneManager.GetZone(DestinationZone)?.ResolveWorldBlueprint()?.Protocol;
						if (text2 == "THIN")
						{
							Popup.ShowFail("You have no bodily tether to recoil.");
						}
						else if (text3 == "THIN")
						{
							if (text2.IsNullOrEmpty())
							{
								Popup.ShowFail(ParentObject.Does("are") + " encoded with an imprint of the Thin World that has no meaning in the Thick World.");
							}
							else
							{
								Popup.ShowFail(ParentObject.Does("are") + " encoded with an imprint of the Thin World that has no meaning in your present context.");
							}
						}
						else
						{
							Popup.ShowFail(ParentObject.Does("are") + " encoded with an imprint that has no meaning in your present context.");
						}
						break;
					}
					case 2:
					{
						string value = GetAnyBasisZone()?.ResolveWorldBlueprint()?.Plane;
						string text = The.ZoneManager.GetZone(DestinationZone)?.ResolveWorldBlueprint()?.Plane;
						if (!value.IsNullOrEmpty())
						{
							Popup.ShowFail("You are stuck in a remote pocket dimension and cannot recoil out.");
						}
						else
						{
							Popup.ShowFail(ParentObject.Does("are") + " encoded with the imprint of a remote pocket dimension, " + text + ", that is inaccessible from your present vibrational plane.");
						}
						break;
					}
					default:
						Popup.ShowFail("You cannot do that here.");
						break;
					}
					break;
				case ActivePartStatus.Rusted:
					Popup.ShowFail(ParentObject.Poss("activation button") + " button is rusted in place.");
					break;
				case ActivePartStatus.Broken:
					Popup.ShowFail(ParentObject.Itis + " broken...");
					break;
				case ActivePartStatus.Booting:
					Popup.ShowFail(ParentObject.Does("are") + " still starting up.");
					break;
				case ActivePartStatus.Unpowered:
					if (num > 0 && ParentObject.QueryCharge(LiveOnly: false, 0L) < num)
					{
						Popup.ShowFail(ParentObject.Does("hum") + " for a moment, then powers down. " + ParentObject.Does("don't", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " have enough charge to function.");
					}
					else
					{
						Popup.ShowFail(ParentObject.Does("don't") + " have enough charge to function.");
					}
					break;
				default:
					if (!customTeleportFailure.IsNullOrEmpty())
					{
						Popup.ShowFail(customTeleportFailure);
					}
					else
					{
						Popup.ShowFail("Nothing happens.");
					}
					break;
				}
			}
			return false;
		}
		Actor.PlayWorldOrUISound(Sound);
		if (Actor.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You activate the recoiler.");
		}
		bool flag = Actor.ZoneTeleport(DestinationZone, DestinationX, DestinationY, FromEvent, ParentObject, Actor);
		if (flag)
		{
			LastTurnUsed = The.CurrentTurn;
		}
		return flag;
	}

	public bool RequireRecoilAbility(GameObject Subject)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return false;
		}
		if (Subject.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (!IsObjectActivePartSubject(Subject))
		{
			return false;
		}
		if (!VisibleInRecoilerList())
		{
			return false;
		}
		Subject.RequirePart<RecoilAbility>();
		return true;
	}
}
