using System;
using XRL.Core;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class ConfuseOnEat : IPart
{
	public string Strength = "1d8";

	public string Duration = "5d3";

	public string Level = "1";

	public int BuildupTimeout = 15;

	public override bool SameAs(IPart p)
	{
		ConfuseOnEat confuseOnEat = p as ConfuseOnEat;
		if (confuseOnEat.Strength == Strength && confuseOnEat.Duration == Duration && confuseOnEat.Level == Level)
		{
			return confuseOnEat.BuildupTimeout == BuildupTimeout;
		}
		return false;
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
			long turns = XRLCore.Core.Game.Turns;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (!gameObjectParameter.Property.ContainsKey("ConfuseOnEatTurn"))
			{
				gameObjectParameter.Property.Add("ConfuseOnEatTurn", XRLCore.Core.Game.Turns.ToString());
			}
			if (!gameObjectParameter.Property.ContainsKey("ConfuseOnEatAmount"))
			{
				gameObjectParameter.Property.Add("ConfuseOnEatAmount", "0");
			}
			long num = gameObjectParameter.GetIntProperty("ConfuseOnEatTurn");
			int num2 = gameObjectParameter.GetIntProperty("ConfuseOnEatAmount");
			if (turns - num > BuildupTimeout)
			{
				num2 = 0;
			}
			if (PerformMentalAttack((MentalAttackEvent MAE) => Confusion.Confuse(MAE, Attack: false, Level.RollCached(), Level.RollCached() + 2, Silent: true), gameObjectParameter, gameObjectParameter, null, "Confuse OnEat", Strength, 8, Duration.RollCached(), int.MinValue, 0, num2 * 3))
			{
				E.RequestInterfaceExit();
			}
			gameObjectParameter.SetLongProperty("ConfuseOnEatTurn", turns);
			gameObjectParameter.SetStringProperty("ConfuseOnEatAmount", (num2 + 1).ToString());
		}
		return base.FireEvent(E);
	}
}
