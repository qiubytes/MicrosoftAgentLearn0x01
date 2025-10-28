using Microsoft.Agents.AI.Workflows;

namespace MAF_WorkFlow0x01
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            // 创建执行器
            UppercaseExecutor uppercase = new();
            ReverseTextExecutor reverse = new();

            // 创建工作流
            WorkflowBuilder builder = new(uppercase);
            builder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
            var workflow = builder.Build();

            // 执行工作流
            await using Run run = await InProcessExecution.RunAsync(workflow, "Hello, World!");
            foreach (WorkflowEvent evt in run.NewEvents)
            {
                switch (evt)
                {
                    case ExecutorCompletedEvent executorComplete:
                        Console.WriteLine($"执行器执行完毕{executorComplete.ExecutorId}: {executorComplete.Data}");
                        break;
                    case WorkflowOutputEvent workflowOutput:
                        Console.WriteLine($"流程输出'{workflowOutput.SourceId}' outputs: {workflowOutput.Data}");
                        break;
                }
            }
        }
    }
}
