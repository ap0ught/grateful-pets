using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
public abstract class PsionicSifrah : SifrahGame
{
	public static readonly string CATEGORY = "Psionic";

	public static bool AnyEnabled
	{
		get
		{
			if (!Options.SifrahRealityDistortion)
			{
				return Options.SifrahPsychicCombat;
			}
			return true;
		}
	}

	public PsionicSifrah()
	{
		CorrectTokenSound = "Sounds/StatusEffects/sfx_statusEffect_spacetimeWeirdness";
		CorrectTokenSoundDelay = 400;
		IncorrectTokenSound = "Spark2";
		IncorrectTokenSoundDelay = 300;
	}

	public override string GetSifrahCategory()
	{
		return CATEGORY;
	}

	public static void AwardInsight()
	{
		if (AnyEnabled)
		{
			SifrahGame.AwardInsight(CATEGORY, "psionics");
		}
	}
}
