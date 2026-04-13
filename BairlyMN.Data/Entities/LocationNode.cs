using System.Collections.Generic;

namespace BairlyMN.Data.Entities;

public class LocationNode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }

    public int? ParentId { get; set; }
    public LocationNode? Parent { get; set; }

    public ICollection<LocationNode> Children { get; set; } = new List<LocationNode>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public int SortOrder { get; set; }
    public bool IsLeaf { get; set; }
    public bool IsActive { get; set; } = true;
}