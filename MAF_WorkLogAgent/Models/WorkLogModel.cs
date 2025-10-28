using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Models
{
    public class WorkLogModel
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        [JsonPropertyName("worklog_content")]
        public string Content { get; set; }
        /// <summary>
        /// 工作类别
        /// </summary>
        [JsonPropertyName("worklog_type")]
        public string Type { get; set; }
        /// <summary>
        /// 风险等级
        /// </summary>
        [JsonPropertyName("risk_level")]
        public string Risk_Level { get; set; }
    }
}
