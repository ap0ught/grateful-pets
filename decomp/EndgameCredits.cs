using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

[ExecuteAlways]
public class EndgameCredits : MonoBehaviour
{
	public LayoutElement topTombPadding;

	public LayoutElement bottomTombPadding;

	public RectTransform tombText;

	public bool testRun;

	public bool running;

	public float runtime;

	public float scrollTime;

	public float scrollSize;

	public RectTransform content;

	public Action complete;

	public Image tombstoneImage;

	public UITextSkin tombstoneText;

	public Sprite[] endmarks;

	public Image ultraMark;

	public UITextSkin creditsTextSkin;

	public float FADEIN_TIME = 10f;

	public CanvasGroup contentGroup;

	private void Start()
	{
	}

	public void SetEndmark(string id, bool ultra)
	{
		tombstoneImage.sprite = endmarks.Where((Sprite e) => e.name == id).FirstOrDefault();
		ultraMark.enabled = ultra;
	}

	public void PrepSizes()
	{
		topTombPadding.preferredHeight = (transform as RectTransform).rect.height / 2f - 100f;
		topTombPadding.minHeight = topTombPadding.preferredHeight;
		bottomTombPadding.preferredHeight = (transform as RectTransform).rect.height + 100f;
		bottomTombPadding.minHeight = bottomTombPadding.minHeight;
		(tombText.transform as RectTransform).sizeDelta = new Vector2((transform as RectTransform).rect.width, (tombText.transform as RectTransform).sizeDelta.y);
		if (!running)
		{
			(transform as RectTransform).anchoredPosition = new Vector2(0f, 0f);
		}
	}

	public void Awake()
	{
		testRun = false;
		running = false;
	}

	private void LateUpdate()
	{
		scrollSize = (content.transform as RectTransform).rect.height;
		if (testRun)
		{
			PrepSizes();
			testRun = false;
			if (!running)
			{
				running = true;
				runtime = 0f - FADEIN_TIME;
				content.anchoredPosition = new Vector2(0f, 0f);
			}
			else
			{
				running = false;
				runtime = 0f;
				content.anchoredPosition = new Vector2(0f, 0f);
			}
		}
	}

	public void Update()
	{
		if (running)
		{
			Run();
		}
	}

	public void Run()
	{
		runtime += Time.deltaTime;
		if (runtime < 0f)
		{
			contentGroup.alpha = (runtime + FADEIN_TIME) / (FADEIN_TIME / 2f);
		}
		else
		{
			contentGroup.alpha = 1f;
			content.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, scrollSize, runtime / scrollTime));
		}
		if (runtime > scrollTime - FADEIN_TIME)
		{
			contentGroup.alpha = 1f - (runtime - (scrollTime - FADEIN_TIME)) / FADEIN_TIME;
		}
		if (runtime > scrollTime)
		{
			running = false;
			if (complete != null)
			{
				Action action = complete;
				complete = null;
				action();
			}
		}
	}
}
