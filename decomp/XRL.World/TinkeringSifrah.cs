using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
public abstract class TinkeringSifrah : SifrahGame
{
	public static readonly string CATEGORY = "Tinkering";

	public static bool AnyEnabled
	{
		get
		{
			if (!Options.SifrahExamine && !Options.SifrahRepair && !Options.SifrahReverseEngineer && !Options.SifrahDisarming)
			{
				return Options.SifrahHacking;
			}
			return true;
		}
	}

	public TinkeringSifrah()
	{
		CorrectTokenSound = "compartment_close";
		CorrectTokenSoundDelay = 500;
		IncorrectTokenSound = "Chop2";
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
			SifrahGame.AwardInsight(CATEGORY, "tinkering");
		}
	}
}
