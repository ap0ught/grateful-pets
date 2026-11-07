using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class BookLine : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public class Context : NavigationContext
	{
		public BookLineData data;
	}

	private Context context = new Context();

	public UITextSkin text;

	public StringBuilder SB = new StringBuilder();

	private List<AbilityManagerSpacer> spacers = new List<AbilityManagerSpacer>();

	public bool selected => context?.IsActive() ?? false;

	public NavigationContext GetNavigationContext()
	{
		return context;
	}

	public void SetupContexts(ScrollChildContext scrollContext)
	{
		scrollContext.proxyTo = context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is BookLineData bookLineData)
		{
			text.SetText(bookLineData.text);
		}
	}

	public int HighlightClosestSpacer(Vector2 screenPosition)
	{
		AbilityManagerSpacer abilityManagerSpacer = ((spacers.Count > 0) ? spacers[0] : null);
		float num = float.MaxValue;
		int result = -1;
		Vector3[] array = new Vector3[4];
		int num2 = 0;
		foreach (AbilityManagerSpacer spacer in spacers)
		{
			(spacer.transform as RectTransform).GetWorldCorners(array);
			float magnitude = (new Vector2((array[0].x + array[2].x) / 2f, (array[0].y + array[2].y) / 2f) - screenPosition).magnitude;
			if (magnitude < num)
			{
				abilityManagerSpacer = spacer;
				num = magnitude;
				result = num2;
			}
			spacer.image.enabled = false;
			num2++;
		}
		if (abilityManagerSpacer != null)
		{
			abilityManagerSpacer.image.enabled = true;
		}
		return result;
	}
}
