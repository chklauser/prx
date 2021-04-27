using System;
using NUnit.Framework;
using Prexonite.Modular;

namespace PrexoniteTests.Tests.Modular
{
    [TestFixture]
    public class ModuleNameTests
    {
        [Test]
        public void ModuleNameToString()
        {
            var moduleName = new ModuleName("This.is.the.name", new Version(1, 2, 3, 4));
            var result = moduleName.ToString();
            Assert.That(result, Is.EqualTo($"{moduleName.Id}/1.2.3.4"));
        }
        [Test]
        public void ModuleNameToStringZeroVersion()
        {
            var moduleName = new ModuleName("This.is.the.name", new Version());
            var result = moduleName.ToString();
            Assert.That(result, Is.EqualTo($"{moduleName.Id}"));
        }
        [Test]
        public void ModuleNameToStringShortVersion3()
        {
            var moduleName = new ModuleName("This.is.the.name", new Version(1, 2, 3));
            var result = moduleName.ToString();
            Assert.That(result, Is.EqualTo($"{moduleName.Id}/1.2.3"));
        }
        [Test]
        public void ModuleNameToStringShortVersion2()
        {
            var moduleName = new ModuleName("This.is.the.name", new Version(1, 2));
            var result = moduleName.ToString();
            Assert.That(result, Is.EqualTo($"{moduleName.Id}/1.2"));
        }

        [Test]
        public void ParseFromStringNoVersion()
        {
            const string rawName = "this.is.the.name";
            var result = ModuleName.TryParse(rawName, out var name);
            Assert.That(result, Is.True, "TryParse({}) is successful", rawName);
            Assert.That(name, Is.Not.Null);
            Assert.That(name, Is.EqualTo(new ModuleName("this.is.the.name", new Version())));
        }

        [Test]
        public void ParseFromStringVersion4()
        {
            const string rawName = "this.is.the.name/1.2.3.4";
            var result = ModuleName.TryParse(rawName, out var name);
            Assert.That(result, Is.True, "TryParse({}) is successful", rawName);
            Assert.That(name, Is.Not.Null);
            Assert.That(name, Is.EqualTo(new ModuleName("this.is.the.name", new Version(1,2,3,4))));
        }

        [Test]
        public void ParseFromStringVersion3()
        {
            const string rawName = "this.is.the.name/1.2.3";
            var result = ModuleName.TryParse(rawName, out var name);
            Assert.That(result, Is.True, "TryParse({}) is successful", rawName);
            Assert.That(name, Is.Not.Null);
            Assert.That(name, Is.EqualTo(new ModuleName("this.is.the.name", new Version(1,2,3))));
        }

        [Test]
        public void ParseFromStringVersion2()
        {
            const string rawName = "this.is.the.name/1.2";
            var result = ModuleName.TryParse(rawName, out var name);
            Assert.That(result, Is.True, "TryParse({}) is successful", rawName);
            Assert.That(name, Is.Not.Null);
            Assert.That(name, Is.EqualTo(new ModuleName("this.is.the.name", new Version(1,2))));
        }

        [Test]
        public void RoundTrip()
        {
            var name1 = new ModuleName("this.is.the.name", new Version(1, 2, 3, 4));
            var result = ModuleName.TryParse(name1.ToString(), out var name2);
            Assert.That(result, Is.True, "TryParse({}) is successful", name1);
            Assert.That(name2, Is.Not.Null);
            Assert.That(name2, Is.EqualTo(name1));
        }
        
    }
}