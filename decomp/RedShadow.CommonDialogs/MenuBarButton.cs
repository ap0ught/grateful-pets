using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedShadow.CommonDialogs;

public class MenuBarButton : MonoBehaviour
{
	public string Text { get; private set; }

	public Menu Menu { get; internal set; }

	public MenuBarButton setText(string text)
	{
		Text = text;
		transform.Find("Text").GetComponent<TextMeshProUGUI>().text = text;
		return this;
	}

	public void onMouseEnter()
	{
		DialogBase topmost = DialogBase.getTopmost();
		if (topmost is Menu)
		{
			((Menu)topmost).cancel();
			onClick();
		}
	}

	public void Start()
	{
		GetComponent<Button>().onClick.AddListener(onClick);
	}

	private void onClick()
	{
		Menu.gameObject.SetActive(value: true);
		Menu.show(GetComponent<RectTransform>());
	}
}
