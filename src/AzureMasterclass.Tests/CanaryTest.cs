using Shouldly;
using Xunit;

namespace AzureMasterclass.Test;

public class CanaryTest
{
    [Fact]
    public void PassingTest()
    {
        const int result = 1 + 1;
        result.ShouldBe(2);
    }
}