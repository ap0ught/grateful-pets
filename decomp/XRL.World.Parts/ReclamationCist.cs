using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ReclamationCist : IPoweredPart
{
	public string ProduceBlueprint = "Food Cube";

	public string RequireGenotype = "True Kin";

	public ReclamationCist()
	{
		ChargeUse = 500;
		WorksOnInventory = true;
		NameForStatus = "ReclamationSystems";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	private bool PerformReclamationOf(GameObject obj)
	{
		GameObjectBlueprint blueprint = obj.GetBlueprint();
		if (!blueprint.DescendsFrom("Corpse"))
		{
			return true;
		}
		if (!RequireGenotype.IsNullOrEmpty())
		{
			string text = obj.GetPropertyOrTag("FromGenotype");
			if (obj.TryGetPart<DismemberedProperties>(out var Part) && !Part.SourceGenotype.IsNullOrEmpty() && Part.BodyPart.Native)
			{
				text = Part.SourceGenotype;
			}
			if (text != RequireGenotype)
			{
				return true;
			}
		}
		bool flag = blueprint.DescendsFrom("BaseLimb");
		CyberneticsButcherableCybernetic part = obj.GetPart<CyberneticsButcherableCybernetic>();
		if (part != null && part.AttemptButcher(ParentObject, Automatic: false, SkipSkill: true, IntoInventory: true, 10))
		{
			if (!ProduceBlueprint.IsNullOrEmpty())
			{
				ParentObject.ReceiveObject(ProduceBlueprint, flag ? Stat.Random(1, 3) : Stat.Random(5, 10));
			}
			return false;
		}
		obj = obj.RemoveOne();
		if (Visible())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("reclaim") + " " + obj.an() + ".");
		}
		obj.Destroy();
		if (!ProduceBlueprint.IsNullOrEmpty())
		{
			ParentObject.ReceiveObject(ProduceBlueprint, flag ? Stat.Random(2, 4) : Stat.Random(6, 12));
		}
		return false;
	}

	public bool PerformReclamation()
	{
		bool flag = ForeachActivePartSubjectWhile(PerformReclamationOf, MayMoveAddOrDestroy: true);
		if (!flag)
		{
			for (int num = Stat.Random(1, 5); num >= 0; num--)
			{
				ParentObject.Bloodsplatter();
			}
			if (ChargeUse > 0)
			{
				ParentObject.UseCharge(ChargeUse, LiveOnly: false, 0L);
			}
		}
		return flag;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			PerformReclamation();
		}
		return base.FireEvent(E);
	}
}
