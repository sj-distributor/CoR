using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using COR.Core.Extensions;

namespace COR.Core
{
    public delegate Task FuncDelegate<T>(T arg, CancellationToken cancellationToken);

    public class CoRProcessor<T>
    {
        private IChainProcessor<T> _firstHandler;

        private readonly List<IChainProcessor<T>> _chainProcessors = new List<IChainProcessor<T>>();

        private FuncDelegate<T> _finallyAction = null;
        private FuncDelegate<T> _beforeAction = null;
        private FuncDelegate<T> _afterAction = null;
        private FuncDelegate<T> _onException = null;

        private CoRProcessor()
        {
        }

        public static CoRProcessor<T> New()
        {
            return new CoRProcessor<T>();
        }

        public CoRProcessor<T> AddRange(IEnumerable<IChainProcessor<T>> processors)
        {
            _chainProcessors.AddRange(processors);
            return this;
        }

        public async Task<T> Execute(T t, CancellationToken token = default)
        {
            _chainProcessors.Add(new EmptyProcessor<T>());
            _firstHandler = BuildChain(_chainProcessors.DistinctBy(x => x.GetType()).ToList());
            
            try
            {
                if (_beforeAction != null)
                    await _beforeAction.Invoke(t, token);

                var handle = await _firstHandler.Handle(t, token);

                if (_afterAction == null) return handle;

                await _afterAction.Invoke(handle, token);

                return handle;
            }
            catch (Exception e)
            {
                if (_onException != null)
                {
                    await _onException.Invoke(t, token);
                }

                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
            finally
            {
                if (_finallyAction != null)
                    await _finallyAction.Invoke(t, token);
            }
        }

        public CoRProcessor<T> Before(FuncDelegate<T> action)
        {
            _beforeAction = action;
            return this;
        }

        public CoRProcessor<T> After(FuncDelegate<T> action)
        {
            _afterAction = action;
            return this;
        }

        public CoRProcessor<T> Finally(FuncDelegate<T> action)
        {
            _finallyAction = action;
            return this;
        }

        public CoRProcessor<T> OnException(FuncDelegate<T> action)
        {
            _onException = action;
            return this;
        }

        private IChainProcessor<T> BuildChain(IReadOnlyList<IChainProcessor<T>> processors)
        {
            if (processors.Count == 0)
                throw new Exception("No processors provided. At least one processor is required.");

            for (var i = 0; i < processors.Count - 1; i++)
            {
                processors[i].Next = processors[i + 1];
            }

            return processors[0];
        }
    }
}