using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace MAF_WorkFlow_IfElse
{
    internal sealed class EmailAssistantExecutor : Executor<DetectionResult, EmailResponse>
    {
        private readonly AIAgent _emailAssistantAgent;

        public EmailAssistantExecutor(AIAgent emailAssistantAgent) : base("EmailAssistantExecutor")
        {
            this._emailAssistantAgent = emailAssistantAgent;
        }

        public override async ValueTask<EmailResponse> HandleAsync(DetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            if (message.IsSpam)
            {
                throw new ArgumentException("This executor should only handle non-spam messages.");
            }

            // Retrieve the email content from shared state
            var email = await context.ReadStateAsync<Email>(message.EmailId, scopeName: EmailStateConstants.EmailStateScope)
                ?? throw new InvalidOperationException("Email not found.");

            // Invoke the agent to draft a response
            var response = await this._emailAssistantAgent.RunAsync(email.EmailContent);
            var emailResponse = JsonSerializer.Deserialize<EmailResponse>(response.Text);

            return emailResponse!;
        }
    }

}
