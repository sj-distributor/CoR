using System.Threading;
using System.Threading.Tasks;

namespace CoRProcessor
{
    public class EmptyProcessor<T> : IChainProcessor<T>
    {
        public IChainProcessor<T> Next { get; set; } = default;

        public Task<T> Handle(T t, CancellationToken token = default)
        {
            return Task.FromResult(t);
        }
    }
}

