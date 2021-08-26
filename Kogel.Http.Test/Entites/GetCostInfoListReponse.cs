using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Http.Test.Entites
{
    public class GetCostInfoListReponse
    {
        /// <summary>
        ///主键
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        ///费用代码
        /// </summary>
        [JsonProperty("cost_code")]
        public string CostCode { get; set; }

        /// <summary>
        ///费用名称
        /// </summary>
        [JsonProperty("cost_name")]
        public string CostName { get; set; }

        /// <summary>
        ///计算公式
        /// </summary>
        [JsonProperty("calculation_formula")]
        public string CalculationFormula { get; set; }

        /// <summary>
        ///说明
        /// </summary>
        [JsonProperty("ex_plain")]
        public string Ex_plain { get; set; }

        /// <summary>
        ///费用类别
        /// </summary>
        [JsonProperty("cost_type")]
        public int CostType { get; set; }

        /// <summary>
        ///创建人
        /// </summary>
        [JsonProperty("create_user")]
        public string CreateUser { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [JsonProperty("create_time")]
        public DateTime CreateTime { get; set; }


    }
}
