using System;
using System.Collections.Generic;

namespace XRL.Core;

[Serializable]
[Obsolete]
public class Scoreboard
{
	public List<ScoreEntry> Scores = new List<ScoreEntry>();
}
