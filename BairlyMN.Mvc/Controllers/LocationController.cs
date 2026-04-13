using BairlyMN.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BairlyMN.Mvc.Controllers;

/// <summary>
/// 3-түвшний байршлын AJAX API
/// GET /api/location/level1          → Хот/Аймаг жагсаалт
/// GET /api/location/children/{id}   → Дүүрэг/Сум эсвэл Хороо/Баг
/// </summary>
[Route("api/location")]
[ApiController]
public class LocationController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public LocationController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Хот / Аймаг (Level 1)
    /// GET /api/location/level1
    /// </summary>
    [HttpGet("level1")]
    public async Task<IActionResult> GetLevel1()
    {
        var nodes = await _db.LocationNodes
            .Where(l => l.Level == 1 && l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new LocationNodeDto(l.Id, l.Name, l.Level))
            .ToListAsync();

        return Ok(nodes);
    }

    /// <summary>
    /// Дүүрэг/Сум эсвэл Хороо/Баг — parent-аас хүүхдийг авна
    /// GET /api/location/children/1
    /// </summary>
    [HttpGet("children/{parentId:int}")]
    public async Task<IActionResult> GetChildren(int parentId)
    {
        var nodes = await _db.LocationNodes
            .Where(l => l.ParentId == parentId && l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new LocationNodeDto(l.Id, l.Name, l.Level))
            .ToListAsync();

        return Ok(nodes);
    }

    /// <summary>
    /// Edit mode-д ашиглана — сонгосон node-оос эцэг гинжийг буцаана
    /// GET /api/location/ancestors/45
    /// Returns: [level1, level2, level3] дарааллаар
    /// </summary>
    [HttpGet("ancestors/{nodeId:int}")]
    public async Task<IActionResult> GetAncestors(int nodeId)
    {
        var chain = new List<LocationNodeDto>();
        var current = await _db.LocationNodes.FindAsync(nodeId);

        while (current != null)
        {
            chain.Insert(0, new LocationNodeDto(current.Id, current.Name, current.Level));
            current = current.ParentId.HasValue
                ? await _db.LocationNodes.FindAsync(current.ParentId.Value)
                : null;
        }

        return Ok(chain);
    }

    // Response DTO — анонимоос илүү тодорхой
    private record LocationNodeDto(int Id, string Name, int Level);
}