using XRL.Language;

namespace XRL.World.AI;

public class AllyDefault : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I was compelled to join " + Grammar.MakePossessive(Name) + " cause.";
	}
}
