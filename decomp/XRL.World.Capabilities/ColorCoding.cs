using XRL.Core;

namespace XRL.World.Capabilities;

public static class ColorCoding
{
	private static GameObject ThePlayer
	{
		get
		{
			if (XRLCore.Core.Game == null)
			{
				return null;
			}
			return The.Player;
		}
	}

	public static char ConsequentialColorChar(GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		if (ColorAsGoodFor != null)
		{
			if (ColorAsGoodFor.IsPlayer())
			{
				return 'G';
			}
			if (ColorAsGoodFor.IsPlayerLed())
			{
				return 'g';
			}
			if (ColorAsGoodFor.IsAlliedTowards(ThePlayer))
			{
				return 'g';
			}
			if (ColorAsGoodFor.Target == ThePlayer)
			{
				return 'R';
			}
			if (ColorAsGoodFor.IsHostileTowards(ThePlayer))
			{
				return 'r';
			}
		}
		if (ColorAsBadFor != null)
		{
			if (ColorAsBadFor.IsPlayer())
			{
				return 'R';
			}
			if (ColorAsBadFor.IsPlayerLed())
			{
				return 'r';
			}
			if (ColorAsBadFor.IsAlliedTowards(ThePlayer))
			{
				return 'r';
			}
			if (ColorAsBadFor.Target == ThePlayer)
			{
				return 'G';
			}
			if (ColorAsBadFor.IsHostileTowards(ThePlayer))
			{
				return 'g';
			}
		}
		return 'y';
	}

	public static string ConsequentialColor(GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null)
	{
		char c = ConsequentialColorChar(ColorAsGoodFor, ColorAsBadFor);
		if (c == 'y')
		{
			return null;
		}
		return "&" + c;
	}

	public static string ConsequentialColorize(string Text, GameObject ColorAsGoodFor = null, GameObject ColorAsBadFor = null, char Default = ' ')
	{
		char c = ConsequentialColorChar(ColorAsGoodFor, ColorAsBadFor);
		if (c == 'y')
		{
			if (Default != ' ')
			{
				return "{{" + Default + "|" + Text + "}}";
			}
			return Text;
		}
		return "{{" + c + "|" + Text + "}}";
	}
}
