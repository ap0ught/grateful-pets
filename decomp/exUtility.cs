using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct exUtility
{
	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T val = lhs;
		lhs = rhs;
		rhs = val;
	}
}
