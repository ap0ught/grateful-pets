using QupKit;

namespace Qud.UI;

public class LegacyViewWindow<T> : WindowBase where T : BaseView, new()
{
	protected T baseView;

	protected BaseView Previous;

	private bool AlreadyInit;

	public override void Init()
	{
		if (!AlreadyInit)
		{
			AlreadyInit = true;
			baseView = new T();
			baseView.AttachTo(gameObject);
			baseView.OnCreate();
		}
	}

	public override void Show()
	{
		baseView.Enter();
		base.Show();
	}

	public override void Hide()
	{
		if (Previous != null)
		{
			LegacyViewManager.Instance.ActiveView = Previous;
			Previous.Enter();
		}
		baseView.Leave();
		base.Hide();
	}
}
