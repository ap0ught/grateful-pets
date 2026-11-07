using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XRL.Rules;

namespace Sheeter;

public class Sheet
{
	public StreamWriter file;

	public List<SheetColumn> columns = new List<SheetColumn>();

	private int nColumn;

	public Sheet(string path)
	{
		File.CreateText(path).Dispose();
		file = new StreamWriter(path, append: false);
	}

	public void finish()
	{
		file.Flush();
		file.Close();
		file.Dispose();
	}

	private void write(double val)
	{
		file.Write(val);
	}

	private void write(float val)
	{
		file.Write(val);
	}

	private void write(int val)
	{
		file.Write(val);
	}

	private void write(string val)
	{
		if (val != ",")
		{
			val = val.Replace(",", "").Replace("\"", "");
		}
		file.Write(val);
	}

	public void writeColumn(string val)
	{
		if (nColumn != 0)
		{
			write(",");
		}
		nColumn++;
		write(val);
	}

	public void writeColumn(int val)
	{
		if (nColumn != 0)
		{
			write(",");
		}
		nColumn++;
		write(val);
	}

	public void writeStats(List<int> values)
	{
		if (nColumn != 0)
		{
			write(",");
		}
		nColumn += 3;
		write(values.Median());
		write(",");
		write(values.Average());
		write(",");
		write(values.StandardDeviation());
	}

	public void writeShortStats(List<int> values)
	{
		if (nColumn != 0)
		{
			write(",");
		}
		nColumn++;
		write(values.Average());
	}

	public void writeDie(string die, int tier, int n = 100, Func<int, int> map = null)
	{
		List<int> list = new List<int>(n);
		if (die.Contains(',') || die.Contains("t"))
		{
			string[] array = die.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Replace("(t)", (tier - 1).ToString());
				array[i] = array[i].Replace("(t-1)", (tier - 1).ToString());
				array[i] = array[i].Replace("(t+1)", (tier - 1).ToString());
			}
			for (int j = 0; j < n; j++)
			{
				int num = 0;
				for (int k = 0; k < array.Length; k++)
				{
					num += Stat.Roll(array[j]);
				}
				list.Add(num);
			}
		}
		else
		{
			for (int l = 0; l < n; l++)
			{
				list.Add(Stat.Roll(die));
			}
		}
		if (map != null)
		{
			for (int m = 0; m < list.Count; m++)
			{
				list[m] = map(list[m]);
			}
		}
		if (nColumn != 0)
		{
			write(",");
		}
		nColumn += 4;
		write(die);
		write(",");
		write(list.Median());
		write(",");
		write(list.Average());
		write(",");
		write(list.StandardDeviation());
	}

	public void endColumns()
	{
		write("\r\n");
	}

	public void endRow()
	{
		if (nColumn != columns.Count)
		{
			throw new Exception("You ended a row before you put all the columns in.");
		}
		write("\r\n");
		nColumn = 0;
	}

	public void addColumn(string name)
	{
		if (columns.Count != 0)
		{
			write(",");
		}
		write(name);
		columns.Add(new SheetColumn(name));
	}

	public void addDieColumn(string name)
	{
		if (columns.Count != 0)
		{
			write(",");
		}
		columns.Add(new SheetColumn(name + "Value"));
		write(name + "Value");
		write(",");
		columns.Add(new SheetColumn(name + "Mean"));
		write(name + "Mean");
		write(",");
		columns.Add(new SheetColumn(name + "Median"));
		write(name + "Median");
		write(",");
		columns.Add(new SheetColumn(name + "SD"));
		write(name + "SD");
	}

	public void addStatColumn(string name)
	{
		if (columns.Count != 0)
		{
			write(",");
		}
		columns.Add(new SheetColumn(name + "Median"));
		write(name + "Median");
		write(",");
		columns.Add(new SheetColumn(name + "Mean"));
		write(name + "Mean");
		write(",");
		columns.Add(new SheetColumn(name + "SD"));
		write(name + "SD");
	}

	public void addShortStatColumn(string name)
	{
		if (columns.Count != 0)
		{
			write(",");
		}
		columns.Add(new SheetColumn(name + "Mean"));
		write(name + "Mean");
	}
}
