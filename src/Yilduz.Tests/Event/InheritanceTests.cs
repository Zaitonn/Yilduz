using Jint;
using Xunit;

namespace Yilduz.Tests.Event;

public sealed class InheritanceTests : TestBase
{
    public InheritanceTests()
    {
        Engine.AddEventsApi();
    }

    [Fact]
    public void ShouldInheritEventPrototype()
    {
        Engine.Execute(
            "class CustomEvent extends Event {}; " + "const customEvent = new CustomEvent('test'); "
        );
        Assert.True(Engine.Evaluate("customEvent instanceof Event").AsBoolean());
    }
}
