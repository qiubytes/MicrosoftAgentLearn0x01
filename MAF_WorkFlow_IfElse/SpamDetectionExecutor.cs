using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;


namespace MAF_WorkFlow_IfElse
{
    internal sealed class SpamDetectionExecutor : Executor<ChatMessage, DetectionResult>
    {
        private readonly AIAgent _spamDetectionAgent;

        public SpamDetectionExecutor(AIAgent spamDetectionAgent) : base("SpamDetectionExecutor")
        {
            this._spamDetectionAgent = spamDetectionAgent;
        }

        public override async ValueTask<DetectionResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            // Generate a random email ID and store the email content to shared state
            var newEmail = new Email
            {
                EmailId = Guid.NewGuid().ToString("N"),
                EmailContent = message.Text
            };
            await context.QueueStateUpdateAsync(newEmail.EmailId, newEmail, scopeName: EmailStateConstants.EmailStateScope);

            // Invoke the agent for spam detection
            var response = await this._spamDetectionAgent.RunAsync(message);
            var detectionResult = JsonSerializer.Deserialize<DetectionResult>(response.Text);

            detectionResult!.EmailId = newEmail.EmailId;
            return detectionResult;
        }
    }
}
