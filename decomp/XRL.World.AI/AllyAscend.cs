namespace XRL.World.AI;

public class AllyAscend : IAllyReason
{
	public override string GetText(GameObject Actor)
	{
		return "I am to ascend the spindle with " + The.Player.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true, IndicateHidden: false, SecondPerson: false) + ".";
	}
}
