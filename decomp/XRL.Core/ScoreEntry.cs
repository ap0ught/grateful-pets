using System;

namespace XRL.Core;

[Serializable]
[Obsolete]
public class ScoreEntry
{
	public int Score;

	public string Description;

	public string Details;

	public ScoreEntry(int _Score, string _Description, string _Details)
	{
		Score = _Score;
		Description = _Description;
		Details = _Details;
	}
}
