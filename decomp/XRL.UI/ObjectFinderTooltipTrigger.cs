using ModelShark;

namespace XRL.UI;

public class ObjectFinderTooltipTrigger : TooltipTrigger
{
	public ObjectFinderLine line;

	public override void StartHover()
	{
		SetupAndGo();
	}

	private async void SetupAndGo()
	{
		await Look.SetupItemTooltipAsync(line.lastData.go, this);
		base.StartHover();
	}
}
