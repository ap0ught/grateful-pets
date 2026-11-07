using XRL.Collections;

namespace Overlay.MapEditor;

public class MapEditorSettings
{
	public string DefaultPath;

	public Rack<string> RecentFiles = new Rack<string>();
}
