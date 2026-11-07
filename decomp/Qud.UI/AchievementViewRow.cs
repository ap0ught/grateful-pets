using System;
using Kobold;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class AchievementViewRow : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public ImageTinyFrame Icon;

	public UITextSkin Name;

	public UITextSkin Description;

	public UITextSkin Date;

	public Image Background;

	public ProgressBar Progress;

	private FrameworkContext _FrameworkContext;

	private bool? WasActive;

	public FrameworkContext FrameworkContext => _FrameworkContext ?? (_FrameworkContext = GetComponent<FrameworkContext>());

	public NavigationContext NavigationContext
	{
		get
		{
			if (!FrameworkContext)
			{
				return null;
			}
			return _FrameworkContext.context;
		}
	}

	public void SetupContexts(ScrollChildContext context)
	{
		FrameworkContext.context = context;
	}

	public void setData(FrameworkDataElement Data)
	{
		if (Data is AchievementInfoData achievementData)
		{
			SetAchievementData(achievementData);
		}
		else if (Data is HiddenAchievementData hiddenData)
		{
			SetHiddenData(hiddenData);
		}
		Icon.Sync(force: true);
		WasActive = null;
		Update();
	}

	public void SetAchievementData(AchievementInfoData Data)
	{
		AchievementInfo achievement = Data.Achievement;
		Name.SetText(achievement.Name);
		Description.SetText(achievement.Description);
		if (achievement.Achieved)
		{
			SetLocked(Value: false);
			Progress.Hide();
			Icon.sprite = SpriteManager.GetUnitySprite(achievement.IconUnlocked);
			if (achievement.TimeStamp != DateTime.MinValue)
			{
				Date.SetText("Unlocked " + achievement.TimeStamp.ToString("f"));
			}
			else
			{
				Date.SetText("");
			}
		}
		else
		{
			SetLocked(Value: true);
			Progress.Set(achievement);
			Icon.sprite = SpriteManager.GetUnitySprite(achievement.IconLocked);
			Date.SetText("");
		}
	}

	public void SetHiddenData(HiddenAchievementData Data)
	{
		int amount = Data.Amount;
		SetLocked(Value: true);
		Progress.Hide();
		Icon.sprite = SpriteManager.GetUnitySprite("UI/Achievements/hidden.png");
		Name.SetText(amount + " hidden achievements remaining");
		Description.SetText("Details will be revealed once unlocked.");
		Date.SetText("");
	}

	public void SetLocked(bool Value)
	{
		if (Value)
		{
			Name.color = The.Color.DarkCyan;
			Description.color = The.Color.Gray;
			Date.color = The.Color.Black;
			Icon.borderColor = The.Color.Black;
			Background.color = Background.color.WithAlpha(0f);
		}
		else
		{
			Name.color = The.Color.Yellow;
			Description.color = The.Color.Gray;
			Date.color = The.Color.Black;
			Icon.borderColor = The.Color.Yellow;
			Background.color = Background.color.WithAlpha(0.25f);
		}
	}

	public void Update()
	{
		bool flag = NavigationContext?.IsActive() ?? false;
		if (flag != WasActive)
		{
			WasActive = flag;
			Background.color = Background.color.WithAlpha(flag ? 0.25f : 0f);
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return null;
	}
}
