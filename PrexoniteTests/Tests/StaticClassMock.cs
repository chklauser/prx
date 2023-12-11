using JetBrains.Annotations;

namespace PrexoniteTests.Tests;

[PublicAPI] // Referenced from unit test (fully qualified)
public static class StaticClassMock
{
    public static string SomeProperty { get; set; } = "";
}