using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoRProcessor
{
    public delegate Task ActionDelegate<T>(T ctx, CancellationToken cancelToken) where T : IChainContext;

    public delegate Task<T> FuncDelegate<T>(T ctx, CancellationToken cancelToken)
        where T : IChainContext;

    public delegate Task<bool> OnExceptionDelegate<T>(T ctx, Exception e, CancellationToken cancelToken)
        where T : IChainContext;

    public class CoRProcessor<T> where T : IChainContext
    {
        private readonly List<IChainProcessor<T>> _chainProcessors = new List<IChainProcessor<T>>();
        private readonly List<FuncDelegate<T>> _delegates = new List<FuncDelegate<T>>();

        private ActionDelegate<T> _finallyAction = null;
        private ActionDelegate<T> _beforeAction = null;
        private ActionDelegate<T> _afterAction = null;
        private OnExceptionDelegate<T> _onException = null;

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

        public async Task<T> Execute(T ctx,  CancellationToken cancelToken = default, params int[] steps)
        {
            try
            {
                if (_beforeAction != null) await _beforeAction.Invoke(ctx, cancelToken);
                
                for (var i = 0; i < _chainProcessors.Count; i++)
                {
                    if (steps.Length > 0 && !steps.Contains(i)) continue;
                    if (ctx.Abort) break;
                    if (_chainProcessors[i].CompensateOnFailure != null) _delegates.Add(_chainProcessors[i].CompensateOnFailure);
                    ctx = await _chainProcessors[i].Handle(ctx, cancelToken);
                }

                if (_afterAction == null) return ctx;

                await _afterAction.Invoke(ctx, cancelToken);

                return ctx;
            }
            catch (Exception e)
            {
                foreach (var funcDelegate in _delegates) await funcDelegate.Invoke(ctx, cancelToken);

                if (_onException != null)
                {
                    var isThrow = await _onException.Invoke(ctx, e, cancelToken);
                    if (!isThrow) return ctx;
                }

                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
            finally
            {
                if (_finallyAction != null)
                    await _finallyAction.Invoke(ctx, cancelToken);
            }
        }

        public CoRProcessor<T> GlobalPreExecute(ActionDelegate<T> action)
        {
            _beforeAction = action;
            return this;
        }

        public CoRProcessor<T> GlobalExecuted(ActionDelegate<T> action)
        {
            _afterAction = action;
            return this;
        }

        public CoRProcessor<T> Finally(ActionDelegate<T> action)
        {
            _finallyAction = action;
            return this;
        }

        public CoRProcessor<T> OnException(OnExceptionDelegate<T> action)
        {
            _onException = action;
            return this;
        }
    }
}