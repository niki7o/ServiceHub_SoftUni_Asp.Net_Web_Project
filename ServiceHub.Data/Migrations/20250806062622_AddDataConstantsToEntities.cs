using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataConstantsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Services",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Services",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f",
                column: "ConcurrencyStamp",
                value: "f14f9a98-c213-4e6e-8683-93a2a51129fc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e",
                column: "ConcurrencyStamp",
                value: "8a251694-6d42-4944-b938-1a64959afcbc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "99049752-95b1-477d-944a-f34589d31b09",
                column: "ConcurrencyStamp",
                value: "04e8c6c7-02b1-4420-93e9-352c7debe61d");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "54f2ffb5-187d-4705-8ba7-eced5295d1b6", "f66ba631-9742-46e6-aff2-88b606948423" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "0c82364e-67b9-4d2a-8eb8-ae1c9f23a9f5", "838cd190-3e91-4fed-a54e-a9904753be41" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "1a293b56-50f1-41cd-978b-416592f6e2ed", "d1e26423-0fa5-40b3-af80-87144b999844" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 6, 6, 26, 21, 203, DateTimeKind.Utc).AddTicks(577));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 6, 6, 26, 21, 203, DateTimeKind.Utc).AddTicks(584));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("1d4ae40b-c305-47b7-beed-163c4a0aeb40"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5030), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5027), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2ef43d87-d749-4d7d-9b7d-f7c4f527bea7"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5072), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5071), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("3a7b8b0c-1d2e-4f5a-a837-3d5e9f1a2b0c"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5086), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5086), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("8edc2d04-00f5-4630-b5a9-4fa499fc7210"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5091), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5090), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b422f89b-e7a3-4130-b899-7b56010007e0"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5067), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5067), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("c10de2fa-b49b-4c0d-9e8f-142b3cd40e6f"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5051), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5051), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("e11e539c-0290-4171-b606-16628d1790b0"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5045), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5045), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f0c72c7b-709d-44b7-81c1-1e5ab73305ec"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5056), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5056), null });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f5e402c0-91ba-4f8e-97d0-3b443fe10d3c"),
                columns: new[] { "ApprovedOn", "CreatedOn", "ImageUrl" },
                values: new object[] { new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5061), new DateTime(2025, 8, 6, 6, 26, 21, 205, DateTimeKind.Utc).AddTicks(5060), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Services");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f",
                column: "ConcurrencyStamp",
                value: "8a9b1750-8670-48ce-a1d1-55467364d659");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e",
                column: "ConcurrencyStamp",
                value: "f7eb7149-a955-4ea8-a1c1-9deeaa3bc15e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "99049752-95b1-477d-944a-f34589d31b09",
                column: "ConcurrencyStamp",
                value: "f19540ea-e6e3-4a1f-9bca-b56e408e75d9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ba7772da-efb2-483a-8210-88e6ee633e96", "d7cc2e1d-d60c-42b6-9938-e67274de1e4e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a474e182-c16a-4d30-8251-ff86a8e4e2cf", "ab4f39d7-e260-44d5-b5be-641791cb3291" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "075f81d2-6a6b-40da-8967-8e3642efe00c", "5b0aa9ad-43a6-475f-b3d2-b80cc1abfea0" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 5, 22, 5, 23, 851, DateTimeKind.Utc).AddTicks(9997));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"),
                column: "CreatedOn",
                value: new DateTime(2025, 8, 5, 22, 5, 23, 852, DateTimeKind.Utc).AddTicks(2));

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("1d4ae40b-c305-47b7-beed-163c4a0aeb40"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(848), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(846) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("2ef43d87-d749-4d7d-9b7d-f7c4f527bea7"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(886), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(886) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("3a7b8b0c-1d2e-4f5a-a837-3d5e9f1a2b0c"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(891), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(890) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("8edc2d04-00f5-4630-b5a9-4fa499fc7210"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(898), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(898) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b422f89b-e7a3-4130-b899-7b56010007e0"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(881), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(881) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("c10de2fa-b49b-4c0d-9e8f-142b3cd40e6f"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(867), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(866) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("e11e539c-0290-4171-b606-16628d1790b0"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(862), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(861) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f0c72c7b-709d-44b7-81c1-1e5ab73305ec"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(871), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(871) });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("f5e402c0-91ba-4f8e-97d0-3b443fe10d3c"),
                columns: new[] { "ApprovedOn", "CreatedOn" },
                values: new object[] { new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(875), new DateTime(2025, 8, 5, 22, 5, 23, 854, DateTimeKind.Utc).AddTicks(875) });
        }
    }
}
