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
            //string[] worklogarray = new string[] {
            //    "完成了项目需求文档的编写，明确了各项功能需求和技术细节。",
            //    "与张晓沟通系统需求。",
            //    "修打印机，但是没修好。"
            //};
            List<string> worklogs = new List<string>();
            Console.WriteLine("请输入工作日志文件目录：");
            string logfile = Console.ReadLine();
            File.ReadLines(logfile).ToList().ForEach(line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    worklogs.Add(line.Trim());
                }
            });
            string[] worklogarray = worklogs.ToArray();
            Console.WriteLine("创建智能体...");
            //创建智能体
            AIAgent worklogclassiftAgent = AgentFactory.GetWorkLogClassifyAgent(chatClient);
            AIAgent worklogriskAgent = AgentFactory.GetWorkLogRiskAgent(chatClient);
            Console.WriteLine("创建流程执行器...");
            //创建工作流程执行器
            var workLogClassifyExecutor = new WorkLogClassifyExecutor(worklogclassiftAgent);
            var workLogRiskLevelExectutor = new WorkLogRiskLevelExectutor(worklogriskAgent);

            //Console.WriteLine("请输入工作日按回车输入空字符结束");
            Console.WriteLine("开始进行日志分析...");
            List<WorkLogModel> workLogModels = new List<WorkLogModel>();
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
                        // Console.WriteLine($"{outputEvent}");
                        if (outputEvent.Data is WorkLogModel wlm)
                        {
                            workLogModels.Add(wlm);
                        }
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
            int lowRisk = workLogModels.Where(w => w.Risk_Level == "低风险").Count();
            int midRisk = workLogModels.Where(w => w.Risk_Level == "中风险").Count();
            int rkl = workLogModels.Where(w => w.Risk_Level == "高风险").Count();
            Console.WriteLine($"日志输入完成，分类完成。一共{workLogModels.Count}条日志，低风险{lowRisk}条,中风险{midRisk}条,高风险{rkl}条");
            string cmd = "";
            while (cmd != "q")
            {
                Console.WriteLine("请输入 1 低风险 2 中风险 3 高风险，进行日志查询，按下q结束：");
                cmd = Console.ReadLine();
                if (cmd == "1")
                {
                    Console.WriteLine("-----------------低风险有以下日志-----------------");
                    foreach (var item in workLogModels.Where(w => w.Risk_Level == "低风险"))
                    {
                        Console.WriteLine($@"日志内容：{item.Content} 日志类型:{item.Type} 风险等级:{item.Risk_Level} 风险原因:{item.Risk_Reason}");
                    }
                }
                else if (cmd == "2")
                {
                    Console.WriteLine("-----------------中风险有以下日志-----------------");
                    foreach (var item in workLogModels.Where(w => w.Risk_Level == "中风险"))
                    {
                        Console.WriteLine($@"日志内容：{item.Content} 日志类型:{item.Type} 风险等级:{item.Risk_Level} 风险原因:{item.Risk_Reason}");
                    }
                }
                else if (cmd == "3")
                {
                    Console.WriteLine("-----------------高风险有以下日志-----------------");
                    foreach (var item in workLogModels.Where(w => w.Risk_Level == "高风险"))
                    {
                        Console.WriteLine($@"日志内容：{item.Content} 日志类型:{item.Type} 风险等级:{item.Risk_Level} 风险原因:{item.Risk_Reason}");
                    }
                }
            }


            Console.ReadLine();
        }
    }
}
