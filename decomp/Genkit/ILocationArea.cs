using System.Collections.Generic;

namespace Genkit;

public interface ILocationArea
{
	IEnumerable<Location2D> EnumerateLocations();

	IEnumerable<Location2D> EnumerateBorderLocations();

	IEnumerable<Location2D> EnumerateNonBorderLocations();

	Location2D GetCenter();

	bool PointIn(Location2D location);
}
