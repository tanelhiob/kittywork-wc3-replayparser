using kittywork.HelloWorld.Business;

namespace kittywork.HelloWorld.Business.Tests;

public class HelloServiceTests
{
    [Fact]
    public void GetMessage_ReturnsHelloWorld()
    {
        var service = new HelloService();
        var message = service.GetMessage();
        Assert.Equal("Hello World", message);
    }
}
