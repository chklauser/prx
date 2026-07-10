/*
 * C# CUP runtime compatibility source recovered from runtime.dll shipped by
 * CSFlex 1.4 and modernized in July 2026. Copyright 1996-1999 Scott Hudson,
 * Frank Flannery, and C. Scott Ananian. See LICENSE.
 */

using System;
using System.Text;

namespace java_cup.runtime;

public abstract class lr_parser
{
	protected const int _error_sync_size = 3;

	protected bool _done_parsing = false;

	protected int tos;

	protected Symbol cur_token;

	protected Stack stack = new Stack();

	protected short[][] production_tab;

	protected short[][] action_tab;

	protected short[][] reduce_tab;

	private Scanner _scanner;

	protected Symbol[] lookahead;

	protected int lookahead_pos;

	public lr_parser()
	{
	}

	public lr_parser(Scanner s)
		: this()
	{
		setScanner(s);
	}

	protected int error_sync_size()
	{
		return 3;
	}

	public abstract short[][] production_table();

	public abstract short[][] action_table();

	public abstract short[][] reduce_table();

	public abstract int start_state();

	public abstract int start_production();

	public abstract int EOF_sym();

	public abstract int error_sym();

	public void done_parsing()
	{
		_done_parsing = true;
	}

	public void setScanner(Scanner s)
	{
		_scanner = s;
	}

	public Scanner getScanner()
	{
		return _scanner;
	}

	public abstract Symbol do_action(int act_num, lr_parser parser, Stack stack, int top);

	public virtual void user_init()
	{
	}

	protected abstract void init_actions();

	public virtual Symbol scan()
	{
		Symbol symbol = getScanner().next_token();
		return (symbol != null) ? symbol : new Symbol(EOF_sym());
	}

	public virtual void report_fatal_error(string message, object info)
	{
		done_parsing();
		report_error(message, info);
		throw new Exception("Can't recover from previous error(s)");
	}

	public virtual void report_error(string message, object info)
	{
		Console.Error.Write(message);
		if (info is Symbol)
		{
			if (((Symbol)info).left != -1)
			{
				Console.Error.WriteLine(" at character {0} of input", ((Symbol)info).left);
			}
			else
			{
				Console.Error.WriteLine();
			}
		}
		else
		{
			Console.Error.WriteLine();
		}
	}

	public virtual void syntax_error(Symbol cur_token)
	{
		report_error("Syntax error", cur_token);
	}

	public virtual void unrecovered_syntax_error(Symbol cur_token)
	{
		report_fatal_error("Couldn't repair and continue parse", cur_token);
	}

	protected short get_action(int state, int sym)
	{
		short[] array = action_tab[state];
		if (array.Length < 20)
		{
			for (int i = 0; i < array.Length; i++)
			{
				short num = array[i++];
				if (num == sym || num == -1)
				{
					return array[i];
				}
			}
			return 0;
		}
		int num2 = 0;
		int num3 = (array.Length - 1) / 2 - 1;
		while (num2 <= num3)
		{
			int i = (num2 + num3) / 2;
			if (sym == array[i * 2])
			{
				return array[i * 2 + 1];
			}
			if (sym > array[i * 2])
			{
				num2 = i + 1;
			}
			else
			{
				num3 = i - 1;
			}
		}
		return array[^1];
	}

	protected short get_reduce(int state, int sym)
	{
		short[] array = reduce_tab[state];
		if (array == null)
		{
			return -1;
		}
		for (int i = 0; i < array.Length; i++)
		{
			short num = array[i++];
			if (num == sym || num == -1)
			{
				return array[i];
			}
		}
		return -1;
	}

	public Symbol parse()
	{
		Symbol symbol = null;
		production_tab = production_table();
		action_tab = action_table();
		reduce_tab = reduce_table();
		init_actions();
		user_init();
		cur_token = scan();
		stack.clear();
		stack.push(new Symbol(0, start_state()));
		tos = 0;
		_done_parsing = false;
		while (!_done_parsing)
		{
			if (cur_token.used_by_parser)
			{
				throw new Exception("Symbol recycling detected (fix your scanner).");
			}
			int num = get_action(((Symbol)stack.peek()).parse_state, cur_token.sym);
			if (num > 0)
			{
				cur_token.parse_state = num - 1;
				cur_token.used_by_parser = true;
				stack.push(cur_token);
				tos++;
				cur_token = scan();
			}
			else if (num < 0)
			{
				symbol = do_action(-num - 1, this, stack, tos);
				short sym = production_tab[-num - 1][0];
				short num2 = production_tab[-num - 1][1];
				for (int i = 0; i < num2; i++)
				{
					stack.pop();
					tos--;
				}
				num = get_reduce(((Symbol)stack.peek()).parse_state, sym);
				symbol.parse_state = num;
				symbol.used_by_parser = true;
				stack.push(symbol);
				tos++;
			}
			else if (num == 0)
			{
				syntax_error(cur_token);
				if (!error_recovery(debug: false))
				{
					unrecovered_syntax_error(cur_token);
					done_parsing();
				}
				else
				{
					symbol = (Symbol)stack.peek();
				}
			}
		}
		return symbol;
	}

	public virtual void debug_message(string mess)
	{
		Console.Error.WriteLine(mess);
	}

	public virtual void dump_stack()
	{
		if (stack == null)
		{
			debug_message("# Stack dump requested, but stack is null");
			return;
		}
		debug_message("============ Parse Stack Dump ============");
		for (int i = 0; i < stack.size(); i++)
		{
			debug_message("Symbol: " + ((Symbol)stack.elementAt(i)).sym + " State: " + ((Symbol)stack.elementAt(i)).parse_state);
		}
		debug_message("==========================================");
	}

	public virtual void debug_reduce(int prod_num, int nt_num, int rhs_size)
	{
		debug_message("# Reduce with prod #" + prod_num + " [NT=" + nt_num + ", SZ=" + rhs_size + "]");
	}

	public virtual void debug_shift(Symbol shift_tkn)
	{
		debug_message("# Shift under term #" + shift_tkn.sym + " to state #" + shift_tkn.parse_state);
	}

	public virtual void debug_stack()
	{
		StringBuilder stringBuilder = new StringBuilder("## STACK:");
		for (int i = 0; i < stack.size(); i++)
		{
			Symbol symbol = (Symbol)stack.elementAt(i);
			stringBuilder.AppendFormat(" <state {0}, sym {1}>", symbol.parse_state, symbol.sym);
			if (i % 3 == 2 || i == stack.size() - 1)
			{
				debug_message(stringBuilder.ToString());
				stringBuilder = new StringBuilder("         ");
			}
		}
	}

	public Symbol debug_parse()
	{
		Symbol symbol = null;
		production_tab = production_table();
		action_tab = action_table();
		reduce_tab = reduce_table();
		debug_message("# Initializing parser");
		init_actions();
		user_init();
		cur_token = scan();
		debug_message("# Current Symbol is #" + cur_token.sym);
		stack.clear();
		stack.push(new Symbol(0, start_state()));
		tos = 0;
		_done_parsing = false;
		while (!_done_parsing)
		{
			if (cur_token.used_by_parser)
			{
				throw new Exception("Symbol recycling detected (fix your scanner).");
			}
			int num = get_action(((Symbol)stack.peek()).parse_state, cur_token.sym);
			if (num > 0)
			{
				cur_token.parse_state = num - 1;
				cur_token.used_by_parser = true;
				debug_shift(cur_token);
				stack.push(cur_token);
				tos++;
				cur_token = scan();
				debug_message("# Current token is " + cur_token);
			}
			else if (num < 0)
			{
				symbol = do_action(-num - 1, this, stack, tos);
				short num2 = production_tab[-num - 1][0];
				short num3 = production_tab[-num - 1][1];
				debug_reduce(-num - 1, num2, num3);
				for (int i = 0; i < num3; i++)
				{
					stack.pop();
					tos--;
				}
				num = get_reduce(((Symbol)stack.peek()).parse_state, num2);
				debug_message("# Reduce rule: top state " + ((Symbol)stack.peek()).parse_state + ", lhs sym " + num2 + " -> state " + num);
				symbol.parse_state = num;
				symbol.used_by_parser = true;
				stack.push(symbol);
				tos++;
				debug_message("# Goto state #" + num);
			}
			else if (num == 0)
			{
				syntax_error(cur_token);
				if (!error_recovery(debug: true))
				{
					unrecovered_syntax_error(cur_token);
					done_parsing();
				}
				else
				{
					symbol = (Symbol)stack.peek();
				}
			}
		}
		return symbol;
	}

	protected bool error_recovery(bool debug)
	{
		if (debug)
		{
			debug_message("# Attempting error recovery");
		}
		if (!find_recovery_config(debug))
		{
			if (debug)
			{
				debug_message("# Error recovery fails");
			}
			return false;
		}
		read_lookahead();
		while (true)
		{
			if (debug)
			{
				debug_message("# Trying to parse ahead");
			}
			if (try_parse_ahead(debug))
			{
				break;
			}
			if (lookahead[0].sym == EOF_sym())
			{
				if (debug)
				{
					debug_message("# Error recovery fails at EOF");
				}
				return false;
			}
			if (debug)
			{
				debug_message("# Consuming Symbol #" + lookahead[0].sym);
			}
			restart_lookahead();
		}
		if (debug)
		{
			debug_message("# Parse-ahead ok, going back to normal parse");
		}
		parse_lookahead(debug);
		return true;
	}

	protected bool shift_under_error()
	{
		return get_action(((Symbol)stack.peek()).parse_state, error_sym()) > 0;
	}

	protected bool find_recovery_config(bool debug)
	{
		if (debug)
		{
			debug_message("# Finding recovery state on stack");
		}
		int right = ((Symbol)stack.peek()).right;
		int left = ((Symbol)stack.peek()).left;
		while (!shift_under_error())
		{
			if (debug)
			{
				debug_message("# Pop stack by one, state was # " + ((Symbol)stack.peek()).parse_state);
			}
			left = ((Symbol)stack.pop()).left;
			tos--;
			if (stack.empty())
			{
				if (debug)
				{
					debug_message("# No recovery state found on stack");
				}
				return false;
			}
		}
		int num = get_action(((Symbol)stack.peek()).parse_state, error_sym());
		if (debug)
		{
			debug_message("# Recover state found (#" + ((Symbol)stack.peek()).parse_state + ")");
			debug_message("# Shifting on error to state #" + (num - 1));
		}
		Symbol symbol = new Symbol(error_sym(), left, right);
		symbol.parse_state = num - 1;
		symbol.used_by_parser = true;
		stack.push(symbol);
		tos++;
		return true;
	}

	protected void read_lookahead()
	{
		lookahead = new Symbol[error_sync_size()];
		for (int i = 0; i < error_sync_size(); i++)
		{
			lookahead[i] = cur_token;
			cur_token = scan();
		}
		lookahead_pos = 0;
	}

	protected Symbol cur_err_token()
	{
		return lookahead[lookahead_pos];
	}

	protected bool advance_lookahead()
	{
		lookahead_pos++;
		return lookahead_pos < error_sync_size();
	}

	protected void restart_lookahead()
	{
		for (int i = 1; i < error_sync_size(); i++)
		{
			lookahead[i - 1] = lookahead[i];
		}
		lookahead[error_sync_size() - 1] = cur_token;
		cur_token = scan();
		lookahead_pos = 0;
	}

	protected bool try_parse_ahead(bool debug)
	{
		virtual_parse_stack virtual_parse_stack2 = new virtual_parse_stack(stack);
		while (true)
		{
			int num = get_action(virtual_parse_stack2.top(), cur_err_token().sym);
			if (num == 0)
			{
				return false;
			}
			if (num > 0)
			{
				virtual_parse_stack2.push(num - 1);
				if (debug)
				{
					debug_message("# Parse-ahead shifts Symbol #" + cur_err_token().sym + " into state #" + (num - 1));
				}
				if (!advance_lookahead())
				{
					return true;
				}
				continue;
			}
			if (-num - 1 == start_production())
			{
				break;
			}
			short num2 = production_tab[-num - 1][0];
			short num3 = production_tab[-num - 1][1];
			for (int i = 0; i < num3; i++)
			{
				virtual_parse_stack2.pop();
			}
			if (debug)
			{
				debug_message("# Parse-ahead reduces: handle size = " + num3 + " lhs = #" + num2 + " from state #" + virtual_parse_stack2.top());
			}
			virtual_parse_stack2.push(get_reduce(virtual_parse_stack2.top(), num2));
			if (debug)
			{
				debug_message("# Goto state #" + virtual_parse_stack2.top());
			}
		}
		if (debug)
		{
			debug_message("# Parse-ahead accepts");
		}
		return true;
	}

	protected void parse_lookahead(bool debug)
	{
		Symbol symbol = null;
		lookahead_pos = 0;
		if (debug)
		{
			debug_message("# Reparsing saved input with actions");
			debug_message("# Current Symbol is #" + cur_err_token().sym);
			debug_message("# Current state is #" + ((Symbol)stack.peek()).parse_state);
		}
		while (!_done_parsing)
		{
			int num = get_action(((Symbol)stack.peek()).parse_state, cur_err_token().sym);
			if (num > 0)
			{
				cur_err_token().parse_state = num - 1;
				cur_err_token().used_by_parser = true;
				if (debug)
				{
					debug_shift(cur_err_token());
				}
				stack.push(cur_err_token());
				tos++;
				if (!advance_lookahead())
				{
					if (debug)
					{
						debug_message("# Completed reparse");
					}
					break;
				}
				if (debug)
				{
					debug_message("# Current Symbol is #" + cur_err_token().sym);
				}
			}
			else if (num < 0)
			{
				symbol = do_action(-num - 1, this, stack, tos);
				short num2 = production_tab[-num - 1][0];
				short num3 = production_tab[-num - 1][1];
				if (debug)
				{
					debug_reduce(-num - 1, num2, num3);
				}
				for (int i = 0; i < num3; i++)
				{
					stack.pop();
					tos--;
				}
				num = (symbol.parse_state = get_reduce(((Symbol)stack.peek()).parse_state, num2));
				symbol.used_by_parser = true;
				stack.push(symbol);
				tos++;
				if (debug)
				{
					debug_message("# Goto state #" + num);
				}
			}
			else if (num == 0)
			{
				report_fatal_error("Syntax error", symbol);
				break;
			}
		}
	}

	protected static short[][] unpackFromStrings(string[] sa)
	{
		StringBuilder stringBuilder = new StringBuilder(sa[0]);
		for (int i = 1; i < sa.Length; i++)
		{
			stringBuilder.Append(sa[i]);
		}
		int num = 0;
		int num2 = (int)(((uint)stringBuilder[num] << 16) | stringBuilder[num + 1]);
		num += 2;
		short[][] array = new short[num2][];
		for (int j = 0; j < num2; j++)
		{
			int num3 = (int)(((uint)stringBuilder[num] << 16) | stringBuilder[num + 1]);
			num += 2;
			array[j] = new short[num3];
			for (int k = 0; k < num3; k++)
			{
				array[j][k] = (short)(stringBuilder[num++] - 2);
			}
		}
		return array;
	}

	protected static short[][] unpackFromShorts(short[] sb)
	{
		int num = 0;
		int num2 = (sb[num] << 16) | (ushort)sb[num + 1];
		num += 2;
		short[][] array = new short[num2][];
		for (int i = 0; i < num2; i++)
		{
			int num3 = (sb[num] << 16) | (ushort)sb[num + 1];
			num += 2;
			array[i] = new short[num3];
			for (int j = 0; j < num3; j++)
			{
				array[i][j] = (short)(sb[num++] - 2);
			}
		}
		return array;
	}
}
