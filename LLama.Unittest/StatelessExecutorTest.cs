using LLama.Common;
using Xunit.Abstractions;

namespace LLama.Unittest
{
    public class StatelessExecutorTest
        : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly LLamaWeights _weights;
        private readonly ModelParams _params;

        public StatelessExecutorTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _params = new ModelParams(Constants.ModelPath)
            {
                ContextSize = 60,
                Seed = 1754,
            };
            _weights = LLamaWeights.LoadFromFile(_params);
        }

        public void Dispose()
        {
            _weights.Dispose();
        }

        [Fact]
        public async Task Stateless()
        {
            var executor = new StatelessExecutor(_weights, _params);

            const string question = "Question. what is a cat?\nAnswer: ";
            var @params = new InferenceParams { MaxTokens = 32, AntiPrompts = new[] { "." } };

            var result1 = string.Join("", await executor.InferAsync(question, @params).ToListAsync());
            var result2 = string.Join("", await executor.InferAsync(question, @params).ToListAsync());

            _testOutputHelper.WriteLine(result1);

            // Check that it produced the exact same result both times
            Assert.Equal(result1, result2);
        }

        [Fact(Skip = "Very very slow in CI")]
        public async Task OutOfContext()
        {
            var executor = new StatelessExecutor(_weights, _params);

            const string question = " Question. cats or dogs?\nAnswer: ";

            // The context size is set to 60. Generate more than that, forcing it to generate a coherent response
            // with a modified context
            var @params = new InferenceParams()
            {
                MaxTokens = 65,
                TokensKeep = question.Length,
            };

            var result1 = string.Join("", await executor.InferAsync(question, @params).ToListAsync());
            var result2 = string.Join("", await executor.InferAsync(question, @params).ToListAsync());

            _testOutputHelper.WriteLine(result1);

            // Check that it produced the exact same result both times
            Assert.Equal(result1, result2);
        }
    }
}