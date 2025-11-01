using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_WorkFlowCreateResponse
{
    /// <summary>
    /// 输入值 和 目标值对比
    /// </summary>
    public class JudgeExecutor() : Executor<int>("Judge")
    {
        private int _targetnum;
        public JudgeExecutor(int targetnum) : this()
        {
            _targetnum = targetnum;
        }
        public override async ValueTask HandleAsync(int message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            if (_targetnum != message)
            {
                await context.YieldOutputAsync("不相等");
            }
            else
            {
                await context.YieldOutputAsync("相等");
            }
        }
    }
}
