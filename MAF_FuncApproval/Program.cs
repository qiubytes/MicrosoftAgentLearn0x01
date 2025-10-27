using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Threading;

namespace MAF_FuncApproval
{
    internal class Program
    {
        [Description("根据地点名称 获取天气")]
        public static string GetWeather(string location)
        {
            // Dummy implementation for weather retrieval
            return $"{location} 的天气晴，25摄氏度";
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            AIFunction weatherFunction = AIFunctionFactory.Create(GetWeather);
#pragma warning disable MEAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            AIFunction approvalRequiredWeatherFunction = new ApprovalRequiredAIFunction(weatherFunction);
#pragma warning restore MEAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            //string apikey = Environment.GetEnvironmentVariable("apikey");
            // 最简单的配置方式
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var apikey = config["apikey"];
            Console.WriteLine($"Secret: {apikey}");
            AIAgent agent = new OpenAIClient(new ApiKeyCredential(apikey),
                 new OpenAIClientOptions()
                 {
                     Endpoint = new Uri("https://open.bigmodel.cn/api/paas/v4/"),

                 })
                 .GetChatClient("glm-4.5-flash")
                 // .GetChatClient("glm-4v-flash")
                 .CreateAIAgent(instructions: "天气助手", tools: [approvalRequiredWeatherFunction]);

            AgentThread agentThread = agent.GetNewThread();
            // Console.WriteLine(await agent.RunAsync("讲一个关于吃水果的笑话"));
            //await foreach (var update in agent.RunStreamingAsync("讲一个关于吃水果的笑话"))
            //{
            //    Console.Write(update);
            //}

            //ChatMessage message = new(ChatRole.User, [
            //    new TextContent("这个图片讲了什么"),
            //    new UriContent("https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png", "image/png")
            //]);

            // Console.WriteLine(await agent.RunAsync(message, agentThread));
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "q") break;
                ChatMessage message = new ChatMessage(ChatRole.User, [new TextContent(input)]);
                AgentRunResponse response = await agent.RunAsync(message, agentThread);
#pragma warning disable MEAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
                var functionApprovalRequests = response.Messages
                                                .SelectMany(x => x.Contents)
                                                .OfType<FunctionApprovalRequestContent>()
                                                .ToList();

                if (functionApprovalRequests.Any())
                {
                    FunctionApprovalRequestContent requestContent = functionApprovalRequests.First();
                    Console.WriteLine($"我们需要您同意AI执行 '{requestContent.FunctionCall.Name}' 方法");
                    string approvalresult = Console.ReadLine();//同意
                    bool bApproved = approvalresult.Equals("同意", StringComparison.OrdinalIgnoreCase);
                    //传递审批结果，继续执行
                    var approvalMessage = new ChatMessage(ChatRole.User, [requestContent.CreateResponse(bApproved)]);
                    Console.WriteLine(await agent.RunAsync(approvalMessage, agentThread));

                }
                else
                {
                    Console.WriteLine(response);
                }

#pragma warning restore MEAI001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            }
            Console.ReadLine();
        }
    }
}
