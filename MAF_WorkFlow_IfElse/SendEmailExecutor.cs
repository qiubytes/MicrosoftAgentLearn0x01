using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;

namespace MAF_WorkFlow_IfElse
{
    internal sealed class SendEmailExecutor : Executor<EmailResponse>
    {
        public SendEmailExecutor() : base("SendEmailExecutor") { }

        public override async ValueTask HandleAsync(EmailResponse message, IWorkflowContext context, CancellationToken cancellationToken = default) =>
            await context.YieldOutputAsync($"邮件发送: {message.Response}");
    }
}
