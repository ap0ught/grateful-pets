using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Monochrome : Effect, ITierInitialized
{
	public bool monochromeApplied;

	public int DrankCure;

	public Monochrome()
	{
		Duration = 1;
		DisplayName = "monochrome";
	}

	public override string GetDetails()
	{
		return "Sees shades of only a single color.";
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override string GetDescription()
	{
		return "monochrome";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyDisease"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Disease", this))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyMonochrome"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Monochrome", this))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_diseased");
		Monochrome effect = Object.GetEffect<Monochrome>();
		if (effect != null)
		{
			effect.Duration += Duration;
			return false;
		}
		if (Object.IsPlayer())
		{
			int num = Stat.Random(2, 6);
			Achievement.GET_MONOCHROME.Unlock();
			Popup.Show("You have contracted monochrome! Color starts to seep out of the world.");
			JournalAPI.AddAccomplishment("You contracted monochrome.", "Woe to the scroundrels and dastards who conspired to have =name= contract univision!", $"Near the location of Omonporch, =name= was captured by bandits. {The.Player.GetPronounProvider().CapitalizedSubjective} languished in captivity for {num} years, eventually contracting glotrot before escaping to {The.Player.CurrentZone.GetTerrainDisplayName()}.", null, "general", MuralCategory.BodyExperienceBad, MuralWeight.Medium, null, -1L);
			monochromeApplied = true;
			GameManager.Instance.GreyscaleLevel++;
			The.Game.SetIntGameState("HasMonochrome", 1);
		}
		MonochromeOnset effect2 = Object.GetEffect<MonochromeOnset>();
		if (effect2 != null && effect2.Duration > 0)
		{
			effect2.Duration = 0;
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (monochromeApplied)
		{
			GameManager.Instance.GreyscaleLevel--;
			The.Game.RemoveIntGameState("HasMonochrome");
		}
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("FlashbangHit");
		Registrar.Register("DrinkingFrom");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (monochromeApplied && !Object.IsPlayer() && Duration == 1)
			{
				Duration = 2;
				GameManager.Instance.GreyscaleLevel--;
			}
			if (monochromeApplied && Object.IsPlayer() && Duration == 2)
			{
				Duration = 1;
				GameManager.Instance.GreyscaleLevel++;
			}
			if (DrankCure > 0)
			{
				DrankCure--;
			}
		}
		else if (E.ID == "FlashbangHit")
		{
			if (DrankCure > 0)
			{
				Duration = 0;
				DrankCure = 0;
				if (monochromeApplied && Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Color starts to seep into the world.");
					Achievement.CURE_MONOCHROME.Unlock();
					Popup.Show("You are cured of monochrome.");
					JournalAPI.AddAccomplishment("You were cured of monochrome and your vision returned to color.", "Blessed was the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year of " + Calendar.GetYear() + " AR, when =name= was cured of greying eye!", "Some time in =year=, =name= came upon the bandits who had captured " + The.Player.GetPronounProvider().Objective + " and let " + The.Player.GetPronounProvider().Objective + " languish in captivity until " + The.Player.GetPronounProvider().Subjective + " contracted monochrome. =name= murdered their leader <spice.elements." + The.Player.GetMythicDomain() + ".murdermethods.!random> and cured " + The.Player.GetPronounProvider().Reflexive + " of the disease.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Medium, null, -1L);
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Container");
			string stringGameState = The.Game.GetStringGameState("MonochromeCure");
			if (gameObjectParameter.LiquidVolume.IsPureLiquid(stringGameState))
			{
				DrankCure = 51;
			}
		}
		return base.FireEvent(E);
	}
}
