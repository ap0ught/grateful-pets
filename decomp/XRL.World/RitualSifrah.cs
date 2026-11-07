using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
public abstract class RitualSifrah : SifrahGame
{
	public static readonly string CATEGORY = "Ritual";

	public static bool AnyEnabled => Options.SifrahItemNaming;

	public RitualSifrah()
	{
		CorrectTokenSound = "Sounds/Interact/sfx_artifact_examination_success_total";
		CorrectTokenSoundDelay = 500;
		IncorrectTokenSound = "Sounds/StatusEffects/sfx_statusEffect_frozen";
		IncorrectTokenSoundDelay = 200;
	}

	public override string GetSifrahCategory()
	{
		return CATEGORY;
	}

	public static void AwardInsight()
	{
		if (AnyEnabled)
		{
			SifrahGame.AwardInsight(CATEGORY, "ritual");
		}
	}
}
