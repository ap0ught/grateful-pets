using System;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Mutating : Effect, ITierInitialized
{
	public string Population = "MutatingResults";

	public bool Triggered;

	public int PermuteMutationsAt;

	public bool MutationsPermuted;

	public Mutating()
	{
		DisplayName = "{{M|mutating}}";
	}

	public Mutating(int Duration, string Population = "MutatingResults")
		: this()
	{
		base.Duration = Duration;
		this.Population = Population;
		PermuteMutationsAt = Duration / 2;
	}

	public Mutating(int Duration, int PermuteMutationsAt, string Population = "MutatingResults")
		: this(Duration, Population)
	{
		this.PermuteMutationsAt = PermuteMutationsAt;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Roll(100, 200);
		PermuteMutationsAt = Duration / 2;
	}

	public override int GetEffectType()
	{
		return 100663300;
	}

	public override bool SameAs(Effect e)
	{
		Mutating mutating = e as Mutating;
		if (mutating.Population != Population)
		{
			return false;
		}
		if (mutating.Triggered != Triggered)
		{
			return false;
		}
		if (mutating.PermuteMutationsAt != PermuteMutationsAt)
		{
			return false;
		}
		if (mutating.MutationsPermuted != MutationsPermuted)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "In the process of mutating.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.IsMutant() && !Object.IsTrueKin())
		{
			return false;
		}
		if (Object.HasEffect<Mutating>())
		{
			return false;
		}
		if (!Object.FireEvent("ApplyMutating"))
		{
			return false;
		}
		Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_neutral-weirdVitality");
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You start to feel unstable.", 'M');
		}
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration > 0)
		{
			Duration--;
			if (Duration < PermuteMutationsAt && !MutationsPermuted)
			{
				MutationsPermuted = true;
				Object.PermuteRandomMutationBuys();
				if (Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You feel increasingly unstable.", 'M');
					Object.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_neutral-weirdVitality");
				}
			}
			if (Duration <= 0 && !Triggered)
			{
				Triggered = true;
				string blueprint = PopulationManager.RollOneFrom(Population).Blueprint;
				if (blueprint == "Mutation")
				{
					MutationEntry mutationEntry = MutationsAPI.FindRandomMutationFor(Object, (MutationEntry e) => !e.IsDefect());
					if (mutationEntry != null)
					{
						PlayWorldSound("Sounds/Misc/sfx_characterMod_mutation_positive");
						if (Object.IsPlayer())
						{
							Popup.Show("Your genome destabilizes and you gain a new mutation:\n\n{{W|" + mutationEntry.GetDisplayName() + "}}", null, null);
							Achievement.MUTATION_FROM_GAMMAMOTH.Unlock();
						}
						MutationsAPI.ApplyMutationTo(Object, mutationEntry);
					}
				}
				else if (blueprint == "Defect")
				{
					MutationEntry mutationEntry2 = MutationsAPI.FindRandomMutationFor(Object, (MutationEntry e) => e.IsDefect(), null, allowMultipleDefects: true);
					if (mutationEntry2 != null)
					{
						PlayWorldSound("Sounds/Misc/sfx_characterMod_mutation_negative");
						if (Object.IsPlayer())
						{
							Popup.Show("Your genome destabilizes and you gain a new defect:\n\n{{W|" + mutationEntry2.GetDisplayName() + "}}", null, null);
						}
						MutationsAPI.ApplyMutationTo(Object, mutationEntry2);
					}
				}
				else if (blueprint.StartsWith("Points:"))
				{
					int num = Stat.Roll(blueprint.Split(':')[1]);
					if ((Object.IsMutant() || Object.IsTrueKin()) && Object.GainMP(num) && Object.IsPlayer())
					{
						Popup.Show("Your genome destabilizes and you gain " + Grammar.Cardinal(num) + " mutation " + ((num == 1) ? "point" : "points") + ".");
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
