using CoRProcessor;

namespace UnitTests.Processors;

public class AbortProcessor : IChainProcessor<NumberContext>
{
    public Task<NumberContext> Handle(NumberContext t, CancellationToken cancellationToken = default)
    {
        t.Abort = true;
        return Task.FromResult(t);
    }

    public FuncDelegate<NumberContext> CompensateOnFailure { get; set; }
}