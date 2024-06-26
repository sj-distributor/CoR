
using CoRProcessor;

namespace UnitTests.Processors;

public class DivisionProcessor : IChainProcessor<NumberContext>
{
    public IChainProcessor<NumberContext> Next { get; set; }
    public async Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        if (t.Operation != Operation.Division) return await Next.Handle(t, token);

        t.Result = decimal.Round(t.Number1 / t.Number2, 2);
        
        return await Next.Handle(t, token);
    }
}