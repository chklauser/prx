 
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
		
		private static readonly Addition _instance = new Addition();
		public static Addition Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Addition operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Addition; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Addition operator requires two arguments.");

			return args[0].Addition(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVAdditionMethod; }
		}
		
		
		#endregion
	}
					
	public class Subtraction : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Subtraction()
		{
		}
		
		private static readonly Subtraction _instance = new Subtraction();
		public static Subtraction Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Subtraction operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Subtraction; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Subtraction operator requires two arguments.");

			return args[0].Subtraction(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVSubtractionMethod; }
		}
		
		
		#endregion
	}
					
	public class Multiplication : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Multiplication()
		{
		}
		
		private static readonly Multiplication _instance = new Multiplication();
		public static Multiplication Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Multiplication operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Multiplication; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Multiplication operator requires two arguments.");

			return args[0].Multiply(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVMultiplyMethod; }
		}
		
		
		#endregion
	}
					
	public class Division : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Division()
		{
		}
		
		private static readonly Division _instance = new Division();
		public static Division Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Division operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Division; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Division operator requires two arguments.");

			return args[0].Division(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVDivisionMethod; }
		}
		
		
		#endregion
	}
					
	public class Modulus : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Modulus()
		{
		}
		
		private static readonly Modulus _instance = new Modulus();
		public static Modulus Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Modulus operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Modulus; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Modulus operator requires two arguments.");

			return args[0].Modulus(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVModulusMethod; }
		}
		
		
		#endregion
	}
					
	public class Power : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Power()
		{
		}
		
		private static readonly Power _instance = new Power();
		public static Power Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Power operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Power; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Power operator requires two arguments.");

			
			PValue rleft,rright;
			var left = args[0];
			var right = args[1];
			if (
				!(left.TryConvertTo(sctx, PType.Real, out rleft) &&
				right.TryConvertTo(sctx, PType.Real, out rright)))
					throw new PrexoniteException("The arguments supplied to the power operator are invalid (cannot be converted to Real).");
			return System.Math.Pow(Convert.ToDouble(rleft.Value), Convert.ToDouble(rright.Value));			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Runtime.RaiseToPowerMethod; }
		}
		
	
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
		
		private static readonly BitwiseAnd _instance = new BitwiseAnd();
		public static BitwiseAnd Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the BitwiseAnd operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.BitwiseAnd; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The BitwiseAnd operator requires two arguments.");

			return args[0].BitwiseAnd(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVBitwiseAndMethod; }
		}
		
		
		#endregion
	}
					
	public class BitwiseOr : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private BitwiseOr()
		{
		}
		
		private static readonly BitwiseOr _instance = new BitwiseOr();
		public static BitwiseOr Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the BitwiseOr operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.BitwiseOr; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The BitwiseOr operator requires two arguments.");

			return args[0].BitwiseOr(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVBitwiseOrMethod; }
		}
		
		
		#endregion
	}
					
	public class ExclusiveOr : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private ExclusiveOr()
		{
		}
		
		private static readonly ExclusiveOr _instance = new ExclusiveOr();
		public static ExclusiveOr Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the ExclusiveOr operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.ExclusiveOr; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The ExclusiveOr operator requires two arguments.");

			return args[0].ExclusiveOr(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVExclusiveOrMethod; }
		}
		
		
		#endregion
	}
					
	public class Equality : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Equality()
		{
		}
		
		private static readonly Equality _instance = new Equality();
		public static Equality Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Equality operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Equality; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Equality operator requires two arguments.");

			return args[0].Equality(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVEqualityMethod; }
		}
		
		
		#endregion
	}
					
	public class Inequality : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private Inequality()
		{
		}
		
		private static readonly Inequality _instance = new Inequality();
		public static Inequality Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the Inequality operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.Inequality; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The Inequality operator requires two arguments.");

			return args[0].Inequality(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVInequalityMethod; }
		}
		
		
		#endregion
	}
					
	public class GreaterThan : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private GreaterThan()
		{
		}
		
		private static readonly GreaterThan _instance = new GreaterThan();
		public static GreaterThan Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the GreaterThan operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.GreaterThan; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The GreaterThan operator requires two arguments.");

			return args[0].GreaterThan(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVGreaterThanMethod; }
		}
		
		
		#endregion
	}
					
	public class GreaterThanOrEqual : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private GreaterThanOrEqual()
		{
		}
		
		private static readonly GreaterThanOrEqual _instance = new GreaterThanOrEqual();
		public static GreaterThanOrEqual Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the GreaterThanOrEqual operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.GreaterThanOrEqual; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The GreaterThanOrEqual operator requires two arguments.");

			return args[0].GreaterThanOrEqual(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVGreaterThanOrEqualMethod; }
		}
		
		
		#endregion
	}
					
	public class LessThan : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private LessThan()
		{
		}
		
		private static readonly LessThan _instance = new LessThan();
		public static LessThan Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the LessThan operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.LessThan; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The LessThan operator requires two arguments.");

			return args[0].LessThan(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVLessThanMethod; }
		}
		
		
		#endregion
	}
					
	public class LessThanOrEqual : BinaryOperatorBase
	{
	
		#region Singleton pattern
		private LessThanOrEqual()
		{
		}
		
		private static readonly LessThanOrEqual _instance = new LessThanOrEqual();
		public static LessThanOrEqual Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the LessThanOrEqual operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.LessThanOrEqual; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 2)
				throw new PrexoniteException("The LessThanOrEqual operator requires two arguments.");

			return args[0].LessThanOrEqual(sctx,args[1]);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVLessThanOrEqualMethod; }
		}
		
		
		#endregion
	}
					
	public class UnaryNegation : UnaryOperatorBase
	{
	
		#region Singleton pattern
		private UnaryNegation()
		{
		}
		
		private static readonly UnaryNegation _instance = new UnaryNegation();
		public static UnaryNegation Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the UnaryNegation operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.UnaryNegation; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 1)
				throw new PrexoniteException("The UnaryNegation operator requires one argument.");

			
			return args[0].UnaryNegation(sctx);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVUnaryNegationMethod; }
		}
		
		
		#endregion
	}
					
	public class OnesComplement : UnaryOperatorBase
	{
	
		#region Singleton pattern
		private OnesComplement()
		{
		}
		
		private static readonly OnesComplement _instance = new OnesComplement();
		public static OnesComplement Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the OnesComplement operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.OnesComplement; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 1)
				throw new PrexoniteException("The OnesComplement operator requires one argument.");

			
			return args[0].OnesComplement(sctx);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVOnesComplementMethod; }
		}
		
		
		#endregion
	}
					
	public class LogicalNot : UnaryOperatorBase
	{
	
		#region Singleton pattern
		private LogicalNot()
		{
		}
		
		private static readonly LogicalNot _instance = new LogicalNot();
		public static LogicalNot Instance
		{
			get { return _instance; }
		}
		#endregion
		
		///<summary>
		///The alias used for the LogicalNot operation by the compiler.
		///</summary>
		public static string DefaultAlias
		{
			get { return OperatorNames.Prexonite.LogicalNot; }
		}
		
		#region PCommand implementation
		
		public override PValue Run(StackContext sctx, PValue[] args)
		{
			if(args.Length < 1)
				throw new PrexoniteException("The LogicalNot operator requires one argument.");

			
			return args[0].LogicalNot(sctx);			
		}
		
		#endregion
		
		#region CIL extension
		
		protected override MethodInfo OperationMethod
		{
			get { return Compiler.Cil.Compiler.PVLogicalNotMethod; }
		}
		
		
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
 
