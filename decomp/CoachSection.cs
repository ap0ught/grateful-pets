using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class CoachSection
{
	public int Depth;

	public CoachSection Parent;

	public Stopwatch Timer = new Stopwatch();

	public TimeSpan Time = TimeSpan.Zero;

	public string Name = "";

	public int Count = 1;

	public bool LogGarbage;

	public long StartGarbage;

	public long TotalGarbage;

	public Dictionary<string, CoachSection> Sections = new Dictionary<string, CoachSection>();

	public CoachSection(string N, bool bTrackGarbage)
	{
		Name = N;
		LogGarbage = bTrackGarbage;
	}

	public long FinalizeTime()
	{
		long num = 0L;
		foreach (string key in Sections.Keys)
		{
			long num2 = Sections[key].FinalizeTime();
			num += num2;
		}
		return num + Timer.ElapsedMilliseconds;
	}

	public void Log(StringBuilder SB)
	{
		if (SB.Length >= 0)
		{
			SB.Append('\n');
		}
		SB.Append('\t', Depth).Append(Name).Append(": ")
			.Append(FinalizeTime())
			.Append("ms");
		foreach (KeyValuePair<string, CoachSection> section in Sections)
		{
			section.Value.Log(SB);
		}
	}
}
