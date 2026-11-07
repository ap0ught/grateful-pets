using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class SphynxSalt_Tonic : ITonicEffect, ITierInitialized
{
	public bool WasPlayer;

	public int HitpointsAtSave;

	public int TemperatureAtSave;

	public bool Overdose;

	private long ActivatedSegment;

	[NonSerialized]
	private Guid GlimpseID;

	public SphynxSalt_Tonic()
	{
	}

	public SphynxSalt_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		if (50.in100())
		{
			Overdose = true;
		}
		Duration = Stat.Roll(20, 40);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{sphynx|sphynx}} {{Y|salt}} tonic";
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{sphynx|sphynx}} {{Y|salt}} tonic";
	}

	public override string GetDetails()
	{
		string text = ((!Object.IsTrueKin()) ? "Immune to confusing attacks.\nActivated mental mutations cool down twice as quickly." : "Immune to confusing attacks.\nCan peer into the near future.");
		if (Options.AnySifrah)
		{
			text += "\nAdds a bonus turn in psionic Sifrah games.";
		}
		return text;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("The clouds part in your mind and a ray of clarity strikes through.");
		}
		if (Object.GetLongProperty("Overdosing", 0L) == 1 || Overdose)
		{
			FireEvent(Event.New("Overdose"));
			if (Options.AnySifrah)
			{
				PsionicSifrah.AwardInsight();
			}
		}
		else if (Options.AnySifrah && 30.in100())
		{
			PsionicSifrah.AwardInsight();
		}
		int num = 0;
		while (Object.HasEffect<Confused>() && ++num < 20)
		{
			Object.RemoveEffect<Confused>();
		}
		if (Object.HasRegisteredEvent("InitiatePrecognition"))
		{
			Event obj = Event.New("InitiatePrecognition", "Duration", Duration);
			Object.FireEvent(obj);
			Duration = obj.GetIntParameter("Duration");
		}
		SphynxSalt_Tonic effect = Object.GetEffect<SphynxSalt_Tonic>();
		if (effect != null)
		{
			effect.Duration += Duration;
			return false;
		}
		if (Object.IsPlayer())
		{
			WasPlayer = true;
			if (Object.IsTrueKin())
			{
				GlimpseID = Precognition.Save();
			}
		}
		else
		{
			WasPlayer = false;
			if (SensePsychic.SensePsychicFromPlayer(Object) != null)
			{
				IComponent<GameObject>.AddPlayerMessage("You sense a subtle psychic disturbance.");
			}
		}
		HitpointsAtSave = Object.hitpoints;
		TemperatureAtSave = Object.Physics.Temperature;
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.IsTrueKin() && WasPlayer)
			{
				if (Popup.ShowYesNo("Your {{sphynx|sphynx}} {{Y|salt}} is about to run out. Would you like to return to the start of your vision?") == DialogResult.Yes)
				{
					AutoAct.Interrupt();
					Precognition.Load(GlimpseID, Object);
				}
			}
			else
			{
				Popup.Show("Your mind clouds over once again.");
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == PooledEvent<GetPsionicSifrahSetupEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		E.Turns++;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			ActivatedAbilities activatedAbilities = Object.ActivatedAbilities;
			if (activatedAbilities?.AbilityByGuid != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value.Class.Contains("Mental"))
					{
						int cooldown = value.Cooldown;
						if (cooldown > 10)
						{
							value.Cooldown = cooldown - 10;
						}
						else if (cooldown > 0)
						{
							value.Cooldown = 0;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyAttackConfusion");
		Registrar.Register("Overdose");
		Registrar.Register("BeforeDie");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyAttackConfusion")
		{
			return false;
		}
		if (E.ID == "BeforeDie" && Duration > 0 && Object.IsTrueKin())
		{
			return Precognition.OnBeforeDie(Object, Guid.Empty, GlimpseID, ref Duration, ref HitpointsAtSave, ref TemperatureAtSave, ref ActivatedSegment, WasPlayer, RealityDistortionBased: true, null);
		}
		if (E.ID == "Overdose" && Duration > 0)
		{
			Duration = 0;
			ApplyOverdose(Object);
		}
		return base.FireEvent(E);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		ApplyOverdose(Object);
	}

	public static void ApplyOverdose(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetLongProperty("Overdosing", 0L) == 1)
			{
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You cannot see to see -- your mind cracks as a bell struck by a hammer.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You cannot see to see -- your mind cracks as a bell struck by a hammer.");
			}
		}
		if (!Object.HasPart<ActivatedAbilities>())
		{
			return;
		}
		ActivatedAbilities part = Object.GetPart<ActivatedAbilities>();
		if (part == null || part.AbilityByGuid == null)
		{
			return;
		}
		foreach (ActivatedAbilityEntry value in part.AbilityByGuid.Values)
		{
			if (value.Class.Contains("Mental") && value.Cooldown <= 2000 && value.Cooldown >= -1)
			{
				value.Cooldown = 2021;
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 30 && num < 40)
			{
				E.Tile = null;
				E.RenderString = "ì";
				E.ColorString = "&C";
			}
		}
		return true;
	}
}
