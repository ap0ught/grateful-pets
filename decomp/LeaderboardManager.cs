using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using XRL.Core;

public class LeaderboardManager : MonoBehaviour
{
	private class LeaderboardRecordRequest
	{
		public string type;

		public string id;

		public int requestTop;

		public int numRecords;

		public int score;

		public bool friendsonly;

		public Action<LeaderboardScores> scoreCallback;
	}

	private const ELeaderboardUploadScoreMethod s_leaderboardMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

	private static CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();

	private static CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();

	private static CallResult<LeaderboardScoresDownloaded_t> m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();

	private static Callback<PersonaStateChange_t> m_personaStateChange;

	private static SteamLeaderboard_t currentLeaderboard;

	private static Dictionary<string, SteamLeaderboard_t> leaderboardID = new Dictionary<string, SteamLeaderboard_t>();

	private static Dictionary<ulong, TaskCompletionSource<string>> nameCache = new Dictionary<ulong, TaskCompletionSource<string>>();

	private static Queue<LeaderboardRecordRequest> requests = new Queue<LeaderboardRecordRequest>();

	private static LeaderboardRecordRequest currentRequest = null;

	private static Action<LeaderboardScores> leaderboardEntriesCallback = null;

	public static Dictionary<string, string> leaderboardresults = new Dictionary<string, string>();

	private static void CompletePendingRequest(LeaderboardScores scores)
	{
		Action<LeaderboardScores> action = leaderboardEntriesCallback;
		leaderboardEntriesCallback = null;
		currentRequest = null;
		if (action != null)
		{
			try
			{
				action(scores);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private static void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
	{
		try
		{
			if (failure)
			{
				Debug.LogError("STEAM LEADERBOARDS: find request failed");
				CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
				return;
			}
			Debug.Log("STEAM LEADERBOARDS: Found - " + pCallback.m_bLeaderboardFound + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);
			currentLeaderboard = pCallback.m_hSteamLeaderboard;
			if (pCallback.m_bLeaderboardFound != 0)
			{
				if (leaderboardID.ContainsKey(currentRequest.id))
				{
					leaderboardID[currentRequest.id] = pCallback.m_hSteamLeaderboard;
				}
				else
				{
					leaderboardID.Add(currentRequest.id, pCallback.m_hSteamLeaderboard);
				}
				if (currentRequest.type == "getleaderboard")
				{
					ELeaderboardDataRequest eLeaderboardDataRequest = (currentRequest.friendsonly ? ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends : ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal);
					int num = Math.Max(1, currentRequest.requestTop);
					int nRangeEnd = num + currentRequest.numRecords;
					SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, eLeaderboardDataRequest, num, nRangeEnd);
					m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();
					m_downloadResult.Set(hAPICall, OnDownloadLeaderboardEntries);
				}
				else if (currentRequest.type == "submitleaderboardscore")
				{
					SteamAPICall_t hAPICall2 = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, currentRequest.score, null, 0);
					m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();
					m_uploadResult.Set(hAPICall2, OnLeaderboardUploadResult);
				}
				else if (currentRequest.type == "getleaderboardrank")
				{
					SteamAPICall_t hAPICall3 = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -4, 5);
					m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();
					m_downloadResult.Set(hAPICall3, OnDownloadLeaderboardEntries);
				}
				else
				{
					CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
				}
			}
			else
			{
				Debug.LogError("Couldn't find leaderboard");
				CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
			}
		}
		catch (Exception ex)
		{
			XRLCore.LogError("OnLeaderboardFindResult", ex);
			CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
		}
	}

	private static void OnDownloadLeaderboardEntries(LeaderboardScoresDownloaded_t pCallback, bool failure)
	{
		if (failure)
		{
			Debug.LogError("STEAM LEADERBOARDS: download request failed");
			CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
			return;
		}
		try
		{
			DownloadUserData(pCallback);
			CompletePendingRequest(new LeaderboardScores(pCallback));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
		}
	}

	private static void DownloadUserData(LeaderboardScoresDownloaded_t pCallback)
	{
		if (m_personaStateChange == null)
		{
			m_personaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
		}
		for (int i = 0; i < pCallback.m_cEntryCount; i++)
		{
			if (SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, null, 0) && !nameCache.TryGetValue(pLeaderboardEntry.m_steamIDUser.m_SteamID, out var value))
			{
				value = new TaskCompletionSource<string>();
				nameCache.Add(pLeaderboardEntry.m_steamIDUser.m_SteamID, value);
				if (!SteamFriends.RequestUserInformation(pLeaderboardEntry.m_steamIDUser, bRequireNameOnly: true))
				{
					value.TrySetResult(SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser));
				}
			}
		}
	}

	private static void OnPersonaStateChange(PersonaStateChange_t pCallback)
	{
		if (nameCache.TryGetValue(pCallback.m_ulSteamID, out var value) && !value.Task.IsCompleted)
		{
			CSteamID steamIDFriend = new CSteamID(pCallback.m_ulSteamID);
			value.TrySetResult(SteamFriends.GetFriendPersonaName(steamIDFriend));
		}
	}

	private static void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
	{
		try
		{
			if (failure)
			{
				Debug.LogError("STEAM LEADERBOARDS: upload request failed");
				CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
				return;
			}
			Debug.Log("STEAM LEADERBOARDS: Completed - " + pCallback.m_bSuccess + " NewScore: " + pCallback.m_nGlobalRankNew + " Score " + pCallback.m_nScore + " HasChanged - " + pCallback.m_bScoreChanged);
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -4, 5);
			m_downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();
			m_downloadResult.Set(hAPICall, OnDownloadLeaderboardEntries);
		}
		catch (Exception ex)
		{
			XRLCore.LogError("OnLeaderboardUploadResult", ex);
			CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
		}
	}

	public void Update()
	{
		if (requests.Count > 0 && currentRequest == null)
		{
			lock (requests)
			{
				currentRequest = requests.Dequeue();
				leaderboardEntriesCallback = currentRequest.scoreCallback;
				FindOrCreateLeaderboard(currentRequest.id);
			}
		}
	}

	public static void FindOrCreateLeaderboard(string id)
	{
		try
		{
			string[] array = id.Split(':');
			string pchLeaderboardName = "unknown " + id;
			if (array.Length >= 3 && array[0] == "weekly")
			{
				pchLeaderboardName = array[0] + " for week " + array[2] + " of " + array[1];
			}
			else if (array.Length >= 3 && array[0] == "daily")
			{
				pchLeaderboardName = array[0] + " for day " + array[2] + " of " + array[1];
			}
			SteamAPICall_t hAPICall = SteamUserStats.FindOrCreateLeaderboard(pchLeaderboardName, ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
			m_findResult = new CallResult<LeaderboardFindResult_t>();
			m_findResult.Set(hAPICall, OnLeaderboardFindResult);
		}
		catch (Exception ex)
		{
			XRLCore.LogError("FindOrCreateLeaderboard", ex);
			CompletePendingRequest(LeaderboardScores.CreateCancelledLeaderboardScores());
		}
	}

	public static Task<PlatformManager.TryGetNetworkResult> TryGoOnline()
	{
		return TryGoOnlineInternal();
	}

	private static Task<PlatformManager.TryGetNetworkResult> TryGoOnlineInternal()
	{
		return Task.FromResult((!PlatformManager.Steam.Initialized) ? PlatformManager.TryGetNetworkResult.Failure : PlatformManager.TryGetNetworkResult.Success);
	}

	public static void TryGoOnline(Action<bool> onComplete)
	{
		onComplete?.Invoke(obj: false);
	}

	public static string SubmitResult(string id, int score, Action<LeaderboardScores> callback)
	{
		string[] array = id.Split(':');
		string leaderboardID = "unknown " + id;
		if (array[0] == "weekly")
		{
			leaderboardID = array[0] + " for week " + array[2] + " of " + array[1];
		}
		if (array[0] == "daily")
		{
			leaderboardID = array[0] + " for day " + array[2] + " of " + array[1];
		}
		PlatformManager.TryGetLeaderboardsReady((bool isReady) =>
		{
			if (!isReady)
			{
				Debug.Log("Can't upload to the leaderboard because Steam isn't connected.");
				callback?.Invoke(null);
				return;
			}
			lock (requests)
			{
				Debug.Log("uploading score(" + score + ") to steam leaderboard(" + leaderboardID + ")");
				LeaderboardRecordRequest item = new LeaderboardRecordRequest
				{
					type = "submitleaderboardscore",
					id = id,
					score = score,
					scoreCallback = callback
				};
				requests.Enqueue(item);
			}
		});
		return leaderboardID;
	}

	public static string GetDailyID(int daysAgo)
	{
		CultureInfo cultureInfo = new CultureInfo("en-US");
		DateTime dateTime = DateTime.Now - new TimeSpan(daysAgo, 0, 0, 0, 0);
		if (dateTime > DateTime.Now)
		{
			dateTime = DateTime.Now;
		}
		if (dateTime < new DateTime(2016, 11, 1))
		{
			dateTime = new DateTime(2016, 11, 1);
		}
		int dayOfYear = cultureInfo.Calendar.GetDayOfYear(dateTime);
		return "daily:" + dateTime.Year + ":" + dayOfYear;
	}

	public static string GetLeaderboardName(string id)
	{
		string[] array = id.Split(':');
		string result = "unknown " + id;
		if (array[0] == "weekly")
		{
			result = array[0] + " for week " + array[2] + " of " + array[1];
		}
		if (array[0] == "daily")
		{
			result = array[0] + " for day " + array[2] + " of " + array[1];
		}
		return result;
	}

	public static async Task<LeaderboardScores> GetLeaderboardAsync(string id, int requestTop, int numRecords, bool friendsOnly, CancellationToken? ct = null)
	{
		switch (await PlatformManager.TryGetLeaderboardsReady())
		{
		case PlatformManager.TryGetNetworkResult.Success:
		{
			ct?.ThrowIfCancellationRequested();
			LeaderboardScores result = await GetLeaderboardAsyncInternal(id, requestTop, numRecords, friendsOnly);
			ct?.ThrowIfCancellationRequested();
			return result;
		}
		case PlatformManager.TryGetNetworkResult.Cancelled:
			return LeaderboardScores.CreateCancelledLeaderboardScores();
		default:
			return null;
		}
	}

	private static Task<LeaderboardScores> GetLeaderboardAsyncInternal(string id, int requestTop, int numRecords, bool friendsOnly)
	{
		TaskCompletionSource<LeaderboardScores> result = new TaskCompletionSource<LeaderboardScores>();
		if (PlatformManager.AreLeaderboardsOffline())
		{
			result.TrySetException(new Exception("Not connected to Steam API"));
		}
		else
		{
			GetLeaderboard(id, requestTop, numRecords, friendsOnly, (LeaderboardScores r) =>
			{
				result.TrySetResult(r);
			});
		}
		return result.Task;
	}

	public static async Task<string> FriendNameAsync(LeaderboardEntry entry)
	{
		return await FriendNameAsyncInternal(entry);
	}

	public static Task<string> FriendNameAsyncInternal(LeaderboardEntry entry)
	{
		if (!string.IsNullOrEmpty(entry.displayName))
		{
			return Task.FromResult(entry.displayName);
		}
		if (ulong.TryParse(entry.id, out var result) && nameCache.TryGetValue(result, out var value))
		{
			return value.Task;
		}
		return Task.FromResult(entry.displayName);
	}

	public static string GetLeaderboard(string id, int requestTop, int numRecords, bool bFriendsOnly, Action<LeaderboardScores> callback)
	{
		Task<PlatformManager.TryGetNetworkResult> task = PlatformManager.TryGetLeaderboardsReady();
		task.Wait();
		switch (task.Result)
		{
		case PlatformManager.TryGetNetworkResult.Success:
			lock (requests)
			{
				Debug.Log("getting steam leaderboard(" + id + ")");
				LeaderboardRecordRequest leaderboardRecordRequest = new LeaderboardRecordRequest();
				leaderboardRecordRequest.type = "getleaderboard";
				leaderboardRecordRequest.id = id;
				leaderboardRecordRequest.scoreCallback = callback;
				leaderboardRecordRequest.requestTop = requestTop;
				leaderboardRecordRequest.numRecords = numRecords;
				leaderboardRecordRequest.friendsonly = bFriendsOnly;
				requests.Enqueue(leaderboardRecordRequest);
				return GetLeaderboardName(id);
			}
		case PlatformManager.TryGetNetworkResult.Cancelled:
			callback?.Invoke(LeaderboardScores.CreateCancelledLeaderboardScores());
			return GetLeaderboardName(id);
		default:
			Debug.Log("Can't upload to the leaderboard because Steam isn't connected.");
			callback?.Invoke(LeaderboardScores.CreateEmptyLeaderboardScores());
			return GetLeaderboardName(id);
		}
	}

	public static async Task<string> GetLeaderboardRank(string leaderboardID, Action<LeaderboardScores> callback)
	{
		switch (await PlatformManager.TryGetLeaderboardsReady())
		{
		case PlatformManager.TryGetNetworkResult.Success:
			lock (requests)
			{
				LeaderboardRecordRequest leaderboardRecordRequest = new LeaderboardRecordRequest();
				leaderboardRecordRequest.type = "getleaderboardrank";
				leaderboardRecordRequest.id = leaderboardID;
				leaderboardRecordRequest.scoreCallback = callback;
				requests.Enqueue(leaderboardRecordRequest);
				return leaderboardID;
			}
		case PlatformManager.TryGetNetworkResult.Cancelled:
			callback?.Invoke(LeaderboardScores.CreateCancelledLeaderboardScores());
			return GetLeaderboardName(leaderboardID);
		default:
			Debug.Log("Can't upload to the leaderboard because Steam isn't connected.");
			callback?.Invoke(LeaderboardScores.CreateEmptyLeaderboardScores());
			return GetLeaderboardName(leaderboardID);
		}
	}
}
