using System;
using System.Collections.Generic;
using System.Text;

namespace ReduxLite.Net
{
    /// <summary>
    /// Actions are payloads of information that send data from your application to your store.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class Action<TKey>
    {
        /// <summary>
        /// Action Type
        /// </summary>
        public virtual string ActionType => this.GetType().FullName;

        /// <summary>
        /// Create Time
        /// </summary>
        public virtual DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Target Id
        /// </summary>
        public virtual TKey TargetId { get; set; }
    }
}
