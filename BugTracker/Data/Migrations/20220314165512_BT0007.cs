using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    public partial class BT0007 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invite_AspNetUsers_InviteeId",
                table: "Invite");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_AspNetUsers_InvitorId",
                table: "Invite");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_Companies_CompanyId",
                table: "Invite");

            migrationBuilder.DropForeignKey(
                name: "FK_Invite_Projects_ProjectId",
                table: "Invite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invite",
                table: "Invite");

            migrationBuilder.RenameTable(
                name: "Invite",
                newName: "Invites");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_ProjectId",
                table: "Invites",
                newName: "IX_Invites_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_InvitorId",
                table: "Invites",
                newName: "IX_Invites_InvitorId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_InviteeId",
                table: "Invites",
                newName: "IX_Invites_InviteeId");

            migrationBuilder.RenameIndex(
                name: "IX_Invite_CompanyId",
                table: "Invites",
                newName: "IX_Invites_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invites",
                table: "Invites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_AspNetUsers_InviteeId",
                table: "Invites",
                column: "InviteeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_AspNetUsers_InvitorId",
                table: "Invites",
                column: "InvitorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Companies_CompanyId",
                table: "Invites",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Projects_ProjectId",
                table: "Invites",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_AspNetUsers_InviteeId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_AspNetUsers_InvitorId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Companies_CompanyId",
                table: "Invites");

            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Projects_ProjectId",
                table: "Invites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invites",
                table: "Invites");

            migrationBuilder.RenameTable(
                name: "Invites",
                newName: "Invite");

            migrationBuilder.RenameIndex(
                name: "IX_Invites_ProjectId",
                table: "Invite",
                newName: "IX_Invite_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Invites_InvitorId",
                table: "Invite",
                newName: "IX_Invite_InvitorId");

            migrationBuilder.RenameIndex(
                name: "IX_Invites_InviteeId",
                table: "Invite",
                newName: "IX_Invite_InviteeId");

            migrationBuilder.RenameIndex(
                name: "IX_Invites_CompanyId",
                table: "Invite",
                newName: "IX_Invite_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invite",
                table: "Invite",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_AspNetUsers_InviteeId",
                table: "Invite",
                column: "InviteeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_AspNetUsers_InvitorId",
                table: "Invite",
                column: "InvitorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_Companies_CompanyId",
                table: "Invite",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invite_Projects_ProjectId",
                table: "Invite",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
