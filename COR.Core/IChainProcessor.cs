using System.Threading;
using System.Threading.Tasks;

namespace COR.Core
{
    public interface IChainProcessor<T>
    {
        IChainProcessor<T> Next { get; set; }
        Task<T> Handle(T t, CancellationToken token = default);
    }
}