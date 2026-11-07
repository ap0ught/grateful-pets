using System;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class ShadeOil_Tonic : ITonicEffect, ITierInitialized
{
	public bool bOverdose;

	public Guid ActivatedAbilityID = Guid.Empty;

	public ShadeOil_Tonic()
	{
	}

	public ShadeOil_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		if (If.CoinFlip())
		{
			bOverdose = true;
		}
		Duration = Stat.Roll(41, 50);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return base.GetEffectType() | 0x100;
	}

	public override string GetDescription()
	{
		return "{{shade|shade oil}} tonic";
	}

	public override string GetStateDescription()
	{
		return "under the effects of {{shade|shade oil}} tonic";
	}

	public override string GetDetails()
	{
		if (Object.IsTrueKin())
		{
			return "+8 DV\nCan phase out of the spatial dimension for 7-9 turns.";
		}
		return "+8 DV\n25% chance per turn to phase out of the spatial dimension for 1-3 turns.";
	}

	public override bool Apply(GameObject Object)
	{
		DidX("begin", "to flicker in and out of corporeality", null, null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		ApplyChanges();
		if (Object.GetLongProperty("Overdosing", 0L) == 1 || bOverdose)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("assume", "full corporeal form again");
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges(bool Silent = false)
	{
		StatShifter.SetStatShift(Object, "DV", 8);
		if (Object.IsTrueKin())
		{
			ActivatedAbilityID = Object.AddActivatedAbility("Phase", "ShadeOilPhase", "Tonics", null, "°", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, null, Renderable.UITile("Mutations/phasing.bmp", 'W', 'C'));
		}
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts(Object);
		Object.RemoveActivatedAbility(ref ActivatedAbilityID);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
		{
			return ID == PooledEvent<GetPsionicSifrahSetupEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, GetStats);
		return base.HandleEvent(E);
	}

	public void GetStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", "7-9 rounds");
	}

	public override bool HandleEvent(GetPsionicSifrahSetupEvent E)
	{
		if (E.Type == "RealityDistortion")
		{
			E.Rating++;
			E.Turns++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0 && !Object.IsTrueKin() && !Object.OnWorldMap() && !Object.HasEffect<Phased>() && 25.in100())
		{
			Object.ApplyEffect(new Phased(Stat.Random(1, 3)));
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		Registrar.Register("Overdose");
		Registrar.Register("ShadeOilPhase");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ShadeOilPhase")
		{
			if (Object.OnWorldMap())
			{
				if (Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
			}
			else
			{
				Object.RemoveActivatedAbility(ref ActivatedAbilityID);
				Object.ApplyEffect(new Phased(Stat.Random(7, 9)));
			}
		}
		else if (E.ID == "TonicAutoApplied")
		{
			if (Duration > 0 && !Object.HasEffect<Phased>())
			{
				if (Object.IsTrueKin())
				{
					StringBuilder stringBuilder = Event.NewStringBuilder();
					GameObject gameObjectParameter = E.GetGameObjectParameter("By");
					stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(GetDescription())).Append(" has been applied");
					if (gameObjectParameter != null)
					{
						stringBuilder.Append(" by ");
						if (!gameObjectParameter.HasProperName)
						{
							if (gameObjectParameter.GetObjectContext() == Object)
							{
								stringBuilder.Append("your ");
							}
							else
							{
								stringBuilder.Append(gameObjectParameter.a);
							}
						}
						stringBuilder.Append(gameObjectParameter.ShortDisplayName);
					}
					stringBuilder.Append(". Do you wish to phase out immediately?");
					if (Popup.ShowYesNo(stringBuilder.ToString(), "Sounds/UI/ui_notification", AllowEscape: false) == DialogResult.Yes)
					{
						FireEvent(Event.New("ShadeOilPhase"));
					}
				}
				else if (25.in100())
				{
					Object.ApplyEffect(new Phased(Stat.Random(1, 3)));
				}
			}
		}
		else if (E.ID == "Overdose")
		{
			if (Duration > 0 && !Object.HasProperty("EvilTwin"))
			{
				Duration = 0;
				ApplyOverdose(Object);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
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
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You flicker through spacetime uncontrollably.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You flicker through spacetime uncontrollably.");
			}
		}
		int i = 0;
		for (int num = Stat.Random(1, 3); i < num; i++)
		{
			EvilTwin.CreateEvilTwin(Object, "Shadow", null, "{{c|You sense a shadowy presence nearby.}}", "&K", null, null, MakeExtras: true, "It's shadow you.", "Entropic-100");
		}
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (Duration > 0 && num > 0 && num < 10)
		{
			E.Tile = null;
			E.RenderString = "@";
			E.ColorString = "&K";
		}
		return true;
	}
}
