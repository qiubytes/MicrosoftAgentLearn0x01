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
                Console.WriteLine($"输入值：{message}，目标值：{_targetnum}，不相等，要求重新输入");
                await context.SendMessageAsync(CmdEnum.INPUT); //触发事件，要求输入
                                                               // await context.YieldOutputAsync("不相等");//不输入，直接返回结果（不循环）
            }
            else
            {
                await context.YieldOutputAsync("相等");
            }
        }
    }
}
