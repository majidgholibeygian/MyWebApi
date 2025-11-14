using Microsoft.Extensions.Logging;
using mywebapi.Controllers;

namespace MyWebApi.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1() 
    {
        var controller = new WeatherForecastController();
        var result = controller.Get();
        Assert.NotNull(result);
    }
}
