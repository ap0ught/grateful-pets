using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GraveMoss : IPart
{
	public bool Triggered;

	public int Turns = 20;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != EffectAppliedEvent.ID && (ID != SingletonEvent<EndTurnEvent>.ID || !Triggered))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsBlood(E.Effect))
		{
			Trigger();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (GameObject.Validate(E.Object) && E.Object.HasPropertyOrTag("Corpse"))
		{
			Trigger();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Triggered && Turns > 0)
		{
			Turns--;
			if (Turns <= 0)
			{
				Cell cell = ParentObject.CurrentCell;
				cell.GetFirstObjectWithPropertyOrTag("Corpse")?.Destroy();
				cell.AddObject("Gorged Growth").MakeActive();
				ParentObject.Destroy();
			}
		}
		return base.HandleEvent(E);
	}

	public void Trigger()
	{
		if (!Triggered)
		{
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("start") + " to fizz hungrily.");
			}
			Triggered = true;
		}
	}

	public bool IsBlood(LiquidVolume Liquid)
	{
		return Liquid?.HasPrimaryOrSecondaryLiquid("blood") ?? false;
	}

	public bool IsBlood(Effect FX)
	{
		if (FX is LiquidCovered liquidCovered && IsBlood(liquidCovered.Liquid))
		{
			return true;
		}
		if (FX is LiquidStained liquidStained && IsBlood(liquidStained.Liquid))
		{
			return true;
		}
		return false;
	}
}
