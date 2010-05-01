/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using Prexonite.Types;
using System.Dynamic;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// PValue objects represents any kind of data inside the Prexonite VM.  They are composed of any CLR object and an instance of <see cref="PType" />.
    /// </summary>
    /// <example>
    /// The following example shows how to create and deal with PValue instances. (Note that all actions inside the Prexonite VM require a reference to the current <see cref="StackContext" />.)
    /// <code>
    /// using Prexonite;
    /// using Prexonite.Types;
    /// public static class PValueSample
    /// {
    ///     public static void main()
    ///     {
    ///         //Create an empty context
    ///         Engine engine = new Engine();
    ///         Application app = new Application();
    ///         PFunction root = new PFunction(app);
    ///         FunctionContext fctx = root.CreateFunctionContext(engine);
    ///         //In your application, you will always have some FunctionContext or StackContext references running around...
    ///         
    ///         //Create an integer PValue
    ///         PValue pv0 = PType.Int.CreatePValue(55);
    ///         //Add this integer to a newly created floating point PValue
    ///         PValue pv1 = pv0.Addition(fctx, PType.Real.CreatePValue(5.36));
    ///         //Multiply the result with 5 (using the implicit conversion operator for integers)
    ///         PValue pv2 = pv1.Multiply(fctx, 5);
    ///         //Finally turn the result into a string using a dynamic (instance) call
    ///         PValue result = pv0.DynamicCall(fctx, new PValue[] {}, PCall.Get, "ToString");
    ///
    ///         Console.WriteLine("The result of (55 + 5.36)*5 is {0}", result.Value);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// <para> 
    ///     The <see cref="Value"/> property only returns null if the <see cref="Type"/> of a PValue is <see cref="PType.Null"/>.
    /// </para>
    /// <para>
    ///     All classes thath inherit from <see cref="PType"/> provide an instance method called <see cref="PType.CreatePValue"/> to safely create PValue instances.
    /// </para>
    /// </remarks>
    /// <seealso cref="PType"/>
    /// <seealso cref="PVariable"/>
    public sealed class PValue : DynamicObject,
                                 IIndirectCall,
                                 IObject
    {
        #region Internals

        private readonly object _value;
        private readonly PType _type;

        /// <summary>
        /// Creates a new instance of PValue with the supplied <paramref name="value"/> and <paramref name="type"/>.
        /// Please note that the constructor does not do any type checking unlike the <see cref="PType.CreatePValue"/> methods.
        /// </summary>
        /// <param name="value">The object to encapsulated in a PValue.</param>
        /// <param name="type">The type of the value inside the Prexonite VM.</param>
        [DebuggerStepThrough]
        public PValue(object value, PType type)
        {
            if (value == null)
                type = NullPType.Instance;
            else if ((object) type == null)
                throw new ArgumentNullException("type");

            _value = value;
            _type = type;
        }

        /// <summary>
        /// Provides readonly access to the CLR object representing the value in the Prexonite VM.
        /// </summary>
        /// <value>An object reference. <see cref="Value"/> is only null, if <see cref="Type"/> returns a <see cref="NullPType"/> object.</value>
        public object Value
        {
            [DebuggerStepThrough]
            get { return _value; }
        }

        /// <summary>
        /// Provides readonly access to the <see cref="PType"/> associated with the PValue object.
        /// </summary>
        /// <value>An instance of <see cref="PType"/>.</value>
        public PType Type
        {
            [DebuggerStepThrough]
            get { return _type; }
        }

        #endregion

        #region PType proxy methods

        /// <summary>
        /// Performs a dynamic (instance) call on the value. You should use <see cref="TryDynamicCall"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the call.</param>
        /// <param name="args">An array of arguments to be passed in the call.</param>
        /// <param name="call">The semantic context of the call. <see cref="PCall.Get"/> and <see cref="PCall.Set"/> will have the same effect in most cases. See the TryDynamicCall of <see cref="PType"/> implementors for details.</param>
        /// <param name="id">The name of the member to call. <paramref name="id"/> might or might not be case sensitive, depending on the PType.
        /// The string can be empty if you want to call the default member (C# only supports default indexers).</param>
        /// <returns>The value returned by the dynamic (instance) call. In case the call does not have a return value, a PValue containing null is returned.</returns>
        /// <exception cref="InvalidCallException">Thrown if the call is not successful.</exception>
        [DebuggerStepThrough]
        public PValue DynamicCall(StackContext sctx, PValue[] args, PCall call, string id)
        {
            return _type.DynamicCall(sctx, this, args, call, id);
        }

        /// <summary>
        /// Tries to perform a dynamic (instance) call on the value and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the call.</param>
        /// <param name="args">An array of arguments to be passed in the call.</param>
        /// <param name="call">The semantic context of the call. <see cref="PCall.Get"/> and <see cref="PCall.Set"/> will have the same effect in most cases. See the TryDynamicCall of <see cref="PType"/> implementors for details.</param>
        /// <param name="id">The name of the member to call. <paramref name="id"/> might or might not be case sensitive, depending on the PType.
        /// The string can be empty if you want to call the default member (C# only supports default indexers).</param>
        /// <param name="result">Contains the value returned by the dynamic (instance) call. In case the call does not have a return value, a PValue containing null is returned.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            return _type.TryDynamicCall(sctx, this, args, call, id, out result);
        }

        /// <summary>
        /// Performs a conversion on the value and returns the resulting PValue. You should use <see cref="TryConvertTo(StackContext,PType,bool,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="target">The type you would like to convert the value to.</param>
        /// <param name="useExplicit">A boolean that indicates whether explicit conversion operators should be used or not</param>
        /// <returns>Contains the value returned by the conversion.</returns>
        /// <exception cref="InvalidConversionException">Thrown if the conversion is not successful.</exception>
        [DebuggerStepThrough]
        public PValue ConvertTo(StackContext sctx, PType target, bool useExplicit)
        {
            return _type.ConvertTo(sctx, this, target, useExplicit);
        }

        /// <summary>
        /// Performs an implicit conversion on the value and returns the resulting PValue. You should use <see cref="TryConvertTo(StackContext,PType,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// This overload does not use explicit conversion operators.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="target">The type you would like to convert the value to.</param>
        /// <returns>Contains the value returned by the conversion.</returns>
        /// <exception cref="InvalidConversionException">Thrown if the conversion is not successful.</exception>
        [DebuggerStepThrough]
        public PValue ConvertTo(StackContext sctx, PType target)
        {
            return ConvertTo(sctx, target, false);
        }

        /// <summary>
        /// Converts the value to to a PValue with <c><see cref="PType.Object">PType.Object</see>[<paramref name="clrTarget">target clr type</paramref>]</c> as it's <see cref="Type"/> and returns the result. You should use <see cref="TryConvertTo(StackContext,System.Type,bool,out PValue)"/> whenever possible to prevent <see cref="InvalidConversionException">InvalidConversionExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="clrTarget">The <see cref="System.Type"/> you would like to convert the value to.</param>
        /// <param name="useExplicit">A boolean that indicates whether explicit conversion operators should be used or not</param>
        /// <returns>Contains the value returned by the conversion.</returns>
        /// <exception cref="InvalidConversionException">Thrown if the conversion is not successful.</exception>
        [DebuggerStepThrough]
        public PValue ConvertTo(StackContext sctx, Type clrTarget, bool useExplicit)
        {
            return _type.ConvertTo(sctx, this, clrTarget, useExplicit);
        }

        /// <summary>
        /// Implicitly converts the value to to a PValue with <c><see cref="PType.Object">PType.Object</see>[<paramref name="clrTarget">target clr type</paramref>]</c> as it's <see cref="Type"/> and returns the result. You should use <see cref="TryConvertTo(StackContext,System.Type,bool,out PValue)"/> whenever possible to prevent <see cref="InvalidConversionException">InvalidConversionExceptions</see> from being thrown too often.
        /// This overload does not use explicit conversion operators.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="clrTarget">The <see cref="System.Type"/> you would like to convert the value to.</param>
        /// <returns>Contains the value returned by the conversion.</returns>
        /// <exception cref="InvalidConversionException">Thrown if the conversion is not successful.</exception>
        [DebuggerStepThrough]
        public PValue ConvertTo(StackContext sctx, Type clrTarget)
        {
            return ConvertTo(sctx, clrTarget, false);
        }

        /// <summary>
        /// Converts the value to an instance of <typeparamref name="T"/> using the ObjectPType.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="useExplicit">A boolean that indicates whether explicit conversion operators should be used or not</param>
        /// <typeparam name="T">The type to convert the PValue into.</typeparam>
        /// <returns>Contains the value returned by the conversion.</returns>
        /// <exception cref="InvalidConversionException">Thrown if the conversion is not successful.</exception>
        [DebuggerStepThrough]
        public T ConvertTo<T>(StackContext sctx, bool useExplicit)
        {
            return (T) _type.ConvertTo(sctx, this, typeof (T), useExplicit).Value;
        }

        /// <summary>
        /// Implicitly converts the value to an instance of <typeparamref name="T"/> using the ObjectPType.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <typeparam name="T">The type to convert the PValue into.</typeparam>
        /// <returns>Contains the value returned by the conversion.</returns>
        /// <exception cref="InvalidConversionException">Thrown if the conversion is not successful.</exception>
        [DebuggerStepThrough]
        public T ConvertTo<T>(StackContext sctx)
        {
            return ConvertTo<T>(sctx, false);
        }

        /// <summary>
        /// Tries to convert the PValue to an object of Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The CLR type to convert the PValue to.</typeparam>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="useExplicit">A boolean that indicates whether explicit conversion operators should be used or not.</param>
        /// <param name="result">Contains the value returned by the conversion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryConvertTo<T>(StackContext sctx, bool useExplicit, out T result)
        {
            result = default(T);
            PValue r;
            if ((!_type.TryConvertTo(sctx, this, sctx.ParentEngine.PTypeMap[typeof (T)], useExplicit, out r)) || !(r.Value is T))
                return false;

            result = (T) r.Value;
            return true;
        }

        /// <summary>
        /// Tries to implicitly convert the PValue to an object of Type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The CLR type to convert the PValue to.</typeparam>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="result">Contains the value returned by the conversion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryConvertTo<T>(StackContext sctx, out T result)
        {
            return TryConvertTo(sctx, false, out result);
        }

        /// <summary>
        /// Tries to convert the value to a PValue with the supplied <paramref name="target">target type</paramref> as it's <see cref="Type"/> and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="target">The type you would like to convert the value to.</param>
        /// <param name="useExplicit">A boolean that indicates whether explicit conversion operators should be used or not</param>
        /// <param name="result">Contains the value returned by the conversion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryConvertTo(
            StackContext sctx, PType target, bool useExplicit, out PValue result)
        {
            return _type.TryConvertTo(sctx, this, target, useExplicit, out result);
        }

        /// <summary>
        /// Tries to implicitly convert the value to a PValue with the supplied <paramref name="target">target type</paramref> as it's <see cref="Type"/> and stores the result in the out parameter <paramref name="result"/>.
        /// This overload does not use explicit conversion operators.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="target">The type you would like to convert the value to.</param>
        /// <param name="result">Contains the value returned by the conversion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryConvertTo(StackContext sctx, PType target, out PValue result)
        {
            return TryConvertTo(sctx, target, false, out result);
        }

        /// <summary>
        /// Tries to convert the value to a PValue with <c><see cref="PType.Object">PType.Object</see>[<paramref name="clrTarget">target clr type</paramref>]</c> as it's <see cref="Type"/> and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="clrTarget">The <see cref="System.Type"/> you would like to convert the value to.</param>
        /// <param name="useExplicit">A boolean that indicates whether explicit conversion operators should be used or not</param>
        /// <param name="result">Contains the value returned by the conversion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryConvertTo(
            StackContext sctx, Type clrTarget, bool useExplicit, out PValue result)
        {
            return _type.TryConvertTo(sctx, this, clrTarget, useExplicit, out result);
        }

        /// <summary>
        /// Tries to implicitly convert the value to a PValue with <c><see cref="PType.Object">PType.Object</see>[<paramref name="clrTarget">target clr type</paramref>]</c> as it's <see cref="Type"/> and stores the result in the out parameter <paramref name="result"/>.
        /// This overload does not use explicit conversion operators.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the conversion.</param>
        /// <param name="clrTarget">The <see cref="System.Type"/> you would like to convert the value to.</param>
        /// <param name="result">Contains the value returned by the conversion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryConvertTo(StackContext sctx, Type clrTarget, out PValue result)
        {
            return TryConvertTo(sctx, clrTarget, false, out result);
        }

        /// <summary>
        /// Tries to perform an indirect call on the value and stores the result in the out parameter <paramref name="result"/>
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the call.</param>
        /// <param name="args">An array of arguments to be passed in the call.</param>
        /// <param name="result">Contains the value returned by the indirect call. In case the call does not have a return value, a PValue containing null is returned.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        [DebuggerStepThrough]
        public bool TryIndirectCall(StackContext sctx, PValue[] args, out PValue result)
        {
            return _type.IndirectCall(sctx, this, args, out result);
        }

        /// <summary>
        /// Performs an indirect call on the value and returns the result. You should use <see cref="TryIndirectCall"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the call.</param>
        /// <param name="args">An array of arguments to be passed in the call.</param>
        /// <returns>Contains the value returned by the indirect call. In case the call does not have a return value, a PValue containing null is returned.</returns>
        /// <exception cref="InvalidCallException">Thrown if the call is not successful.</exception>
        [DebuggerStepThrough]
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _type.IndirectCall(sctx, this, args);
        }

        #region Operators

        #region Try-Variants

        //UNARY

        /// <summary>
        /// Tries to perform a unary negation on the value and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the unary negation.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the unary negation was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        [DebuggerStepThrough]
        public bool UnaryNegation(StackContext sctx, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.UnaryNegation(sctx, this, out result);
        }

        /// <summary>
        /// Tries to perform a logical not operation on the value and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the logical not operation.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the logical not operation was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        [DebuggerStepThrough]
        public bool LogicalNot(StackContext sctx, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.LogicalNot(sctx, this, out result);
        }

        /// <summary>
        /// Tries to take the values complement and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to take the value's complement.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if taking the values complement was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        [DebuggerStepThrough]
        public bool OnesComplement(StackContext sctx, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.OnesComplement(sctx, this, out result);
        }

        /// <summary>
        /// Tries to increment the value and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to increment the value.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if incrementing the value was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        [DebuggerStepThrough]
        public bool Increment(StackContext sctx, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.Increment(sctx, this, out result);
        }

        /// <summary>
        /// Tries to decrement the value and stores the result in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to decrement the value.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if decrementing the value was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        [DebuggerStepThrough]
        public bool Decrement(StackContext sctx, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.Decrement(sctx, this, out result);
        }

        //BINARY

        /// <summary>
        /// Tries to apply the addition operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the addition. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the addition operator.</param>
        /// <param name="rightOperand">The right operand of the addition.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the addition was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Addition(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Addition(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Addition(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the subtraction operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the subtraction. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the subtraction operator.</param>
        /// <param name="rightOperand">The right operand of the subtraction.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the subtraction was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Subtraction(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Subtraction(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Subtraction(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the multiplication operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the multiplication. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the multiplication operator.</param>
        /// <param name="rightOperand">The right operand of the multiplication.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the multiplication was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Multiply(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Multiply(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Multiply(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the division operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the division. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the division operator.</param>
        /// <param name="rightOperand">The right operand of the division.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the division was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Division(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Division(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Division(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the modulus operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the modulus division. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the modulus operator.</param>
        /// <param name="rightOperand">The right operand of the modulus division.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the modulus division was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Modulus(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Modulus(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Modulus(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the bitwise AND operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the bitwise AND operation. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the bitwise AND operator.</param>
        /// <param name="rightOperand">The right operand of the bitwise AND operator.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the bitwise AND operation was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool BitwiseAnd(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.BitwiseAnd(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.BitwiseAnd(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the bitwise OR operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the bitwise OR operation. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the bitwise OR operator.</param>
        /// <param name="rightOperand">The right operand of the bitwise OR operator.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the bitwise OR operation was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool BitwiseOr(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.BitwiseOr(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.BitwiseOr(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to apply the bitwise XOR operator to the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the bitwise XOR operation. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to apply the bitwise XOR operator.</param>
        /// <param name="rightOperand">The right operand of the bitwise XOR operator.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the bitwise XOR operation was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool ExclusiveOr(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.ExclusiveOr(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.ExclusiveOr(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to test the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand for equality. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the equality test. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for equality.</param>
        /// <param name="rightOperand">The right operand of the equality test.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the equality test was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Equality(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Equality(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Equality(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to test the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand for inequality. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the inequality test. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for inequality.</param>
        /// <param name="rightOperand">The right operand of the inequality test.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the inequality test was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool Inequality(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.Inequality(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.Inequality(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to test if the instance as the left operand is greater than the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the greater than-test. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for greater than.</param>
        /// <param name="rightOperand">The right operand of the greater than-test.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the greater than-test was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool GreaterThan(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.GreaterThan(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.GreaterThan(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to test if the instance as the left operand is greater than or equal to the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the greater than or equal-test. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for greater than or equal.</param>
        /// <param name="rightOperand">The right operand of the greater than or equal-test.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the greater than or equal-test was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool GreaterThanOrEqual(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.GreaterThanOrEqual(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.GreaterThanOrEqual(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to test if the instance as the left operand is less than the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the less than-test. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for less than.</param>
        /// <param name="rightOperand">The right operand of the less than-test.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the less than-test was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool LessThan(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.LessThan(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.LessThan(sctx, this, rightOperand, out result);
        }

        /// <summary>
        /// Tries to test if the instance as the left operand is less than or equal to the supplied <paramref name="rightOperand"/> as the right operand. 
        /// The method first gives the left operand's <see cref="Type"/> the chance to handle the less than or equal-test. Should that fail, the right operand's <see cref="PType"/> is used.
        /// The result is stored in the out parameter <paramref name="result"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for less than or equal.</param>
        /// <param name="rightOperand">The right operand of the less than or equal-test.</param>
        /// <param name="result">Contains the value returned by the call.</param>
        /// <returns>True if the less than or equal-test was successful; false otherwise.</returns>
        /// <remarks>Note that the value of <paramref name="result" /> is undefined (and therefor not to be used) if the method call returned false.</remarks>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        [DebuggerStepThrough]
        public bool LessThanOrEqual(StackContext sctx, PValue rightOperand, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (Type.LessThanOrEqual(sctx, this, rightOperand, out result))
                return true;
            else
                return rightOperand.Type.LessThanOrEqual(sctx, this, rightOperand, out result);
        }

        #endregion //Try-Variants

        #region Failing-Variants

        //UNARY

        /// <summary>
        /// Performs a unary negation on the value and returns the result. You should use <see cref="UnaryNegation(StackContext,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the unary negation.</param>
        /// <returns>The result of the unary negation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        public PValue UnaryNegation(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.UnaryNegation(sctx, this);
        }

        /// <summary>
        /// Performs a logical not operation on the value and returns the result. You should use <see cref="LogicalNot(StackContext,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the logical not operation.</param>
        /// <returns>The result of the logical not operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        public PValue LogicalNot(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.LogicalNot(sctx, this);
        }

        /// <summary>
        /// Returns the value's complement. You should use <see cref="OnesComplement(StackContext,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to take the value's complement.</param>
        /// <returns>The result of the logical not operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        public PValue OnesComplement(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.OnesComplement(sctx, this);
        }

        /// <summary>
        /// Increments the value (non-destructive) and returns the result. You should use <see cref="Increment(StackContext,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to increment the value.</param>
        /// <returns>The incremented value.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        public PValue Increment(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.Increment(sctx, this);
        }

        /// <summary>
        /// Decrements the value (non-destructive) and returns the result. You should use <see cref="Decrement(StackContext,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to decrement the value.</param>
        /// <returns>The decremented value.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="sctx"/> is null.</exception>
        public PValue Decrement(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            return Type.Decrement(sctx, this);
        }

        //BINARY
        /// <summary>
        /// Performs the addition of the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the addition. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Addition(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the addition.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Addition(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Addition(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Addition operator.");
        }

        /// <summary>
        /// Performs the subtraction of the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the subtraction. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Subtraction(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the subtraction.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Subtraction(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Subtraction(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Subtraction operator.");
        }

        /// <summary>
        /// Performs the multiplication of the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the multiplication. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Multiply(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the multiplication.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Multiply(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Multiply(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Multiply operator.");
        }

        /// <summary>
        /// Performs the division of the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the division. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Division(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the division.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Division(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Division(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Division operator.");
        }

        /// <summary>
        /// Performs the modulus division of the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the modulus division. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Modulus(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the modulus division.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the modulus division.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Modulus(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Modulus(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Modulus operator.");
        }

        /// <summary>
        /// Performs the bitwise AND operation on the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the bitwise AND operation. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="BitwiseAnd(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the bitwise AND operation.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the bitwise AND operation.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue BitwiseAnd(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (BitwiseAnd(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the BitwiseAnd operator.");
        }

        /// <summary>
        /// Performs the bitwise OR operation on the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the bitwise OR operation. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="BitwiseOr(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the bitwise OR operation.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the bitwise OR operation.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue BitwiseOr(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (BitwiseOr(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the BitwiseOr operator.");
        }

        /// <summary>
        /// Performs the bitwise XOR operation on the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the bitwise XOR operation. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="ExclusiveOr(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the bitwise XOR operation.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the bitwise XOR operation.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue ExclusiveOr(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (ExclusiveOr(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the ExclusiveOr operator.");
        }

        /// <summary>
        /// Tests the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand for equality.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the test for equality. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Equality(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for equality.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the test.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Equality(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Equality(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Equality operator.");
        }

        /// <summary>
        /// Tests the instance as the left operand and the supplied <paramref name="rightOperand"/> as the right operand for inequality.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the test for inequality. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="Inequality(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to test for inequality.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the test.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue Inequality(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (Inequality(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the Inequality operator.");
        }

        /// <summary>
        /// Tests if the instance as the left operand is greater than the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the greater than-test. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="GreaterThan(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the greater than-test.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the test.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue GreaterThan(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (GreaterThan(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the GreaterThan operator.");
        }

        /// <summary>
        /// Tests if the instance as the left operand is greater than or equal the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the greater than or equal-test. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="GreaterThanOrEqual(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the greater than or equal-test.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the test.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue GreaterThanOrEqual(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (GreaterThanOrEqual(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the GreaterThanOrEqual operator.");
        }

        /// <summary>
        /// Tests if the instance as the left operand is less than the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the less than-test. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="LessThan(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the less than-test.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the test.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue LessThan(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (LessThan(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " supports the LessThan operator.");
        }

        /// <summary>
        /// Tests if the instance as the left operand is less than the supplied <paramref name="rightOperand"/> as the right operand.
        /// This method first gives the left operand's <see cref="Type"/> the chance to carry out the less than-test. Should that fail, the right operands <see cref="Type"/> is used.
        /// In case neither the left nor the right operand's type can handle the operation, an <see cref="InvalidCallException"/> is thrown.
        /// You should use <see cref="LessThanOrEqual(StackContext,PValue,out PValue)"/> whenever possible to prevent <see cref="InvalidCallException">InvalidCallExceptions</see> from being thrown too often.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the less than-test.</param>
        /// <param name="rightOperand">The right operand to the operation.</param>
        /// <returns>The result of the test.</returns>
        /// <exception cref="ArgumentNullException">If either <paramref name="sctx"/> or <paramref name="rightOperand"/> are null.</exception>
        /// <exception cref="InvalidCallException">If neither the left nor the right operand's <see cref="Type"/> can handle the operation.</exception>
        public PValue LessThanOrEqual(StackContext sctx, PValue rightOperand)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand");
            PValue result;
            if (LessThanOrEqual(sctx, rightOperand, out result))
                return result;
            else
                throw new InvalidCallException(
                    "Neither " + Type + " nor " + rightOperand.Type +
                    " LessThanOrEqual the Addition operator.");
        }

        #endregion //Failing-Variants

        #endregion //Operators

        #endregion //PType Proxy

        #region Interaction

        /// <summary>
        /// Allows you to prevent the Prexonite VM from performing type conversions on the PValue object.
        /// </summary>
        /// <value>A boolean value that indicates whether the type lock is in action or not.</value>
        public bool IsTypeLocked { get; set; }

        /// <summary>
        /// Indicates whether the PValue object contains a null reference or not.
        /// </summary>
        /// <value>True if <see cref="Value"/> is null; false otherwise.</value>
        public bool IsNull
        {
            [DebuggerStepThrough]
            get { return _value == null; }
        }

        /// <summary>
        /// Provides access to the (CLR) <see cref="System.Type"/> of the value encapsulated in PValue (or null if the <see cref="Value"/> itself is null).
        /// </summary>
        /// <value>The <see cref="System.Type"/> of the value encapsulated in PValue or null if that value is null.</value>
        public Type ClrType
        {
            [DebuggerStepThrough]
            get { return _value != null ? _value.GetType() : null; }
        }

        /// <summary>
        /// Creates a new PValue with the same value but the supplied <paramref name="type"/> as it's <see cref="PType"/>.
        /// </summary>
        /// <param name="type">The <see cref="PType"/> for PValue to be constructed.</param>
        /// <returns>A new PValue object with the same <see cref="Value"/> as the current instance but with the supplied <paramref name="type"/> as it's <see cref="PType"/>.</returns>
        /// <remarks>If <see cref="Value"/> is null, the PType of the returned PValue will bee <see cref="PType.Null"/>.
        /// Also note that not all <see cref="PType"/> constructors support other CLR types than their natural representation.</remarks>
        [DebuggerStepThrough]
        public PValue ReinterpretAs(PType type)
        {
            return type.CreatePValue(_value);
        }

        /// <summary>
        /// Creates a new PValue object with the same value but with it's natural CLR type instead.
        /// </summary>
        /// <returns>A new PValue object with the same value but with it's natural CLR type instead (<c><see cref="ObjectPType"/>[<see cref="Value"/>.GetType()]</c>).</returns>
        /// <remarks>If <see cref="Value"/> is null, the PType of the returned PValue will bee <see cref="PType.Null"/>.</remarks>
        [DebuggerStepThrough]
        public PValue ToObject()
        {
            return PType.Object.CreatePValue(_value);
        }

        /// <summary>
        /// Returns a human read-able string representation of the PValue instance.
        /// </summary>
        /// <returns>A string that represents the PValue object in human read-able form.</returns>
        /// <remarks>These strings cannot be used for round trips as <c>Object.ToString</c> is a narrowing conversion in most cases.</remarks>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return
                String.Concat(
                    "{", (_value == null ? "-NULL-" : String.Concat(_value, "~", _type)), "}");
        }

        /// <summary>
        /// CallToString tries to invoke "ToString" on the <see cref="Value"/> and returns the result. In case the call fails, CallToString falls back to <see cref="ToString"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to execute the call to "ToString".</param>
        /// <returns>Either the result of the "ToString" call or, in case of a failure, the result of <see cref="ToString">PValue.ToString</see>.</returns>
        /// <remarks>Note that unlike in the CLR, null values in Prexonite <strong>do</strong> implement "ToString".</remarks>
        [DebuggerStepThrough]
        public string CallToString(StackContext sctx)
        {
            PValue text;
            if (Type == PType.String)
                return (string) Value;
            else if (TryDynamicCall(sctx, new PValue[] {}, PCall.Get, "ToString", out text))
                return text.Value.ToString();
            else
                return ToString();
        }

        public override int GetHashCode()
        {
            if (_value == null)
                return 0;
            else
                return _type.GetHashCode() ^ _value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as PValue;
            if (o == null)
                return false;

            if (o.IsNull && IsNull)
                return true;

            return _type.Equals(o._type) && _value.Equals(o._value);
        }

        #endregion

        #region CLR Conversion operators

        //only the natural representation is implicit!
        //PInt

        /// <summary>
        /// Encapsulates an integer (System.Int32) as a PValue object.
        /// </summary>
        /// <param name="number">A System.Int32 to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied integer with <see cref="PType.Int"/> as its PType.</returns>
        /// <remarks>As System.Int32 is the <strong>natural representation</strong> of <see cref="PType.Int"/> values, this conversion is implicit.</remarks>
        [DebuggerStepThrough]
        public static implicit operator PValue(Int32 number)
        {
            return PType.Int.CreatePValue(number);
        }

        /// <summary>
        /// Represents an unsigned integer (System.UInt32) as an integer (System.Int32) in a PValue object.
        /// </summary>
        /// <param name="number">A System.UInt32 to be used like a System.Int32 within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied integer (signed) with <see cref="PType.Int"/> as its PType.</returns>
        /// <remarks><see cref="PType.Int"/> cannot represent integers other than System.Int32, but you may wish to convert them to signed integers and then create the appropriate PValues, which is exactly what this conversion operator does.
        /// If you have to do unsigned integer math inside the Prexonite VM, use <c><see cref="ObjectPType">PType.Object</see>[typeof(System.UInt32)]</c> as the PType.</remarks>
        [DebuggerNonUserCode, CLSCompliant(false)]
        public static explicit operator PValue(UInt32 number)
        {
            return PType.Int.CreatePValue((int) number);
        }

        /// <summary>
        /// Represents an unsigned byte (System.Byte) as an integer (System.Int32) in a PValue object.
        /// </summary>
        /// <param name="number">A System.Byte to be used like a System.Int32 within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied byte as an integer with <see cref="PType.Int"/> as its PType.</returns>
        /// <remarks><see cref="PType.Int"/> cannot represent integers other than System.Int32, but you may wish to convert them to integers and then create the appropriate PValues, which is exactly what this conversion operator does.</remarks>
        [DebuggerNonUserCode, CLSCompliant(false)]
        public static explicit operator PValue(Byte number)
        {
            return PType.Int.CreatePValue((int) number);
        }

        /// <summary>
        /// Represents an signed byte (System.SByte) as an integer (System.Int32) in a PValue object.
        /// </summary>
        /// <param name="number">A System.SByte to be used like a System.Int32 within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied signed byte as an integer with <see cref="PType.Int"/> as its PType.</returns>
        /// <remarks><see cref="PType.Int"/> cannot represent integers other than System.Int32, but you may wish to convert them to integers and then create the appropriate PValues, which is exactly what this conversion operator does.</remarks>
        [DebuggerNonUserCode, CLSCompliant(false)]
        public static explicit operator PValue(SByte number)
        {
            return PType.Int.CreatePValue((int) number);
        }

        /// <summary>
        /// Truncates a long integer (System.Int64) to an integer (System.Int32) in a PValue object.
        /// </summary>
        /// <param name="number">A System.Int64 to be used like a System.Int32 within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied long integer truncated to an integer with <see cref="PType.Int"/> as its PType.</returns>
        /// <remarks><see cref="PType.Int"/> cannot represent integers other than System.Int32, but you may wish to convert them to integers and then create the appropriate PValues, which is exactly what this conversion operator does.
        /// If you have to do long integer math inside the Prexonite VM, use <c><see cref="ObjectPType">PType.Object</see>[typeof(System.Int64)]</c> as the PType.</remarks>
        [DebuggerStepThrough]
        public static explicit operator PValue(Int64 number)
        {
            return PType.Int.CreatePValue((int) number);
        }

        /// <summary>
        /// Represents a short integer (System.Int16) as an integer (System.Int32) in a PValue object.
        /// </summary>
        /// <param name="number">A System.Int16 to be used like a System.Int32 within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied short integer as an integer with <see cref="PType.Int"/> as its PType.</returns>
        /// <remarks><see cref="PType.Int"/> cannot represent integers other than System.Int32, but you may wish to convert them to integers and then create the appropriate PValues, which is exactly what this conversion operator does.</remarks>
        [DebuggerStepThrough]
        public static explicit operator PValue(Int16 number)
        {
            return PType.Int.CreatePValue((int) number);
        }

        //PReal
        /// <summary>
        /// Encapsulates a double precision floating point number (System.Double) as a PValue object.
        /// </summary>
        /// <param name="number">A System.Double to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied double with <see cref="PType.Real"/> as its PType.</returns>
        /// <remarks>As System.Double is the <strong>natural representation</strong> of <see cref="PType.Real"/> values, this conversion is implicit.</remarks>
        [DebuggerStepThrough]
        public static implicit operator PValue(Double number)
        {
            return PType.Real.CreatePValue(number);
        }

        /// <summary>
        /// Encapsulates a single precision floating point number (System.Single) as a double precision floating point number in a PValue object.
        /// </summary>
        /// <param name="number">A System.Single to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied float with <see cref="PType.Real"/> as its PType.</returns>
        /// <remarks><see cref="PType.Real"/> cannot represent floating point numbers other than System.Double, but you may wish to convert them to double precision floating point numbers and then create the appropriate PValues, which is exactly what this conversion operator does.</remarks>
        [DebuggerStepThrough]
        public static explicit operator PValue(Single number)
        {
            return PType.Real.CreatePValue((double) number);
        }

        /// <summary>
        /// Truncates a decimal number (System.Decimal) to a double precision floating point number, encapsulated in a PValue object.
        /// </summary>
        /// <param name="number">A System.Decimal to be used as a double precision floating point number within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied decimal number as a double precision floating point number with <see cref="PType.Real"/> as its PType.</returns>
        /// <remarks><see cref="PType.Real"/> cannot represent a decimal number, but you may wish to convert it to double precision floating point numbers and then create the appropriate PValue, which is exactly what this conversion operator does.</remarks>
        [DebuggerStepThrough]
        public static explicit operator PValue(Decimal number)
        {
            return PType.Object.CreatePValue((double) number);
        }

        //PChar

        /// <summary>
        /// Encapsulates a char value (System.Char) as a PValue object.
        /// </summary>
        /// <param name="c">A System.Char to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied char value with <see cref="PType.Char"/> as its PType.</returns>
        /// <remarks>As System.Char ist the <strong>natural representation</strong> of <see cref="PType.Char"/> values, the conversion is implicit.</remarks>
        public static implicit operator PValue(char c)
        {
            return CharPType.CreatePValue(c);
        }

        //PBool
        /// <summary>
        /// Encapsulates a boolean value (System.Boolean) as a PValue object.
        /// </summary>
        /// <param name="state">A System.Boolean to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied boolean value with <see cref="PType.Bool"/> as its PType.</returns>
        /// <remarks>As System.Boolean is the <strong>natural representation</strong> of <see cref="PType.Bool"/> values, this conversion is implicit.</remarks>
        [DebuggerStepThrough]
        public static implicit operator PValue(Boolean state)
        {
            return PType.Bool.CreatePValue(state);
        }

        //PString
        /// <summary>
        /// Encapsulates a string (System.String) as a PValue object.
        /// </summary>
        /// <param name="text">A System.String to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied string with <see cref="PType.String"/> as its PType.</returns>
        /// <remarks>As System.String is the <strong>natural representation</strong> of <see cref="PType.String"/> values, this conversion is implicit.</remarks>
        [DebuggerStepThrough]
        public static implicit operator PValue(String text)
        {
            return PType.String.CreatePValue(text);
        }

        //PList
        /// <summary>
        /// Encapsulates a list of PValues (System.Collections.Generic.List&lt;PValue&gt;) as a PValue object.
        /// </summary>
        /// <param name="list">A System.Collections.Generic.List&lt;PValue&gt; to be used within the Prexonite VM.</param>
        /// <returns>A PValue object containing the supplied list with <see cref="PType.List"/> as its PType.</returns>
        /// <remarks>Although System.Collections.Generic.List&lt;PValue&gt; is the <strong>natural representation</strong> of <see cref="PType.String"/> values, this conversion is <strong>not</strong> implicit, as it might lead to programming mistakes.</remarks>
        [DebuggerStepThrough]
        public static explicit operator PValue(List<PValue> list)
        {
            return PType.List.CreatePValue(list);
        }

        /// <summary>
        /// Encapsulates a key-value pair of PValues as a PValue object.
        /// </summary>
        /// <param name="pair">A key-value pair of PValues</param>
        /// <returns>A PValue object containing the supplied key-value pair.</returns>
        [DebuggerStepThrough]
        public static implicit operator PValue(PValueKeyValuePair pair)
        {
            return PType.Object.CreatePValue(pair);
        }

        #endregion

        #region IObject Members

        bool IObject.TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (Engine.StringsAreEqual(id, "self"))
                result = this;
            else
                result = null;

            return result != null;
        }

        #endregion

        public static string ToDebugString(PValue val)
        {
            if (val == null)
                return "null";
            switch (val.Type.ToBuiltIn())
            {
                case PType.BuiltIn.Int:
                case PType.BuiltIn.Real:
                case PType.BuiltIn.Bool:
                    return val.Value.ToString();
                case PType.BuiltIn.String:
                    return "\"" + StringPType.Escape(val.Value as string) + "\"";
                case PType.BuiltIn.Null:
                    return NullPType.Literal;
                case PType.BuiltIn.Object:
                    return "{" + val.Value + "}";
                case PType.BuiltIn.List:
                    var lst = val.Value as List<PValue>;
                    if (lst == null)
                        return "[]";
                    var buffer = new StringBuilder("[");
                    for (var i = 0; i < lst.Count - 1; i++)
                    {
                        buffer.Append(ToDebugString(lst[i]));
                        buffer.Append(",");
                    }
                    if (lst.Count > 0)
                    {
                        buffer.Append(ToDebugString(lst[lst.Count - 1]));
                    }
                    buffer.Append("]");
                    return buffer.ToString();
                default:
                    return "#" + val + "#";
            }
        }

        #region DLR (dynamic) interface

        private static bool _tryParseCall(object[] args, out StackContext sctx, out PValue[] icargs)
        {
            if (args.Length > 0 && (sctx = args[0] as StackContext) != null)
            {
                var localStcx = sctx;
                icargs = args.Skip(1).Select(o => o as PValue ?? localStcx.CreateNativePValue(o)).ToArray();
                return true;
            }
            else
            {
                sctx = null;
                icargs = null;
                return false;
            }
        }

        public override bool  TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = null;

            StackContext sctx;
            PValue[] icargs;
            if (_tryParseCall(args, out sctx, out icargs))
                result = IndirectCall(sctx, icargs);

            return result != null || base.TryInvoke(binder, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            StackContext sctx;
            PValue[] icargs;

            PValue pvresult;
            if (_tryParseCall(args, out sctx, out icargs) && TryDynamicCall(sctx, icargs, PCall.Get, binder.Name, out pvresult))
                result = pvresult;

            return result != null || base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;
            StackContext sctx;
            PValue[] icargs; 

            PValue pvresult;
            if (_tryParseCall(indexes, out sctx, out icargs) && TryDynamicCall(sctx, icargs, PCall.Get, String.Empty, out pvresult))
            {
                result = pvresult;
            }

            return result != null || base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            StackContext sctx;
            PValue[] icargs;
            var args = new object[indexes.Length + 1];
            Array.Copy(indexes, args, indexes.Length);
            args[args.Length - 1] = value;

            PValue pvresult;
            return (_tryParseCall(args, out sctx, out icargs) && TryDynamicCall(sctx, icargs, PCall.Set, String.Empty, out pvresult))
                   || base.TrySetIndex(binder, indexes, value);
        }

        #endregion
    }
}