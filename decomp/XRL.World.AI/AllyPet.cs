namespace XRL.World.AI;

public class AllyPet : IAllyReason
{
	public override string GetText(GameObject Actor)
	{
		return "I joined " + The.Player.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: false) + " at the start of " + The.Player.its + " adventure.";
	}
}
