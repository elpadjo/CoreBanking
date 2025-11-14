using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewAccountColsForPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableBalanceCurrency",
                table: "Accounts");

            migrationBuilder.AddColumn<bool>(
                name: "EnableLowBalanceAlerts",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTransactionAlerts",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LowBalanceThreshold",
                table: "Accounts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonthlyStatementPreference",
                table: "Accounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValue: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableLowBalanceAlerts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "EnableTransactionAlerts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LowBalanceThreshold",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MonthlyStatementPreference",
                table: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "AvailableBalanceCurrency",
                table: "Accounts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "NGN");

        }
    }
}
