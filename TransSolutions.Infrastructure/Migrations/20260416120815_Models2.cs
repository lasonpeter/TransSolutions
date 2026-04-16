using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransSolutions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Models2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoadTrip_Drivers_DriverId",
                table: "RoadTrip");

            migrationBuilder.DropForeignKey(
                name: "FK_RoadTrip_Vehicles_VehicleId",
                table: "RoadTrip");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadTrip",
                table: "RoadTrip");

            migrationBuilder.RenameTable(
                name: "RoadTrip",
                newName: "RoadTrips");

            migrationBuilder.RenameIndex(
                name: "IX_RoadTrip_VehicleId",
                table: "RoadTrips",
                newName: "IX_RoadTrips_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoadTrip_DriverId",
                table: "RoadTrips",
                newName: "IX_RoadTrips_DriverId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadTrips",
                table: "RoadTrips",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadTrips_Drivers_DriverId",
                table: "RoadTrips",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadTrips_Vehicles_VehicleId",
                table: "RoadTrips",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoadTrips_Drivers_DriverId",
                table: "RoadTrips");

            migrationBuilder.DropForeignKey(
                name: "FK_RoadTrips_Vehicles_VehicleId",
                table: "RoadTrips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadTrips",
                table: "RoadTrips");

            migrationBuilder.RenameTable(
                name: "RoadTrips",
                newName: "RoadTrip");

            migrationBuilder.RenameIndex(
                name: "IX_RoadTrips_VehicleId",
                table: "RoadTrip",
                newName: "IX_RoadTrip_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoadTrips_DriverId",
                table: "RoadTrip",
                newName: "IX_RoadTrip_DriverId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadTrip",
                table: "RoadTrip",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadTrip_Drivers_DriverId",
                table: "RoadTrip",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoadTrip_Vehicles_VehicleId",
                table: "RoadTrip",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }
    }
}
