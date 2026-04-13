using Microsoft.AspNetCore.Mvc.Rendering;

namespace BairlyMN.Mvc.ViewModels;

public class FilterFormModel
{
    public ListingFilterViewModel Filter { get; set; } = new();
    public IEnumerable<SelectListItem> Categories { get; set; } =
        Enumerable.Empty<SelectListItem>();
    /// <summary>
    /// "desk" эсвэл "mob" — DOM id давхардахаас сэргийлнэ
    /// </summary>
    public string IdPrefix { get; set; } = "desk";
}