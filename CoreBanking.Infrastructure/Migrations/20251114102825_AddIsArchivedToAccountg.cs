using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToAccountg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BVN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentBalance_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentBalance_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "NGN"),
                    AvailableBalance_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableBalance_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "NGN"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOpened = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateClosed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    AccountStatus = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transaction_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Transaction_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    RunningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_RelatedAccountId",
                        column: x => x.RelatedAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Transfer_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Transfer_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitiatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "BVN", "CreditScore", "DateCreated", "DateOfBirth", "DateUpdated", "DeletedAt", "DeletedBy", "FirstName", "IsActive", "IsDeleted", "LastName", "Address_City", "Address_Country", "Address_State", "Address_Street", "Address_ZipCode", "Email", "PhoneNumber" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-1234-5678-9abc-123456789abc"), "20000000009", 40, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Alice", true, false, "Johnson", "Lagos", "Nigeria", "Lagos", "123 Main St", "100001", "alice.johnson@email.com", "555-0101" },
                    { new Guid("b2c3d4e5-2345-6789-abcd-234567890bcd"), "20000000010", 55, new DateTime(2024, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1988, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Bob", true, false, "Smith", "Lagos", "Nigeria", "Lagos", "456 Victoria Island", "101001", "bob.smith@email.com", "555-0202" }
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountNumber", "AccountStatus", "AccountType", "CustomerId", "DateClosed", "DateCreated", "DateOpened", "DateUpdated", "DeletedAt", "DeletedBy", "IsDeleted", "AvailableBalance_Amount", "AvailableBalance_Currency", "CurrentBalance_Amount", "CurrentBalance_Currency" },
                values: new object[,]
                {
                    { new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"), "1000000001", 0, "Checking", new Guid("a1b2c3d4-1234-5678-9abc-123456789abc"), null, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 9, 11, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, 1500.00m, "NGN", 1500.00m, "NGN" },
                    { new Guid("d4e5f6a7-4567-8901-def2-456789012def"), "1000000002", 0, "Savings", new Guid("b2c3d4e5-2345-6789-abcd-234567890bcd"), null, new DateTime(2024, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, null, false, 5000.00m, "NGN", 5000.00m, "NGN" }
                });

            migrationBuilder.InsertData(
                table: "Transfers",
                columns: new[] { "Id", "CompletedAt", "DateCreated", "DateUpdated", "Description", "FromAccountId", "InitiatedAt", "Reference", "ScheduledAt", "Status", "ToAccountId", "Transfer_Amount", "Transfer_Currency" },
                values: new object[,]
                {
                    { new Guid("a7b8c9d0-7890-1234-0003-789012345003"), new DateTime(2024, 10, 5, 14, 20, 3, 0, DateTimeKind.Utc), new DateTime(2024, 10, 5, 14, 20, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 5, 14, 20, 3, 0, DateTimeKind.Utc), "Purchase payment - Failed: Insufficient funds", new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"), new DateTime(2024, 10, 5, 14, 20, 0, 0, DateTimeKind.Utc), "TRF-20241005-003", null, "Failed", new Guid("d4e5f6a7-4567-8901-def2-456789012def"), 2500.00m, "NGN" },
                    { new Guid("e5f6a7b8-5678-9012-0001-567890123001"), new DateTime(2024, 10, 1, 10, 30, 5, 0, DateTimeKind.Utc), new DateTime(2024, 10, 1, 10, 30, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 1, 10, 30, 5, 0, DateTimeKind.Utc), "Payment for services", new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"), new DateTime(2024, 10, 1, 10, 30, 0, 0, DateTimeKind.Utc), "TRF-20241001-001", null, "Completed", new Guid("d4e5f6a7-4567-8901-def2-456789012def"), 500.00m, "NGN" },
                    { new Guid("f6a7b8c9-6789-0123-0002-678901234002"), null, new DateTime(2024, 11, 10, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 11, 10, 8, 0, 0, 0, DateTimeKind.Utc), "Scheduled monthly payment", new Guid("d4e5f6a7-4567-8901-def2-456789012def"), new DateTime(2024, 11, 10, 8, 0, 0, 0, DateTimeKind.Utc), "TRF-20241110-002", new DateTime(2024, 11, 15, 9, 0, 0, 0, DateTimeKind.Utc), "Pending", new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"), 1000.00m, "NGN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CustomerId",
                table: "Accounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RelatedAccountId",
                table: "Transactions",
                column: "RelatedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FromAccountId",
                table: "Transfers",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Reference",
                table: "Transfers",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ScheduledAt",
                table: "Transfers",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Status",
                table: "Transfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ToAccountId",
                table: "Transfers",
                column: "ToAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
