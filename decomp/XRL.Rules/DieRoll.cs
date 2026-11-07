using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.World;

namespace XRL.Rules;

[Serializable]
public class DieRoll
{
	public const int TYPE_UNKNOWN = 0;

	public const int TYPE_DIE = 1;

	public const int TYPE_RANGE = 2;

	public const int TYPE_CONSTANT = 3;

	public const int TYPE_ADD = 4;

	public const int TYPE_SUBTRACT = 5;

	public const int TYPE_MULTIPLY = 6;

	public const int TYPE_DIVIDE = 7;

	public const int TYPE_NEGATE = 8;

	public const int TYPE_ALTERNATE = 9;

	public int Type;

	public int LeftValue;

	public int RightValue;

	public DieRoll Left;

	public DieRoll Right;

	public List<DieRoll> Set;

	public string Channel;

	public DieRoll()
	{
	}

	public DieRoll(DieRoll source)
	{
		Type = source.Type;
		LeftValue = source.LeftValue;
		RightValue = source.RightValue;
		if (source.Left != null)
		{
			Left = new DieRoll(source.Left);
		}
		if (source.Right != null)
		{
			Right = new DieRoll(source.Right);
		}
		if (source.Set != null)
		{
			Set = new List<DieRoll>(source.Set);
		}
	}

	public DieRoll(string Dice)
		: this()
	{
		Parse(Dice);
	}

	public DieRoll(string Dice, out bool AnyUnknown)
		: this()
	{
		Parse(Dice, out AnyUnknown);
	}

	public DieRoll(int Type, int Left, int Right)
		: this()
	{
		this.Type = Type;
		LeftValue = Left;
		RightValue = Right;
	}

	public DieRoll(int Type, DieRoll Left, DieRoll Right)
		: this()
	{
		this.Type = Type;
		this.Left = Left;
		this.Right = Right;
	}

	public DieRoll(int Type, int Left, DieRoll Right)
		: this()
	{
		this.Type = Type;
		LeftValue = Left;
		this.Right = Right;
	}

	public DieRoll(int Type, DieRoll Left, int Right)
		: this()
	{
		this.Type = Type;
		this.Left = Left;
		RightValue = Right;
	}

	public DieRoll(int Type, int Left, string Right)
		: this()
	{
		this.Type = Type;
		LeftValue = Left;
		this.Right = new DieRoll(Right);
	}

	public DieRoll(int Type, string Left, int Right)
		: this()
	{
		this.Type = Type;
		this.Left = new DieRoll(Left);
		RightValue = Right;
	}

	public int Generate(int Low, int High)
	{
		return Stat.GlobalChannelRandom(Channel, Low, High);
	}

	public int Resolve()
	{
		switch (Type)
		{
		case 0:
			return 0;
		case 1:
		{
			int num = ((Left == null) ? LeftValue : Left.Resolve());
			int high = ((Right == null) ? RightValue : Right.Resolve());
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += Generate(1, high);
			}
			return num2;
		}
		case 2:
			return Generate((Left == null) ? LeftValue : Left.Resolve(), (Right == null) ? RightValue : Right.Resolve());
		case 3:
			return LeftValue;
		case 4:
			return ((Left == null) ? LeftValue : Left.Resolve()) + ((Right == null) ? RightValue : Right.Resolve());
		case 5:
			return ((Left == null) ? LeftValue : Left.Resolve()) - ((Right == null) ? RightValue : Right.Resolve());
		case 6:
			return ((Left == null) ? LeftValue : Left.Resolve()) * ((Right == null) ? RightValue : Right.Resolve());
		case 7:
		{
			int num3 = ((Right == null) ? RightValue : Right.Resolve());
			if (num3 == 0)
			{
				return 0;
			}
			return ((Left == null) ? LeftValue : Left.Resolve()) / num3;
		}
		case 8:
			return -((Left == null) ? LeftValue : Left.Resolve());
		case 9:
			if (Set != null && Set.Count > 0)
			{
				if (Set.Count == 1)
				{
					return Set[0].Resolve();
				}
				int index = Stat.GlobalChannelRandom(Channel, 0, Set.Count - 1);
				return Set[index].Resolve();
			}
			if (Generate(0, 1) != 0)
			{
				if (Right != null)
				{
					return Right.Resolve();
				}
				return RightValue;
			}
			if (Left != null)
			{
				return Left.Resolve();
			}
			return LeftValue;
		default:
			return LeftValue;
		}
	}

	public int AverageRounded()
	{
		return (int)Math.Round(Average());
	}

	public double Average()
	{
		switch (Type)
		{
		case 0:
			return 0.0;
		case 1:
		{
			double num3 = ((Left == null) ? ((double)LeftValue) : Left.Average());
			double num4 = ((Right == null) ? ((double)RightValue) : Right.Average());
			return (1.0 + num4) * num3 / 2.0;
		}
		case 2:
		{
			double num5 = ((Left == null) ? ((double)LeftValue) : Left.Average());
			double num6 = ((Right == null) ? ((double)RightValue) : Right.Average());
			return (num5 + num6) / 2.0;
		}
		case 3:
			return LeftValue;
		case 4:
			return ((Left == null) ? ((double)LeftValue) : Left.Average()) + ((Right == null) ? ((double)RightValue) : Right.Average());
		case 5:
			return ((Left == null) ? ((double)LeftValue) : Left.Average()) - ((Right == null) ? ((double)RightValue) : Right.Average());
		case 6:
			return ((Left == null) ? ((double)LeftValue) : Left.Average()) * ((Right == null) ? ((double)RightValue) : Right.Average());
		case 7:
		{
			double num7 = ((Right == null) ? ((double)RightValue) : Right.Average());
			if (num7 == 0.0)
			{
				return 0.0;
			}
			return ((Left == null) ? ((double)LeftValue) : Left.Average()) / num7;
		}
		case 8:
			return 0.0 - ((Left == null) ? ((double)LeftValue) : Left.Average());
		case 9:
		{
			if (Set != null && Set.Count > 0)
			{
				return Set.Select((DieRoll s) => s.Average()).Average();
			}
			double num = ((Left == null) ? ((double)LeftValue) : Left.Average());
			double num2 = ((Right == null) ? ((double)RightValue) : Right.Average());
			return (num + num2) / 2.0;
		}
		default:
			return LeftValue;
		}
	}

	public int Min()
	{
		switch (Type)
		{
		case 0:
			return 0;
		case 1:
		case 2:
			if (Left != null)
			{
				return Left.Min();
			}
			return LeftValue;
		case 3:
			return LeftValue;
		case 4:
			return ((Left == null) ? LeftValue : Left.Min()) + ((Right == null) ? RightValue : Right.Min());
		case 5:
			return ((Left == null) ? LeftValue : Left.Min()) - ((Right == null) ? RightValue : Right.Min());
		case 6:
			return ((Left == null) ? LeftValue : Left.Min()) * ((Right == null) ? RightValue : Right.Min());
		case 7:
		{
			int num6 = ((Right == null) ? RightValue : Right.Min());
			if (num6 == 0)
			{
				return 0;
			}
			return ((Left == null) ? LeftValue : Left.Min()) / num6;
		}
		case 8:
			return -((Left == null) ? LeftValue : Left.Min());
		case 9:
		{
			if (Set != null && Set.Count > 0)
			{
				int num = int.MaxValue;
				{
					foreach (DieRoll item in Set)
					{
						int num2 = item.Min();
						if (num2 < num)
						{
							num = num2;
						}
					}
					return num;
				}
			}
			int num3 = int.MaxValue;
			int num4 = ((Left == null) ? LeftValue : Left.Min());
			if (num4 < num3)
			{
				num3 = num4;
			}
			int num5 = ((Right == null) ? RightValue : Right.Min());
			if (num5 < num3)
			{
				num3 = num5;
			}
			return num3;
		}
		default:
			return LeftValue;
		}
	}

	public int Max()
	{
		switch (Type)
		{
		case 0:
			return 0;
		case 1:
		case 6:
			return ((Left == null) ? LeftValue : Left.Max()) * ((Right == null) ? RightValue : Right.Max());
		case 2:
			if (Right != null)
			{
				return Right.Max();
			}
			return RightValue;
		case 3:
			return LeftValue;
		case 4:
			return ((Left == null) ? LeftValue : Left.Max()) + ((Right == null) ? RightValue : Right.Max());
		case 5:
			return ((Left == null) ? LeftValue : Left.Max()) - ((Right == null) ? RightValue : Right.Max());
		case 7:
		{
			int num6 = ((Right == null) ? RightValue : Right.Max());
			if (num6 == 0)
			{
				return 0;
			}
			return ((Left == null) ? LeftValue : Left.Max()) / num6;
		}
		case 8:
			return -((Left == null) ? LeftValue : Left.Max());
		case 9:
		{
			if (Set != null && Set.Count > 0)
			{
				int num = int.MinValue;
				{
					foreach (DieRoll item in Set)
					{
						int num2 = item.Max();
						if (num2 > num)
						{
							num = num2;
						}
					}
					return num;
				}
			}
			int num3 = int.MinValue;
			int num4 = ((Left == null) ? LeftValue : Left.Max());
			if (num4 > num3)
			{
				num3 = num4;
			}
			int num5 = ((Right == null) ? RightValue : Right.Max());
			if (num5 > num3)
			{
				num3 = num5;
			}
			return num3;
		}
		default:
			return LeftValue;
		}
	}

	public override string ToString()
	{
		string text = ToStringInner();
		if (!string.IsNullOrEmpty(Channel))
		{
			text = text + "[" + Channel + "]";
		}
		return text;
	}

	private string ToStringInner()
	{
		switch (Type)
		{
		case 0:
			return "";
		case 1:
			return LeftString() + "d" + RightString();
		case 2:
			return LeftString() + "-" + RightString();
		case 3:
			return LeftString();
		case 4:
		{
			string text4 = RightString();
			if (text4 == "0")
			{
				return LeftString();
			}
			return LeftString() + "+" + text4;
		}
		case 5:
		{
			string text = RightString();
			if (text == "0")
			{
				return LeftString();
			}
			return LeftString() + "-" + text;
		}
		case 6:
		{
			string text2 = RightString();
			if (text2 == "1")
			{
				return LeftString();
			}
			return LeftString() + "x" + text2;
		}
		case 7:
		{
			string text3 = RightString();
			if (text3 == "1")
			{
				return LeftString();
			}
			return LeftString() + "/" + text3;
		}
		case 8:
			return "-" + LeftString();
		case 9:
			if (Set != null && Set.Count > 0)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				for (int i = 0; i < Set.Count; i++)
				{
					stringBuilder.Compound(Set[i].ToString(), '|');
				}
				return stringBuilder.ToString();
			}
			return LeftString() + "|" + RightString();
		default:
			return "";
		}
	}

	private string LeftString()
	{
		if (Left == null)
		{
			return LeftValue.ToString();
		}
		return Left.ToString();
	}

	private string RightString()
	{
		if (Right == null)
		{
			return RightValue.ToString();
		}
		return Right.ToString();
	}

	private void IncorporateLeft(string Dice, ref bool AnyUnknown)
	{
		if (!int.TryParse(Dice, out LeftValue))
		{
			Left = new DieRoll(Dice, out var AnyUnknown2);
			if (AnyUnknown2)
			{
				AnyUnknown = true;
			}
			if (Left.Type == 3)
			{
				LeftValue = Left.LeftValue;
				Left = null;
			}
		}
	}

	private void IncorporateRight(string Dice, ref bool AnyUnknown)
	{
		if (!int.TryParse(Dice, out RightValue))
		{
			Right = new DieRoll(Dice, out var AnyUnknown2);
			if (AnyUnknown2)
			{
				AnyUnknown = true;
			}
			if (Right.Type == 3)
			{
				RightValue = Right.LeftValue;
				Right = null;
			}
		}
	}

	public void Parse(string Dice, out bool AnyUnknown)
	{
		AnyUnknown = false;
		if (string.IsNullOrEmpty(Dice))
		{
			Type = 0;
			AnyUnknown = true;
			return;
		}
		Stat.TryExtractChannel(ref Dice, ref Channel);
		if (Dice[0] == '-')
		{
			if (int.TryParse(Dice, out LeftValue))
			{
				Type = 3;
				return;
			}
			Type = 8;
			IncorporateLeft(Dice.Substring(1), ref AnyUnknown);
		}
		else if (Dice.Contains("|"))
		{
			Type = 9;
			string[] array = Dice.Split('|');
			switch (array.Length)
			{
			case 0:
				Type = 0;
				AnyUnknown = true;
				return;
			case 1:
			{
				Parse(array[0], out var AnyUnknown2);
				if (AnyUnknown2)
				{
					AnyUnknown = true;
				}
				return;
			}
			case 2:
				Type = 9;
				IncorporateLeft(array[0], ref AnyUnknown);
				IncorporateRight(array[1], ref AnyUnknown);
				return;
			}
			Type = 9;
			Set = new List<DieRoll>(array.Length);
			string[] array2 = array;
			foreach (string dice in array2)
			{
				Set.Add(new DieRoll(dice));
			}
		}
		else if (Dice.Contains("+"))
		{
			string[] array3 = Dice.Split(Stat.splitterPlus, 2);
			Type = 4;
			IncorporateLeft(array3[0], ref AnyUnknown);
			IncorporateRight(array3[1], ref AnyUnknown);
		}
		else if (Dice.Contains("d") && Dice.Contains("-"))
		{
			string[] array4 = Dice.Split(Stat.splitterMinus, 2);
			Type = 5;
			IncorporateLeft(array4[0], ref AnyUnknown);
			IncorporateRight(array4[1], ref AnyUnknown);
		}
		else if (Dice.Contains("x"))
		{
			string[] array5 = Dice.Split(Stat.splitterX, 2);
			Type = 6;
			IncorporateLeft(array5[0], ref AnyUnknown);
			IncorporateRight(array5[1], ref AnyUnknown);
		}
		else if (Dice.Contains("/"))
		{
			string[] array6 = Dice.Split(Stat.splitterSlash, 2);
			Type = 7;
			IncorporateLeft(array6[0], ref AnyUnknown);
			IncorporateRight(array6[1], ref AnyUnknown);
		}
		else if (Dice.Contains("d"))
		{
			string[] array7 = Dice.Split(Stat.splitterD, 2);
			Type = 1;
			IncorporateLeft(array7[0], ref AnyUnknown);
			IncorporateRight(array7[1], ref AnyUnknown);
		}
		else if (Dice.Contains("-"))
		{
			string[] array8 = Dice.Split(Stat.splitterMinus, 2);
			Type = 2;
			IncorporateLeft(array8[0], ref AnyUnknown);
			IncorporateRight(array8[1], ref AnyUnknown);
		}
		else if (int.TryParse(Dice, out LeftValue))
		{
			Type = 3;
		}
		else
		{
			Type = 0;
			AnyUnknown = true;
		}
	}

	public void Parse(string Dice)
	{
		Parse(Dice, out var _);
	}

	public DieRoll FindType(int TargetType)
	{
		if (Type == TargetType)
		{
			return this;
		}
		if (Left != null)
		{
			DieRoll dieRoll = Left.FindType(TargetType);
			if (dieRoll != null)
			{
				return dieRoll;
			}
		}
		if (Right != null)
		{
			DieRoll dieRoll2 = Right.FindType(TargetType);
			if (dieRoll2 != null)
			{
				return dieRoll2;
			}
		}
		if (Set != null)
		{
			foreach (DieRoll item in Set)
			{
				DieRoll dieRoll3 = item.FindType(TargetType);
				if (dieRoll3 != null)
				{
					return dieRoll3;
				}
			}
		}
		return null;
	}

	public DieRoll FindTypeWithConstantLeft(int TargetType)
	{
		if (Type == TargetType && Left == null)
		{
			return this;
		}
		if (Left != null)
		{
			DieRoll dieRoll = Left.FindTypeWithConstantLeft(TargetType);
			if (dieRoll != null)
			{
				return dieRoll;
			}
		}
		if (Right != null)
		{
			DieRoll dieRoll2 = Right.FindTypeWithConstantLeft(TargetType);
			if (dieRoll2 != null)
			{
				return dieRoll2;
			}
		}
		if (Set != null)
		{
			foreach (DieRoll item in Set)
			{
				DieRoll dieRoll3 = item.FindTypeWithConstantLeft(TargetType);
				if (dieRoll3 != null)
				{
					return dieRoll3;
				}
			}
		}
		return null;
	}

	public DieRoll FindTypeWithConstantRight(int TargetType)
	{
		if (Type == TargetType && Right == null)
		{
			return this;
		}
		if (Left != null)
		{
			DieRoll dieRoll = Left.FindTypeWithConstantRight(TargetType);
			if (dieRoll != null)
			{
				return dieRoll;
			}
		}
		if (Right != null)
		{
			DieRoll dieRoll2 = Right.FindTypeWithConstantRight(TargetType);
			if (dieRoll2 != null)
			{
				return dieRoll2;
			}
		}
		if (Set != null)
		{
			foreach (DieRoll item in Set)
			{
				DieRoll dieRoll3 = item.FindTypeWithConstantRight(TargetType);
				if (dieRoll3 != null)
				{
					return dieRoll3;
				}
			}
		}
		return null;
	}

	public DieRoll FindTypeWithConstantBoth(int TargetType)
	{
		if (Type == TargetType && Right == null && Left == null)
		{
			return this;
		}
		if (Left != null)
		{
			DieRoll dieRoll = Left.FindTypeWithConstantBoth(TargetType);
			if (dieRoll != null)
			{
				return dieRoll;
			}
		}
		if (Right != null)
		{
			DieRoll dieRoll2 = Right.FindTypeWithConstantBoth(TargetType);
			if (dieRoll2 != null)
			{
				return dieRoll2;
			}
		}
		if (Set != null)
		{
			foreach (DieRoll item in Set)
			{
				DieRoll dieRoll3 = item.FindTypeWithConstantBoth(TargetType);
				if (dieRoll3 != null)
				{
					return dieRoll3;
				}
			}
		}
		return null;
	}

	public void AdjustDieSize(int Amount)
	{
		if (Amount == 0)
		{
			return;
		}
		if (Type == 3)
		{
			Type = ((Amount < 0) ? 5 : 4);
			Right = new DieRoll("1d" + Math.Abs(Amount));
			return;
		}
		DieRoll dieRoll = FindTypeWithConstantRight(1);
		if (dieRoll != null)
		{
			dieRoll.RightValue += Amount;
			return;
		}
		DieRoll dieRoll2 = FindTypeWithConstantRight(2);
		if (dieRoll2 != null)
		{
			dieRoll2.RightValue += Amount;
			return;
		}
		Left = new DieRoll(this);
		LeftValue = 0;
		Type = ((Amount < 0) ? 5 : 4);
		Right = new DieRoll("1d" + Math.Abs(Amount));
		RightValue = 0;
	}

	public static string AdjustDieSize(string Roll, int Amount)
	{
		if (Amount == 0)
		{
			return Roll;
		}
		DieRoll dieRoll = new DieRoll(Roll);
		dieRoll.AdjustDieSize(Amount);
		return dieRoll.ToString();
	}

	public void AdjustResult(int Amount)
	{
		if (Amount == 0)
		{
			return;
		}
		if (Type == 3)
		{
			LeftValue += Amount;
			return;
		}
		DieRoll dieRoll = FindTypeWithConstantRight(4);
		if (dieRoll != null)
		{
			dieRoll.RightValue += Amount;
			if (dieRoll.RightValue < 0)
			{
				dieRoll.RightValue = -dieRoll.RightValue;
				dieRoll.Type = 5;
			}
			return;
		}
		DieRoll dieRoll2 = FindTypeWithConstantRight(5);
		if (dieRoll2 != null)
		{
			dieRoll2.RightValue -= Amount;
			if (dieRoll2.RightValue < 0)
			{
				dieRoll2.RightValue = -dieRoll2.RightValue;
				dieRoll2.Type = 4;
			}
			return;
		}
		DieRoll dieRoll3 = FindTypeWithConstantLeft(4);
		if (dieRoll3 != null)
		{
			dieRoll3.LeftValue += Amount;
			if (dieRoll3.LeftValue < 0)
			{
				dieRoll3.LeftValue = -dieRoll3.LeftValue;
				dieRoll3.Type = 5;
			}
			return;
		}
		DieRoll dieRoll4 = FindTypeWithConstantLeft(5);
		if (dieRoll4 != null)
		{
			dieRoll4.LeftValue -= Amount;
			if (dieRoll4.LeftValue < 0)
			{
				dieRoll4.LeftValue = -dieRoll4.LeftValue;
				dieRoll4.Type = 4;
			}
			return;
		}
		DieRoll dieRoll5 = FindTypeWithConstantBoth(2);
		if (dieRoll5 != null && dieRoll5.LeftValue + Amount >= 0 && dieRoll5.RightValue + Amount >= 0)
		{
			dieRoll5.LeftValue += Amount;
			dieRoll5.RightValue += Amount;
			return;
		}
		Left = new DieRoll(this);
		LeftValue = 0;
		Type = ((Amount < 0) ? 5 : 4);
		Right = null;
		RightValue = Math.Abs(Amount);
	}

	public static string AdjustResult(string Roll, int Amount)
	{
		if (Amount == 0)
		{
			return Roll;
		}
		DieRoll dieRoll = new DieRoll(Roll);
		dieRoll.AdjustResult(Amount);
		return dieRoll.ToString();
	}
}
