
using CoRProcessor;

namespace UnitTests.Processors;

public class MultiplicationProcessor : IChainProcessor<NumberContext>
{
    public Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        if (t.Operation != Operation.Multiplication)  return Task.FromResult(t);

        t.Result = t.Number1 * t.Number2;
        
        return Task.FromResult(t);
    }

    public FuncDelegate<NumberContext> CompensateOnFailure { get; set; }
}