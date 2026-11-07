using ConsoleLib.Console;
using XRL.UI.Framework;
using XRL.World;

namespace Qud.UI;

public interface IStatusScreen
{
	string GetNavigationCategory();

	IRenderable GetTabIcon();

	string GetTabString();

	bool WantsCategoryBar();

	NavigationContext ShowScreen(GameObject GO, StatusScreensScreen parent);

	void HideScreen();

	bool Exit();

	void CheckLayoutFrame();

	void ResetLayoutFrame(int Frames);

	void PrepareLayoutFrame();

	void FilterUpdated(string filterText);

	void HandleMenuOption(FrameworkDataElement data);

	void Prewarm();
}
