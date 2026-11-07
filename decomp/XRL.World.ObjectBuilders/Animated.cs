using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class Animated : IObjectBuilder
{
	public override void Apply(GameObject Object, string Context)
	{
		AnimateObject.Animate(Object);
	}
}
