namespace XRL.World.ZoneBuilders;

public class Music
{
	public string Track = "";

	public string Chance = "100";

	public bool BuildZone(Zone Z)
	{
		Z.SetMusic(Track);
		return true;
	}
}
