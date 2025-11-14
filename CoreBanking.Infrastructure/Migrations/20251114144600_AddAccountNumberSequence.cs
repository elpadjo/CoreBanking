using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the sequence
            migrationBuilder.Sql(@"
            CREATE SEQUENCE [dbo].[AccountNumberSeq] 
                AS BIGINT
                START WITH 1000000020
                INCREMENT BY 1
                MINVALUE 1000000020
                MAXVALUE 9999999999
                NO CYCLE;"); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the sequence in rollback
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[AccountNumberSeq]");
        }
    }
}
