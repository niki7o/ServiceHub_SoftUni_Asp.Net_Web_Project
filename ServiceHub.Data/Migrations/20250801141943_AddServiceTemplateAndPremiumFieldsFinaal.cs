using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTemplateAndPremiumFieldsFinaal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Services",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "Services",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Services",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastServiceCreationDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f",
                column: "ConcurrencyStamp",
                value: "84b4d27b-a4bc-4dad-92da-029924f684fe");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e",
                column: "ConcurrencyStamp",
                value: "f541ce60-648d-4f24-aefb-b9d6fd85c2ab");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "99049752-95b1-477d-944a-f34589d31b09",
                column: "ConcurrencyStamp",
                value: "9122216c-0d61-4e8f-b29f-538fa5ad118c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o",
                columns: new[] { "ConcurrencyStamp", "LastServiceCreationDate", "SecurityStamp" },
                values: new object[] { "372293cc-9cb6-43c5-8687-f1b00a5c0ae9", null, "93e7bdb2-ef35-4c89-b409-859a65391d5d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p",
                columns: new[] { "ConcurrencyStamp", "LastServiceCreationDate", "SecurityStamp" },
                values: new object[] { "1ca176f1-355f-46ba-9e98-d622b346a6fd", null, "7a2c2d01-42ec-446e-907c-c4c0c6c9eed4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q",
                columns: new[] { "ConcurrencyStamp", "LastServiceCreationDate", "SecurityStamp" },
                values: new object[] { "9deb2551-aede-4306-9c4d-b91f0bd1aefc", null, "ced19f4c-5b40-4d9c-9ea5-d980c89956b4" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 14, 19, 42, 188, DateTimeKind.Utc).AddTicks(8327));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 14, 19, 42, 188, DateTimeKind.Utc).AddTicks(8330));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("1d4ae40b-c305-47b7-beed-163c4a0aeb40"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6770), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6767), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2ef43d87-d749-4d7d-9b7d-f7c4f527bea7"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6816), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6816), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("3a7b8b0c-1d2e-4f5a-a837-3d5e9f1a2b0c"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6820), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6820), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("8edc2d04-00f5-4630-b5a9-4fa499fc7210"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6824), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6824), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b422f89b-e7a3-4130-b899-7b56010007e0"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6812), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6811), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("c10de2fa-b49b-4c0d-9e8f-142b3cd40e6f"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6786), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6786), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("e11e539c-0290-4171-b606-16628d1790b0"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6781), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6781), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f0c72c7b-709d-44b7-81c1-1e5ab73305ec"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6791), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6790), true, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f5e402c0-91ba-4f8e-97d0-3b443fe10d3c"),
                columns: new[] { "ApprovedByUserId", "ApprovedOn", "CreatedByUserId", "CreatedOn", "IsApproved", "IsTemplate" },
                values: new object[] { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6806), "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", new DateTime(2025, 8, 1, 14, 19, 42, 190, DateTimeKind.Utc).AddTicks(6806), true, false });

            migrationBuilder.CreateIndex(
                name: "IX_Services_ApprovedByUserId",
                table: "Services",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedByUserId",
                table: "Services",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_AspNetUsers_ApprovedByUserId",
                table: "Services",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_AspNetUsers_CreatedByUserId",
                table: "Services",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_AspNetUsers_ApprovedByUserId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_AspNetUsers_CreatedByUserId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ApprovedByUserId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_CreatedByUserId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "LastServiceCreationDate",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f",
                column: "ConcurrencyStamp",
                value: "fe74c9d2-fc5c-4058-b95f-5c40a2a3c3f4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e",
                column: "ConcurrencyStamp",
                value: "9879ecec-d1b8-48aa-9895-665e8f1df7ec");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "99049752-95b1-477d-944a-f34589d31b09",
                column: "ConcurrencyStamp",
                value: "ab30b278-1fe4-4058-bdf5-c97b0317bb49");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "17f2c00d-127e-4a60-8b55-b7d857a45938", "81b41e60-bbfd-426e-a451-b324afac9042" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "bb81e0bc-25e2-4eda-9092-eaefb8045765", "91973d3d-b866-422d-b759-6dc734d7fad2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "960af341-2093-42d2-9174-7a28273b0431", "11d5fbe2-e03e-4300-869c-4c72a147b85b" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 82, DateTimeKind.Utc).AddTicks(3671));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 82, DateTimeKind.Utc).AddTicks(3689));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("1d4ae40b-c305-47b7-beed-163c4a0aeb40"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(613));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2ef43d87-d749-4d7d-9b7d-f7c4f527bea7"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(640));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("3a7b8b0c-1d2e-4f5a-a837-3d5e9f1a2b0c"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(735));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("8edc2d04-00f5-4630-b5a9-4fa499fc7210"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(739));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b422f89b-e7a3-4130-b899-7b56010007e0"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(636));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("c10de2fa-b49b-4c0d-9e8f-142b3cd40e6f"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(624));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("e11e539c-0290-4171-b606-16628d1790b0"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(621));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f0c72c7b-709d-44b7-81c1-1e5ab73305ec"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(628));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f5e402c0-91ba-4f8e-97d0-3b443fe10d3c"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(631));
        }
    }
}
