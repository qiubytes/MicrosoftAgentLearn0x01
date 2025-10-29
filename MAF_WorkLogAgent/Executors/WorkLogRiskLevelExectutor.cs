using MAF_WorkLogAgent.Dtos;
using MAF_WorkLogAgent.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Executors
{
    internal class WorkLogRiskLevelExectutor : Executor<WorkLogTypeDto>
    {
        private AIAgent _riskAgent;
        public WorkLogRiskLevelExectutor(AIAgent riskAgent) : base("SendEmailExecutor")
        {
            this._riskAgent = riskAgent;
        }

        public override async ValueTask HandleAsync(WorkLogTypeDto message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            WorkLogModel model = await context.ReadStateAsync<WorkLogModel>(message.Id, scopeName: "myscope");
            var response = await this._riskAgent.RunAsync(model.Content);
            string restext = response.Text.Replace("```json", "").Replace("```", "");
            var detectionResult = JsonSerializer.Deserialize<WorkLogRiskDto>(restext);
            model.Type = message.Type;
            model.ClassifyReason = message.ClassifyReason;
            model.Risk_Level = detectionResult.Risk_Level;
            model.Risk_Reason = detectionResult.Risk_Reason;
            await context.YieldOutputAsync(model);
            //await context.YieldOutputAsync($"日志内容：{model.Content} 分类结果: {message.Type} 风险等级：{detectionResult.Risk_Level},风险评估原因:{detectionResult.Risk_Reason}");
        }

    }

}
