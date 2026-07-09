

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests;

[Parallelizable(ParallelScope.Fixtures)]
[TestFixture]
public class CilRuntime
{
    [Test]
    public void RuntimeMethodsLinked()
    {
        var rt = typeof (Runtime);
        var cs = from m in rt.GetMembers(BindingFlags.Static | BindingFlags.Public)
            where m.Name.EndsWith("PrepareTargets") && m is PropertyInfo || m is FieldInfo
            let v = _invokeStatic(m) 
            select Tuple.Create(m,v);

        foreach (var t in cs)
            Assert.That(t.Item2, Is.Not.Null,
                $"The field/property Runtime.{t.Item1.Name} is null.");
    }

    object? _invokeStatic(MemberInfo m)
    {
        if(m is PropertyInfo)
        {
            var p = (PropertyInfo) m;
            return p.GetValue(null, []);
        }
        else if(m is FieldInfo)
        {
            var f = (FieldInfo) m;
            return f.GetValue(null);
        }
        else
        {
            var message = string.Format("The member {1}.{0} is not a property or field.", m.Name, m.DeclaringType);
            Assert.Fail(message);
// ReSharper disable HeuristicUnreachableCode
            throw new(message);
// ReSharper restore HeuristicUnreachableCode
        }
    }
}