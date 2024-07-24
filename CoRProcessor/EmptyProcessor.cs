using System.Threading;
using System.Threading.Tasks;

namespace CoRProcessor
{
    public class EmptyProcessor<T> : IChainProcessor<T> where T : IChainContext
    {
        public Task<T> Handle(T t, CancellationToken token = default)
        {
            return Task.FromResult(t);
        }

        public FuncDelegate<T> CompensateOnFailure { get; set; } = (ctx, token) => Task.FromResult(ctx);
    }
}

