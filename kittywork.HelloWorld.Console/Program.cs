using kittywork.HelloWorld.Business;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<IHelloService, HelloService>();
using var provider = services.BuildServiceProvider();

var service = provider.GetRequiredService<IHelloService>();
Console.WriteLine(service.GetMessage());
