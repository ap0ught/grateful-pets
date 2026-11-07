public class LeaderboardEntry
{
	public string id;

	public string displayName;

	public int score;

	public int rank;

	public bool isCurrentUser;

	public static LeaderboardEntry GenerateEmpty()
	{
		return new LeaderboardEntry
		{
			displayName = string.Empty,
			id = string.Empty,
			rank = 0,
			score = 0,
			isCurrentUser = false
		};
	}
}
