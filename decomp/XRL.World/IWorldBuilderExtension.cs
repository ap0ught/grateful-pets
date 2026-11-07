namespace XRL.World;

public class IWorldBuilderExtension
{
	public virtual void OnBeforeBuild(string world, object builder)
	{
	}

	public virtual void OnAfterBuild(string world, object builder)
	{
	}
}
