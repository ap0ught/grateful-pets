using XRL.World.Parts;

namespace XRL.World.AI;

public class AllyPilot : IAllyReason
{
	public override ReplaceTarget Replace => ReplaceTarget.Type;

	public override string GetText(GameObject Actor)
	{
		if (Actor.TryGetPart<Vehicle>(out var Part) && !Part.PilotID.IsNullOrEmpty())
		{
			return "I am being piloted by " + Part.Pilot.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: false, Short: true, BaseOnly: true, IndicateHidden: false, SecondPerson: false) + ".";
		}
		return "I am being piloted.";
	}
}
