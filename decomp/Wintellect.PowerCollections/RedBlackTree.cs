using System;
using System.Collections;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
internal class RedBlackTree<T> : IEnumerable<T>, IEnumerable
{
	[Serializable]
	private class Node
	{
		public Node left;

		public Node right;

		public T item;

		private const uint REDMASK = 2147483648u;

		private uint count;

		public bool IsRed
		{
			get
			{
				return (count & 0x80000000u) != 0;
			}
			set
			{
				if (value)
				{
					count |= 2147483648u;
				}
				else
				{
					count &= 2147483647u;
				}
			}
		}

		public int Count
		{
			get
			{
				return (int)(count & 0x7FFFFFFF);
			}
			set
			{
				count = (count & 0x80000000u) | (uint)value;
			}
		}

		public void IncrementCount()
		{
			count++;
		}

		public void DecrementCount()
		{
			count--;
		}

		public Node Clone()
		{
			Node node = new Node();
			node.item = item;
			node.count = count;
			if (left != null)
			{
				node.left = left.Clone();
			}
			if (right != null)
			{
				node.right = right.Clone();
			}
			return node;
		}
	}

	public delegate int RangeTester(T item);

	private IComparer<T> comparer;

	private Node root;

	private int count;

	private int changeStamp;

	private Node[] stack;

	public int ElementCount => count;

	private Node[] GetNodeStack()
	{
		int num;
		if (count < 1024)
		{
			num = 21;
		}
		else
		{
			num = ((count >= 65536) ? 65 : 41);
		}
		if (stack == null || stack.Length < num)
		{
			stack = new Node[num];
		}
		return stack;
	}

	internal void StopEnumerations()
	{
		changeStamp++;
	}

	private void CheckEnumerationStamp(int startStamp)
	{
		if (startStamp != changeStamp)
		{
			throw new InvalidOperationException(Strings.ChangeDuringEnumeration);
		}
	}

	public RedBlackTree(IComparer<T> comparer)
	{
		this.comparer = comparer;
		count = 0;
		root = null;
	}

	public RedBlackTree<T> Clone()
	{
		RedBlackTree<T> redBlackTree = new RedBlackTree<T>(comparer);
		redBlackTree.count = count;
		if (root != null)
		{
			redBlackTree.root = root.Clone();
		}
		return redBlackTree;
	}

	public bool Find(T key, bool findFirst, bool replace, out T item)
	{
		Node node = root;
		Node node2 = null;
		while (node != null)
		{
			int num = comparer.Compare(key, node.item);
			if (num < 0)
			{
				node = node.left;
				continue;
			}
			if (num > 0)
			{
				node = node.right;
				continue;
			}
			node2 = node;
			node = ((!findFirst) ? node.right : node.left);
		}
		if (node2 != null)
		{
			item = node2.item;
			if (replace)
			{
				node2.item = key;
			}
			return true;
		}
		item = default;
		return false;
	}

	public int FindIndex(T key, bool findFirst)
	{
		T item;
		if (findFirst)
		{
			return FirstItemInRange(EqualRangeTester(key), out item);
		}
		return LastItemInRange(EqualRangeTester(key), out item);
	}

	public T GetItemByIndex(int index)
	{
		if (index < 0 || index >= count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		Node node = root;
		while (true)
		{
			int num = ((node.left != null) ? node.left.Count : 0);
			if (num > index)
			{
				node = node.left;
				continue;
			}
			if (num == index)
			{
				break;
			}
			index -= num + 1;
			node = node.right;
		}
		return node.item;
	}

	public bool Insert(T item, DuplicatePolicy dupPolicy, out T previous)
	{
		Node node = root;
		Node node2 = null;
		Node node3 = null;
		Node ggparent = null;
		bool flag = false;
		bool flag2 = false;
		Node node4 = null;
		StopEnumerations();
		bool flag3 = dupPolicy != DuplicatePolicy.InsertFirst && dupPolicy != DuplicatePolicy.InsertLast;
		Node[] array = null;
		int num = 0;
		if (flag3)
		{
			array = GetNodeStack();
		}
		bool rotated;
		while (node != null)
		{
			if (node.left != null && node.left.IsRed && node.right != null && node.right.IsRed)
			{
				node = InsertSplit(ggparent, node3, node2, node, out rotated);
				if (flag3 & rotated)
				{
					num -= 2;
					if (num < 0)
					{
						num = 0;
					}
				}
			}
			ggparent = node3;
			node3 = node2;
			node2 = node;
			int num2 = comparer.Compare(item, node.item);
			if (num2 == 0)
			{
				switch (dupPolicy)
				{
				case DuplicatePolicy.DoNothing:
				{
					previous = node.item;
					for (int i = 0; i < num; i++)
					{
						array[i].DecrementCount();
					}
					return false;
				}
				case DuplicatePolicy.InsertFirst:
				case DuplicatePolicy.ReplaceFirst:
					node4 = node;
					num2 = -1;
					break;
				default:
					node4 = node;
					num2 = 1;
					break;
				}
			}
			node.IncrementCount();
			if (flag3)
			{
				array[num++] = node;
			}
			if (num2 < 0)
			{
				node = node.left;
				flag = true;
				flag2 = false;
			}
			else
			{
				node = node.right;
				flag2 = true;
				flag = false;
			}
		}
		if (node4 != null)
		{
			previous = node4.item;
			if (node4 != null && (dupPolicy == DuplicatePolicy.ReplaceFirst || dupPolicy == DuplicatePolicy.ReplaceLast))
			{
				node4.item = item;
				for (int j = 0; j < num; j++)
				{
					array[j].DecrementCount();
				}
				return false;
			}
		}
		else
		{
			previous = default;
		}
		node = new Node();
		node.item = item;
		node.Count = 1;
		if (flag)
		{
			node2.left = node;
		}
		else if (flag2)
		{
			node2.right = node;
		}
		else
		{
			root = node;
		}
		InsertSplit(ggparent, node3, node2, node, out rotated);
		count++;
		return node4 == null;
	}

	private Node InsertSplit(Node ggparent, Node gparent, Node parent, Node node, out bool rotated)
	{
		if (node != root)
		{
			node.IsRed = true;
		}
		if (node.left != null)
		{
			node.left.IsRed = false;
		}
		if (node.right != null)
		{
			node.right.IsRed = false;
		}
		if (parent != null && parent.IsRed)
		{
			if (gparent.left == parent != (parent.left == node))
			{
				Rotate(gparent, parent, node);
				parent = node;
			}
			gparent.IsRed = true;
			Rotate(ggparent, gparent, parent);
			parent.IsRed = false;
			rotated = true;
			return parent;
		}
		rotated = false;
		return node;
	}

	private void Rotate(Node node, Node child, Node gchild)
	{
		if (gchild == child.left)
		{
			child.left = gchild.right;
			gchild.right = child;
		}
		else
		{
			child.right = gchild.left;
			gchild.left = child;
		}
		child.Count = ((child.left != null) ? child.left.Count : 0) + ((child.right != null) ? child.right.Count : 0) + 1;
		gchild.Count = ((gchild.left != null) ? gchild.left.Count : 0) + ((gchild.right != null) ? gchild.right.Count : 0) + 1;
		if (node == null)
		{
			root = gchild;
		}
		else if (child == node.left)
		{
			node.left = gchild;
		}
		else
		{
			node.right = gchild;
		}
	}

	public bool Delete(T key, bool deleteFirst, out T item)
	{
		return DeleteItemFromRange(EqualRangeTester(key), deleteFirst, out item);
	}

	public void Clear()
	{
		root = null;
		if (count > 0)
		{
			count = 0;
			Array.Clear(stack, 0, stack.Length);
		}
		changeStamp++;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return EnumerateRange(EntireRangeTester).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public RangeTester BoundedRangeTester(bool useFirst, T first, bool useLast, T last)
	{
		return (T item) =>
		{
			if (useFirst && comparer.Compare(first, item) > 0)
			{
				return -1;
			}
			return (useLast && comparer.Compare(last, item) <= 0) ? 1 : 0;
		};
	}

	public RangeTester DoubleBoundedRangeTester(T first, bool firstInclusive, T last, bool lastInclusive)
	{
		return (T item) =>
		{
			if (firstInclusive)
			{
				if (comparer.Compare(first, item) > 0)
				{
					return -1;
				}
			}
			else if (comparer.Compare(first, item) >= 0)
			{
				return -1;
			}
			if (lastInclusive)
			{
				if (comparer.Compare(last, item) < 0)
				{
					return 1;
				}
			}
			else if (comparer.Compare(last, item) <= 0)
			{
				return 1;
			}
			return 0;
		};
	}

	public RangeTester LowerBoundedRangeTester(T first, bool inclusive)
	{
		return (T item) =>
		{
			if (inclusive)
			{
				if (comparer.Compare(first, item) > 0)
				{
					return -1;
				}
				return 0;
			}
			return (comparer.Compare(first, item) >= 0) ? (-1) : 0;
		};
	}

	public RangeTester UpperBoundedRangeTester(T last, bool inclusive)
	{
		return (T item) =>
		{
			if (inclusive)
			{
				if (comparer.Compare(last, item) < 0)
				{
					return 1;
				}
				return 0;
			}
			return (comparer.Compare(last, item) <= 0) ? 1 : 0;
		};
	}

	public RangeTester EqualRangeTester(T equalTo)
	{
		return (T item) => comparer.Compare(item, equalTo);
	}

	public int EntireRangeTester(T item)
	{
		return 0;
	}

	public IEnumerable<T> EnumerateRange(RangeTester rangeTester)
	{
		return EnumerateRangeInOrder(rangeTester, root);
	}

	private IEnumerable<T> EnumerateRangeInOrder(RangeTester rangeTester, Node node)
	{
		int startStamp = changeStamp;
		if (node == null)
		{
			yield break;
		}
		int compare = rangeTester(node.item);
		if (compare >= 0)
		{
			foreach (T item in EnumerateRangeInOrder(rangeTester, node.left))
			{
				yield return item;
				CheckEnumerationStamp(startStamp);
			}
		}
		if (compare == 0)
		{
			yield return node.item;
			CheckEnumerationStamp(startStamp);
		}
		if (compare > 0)
		{
			yield break;
		}
		foreach (T item2 in EnumerateRangeInOrder(rangeTester, node.right))
		{
			yield return item2;
			CheckEnumerationStamp(startStamp);
		}
	}

	public IEnumerable<T> EnumerateRangeReversed(RangeTester rangeTester)
	{
		return EnumerateRangeInReversedOrder(rangeTester, root);
	}

	private IEnumerable<T> EnumerateRangeInReversedOrder(RangeTester rangeTester, Node node)
	{
		int startStamp = changeStamp;
		if (node == null)
		{
			yield break;
		}
		int compare = rangeTester(node.item);
		if (compare <= 0)
		{
			foreach (T item in EnumerateRangeInReversedOrder(rangeTester, node.right))
			{
				yield return item;
				CheckEnumerationStamp(startStamp);
			}
		}
		if (compare == 0)
		{
			yield return node.item;
			CheckEnumerationStamp(startStamp);
		}
		if (compare < 0)
		{
			yield break;
		}
		foreach (T item2 in EnumerateRangeInReversedOrder(rangeTester, node.left))
		{
			yield return item2;
			CheckEnumerationStamp(startStamp);
		}
	}

	public bool DeleteItemFromRange(RangeTester rangeTester, bool deleteFirst, out T item)
	{
		StopEnumerations();
		if (root == null)
		{
			item = default;
			return false;
		}
		Node[] nodeStack = GetNodeStack();
		int num = 0;
		Node node = root;
		Node node3;
		Node node4;
		Node node2 = (node3 = (node4 = null));
		Node node5 = null;
		while (true)
		{
			if ((node.left == null || !node.left.IsRed) && (node.right == null || !node.right.IsRed))
			{
				if (node3 == null)
				{
					node.IsRed = true;
				}
				else if ((node2.left == null || !node2.left.IsRed) && (node2.right == null || !node2.right.IsRed))
				{
					node.IsRed = true;
					node2.IsRed = true;
					node3.IsRed = false;
				}
				else
				{
					if (node3.left == node && (node2.right == null || !node2.right.IsRed))
					{
						Node left = node2.left;
						Rotate(node3, node2, left);
						node2 = left;
					}
					else if (node3.right == node && (node2.left == null || !node2.left.IsRed))
					{
						Node right = node2.right;
						Rotate(node3, node2, right);
						node2 = right;
					}
					Rotate(node4, node3, node2);
					node.IsRed = true;
					node2.IsRed = true;
					node2.left.IsRed = false;
					node2.right.IsRed = false;
					node2.DecrementCount();
					nodeStack[num - 1] = node2;
					node3.DecrementCount();
					nodeStack[num++] = node3;
				}
			}
			do
			{
				node.DecrementCount();
				nodeStack[num++] = node;
				int num2 = rangeTester(node.item);
				Node node6;
				Node node7;
				if (num2 == 0)
				{
					node5 = node;
					if (deleteFirst)
					{
						node6 = node.left;
						node7 = node.right;
					}
					else
					{
						node6 = node.right;
						node7 = node.left;
					}
				}
				else if (num2 > 0)
				{
					node6 = node.left;
					node7 = node.right;
				}
				else
				{
					node6 = node.right;
					node7 = node.left;
				}
				if (node6 != null)
				{
					node4 = node3;
					node3 = node;
					node = node6;
					node2 = node7;
					continue;
				}
				if (node5 == null)
				{
					for (int i = 0; i < num; i++)
					{
						nodeStack[i].IncrementCount();
					}
					if (root != null)
					{
						root.IsRed = false;
					}
					item = default;
					return false;
				}
				item = node5.item;
				if (node5 != node)
				{
					node5.item = node.item;
				}
				Node node8;
				if (node.left != null)
				{
					node8 = node.left;
					node8.IsRed = false;
				}
				else if (node.right != null)
				{
					node8 = node.right;
					node8.IsRed = false;
				}
				else
				{
					node8 = null;
				}
				if (node3 == null)
				{
					root = node8;
				}
				else if (node3.left == node)
				{
					node3.left = node8;
				}
				else
				{
					node3.right = node8;
				}
				if (root != null)
				{
					root.IsRed = false;
				}
				count--;
				return true;
			}
			while (!node3.IsRed && node.IsRed);
			if (!node3.IsRed)
			{
				Rotate(node4, node3, node2);
				node2.DecrementCount();
				nodeStack[num - 1] = node2;
				node3.DecrementCount();
				nodeStack[num++] = node3;
				node2.IsRed = false;
				node3.IsRed = true;
				node4 = node2;
				node2 = ((node3.left == node) ? node3.right : node3.left);
			}
		}
	}

	public int DeleteRange(RangeTester rangeTester)
	{
		int num = 0;
		bool flag;
		do
		{
			flag = DeleteItemFromRange(rangeTester, deleteFirst: true, out var _);
			if (flag)
			{
				num++;
			}
		}
		while (flag);
		return num;
	}

	public int CountRange(RangeTester rangeTester)
	{
		return CountRangeUnderNode(rangeTester, root, belowRangeTop: false, aboveRangeBottom: false);
	}

	private int CountRangeUnderNode(RangeTester rangeTester, Node node, bool belowRangeTop, bool aboveRangeBottom)
	{
		if (node != null)
		{
			if (belowRangeTop & aboveRangeBottom)
			{
				return node.Count;
			}
			int num = rangeTester(node.item);
			if (num == 0)
			{
				int num2 = 1;
				num2 += CountRangeUnderNode(rangeTester, node.left, belowRangeTop: true, aboveRangeBottom);
				return num2 + CountRangeUnderNode(rangeTester, node.right, belowRangeTop, aboveRangeBottom: true);
			}
			if (num < 0)
			{
				return CountRangeUnderNode(rangeTester, node.right, belowRangeTop, aboveRangeBottom);
			}
			return CountRangeUnderNode(rangeTester, node.left, belowRangeTop, aboveRangeBottom);
		}
		return 0;
	}

	public int FirstItemInRange(RangeTester rangeTester, out T item)
	{
		Node node = root;
		Node node2 = null;
		int num = 0;
		int result = -1;
		while (node != null)
		{
			int num2 = rangeTester(node.item);
			if (num2 == 0)
			{
				node2 = node;
				result = ((node.left == null) ? num : (num + node.left.Count));
			}
			if (num2 >= 0)
			{
				node = node.left;
				continue;
			}
			num = ((node.left == null) ? (num + 1) : (num + (node.left.Count + 1)));
			node = node.right;
		}
		if (node2 != null)
		{
			item = node2.item;
			return result;
		}
		item = default;
		return -1;
	}

	public int LastItemInRange(RangeTester rangeTester, out T item)
	{
		Node node = root;
		Node node2 = null;
		int num = 0;
		int result = -1;
		while (node != null)
		{
			int num2 = rangeTester(node.item);
			if (num2 == 0)
			{
				node2 = node;
				result = ((node.left == null) ? num : (num + node.left.Count));
			}
			if (num2 <= 0)
			{
				num = ((node.left == null) ? (num + 1) : (num + (node.left.Count + 1)));
				node = node.right;
			}
			else
			{
				node = node.left;
			}
		}
		if (node2 != null)
		{
			item = node2.item;
			return result;
		}
		item = default;
		return result;
	}
}
