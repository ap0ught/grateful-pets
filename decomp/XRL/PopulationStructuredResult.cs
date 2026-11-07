using System.Collections.Generic;

namespace XRL;

public class PopulationStructuredResult
{
	public string Hint;

	public List<PopulationResult> Objects = new List<PopulationResult>();

	public List<PopulationStructuredResult> ChildGroups = new List<PopulationStructuredResult>();
}
