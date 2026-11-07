using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class KeybindBox : MonoBehaviour
{
	public Image backgroundImage;

	public Image fullBorderImage;

	public UITextSkin textSkin;

	public FrameworkContext context;

	public bool editMode;

	public bool selectedMode;

	public bool forceUpdate;

	public string boxText = "{{c|None}}";

	private bool? lastEditMode;

	private bool? wasSelected;

	private void Start()
	{
		forceUpdate = true;
	}

	public void Apply()
	{
		forceUpdate = true;
		Update();
	}

	public void Update()
	{
		bool? flag = context.context?.IsActive();
		if (wasSelected == flag && lastEditMode == editMode && !forceUpdate)
		{
			return;
		}
		lastEditMode = editMode;
		wasSelected = flag;
		forceUpdate = false;
		textSkin.text = boxText;
		selectedMode = flag == true;
		int num;
		Color color;
		if (!selectedMode)
		{
			num = (editMode ? 1 : 0);
			if (num == 0)
			{
				color = new Color(fullBorderImage.color.r, fullBorderImage.color.g, fullBorderImage.color.b, 0f);
				goto IL_0146;
			}
		}
		else
		{
			num = 1;
		}
		color = new Color(fullBorderImage.color.r, fullBorderImage.color.g, fullBorderImage.color.b, 1f);
		goto IL_0146;
		IL_0146:
		Color color2 = color;
		if (fullBorderImage.color != color2)
		{
			fullBorderImage.color = color2;
		}
		color2 = ((num != 0) ? new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f) : new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0f));
		if (backgroundImage.color != color2)
		{
			backgroundImage.color = color2;
		}
		if (editMode)
		{
			if (CapabilityManager.CurrentPlatformClassification() == CapabilityManager.PlatformClassification.Console)
			{
				textSkin.text = "{{R|...}}";
			}
			else
			{
				textSkin.text = "{{R|press key...}}";
			}
		}
		textSkin.Apply();
	}
}
