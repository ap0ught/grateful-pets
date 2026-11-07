using System;
using XRL.Language;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModMicroserrated : IModification
{
	public int Chance = 7;

	public ModMicroserrated()
	{
	}

	public ModMicroserrated(int Tier)
		: base(Tier)
	{
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModMicroserrated).Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<ModSerrated>();
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{Y|mi{{R|c}}roserra{{R|t}}ed}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("might", 1);
		}
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
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModMicroserrated Dismember", Chance, subject).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				bool flag = 3.in1000();
				Axe_Dismember.Dismember(gameObjectParameter, gameObjectParameter2, null, null, ParentObject, null, "sfx_characterTrigger_dismember", flag, !flag);
			}
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Microserrated: This weapon has a chance to dismember opponents.";
	}

	public string GetInstanceDescription()
	{
		int number = GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModMicroserrated Dismember", Chance);
		return "Microserrated: This weapon has " + Grammar.A(number) + "% chance to dismember opponents.";
	}
}
