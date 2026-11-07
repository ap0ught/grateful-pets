using XRL.World;

namespace XRL.Core;

public interface IActionCommand : IComposite
{
	const int PRIORITY_VERY_LOW = 15000;

	const int PRIORITY_LOW = 30000;

	const int PRIORITY_MEDIUM = 45000;

	const int PRIORITY_HIGH = 60000;

	const int PRIORITY_VERY_HIGH = 75000;

	const int PRIORITY_ADJUST_VERY_SMALL = 1;

	const int PRIORITY_ADJUST_SMALL = 10;

	const int PRIORITY_ADJUST_MEDIUM = 100;

	const int PRIORITY_ADJUST_LARGE = 1000;

	const int PRIORITY_ADJUST_VERY_LARGE = 10000;

	int Priority => 45000;

	void Execute(XRLGame Game, ActionManager Manager);
}
