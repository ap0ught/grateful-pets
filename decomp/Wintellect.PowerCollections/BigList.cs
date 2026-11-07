using System;
using System.Collections.Generic;

namespace Wintellect.PowerCollections;

[Serializable]
public class BigList<T> : ListBase<T>, ICloneable
{
	[Serializable]
	private abstract class Node
	{
		public int count;

		protected volatile bool shared;

		public int Count => count;

		public bool Shared => shared;

		public abstract int Depth { get; }

		public void MarkShared()
		{
			shared = true;
		}

		public abstract T GetAt(int index);

		public abstract Node Subrange(int first, int last);

		public abstract Node SetAt(int index, T item);

		public abstract Node SetAtInPlace(int index, T item);

		public abstract Node Append(Node node, bool nodeIsUnused);

		public abstract Node AppendInPlace(Node node, bool nodeIsUnused);

		public abstract Node AppendInPlace(T item);

		public abstract Node RemoveRange(int first, int last);

		public abstract Node RemoveRangeInPlace(int first, int last);

		public abstract Node Insert(int index, Node node, bool nodeIsUnused);

		public abstract Node InsertInPlace(int index, T item);

		public abstract Node InsertInPlace(int index, Node node, bool nodeIsUnused);

		public Node Prepend(Node node, bool nodeIsUnused)
		{
			if (nodeIsUnused)
			{
				return node.AppendInPlace(this, nodeIsUnused: false);
			}
			return node.Append(this, nodeIsUnused: false);
		}

		public Node PrependInPlace(Node node, bool nodeIsUnused)
		{
			if (nodeIsUnused)
			{
				return node.AppendInPlace(this, !shared);
			}
			return node.Append(this, !shared);
		}

		public abstract Node PrependInPlace(T item);

		public bool IsBalanced()
		{
			if (Depth <= 44)
			{
				return Count >= BigList<T>.FIBONACCI[Depth];
			}
			return false;
		}

		public bool IsAlmostBalanced()
		{
			if (Depth != 0)
			{
				if (Depth - 1 <= 44)
				{
					return Count >= BigList<T>.FIBONACCI[Depth - 1];
				}
				return false;
			}
			return true;
		}
	}

	[Serializable]
	private sealed class LeafNode : Node
	{
		public T[] items;

		public override int Depth => 0;

		public LeafNode(T item)
		{
			count = 1;
			items = new T[120];
			items[0] = item;
		}

		public LeafNode(int count, T[] newItems)
		{
			base.count = count;
			items = newItems;
		}

		public override T GetAt(int index)
		{
			return items[index];
		}

		public override Node SetAtInPlace(int index, T item)
		{
			if (shared)
			{
				return SetAt(index, item);
			}
			items[index] = item;
			return this;
		}

		public override Node SetAt(int index, T item)
		{
			T[] array = (T[])items.Clone();
			array[index] = item;
			return new LeafNode(count, array);
		}

		private bool MergeLeafInPlace(Node other)
		{
			int num;
			if (other is LeafNode leafNode && (num = leafNode.Count + count) <= 120)
			{
				if (num > items.Length)
				{
					T[] destinationArray = new T[120];
					Array.Copy(items, 0, destinationArray, 0, count);
					items = destinationArray;
				}
				Array.Copy(leafNode.items, 0, items, count, leafNode.count);
				count = num;
				return true;
			}
			return false;
		}

		private Node MergeLeaf(Node other)
		{
			int num;
			if (other is LeafNode leafNode && (num = leafNode.Count + count) <= 120)
			{
				T[] array = new T[120];
				Array.Copy(items, 0, array, 0, count);
				Array.Copy(leafNode.items, 0, array, count, leafNode.count);
				return new LeafNode(num, array);
			}
			return null;
		}

		public override Node PrependInPlace(T item)
		{
			if (shared)
			{
				return Prepend(new LeafNode(item), nodeIsUnused: true);
			}
			if (count < 120)
			{
				if (count == items.Length)
				{
					T[] destinationArray = new T[120];
					Array.Copy(items, 0, destinationArray, 1, count);
					items = destinationArray;
				}
				else
				{
					Array.Copy(items, 0, items, 1, count);
				}
				items[0] = item;
				count++;
				return this;
			}
			return new ConcatNode(new LeafNode(item), this);
		}

		public override Node AppendInPlace(T item)
		{
			if (shared)
			{
				return Append(new LeafNode(item), nodeIsUnused: true);
			}
			if (count < 120)
			{
				if (count == items.Length)
				{
					T[] destinationArray = new T[120];
					Array.Copy(items, 0, destinationArray, 0, count);
					items = destinationArray;
				}
				items[count] = item;
				count++;
				return this;
			}
			return new ConcatNode(this, new LeafNode(item));
		}

		public override Node AppendInPlace(Node node, bool nodeIsUnused)
		{
			if (shared)
			{
				return Append(node, nodeIsUnused);
			}
			if (MergeLeafInPlace(node))
			{
				return this;
			}
			if (node is ConcatNode concatNode && MergeLeafInPlace(concatNode.left))
			{
				if (!nodeIsUnused)
				{
					concatNode.right.MarkShared();
				}
				return new ConcatNode(this, concatNode.right);
			}
			if (!nodeIsUnused)
			{
				node.MarkShared();
			}
			return new ConcatNode(this, node);
		}

		public override Node Append(Node node, bool nodeIsUnused)
		{
			Node result;
			if ((result = MergeLeaf(node)) != null)
			{
				return result;
			}
			if (node is ConcatNode concatNode && (result = MergeLeaf(concatNode.left)) != null)
			{
				if (!nodeIsUnused)
				{
					concatNode.right.MarkShared();
				}
				return new ConcatNode(result, concatNode.right);
			}
			if (!nodeIsUnused)
			{
				node.MarkShared();
			}
			MarkShared();
			return new ConcatNode(this, node);
		}

		public override Node InsertInPlace(int index, T item)
		{
			if (shared)
			{
				return Insert(index, new LeafNode(item), nodeIsUnused: true);
			}
			if (count < 120)
			{
				if (count == items.Length)
				{
					T[] destinationArray = new T[120];
					if (index > 0)
					{
						Array.Copy(items, 0, destinationArray, 0, index);
					}
					if (count > index)
					{
						Array.Copy(items, index, destinationArray, index + 1, count - index);
					}
					items = destinationArray;
				}
				else if (count > index)
				{
					Array.Copy(items, index, items, index + 1, count - index);
				}
				items[index] = item;
				count++;
				return this;
			}
			if (index == count)
			{
				return new ConcatNode(this, new LeafNode(item));
			}
			if (index == 0)
			{
				return new ConcatNode(new LeafNode(item), this);
			}
			T[] array = new T[120];
			Array.Copy(items, 0, array, 0, index);
			array[index] = item;
			LeafNode left = new LeafNode(index + 1, array);
			T[] array2 = new T[count - index];
			Array.Copy(items, index, array2, 0, count - index);
			Node right = new LeafNode(count - index, array2);
			return new ConcatNode(left, right);
		}

		public override Node InsertInPlace(int index, Node node, bool nodeIsUnused)
		{
			if (shared)
			{
				return Insert(index, node, nodeIsUnused);
			}
			int num;
			if (node is LeafNode leafNode && (num = leafNode.Count + count) <= 120)
			{
				if (num > items.Length)
				{
					T[] destinationArray = new T[120];
					Array.Copy(items, 0, destinationArray, 0, index);
					Array.Copy(leafNode.items, 0, destinationArray, index, leafNode.Count);
					Array.Copy(items, index, destinationArray, index + leafNode.Count, count - index);
					items = destinationArray;
				}
				else
				{
					Array.Copy(items, index, items, index + leafNode.Count, count - index);
					Array.Copy(leafNode.items, 0, items, index, leafNode.count);
				}
				count = num;
				return this;
			}
			if (index == 0)
			{
				return PrependInPlace(node, nodeIsUnused);
			}
			if (index == count)
			{
				return AppendInPlace(node, nodeIsUnused);
			}
			T[] array = new T[index];
			Array.Copy(items, 0, array, 0, index);
			LeafNode leafNode2 = new LeafNode(index, array);
			T[] array2 = new T[count - index];
			Array.Copy(items, index, array2, 0, count - index);
			Node node2 = new LeafNode(count - index, array2);
			return leafNode2.AppendInPlace(node, nodeIsUnused).AppendInPlace(node2, nodeIsUnused: true);
		}

		public override Node Insert(int index, Node node, bool nodeIsUnused)
		{
			int num;
			if (node is LeafNode leafNode && (num = leafNode.Count + count) <= 120)
			{
				T[] array = new T[120];
				Array.Copy(items, 0, array, 0, index);
				Array.Copy(leafNode.items, 0, array, index, leafNode.Count);
				Array.Copy(items, index, array, index + leafNode.Count, count - index);
				return new LeafNode(num, array);
			}
			if (index == 0)
			{
				return Prepend(node, nodeIsUnused);
			}
			if (index == count)
			{
				return Append(node, nodeIsUnused);
			}
			T[] array2 = new T[index];
			Array.Copy(items, 0, array2, 0, index);
			LeafNode leafNode2 = new LeafNode(index, array2);
			T[] array3 = new T[count - index];
			Array.Copy(items, index, array3, 0, count - index);
			Node node2 = new LeafNode(count - index, array3);
			return leafNode2.AppendInPlace(node, nodeIsUnused).AppendInPlace(node2, nodeIsUnused: true);
		}

		public override Node RemoveRangeInPlace(int first, int last)
		{
			if (shared)
			{
				return RemoveRange(first, last);
			}
			if (first <= 0 && last >= count - 1)
			{
				return null;
			}
			if (first < 0)
			{
				first = 0;
			}
			if (last >= count)
			{
				last = count - 1;
			}
			int num = first + (count - last - 1);
			if (count > last + 1)
			{
				Array.Copy(items, last + 1, items, first, count - last - 1);
			}
			for (int i = num; i < count; i++)
			{
				items[i] = default;
			}
			count = num;
			return this;
		}

		public override Node RemoveRange(int first, int last)
		{
			if (first <= 0 && last >= count - 1)
			{
				return null;
			}
			if (first < 0)
			{
				first = 0;
			}
			if (last >= count)
			{
				last = count - 1;
			}
			int num = first + (count - last - 1);
			T[] array = new T[num];
			if (first > 0)
			{
				Array.Copy(items, 0, array, 0, first);
			}
			if (count > last + 1)
			{
				Array.Copy(items, last + 1, array, first, count - last - 1);
			}
			return new LeafNode(num, array);
		}

		public override Node Subrange(int first, int last)
		{
			if (first <= 0 && last >= count - 1)
			{
				MarkShared();
				return this;
			}
			if (first < 0)
			{
				first = 0;
			}
			if (last >= count)
			{
				last = count - 1;
			}
			int num = last - first + 1;
			T[] array = new T[num];
			Array.Copy(items, first, array, 0, num);
			return new LeafNode(num, array);
		}
	}

	[Serializable]
	private sealed class ConcatNode : Node
	{
		public Node left;

		public Node right;

		private short depth;

		public override int Depth => depth;

		public ConcatNode(Node left, Node right)
		{
			this.left = left;
			this.right = right;
			count = left.Count + right.Count;
			if (left.Depth > right.Depth)
			{
				depth = (short)(left.Depth + 1);
			}
			else
			{
				depth = (short)(right.Depth + 1);
			}
		}

		private Node NewNode(Node newLeft, Node newRight)
		{
			if (left == newLeft)
			{
				if (right == newRight)
				{
					MarkShared();
					return this;
				}
				left.MarkShared();
			}
			else if (right == newRight)
			{
				right.MarkShared();
			}
			if (newLeft == null)
			{
				return newRight;
			}
			if (newRight == null)
			{
				return newLeft;
			}
			return new ConcatNode(newLeft, newRight);
		}

		private Node NewNodeInPlace(Node newLeft, Node newRight)
		{
			if (newLeft == null)
			{
				return newRight;
			}
			if (newRight == null)
			{
				return newLeft;
			}
			left = newLeft;
			right = newRight;
			count = left.Count + right.Count;
			if (left.Depth > right.Depth)
			{
				depth = (short)(left.Depth + 1);
			}
			else
			{
				depth = (short)(right.Depth + 1);
			}
			return this;
		}

		public override T GetAt(int index)
		{
			int num = left.Count;
			if (index < num)
			{
				return left.GetAt(index);
			}
			return right.GetAt(index - num);
		}

		public override Node SetAtInPlace(int index, T item)
		{
			if (shared)
			{
				return SetAt(index, item);
			}
			int num = left.Count;
			if (index < num)
			{
				Node node = left.SetAtInPlace(index, item);
				if (node != left)
				{
					return NewNodeInPlace(node, right);
				}
				return this;
			}
			Node node2 = right.SetAtInPlace(index - num, item);
			if (node2 != right)
			{
				return NewNodeInPlace(left, node2);
			}
			return this;
		}

		public override Node SetAt(int index, T item)
		{
			int num = left.Count;
			if (index < num)
			{
				return NewNode(left.SetAt(index, item), right);
			}
			return NewNode(left, right.SetAt(index - num, item));
		}

		public override Node PrependInPlace(T item)
		{
			if (shared)
			{
				return Prepend(new LeafNode(item), nodeIsUnused: true);
			}
			if (left.Count < 120 && !left.Shared && left is LeafNode { Count: var num } leafNode)
			{
				if (num == leafNode.items.Length)
				{
					T[] array = new T[120];
					Array.Copy(leafNode.items, 0, array, 1, num);
					leafNode.items = array;
				}
				else
				{
					Array.Copy(leafNode.items, 0, leafNode.items, 1, num);
				}
				leafNode.items[0] = item;
				leafNode.count++;
				count++;
				return this;
			}
			return new ConcatNode(new LeafNode(item), this);
		}

		public override Node AppendInPlace(T item)
		{
			if (shared)
			{
				return Append(new LeafNode(item), nodeIsUnused: true);
			}
			if (right.Count < 120 && !right.Shared && right is LeafNode { Count: var num } leafNode)
			{
				if (num == leafNode.items.Length)
				{
					T[] array = new T[120];
					Array.Copy(leafNode.items, 0, array, 0, num);
					leafNode.items = array;
				}
				leafNode.items[num] = item;
				leafNode.count++;
				count++;
				return this;
			}
			return new ConcatNode(this, new LeafNode(item));
		}

		public override Node AppendInPlace(Node node, bool nodeIsUnused)
		{
			if (shared)
			{
				return Append(node, nodeIsUnused);
			}
			if (right.Count + node.Count <= 120 && right is LeafNode && node is LeafNode)
			{
				return NewNodeInPlace(left, right.AppendInPlace(node, nodeIsUnused));
			}
			if (!nodeIsUnused)
			{
				node.MarkShared();
			}
			return new ConcatNode(this, node);
		}

		public override Node Append(Node node, bool nodeIsUnused)
		{
			if (right.Count + node.Count <= 120 && right is LeafNode && node is LeafNode)
			{
				return NewNode(left, right.Append(node, nodeIsUnused));
			}
			MarkShared();
			if (!nodeIsUnused)
			{
				node.MarkShared();
			}
			return new ConcatNode(this, node);
		}

		public override Node InsertInPlace(int index, T item)
		{
			if (shared)
			{
				return Insert(index, new LeafNode(item), nodeIsUnused: true);
			}
			int num = left.Count;
			if (index <= num)
			{
				return NewNodeInPlace(left.InsertInPlace(index, item), right);
			}
			return NewNodeInPlace(left, right.InsertInPlace(index - num, item));
		}

		public override Node InsertInPlace(int index, Node node, bool nodeIsUnused)
		{
			if (shared)
			{
				return Insert(index, node, nodeIsUnused);
			}
			int num = left.Count;
			if (index < num)
			{
				return NewNodeInPlace(left.InsertInPlace(index, node, nodeIsUnused), right);
			}
			return NewNodeInPlace(left, right.InsertInPlace(index - num, node, nodeIsUnused));
		}

		public override Node Insert(int index, Node node, bool nodeIsUnused)
		{
			int num = left.Count;
			if (index < num)
			{
				return NewNode(left.Insert(index, node, nodeIsUnused), right);
			}
			return NewNode(left, right.Insert(index - num, node, nodeIsUnused));
		}

		public override Node RemoveRangeInPlace(int first, int last)
		{
			if (shared)
			{
				return RemoveRange(first, last);
			}
			if (first <= 0 && last >= count - 1)
			{
				return null;
			}
			int num = left.Count;
			Node newLeft = left;
			Node newRight = right;
			if (first < num)
			{
				newLeft = left.RemoveRangeInPlace(first, last);
			}
			if (last >= num)
			{
				newRight = right.RemoveRangeInPlace(first - num, last - num);
			}
			return NewNodeInPlace(newLeft, newRight);
		}

		public override Node RemoveRange(int first, int last)
		{
			if (first <= 0 && last >= count - 1)
			{
				return null;
			}
			int num = left.Count;
			Node newLeft = left;
			Node newRight = right;
			if (first < num)
			{
				newLeft = left.RemoveRange(first, last);
			}
			if (last >= num)
			{
				newRight = right.RemoveRange(first - num, last - num);
			}
			return NewNode(newLeft, newRight);
		}

		public override Node Subrange(int first, int last)
		{
			if (first <= 0 && last >= count - 1)
			{
				MarkShared();
				return this;
			}
			int num = left.Count;
			Node node = null;
			Node node2 = null;
			if (first < num)
			{
				node = left.Subrange(first, last);
			}
			if (last >= num)
			{
				node2 = right.Subrange(first - num, last - num);
			}
			if (node == null)
			{
				return node2;
			}
			if (node2 == null)
			{
				return node;
			}
			return new ConcatNode(node, node2);
		}
	}

	[Serializable]
	private class BigListRange : ListBase<T>
	{
		private BigList<T> wrappedList;

		private int start;

		private int count;

		public override int Count => Math.Min(count, wrappedList.Count - start);

		public override T this[int index]
		{
			get
			{
				if (index < 0 || index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return wrappedList[start + index];
			}
			set
			{
				if (index < 0 || index >= count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				wrappedList[start + index] = value;
			}
		}

		public BigListRange(BigList<T> wrappedList, int start, int count)
		{
			this.wrappedList = wrappedList;
			this.start = start;
			this.count = count;
		}

		public override void Clear()
		{
			if (wrappedList.Count - start < count)
			{
				count = wrappedList.Count - start;
			}
			while (count > 0)
			{
				wrappedList.RemoveAt(start + count - 1);
				count--;
			}
		}

		public override void Insert(int index, T item)
		{
			if (index < 0 || index > count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			wrappedList.Insert(start + index, item);
			count++;
		}

		public override void RemoveAt(int index)
		{
			if (index < 0 || index >= count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			wrappedList.RemoveAt(start + index);
			count--;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			return wrappedList.GetEnumerator(start, count);
		}
	}

	private const uint MAXITEMS = 2147483646u;

	private const int MAXLEAF = 120;

	private const int BALANCEFACTOR = 6;

	private static readonly int[] FIBONACCI = new int[46]
	{
		1, 2, 3, 5, 8, 13, 21, 34, 55, 89,
		144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946,
		17711, 28657, 46368, 75025, 121393, 196418, 317811, 514229, 832040, 1346269,
		2178309, 3524578, 5702887, 9227465, 14930352, 24157817, 39088169, 63245986, 102334155, 165580141,
		267914296, 433494437, 701408733, 1134903170, 1836311903, 2147483647
	};

	private const int MAXFIB = 44;

	private Node root;

	private int changeStamp;

	public sealed override int Count
	{
		get
		{
			if (root == null)
			{
				return 0;
			}
			return root.Count;
		}
	}

	public sealed override T this[int index]
	{
		get
		{
			if (root == null || index < 0 || index >= root.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			Node node = root;
			for (ConcatNode concatNode = node as ConcatNode; concatNode != null; concatNode = node as ConcatNode)
			{
				int count = concatNode.left.Count;
				if (index < count)
				{
					node = concatNode.left;
				}
				else
				{
					node = concatNode.right;
					index -= count;
				}
			}
			return ((LeafNode)node).items[index];
		}
		set
		{
			if (root == null || index < 0 || index >= root.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			StopEnumerations();
			if (root.Shared)
			{
				root = root.SetAt(index, value);
			}
			Node node = root;
			for (ConcatNode concatNode = node as ConcatNode; concatNode != null; concatNode = node as ConcatNode)
			{
				int count = concatNode.left.Count;
				if (index < count)
				{
					node = concatNode.left;
					if (node.Shared)
					{
						concatNode.left = node.SetAt(index, value);
						return;
					}
				}
				else
				{
					node = concatNode.right;
					index -= count;
					if (node.Shared)
					{
						concatNode.right = node.SetAt(index, value);
						return;
					}
				}
			}
			((LeafNode)node).items[index] = value;
		}
	}

	private void StopEnumerations()
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

	public BigList()
	{
		root = null;
	}

	public BigList(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		root = NodeFromEnumerable(collection);
		CheckBalance();
	}

	public BigList(IEnumerable<T> collection, int copies)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		root = NCopiesOfNode(copies, NodeFromEnumerable(collection));
		CheckBalance();
	}

	public BigList(BigList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list.root == null)
		{
			root = null;
			return;
		}
		list.root.MarkShared();
		root = list.root;
	}

	public BigList(BigList<T> list, int copies)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (list.root == null)
		{
			root = null;
			return;
		}
		list.root.MarkShared();
		root = NCopiesOfNode(copies, list.root);
	}

	private BigList(Node node)
	{
		root = node;
		CheckBalance();
	}

	public sealed override void Clear()
	{
		StopEnumerations();
		root = null;
	}

	public sealed override void Insert(int index, T item)
	{
		StopEnumerations();
		if ((uint)(Count + 1) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		if (index <= 0 || index >= Count)
		{
			if (index == 0)
			{
				AddToFront(item);
				return;
			}
			if (index != Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			Add(item);
		}
		else if (root == null)
		{
			root = new LeafNode(item);
		}
		else
		{
			Node node = root.InsertInPlace(index, item);
			if (node != root)
			{
				root = node;
				CheckBalance();
			}
		}
	}

	public void InsertRange(int index, IEnumerable<T> collection)
	{
		StopEnumerations();
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (index <= 0 || index >= Count)
		{
			if (index == 0)
			{
				AddRangeToFront(collection);
				return;
			}
			if (index == Count)
			{
				AddRange(collection);
				return;
			}
			throw new ArgumentOutOfRangeException("index");
		}
		Node node = NodeFromEnumerable(collection);
		if (node == null)
		{
			return;
		}
		if (root == null)
		{
			root = node;
			return;
		}
		if ((uint)(Count + node.Count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		Node node2 = root.InsertInPlace(index, node, nodeIsUnused: true);
		if (node2 != root)
		{
			root = node2;
			CheckBalance();
		}
	}

	public void InsertRange(int index, BigList<T> list)
	{
		StopEnumerations();
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if ((uint)(Count + list.Count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		if (index <= 0 || index >= Count)
		{
			if (index == 0)
			{
				AddRangeToFront(list);
				return;
			}
			if (index != Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			AddRange(list);
		}
		else
		{
			if (list.Count == 0)
			{
				return;
			}
			if (root == null)
			{
				list.root.MarkShared();
				root = list.root;
				return;
			}
			if (list.root == root)
			{
				root.MarkShared();
			}
			Node node = root.InsertInPlace(index, list.root, nodeIsUnused: false);
			if (node != root)
			{
				root = node;
				CheckBalance();
			}
		}
	}

	public sealed override void RemoveAt(int index)
	{
		RemoveRange(index, 1);
	}

	public void RemoveRange(int index, int count)
	{
		if (count != 0)
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (count < 0 || count > Count - index)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			StopEnumerations();
			Node node = root.RemoveRangeInPlace(index, index + count - 1);
			if (node != root)
			{
				root = node;
				CheckBalance();
			}
		}
	}

	public sealed override void Add(T item)
	{
		if ((uint)(Count + 1) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		StopEnumerations();
		if (root == null)
		{
			root = new LeafNode(item);
			return;
		}
		Node node = root.AppendInPlace(item);
		if (node != root)
		{
			root = node;
			CheckBalance();
		}
	}

	public void AddToFront(T item)
	{
		if ((uint)(Count + 1) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		StopEnumerations();
		if (root == null)
		{
			root = new LeafNode(item);
			return;
		}
		Node node = root.PrependInPlace(item);
		if (node != root)
		{
			root = node;
			CheckBalance();
		}
	}

	public void AddRange(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		StopEnumerations();
		Node node = NodeFromEnumerable(collection);
		if (node == null)
		{
			return;
		}
		if (root == null)
		{
			root = node;
			CheckBalance();
			return;
		}
		if ((uint)(Count + node.count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		Node node2 = root.AppendInPlace(node, nodeIsUnused: true);
		if (node2 != root)
		{
			root = node2;
			CheckBalance();
		}
	}

	public void AddRangeToFront(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		StopEnumerations();
		Node node = NodeFromEnumerable(collection);
		if (node == null)
		{
			return;
		}
		if (root == null)
		{
			root = node;
			CheckBalance();
			return;
		}
		if ((uint)(Count + node.Count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		Node node2 = root.PrependInPlace(node, nodeIsUnused: true);
		if (node2 != root)
		{
			root = node2;
			CheckBalance();
		}
	}

	public BigList<T> Clone()
	{
		if (root == null)
		{
			return new BigList<T>();
		}
		root.MarkShared();
		return new BigList<T>(root);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public BigList<T> CloneContents()
	{
		if (root == null)
		{
			return new BigList<T>();
		}
		if (!Util.IsCloneableType(typeof(T), out var isValue))
		{
			throw new InvalidOperationException(string.Format(Strings.TypeNotCloneable, typeof(T).FullName));
		}
		if (isValue)
		{
			return Clone();
		}
		return new BigList<T>(Algorithms.Convert(this, (T item) => (item == null) ? default(T) : ((T)((ICloneable)(object)item).Clone())));
	}

	public void AddRange(BigList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if ((uint)(Count + list.Count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		if (list.Count == 0)
		{
			return;
		}
		StopEnumerations();
		if (root == null)
		{
			list.root.MarkShared();
			root = list.root;
			return;
		}
		Node node = root.AppendInPlace(list.root, nodeIsUnused: false);
		if (node != root)
		{
			root = node;
			CheckBalance();
		}
	}

	public void AddRangeToFront(BigList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if ((uint)(Count + list.Count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		if (list.Count == 0)
		{
			return;
		}
		StopEnumerations();
		if (root == null)
		{
			list.root.MarkShared();
			root = list.root;
			return;
		}
		Node node = root.PrependInPlace(list.root, nodeIsUnused: false);
		if (node != root)
		{
			root = node;
			CheckBalance();
		}
	}

	public static BigList<T> operator +(BigList<T> first, BigList<T> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if ((uint)(first.Count + second.Count) > 2147483646u)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		if (first.Count == 0)
		{
			return second.Clone();
		}
		if (second.Count == 0)
		{
			return first.Clone();
		}
		BigList<T> bigList = new BigList<T>(first.root.Append(second.root, nodeIsUnused: false));
		bigList.CheckBalance();
		return bigList;
	}

	public BigList<T> GetRange(int index, int count)
	{
		if (count == 0)
		{
			return new BigList<T>();
		}
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || count > Count - index)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return new BigList<T>(root.Subrange(index, index + count - 1));
	}

	public sealed override IList<T> Range(int index, int count)
	{
		if (index < 0 || index > Count || (index == Count && count != 0))
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || count > Count || count + index > Count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return new BigListRange(this, index, count);
	}

	private IEnumerator<T> GetEnumerator(int start, int maxItems)
	{
		int startStamp = changeStamp;
		if (root == null || maxItems <= 0)
		{
			yield break;
		}
		ConcatNode[] stack = new ConcatNode[root.Depth];
		bool[] leftStack = new bool[root.Depth];
		int stackPtr = 0;
		int startIndex = 0;
		Node node = root;
		if (start != 0)
		{
			if (start < 0 || start >= root.Count)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			ConcatNode concatNode = node as ConcatNode;
			startIndex = start;
			while (concatNode != null)
			{
				stack[stackPtr] = concatNode;
				int count = concatNode.left.Count;
				if (startIndex < count)
				{
					leftStack[stackPtr] = true;
					node = concatNode.left;
				}
				else
				{
					leftStack[stackPtr] = false;
					node = concatNode.right;
					startIndex -= count;
				}
				int num = stackPtr + 1;
				stackPtr = num;
				concatNode = node as ConcatNode;
			}
		}
		while (true)
		{
			int num;
			if (node is ConcatNode concatNode2)
			{
				stack[stackPtr] = concatNode2;
				leftStack[stackPtr] = true;
				num = stackPtr + 1;
				stackPtr = num;
				node = concatNode2.left;
				continue;
			}
			LeafNode currentLeaf = (LeafNode)node;
			int limit = currentLeaf.Count;
			if (limit > startIndex + maxItems)
			{
				limit = startIndex + maxItems;
			}
			for (int i = startIndex; i < limit; i = num)
			{
				yield return currentLeaf.items[i];
				CheckEnumerationStamp(startStamp);
				num = i + 1;
			}
			maxItems -= limit - startIndex;
			if (maxItems <= 0)
			{
				break;
			}
			startIndex = 0;
			ConcatNode concatNode3;
			do
			{
				if (stackPtr == 0)
				{
					yield break;
				}
				num = stackPtr - 1;
				stackPtr = num;
				concatNode3 = stack[num];
			}
			while (!leftStack[stackPtr]);
			leftStack[stackPtr] = false;
			num = stackPtr + 1;
			stackPtr = num;
			node = concatNode3.right;
		}
	}

	public sealed override IEnumerator<T> GetEnumerator()
	{
		return GetEnumerator(0, int.MaxValue);
	}

	private Node NodeFromEnumerable(IEnumerable<T> collection)
	{
		Node node = null;
		IEnumerator<T> enumerator = collection.GetEnumerator();
		LeafNode leafNode;
		while ((leafNode = LeafFromEnumerator(enumerator)) != null)
		{
			if (node == null)
			{
				node = leafNode;
				continue;
			}
			if ((uint)(node.count + leafNode.count) > 2147483646u)
			{
				throw new InvalidOperationException(Strings.CollectionTooLarge);
			}
			node = node.AppendInPlace(leafNode, nodeIsUnused: true);
		}
		return node;
	}

	private LeafNode LeafFromEnumerator(IEnumerator<T> enumerator)
	{
		int num = 0;
		T[] array = null;
		while (num < 120 && enumerator.MoveNext())
		{
			if (num == 0)
			{
				array = new T[120];
			}
			array[num++] = enumerator.Current;
		}
		if (array != null)
		{
			return new LeafNode(num, array);
		}
		return null;
	}

	private Node NCopiesOfNode(int copies, Node node)
	{
		if (copies < 0)
		{
			throw new ArgumentOutOfRangeException("copies", Strings.ArgMustNotBeNegative);
		}
		if (copies == 0 || node == null)
		{
			return null;
		}
		if (copies == 1)
		{
			return node;
		}
		if ((long)copies * (long)node.count > 2147483646)
		{
			throw new InvalidOperationException(Strings.CollectionTooLarge);
		}
		int num = 1;
		Node node2 = node;
		Node node3 = null;
		while (copies > 0)
		{
			node2.MarkShared();
			if ((copies & num) != 0)
			{
				copies -= num;
				node3 = ((node3 != null) ? node3.Append(node2, nodeIsUnused: false) : node2);
			}
			num *= 2;
			node2 = node2.Append(node2, nodeIsUnused: false);
		}
		return node3;
	}

	private void CheckBalance()
	{
		if (root != null && root.Depth > 6 && (root.Depth - 6 > 44 || Count < FIBONACCI[root.Depth - 6]))
		{
			Rebalance();
		}
	}

	internal void Rebalance()
	{
		if (root == null || root.Depth <= 1 || (root.Depth - 2 <= 44 && Count >= FIBONACCI[root.Depth - 2]))
		{
			return;
		}
		int i;
		for (i = 0; i <= 44 && root.Count >= FIBONACCI[i]; i++)
		{
		}
		Node[] array = new Node[i];
		AddNodeToRebalanceArray(array, root, shared: false);
		Node node = null;
		for (int j = 0; j < i; j++)
		{
			Node node2 = array[j];
			if (node2 != null)
			{
				node = ((node != null) ? node.PrependInPlace(node2, !node2.Shared) : node2);
			}
		}
		root = node;
	}

	private void AddNodeToRebalanceArray(Node[] rebalanceArray, Node node, bool shared)
	{
		if (node.Shared)
		{
			shared = true;
		}
		if (node.IsBalanced())
		{
			if (shared)
			{
				node.MarkShared();
			}
			AddBalancedNodeToRebalanceArray(rebalanceArray, node);
		}
		else
		{
			ConcatNode concatNode = (ConcatNode)node;
			AddNodeToRebalanceArray(rebalanceArray, concatNode.left, shared);
			AddNodeToRebalanceArray(rebalanceArray, concatNode.right, shared);
		}
	}

	private void AddBalancedNodeToRebalanceArray(Node[] rebalanceArray, Node balancedNode)
	{
		Node node = null;
		int count = balancedNode.Count;
		int i;
		for (i = 0; count >= FIBONACCI[i + 1]; i++)
		{
			Node node2 = rebalanceArray[i];
			if (node2 != null)
			{
				rebalanceArray[i] = null;
				node = ((node != null) ? node.PrependInPlace(node2, !node2.Shared) : node2);
			}
		}
		if (node != null)
		{
			balancedNode = balancedNode.PrependInPlace(node, !node.Shared);
		}
		while (true)
		{
			Node node3 = rebalanceArray[i];
			if (node3 != null)
			{
				rebalanceArray[i] = null;
				balancedNode = balancedNode.PrependInPlace(node3, !node3.Shared);
			}
			if (balancedNode.Count < FIBONACCI[i + 1])
			{
				break;
			}
			i++;
		}
		rebalanceArray[i] = balancedNode;
	}

	public new BigList<TDest> ConvertAll<TDest>(Converter<T, TDest> converter)
	{
		return new BigList<TDest>(Algorithms.Convert(this, converter));
	}

	public void Reverse()
	{
		Algorithms.ReverseInPlace(this);
	}

	public void Reverse(int start, int count)
	{
		Algorithms.ReverseInPlace(Range(start, count));
	}

	public void Sort()
	{
		Sort(Comparers.DefaultComparer<T>());
	}

	public void Sort(IComparer<T> comparer)
	{
		Algorithms.SortInPlace(this, comparer);
	}

	public void Sort(Comparison<T> comparison)
	{
		Sort(Comparers.ComparerFromComparison(comparison));
	}

	public int BinarySearch(T item)
	{
		return BinarySearch(item, Comparers.DefaultComparer<T>());
	}

	public int BinarySearch(T item, IComparer<T> comparer)
	{
		if (Algorithms.BinarySearch(this, item, comparer, out var index) == 0)
		{
			return ~index;
		}
		return index;
	}

	public int BinarySearch(T item, Comparison<T> comparison)
	{
		return BinarySearch(item, Comparers.ComparerFromComparison(comparison));
	}
}
