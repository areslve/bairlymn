using BairlyMN.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BairlyMN.Data.Seeding;

public static class LocationSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, string jsonPath)
    {
        if (await db.LocationNodes.AnyAsync()) return;

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"[LocationSeeder] JSON файл олдсонгүй: {jsonPath}");
            return;
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var nodes = JsonSerializer.Deserialize<List<LocationJsonNode>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (nodes == null) return;

        foreach (var node in nodes)
            await InsertNodeAsync(db, node, parentId: null);

        await db.SaveChangesAsync();
        Console.WriteLine("[LocationSeeder] Байршлын мэдээлэл амжилттай нэмэгдлээ.");
    }

    private static async Task InsertNodeAsync(
        ApplicationDbContext db,
        LocationJsonNode node,
        int? parentId)
    {
        var entity = new LocationNode
        {
            Name = node.Name,
            Level = node.Level,
            ParentId = parentId,
            IsActive = true
        };

        db.LocationNodes.Add(entity);
        await db.SaveChangesAsync(); // Id авахын тулд

        if (node.Children != null)
        {
            foreach (var child in node.Children)
                await InsertNodeAsync(db, child, entity.Id);
        }
    }

    // JSON дүрслэлийн загвар
    private class LocationJsonNode
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<LocationJsonNode>? Children { get; set; }
    }
}