using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Dtos
{
    /// <summary>
    /// 模型返回 工作风险等级 DTO
    /// </summary>
    public sealed class WorkLogRiskDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }
        /// <summary>
        /// 工作风险等级
        /// </summary>
        [JsonPropertyName("risk_level")]
        public string Risk_Level { get; set; }
    }
}
