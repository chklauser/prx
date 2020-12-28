// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using NUnit.Framework.Constraints;
using Prexonite;

namespace PrexoniteTests
{
    public static class Meta
    {
        public static Constraint ContainsKey(string key)
        {
            return new ContainsKeyConstraint(key);
        }

        public static Constraint Contains(string key, MetaEntry value)
        {
            return new ExactEqualityConstraint(key, value);
        }

        public static Constraint ContainsExact(string key, MetaEntry value)
        {
            return
                new ContainsKeyConstraint(key).And.Matches(new ExactEqualityConstraint(key, value));
        }

        private class ExactEqualityConstraint : Constraint
        {
            private readonly string _key;
            private readonly MetaEntry _expectedEntry;

            public ExactEqualityConstraint(string key, MetaEntry expectedEntry) : base(key, expectedEntry)
            {
                if (expectedEntry == null)
                    throw new ArgumentNullException(nameof(expectedEntry));

                _key = key ?? throw new ArgumentNullException(nameof(key));
                _expectedEntry = expectedEntry;
            }

            #region Overrides of Constraint

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                object actualObj = actual;
                return new ConstraintResult(this, actualObj, _matches(actualObj));
            }

            private bool _matches(object actual)
            {
                if (!(actual is IHasMetaTable ihmt))
                {
                    return false;
                }

                var actualEntry = ihmt.Meta[_key];

                return actualEntry.EntryType == _expectedEntry.EntryType && actualEntry.Equals(_expectedEntry);
            }

            #endregion
        }

        private class ContainsKeyConstraint : Constraint
        {
            private readonly string _key;

            public ContainsKeyConstraint(string key) : base(key)
            {
                _key = key ?? throw new ArgumentNullException(nameof(key));
            }

            #region Overrides of Constraint

            public override ConstraintResult ApplyTo<TActual>(TActual actual)
            {
                var actualValue = actual;
                return new ConstraintResult(this, actualValue, _matches(actualValue));
            }

            private bool _matches(object actual)
            {
                IHasMetaTable ihmt;
                MetaTable mt;
                if ((ihmt = actual as IHasMetaTable) != null)
                    mt = ihmt.Meta;
                else
                    return false;

                return mt.ContainsKey(_key);
            }

            #endregion
        }
    }
}