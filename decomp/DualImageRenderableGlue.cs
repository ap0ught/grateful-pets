using ConsoleLib.Console;
using ModelShark;
using UnityEngine;
using XRL.UI;

public class DualImageRenderableGlue : MonoBehaviour, Tooltip.SetupHelper
{
	public struct DualImageRenderableGlueData(IRenderable one, IRenderable two)
	{
		public IRenderable imageOneRenderable = one;

		public IRenderable imageTwoRenderable = two;
	}

	public IRenderableDisplayManager image;

	public IRenderableDisplayManager image2;

	public void BeforeShow(TooltipTrigger trigger, Tooltip tooltip)
	{
		if (trigger.AdditionalData is DualImageRenderableGlueData dualImageRenderableGlueData)
		{
			image.FromRenderable(dualImageRenderableGlueData.imageOneRenderable);
			image2.FromRenderable(dualImageRenderableGlueData.imageTwoRenderable);
		}
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			transform.localScale = new Vector3(0.75f, 0.75f, 1f);
		}
		else
		{
			transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}
}
