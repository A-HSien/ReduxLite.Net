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
        /// 動作類型
        /// </summary>
        public virtual string ActionType => this.GetType().FullName;

        /// <summary>
        /// 建立時間
        /// </summary>
        public virtual DateTime CreateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 操作目標的識別碼
        /// </summary>
        public virtual TKey TargetId { get; set; }
    }
}
