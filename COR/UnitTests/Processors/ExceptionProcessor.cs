using COR.Core;

namespace UnitTests.Processors;

public class ExceptionProcessor : IChainProcessor<NumberContext>
{
    public IChainProcessor<NumberContext> Next { get; set; }
    public async Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        var n = 1;
        var m = 0;

        var result = n / m;

        return await Next.Handle(t, token);
    }
}