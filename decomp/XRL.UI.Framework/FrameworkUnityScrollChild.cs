namespace XRL.UI.Framework;

public class FrameworkUnityScrollChild : FrameworkContext
{
	private IFrameworkControl _FrameworkControl;

	public IFrameworkControl FrameworkControl => _FrameworkControl ?? (_FrameworkControl = GetComponent<IFrameworkControl>());

	public virtual ScrollChildContext scrollContext
	{
		get
		{
			return (ScrollChildContext)context;
		}
		set
		{
			context = value;
		}
	}

	public void Setup(FrameworkDataElement data, ScrollChildContext scrollContext)
	{
		context = scrollContext;
		if (FrameworkControl == null)
		{
			MetricsManager.LogError("The control element " + name + " doesn't have an IFrameworkControl component. It needs one to recieve the framework data.");
			return;
		}
		FrameworkControl.setData(data);
		if (FrameworkControl is IFrameworkControlSubcontexts frameworkControlSubcontexts)
		{
			frameworkControlSubcontexts.SetupContexts(scrollContext);
		}
		context.Setup();
	}
}
