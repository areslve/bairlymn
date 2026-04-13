using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BairlyMN.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendPropertyDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasFence",
                table: "LandDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGas",
                table: "LandDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasInternet",
                table: "LandDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LandOwnershipType",
                table: "LandDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LandSlope",
                table: "LandDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "HouseDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingType",
                table: "HouseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "HouseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloorMaterial",
                table: "HouseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAircon",
                table: "HouseDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBasement",
                table: "HouseDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSauna",
                table: "HouseDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HeatingType",
                table: "HouseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFurnished",
                table: "HouseDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoofType",
                table: "HouseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsPets",
                table: "ApartmentDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsSmoking",
                table: "ApartmentDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableRooms",
                table: "ApartmentDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalconyArea",
                table: "ApartmentDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "ApartmentDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingType",
                table: "ApartmentDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoorsCount",
                table: "ApartmentDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloorMaterial",
                table: "ApartmentDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAircon",
                table: "ApartmentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSecurity",
                table: "ApartmentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStorage",
                table: "ApartmentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HeatingType",
                table: "ApartmentDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFurnished",
                table: "ApartmentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSharedApartment",
                table: "ApartmentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "KitchenArea",
                table: "ApartmentDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LivingArea",
                table: "ApartmentDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LivingRooms",
                table: "ApartmentDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinRentMonths",
                table: "ApartmentDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WindowType",
                table: "ApartmentDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasFence",
                table: "LandDetails");

            migrationBuilder.DropColumn(
                name: "HasGas",
                table: "LandDetails");

            migrationBuilder.DropColumn(
                name: "HasInternet",
                table: "LandDetails");

            migrationBuilder.DropColumn(
                name: "LandOwnershipType",
                table: "LandDetails");

            migrationBuilder.DropColumn(
                name: "LandSlope",
                table: "LandDetails");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "BuildingType",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "FloorMaterial",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "HasAircon",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "HasBasement",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "HasSauna",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "HeatingType",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "IsFurnished",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "RoofType",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "AllowsPets",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "AllowsSmoking",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "AvailableRooms",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "BalconyArea",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "BuildingType",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "DoorsCount",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "FloorMaterial",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "HasAircon",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "HasSecurity",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "HasStorage",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "HeatingType",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "IsFurnished",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "IsSharedApartment",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "KitchenArea",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "LivingArea",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "LivingRooms",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "MinRentMonths",
                table: "ApartmentDetails");

            migrationBuilder.DropColumn(
                name: "WindowType",
                table: "ApartmentDetails");
        }
    }
}
