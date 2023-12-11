using NUnit.Framework;
using Prexonite;

namespace PrexoniteTests.Tests;

[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self)]
[TestFixture]
public class MetaEntryTests
{
    [Test]
    public void DotSeparatedStoreWithoutQuotes()
    {
        Assert.That(new MetaEntry("a.b.c").ToString(), Is.EqualTo("a.b.c"));
    }

    [Test]
    public void DotSeparatedStoreInteriorNonId()
    {
        Assert.That(new MetaEntry("a.%^.c").ToString(), Is.EqualTo("a.$\"%^\".c"));
    }

    [Test]
    public void SingleIdStore()
    {
        Assert.That(new MetaEntry("c").ToString(), Is.EqualTo("c"));
    }

    [Test]
    public void NonIdStore()
    {
        Assert.That(new MetaEntry("%^").ToString(), Is.EqualTo("\"%^\""));
    }

    [Test]
    public void VersionStore()
    {
        Assert.That(new MetaEntry("0.0").ToString(), Is.EqualTo("0.0"));
        Assert.That(new MetaEntry("0.0.1").ToString(), Is.EqualTo("0.0.1"));
        Assert.That(new MetaEntry("0.0.1.2").ToString(), Is.EqualTo("0.0.1.2"));
    }
}