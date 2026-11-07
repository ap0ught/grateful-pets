using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

/// This class is not used in the base game.
[Serializable]
[RequiresMod(WorkshopID = 2242612659uL)]
public class CardiacArrest : Effect, ITierInitialized
{
	public int HeartsStopped = 1;

	public int Progress;

	public int IllFactor = 5;

	public string SaveAttribute = "Toughness,Willpower";

	public int SaveTarget = 30;

	public string SaveVs = "CardiacArrest CardiacArrestRecovery";

	public CardiacArrest()
	{
		Duration = 1;
		DisplayName = "{{W|cardiac arrest}}";
	}

	public CardiacArrest(int HeartsStopped)
		: this()
	{
		this.HeartsStopped = HeartsStopped;
	}

	public void Initialize(int Tier)
	{
		Tier = XRL.World.Capabilities.Tier.Constrain(Stat.Random(Tier - 2, Tier + 2));
		SaveTarget = 16 + Tier * 3;
	}

	public override int GetEffectType()
	{
		return 100663312;
	}

	public override string GetStateDescription()
	{
		return "{{W|in cardiac arrest}}";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("-1 to every attribute per turn (current: " + -Progress + ").");
		if (HeartsStopped < Object.GetHeartCount())
		{
			stringBuilder.Compound("Unable to progress until all hearts have stopped (current:", "\n").Append(HeartsStopped).Append(").");
		}
		stringBuilder.Compound("Dies when any attribute reaches zero.", "\n").Compound("Doesn't regenerate hit points.", "\n").Compound("Healing effects are only half as effective.", "\n");
		if (!string.IsNullOrEmpty(SaveAttribute))
		{
			stringBuilder.Compound("Difficulty ", "\n").Append(SaveTarget).Append(' ')
				.Append(SaveAttribute.Replace(",", "/"))
				.Append(" save each turn to recover.");
		}
		stringBuilder.Compound("Electrical damage has a percentage chance to cause recovery equal to the damage taken.", "\n");
		if (IllFactor > 0)
		{
			stringBuilder.Compound("Will become ill upon recovery for 5 turns per attribute penalty point.", "\n");
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		int heartCount = Object.GetHeartCount();
		if (heartCount < 1)
		{
			return false;
		}
		CardiacArrest effect = Object.GetEffect<CardiacArrest>();
		if (effect != null)
		{
			if (effect.HeartsStopped < heartCount)
			{
				DidX("go", "into another cardiac arrest", "!", null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Object.IsPlayer());
				effect.HeartsStopped += HeartsStopped;
			}
			return false;
		}
		DidX("go", "into cardiac arrest", "!", null, null, null, Object, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Object.IsPlayer());
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		if (Object.IsPlayer())
		{
			if (HeartsStopped > 1)
			{
				Popup.Show("{{G|Your hearts restart!}}");
			}
			else
			{
				Popup.Show("{{G|Your heart restarts!}}");
			}
		}
		else
		{
			DidX("look", "less stricken", null, null, null, Object);
		}
		int num = Progress * IllFactor;
		if (num > 0)
		{
			Object.ApplyEffect(new Ill(num, 1, "You feel shaken and infirm.", Object.IsPlayer()));
		}
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (XRLCore.CurrentFrame > 45)
		{
			E.Tile = null;
			E.RenderString = "û";
			E.ColorString = "&W^k";
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetCompanionStatusEvent>.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCompanionStatusEvent E)
	{
		if (E.Object == Object)
		{
			E.AddStatus("in cardiac arrest", 30);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object == Object && E.Damage.IsElectricDamage() && E.Damage.Amount.in100())
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CardiacArrestProgress();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Recuperating");
		Registrar.Register("Regenerating");
		Registrar.Register("Healing");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 2);
		}
		else
		{
			if (E.ID == "Regenerating")
			{
				E.SetParameter("Amount", 0);
				return false;
			}
			if (E.ID == "Recuperating")
			{
				Object.RemoveEffect(this);
			}
		}
		return base.FireEvent(E);
	}

	public void CardiacArrestProgress()
	{
		int heartCount = Object.GetHeartCount();
		bool flag = HeartsStopped >= heartCount;
		if (flag)
		{
			Progress++;
			foreach (string attribute in Statistic.Attributes)
			{
				if (Object.Stat(attribute) <= 1)
				{
					Object.Die(null, "cardiac arrest", "Your " + ((heartCount > 1) ? "hearts" : "heart") + " stopped.", Object.Its + " " + ((heartCount > 1) ? "hearts" : "heart") + " stopped.");
					return;
				}
				StatShifter.SetStatShift(attribute, -Progress);
			}
		}
		if (!string.IsNullOrEmpty(SaveAttribute) && Object.MakeSave(SaveAttribute, SaveTarget, null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, !flag))
		{
			Object.RemoveEffect(this);
		}
	}
}
