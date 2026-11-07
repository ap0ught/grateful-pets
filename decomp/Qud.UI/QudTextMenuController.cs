using UnityEngine;

namespace Qud.UI;

[ExecuteAlways]
public class QudTextMenuController : QudBaseMenuController<QudMenuItem, SelectableTextMenuItem>
{
	public override void Update()
	{
		base.Update();
		CheckKeyInteractions(menuData, Activate);
	}
}
