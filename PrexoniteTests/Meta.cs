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
            private MetaEntry _actualEntry;
            private bool _typeMismatch;

            public ExactEqualityConstraint(string key, MetaEntry expectedEntry)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                if (expectedEntry == null)
                    throw new ArgumentNullException("expectedEntry");

                _key = key;
                _expectedEntry = expectedEntry;
            }

            #region Overrides of Constraint

            public override bool Matches(object actual)
            {
                this.actual = actual;

                var ihmt = actual as IHasMetaTable;
                if (ihmt == null)
                {
                    this.actual = null;
                    return false;
                }

                _actualEntry = ihmt.Meta[_key];

                if (_actualEntry.EntryType != _expectedEntry.EntryType)
                {
                    _typeMismatch = true;
                    return false;
                }

                _typeMismatch = false;
                return _actualEntry.Equals(_expectedEntry);
            }

            public override void WriteDescriptionTo(MessageWriter writer)
            {
                if (actual == null)
                {
                    writer.WriteMessageLine("Actual value does not have a meta table.");
                }
                else if (_typeMismatch)
                {
                    writer.WriteMessageLine("Meta entry type doesn't match.");
                    writer.WriteExpectedValue(_expectedEntry.EntryType);
                    writer.WriteActualValue(_actualEntry.EntryType);
                }
                else
                {
                    writer.WriteMessageLine(
                        "actual meta entry {0} should match expected entry {1}", _actualEntry,
                        _expectedEntry);
                }
            }

            #endregion
        }

        private class ContainsKeyConstraint : Constraint
        {
            private readonly string _key;

            public ContainsKeyConstraint(string key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                _key = key;
            }

            #region Overrides of Constraint

            public override bool Matches(object actual)
            {
                this.actual = actual;

                IHasMetaTable ihmt;
                MetaTable mt;
                if ((ihmt = actual as IHasMetaTable) != null)
                    mt = ihmt.Meta;
                else
                    return false;

                return mt.ContainsKey(_key);
            }

            public
                override void WriteDescriptionTo(MessageWriter writer)
            {
                writer.WriteMessageLine(
                    "meta table of object {0} should contain an entry for key \"{1}\"", actual, _key);
            }

            #endregion
        }
    }
}