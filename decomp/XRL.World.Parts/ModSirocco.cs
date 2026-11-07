using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModSirocco : IModification
{
	public ModSirocco()
	{
	}

	public ModSirocco(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "Sirocco";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<MeleeWeapon>();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("salt", 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Sirocco: Drains 1 Toughness from any organic target this weapon damages for 3-4 turns.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter.IsOrganic && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
				gameObjectParameter.ApplyEffect(new ToughnessDrained(Stat.Random(3, 4)));
				gameObjectParameter2.ApplyEffect(new ToughnessBoosted(Stat.Random(3, 4)));
				gameObjectParameter.Bloodsplatter();
			}
		}
		return base.FireEvent(E);
	}
}
