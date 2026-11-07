namespace XRL.World;

public struct OrderedBuilderBlueprint(ZoneBuilderBlueprint Blueprint, int Priority)
{
	public ZoneBuilderBlueprint Blueprint = Blueprint;

	public int Priority = Priority;
}
