/*
 * C# CUP runtime compatibility source recovered from runtime.dll shipped by
 * CSFlex 1.4 and modernized in July 2026. Copyright 1996-1999 Scott Hudson,
 * Frank Flannery, and C. Scott Ananian. See LICENSE.
 */

namespace java_cup.runtime;

public class Symbol
{
	public int sym;

	public int parse_state;

	internal bool used_by_parser = false;

	public int left;

	public int right;

	public object value;

	public Symbol(int id, int l, int r, object o)
		: this(id)
	{
		left = l;
		right = r;
		value = o;
	}

	public Symbol(int id, object o)
		: this(id, -1, -1, o)
	{
	}

	public Symbol(int id, int l, int r)
		: this(id, l, r, null)
	{
	}

	public Symbol(int sym_num)
		: this(sym_num, -1)
	{
		left = -1;
		right = -1;
		value = null;
	}

	internal Symbol(int sym_num, int state)
	{
		sym = sym_num;
		parse_state = state;
	}

	public override string ToString()
	{
		return "#" + sym;
	}
}
