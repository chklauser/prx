/*
 * C# CUP runtime compatibility source recovered from runtime.dll shipped by
 * CSFlex 1.4 and modernized in July 2026. Copyright 1996-1999 Scott Hudson,
 * Frank Flannery, and C. Scott Ananian. See LICENSE.
 */

using System;

namespace java_cup.runtime;

public class virtual_parse_stack
{
	protected Stack real_stack;

	protected int real_next;

	protected Stack vstack;

	public virtual_parse_stack(Stack shadowing_stack)
	{
		if (shadowing_stack == null)
		{
			throw new Exception("Internal parser error: attempt to create null virtual stack");
		}
		real_stack = shadowing_stack;
		vstack = new Stack();
		real_next = 0;
		get_from_real();
	}

	protected void get_from_real()
	{
		if (real_next < real_stack.size())
		{
			Symbol symbol = (Symbol)real_stack.elementAt(real_stack.size() - 1 - real_next);
			real_next++;
			vstack.push(symbol.parse_state);
		}
	}

	public bool empty()
	{
		return vstack.empty();
	}

	public int top()
	{
		if (empty())
		{
			throw new Exception("Internal parser error: top() called on empty virtual stack");
		}
		return (int)vstack.peek();
	}

	public void pop()
	{
		if (empty())
		{
			throw new Exception("Internal parser error: pop from empty virtual stack");
		}
		vstack.pop();
		if (empty())
		{
			get_from_real();
		}
	}

	public void push(int state_num)
	{
		vstack.push(state_num);
	}
}
