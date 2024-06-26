using COR.Core;

namespace UnitTests.Processors;

public class SubtractionProcessor : IChainProcessor<NumberContext>
{
    public IChainProcessor<NumberContext> Next { get; set; }
    public async Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        if (t.Operation != Operation.Subtraction) return await Next.Handle(t, token);

        t.Result = t.Number1 - t.Number2;
        
        return await Next.Handle(t, token);
    }
}