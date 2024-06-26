using System.Threading;
using System.Threading.Tasks;

namespace CoRProcessor
{
    public interface IChainProcessor<T>
    {
        IChainProcessor<T> Next { get; set; }
        Task<T> Handle(T t, CancellationToken token = default);
    }
}