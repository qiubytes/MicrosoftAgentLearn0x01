using MAF_WorkLogAgent.Dtos;
using MAF_WorkLogAgent.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Executors
{
    internal class WorkLogRiskLevelExectutor : Executor<WorkLogTypeDto>
    {
        public WorkLogRiskLevelExectutor(AIAgent riskAgent) : base("SendEmailExecutor") { }

        public override async ValueTask HandleAsync(WorkLogTypeDto message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            WorkLogModel model = await context.ReadStateAsync<WorkLogModel>(message.Id, scopeName: "myscope");
            await context.YieldOutputAsync($"日志内容：{model.Content} 分类结果: {message.Type}");
        }

    }

}
