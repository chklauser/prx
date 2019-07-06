// This is the output code from your template
// you only get syntax-highlighting here - not intellisense
using System;
using System.Reflection;
using System.Collections.Generic;
using Prexonite.Types;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.Operators{
  			
	public class Addition : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Addition()
		{
		}
		
		public static Addition Instance { get; } = new Addition();
		#endregion
		
		///<summary>
		///The alias used for the Addition operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Addition;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Addition operator requires two arguments.");

			return args[0].Addition(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVAdditionMethod;
		
		
		#endregion
	}
					
	public class Subtraction : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Subtraction()
		{
		}
		
		public static Subtraction Instance { get; } = new Subtraction();
		#endregion
		
		///<summary>
		///The alias used for the Subtraction operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Subtraction;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Subtraction operator requires two arguments.");

			return args[0].Subtraction(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVSubtractionMethod;
		
		
		#endregion
	}
					
	public class Multiplication : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Multiplication()
		{
		}
		
		public static Multiplication Instance { get; } = new Multiplication();
		#endregion
		
		///<summary>
		///The alias used for the Multiplication operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Multiplication;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Multiplication operator requires two arguments.");

			return args[0].Multiply(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVMultiplyMethod;
		
		
		#endregion
	}
					
	public class Division : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Division()
		{
		}
		
		public static Division Instance { get; } = new Division();
		#endregion
		
		///<summary>
		///The alias used for the Division operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Division;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Division operator requires two arguments.");

			return args[0].Division(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVDivisionMethod;
		
		
		#endregion
	}
					
	public class Modulus : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Modulus()
		{
		}
		
		public static Modulus Instance { get; } = new Modulus();
		#endregion
		
		///<summary>
		///The alias used for the Modulus operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Modulus;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Modulus operator requires two arguments.");

			return args[0].Modulus(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVModulusMethod;
		
		
		#endregion
	}
					
	public class Power : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Power()
		{
		}
		
		public static Power Instance { get; } = new Power();
		#endregion
		
		///<summary>
		///The alias used for the Power operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Power;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Power operator requires two arguments.");

			var left = args[0];
			var right = args[1];
			if (
				!(left.TryConvertTo(sctx, PType.Real, out var rleft) &&
				right.TryConvertTo(sctx, PType.Real, out var rright)))
					throw new PrexoniteException("The arguments supplied to the power operator are invalid (cannot be converted to Real).");
			return System.Math.Pow(Convert.ToDouble(rleft.Value), Convert.ToDouble(rright.Value));			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Runtime.RaiseToPowerMethod;
		
	
		public override void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv, int dynamicArgc)
		{
			if(dynamicArgc >= 2)
			{
				state.EmitIgnoreArguments(dynamicArgc-2);
			}
			else if(dynamicArgc == 1)
			{
				staticArgv[0].EmitLoadAsPValue(state);
			}
			else
			{
				staticArgv[0].EmitLoadAsPValue(state);
				staticArgv[1].EmitLoadAsPValue(state);
			}
			state.EmitLoadLocal(state.SctxLocal);
            state.Il.EmitCall(System.Reflection.Emit.OpCodes.Call, OperationMethod, null);
		}
		
		#endregion
	}
					
	public class BitwiseAnd : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private BitwiseAnd()
		{
		}
		
		public static BitwiseAnd Instance { get; } = new BitwiseAnd();
		#endregion
		
		///<summary>
		///The alias used for the BitwiseAnd operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.BitwiseAnd;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The BitwiseAnd operator requires two arguments.");

			return args[0].BitwiseAnd(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVBitwiseAndMethod;
		
		
		#endregion
	}
					
	public class BitwiseOr : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private BitwiseOr()
		{
		}
		
		public static BitwiseOr Instance { get; } = new BitwiseOr();
		#endregion
		
		///<summary>
		///The alias used for the BitwiseOr operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.BitwiseOr;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The BitwiseOr operator requires two arguments.");

			return args[0].BitwiseOr(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVBitwiseOrMethod;
		
		
		#endregion
	}
					
	public class ExclusiveOr : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private ExclusiveOr()
		{
		}
		
		public static ExclusiveOr Instance { get; } = new ExclusiveOr();
		#endregion
		
		///<summary>
		///The alias used for the ExclusiveOr operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.ExclusiveOr;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The ExclusiveOr operator requires two arguments.");

			return args[0].ExclusiveOr(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVExclusiveOrMethod;
		
		
		#endregion
	}
					
	public class Equality : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Equality()
		{
		}
		
		public static Equality Instance { get; } = new Equality();
		#endregion
		
		///<summary>
		///The alias used for the Equality operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Equality;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Equality operator requires two arguments.");

			return args[0].Equality(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVEqualityMethod;
		
		
		#endregion
	}
					
	public class Inequality : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Inequality()
		{
		}
		
		public static Inequality Instance { get; } = new Inequality();
		#endregion
		
		///<summary>
		///The alias used for the Inequality operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.Inequality;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Inequality operator requires two arguments.");

			return args[0].Inequality(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVInequalityMethod;
		
		
		#endregion
	}
					
	public class GreaterThan : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private GreaterThan()
		{
		}
		
		public static GreaterThan Instance { get; } = new GreaterThan();
		#endregion
		
		///<summary>
		///The alias used for the GreaterThan operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.GreaterThan;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The GreaterThan operator requires two arguments.");

			return args[0].GreaterThan(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVGreaterThanMethod;
		
		
		#endregion
	}
					
	public class GreaterThanOrEqual : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private GreaterThanOrEqual()
		{
		}
		
		public static GreaterThanOrEqual Instance { get; } = new GreaterThanOrEqual();
		#endregion
		
		///<summary>
		///The alias used for the GreaterThanOrEqual operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.GreaterThanOrEqual;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The GreaterThanOrEqual operator requires two arguments.");

			return args[0].GreaterThanOrEqual(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVGreaterThanOrEqualMethod;
		
		
		#endregion
	}
					
	public class LessThan : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private LessThan()
		{
		}
		
		public static LessThan Instance { get; } = new LessThan();
		#endregion
		
		///<summary>
		///The alias used for the LessThan operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.LessThan;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The LessThan operator requires two arguments.");

			return args[0].LessThan(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVLessThanMethod;
		
		
		#endregion
	}
					
	public class LessThanOrEqual : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private LessThanOrEqual()
		{
		}
		
		public static LessThanOrEqual Instance { get; } = new LessThanOrEqual();
		#endregion
		
		///<summary>
		///The alias used for the LessThanOrEqual operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.LessThanOrEqual;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The LessThanOrEqual operator requires two arguments.");

			return args[0].LessThanOrEqual(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVLessThanOrEqualMethod;
		
		
		#endregion
	}
					
	public class UnaryNegation : UnaryOperatorBase
	{
	
		#region Singleton pattern
		private UnaryNegation()
		{
		}
		
		public static UnaryNegation Instance { get; } = new UnaryNegation();
		#endregion
		
		///<summary>
		///The alias used for the UnaryNegation operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.UnaryNegation;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 1)
				throw new PrexoniteException("The UnaryNegation operator requires one argument.");

			
			return args[0].UnaryNegation(sctx);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVUnaryNegationMethod;
		
		
		#endregion
	}
					
	public class OnesComplement : UnaryOperatorBase
	{
	
		#region Singleton pattern
		private OnesComplement()
		{
		}
		
		public static OnesComplement Instance { get; } = new OnesComplement();
		#endregion
		
		///<summary>
		///The alias used for the OnesComplement operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.OnesComplement;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 1)
				throw new PrexoniteException("The OnesComplement operator requires one argument.");

			
			return args[0].OnesComplement(sctx);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVOnesComplementMethod;
		
		
		#endregion
	}
					
	public class LogicalNot : UnaryOperatorBase
	{
	
		#region Singleton pattern
		private LogicalNot()
		{
		}
		
		public static LogicalNot Instance { get; } = new LogicalNot();
		#endregion
		
		///<summary>
		///The alias used for the LogicalNot operation by the compiler.
		///</summary>
		public static string DefaultAlias => OperatorNames.Prexonite.LogicalNot;
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 1)
				throw new PrexoniteException("The LogicalNot operator requires one argument.");

			
			return args[0].LogicalNot(sctx);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod => Compiler.Cil.Compiler.PVLogicalNotMethod;
		
		
		#endregion
	}
			
	public static class OperatorCommands
	{
		public static void AddToEngine(Engine engine)
		{
			var cs = engine.Commands;
			cs.AddEngineCommand(Addition.DefaultAlias, Addition.Instance);
			cs.AddEngineCommand(Subtraction.DefaultAlias, Subtraction.Instance);
			cs.AddEngineCommand(Multiplication.DefaultAlias, Multiplication.Instance);
			cs.AddEngineCommand(Division.DefaultAlias, Division.Instance);
			cs.AddEngineCommand(Modulus.DefaultAlias, Modulus.Instance);
			cs.AddEngineCommand(Power.DefaultAlias, Power.Instance);
			cs.AddEngineCommand(BitwiseAnd.DefaultAlias, BitwiseAnd.Instance);
			cs.AddEngineCommand(BitwiseOr.DefaultAlias, BitwiseOr.Instance);
			cs.AddEngineCommand(ExclusiveOr.DefaultAlias, ExclusiveOr.Instance);
			cs.AddEngineCommand(Equality.DefaultAlias, Equality.Instance);
			cs.AddEngineCommand(Inequality.DefaultAlias, Inequality.Instance);
			cs.AddEngineCommand(GreaterThan.DefaultAlias, GreaterThan.Instance);
			cs.AddEngineCommand(GreaterThanOrEqual.DefaultAlias, GreaterThanOrEqual.Instance);
			cs.AddEngineCommand(LessThan.DefaultAlias, LessThan.Instance);
			cs.AddEngineCommand(LessThanOrEqual.DefaultAlias, LessThanOrEqual.Instance);
			cs.AddEngineCommand(UnaryNegation.DefaultAlias, UnaryNegation.Instance);
			cs.AddEngineCommand(OnesComplement.DefaultAlias, OnesComplement.Instance);
			cs.AddEngineCommand(LogicalNot.DefaultAlias, LogicalNot.Instance);
		}
		
		private static readonly Lazy<Dictionary<string,string>> _reverseLiteralMap = 
			new Lazy<Dictionary<string,string>>(() => 
				new Dictionary<string,string>
				{
					{OperatorNames.Prexonite.Addition, "(+)"},
					{OperatorNames.Prexonite.Subtraction, "(-)"},
					{OperatorNames.Prexonite.Multiplication, "(*)"},
					{OperatorNames.Prexonite.Division, "(/)"},
					{OperatorNames.Prexonite.Modulus, "$" + OperatorNames.Prexonite.Modulus},
					{OperatorNames.Prexonite.Power, "(^)"},
					{OperatorNames.Prexonite.BitwiseAnd, "(&)"},
					{OperatorNames.Prexonite.BitwiseOr, "(|)"},
					{OperatorNames.Prexonite.ExclusiveOr, "$" + OperatorNames.Prexonite.ExclusiveOr},
					{OperatorNames.Prexonite.Equality, "(==)"},
					{OperatorNames.Prexonite.Inequality, "(!=)"},
					{OperatorNames.Prexonite.GreaterThan, "(>)"},
					{OperatorNames.Prexonite.GreaterThanOrEqual, "(>=)"},
					{OperatorNames.Prexonite.LessThan, "(<)"},
					{OperatorNames.Prexonite.LessThanOrEqual, "(<=)"},
					{OperatorNames.Prexonite.UnaryNegation, "(-.)"},
					{OperatorNames.Prexonite.OnesComplement, "$" + OperatorNames.Prexonite.OnesComplement},
					{OperatorNames.Prexonite.LogicalNot, "$" + OperatorNames.Prexonite.LogicalNot},
				}, System.Threading.LazyThreadSafetyMode.PublicationOnly);
		public static bool TryGetLiteral(string id, out string literal)
		{
			return _reverseLiteralMap.Value.TryGetValue(id, out literal);
		}
	}
}
 
