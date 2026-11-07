using Steamworks;

public class LeaderboardScores
{
	public enum Result
	{
		Success,
		Failure,
		Cancelled
	}

	private LeaderboardScoresDownloaded_t m_result;

	public LeaderboardScores(LeaderboardScoresDownloaded_t result)
	{
		m_result = result;
	}

	public Result GetResult()
	{
		return Result.Success;
	}

	public int GetEntryCount()
	{
		return m_result.m_cEntryCount;
	}

	public string GetLeaderboardID()
	{
		return m_result.m_hSteamLeaderboard.ToString();
	}

	public LeaderboardEntry GetLeaderboardEntry(int index)
	{
		if (index < 0 || index >= m_result.m_cEntryCount)
		{
			return null;
		}
		LeaderboardEntry_t pLeaderboardEntry = default;
		if (!SteamUserStats.GetDownloadedLeaderboardEntry(m_result.m_hSteamLeaderboardEntries, index, out pLeaderboardEntry, null, 0))
		{
			return null;
		}
		SteamFriends.RequestUserInformation(pLeaderboardEntry.m_steamIDUser, bRequireNameOnly: true);
		return new LeaderboardEntry
		{
			id = pLeaderboardEntry.m_steamIDUser.m_SteamID.ToString(),
			displayName = SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser),
			score = pLeaderboardEntry.m_nScore,
			rank = pLeaderboardEntry.m_nGlobalRank,
			isCurrentUser = (pLeaderboardEntry.m_steamIDUser == SteamUser.GetSteamID())
		};
	}

	public static LeaderboardScores CreateEmptyLeaderboardScores()
	{
		return new LeaderboardScores(default);
	}

	public static LeaderboardScores CreateCancelledLeaderboardScores()
	{
		return null;
	}
}
