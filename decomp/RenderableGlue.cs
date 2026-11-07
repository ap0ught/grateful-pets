using ConsoleLib.Console;
using ModelShark;
using UnityEngine;

public class RenderableGlue : MonoBehaviour, Tooltip.SetupHelper
{
	public IRenderableDisplayManager image;

	public void BeforeShow(TooltipTrigger trigger, Tooltip tooltip)
	{
		if (trigger.AdditionalData is DualImageRenderableGlue.DualImageRenderableGlueData dualImageRenderableGlueData)
		{
			image.FromRenderable(dualImageRenderableGlueData.imageOneRenderable);
		}
		else
		{
			image.FromRenderable(trigger.AdditionalData as IRenderable);
		}
	}
}
