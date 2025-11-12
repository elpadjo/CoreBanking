using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTeamJade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactInfo_Address_City",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo_Address_Country",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo_Address_State",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo_Address_Street",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo_Address_ZipCode",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                column: "DateOpened",
                value: new DateTime(2025, 10, 23, 11, 57, 8, 60, DateTimeKind.Utc).AddTicks(5132));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactInfo_Address_City",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContactInfo_Address_Country",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContactInfo_Address_State",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContactInfo_Address_Street",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContactInfo_Address_ZipCode",
                table: "Customers");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                column: "DateOpened",
                value: new DateTime(2025, 10, 22, 11, 41, 57, 127, DateTimeKind.Utc).AddTicks(4771));
        }
    }
}
