using System;
using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI;

public class FadeText : MonoBehaviour
{
	public Image Skin;

	public float FadeIn;

	public float Hold;

	public float FadeOut;

	public bool EndDestroy;

	public Action After;

	private int Stage;

	private float Elapsed;

	public void Reset()
	{
		Stage = 0;
		Elapsed = 0f;
		Skin.color = Skin.color.WithAlpha(0f);
	}

	public void Awake()
	{
		Stage = 0;
		Elapsed = 0f;
		Skin.color = Skin.color.WithAlpha(0f);
	}

	public void SetStage(int Stage)
	{
		this.Stage = Stage;
		Elapsed = 0f;
	}

	public void Update()
	{
		if (Stage != 2 && Stage != 3)
		{
			Elapsed += Time.deltaTime;
		}
		if (Stage == 0)
		{
			float alpha = Mathf.Clamp01(Elapsed / FadeIn);
			Skin.color = Skin.color.WithAlpha(alpha);
			if (Elapsed >= FadeIn)
			{
				SetStage(1);
			}
		}
		else if (Stage == 1)
		{
			Skin.color = Skin.color.WithAlpha(1f);
			if (Elapsed >= Hold)
			{
				SetStage(2);
			}
		}
		else if (Stage == 2)
		{
			Skin.color = Skin.color.WithAlpha(1f);
			TutorialManager.ShowIntermissionPopupAsync("<nohighlight>", () =>
			{
				Stage = 4;
			}, 0, 0f, "s");
			Stage = 3;
		}
		else
		{
			if (Stage != 4)
			{
				return;
			}
			if (After != null)
			{
				Action after = After;
				After = null;
				after();
			}
			float alpha2 = Mathf.Clamp01(1f - Elapsed / FadeIn);
			Skin.color = Skin.color.WithAlpha(alpha2);
			if (Elapsed >= FadeOut)
			{
				SetStage(0);
				gameObject.SetActive(value: false);
				if (EndDestroy)
				{
					gameObject.Destroy();
				}
			}
		}
	}
}
