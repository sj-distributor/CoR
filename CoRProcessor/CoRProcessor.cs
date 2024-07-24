using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoRProcessor
{
    public delegate Task ActionDelegate<T>(T arg, CancellationToken cancellationToken) where T : IChainContext;

    public delegate Task<T> FuncDelegate<T>(T arg, CancellationToken cancellationToken)
        where T : IChainContext;

    public delegate Task<bool> OnExceptionDelegate<T>(T arg, Exception e, CancellationToken cancellationToken)
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

        public async Task<T> Execute(T t, CancellationToken token = default)
        {
            var context = t;
            try
            {
                if (_beforeAction != null) await _beforeAction.Invoke(t, token);

                foreach (var chainProcessor in _chainProcessors)
                {
                    if (context.Abort) break;
                    if (chainProcessor.CompensateOnFailure != null) _delegates.Add(chainProcessor.CompensateOnFailure);
                    context = await chainProcessor.Handle(context, token);
                }

                if (_afterAction == null) return context;

                await _afterAction.Invoke(context, token);

                return context;
            }
            catch (Exception e)
            {
                foreach (var funcDelegate in _delegates) await funcDelegate.Invoke(context, token);

                if (_onException != null)
                {
                    var isThrow = await _onException.Invoke(context, e, token);
                    if (!isThrow) return context;
                }

                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
            finally
            {
                if (_finallyAction != null)
                    await _finallyAction.Invoke(context, token);
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