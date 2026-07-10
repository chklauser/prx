using System;
using NUnit.Framework.Constraints;
using Prexonite;

namespace PrexoniteTests;

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
        return new ContainsKeyConstraint(key).And.Matches(new ExactEqualityConstraint(key, value));
    }

    class ExactEqualityConstraint : Constraint
    {
        readonly string _key;
        readonly MetaEntry _expectedEntry;

        public ExactEqualityConstraint(string key, MetaEntry expectedEntry)
            : base(key, expectedEntry)
        {
            if (expectedEntry == null)
                throw new ArgumentNullException(nameof(expectedEntry));

            _key = key ?? throw new ArgumentNullException(nameof(key));
            _expectedEntry = expectedEntry;
        }

        #region Overrides of Constraint

        public override string Description =>
            $"meta entry '{_key}' exactly equals {_expectedEntry}";

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            object actualObj = actual;
            return new(this, actualObj, _matches(actualObj));
        }

        bool _matches(object actual)
        {
            if (actual is not IHasMetaTable ihmt)
            {
                return false;
            }

            var actualEntry = ihmt.Meta[_key];

            return actualEntry.EntryType == _expectedEntry.EntryType
                && actualEntry.Equals(_expectedEntry);
        }

        #endregion
    }

    class ContainsKeyConstraint : Constraint
    {
        readonly string _key;

        public ContainsKeyConstraint(string key)
            : base(key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        #region Overrides of Constraint

        public override string Description => $"contains meta key '{_key}'";

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            var actualValue = actual;
            return new(this, actualValue, _matches(actualValue));
        }

        bool _matches(object actual)
        {
            IHasMetaTable? ihmt;
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
