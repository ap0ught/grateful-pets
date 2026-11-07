using System;
using System.Collections.Generic;
using System.Linq;
using FuzzySharp.Edits;

namespace FuzzySharp;

public static class Levenshtein
{
	private static EditOp[] GetEditOps<T>(T[] arr1, T[] arr2) where T : IEquatable<T>
	{
		return GetEditOps(arr1.Length, arr1, arr2.Length, arr2);
	}

	private static EditOp[] GetEditOps(string s1, string s2)
	{
		return GetEditOps(s1.Length, s1.ToCharArray(), s2.Length, s2.ToCharArray());
	}

	private static EditOp[] GetEditOps<T>(int len1, T[] c1, int len2, T[] c2) where T : IEquatable<T>
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (len1 > 0 && len2 > 0)
		{
			ref readonly T reference = ref c1[num];
			T other = c2[num2];
			if (!reference.Equals(other))
			{
				break;
			}
			len1--;
			len2--;
			num++;
			num2++;
			num3++;
		}
		int o = num3;
		while (len1 > 0 && len2 > 0)
		{
			ref readonly T reference2 = ref c1[num + len1 - 1];
			T other2 = c2[num2 + len2 - 1];
			if (!reference2.Equals(other2))
			{
				break;
			}
			len1--;
			len2--;
		}
		len1++;
		len2++;
		int[] array = new int[len2 * len1];
		for (int i = 0; i < len2; i++)
		{
			array[i] = i;
		}
		for (int i = 1; i < len1; i++)
		{
			array[len2 * i] = i;
		}
		for (int i = 1; i < len1; i++)
		{
			int num4 = (i - 1) * len2;
			int num5 = i * len2;
			int num6 = num5 + len2 - 1;
			T val = c1[num + i - 1];
			int num7 = num2;
			int num8 = i;
			num5++;
			while (num5 <= num6)
			{
				int num9 = array[num4++] + ((!val.Equals(c2[num7++])) ? 1 : 0);
				num8++;
				if (num8 > num9)
				{
					num8 = num9;
				}
				num9 = array[num4] + 1;
				if (num8 > num9)
				{
					num8 = num9;
				}
				array[num5++] = num8;
			}
		}
		return EditOpsFromCostMatrix(len1, c1, num, num3, len2, c2, num2, o, array);
	}

	private static EditOp[] EditOpsFromCostMatrix<T>(int len1, T[] c1, int p1, int o1, int len2, T[] c2, int p2, int o2, int[] matrix) where T : IEquatable<T>
	{
		int num = 0;
		int num2 = matrix[len1 * len2 - 1];
		EditOp[] array = new EditOp[num2];
		int num3 = len1 - 1;
		int num4 = len2 - 1;
		int num5 = len1 * len2 - 1;
		while (num3 > 0 || num4 > 0)
		{
			if (num3 != 0 && num4 != 0 && matrix[num5] == matrix[num5 - len2 - 1])
			{
				ref readonly T reference = ref c1[p1 + num3 - 1];
				T other = c2[p2 + num4 - 1];
				if (reference.Equals(other))
				{
					num3--;
					num4--;
					num5 -= len2 + 1;
					num = 0;
					continue;
				}
			}
			if (num < 0 && num4 != 0 && matrix[num5] == matrix[num5 - 1] + 1)
			{
				EditOp editOp = new EditOp();
				num2--;
				array[num2] = editOp;
				editOp.EditType = EditType.INSERT;
				editOp.SourcePos = num3 + o1;
				editOp.DestPos = --num4 + o2;
				num5--;
				continue;
			}
			if (num > 0 && num3 != 0 && matrix[num5] == matrix[num5 - len2] + 1)
			{
				EditOp editOp2 = new EditOp();
				num2--;
				array[num2] = editOp2;
				editOp2.EditType = EditType.DELETE;
				editOp2.SourcePos = --num3 + o1;
				editOp2.DestPos = num4 + o2;
				num5 -= len2;
				continue;
			}
			if (num3 != 0 && num4 != 0 && matrix[num5] == matrix[num5 - len2 - 1] + 1)
			{
				num2--;
				EditOp editOp3 = (array[num2] = new EditOp());
				editOp3.EditType = EditType.REPLACE;
				editOp3.SourcePos = --num3 + o1;
				editOp3.DestPos = --num4 + o2;
				num5 -= len2 + 1;
				num = 0;
				continue;
			}
			if (num == 0 && num4 != 0 && matrix[num5] == matrix[num5 - 1] + 1)
			{
				num2--;
				EditOp editOp4 = (array[num2] = new EditOp());
				editOp4.EditType = EditType.INSERT;
				editOp4.SourcePos = num3 + o1;
				editOp4.DestPos = --num4 + o2;
				num5--;
				num = -1;
				continue;
			}
			if (num == 0 && num3 != 0 && matrix[num5] == matrix[num5 - len2] + 1)
			{
				num2--;
				EditOp editOp5 = (array[num2] = new EditOp());
				editOp5.EditType = EditType.DELETE;
				editOp5.SourcePos = --num3 + o1;
				editOp5.DestPos = num4 + o2;
				num5 -= len2;
				num = 1;
				continue;
			}
			throw new InvalidOperationException("Cant calculate edit op");
		}
		return array;
	}

	public static MatchingBlock[] GetMatchingBlocks<T>(T[] s1, T[] s2) where T : IEquatable<T>
	{
		return GetMatchingBlocks(s1.Length, s2.Length, GetEditOps(s1, s2));
	}

	public static MatchingBlock[] GetMatchingBlocks(string s1, string s2)
	{
		return GetMatchingBlocks(s1.Length, s2.Length, GetEditOps(s1, s2));
	}

	public static MatchingBlock[] GetMatchingBlocks(int len1, int len2, OpCode[] ops)
	{
		int num = ops.Length;
		int num2 = 0;
		int num3 = 0;
		int num4 = num;
		while (num4-- != 0)
		{
			if (ops[num2].EditType == EditType.KEEP)
			{
				num3++;
				while (num4 != 0 && ops[num2].EditType == EditType.KEEP)
				{
					num4--;
					num2++;
				}
				if (num4 == 0)
				{
					break;
				}
			}
			num2++;
		}
		MatchingBlock[] array = new MatchingBlock[num3 + 1];
		int num5 = 0;
		num2 = 0;
		array[num5] = new MatchingBlock();
		num4 = num;
		while (num4 != 0)
		{
			if (ops[num2].EditType == EditType.KEEP)
			{
				array[num5].SourcePos = ops[num2].SourceBegin;
				array[num5].DestPos = ops[num2].DestBegin;
				while (num4 != 0 && ops[num2].EditType == EditType.KEEP)
				{
					num4--;
					num2++;
				}
				if (num4 == 0)
				{
					array[num5].Length = len1 - array[num5].SourcePos;
					num5++;
					break;
				}
				array[num5].Length = ops[num2].SourceBegin - array[num5].SourcePos;
				num5++;
				array[num5] = new MatchingBlock();
			}
			num4--;
			num2++;
		}
		MatchingBlock matchingBlock = new MatchingBlock
		{
			SourcePos = len1,
			DestPos = len2,
			Length = 0
		};
		array[num5] = matchingBlock;
		return array;
	}

	private static MatchingBlock[] GetMatchingBlocks(int len1, int len2, EditOp[] ops)
	{
		int num = ops.Length;
		int num2 = 0;
		int i = 0;
		int num4;
		int num3 = (num4 = 0);
		int num5 = num;
		while (num5 != 0)
		{
			for (; ops[i].EditType == EditType.KEEP; i++)
			{
				if (--num5 == 0)
				{
					break;
				}
			}
			if (num5 == 0)
			{
				break;
			}
			if (num3 < ops[i].SourcePos || num4 < ops[i].DestPos)
			{
				num2++;
				num3 = ops[i].SourcePos;
				num4 = ops[i].DestPos;
			}
			EditType editType = ops[i].EditType;
			switch (editType)
			{
			case EditType.REPLACE:
				do
				{
					num3++;
					num4++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.DELETE:
				do
				{
					num3++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.INSERT:
				break;
			default:
				continue;
			}
			do
			{
				num4++;
				num5--;
				i++;
			}
			while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
		}
		if (num3 < len1 || num4 < len2)
		{
			num2++;
		}
		MatchingBlock[] array = new MatchingBlock[num2 + 1];
		i = 0;
		num3 = (num4 = 0);
		int num6 = 0;
		num5 = num;
		while (num5 != 0)
		{
			for (; ops[i].EditType == EditType.KEEP; i++)
			{
				if (--num5 == 0)
				{
					break;
				}
			}
			if (num5 == 0)
			{
				break;
			}
			if (num3 < ops[i].SourcePos || num4 < ops[i].DestPos)
			{
				MatchingBlock matchingBlock = new MatchingBlock();
				matchingBlock.SourcePos = num3;
				matchingBlock.DestPos = num4;
				matchingBlock.Length = ops[i].SourcePos - num3;
				num3 = ops[i].SourcePos;
				num4 = ops[i].DestPos;
				array[num6++] = matchingBlock;
			}
			EditType editType = ops[i].EditType;
			switch (editType)
			{
			case EditType.REPLACE:
				do
				{
					num3++;
					num4++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.DELETE:
				do
				{
					num3++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.INSERT:
				break;
			default:
				continue;
			}
			do
			{
				num4++;
				num5--;
				i++;
			}
			while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
		}
		if (num3 < len1 || num4 < len2)
		{
			MatchingBlock matchingBlock2 = new MatchingBlock();
			matchingBlock2.SourcePos = num3;
			matchingBlock2.DestPos = num4;
			matchingBlock2.Length = len1 - num3;
			array[num6++] = matchingBlock2;
		}
		MatchingBlock matchingBlock3 = new MatchingBlock();
		matchingBlock3.SourcePos = len1;
		matchingBlock3.DestPos = len2;
		matchingBlock3.Length = 0;
		array[num6] = matchingBlock3;
		return array;
	}

	private static OpCode[] EditOpsToOpCodes(EditOp[] ops, int len1, int len2)
	{
		int num = ops.Length;
		int i = 0;
		int num2 = 0;
		int num4;
		int num3 = (num4 = 0);
		int num5 = num;
		EditType editType;
		while (num5 != 0)
		{
			for (; ops[i].EditType == EditType.KEEP; i++)
			{
				if (--num5 == 0)
				{
					break;
				}
			}
			if (num5 == 0)
			{
				break;
			}
			if (num3 < ops[i].SourcePos || num4 < ops[i].DestPos)
			{
				num2++;
				num3 = ops[i].SourcePos;
				num4 = ops[i].DestPos;
			}
			num2++;
			editType = ops[i].EditType;
			switch (editType)
			{
			case EditType.REPLACE:
				do
				{
					num3++;
					num4++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.DELETE:
				do
				{
					num3++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.INSERT:
				break;
			default:
				continue;
			}
			do
			{
				num4++;
				num5--;
				i++;
			}
			while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
		}
		if (num3 < len1 || num4 < len2)
		{
			num2++;
		}
		OpCode[] array = new OpCode[num2];
		i = 0;
		num3 = (num4 = 0);
		int j = 0;
		for (num5 = num; num5 != 0; array[j].EditType = editType, array[j].SourceEnd = num3, array[j].DestEnd = num4, j++)
		{
			for (; ops[i].EditType == EditType.KEEP; i++)
			{
				if (--num5 == 0)
				{
					break;
				}
			}
			if (num5 == 0)
			{
				break;
			}
			OpCode opCode = (array[j] = new OpCode());
			opCode.SourceBegin = num3;
			opCode.DestBegin = num4;
			if (num3 < ops[i].SourcePos || num4 < ops[i].DestPos)
			{
				opCode.EditType = EditType.KEEP;
				int num6 = (opCode.SourceEnd = ops[i].SourcePos);
				num3 = num6;
				num6 = (opCode.DestEnd = ops[i].DestPos);
				num4 = num6;
				j++;
				OpCode opCode2 = (array[j] = new OpCode());
				opCode2.SourceBegin = num3;
				opCode2.DestBegin = num4;
			}
			editType = ops[i].EditType;
			switch (editType)
			{
			case EditType.REPLACE:
				do
				{
					num3++;
					num4++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.DELETE:
				do
				{
					num3++;
					num5--;
					i++;
				}
				while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
				continue;
			case EditType.INSERT:
				break;
			default:
				continue;
			}
			do
			{
				num4++;
				num5--;
				i++;
			}
			while (num5 != 0 && ops[i].EditType == editType && num3 == ops[i].SourcePos && num4 == ops[i].DestPos);
		}
		if (num3 < len1 || num4 < len2)
		{
			if (array[j] == null)
			{
				array[j] = new OpCode();
			}
			array[j].EditType = EditType.KEEP;
			array[j].SourceBegin = num3;
			array[j].DestBegin = num4;
			array[j].SourceEnd = len1;
			array[j].DestEnd = len2;
			j++;
		}
		return array;
	}

	public static int EditDistance(string s1, string s2, int xcost = 0)
	{
		return EditDistance(s1.ToCharArray(), s2.ToCharArray(), xcost);
	}

	public static int EditDistance<T>(T[] c1, T[] c2, int xcost = 0) where T : IEquatable<T>
	{
		int num = 0;
		int num2 = 0;
		int num3 = c1.Length;
		int num4 = c2.Length;
		while (num3 > 0 && num4 > 0)
		{
			ref readonly T reference = ref c1[num];
			T other = c2[num2];
			if (reference.Equals(other))
			{
				num3--;
				num4--;
				num++;
				num2++;
				continue;
			}
			break;
		}
		while (num3 > 0 && num4 > 0)
		{
			ref readonly T reference2 = ref c1[num + num3 - 1];
			T other2 = c2[num2 + num4 - 1];
			if (!reference2.Equals(other2))
			{
				break;
			}
			num3--;
			num4--;
		}
		if (num3 == 0)
		{
			return num4;
		}
		if (num4 == 0)
		{
			return num3;
		}
		if (num3 > num4)
		{
			int num5 = num3;
			int num6 = num;
			num3 = num4;
			num4 = num5;
			num = num2;
			num2 = num6;
			T[] array = c2;
			c2 = c1;
			c1 = array;
		}
		if (num3 == 1)
		{
			if (xcost != 0)
			{
				return num4 + 1 - 2 * Memchr(c2, num2, c1[num], num4);
			}
			return num4 - Memchr(c2, num2, c1[num], num4);
		}
		num3++;
		num4++;
		int num7 = num3 >> 1;
		int[] array2 = new int[num4];
		int num8 = num4 - 1;
		for (int i = 0; i < num4 - ((xcost == 0) ? num7 : 0); i++)
		{
			array2[i] = i;
		}
		if (xcost != 0)
		{
			for (int i = 1; i < num3; i++)
			{
				int num9 = 1;
				T val = c1[num + i - 1];
				int num10 = num2;
				int num11 = i;
				int num12 = i;
				while (num9 <= num8)
				{
					num12 = ((!val.Equals(c2[num10++])) ? (num12 + 1) : (--num11));
					num11 = array2[num9];
					num11++;
					if (num12 > num11)
					{
						num12 = num11;
					}
					array2[num9++] = num12;
				}
			}
		}
		else
		{
			array2[0] = num3 - num7 - 1;
			for (int i = 1; i < num3; i++)
			{
				T val2 = c1[num + i - 1];
				int num14;
				int num15;
				int num19;
				int num18;
				if (i >= num3 - num7)
				{
					int num13 = i - (num3 - num7);
					num14 = num2 + num13;
					num15 = num13;
					int num16 = array2[num15++];
					T other3 = c2[num14++];
					int num17 = num16 + ((!val2.Equals(other3)) ? 1 : 0);
					num18 = array2[num15];
					num18++;
					num19 = num18;
					if (num18 > num17)
					{
						num18 = num17;
					}
					array2[num15++] = num18;
				}
				else
				{
					num15 = 1;
					num14 = num2;
					num19 = (num18 = i);
				}
				if (i <= num7 + 1)
				{
					num8 = num4 + i - num7 - 2;
				}
				while (num15 <= num8)
				{
					int num20 = --num19;
					T other4 = c2[num14++];
					int num21 = num20 + ((!val2.Equals(other4)) ? 1 : 0);
					num18++;
					if (num18 > num21)
					{
						num18 = num21;
					}
					num19 = array2[num15];
					num19++;
					if (num18 > num19)
					{
						num18 = num19;
					}
					array2[num15++] = num18;
				}
				if (i <= num7)
				{
					int num22 = --num19;
					T other5 = c2[num14];
					int num23 = num22 + ((!val2.Equals(other5)) ? 1 : 0);
					num18++;
					if (num18 > num23)
					{
						num18 = num23;
					}
					array2[num15] = num18;
				}
			}
		}
		return array2[num8];
	}

	private static int Memchr<T>(T[] haystack, int offset, T needle, int num) where T : IEquatable<T>
	{
		if (num != 0)
		{
			int num2 = 0;
			do
			{
				if (haystack[offset + num2].Equals(needle))
				{
					return 1;
				}
				num2++;
			}
			while (--num != 0);
		}
		return 0;
	}

	public static double GetRatio<T>(T[] input1, T[] input2) where T : IEquatable<T>
	{
		int num = input1.Length;
		int num2 = input2.Length;
		int num3 = num + num2;
		int num4 = EditDistance(input1, input2, 1);
		if (num4 != 0)
		{
			return (double)(num3 - num4) / (double)num3;
		}
		return 1.0;
	}

	public static double GetRatio<T>(IEnumerable<T> input1, IEnumerable<T> input2) where T : IEquatable<T>
	{
		T[] array = input1.ToArray();
		T[] array2 = input2.ToArray();
		int num = array.Length;
		int num2 = array2.Length;
		int num3 = num + num2;
		int num4 = EditDistance(array, array2, 1);
		if (num4 != 0)
		{
			return (double)(num3 - num4) / (double)num3;
		}
		return 1.0;
	}

	public static double GetRatio(string s1, string s2)
	{
		return GetRatio(s1.ToCharArray(), s2.ToCharArray());
	}
}
