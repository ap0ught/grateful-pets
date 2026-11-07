using XRL.Language;

namespace XRL.World.AI;

public class AllyPack : IAllyReasonSourced
{
	public override string GetText(GameObject Actor)
	{
		return "I am part of " + Grammar.MakePossessive(Name) + " pack.";
	}
}
