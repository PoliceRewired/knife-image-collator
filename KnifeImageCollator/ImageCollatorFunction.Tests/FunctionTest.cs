using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;

namespace ImageCollatorFunction.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task TestFunctionWithoutParamsReturnsResult()
        {
            var function = new Function();
            var context = new TestLambdaContext();
            var inputs = new ImageCollatorInputs()
            {
                accounts = "",
                collation = "download",
                period = "today"
            };
            var result = await function.FunctionHandler(inputs, context);

            Assert.NotNull(result);
        }
    }
}
