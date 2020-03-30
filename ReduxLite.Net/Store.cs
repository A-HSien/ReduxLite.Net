using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ReduxLite.Net
{

    /// <summary>
    /// The Store is the object that brings actions and reducers together.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class Store<TState, TKey>
    {
        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="logger"></param>
        public Store(ILogger<Store<TState, TKey>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Set finder and writer, for persistence
        /// </summary>
        /// <param name="finder"></param>
        /// <param name="writer"></param>
        public void Set(Func<IQueryable<Action<TKey>>> finder, System.Action<Action<TKey>> writer)
        {
            _finder = finder;
            _writer = writer;
        }

        private readonly ILogger _logger;
        private Func<IQueryable<Action<TKey>>> _finder;
        private System.Action<Action<TKey>> _writer;



        private ConcurrentDictionary<string, ConcurrentBag<Reducer<Action<TKey>, TKey, TState>>> _reducerMap
            = new ConcurrentDictionary<string, ConcurrentBag<Reducer<Action<TKey>, TKey, TState>>>();

        private static ConcurrentBag<Reducer<Action<TKey>, TKey, TState>> createReducerGroup() =>
             new ConcurrentBag<Reducer<Action<TKey>, TKey, TState>>();

        /// <summary>
        /// Reducer Register
        /// </summary>
        /// <typeparam name="TAction"></typeparam>
        /// <param name="reducer"></param>
        public void Register<TAction>(Reducer<TAction, TKey, TState> reducer)
            where TAction : Action<TKey>
        {
            var actionType = reducer?.Action.ActionType;
            Func<Action<TKey>, TState, TState> innerHandler = (args, currentState) =>
            {
                _logger?.LogDebug($"Reducer triggered. ActionType: {reducer.Action.ActionType}");
                return reducer.Handler((TAction)args, currentState);
            };

            var innerReducer = new Reducer<Action<TKey>, TKey, TState>(innerHandler, typeof(TAction));
            if (!_reducerMap.ContainsKey(actionType))
                _reducerMap.TryAdd(actionType, createReducerGroup());

            _reducerMap.TryGetValue(actionType, out var reducers);
            reducers.Add(innerReducer);
        }

        private ConcurrentBag<Action<TState, TState>> _monitors = new ConcurrentBag<Action<TState, TState>>();

        /// <summary>
        /// Attach Monitor
        /// </summary>
        /// <param name="monitor"></param>
        public void AttachMonitor(Action<TState, TState> monitor)
        {
            _monitors.Add(monitor);
        }

        /// <summary>
        /// Dispatch action
        /// </summary>
        /// <typeparam name="TAction"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public TAction Dispatch<TAction>(TAction action)
            where TAction : Action<TKey>
        {
            if (_monitors.Count > 0)
            {
                var actions = _finder().Where(e => e.TargetId.Equals(action.TargetId)).ToArray();
                var latestState = GetState(actions);
                var modifiedState = GetState(actions);
                modifiedState = applyReducers(modifiedState, action);
                foreach (var monitor in _monitors) monitor(latestState, modifiedState);
            }

            _writer(action);
            return action;
        }

        /// <summary>
        /// Get Actions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Action<TKey>> GetActions(TKey id)
        {
            var actions = _finder()
                .Where(e => e.TargetId.Equals(id))
                .OrderBy(e => e.CreateTime)
                .ToArray();
            return actions;
        }

        /// <summary>
        /// Get State
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TState GetState(TKey id)
        {
            var actions = GetActions(id);
            var target = GetState(actions);
            return target;
        }

        /// <summary>
        /// Get State
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TState GetState(Action<TKey> action)
        {
            var target = Activator.CreateInstance<TState>();
            return GetState(target, action); ;
        }

        /// <summary>
        /// Get State
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        public TState GetState(IEnumerable<Action<TKey>> actions)
        {
            var target = Activator.CreateInstance<TState>();
            return GetState(target, actions);
        }

        /// <summary>
        /// Get State
        /// </summary>
        /// <param name="target"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TState GetState(TState target, Action<TKey> action)
        {
            target = applyReducers(target, action);
            return target;
        }

        /// <summary>
        /// Get State
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="actions">The actions.</param>
        /// <returns></returns>
        public TState GetState(TState target, params Action<TKey>[] actions)
        {
            if (actions == null || actions.Length == 0)
                return target;

            return GetState(target, actions.AsEnumerable());
        }


        /// <summary>
        /// Get State
        /// </summary>
        /// <param name="target"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public TState GetState(TState target, IEnumerable<Action<TKey>> actions)
        {
            actions.OrderBy(a => a.CreateTime).Aggregate(target, (states, action) => states = applyReducers(states, action));
            return target;
        }

        private TState applyReducers(TState target, Action<TKey> action)
        {
            _reducerMap.TryGetValue(action.ActionType, out var reducers);
            if (reducers == null) return target;
            reducers.Aggregate(target, (states, reducer) => states = reducer.Handler(action, states));
            return target;
        }
    }
}
