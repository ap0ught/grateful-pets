namespace Qud.UI;

public interface IControlledSelectable
{
	bool IsSelected();

	void Select();

	bool IsInFullView();

	bool IsInView();
}
