using EnhancedUI.EnhancedScroller;
using Qud.UI;
using XRL.UI;

public class ModCellView : EnhancedScrollerCellView
{
	public UITextSkin modPath;

	public ModScrollerData data;

	public void SetData(ModScrollerData data)
	{
		this.data = data;
		modPath.SetText(data.info.ID);
	}

	public void OnSelected()
	{
		SingletonWindowBase<SteamWorkshopUploaderView>.instance.SetModInfo(data.info);
	}
}
