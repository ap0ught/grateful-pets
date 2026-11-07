namespace XRL.World.Capabilities;

public static class DifficultyEvaluation
{
	public const int IMPOSSIBLE = 15;

	public const int VERY_TOUGH = 10;

	public const int TOUGH = 5;

	public const int AVERAGE = -5;

	public const int EASY = -10;

	public const int TRIVIAL = int.MinValue;

	public static int? GetDifficultyRating(GameObject Subject, GameObject Actor = null, bool IgnoreHideCon = false)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return -20;
		}
		if (!GameObject.Validate(ref Actor))
		{
			Actor = The.Player;
			if (!GameObject.Validate(ref Actor))
			{
				return null;
			}
		}
		if (!Subject.IsCombatObject())
		{
			return null;
		}
		if (!IgnoreHideCon && Subject.HasPropertyOrTag("HideCon"))
		{
			return null;
		}
		int num = Subject.Stat("Level");
		string text = (Subject.IsPlayer() ? "Player" : Subject.GetPropertyOrTag("Role"));
		int num2 = num + GetRoleLevelOffset(text);
		int num3 = Actor.Stat("Level");
		string text2 = (Actor.IsPlayer() ? "Player" : Actor.GetPropertyOrTag("Role"));
		int num4 = num3 + GetRoleLevelOffset(text2);
		return GetDifficultyEvaluationEvent.GetFor(Subject, Actor, text, text2, num, num3, num2, num4, num2 - num4);
	}

	public static int GetRoleLevelOffset(string Role)
	{
		return Role switch
		{
			"Minion" => -5, 
			"Skirmisher" => -3, 
			"Leader" => 3, 
			"Summoner" => 4, 
			"Hero" => 5, 
			_ => 0, 
		};
	}

	public static string GetDifficultyDescription(GameObject Subject, GameObject Actor = null, int? Rating = null, bool IgnoreHideCon = false)
	{
		if (!Rating.HasValue)
		{
			Rating = GetDifficultyRating(Subject, Actor, IgnoreHideCon);
			if (!Rating.HasValue)
			{
				return null;
			}
		}
		if (Rating >= 15)
		{
			return "{{R|Impossible}}";
		}
		if (Rating >= 10)
		{
			return "{{r|Very Tough}}";
		}
		if (Rating >= 5)
		{
			return "{{W|Tough}}";
		}
		if (Rating >= -5)
		{
			return "{{w|Average}}";
		}
		if (Rating >= -10)
		{
			return "{{g|Easy}}";
		}
		return "{{G|Trivial}}";
	}

	public static void GetDifficultyDescription(out string Description, out string Color, GameObject Subject, GameObject Actor = null, int? Rating = null, bool IgnoreHideCon = false)
	{
		Description = null;
		Color = null;
		if (!Rating.HasValue)
		{
			Rating = GetDifficultyRating(Subject, Actor, IgnoreHideCon);
			if (!Rating.HasValue)
			{
				return;
			}
		}
		if (Rating >= 15)
		{
			Description = "Impossible";
			Color = "R";
		}
		else if (Rating >= 10)
		{
			Description = "Very Tough";
			Color = "r";
		}
		else if (Rating >= 5)
		{
			Description = "Tough";
			Color = "W";
		}
		else if (Rating >= -5)
		{
			Description = "Average";
			Color = "w";
		}
		else if (Rating >= -10)
		{
			Description = "Easy";
			Color = "g";
		}
		else
		{
			Description = "Trivial";
			Color = "G";
		}
	}

	public static int GetDifficultyFromDescription(string Description)
	{
		switch (Description)
		{
		case "{{R|Impossible}}":
		case "&RImpossible":
		case "Impossible":
			return 15;
		case "{{r|Very Tough}}":
		case "&rVery Tough":
		case "Very Tough":
			return 10;
		case "{{W|Tough}}":
		case "&WTough":
		case "Tough":
			return 5;
		case "{{w|Average}}":
		case "&wAverage":
		case "Average":
			return -5;
		case "{{g|Easy}}":
		case "&gEasy":
		case "Easy":
			return -10;
		default:
			return int.MinValue;
		}
	}
}
