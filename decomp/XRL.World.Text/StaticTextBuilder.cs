namespace XRL.World.Text;

public class StaticTextBuilder : CompositedTextBuilder
{
	protected override bool Finalizable => true;

	public StaticTextBuilder(int Capacity = 64)
		: base(Capacity)
	{
	}
}
