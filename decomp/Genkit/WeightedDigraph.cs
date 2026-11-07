using System;
using System.Collections.Generic;
using XRL.Collections;

namespace Genkit;

public class WeightedDigraph
{
	private class Vertex
	{
		public int Target = -1;

		public Rack<Arc> Arcs = new Rack<Arc>();

		public ulong OutgoingWeight;

		public ulong IncomingWeight;
	}

	private struct Arc(int Index, uint Weight)
	{
		public int Index = Index;

		public uint Weight = Weight;
	}

	private class CircularGraphException : Exception
	{
		public RingDeque<int> Stack = new RingDeque<int>();

		public CircularGraphException(int Index)
		{
			Stack.Enqueue(Index);
		}
	}

	private Vertex[] Vertices;

	private int Length;

	public WeightedDigraph(int Length)
	{
		Vertices = new Vertex[Length];
		this.Length = Length;
		for (int i = 0; i < Length; i++)
		{
			Vertices[i] = new Vertex();
		}
	}

	public void AddArc(int From, int To, uint Weight)
	{
		Vertices[To].Arcs.Add(new Arc(From, Weight));
		Vertices[From].OutgoingWeight += Weight;
		Vertices[To].IncomingWeight += Weight;
	}

	public int GetTarget(int Index)
	{
		return Vertices[Index].Target;
	}

	public void Resolve()
	{
		int i = 0;
		int num = 0;
		for (; i < Length; i++)
		{
			ulong num2 = ulong.MaxValue;
			ulong num3 = 0uL;
			for (int j = 0; j < Length; j++)
			{
				Vertex vertex = Vertices[j];
				if (vertex.Target == -1)
				{
					ulong outgoingWeight = vertex.OutgoingWeight;
					if (outgoingWeight == 0L)
					{
						num = j;
						break;
					}
					ulong incomingWeight = vertex.IncomingWeight;
					if (num3 == 0L || (incomingWeight != 0 && (outgoingWeight < num2 || (outgoingWeight == num2 && incomingWeight > num3))))
					{
						num = j;
						num2 = outgoingWeight;
						num3 = incomingWeight;
					}
				}
			}
			Vertices[num].Target = i;
			foreach (Arc arc in Vertices[num].Arcs)
			{
				Vertices[arc.Index].OutgoingWeight -= arc.Weight;
			}
		}
	}

	public IEnumerable<RingDeque<int>> YieldCycles(uint MinWeight)
	{
		for (int v = 0; v < Length; v++)
		{
			RingDeque<int> ringDeque = null;
			try
			{
				Visit(v, Vertices, MinWeight);
			}
			catch (CircularGraphException ex)
			{
				ringDeque = ex.Stack;
			}
			if (ringDeque != null)
			{
				yield return ringDeque;
			}
		}
		static void Visit(int Index, Vertex[] Vertices, uint num)
		{
			Vertex vertex = Vertices[Index];
			int target = vertex.Target;
			if (target == int.MinValue)
			{
				throw new CircularGraphException(Index);
			}
			vertex.Target = int.MinValue;
			try
			{
				foreach (Arc arc in vertex.Arcs)
				{
					if (arc.Weight >= num)
					{
						try
						{
							Visit(arc.Index, Vertices, num);
						}
						catch (CircularGraphException ex2)
						{
							ex2.Stack.Enqueue(Index);
							throw;
						}
					}
				}
			}
			finally
			{
				vertex.Target = target;
			}
		}
	}
}
