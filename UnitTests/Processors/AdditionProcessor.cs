using CoRProcessor;

namespace UnitTests.Processors;

public class AdditionProcessor : IChainProcessor<NumberContext>
{
    public Task<NumberContext> Handle(NumberContext t, CancellationToken token = default)
    {
        if (t.Operation != Operation.Addition) return Task.FromResult(t);

        t.Result += t.Number1 + t.Number2;

        return Task.FromResult(t);
    }

    public FuncDelegate<NumberContext> CompensateOnFailure { get; set; } = (context, token) =>
    {
        context.Number1 = 100.00m;
        context.Number2 = 100.00m;
        context.Result += 1;
        return Task.FromResult(context);
    }; 
}