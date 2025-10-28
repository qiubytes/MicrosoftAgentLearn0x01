using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace MAF_WorkFlow0x01
{
    internal sealed class ReverseTextExecutor() : ReflectingExecutor<ReverseTextExecutor>("ReverseTextExecutor"),
    IMessageHandler<string, string>
    {
        public ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            // Reverse the input text
            return ValueTask.FromResult(new string(input.Reverse().ToArray()));
        }
    }
}
