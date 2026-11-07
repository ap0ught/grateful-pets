using XRL.World;

namespace XRL.Core;

public interface ISystemSaveCommand : IActionCommand, IComposite
{
	bool IComposite.WantFieldReflection => false;

	int IActionCommand.Priority => 15000;

	string Type { get; }
}
