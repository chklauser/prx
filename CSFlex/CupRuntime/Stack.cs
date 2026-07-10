/*
 * C# CUP runtime compatibility source recovered from runtime.dll shipped by
 * CSFlex 1.4 and modernized in July 2026. Copyright 1996-1999 Scott Hudson,
 * Frank Flannery, and C. Scott Ananian. See LICENSE.
 */

using System.Collections;

namespace java_cup;

public class Stack
{
	private ArrayList back = new ArrayList();

	public void push(object item)
	{
		back.Add(item);
	}

	public object pop()
	{
		try
		{
			return peek();
		}
		finally
		{
			back.RemoveAt(back.Count - 1);
		}
	}

	public object peek()
	{
		return back[back.Count - 1];
	}

	public bool empty()
	{
		return back.Count == 0;
	}

	public void clear()
	{
		back.Clear();
	}

	public int size()
	{
		return back.Count;
	}

	public object elementAt(int idx)
	{
		return back[idx];
	}

	public void setElementAt(object new_item, int idx)
	{
		back[idx] = new_item;
	}
}
