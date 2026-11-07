using System;
using XRL.Core;
using XRL.Wish;

namespace XRL.World;

[Serializable]
[HasWishCommand]
public static class Calendar
{
	public const int TurnsPerYear = 438000;

	public const int TurnsPerDay = 1200;

	public const int TurnsPerHour = 50;

	public const int StartOfDay = 3250;

	public const int StartOfNight = 10000;

	[Obsolete("Use TurnsPerDay")]
	public const int turnsPerDay = 1200;

	[Obsolete("Use TurnsPerHour")]
	public const int turnsPerHour = 50;

	[Obsolete("Use StartOfDay")]
	public const int startOfDay = 3250;

	[Obsolete("Use StartOfNight")]
	public const int startOfNight = 10000;

	public static long TotalTimeTicks => XRLCore.Core.Game.TimeTicks;

	public static int CurrentDaySegment => (int)(TotalTimeTicks % 1200) * 10;

	public static int GetYear()
	{
		return (int)(TotalTimeTicks / 438000) + 1001;
	}

	public static int GetYear(long Time)
	{
		return (int)(Time / 438000) + 1001;
	}

	public static string GetMonth()
	{
		return GetMonth((int)TotalTimeTicks % 438000);
	}

	public static string GetMonth(long Time)
	{
		return GetMonth((int)(Time % 438000));
	}

	public static string GetMonth(int TimeOfYear)
	{
		if (TimeOfYear < 216001)
		{
			if (TimeOfYear < 144001)
			{
				if (TimeOfYear < 72001)
				{
					if (TimeOfYear < 36001)
					{
						return "Nivvun Ut";
					}
					return "Iyur Ut";
				}
				if (TimeOfYear < 108001)
				{
					return "Simmun Ut";
				}
				return "Tuum Ut";
			}
			if (TimeOfYear < 180001)
			{
				return "Ubu Ut";
			}
			return "Uulu Ut";
		}
		if (TimeOfYear < 330001)
		{
			if (TimeOfYear < 258001)
			{
				if (TimeOfYear < 222001)
				{
					return "Ut yara Ux";
				}
				return "Tishru i Ux";
			}
			if (TimeOfYear < 294001)
			{
				return "Tishru ii Ux";
			}
			return "Kisu Ux";
		}
		if (TimeOfYear < 402001)
		{
			if (TimeOfYear < 366001)
			{
				return "Tebet Ux";
			}
			return "Shwut Ux";
		}
		if (TimeOfYear < 438001)
		{
			return "Uru Ux";
		}
		return "Uru Ux";
	}

	public static string GetMarginaliaTime(long Time)
	{
		return "On the " + GetDay(Time) + " of " + GetMonth(Time);
	}

	public static int GetDayOfYear(long Time)
	{
		return (int)(Time % 438000);
	}

	public static string GetDay()
	{
		return GetDay((int)TotalTimeTicks % 438000);
	}

	public static string GetDay(long Time)
	{
		return GetDay((int)(Time % 438000));
	}

	public static string GetDay(int TimeOfYear)
	{
		if (TimeOfYear > 216000 && TimeOfYear < 222001)
		{
			if (TimeOfYear < 217201)
			{
				return "1st";
			}
			if (TimeOfYear < 218401)
			{
				return "2nd";
			}
			if (TimeOfYear < 219601)
			{
				return "3rd";
			}
			if (TimeOfYear < 220801)
			{
				return "4th";
			}
			if (TimeOfYear < 222001)
			{
				return "5th";
			}
			return "0th";
		}
		if (TimeOfYear > 222000)
		{
			TimeOfYear -= 6000;
		}
		int num = TimeOfYear % 36000;
		if (num < 1200)
		{
			return "1st";
		}
		if (num < 2400)
		{
			return "2nd";
		}
		if (num < 3600)
		{
			return "3rd";
		}
		if (num < 4800)
		{
			return "4th";
		}
		if (num < 6000)
		{
			return "5th";
		}
		if (num < 7200)
		{
			return "6th";
		}
		if (num < 8400)
		{
			return "7th";
		}
		if (num < 9600)
		{
			return "8th";
		}
		if (num < 10800)
		{
			return "9th";
		}
		if (num < 12000)
		{
			return "10th";
		}
		if (num < 13200)
		{
			return "11th";
		}
		if (num < 14400)
		{
			return "12th";
		}
		if (num < 15600)
		{
			return "13th";
		}
		if (num < 16800)
		{
			return "14th";
		}
		if (num < 18000)
		{
			return "Ides";
		}
		if (num < 19200)
		{
			return "16th";
		}
		if (num < 20400)
		{
			return "17th";
		}
		if (num < 21600)
		{
			return "18th";
		}
		if (num < 22800)
		{
			return "19th";
		}
		if (num < 24000)
		{
			return "20th";
		}
		if (num < 25200)
		{
			return "21st";
		}
		if (num < 26400)
		{
			return "22nd";
		}
		if (num < 27600)
		{
			return "23rd";
		}
		if (num < 28800)
		{
			return "24th";
		}
		if (num < 30000)
		{
			return "25th";
		}
		if (num < 31200)
		{
			return "26th";
		}
		if (num < 32400)
		{
			return "27th";
		}
		if (num < 33600)
		{
			return "28th";
		}
		if (num < 34800)
		{
			return "29th";
		}
		if (num < 36000)
		{
			return "30th";
		}
		return "0th";
	}

	public static string GetTime()
	{
		return GetTime((int)TotalTimeTicks % 1200);
	}

	public static string GetTime(long Time)
	{
		return GetTime((int)(Time % 1200));
	}

	public static string GetTime(int TimeOfDay)
	{
		if (TimeOfDay < 451)
		{
			if (TimeOfDay < 151)
			{
				if (TimeOfDay >= 0 && TimeOfDay < 26)
				{
					return "Beetle Moon Zenith";
				}
				return "Waning Beetle Moon";
			}
			if (TimeOfDay < 301)
			{
				return "The Shallows";
			}
			return "Harvest Dawn";
		}
		if (TimeOfDay < 901)
		{
			if (TimeOfDay < 626)
			{
				if (TimeOfDay < 576)
				{
					return "Waxing Salt Sun";
				}
				return "High Salt Sun";
			}
			if (TimeOfDay < 751)
			{
				return "Waning Salt Sun";
			}
			return "Hindsun";
		}
		if (TimeOfDay < 1176)
		{
			if (TimeOfDay < 1051)
			{
				return "Jeweled Dusk";
			}
			return "Waxing Beetle Moon";
		}
		if (TimeOfDay < 1201)
		{
			return "Beetle Moon Zenith";
		}
		return "Zero Hour";
	}

	public static bool IsDay()
	{
		if (CurrentDaySegment >= 2500)
		{
			return CurrentDaySegment < 9124;
		}
		return false;
	}

	[WishCommand("advanceticks", null)]
	private static void WishAdvanceTime(string Ticks)
	{
		if (long.TryParse(Ticks, out var result))
		{
			The.Game.TimeTicks += result;
		}
	}

	[Obsolete("Use GetYear()")]
	public static int getYear()
	{
		return (int)(TotalTimeTicks / 438000) + 1001;
	}

	[Obsolete("Use GetMonth()")]
	public static string getMonth(int _dayOfYear = -1)
	{
		if (_dayOfYear > -1)
		{
			return GetMonth(_dayOfYear);
		}
		return GetMonth();
	}

	[Obsolete("Use GetDay()")]
	public static string getDay(int _dayOfYear = -1)
	{
		if (_dayOfYear > -1)
		{
			return GetDay(_dayOfYear);
		}
		return GetDay();
	}

	[Obsolete("Use GetTime()")]
	public static string getTime(string zoneID = null)
	{
		return GetTime();
	}

	[Obsolete("Use GetTime(int)")]
	public static string getTime(int minute)
	{
		return GetTime(minute);
	}
}
