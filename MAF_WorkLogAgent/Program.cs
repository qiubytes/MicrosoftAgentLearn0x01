using MAF_WorkLogAgent.Agents;
using MAF_WorkLogAgent.Executors;
using MAF_WorkLogAgent.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace MAF_WorkLogAgent
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("工作日志分析工具，启动中...");
            var config = new ConfigurationBuilder()
         .AddUserSecrets<Program>()
         .Build();

            var apikey = config["apikey"];
            // Console.WriteLine($"Secret: {apikey}");

            IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apikey),
            new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://api.siliconflow.cn/v1/"),

            })
            .GetChatClient("Qwen/Qwen2.5-7B-Instruct").AsIChatClient();
            /* deepseek-ai/DeepSeek-R1-0528-Qwen3-8B
             * THUDM/GLM-4-9B-0414
             * Qwen/Qwen2.5-7B-Instruct
             */
            string[] worklogarray = new string[] {
                "完成了项目需求文档的编写，明确了各项功能需求和技术细节。",
                "与张晓沟通系统需求。",
                "修打印机，但是没修好。"
            };
            Console.WriteLine("创建智能体");
            //创建智能体
            AIAgent worklogclassiftAgent = AgentFactory.GetWorkLogClassifyAgent(chatClient);
            AIAgent worklogriskAgent = AgentFactory.GetWorkLogRiskAgent(chatClient);
            Console.WriteLine("创建流程执行器");
            //创建工作流程执行器
            var workLogClassifyExecutor = new WorkLogClassifyExecutor(worklogclassiftAgent);
            var workLogRiskLevelExectutor = new WorkLogRiskLevelExectutor(worklogriskAgent);



            //Console.WriteLine("请输入工作日按回车输入空字符结束");
            for (int i = 0; i < worklogarray.Length; i++)
            {
                string input = worklogarray[i];
                //创建工作流
                var workflow = new WorkflowBuilder(workLogClassifyExecutor)
                  .AddEdge(workLogClassifyExecutor, workLogRiskLevelExectutor)
                 .WithOutputFrom(workLogRiskLevelExectutor)
                  .Build();
                //执行工作流 传入工作日志内容
                StreamingRun run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, input));
                await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

                await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
                {
                    if (evt is WorkflowOutputEvent outputEvent)
                    {
                        Console.WriteLine($"{outputEvent}");
                    }
                }

                //var response = await worklogclassiftAgent.RunAsync(input);
                //string restext = response.Text.Replace("```json", "").Replace("```", "");
                //var workloginfo = JsonSerializer.Deserialize<WorkLogModel>(restext);
                //Console.WriteLine($@" 日志内容：{workloginfo.Content} 日志类型:{workloginfo.Type} 日志风险:{workloginfo.Risk_Level}");
            }
            //workLogModels.Add(workloginfo);
            //var workloginfo = response.Deserialize<WorkLogModel>(JsonSerializerOptions.Web);
            //input = Console.ReadLine();

            Console.WriteLine("日志输入完成，分类完成。");
            Console.ReadLine();
        }
    }
}
