using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMailboxModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MailboxMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 20000, nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeletedBySender = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailboxMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailboxMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MailboxAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Extension = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FileDeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsFileDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailboxAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailboxAttachments_MailboxMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "MailboxMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MailboxRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecipientUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ReadAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeletedByRecipient = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailboxRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MailboxRecipients_MailboxMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "MailboxMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailboxRecipients_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailboxAttachments_Deleted_ExpiresAtUtc",
                table: "MailboxAttachments",
                columns: new[] { "IsFileDeleted", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MailboxAttachments_MessageId",
                table: "MailboxAttachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MailboxAttachments_StoredFileName_Unique",
                table: "MailboxAttachments",
                column: "StoredFileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailboxMessages_SenderUserId_SentAtUtc",
                table: "MailboxMessages",
                columns: new[] { "SenderUserId", "SentAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MailboxMessages_Subject",
                table: "MailboxMessages",
                column: "Subject");

            migrationBuilder.CreateIndex(
                name: "IX_MailboxRecipients_MessageId_RecipientUserId_Unique",
                table: "MailboxRecipients",
                columns: new[] { "MessageId", "RecipientUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailboxRecipients_User_Read_Deleted",
                table: "MailboxRecipients",
                columns: new[] { "RecipientUserId", "IsRead", "IsDeletedByRecipient" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MailboxAttachments");

            migrationBuilder.DropTable(
                name: "MailboxRecipients");

            migrationBuilder.DropTable(
                name: "MailboxMessages");
        }
    }
}
