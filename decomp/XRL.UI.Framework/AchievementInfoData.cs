namespace XRL.UI.Framework;

public class AchievementInfoData : FrameworkDataElement
{
	public AchievementInfo Achievement;

	public AchievementInfoData(AchievementInfo Achievement)
	{
		this.Achievement = Achievement;
		Id = Achievement.ID;
		Description = Achievement.Name;
	}
}
