using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;


namespace MAF_WorkFlow_IfElse
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

            var apikey = config["apikey"];
            Console.WriteLine($"Secret: {apikey}");

            IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apikey),
            new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://api.siliconflow.cn/v1/"),

            })
            .GetChatClient("deepseek-ai/DeepSeek-R1-0528-Qwen3-8B").AsIChatClient();

            //.CreateAIAgent();//instructions: "你擅长于讲笑话"
            // Create agents
            AIAgent spamDetectionAgent = GetSpamDetectionAgent(chatClient);
            AIAgent emailAssistantAgent = GetEmailAssistantAgent(chatClient);
            // Create executors
            var spamDetectionExecutor = new SpamDetectionExecutor(spamDetectionAgent);
            var emailAssistantExecutor = new EmailAssistantExecutor(emailAssistantAgent);
            var sendEmailExecutor = new SendEmailExecutor();
            var handleSpamExecutor = new HandleSpamExecutor();

            // Build the workflow with conditional edges
            var workflow = new WorkflowBuilder(spamDetectionExecutor)
                // Non-spam path: route to email assistant when IsSpam = false
                .AddEdge(spamDetectionExecutor, emailAssistantExecutor, condition: GetCondition(expectedResult: false))
                .AddEdge(emailAssistantExecutor, sendEmailExecutor)
                // Spam path: route to spam handler when IsSpam = true
                .AddEdge(spamDetectionExecutor, handleSpamExecutor, condition: GetCondition(expectedResult: true))
               .WithOutputFrom(handleSpamExecutor, sendEmailExecutor)
                .Build();

            // Execute the workflow with sample spam email
            string emailContent = "恭喜！您已赢得100万美元奖金！点击此处立即领取！";
            //string emailContent = "老师您好，这是今天的工作日志，请查收";
            StreamingRun run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, emailContent));
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is WorkflowOutputEvent outputEvent)
                {
                    Console.WriteLine($"{outputEvent}");
                }
            }
            Console.ReadLine();
        }
        private static Func<object?, bool> GetCondition(bool expectedResult) =>
    detectionResult => detectionResult is DetectionResult result && result.IsSpam == expectedResult;
        /// <summary>
        /// Creates a spam detection agent.
        /// </summary>
        /// <returns>A ChatClientAgent configured for spam detection</returns>
        private static ChatClientAgent GetSpamDetectionAgent(IChatClient chatClient) =>
            new(chatClient, new ChatClientAgentOptions(instructions: "你是一名垃圾邮件检测助手，负责识别垃圾邮件。请用中文回复")
            {
                ChatOptions = new()
                {
                    //            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                    //schema: AIJsonUtilities.CreateJsonSchema(typeof(DetectionResult)),
                    //schemaName: "DetectionResult",
                    //schemaDescription: "DetectionResult about a Email including their IsSpam, Reason")
                    ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(DetectionResult)))
                }
            });

        /// <summary>
        /// Creates an email assistant agent.
        /// </summary>
        /// <returns>A ChatClientAgent configured for email assistance</returns>
        private static ChatClientAgent GetEmailAssistantAgent(IChatClient chatClient) =>
            new(chatClient, new ChatClientAgentOptions(instructions: "您是一名电子邮件助手，帮助用户撰写专业邮件回复。")
            {
                ChatOptions = new()
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(EmailResponse)))
                }
            });
    }
}
