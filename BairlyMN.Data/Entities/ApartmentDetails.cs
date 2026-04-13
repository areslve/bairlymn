namespace BairlyMN.Data.Entities;

public class ApartmentDetails
{
    public int Id { get; set; }
    public int ListingId { get; set; }

    // Байршил
    public int? Floor { get; set; }           // Давхар
    public int? TotalFloors { get; set; }     // Нийт давхар

    // Өрөө
    public int? Rooms { get; set; }           // Өрөөний тоо
    public int? Bathrooms { get; set; }       // Угаалгын өрөө
    public int? LivingRooms { get; set; }     // Зочны өрөө

    // Нэмэлт хэмжээс
    public decimal? KitchenArea { get; set; }  // Гал тогооны талбай м²
    public decimal? LivingArea { get; set; }   // Амьдрах талбай м²
    public decimal? BalconyArea { get; set; }  // Тагтны талбай м²

    // Барилгын мэдээлэл
    public int? BuildYear { get; set; }        // Баригдсан он
    public string? Condition { get; set; }     // Нөхцөл байдал
    public string? BuildingType { get; set; }  // Барилгын төрөл (Панел, Тоосго, Монолит)

    // Дотоод
    public string? WindowType { get; set; }    // Цонхны төрөл
    public string? FloorMaterial { get; set; } // Шалны материал
    public string? HeatingType { get; set; }   // Халаалтын төрөл
    public int? DoorsCount { get; set; }       // Хаалганы тоо

    // Тохиромж
    public bool HasBalcony { get; set; }       // Тагт
    public bool HasParking { get; set; }       // Зогсоол
    public bool HasElevator { get; set; }      // Лифт
    public bool HasStorage { get; set; }       // Агуулах/Нөөц өрөө
    public bool HasSecurity { get; set; }      // Харуул хамгаалалт
    public bool IsFurnished { get; set; }      // Тавилгатай
    public bool HasAircon { get; set; }        // Агааржуулагчтай

    // Түрээсийн нэмэлт
    public bool? AllowsPets { get; set; }      // Тэжээвэр амьтан зөвшөөрнө
    public bool? AllowsSmoking { get; set; }   // Тамхи татахыг зөвшөөрнө
    public int? MinRentMonths { get; set; }    // Хамгийн бага түрээсийн хугацаа (сар)

    // Хажуу өрөө (Share орон сууц түрээслэх)
    public bool IsSharedApartment { get; set; }  // Хуваалцсан орон сууц
    public int? AvailableRooms { get; set; }     // Нээлттэй өрөөний тоо

    public Listing Listing { get; set; } = null!;
}