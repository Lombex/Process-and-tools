using CSharpAPI.Models;
using CSharpAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public class HistoryService
    {
        private readonly SQLiteDatabase _Db;

        public HistoryService(SQLiteDatabase context)
        {
            _Db = context;
        }

        public async Task LogAsync(EntityType entityType, string entityId, string action, string changes)
{
    Console.WriteLine($"Logging history: {entityType}, {entityId}, {action}, {changes}");
    var history = new History
    {
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        Changes = changes,
        Timestamp = DateTime.UtcNow
    };

    _Db.History.Add(history);
    await _Db.SaveChangesAsync();
}

    }
}