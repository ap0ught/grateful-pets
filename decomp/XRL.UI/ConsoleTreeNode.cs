using System.Collections.Generic;

namespace XRL.UI;

public abstract class ConsoleTreeNode<T> where T : ConsoleTreeNode<T>
{
	public string Category;

	public bool Expand;

	public T ParentNode;

	public virtual bool Visible => ParentNode?.Expand ?? true;

	public ConsoleTreeNode(string Category = "", bool Expand = false, T ParentNode = null)
	{
		this.Category = Category;
		this.Expand = Expand;
		this.ParentNode = ParentNode;
	}

	public static int NextVisible(List<T> Nodes, ref int Index, int Mod = 0)
	{
		int i;
		for (i = Index + Mod; i < Nodes.Count && !Nodes[i].Visible; i++)
		{
		}
		if (i != Nodes.Count)
		{
			return Index = i;
		}
		return Index;
	}

	public static int PrevVisible(List<T> Nodes, ref int Index, int Mod = 0)
	{
		int num = Index + Mod;
		while (num >= 0 && !Nodes[num].Visible)
		{
			num--;
		}
		if (num != -1)
		{
			return Index = num;
		}
		return Index;
	}
}
