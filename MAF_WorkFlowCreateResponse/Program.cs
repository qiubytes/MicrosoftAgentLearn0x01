using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

namespace MAF_WorkFlowCreateResponse
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
            // Console.WriteLine($"Secret: {apikey}");

            IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apikey),
            new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://api.siliconflow.cn/v1/"),

            })
            .GetChatClient("Qwen/Qwen2.5-7B-Instruct").AsIChatClient();
            //输入CmdEnum  输出int  （触发事件）
            RequestPort numberRequestPort = RequestPort.Create<CmdEnum, int>("numberInput");

            JudgeExecutor judgeExecutor = new(42);

            Workflow workflow = new WorkflowBuilder(numberRequestPort)
               .AddEdge(numberRequestPort, judgeExecutor)
               .AddEdge(judgeExecutor, numberRequestPort) //循环输入
               .WithOutputFrom(judgeExecutor)
               .Build();
            await using StreamingRun handle = await InProcessExecution.StreamAsync(workflow, CmdEnum.INPUT);//传入枚举类型，事件里面输入
            await foreach (WorkflowEvent evt in handle.WatchStreamAsync())
            {
                switch (evt)
                {
                    case RequestInfoEvent requestInputEvt:
                        if (requestInputEvt.Request.DataIs<CmdEnum>())
                        {
                            CmdEnum cmdEnum = requestInputEvt.Request.DataAs<CmdEnum>();
                            if (cmdEnum == CmdEnum.INPUT)
                            {
                                string inputstr = Console.ReadLine();
                                ExternalResponse response = requestInputEvt.Request.CreateResponse(Convert.ToInt32(inputstr));
                                await handle.SendResponseAsync(response); //RequestPort的返回值 传递到下一个节点
                            }
                        }
                        //ExternalResponse response = HandleExternalRequest(requestInputEvt.Request);
                        //await handle.SendResponseAsync(response);
                        break;

                    case WorkflowOutputEvent outputEvt:
                        // The workflow has yielded output
                        Console.WriteLine($"流程执行完毕，返回结果: {outputEvt.Data}");
                        return;
                }
            }
            Console.WriteLine("运行完成");
        }
    }
}
