namespace Qud.UI;

public interface IControlledInputField : IControlledSelectable
{
	bool isFocused { get; }

	bool selected { get; set; }

	void Init();
}
