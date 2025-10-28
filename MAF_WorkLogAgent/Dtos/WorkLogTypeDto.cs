using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Dtos
{
    /// <summary>
    /// 模型返回 工作类别 DTO
    /// </summary>
    public sealed class WorkLogTypeDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }
        /// <summary>
        /// 工作类别
        /// </summary>
        [JsonPropertyName("worklog_type")]
        public string Type { get; set; }
    }
}
