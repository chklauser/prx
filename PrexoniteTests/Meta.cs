using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return new ContainsKeyConstraint(key).And.Matches(new ExactEqualityConstraint(key,value));
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

                if(_actualEntry.EntryType != _expectedEntry.EntryType)
                {
                    _typeMismatch = true;
                    return false;
                }

                _typeMismatch = false;
                return _actualEntry.Equals(_expectedEntry);
            }

            public override void WriteDescriptionTo(MessageWriter writer)
            {
                if(actual == null)
                {
                    writer.WriteMessageLine("Actual value does not have a meta table.");
                }
                else if(_typeMismatch)
                {
                    writer.WriteMessageLine("Meta entry type doesn't match.");
                    writer.WriteExpectedValue(_expectedEntry.EntryType);
                    writer.WriteActualValue(_actualEntry.EntryType);
                }
                else
                {
                    writer.WriteMessageLine("actual meta entry {0} should match expected entry {1}", _actualEntry, _expectedEntry);
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
                if((ihmt = actual as IHasMetaTable) != null)
                    mt = ihmt.Meta;
                else
                    return false;

                return mt.ContainsKey(_key);
            }

            override 

            public void WriteDescriptionTo(MessageWriter writer)
            {
                writer.WriteMessageLine("meta table of object {0} should contain an entry for key \"{1}\"", actual, _key);
            }

            #endregion
        }
    }
}
