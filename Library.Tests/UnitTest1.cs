using Xunit;

namespace Library.Tests
{
    public class BasicTests
    {
        [Fact]
        public void SimpleMathTest()
        {
            int a = 2;
            int b = 3;

            Assert.Equal(5, a + b);
        }
    }
}