
using CoRProcessor;

namespace UnitTests.Processors;

public class ExceptionProcessor : IChainProcessor<NumberContext>
{
    public Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        var n = 1;
        var m = 0;

        var result = n / m;

        return Task.FromResult(t);
    }

    public FuncDelegate<NumberContext> CompensateOnFailure { get; set; }
}