using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class ModFactionSlayer : IModification
{
	public string Faction;

	public ModFactionSlayer()
	{
	}

	public ModFactionSlayer(int Tier)
		: base(Tier)
	{
	}

	public ModFactionSlayer(int Tier, string Faction)
		: this(Tier)
	{
		if (!Factions.Exists(Faction))
		{
			string newFaction = CompatManager.GetNewFaction(Faction);
			if (newFaction != null)
			{
				MetricsManager.LogWarning("faction name " + Faction + " should be " + newFaction + ", support will be removed after Q2 2024");
				Faction = newFaction;
			}
		}
		this.Faction = Faction;
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		if (Factions.Exists(Faction))
		{
			return;
		}
		string newFaction = CompatManager.GetNewFaction(Faction);
		if (newFaction != null)
		{
			if (Reader.FileVersion >= 350)
			{
				MetricsManager.LogWarning("faction name " + Faction + " should be " + newFaction + ", support will be removed after Q2 2024");
			}
			Faction = newFaction;
		}
		else
		{
			Faction byDisplayName = Factions.GetByDisplayName(Faction);
			if (byDisplayName != null)
			{
				Faction = byDisplayName.Name;
			}
		}
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "ProfilingEngine";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModFactionSlayer).Faction != Faction)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int num = GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModFactionSlayer Decapitate", Tier);
		E.Postfix.AppendRules(num + "% chance to behead " + XRL.World.Faction.GetFormattedName(Faction) + " on hit.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter2.IsFactionMember(Faction))
			{
				GameObject parentObject = ParentObject;
				GameObject subject = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModFactionSlayer Decapitate", Tier, subject).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					Axe_Decapitate.Decapitate(gameObjectParameter, gameObjectParameter2);
				}
			}
		}
		return base.FireEvent(E);
	}
}
