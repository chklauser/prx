using System.Text;

/*
 * C# CUP runtime compatibility source recovered from runtime.dll shipped by
 * CSFlex 1.4 and modernized in July 2026. Copyright 1996-1999 Scott Hudson,
 * Frank Flannery, and C. Scott Ananian. See LICENSE.
 */

namespace java_cup;

public class Integer
{
	private int value;

	public Integer(int val)
	{
		value = val;
	}

	public int intValue()
	{
		return value;
	}

	public static string toOctalString(int c)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (c > 0)
		{
			int num = c & 7;
			c >>= 3;
			stringBuilder.Insert(0, (char)(num + 48));
		}
		if (stringBuilder.Length == 0)
		{
			return "0";
		}
		return stringBuilder.ToString();
	}

	public static string toHexString(int c)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (c > 0)
		{
			int num = c & 0xF;
			c >>= 4;
			if (num >= 10)
			{
				stringBuilder.Insert(0, (char)(num + 97 - 10));
			}
			else
			{
				stringBuilder.Insert(0, (char)(num + 48));
			}
		}
		if (stringBuilder.Length == 0)
		{
			return "0";
		}
		return stringBuilder.ToString();
	}
}
