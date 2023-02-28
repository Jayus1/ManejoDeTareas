using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareaMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT EXISTS(SELECT Id FROM AspNetRoles WHERE Id= 'f23d4f66-fd41-4104-8290-27a547ba9252')
                                  BEGIN
                                  INSERT AspNetRoles (Id, [Name], [NormalizedName]) 
                                  VALUES ('f23d4f66-fd41-4104-8290-27a547ba9252','admin','ADMIN')
                                  END
                                  ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE AspNetRoles WHERE Id= 'f23d4f66-fd41-4104-8290-27a547ba9252'");
        }
    }
}
