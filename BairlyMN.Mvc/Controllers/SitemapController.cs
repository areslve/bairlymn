using BairlyMN.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BairlyMN.Mvc.Controllers;

public class SitemapController : Controller
{
    private readonly ApplicationDbContext _db;

    public SitemapController(ApplicationDbContext db) => _db = db;

    [Route("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Static pages
        foreach (var path in new[] { "/", "/Listings", "/Account/Login", "/Account/Register" })
        {
            sb.AppendLine($"""
                <url>
                  <loc>{baseUrl}{path}</loc>
                  <changefreq>daily</changefreq>
                  <priority>{(path == "/" ? "1.0" : "0.8")}</priority>
                </url>
            """);
        }

        // Active listings
        var listings = await _db.Listings
            .Where(l => l.Status == BairlyMN.Domain.Enums.ListingStatus.Active)
            .Select(l => new { l.Id, l.UpdatedAt })
            .OrderByDescending(l => l.UpdatedAt)
            .Take(1000)
            .ToListAsync();

        foreach (var l in listings)
        {
            sb.AppendLine($"""
                <url>
                  <loc>{baseUrl}/Listings/Details/{l.Id}</loc>
                  <lastmod>{l.UpdatedAt:yyyy-MM-dd}</lastmod>
                  <changefreq>weekly</changefreq>
                  <priority>0.6</priority>
                </url>
            """);
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}