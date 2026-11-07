using System;

namespace Battlehub.UIControls;

public class ParentChangedEventArgs : EventArgs
{
	public TreeViewItem OldParent { get; private set; }

	public TreeViewItem NewParent { get; private set; }

	public ParentChangedEventArgs(TreeViewItem oldParent, TreeViewItem newParent)
	{
		OldParent = oldParent;
		NewParent = newParent;
	}
}
