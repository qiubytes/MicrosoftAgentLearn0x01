using MAF_WorkLogAgent.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_WorkLogAgent.Agents
{
    public class AgentFactory
    {
        /// <summary>
        /// 工作日志分类智能体
        /// </summary>
        /// <param name="chatClient"></param>
        /// <returns></returns>
        public static ChatClientAgent GetWorkLogClassifyAgent(IChatClient chatClient)
        {
            string instructions = @" 
你是一名工作日志分类助手，负责将工作日志分类为以下类别之一：  
                                    创作型：完成/编写/设计/创作 → 需求文档、方案设计 
                                    沟通型：会议/讨论/沟通/对齐 → 团队例会、跨部门沟通 
                                    问题解决型：修复/解决/调试/优化 → Bug修复、性能优化 
                                    规划型：计划/规划/安排 → 排期、计划制定 
                                    行政型：汇报/整理/提交 → 周报、文档整理 

风险等级：根据日志内容评估其风险等级，分为低、中、高三级。 
根据用户提供的工作日志内容，判断其最符合上述哪一类别，并以JSON格式返回结果。
返回的JSON格式 包括 日志内容(worklog_content) 、工作类别(worklog_type) 、风险等级(risk_level)
                                ";
            return new ChatClientAgent(chatClient, new ChatClientAgentOptions(instructions: instructions)
            {
                ChatOptions = new()
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(WorkLogModel)))
                }
            });
        }
        /// <summary>
        /// 工作日志风险评估智能体
        /// </summary>
        /// <param name="chatClient"></param>
        /// <returns></returns>
        public static ChatClientAgent GetWorkLogRiskAgent(IChatClient chatClient)
        {
            string instructions = @" 
                                    你是一名工作日志风险评估助手：  
 
                                    对每项任务进行风险评估，只输出“低风险”、“中风险”或“高风险”。

                                    风险评估标准：
                                    - **高风险**：存在阻塞、严重延迟、关键问题或可能造成重大影响
                                    - **中风险**：有小问题、轻微延迟或潜在隐患  
                                    - **低风险**：正常推进、无显著问题
   
                                    返回的JSON格式 包括  风险等级(risk_level)
                                ";
            return new ChatClientAgent(chatClient, new ChatClientAgentOptions(instructions: instructions)
            {
                ChatOptions = new()
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(WorkLogModel)))
                }
            });
        }

        /// <summary>
        /// Creates an email assistant agent.
        /// </summary>
        /// <returns>A ChatClientAgent configured for email assistance</returns>
        //public static ChatClientAgent GetEmailAssistantAgent(IChatClient chatClient) =>
        //    new(chatClient, new ChatClientAgentOptions(instructions: "您是一名电子邮件助手，帮助用户撰写专业邮件回复。")
        //    {
        //        ChatOptions = new()
        //        {
        //            ResponseFormat = ChatResponseFormat.ForJsonSchema(AIJsonUtilities.CreateJsonSchema(typeof(EmailResponse)))
        //        }
        //    });
    }
}
