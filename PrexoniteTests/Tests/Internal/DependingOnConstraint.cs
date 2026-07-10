using System;
using System.Linq;
using NUnit.Framework.Constraints;
using Prexonite.Compiler.Build;
using Prexonite.Modular;

namespace PrexoniteTests.Tests.Internal;

public class DependingOnConstraint : Constraint
{
    readonly ModuleName _dependency;

    public DependingOnConstraint(ModuleName dependency)
        : base(dependency)
    {
        _dependency = dependency;
    }

    public DependingOnConstraint(string name)
        : base(name)
    {
        if (ModuleName.TryParse(name, out var moduleName))
            _dependency = moduleName;
        else
            throw new ArgumentException($"The string {name} is not a valid module name.");
    }

    public override string Description => $"depends on {_dependency}";

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        var actualValue = actual;
        return new(this, actualValue, _matches(actualValue));
    }

    bool _matches(object actualValue)
    {
        return actualValue is ITargetDescription desc
            && desc.Dependencies.Any(n => n.Equals(_dependency));
    }
}
