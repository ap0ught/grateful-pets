using System;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SapOnPenetration : IPart
{
	public int Chance = 100;

	public string Stat = "Hitpoints";

	public string Amount = "1-2";

	public bool NaturalOnly = true;

	[NonSerialized]
	private bool ShownPopup;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == GetDifficultyEvaluationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDifficultyEvaluationEvent E)
	{
		if (GameObject.Validate(E.Actor) && E.Actor.HasStat(Stat))
		{
			E.MinimumRating(5);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("AttackerHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit")
		{
			if (E.GetIntParameter("Penetrations") > 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject Object = E.GetGameObjectParameter("Defender");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
				GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
				if (GameObject.Validate(ref Object) && Object.HasStat(Stat))
				{
					GameObject subject = Object;
					GameObject projectile = gameObjectParameter3;
					if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, gameObjectParameter2, "Part SapOnPenetration Activation", Chance, subject, projectile).in100() && (!NaturalOnly || gameObjectParameter2 == null || gameObjectParameter2.IsNatural()))
					{
						E.SetFlag("DidSpecialEffect", State: true);
						string text = E.GetStringParameter("Properties") ?? "";
						if (!text.Contains("DrainedStat"))
						{
							E.SetParameter("Properties", (text == "") ? "DrainedStat" : (text + ",DrainedStat"));
						}
					}
				}
			}
		}
		else if (E.ID == "AttackerAfterDamage")
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter5 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter6 = E.GetGameObjectParameter("Weapon");
			if ((E.GetStringParameter("Properties") ?? "").Contains("DrainedStat"))
			{
				int num = XRL.Rules.Stat.Roll(Amount);
				string text2 = ((num != 1) ? "points" : "point");
				bool flag = !ShownPopup && gameObjectParameter5.IsPlayerControlled();
				gameObjectParameter5.GetStat(Stat).BaseValue -= num;
				DidXToY("permanently drain", gameObjectParameter5, Statistic.GetStatNarration(Stat) + " by " + num + " " + text2, "!", null, null, null, gameObjectParameter5, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag);
				E.SetFlag("DidSpecialEffect", State: true);
				if (!gameObjectParameter5.IsHostileTowards(gameObjectParameter4))
				{
					gameObjectParameter5.AddOpinion<OpinionAttack>(gameObjectParameter4, gameObjectParameter6);
				}
				ShownPopup |= flag;
				if (gameObjectParameter5.IsPlayer())
				{
					gameObjectParameter5.PlayWorldSound("sfx_characterMod_generic_negative_gain");
					AutoAct.Interrupt(gameObjectParameter4, ShowIndicator: true, IsThreat: true);
				}
			}
		}
		return base.FireEvent(E);
	}
}
