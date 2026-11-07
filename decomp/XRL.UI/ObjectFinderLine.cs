using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.UI;

public class ObjectFinderLine : MonoBehaviour, IFrameworkControl, IPointerExitHandler, IEventSystemHandler
{
	public class Data : FrameworkDataElement
	{
		public string RightText;

		public string PrefixText;

		public Renderable Icon;

		public XRL.World.GameObject go;

		public ObjectFinder.Context context;
	}

	public UIThreeColorProperties ObjectIcon;

	public UITextSkin PrefixText;

	public UITextSkin ObjectDescription;

	public UITextSkin RightText;

	public FrameworkContext navContextHolder;

	public Data lastData;

	public NavigationContext GetNavigationContext()
	{
		return navContextHolder.context;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (GetNavigationContext().IsActive() && !SingletonWindowBase<NearbyItemsWindow>.instance.KeyboardMode)
		{
			NavigationController.instance.activeContext = null;
		}
	}

	public void setData(FrameworkDataElement data)
	{
		lastData = null;
		if (data is Data data2)
		{
			lastData = data2;
			if (string.IsNullOrWhiteSpace(data2.RightText))
			{
				RightText.gameObject.SetActive(value: false);
			}
			else
			{
				RightText.gameObject.SetActive(value: true);
				RightText.SetText(data2.RightText);
			}
			if (string.IsNullOrWhiteSpace(data2.PrefixText))
			{
				PrefixText.gameObject.SetActive(value: false);
			}
			else
			{
				PrefixText.gameObject.SetActive(value: true);
				PrefixText.SetText(data2.PrefixText);
			}
			ObjectDescription.SetText(data2.Description);
			if (data2.Icon == null)
			{
				ObjectIcon.gameObject.SetActive(value: false);
				return;
			}
			ObjectIcon.gameObject.SetActive(value: true);
			ObjectIcon.FromRenderable(data2.Icon);
		}
	}
}
