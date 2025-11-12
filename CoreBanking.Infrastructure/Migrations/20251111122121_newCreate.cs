using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Transactions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Transactions",
                newName: "DateUpdated");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Customers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Accounts",
                newName: "CurrentCurrency");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Accounts",
                newName: "CurrentAmount");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Accounts",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedAccountId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RunningBalance",
                table: "Transactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "Transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BVN",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableAmount",
                table: "Accounts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AvailableCurrency",
                table: "Accounts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "NGN");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateClosed",
                table: "Accounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Accounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "Accounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                columns: new[] { "AccountNumber", "AccountStatus", "DateClosed", "DateCreated", "DateOpened", "DateUpdated", "AvailableAmount", "AvailableCurrency", "CurrentAmount" },
                values: new object[] { "1234567890", 0, null, new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 11, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), 10000.00m, "NGN", 10500.00m });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-1234-5678-9abc-123456789abc"),
                columns: new[] { "BVN", "CreditScore", "DateCreated", "DateOfBirth", "DateUpdated" },
                values: new object[] { "20000000009", 40, new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountId", "DateCreated", "DateUpdated", "Description", "Reference", "RelatedAccountId", "RunningBalance", "TransactionReference", "Type", "Amount", "Currency" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"), new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Initial Deposit", "DEP-001", null, 50000.00m, "20241022120000-11111111", "Deposit", 1500.00m, "NGN" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"), new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 1, 10, 0, 0, 0, DateTimeKind.Utc), "ATM Withdrawal", "WTH-001", null, 45000.00m, "20241023100000-22222222", "Withdrawal", 1500.00m, "NGN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RelatedAccountId",
                table: "Transactions",
                column: "RelatedAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_RelatedAccountId",
                table: "Transactions",
                column: "RelatedAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_RelatedAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RelatedAccountId",
                table: "Transactions");

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RelatedAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RunningBalance",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BVN",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreditScore",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AvailableAmount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AvailableCurrency",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DateClosed",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Transactions",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "DateUpdated",
                table: "Transactions",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Customers",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "CurrentCurrency",
                table: "Accounts",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "CurrentAmount",
                table: "Accounts",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Accounts",
                newName: "AccountId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                columns: new[] { "AccountNumber", "DateOpened", "IsActive", "Amount" },
                values: new object[] { "1000000001", new DateTime(2025, 10, 15, 14, 5, 26, 913, DateTimeKind.Utc).AddTicks(8439), true, 1500.00m });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: new Guid("a1b2c3d4-1234-5678-9abc-123456789abc"),
                columns: new[] { "DateCreated", "Email", "PhoneNumber" },
                values: new object[] { new DateTime(2025, 10, 5, 14, 5, 26, 912, DateTimeKind.Utc).AddTicks(1781), "alice.johnson@email.com", "555-0101" });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
