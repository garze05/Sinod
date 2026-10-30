using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sinod.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteCommercialFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Never silently discard duplicate historical events or accepted versions.
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM "OdEvents" GROUP BY "OpportunityId" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Resolve duplicate events per opportunity before applying CompleteCommercialFlow';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "Quotations" WHERE "State" = 2 GROUP BY "OpportunityId" HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Resolve duplicate accepted quotations before applying CompleteCommercialFlow';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_OdClients_Companies_CompanyId",
                table: "OdClients");

            migrationBuilder.DropForeignKey(
                name: "FK_OdEvents_Opportunities_OpportunityId",
                table: "OdEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_OdServices_OdServicePricingProfiles_PricingProfileId",
                table: "OdServices");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_OdClients_ClientId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityServices_OdServices_ServicesId",
                table: "OpportunityServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityServices_Opportunities_OpportunityId",
                table: "OpportunityServices");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Opportunities_OpportunityId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_OpportunityId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_OdEvents_OpportunityId",
                table: "OdEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientSnapshot",
                table: "Quotations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Quotations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventDate",
                table: "Quotations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsMunicipal",
                table: "Quotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationSnapshot",
                table: "Quotations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "Quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Surcharge",
                table: "Quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SurchargePercent",
                table: "Quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Quotations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Transport",
                table: "Quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Quotations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "Opportunities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "ClosureReason",
                table: "Opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "OdEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "OdEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OdEvents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "OdEvents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OdEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposits_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    BaseDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationItem_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // The old model had neither a version nor an economic snapshot. Preserve
            // those rows, number them deterministically, and require a new complete
            // version before they can be used for a new commercial commitment.
            migrationBuilder.Sql("""
                UPDATE "Quotations" AS q
                SET "Version" = versions.version,
                    "CreatedAt" = o."CreatedAt",
                    "EventDate" = COALESCE(o."EventDate", '-infinity'::timestamptz)
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "OpportunityId" ORDER BY "Id")::integer AS version
                    FROM "Quotations"
                ) AS versions, "Opportunities" AS o
                WHERE q."Id" = versions."Id" AND q."OpportunityId" = o."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_OneAcceptedPerOpportunity",
                table: "Quotations",
                column: "OpportunityId",
                unique: true,
                filter: "\"State\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_OpportunityId_Version",
                table: "Quotations",
                columns: new[] { "OpportunityId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OdEvents_OpportunityId",
                table: "OdEvents",
                column: "OpportunityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OdEvents_QuotationId",
                table: "OdEvents",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_OpportunityId",
                table: "Deposits",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItem_QuotationId",
                table: "QuotationItem",
                column: "QuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OdClients_Companies_CompanyId",
                table: "OdClients",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OdEvents_Opportunities_OpportunityId",
                table: "OdEvents",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OdEvents_Quotations_QuotationId",
                table: "OdEvents",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OdServices_OdServicePricingProfiles_PricingProfileId",
                table: "OdServices",
                column: "PricingProfileId",
                principalTable: "OdServicePricingProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_OdClients_ClientId",
                table: "Opportunities",
                column: "ClientId",
                principalTable: "OdClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityServices_OdServices_ServicesId",
                table: "OpportunityServices",
                column: "ServicesId",
                principalTable: "OdServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityServices_Opportunities_OpportunityId",
                table: "OpportunityServices",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Opportunities_OpportunityId",
                table: "Quotations",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OdClients_Companies_CompanyId",
                table: "OdClients");

            migrationBuilder.DropForeignKey(
                name: "FK_OdEvents_Opportunities_OpportunityId",
                table: "OdEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_OdEvents_Quotations_QuotationId",
                table: "OdEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_OdServices_OdServicePricingProfiles_PricingProfileId",
                table: "OdServices");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_OdClients_ClientId",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityServices_OdServices_ServicesId",
                table: "OpportunityServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityServices_Opportunities_OpportunityId",
                table: "OpportunityServices");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Opportunities_OpportunityId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "QuotationItem");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_OneAcceptedPerOpportunity",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_OpportunityId_Version",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_OdEvents_OpportunityId",
                table: "OdEvents");

            migrationBuilder.DropIndex(
                name: "IX_OdEvents_QuotationId",
                table: "OdEvents");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "ClientSnapshot",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "EventDate",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "IsMunicipal",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "LocationSnapshot",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Surcharge",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SurchargePercent",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Transport",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "ClosureReason",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "OdEvents");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "OdEvents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OdEvents");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "OdEvents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OdEvents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "Opportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_OpportunityId",
                table: "Quotations",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OdEvents_OpportunityId",
                table: "OdEvents",
                column: "OpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_OdClients_Companies_CompanyId",
                table: "OdClients",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OdEvents_Opportunities_OpportunityId",
                table: "OdEvents",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OdServices_OdServicePricingProfiles_PricingProfileId",
                table: "OdServices",
                column: "PricingProfileId",
                principalTable: "OdServicePricingProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_OdClients_ClientId",
                table: "Opportunities",
                column: "ClientId",
                principalTable: "OdClients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityServices_OdServices_ServicesId",
                table: "OpportunityServices",
                column: "ServicesId",
                principalTable: "OdServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityServices_Opportunities_OpportunityId",
                table: "OpportunityServices",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Opportunities_OpportunityId",
                table: "Quotations",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
