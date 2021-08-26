using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Net.Test.Entites
{
    public class ResultResponse
    {
        /// <summary>
        /// 响应码 200为成功 401为未授权 500内部报错等 对应httpcode
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultResponse<T> : ResultResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
