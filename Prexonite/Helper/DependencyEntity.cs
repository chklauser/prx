using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite
{

    public static class DependencyEntity<T>
    {
        public static DependencyEntity<T, PValue> CreateDynamic(StackContext sctx, T name, PValue value, PValue getDependencies)
        {
            return new DependencyEntity<T, PValue>(name, value, _dynamicallyCallGetDependencies(sctx, getDependencies));
        }

        private static Func<PValue, IEnumerable<T>> _dynamicallyCallGetDependencies(StackContext sctx, PValue getDependenciesPV)
        {
            if (getDependenciesPV == null)
                return null;

            return value =>
                       {
                           var depsPV = getDependenciesPV.IndirectCall(sctx, new[] {value});

                           var depsDynamic = Commands.List.Map._ToEnumerable(sctx, depsPV);
                           if(depsDynamic == null)
                               throw new PrexoniteException("getDependencies function did not return enumerable.");

                           return depsDynamic as IEnumerable<T> 
                               ?? (from pv in depsDynamic
                                   select pv.ConvertTo<T>(sctx, true));
                       };
        }
    }

    public class DependencyEntity<TKey,TValue> : IDependent<TKey>
    {
        private readonly TKey _name;
        private readonly TValue _value;
        private readonly Func<TValue, IEnumerable<TKey>> _getDependencies;

        public DependencyEntity(TKey name, TValue value, Func<TValue, IEnumerable<TKey>> getDependencies)
        {
            if (getDependencies == null)
                throw new NullReferenceException("getDependencies");
            _name = name;
            _getDependencies = getDependencies;
            _value = value;
        }

        #region Implementation of INamed<TKey>

        public TKey Name
        {
            get { return _name; }
        }

        #endregion

        public TValue Value
        {
            get { return _value; }
        }

        #region Implementation of IDependent<TKey>

        public IEnumerable<TKey> GetDependencies()
        {
            return _getDependencies(_value);
        }

        #endregion
    }
}
