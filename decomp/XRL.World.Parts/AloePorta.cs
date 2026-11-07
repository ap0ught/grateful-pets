using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AloePorta : IActivePart
{
	public string GroupKey = "AloePorta";

	public int CooldownLeft;

	public string Cooldown = "1";

	public int Chance = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs;

	public bool TriggerOnSaveSuccess = true;

	public bool DoCooldownRecolor;

	public string CooldownColorString = "&w";

	public new string ReadyColorString = "&W";

	public string CooldownTileColor = "&w";

	public string ReadyTileColor = "&W";

	public string CooldownDetailColor = "b";

	public new string ReadyDetailColor = "B";

	public AloePorta()
	{
		WorksOnCellContents = true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("ObjectEnteredCell");
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<InterruptAutowalkEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (!ParentObject.IsHidden)
		{
			E.IndicateObject = ParentObject;
		}
		return false;
	}

	public override bool SameAs(IPart p)
	{
		DischargeOnStep dischargeOnStep = p as DischargeOnStep;
		if (dischargeOnStep.Chance != Chance)
		{
			return false;
		}
		if (dischargeOnStep.SaveStat != SaveStat)
		{
			return false;
		}
		if (dischargeOnStep.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (dischargeOnStep.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (dischargeOnStep.SaveVs != SaveVs)
		{
			return false;
		}
		if (dischargeOnStep.CooldownLeft != CooldownLeft)
		{
			return false;
		}
		if (dischargeOnStep.TriggerOnSaveSuccess != TriggerOnSaveSuccess)
		{
			return false;
		}
		if (dischargeOnStep.Cooldown != Cooldown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void SyncColor()
	{
		if (!DoCooldownRecolor)
		{
			return;
		}
		if (CooldownLeft > 0)
		{
			if (!string.IsNullOrEmpty(CooldownColorString))
			{
				ParentObject.Render.ColorString = CooldownColorString;
			}
			if (!string.IsNullOrEmpty(CooldownTileColor))
			{
				ParentObject.Render.TileColor = CooldownTileColor;
			}
			if (!string.IsNullOrEmpty(CooldownDetailColor))
			{
				ParentObject.Render.DetailColor = CooldownDetailColor;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(ReadyColorString))
			{
				ParentObject.Render.ColorString = ReadyColorString;
			}
			if (!string.IsNullOrEmpty(ReadyTileColor))
			{
				ParentObject.Render.TileColor = ReadyTileColor;
			}
			if (!string.IsNullOrEmpty(ReadyDetailColor))
			{
				ParentObject.Render.DetailColor = ReadyDetailColor;
			}
		}
	}

	public bool ValidStepTarget(GameObject obj)
	{
		if (obj.IsCombat)
		{
			return obj.FlightMatches(ParentObject);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && E.ID == "EndTurn" && CooldownLeft > 0)
		{
			CooldownLeft--;
			if (CooldownLeft <= 0)
			{
				SyncColor();
			}
		}
		if (E.ID == "ObjectEnteredCell")
		{
			if (CooldownLeft <= 0 && GetActivePartFirstSubject(ValidStepTarget) != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (Chance >= 100 || Stat.Random(1, 100) < Chance))
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects(ValidStepTarget))
				{
					if (activePartSubject.HasStringProperty("aloevoltatarget") || (!string.IsNullOrEmpty(SaveStat) && activePartSubject.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat) != TriggerOnSaveSuccess))
					{
						continue;
					}
					List<GameObject> objects = ParentObject.Physics.CurrentCell.ParentZone.GetObjects((GameObject o) =>
					{
						if (o != ParentObject && o.HasPart<AloePorta>() && o.GetPart<AloePorta>().GroupKey == GroupKey && o.GetPart<AloePorta>().CooldownLeft <= 0)
						{
							if (o.Physics.CurrentCell.IsSolid())
							{
								return false;
							}
							if (o.Physics.CurrentCell.IsPassable(o))
							{
								return true;
							}
						}
						return false;
					});
					if (objects.Count > 0)
					{
						GameObject randomElement = objects.GetRandomElement();
						CooldownLeft = Stat.Roll(Cooldown);
						randomElement.GetPart<AloePorta>().CooldownLeft = Stat.Roll(randomElement.GetPart<AloePorta>().Cooldown);
						activePartSubject.TeleportSwirl(null, "&C", Voluntary: false, null, 'ù', IsOut: true);
						activePartSubject.TeleportTo(randomElement.Physics.CurrentCell, 0);
						activePartSubject.TeleportSwirl();
					}
				}
			}
		}
		else if (E.ID == "ObjectCreated")
		{
			SyncColor();
		}
		return base.FireEvent(E);
	}
}
