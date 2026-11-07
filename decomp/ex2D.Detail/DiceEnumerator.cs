using System;
using System.Collections;
using System.Collections.Generic;

namespace ex2D.Detail;

public struct DiceEnumerator : IEnumerator<exTextureInfo.Dice>, IEnumerator, IDisposable, IEnumerable<exTextureInfo.Dice>, IEnumerable
{
	public const int EMPTY = -1;

	public const int MAX = -2;

	public const int MAX_ROTATED = -3;

	private List<int> diceData;

	private int dataIndex;

	private int diceUnitWidth;

	private int diceUnitHeight;

	public exTextureInfo.Dice Current
	{
		get
		{
			exTextureInfo.Dice result = default;
			if (diceData[dataIndex] == -1)
			{
				result.sizeType = exTextureInfo.DiceType.Empty;
				return result;
			}
			if (diceData[dataIndex] >= 0)
			{
				result.offset_x = diceData[dataIndex];
				result.offset_y = diceData[dataIndex + 1];
				result.width = diceData[dataIndex + 2];
				result.height = diceData[dataIndex + 3];
				result.x = diceData[dataIndex + 4];
				result.y = diceData[dataIndex + 5];
				if (result.width < 0)
				{
					result.rotated = true;
					result.width = -result.width;
				}
				if (result.width == diceUnitWidth && result.height == diceUnitHeight)
				{
					result.sizeType = exTextureInfo.DiceType.Max;
				}
				else if (result.width == 0 || result.height == 0)
				{
					result.sizeType = exTextureInfo.DiceType.Empty;
				}
				else
				{
					result.sizeType = exTextureInfo.DiceType.Trimmed;
				}
			}
			else
			{
				result.sizeType = exTextureInfo.DiceType.Max;
				result.x = diceData[dataIndex + 1];
				result.y = diceData[dataIndex + 2];
				result.width = diceUnitWidth;
				result.height = diceUnitHeight;
				result.rotated = diceData[dataIndex] == -3;
			}
			return result;
		}
	}

	object IEnumerator.Current => Current;

	public DiceEnumerator(List<int> _diceData, exTextureInfo _textureInfo)
	{
		diceData = _diceData;
		diceUnitWidth = _diceData[0];
		diceUnitHeight = _diceData[1];
		dataIndex = -1;
		Reset();
	}

	public IEnumerator<exTextureInfo.Dice> GetEnumerator()
	{
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this;
	}

	public static void AddDiceData(exTextureInfo _textureInfo, List<int> _diceData, exTextureInfo.Dice _dice)
	{
		_diceData.Add(_dice.offset_x);
		_diceData.Add(_dice.offset_y);
		_diceData.Add(_dice.rotated ? (-_dice.width) : _dice.width);
		_diceData.Add(_dice.height);
		_diceData.Add(_dice.x);
		_diceData.Add(_dice.y);
	}

	public bool MoveNext()
	{
		if (dataIndex == -1)
		{
			dataIndex = 2;
			if (diceData == null)
			{
				return false;
			}
		}
		else if (diceData[dataIndex] >= 0)
		{
			dataIndex += 6;
		}
		else if (diceData[dataIndex] == -2 || diceData[dataIndex] == -3)
		{
			dataIndex += 3;
		}
		else
		{
			dataIndex++;
		}
		return dataIndex < diceData.Count;
	}

	public void Dispose()
	{
		diceData = null;
	}

	public void Reset()
	{
		dataIndex = -1;
	}
}
