using System;
using System.Collections.Generic;
using System.Text;

namespace ReduxLite.Net
{
    /// <summary>
    /// Reducers specify how the application's state changes in response to actions sent to the store.
    /// </summary>
    /// <typeparam name="TAction"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TState"></typeparam>
    public class Reducer<TAction, TKey, TState>
        where TAction : Action<TKey>
    {
        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="handler"></param>
        public Reducer(Func<TAction, TState, TState> handler)
        {
            Action = Activator.CreateInstance<TAction>();
            Handler = handler;
        }

        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="actionType"></param>
        public Reducer(Func<Action<TKey>, TState, TState> handler, Type actionType)
        {
            Action = (TAction)Activator.CreateInstance(actionType);
            Handler = handler;
        }

        /// <summary>
        /// Corresponding Action
        /// </summary>
        public TAction Action { get; private set; }

        /// <summary>
        /// Reducing function
        /// </summary>
        public Func<TAction, TState, TState> Handler { get; private set; }
    }
}
