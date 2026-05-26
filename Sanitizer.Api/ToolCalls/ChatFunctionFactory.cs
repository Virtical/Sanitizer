using System.ComponentModel;
using System.Data;
using Microsoft.Extensions.AI;

namespace Sanitizer.Api.ToolCalls;

public static class ChatFunctionFactory
{
    public static List<AITool> CreateTools()
    {
        return
        [
            AIFunctionFactory.Create(
                () => DateTime.UtcNow,
                "get_current_utc_time",
                "Получает текущее время в UTC. Используй когда пользователь спрашивает о текущем времени."
            ),
        ];
    }
}
