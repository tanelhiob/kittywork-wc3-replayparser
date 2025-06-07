using kittywork.HelloWorld.Business;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public void CanResolve_IHelloService_FromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<IHelloService, HelloService>();
        var provider = services.BuildServiceProvider();

        var resolved = provider.GetRequiredService<IHelloService>();

        Assert.IsType<HelloService>(resolved);
        Assert.Equal("Hello World", resolved.GetMessage());
    }
}
