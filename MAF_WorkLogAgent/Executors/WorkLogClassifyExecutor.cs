using MAF_WorkLogAgent.Dtos;
using MAF_WorkLogAgent.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Executors
{
    /// <summary>
    /// 日志分类执行器
    /// </summary>
    public class WorkLogClassifyExecutor : Executor<ChatMessage, WorkLogTypeDto>
    {
        private readonly AIAgent _classifyAgent;

        public WorkLogClassifyExecutor(AIAgent classifyAgent) : base("WorkLogClassifyExecutor")
        {
            this._classifyAgent = classifyAgent;
        }

        public override async ValueTask<WorkLogTypeDto> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {

            var workLog = new WorkLogModel()
            {
                Id = Guid.NewGuid().ToString("N"),
                Content = message.Text
            };
            //存入流程状态队列 （传递参数）
            await context.QueueStateUpdateAsync<WorkLogModel>(workLog.Id, workLog, scopeName: "myscope");

            // Invoke the agent for spam detection
            var response = await this._classifyAgent.RunAsync(message);
            string restext = response.Text.Replace("```json", "").Replace("```", "");
            var detectionResult = JsonSerializer.Deserialize<WorkLogTypeDto>(restext);
            //传递分类结果到下一个节点
            //detectionResult!.EmailId = newEmail.EmailId;
            detectionResult.Id = workLog.Id;
            return detectionResult;
        }
    }
}
