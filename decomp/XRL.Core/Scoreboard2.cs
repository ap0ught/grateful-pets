using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Platform.IO;

namespace XRL.Core;

public class Scoreboard2
{
	private SortScoreEntry2 sorter = new SortScoreEntry2();

	public List<ScoreEntry2> Scores = new List<ScoreEntry2>();

	public void Sort()
	{
		Scores.Sort(sorter);
	}

	public void Add(int Score, string Details, long Turn, string GameId, string GameMode, bool ReplaceOnId = false, int Level = 0, string Name = "")
	{
		if (ReplaceOnId && !string.IsNullOrEmpty(GameId))
		{
			if (Scores.Any((ScoreEntry2 e) => e.GameId == GameId))
			{
				if (Scores.Where((ScoreEntry2 e) => e.GameId == GameId).First().Score < Score)
				{
					Scores.RemoveAll((ScoreEntry2 e) => e.GameId == GameId);
					Scores.Add(new ScoreEntry2(Score, Details, Turn, GameId, GameMode, Level, Name));
				}
			}
			else
			{
				Scores.Add(new ScoreEntry2(Score, Details, Turn, GameId, GameMode, Level, Name));
			}
		}
		else
		{
			Scores.Add(new ScoreEntry2(Score, Details, Turn, GameId, GameMode, Level, Name));
		}
		Save().Wait();
	}

	public async Task Save()
	{
		try
		{
			Sort();
			await Platform.IO.File.WriteAllJsonAsync(DataManager.SyncedPath("HighScores.json"), this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Scoreboard Save", x);
		}
	}

	public static async Task UpgradeScoreIfNecessary()
	{
		string oldSavePath = DataManager.LocalPath("HighScores.dat");
		if (await Platform.IO.File.ExistsAsync(DataManager.SyncedPath("HighScores.json")) || !(await Platform.IO.File.ExistsAsync(oldSavePath)))
		{
			return;
		}
		LoadBytesResult loadBytesResult = await Blob.ReadAllBytesAsync(oldSavePath);
		if (!loadBytesResult.LogErrorIfFailed().WasSuccessful())
		{
			return;
		}
		using MemoryStream oldLoadStream = new MemoryStream(loadBytesResult.content);
		Scoreboard obj = ((IFormatter)new BinaryFormatter()).Deserialize((Stream)oldLoadStream) as Scoreboard;
		Scoreboard2 scoreboard = new Scoreboard2();
		foreach (ScoreEntry score in obj.Scores)
		{
			scoreboard.Scores.Add(new ScoreEntry2(score));
		}
		await scoreboard.Save();
	}

	public static async Task<Scoreboard2> Load()
	{
		await UpgradeScoreIfNecessary();
		Scoreboard2 result = null;
		try
		{
			if (await Platform.IO.File.ExistsAsync(DataManager.SyncedPath("HighScores.json")))
			{
				result = await Platform.IO.File.ReadAllJsonAsync<Scoreboard2>(DataManager.SyncedPath("HighScores.json"));
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Exception loading high scores", x);
			result = new Scoreboard2();
		}
		if (result == null)
		{
			result = new Scoreboard2();
		}
		if (result.Scores == null)
		{
			result.Scores = new List<ScoreEntry2>();
		}
		result.Scores.RemoveAll((ScoreEntry2 s) => s == null);
		return result;
	}
}
