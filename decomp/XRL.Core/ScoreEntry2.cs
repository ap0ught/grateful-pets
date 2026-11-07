using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Platform.IO;

namespace XRL.Core;

public class ScoreEntry2
{
	public int Score;

	public string Details;

	public long Turns;

	public string GameId;

	public string GameMode;

	public string Name;

	public int Level;

	public int Version;

	public static Regex RE_Level = new Regex("You were level \\{\\{C\\|(?<Level>\\d+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	public static Regex RE_Turns = new Regex("You survived for \\{\\{C\\|(?<Turns>\\d+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	public static Regex RE_Name = new Regex("\\{\\{C\\|.\\}\\} Game summary for \\{\\{W\\|(?<Name>[^\\n]*)\\}\\} \\{\\{C\\|.\\}\\}", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	public static Regex RE_Mode = new Regex("This game was played in (?<Mode>.+?) mode.", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	public ScoreEntry2()
	{
	}

	[Obsolete]
	public ScoreEntry2(ScoreEntry legacy)
	{
		Score = legacy.Score;
		Details = legacy.Details;
		Turns = 0L;
		GameId = "";
		GameMode = "Classic";
	}

	public ScoreEntry2(int _Score, string _Details, long Turns, string GameId, string GameMode, int Level, string Name)
	{
		Score = _Score;
		Details = _Details;
		this.Turns = Turns;
		this.GameId = GameId;
		this.GameMode = GameMode;
		this.Level = Level;
		this.Name = Name;
		Version = 2;
	}

	public void CheckVersion()
	{
		if (Version < 2)
		{
			Match match = RE_Level.Match(Details);
			if (match.Success)
			{
				Level = Convert.ToInt32(match.Groups["Level"].Value);
			}
			match = RE_Turns.Match(Details);
			if (match.Success)
			{
				Turns = Convert.ToInt32(match.Groups["Turns"].Value);
			}
			match = RE_Name.Match(Details);
			if (match.Success)
			{
				Name = match.Groups["Name"].Value;
			}
			match = RE_Mode.Match(Details);
			if (match.Success)
			{
				GameMode = match.Groups["Mode"].Value;
			}
			Version = 2;
		}
	}

	public bool HasCoda()
	{
		return File.Exists(DataManager.SyncedPath("Codas/coda-" + GameId + ".sav.gz"));
	}

	public async Task<XRLGame> LoadCoda()
	{
		if (File.Exists(DataManager.SyncedPath("Codas/coda-" + GameId.ToLower() + ".sav.gz")))
		{
			XRLGame xRLGame = await XRLGame.LoadGame(DataManager.SyncedPath("Codas/coda-" + GameId.ToLower()));
			if (xRLGame != null)
			{
				xRLGame.DontSaveThisIsAReplay = true;
				return xRLGame;
			}
		}
		return null;
	}

	public void DeleteCoda()
	{
		File.Delete(DataManager.SyncedPath("Codas/coda-" + GameId.ToLower() + ".sav.gz"));
	}
}
