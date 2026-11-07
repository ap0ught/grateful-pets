using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Nectar_Tonic_Applicator : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyTonic");
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage == null && (E.ForPermission || E.Actor.Health() < 0.1))
		{
			E.ApplyScore(1);
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			int intParameter = E.GetIntParameter("Dosage");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Subject");
			string text = "";
			if (intParameter <= 0 || (ParentObject != null && ParentObject.IsTemporary && !gameObjectParameter.IsTemporary))
			{
				text += "The experience is fleeting.";
			}
			else
			{
				gameObjectParameter.PermuteRandomMutationBuys();
				if (gameObjectParameter.IsTrueKin())
				{
					if (gameObjectParameter.HasStat("AP"))
					{
						gameObjectParameter.GetStat("AP").BaseValue += intParameter;
						text = text + "{{C|You gain " + intParameter.Things("attribute point") + "!}}";
					}
				}
				else if (50.in100())
				{
					int num = Stat.Random(1, 6);
					string text2 = "Strength";
					if (num == 1)
					{
						text2 = "Strength";
					}
					if (num == 2)
					{
						text2 = "Intelligence";
					}
					if (num == 3)
					{
						text2 = "Ego";
					}
					if (num == 4)
					{
						text2 = "Agility";
					}
					if (num == 5)
					{
						text2 = "Willpower";
					}
					if (num == 6)
					{
						text2 = "Toughness";
					}
					if (gameObjectParameter.HasStat(text2))
					{
						gameObjectParameter.GetStat(text2).BaseValue += intParameter;
						text = text + "{{C|You gain " + intParameter.Things("point") + " of " + text2 + "!}}";
					}
				}
				else if (gameObjectParameter.HasStat("MP"))
				{
					gameObjectParameter.GainMP(intParameter);
					text = text + "{{C|You gain " + intParameter.Things("mutation point") + "!}}";
				}
				if (gameObjectParameter.IsPlayer())
				{
					string text3 = "You taste life as it was distilled by the Eaters, Qud's primordial masons.";
					if (!text.IsNullOrEmpty())
					{
						text3 = text3 + "\n\n" + text;
					}
					Popup.Show(text3);
					gameObjectParameter.GetPart<Leveler>().SifrahInsights();
				}
			}
		}
		return base.FireEvent(E);
	}
}
