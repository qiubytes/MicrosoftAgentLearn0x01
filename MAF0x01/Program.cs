using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using ModelContextProtocol.Client;
namespace MAF0x01
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
            //string apikey = Environment.GetEnvironmentVariable("apikey");
            var config = new ConfigurationBuilder()
           .AddUserSecrets<Program>()
           .Build();

            //MCP服务 https://www.modelscope.cn/mcp
            await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
            {
                Name = "playwrightmcp",
                Command = "npx",
                Arguments = [
                        "@playwright/mcp@latest"
                    ],
            }));
            var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
            var apikey = config["apikey"];
            Console.WriteLine($"Secret: {apikey}");
            AIAgent agent = new OpenAIClient(new ApiKeyCredential(apikey),
                new OpenAIClientOptions()
                {
                    Endpoint = new Uri("https://open.bigmodel.cn/api/paas/v4/"),

                })
                  .GetChatClient("glm-4.5-flash")
                // .GetChatClient("glm-4v-flash")
                .CreateAIAgent(instructions: "聊天助手", tools: [AIFunctionFactory.Create(GetWeather), .. mcpTools.Cast<AITool>()]);

            AgentThread agentThread = agent.GetNewThread();
            // Console.WriteLine(await agent.RunAsync("讲一个关于吃水果的笑话"));
            //await foreach (var update in agent.RunStreamingAsync("讲一个关于吃水果的笑话"))
            //{
            //    Console.Write(update);
            //}

            //识别图片内容
            //ChatMessage message = new(ChatRole.User, [
            //    new TextContent("这个图片讲了什么"),
            //    new UriContent("https://www.baidu.com/img/PCtm_d9c8750bed0b3c7d089fa7d55720d6cf.png", "image/png")
            //]);

            //Console.WriteLine(await agent.RunAsync(message, agentThread));
            while (true)
            {
                ChatMessage message1 = new ChatMessage() { Role = ChatRole.User };
                string input = Console.ReadLine();
                message1.Contents.Add(new TextContent(input));
                if (input == "q") break;

                Console.WriteLine(await agent.RunAsync(message1, agentThread));

            }
            Console.ReadLine();
        }
    }
}
