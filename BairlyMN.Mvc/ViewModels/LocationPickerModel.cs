namespace BairlyMN.Mvc.ViewModels;

/// <summary>
/// _LocationPicker partial-д дамжуулах model
/// </summary>
public class LocationPickerModel
{
    /// <summary>HTML id prefix — нэг хуудсанд олон picker байж болно</summary>
    public string Prefix { get; set; } = "loc";

    /// <summary>Edit mode-д pre-select хийх LocationNode.Id</summary>
    public int? SelectedId { get; set; }

    /// <summary>Label харуулах эсэх</summary>
    public bool ShowLabel { get; set; } = true;
}