using System.Threading;
using System.Threading.Tasks;

namespace CoRProcessor
{
    public interface IChainProcessor<T> where T : IChainContext
    {
        Task<T> Handle(T ctx, CancellationToken cancelToken = default);
        FuncDelegate<T> CompensateOnFailure { get; set; }
    }
}