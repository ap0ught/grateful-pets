using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Conversations.Parts;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Glotrot : Effect, ITierInitialized
{
	public int Stage = 1;

	public int Count;

	public int DrankIck;

	[NonSerialized]
	private long TearTurn = -1L;

	public Glotrot()
	{
		DisplayName = "glotrot";
	}

	public override string GetDetails()
	{
		if (Stage < 3)
		{
			return "Tongue is rotting away.\nStarts bleeding when eating or drinking.";
		}
		return "Tongue has rotted away.\nCan't speak.";
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Glotrot>())
		{
			return false;
		}
		if (Object.FireEvent("ApplyDisease") && ApplyEffectEvent.Check(Object, "Disease", this) && Object.FireEvent("ApplyGlotrot") && ApplyEffectEvent.Check(Object, "Glotrot", this))
		{
			Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_diseased");
			if (Object.IsPlayer())
			{
				int num = Stat.Random(2, 6);
				Achievement.GET_GLOTROT.Unlock();
				Popup.Show("You have contracted glotrot! Your tongue begins to bleed as the muscle rots away.");
				JournalAPI.AddAccomplishment("You contracted glotrot.", "Woe to the scroundrels and dastards who conspired to have =name= contract the rotting tongue!", $"Near the location of Golgotha, =name= was captured by bandits. {The.Player.GetPronounProvider().CapitalizedSubjective} languished in captivity for {num} years, eventually contracting glotrot before escaping to {The.Player.CurrentZone.GetTerrainDisplayName()}.", null, "general", MuralCategory.BodyExperienceBad, MuralWeight.Medium, null, -1L);
				Object.ApplyEffect(new Bleeding("1", 25, Object));
				AskPulldown();
			}
			Duration = 1;
			Stage = 1;
			return true;
		}
		return false;
	}

	public override string GetDescription()
	{
		if (DrankIck > 0)
		{
			return null;
		}
		return "{{glotrot|glotrot}}";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AnyRegenerableLimbsEvent.ID && ID != BeginConversationEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetTradePerformanceEvent>.ID)
		{
			return ID == RegenerateLimbEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AnyRegenerableLimbsEvent E)
	{
		if (E.IncludeMinor && ((Stage >= 3 && E.Voluntary) || DrankIck > 0))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RegenerateLimbEvent E)
	{
		if (E.IncludeMinor && ((Stage >= 3 && E.Voluntary) || DrankIck > 0))
		{
			RegrowTongue();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		AdvanceGlotrot(1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTradePerformanceEvent E)
	{
		E.LinearAdjustment -= 3.0;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (Object.IsPlayer() && Stage >= 3 && !Object.HasPart<Telepathy>())
		{
			ConversationUI.CurrentConversation?.AddPart(new GlotrotFilter
			{
				Propagation = 1
			});
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDrank");
		Registrar.Register("DrinkingFrom");
		Registrar.Register("Eating");
		Registrar.Register("EndTurn");
		Registrar.Register("Regenera");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenera" && E.GetIntParameter("Level") >= 5 && (!E.HasIntParameter("Involuntary") || DrankIck > 0))
		{
			RegrowTongue();
		}
		else if (E.ID == "AfterDrank" || E.ID == "Eating")
		{
			if (Stage < 3 && !E.HasFlag("External") && The.Game.Turns != TearTurn)
			{
				if (Object.IsPlayer())
				{
					Popup.ShowBlock("You tear open the tender muscle fibers of your tongue.");
				}
				Object.ApplyEffect(new Bleeding("1", 25, Object));
				AskPulldown();
				TearTurn = The.Game.Turns;
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Container");
			if (IsFlamingIck(gameObjectParameter))
			{
				DrankIck = 100;
				if (Object.IsPlayer())
				{
					Popup.Show("It tastes even worse than you had imagined -- like a dead turtle boiled in phlegm.");
				}
			}
			else if (25.in100())
			{
				int num = Math.Min(4, gameObjectParameter.LiquidVolume?.Volume ?? 0);
				if (num > 0)
				{
					gameObjectParameter.LiquidVolume?.UseDrams(num);
				}
				Cell cell = Object.CurrentCell;
				if (cell != null && !cell.OnWorldMap())
				{
					GameObject gameObject = GameObject.Create("Water");
					LiquidVolume liquidVolume = gameObject.LiquidVolume;
					liquidVolume.InitialLiquid = "water-1000";
					liquidVolume.Volume = num;
					liquidVolume.AddDrams("putrid", 1);
					cell.AddObject(gameObject);
				}
			}
		}
		return base.FireEvent(E);
	}

	public void RegrowTongue()
	{
		if (Duration <= 0)
		{
			return;
		}
		Stage = 1;
		if (DrankIck > 0)
		{
			if (Object.IsPlayer())
			{
				Achievement.CURE_GLOTROT.Unlock();
				Popup.ShowBlock("You are cured of glotrot. Your tongue regrows.");
				JournalAPI.AddAccomplishment("You were cured of glotrot and your tongue regrew.", "Blessed was the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year of " + Calendar.GetYear() + " AR, when =name= was cured of the rotting tongue!", "Some time in =year=, =name= came upon the bandits who had captured " + The.Player.GetPronounProvider().Objective + " and let " + The.Player.GetPronounProvider().Objective + " languish in captivity until " + The.Player.GetPronounProvider().Subjective + " contracted glotrot. =name= murdered their leader <spice.elements." + The.Player.GetMythicDomain() + ".murdermethods.!random> and cured " + The.Player.GetPronounProvider().Reflexive + " of the disease.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Medium, null, -1L);
			}
			Duration = 0;
			Stage = 0;
		}
		else if (Object.IsPlayer())
		{
			Popup.Show("Your tongue regrows.");
			JournalAPI.AddAccomplishment("Your tongue regrew.", "Remember the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year of " + Calendar.GetYear() + " AR, when through sheer strength of will =name= regrew " + The.Player.GetPronounProvider().PossessiveAdjective + " tongue.", "One auspicious eve while exploring the chutes of Golgotha, =name= dreamed that " + The.Player.GetPronounProvider().Subjective + " contracted a rotting disease and that " + The.Player.GetPronounProvider().PossessiveAdjective + " tongue festered and fell out. From then on, =name= vowed to always keep " + The.Player.GetPronounProvider().PossessiveAdjective + " tongue on his person.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Medium, null, -1L);
		}
	}

	public void AdvanceGlotrot(int Amount)
	{
		if (DrankIck > 0)
		{
			return;
		}
		Count += Amount;
		if (Count < 1200 || Stage >= 3)
		{
			return;
		}
		Stage++;
		if (Stage == 2)
		{
			if (Object.IsPlayer())
			{
				Popup.ShowBlock("Your tongue begins to bleed as the muscle rots away.");
			}
			Object.ApplyEffect(new Bleeding("1", 25, Object));
			AskPulldown();
		}
		if (Stage == 3)
		{
			if (Object.IsPlayer())
			{
				Popup.ShowBlock("Your tongue has rotted away.");
				JournalAPI.AddAccomplishment("Your tongue rotted away.", "On " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year of " + Calendar.GetYear() + " AR, =name= took a vow of silence and removed " + The.Player.GetPronounProvider().PossessiveAdjective + " own tongue.", "One auspicious eve while exploring the chutes of Golgotha, =name= dreamed that " + The.Player.GetPronounProvider().Subjective + " contracted a rotting disease and that " + The.Player.GetPronounProvider().PossessiveAdjective + " tongue festered and fell out. From then on, =name= vowed to always keep " + The.Player.GetPronounProvider().PossessiveAdjective + " tongue on his person.", null, "general", MuralCategory.BodyExperienceBad, MuralWeight.Medium, null, -1L);
			}
			Object.ApplyEffect(new Bleeding("1", 25, Object));
			AskPulldown();
		}
	}

	public void AskPulldown()
	{
		if (Object.OnWorldMap() && Popup.ShowYesNo("Do you want to stop travelling?") == DialogResult.Yes)
		{
			Object.PullDown();
		}
	}

	public static bool IsIck(LiquidVolume Volume)
	{
		if (Volume == null)
		{
			return false;
		}
		if (Volume.ComponentLiquids.Count < 3)
		{
			return false;
		}
		if (Volume.IsOpenVolume())
		{
			return false;
		}
		for (int i = 1; i <= 3; i++)
		{
			string stringGameState = The.Game.GetStringGameState("GlotrotCure" + i);
			if (!Volume.HasLiquid(stringGameState))
			{
				return false;
			}
			if (i == 3 && Volume.GetPrimaryLiquidID() != stringGameState)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsIck(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		return IsIck(Object.LiquidVolume);
	}

	public static bool IsFlamingIck(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (Object.IsAflame())
		{
			return IsIck(Object);
		}
		return false;
	}
}
