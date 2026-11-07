using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AiUnity.Common.LogUI.Scripts;

public class TogglePanelButton : MonoBehaviour
{
	public bool panelOpen;

	public GameObject MenuPanel;

	private Sprite normalSprite;

	public Sprite pressedSprite;

	private LayoutElement MenuPanelLayoutElement;

	private float openHeight;

	private Image buttonImage;

	public Button Button { get; set; }

	public void TogglePanel()
	{
		TogglePanel(!panelOpen);
	}

	public void TogglePanel(bool panelOpen, bool animate = true)
	{
		this.panelOpen = panelOpen;
		buttonImage.overrideSprite = (this.panelOpen ? pressedSprite : null);
		StartCoroutine(TogglePanelAnimate(animate));
	}

	public IEnumerator TogglePanelAnimate(bool animate = true)
	{
		float targetHeight = (panelOpen ? openHeight : 0f);
		MenuPanel.SetActive(value: true);
		yield return StartCoroutine(AnimateGameConsole(targetHeight, animate));
		MenuPanel.SetActive(panelOpen);
	}

	private IEnumerator AnimateGameConsole(float targetHeight, bool animate)
	{
		float timeToLerp = 1f;
		float timeLerped = (animate ? 0f : timeToLerp);
		float startHeight = MenuPanelLayoutElement.preferredHeight;
		while (timeLerped <= timeToLerp)
		{
			timeLerped += Time.deltaTime;
			MenuPanelLayoutElement.preferredHeight = Mathf.Lerp(startHeight, targetHeight, timeLerped / timeToLerp);
			if (animate)
			{
				yield return null;
			}
		}
	}

	private void Start()
	{
		MenuPanelLayoutElement = MenuPanel.GetComponent<LayoutElement>();
		openHeight = MenuPanelLayoutElement.preferredHeight;
		Button = GetComponent<Button>();
		Button.onClick.AddListener(TogglePanel);
		buttonImage = GetComponent<Image>();
		panelOpen = true;
		TogglePanel(panelOpen, animate: false);
	}
}
