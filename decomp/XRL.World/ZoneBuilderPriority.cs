namespace XRL.World;

public static class ZoneBuilderPriority
{
	public const int OVERRIDE = -1000;

	public const int EXTREMELY_EARLY = 1000;

	public const int VERY_EARLY = 2000;

	public const int EARLY = 3000;

	public const int NORMAL = 4000;

	public const int MID = 4500;

	public const int LATE = 5000;

	public const int VERY_LATE = 6000;

	public const int AFTER_TERRAIN = 6000;

	public const int EXTREMELY_LATE = 7000;

	public const int ADJUST_SMALL = 1;

	public const int ADJUST_MEDIUM = 10;

	public const int ADJUST_LARGE = 100;
}
