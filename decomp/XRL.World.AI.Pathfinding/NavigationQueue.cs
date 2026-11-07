using XRL.Collections;

namespace XRL.World.AI.Pathfinding;

internal class NavigationQueue
{
	private Rack<GridNode> Items = new Rack<GridNode>();

	public int Count => Items.Count;

	public void Clear()
	{
		Items.Clear();
	}

	public void Enqueue(GridNode Item)
	{
		int num = Items.Count;
		Items.Add(Item);
		Item.Index = num;
		GridNode[] array = Items.GetArray();
		while (num != 0)
		{
			int num2 = (num - 1) / 2;
			if (array[num].CompareTo(array[num2]) < 0)
			{
				GridNode gridNode = array[num];
				array[num] = array[num2];
				array[num].Index = num;
				array[num2] = gridNode;
				array[num2].Index = num2;
				num = num2;
				continue;
			}
			break;
		}
	}

	public void Requeue(GridNode Item)
	{
		int num = Item.Index;
		GridNode[] array = Items.GetArray();
		while (num > 0)
		{
			int num2 = (num - 1) / 2;
			if (array[num].CompareTo(array[num2]) < 0)
			{
				GridNode gridNode = array[num];
				array[num] = array[num2];
				array[num].Index = num;
				array[num2] = gridNode;
				array[num2].Index = num2;
				num = num2;
				continue;
			}
			break;
		}
	}

	public GridNode Dequeue()
	{
		GridNode[] array = Items.GetArray();
		GridNode result = array[0];
		int num = Items.Count - 1;
		array[0] = array[num];
		array[0].Index = 0;
		Items.RemoveAt(num);
		int num2 = 0;
		while (true)
		{
			int num3 = num2;
			int num4 = 2 * num2 + 1;
			int num5 = num4 + 1;
			if (num4 < num && array[num2].CompareTo(array[num4]) > 0)
			{
				num2 = num4;
			}
			if (num5 < num && array[num2].CompareTo(array[num5]) > 0)
			{
				num2 = num5;
			}
			if (num2 == num3)
			{
				break;
			}
			GridNode gridNode = array[num2];
			array[num2] = array[num3];
			array[num2].Index = num2;
			array[num3] = gridNode;
			array[num3].Index = num3;
		}
		return result;
	}
}
