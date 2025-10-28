using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;

namespace MAF_WorkFlow_IfElse
{
    internal sealed class HandleSpamExecutor : Executor<DetectionResult>
    {
        public HandleSpamExecutor() : base("HandleSpamExecutor") { }

        public override async ValueTask HandleAsync(DetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            if (message.IsSpam)
            {
                await context.YieldOutputAsync($"标记为垃圾邮件的电子邮件: {message.Reason}");
            }
            else
            {
                throw new ArgumentException("This executor should only handle spam messages.");
            }
        }
    }
}
