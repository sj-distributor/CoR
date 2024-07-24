
using CoRProcessor;

namespace UnitTests.Processors;

public class DivisionProcessor : IChainProcessor<NumberContext>
{
    public Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        if (t.Operation != Operation.Division)  return Task.FromResult(t);

        t.Result = decimal.Round(t.Number1 / t.Number2, 2);
        
        return Task.FromResult(t);
    }

    public FuncDelegate<NumberContext> CompensateOnFailure { get; set; }
}