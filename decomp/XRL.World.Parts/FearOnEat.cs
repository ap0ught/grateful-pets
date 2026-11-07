using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FearOnEat : IPart
{
	public string Strength = "1d8";

	public string Duration = "2d4";

	public override bool SameAs(IPart p)
	{
		FearOnEat fearOnEat = p as FearOnEat;
		if (fearOnEat.Strength != Strength)
		{
			return false;
		}
		if (fearOnEat.Duration != Duration)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject Eater = E.GetGameObjectParameter("Eater");
			if (PerformMentalAttack((MentalAttackEvent MAE) => Terrified.Attack(MAE, null, Eater.CurrentCell), Eater, Eater, null, "Terrify OnEat", Strength, 8, Duration.RollCached()) && Eater.IsPlayer())
			{
				Eater.ForfeitTurn();
				E.RequestInterfaceExit();
			}
		}
		return base.FireEvent(E);
	}
}
