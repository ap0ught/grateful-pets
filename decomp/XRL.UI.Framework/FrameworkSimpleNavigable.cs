namespace XRL.UI.Framework;

public class FrameworkSimpleNavigable : FrameworkContext
{
	public NavigationContext _context = new NavigationContext();

	public override NavigationContext context
	{
		get
		{
			return _context;
		}
		set
		{
			_context = value;
		}
	}
}
