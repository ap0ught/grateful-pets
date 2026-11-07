using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class GlotrotOnset : Effect, ITierInitialized
{
	public int Stage;

	public int Bonus;

	public int Days;

	public int Count;

	public bool SawSore;

	public GlotrotOnset()
	{
		DisplayName = "{{r|sore throat}}";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override string GetDetails()
	{
		return "Throat is sore and tongue is swollen.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Glotrot>() || Object.HasEffect<GlotrotOnset>())
		{
			return false;
		}
		if (Object.FireEvent("ApplyDiseaseOnset") && ApplyEffectEvent.Check(Object, "DiseaseOnset", this) && Object.FireEvent("ApplyGlotrot") && ApplyEffectEvent.Check(Object, "Glotrot", this))
		{
			Duration = 1;
			return true;
		}
		return false;
	}

	public override string GetDescription()
	{
		return "{{r|sore throat}}";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDiseaseOnsetEvent>.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDiseaseOnsetEvent E)
	{
		if (E.Effect == null)
		{
			E.Effect = this;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (Bonus != 0 && E.Vs == "Glotrot Disease Onset")
		{
			E.Roll += Bonus;
			if (E.Actual)
			{
				Bonus = 0;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Eating");
		Registrar.Register("EndTurn");
		Registrar.Register("DrinkingFrom");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Count++;
			if (Count >= 1200 && Duration > 0)
			{
				Count = 0;
				Days++;
				if (Object.MakeSave("Toughness", 13, null, null, "Glotrot Disease Onset"))
				{
					Stage--;
					if (SawSore && Stage > -2 && Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You feel a bit better.");
					}
				}
				else
				{
					Stage++;
					if (Stage < 3)
					{
						if (Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Your throat feels sore.");
						}
						SawSore = true;
					}
				}
				if (Stage <= -2)
				{
					if (SawSore && Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You feel better.");
					}
					Duration = 0;
				}
				else if (Stage >= 3 || Days >= 5)
				{
					Duration = 0;
					Object.ApplyEffect(new Glotrot());
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Container");
			if (Glotrot.IsFlamingIck(gameObjectParameter))
			{
				Duration = 0;
				if (Object.IsPlayer())
				{
					Popup.Show("It tastes even worse than you had imagined -- like a dead turtle boiled in phlegm.");
				}
			}
			else
			{
				LiquidVolume liquidVolume = gameObjectParameter.LiquidVolume;
				if (liquidVolume != null && liquidVolume.HasPrimaryOrSecondaryLiquid("honey") && Bonus < 3)
				{
					Bonus = 3;
				}
			}
		}
		else if (E.ID == "Eating")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Food");
			if (gameObjectParameter2 == null)
			{
				return true;
			}
			if (Bonus > 0)
			{
				return true;
			}
			if (gameObjectParameter2.Blueprint.Contains("Yuckwheat") && Bonus < 3)
			{
				Bonus = 3;
			}
		}
		return base.FireEvent(E);
	}
}
