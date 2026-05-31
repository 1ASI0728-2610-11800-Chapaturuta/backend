using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Frock_backend.Migrations
{
    /// <inheritdoc />
    public partial class Capitulo5Restructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    document_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    photo_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    license_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    license_category = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    vehicle_plate = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    vehicle_brand = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    vehicle_model = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    vehicle_year = table.Column<int>(type: "int", nullable: false),
                    vehicle_capacity = table.Column<int>(type: "int", nullable: false),
                    vehicle_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    is_available = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_drivers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    method = table.Column<string>(type: "longtext", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    external_reference = table.Column<string>(type: "longtext", nullable: true),
                    reference_type = table.Column<string>(type: "longtext", nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_payments", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    plan_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    target_role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    billing_cycle = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    benefits = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    discovery_quota = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_plans", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "regions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_regions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "route_durations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_tariff = table.Column<int>(type: "int", nullable: false),
                    fk_id_route = table.Column<int>(type: "int", nullable: false),
                    estimated_minutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_route_durations", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    price = table.Column<double>(type: "double", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    frequency = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    distance_meters = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    duration_seconds = table.Column<int>(type: "int", nullable: true),
                    geometry = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_routes", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    fk_id_plan = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    starts_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ends_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    auto_renew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fk_id_payment = table.Column<int>(type: "int", nullable: true),
                    discovery_usage_in_cycle = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_subscriptions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tariffs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_driver = table.Column<int>(type: "int", nullable: false),
                    base_fare = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    price_per_km = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    price_per_minute = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    min_fare = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    available_days = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_tariffs", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "longtext", nullable: false),
                    username = table.Column<string>(type: "longtext", nullable: false),
                    role = table.Column<string>(type: "longtext", nullable: false),
                    password_hash = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_payment = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_refunds", x => x.id);
                    table.ForeignKey(
                        name: "f_k_refunds_payments_fk_id_payment",
                        column: x => x.fk_id_payment,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "provinces",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    fk_id_region = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_provinces", x => x.id);
                    table.ForeignKey(
                        name: "f_k_provinces__region_fk_id_region",
                        column: x => x.fk_id_region,
                        principalTable: "regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    route_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<string>(type: "longtext", nullable: false),
                    end_time = table.Column<string>(type: "longtext", nullable: false),
                    day_of_week = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    enabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_schedules", x => x.id);
                    table.ForeignKey(
                        name: "f_k_schedules_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_collections", x => x.id);
                    table.ForeignKey(
                        name: "f_k_collections__user_fk_id_user",
                        column: x => x.fk_id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Info"),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notifications", x => x.id);
                    table.ForeignKey(
                        name: "f_k_notifications_users_fk_id_user",
                        column: x => x.fk_id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ratings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    fk_id_driver = table.Column<int>(type: "int", nullable: false),
                    fk_id_trip = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ratings", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ratings_users_fk_id_driver",
                        column: x => x.fk_id_driver,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_ratings_users_fk_id_user",
                        column: x => x.fk_id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    fk_id_province = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_districts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_districts__province_fk_id_province",
                        column: x => x.fk_id_province,
                        principalTable: "provinces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collection_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_collection = table.Column<int>(type: "int", nullable: false),
                    fk_id_route = table.Column<int>(type: "int", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_collection_items", x => x.id);
                    table.ForeignKey(
                        name: "f_k_collection_items__routes_fk_id_route",
                        column: x => x.fk_id_route,
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_collection_items_collections_fk_id_collection",
                        column: x => x.fk_id_collection,
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "stops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    google_maps_url = table.Column<string>(type: "longtext", nullable: true),
                    image_url = table.Column<string>(type: "longtext", nullable: true),
                    fk_id_driver = table.Column<int>(type: "int", nullable: false),
                    address = table.Column<string>(type: "longtext", nullable: false),
                    reference = table.Column<string>(type: "longtext", nullable: false),
                    fk_id_district = table.Column<int>(type: "int", nullable: false),
                    latitude = table.Column<double>(type: "double", nullable: true),
                    longitude = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_stops", x => x.id);
                    table.ForeignKey(
                        name: "f_k_stops_districts_fk_id_district",
                        column: x => x.fk_id_district,
                        principalTable: "districts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_stops_drivers_fk_id_driver",
                        column: x => x.fk_id_driver,
                        principalTable: "drivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "route_stops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_stop_id = table.Column<int>(type: "int", nullable: false),
                    f_k_route_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_route_stops", x => x.id);
                    table.ForeignKey(
                        name: "f_k_route_stops__stop_fk_stop_id",
                        column: x => x.fk_stop_id,
                        principalTable: "stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_route_stops_routes_f_k_route_id",
                        column: x => x.f_k_route_id,
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    fk_id_driver = table.Column<int>(type: "int", nullable: true),
                    fk_id_route = table.Column<int>(type: "int", nullable: false),
                    fk_id_origin_stop = table.Column<int>(type: "int", nullable: false),
                    fk_id_destination_stop = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    price = table.Column<double>(type: "double", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    available_seats = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_trips", x => x.id);
                    table.ForeignKey(
                        name: "f_k_trips__routes_fk_id_route",
                        column: x => x.fk_id_route,
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_trips__stop_fk_id_destination_stop",
                        column: x => x.fk_id_destination_stop,
                        principalTable: "stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_trips__stop_fk_id_origin_stop",
                        column: x => x.fk_id_origin_stop,
                        principalTable: "stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_trips_users_fk_id_driver",
                        column: x => x.fk_id_driver,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_trips_users_fk_id_user",
                        column: x => x.fk_id_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fk_id_user = table.Column<int>(type: "int", nullable: false),
                    fk_id_trip = table.Column<int>(type: "int", nullable: false),
                    document_type = table.Column<string>(type: "longtext", nullable: false),
                    document_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    seats = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    fk_id_payment = table.Column<int>(type: "int", nullable: true),
                    reserved_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_reservations", x => x.id);
                    table.ForeignKey(
                        name: "f_k_reservations__trips_fk_id_trip",
                        column: x => x.fk_id_trip,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_collection_items_fk_id_collection",
                table: "collection_items",
                column: "fk_id_collection");

            migrationBuilder.CreateIndex(
                name: "i_x_collection_items_fk_id_route",
                table: "collection_items",
                column: "fk_id_route");

            migrationBuilder.CreateIndex(
                name: "i_x_collections_fk_id_user",
                table: "collections",
                column: "fk_id_user");

            migrationBuilder.CreateIndex(
                name: "i_x_districts_fk_id_province",
                table: "districts",
                column: "fk_id_province");

            migrationBuilder.CreateIndex(
                name: "i_x_notifications_fk_id_user",
                table: "notifications",
                column: "fk_id_user");

            migrationBuilder.CreateIndex(
                name: "i_x_provinces_fk_id_region",
                table: "provinces",
                column: "fk_id_region");

            migrationBuilder.CreateIndex(
                name: "i_x_ratings_fk_id_driver",
                table: "ratings",
                column: "fk_id_driver");

            migrationBuilder.CreateIndex(
                name: "i_x_ratings_fk_id_user",
                table: "ratings",
                column: "fk_id_user");

            migrationBuilder.CreateIndex(
                name: "i_x_refunds_fk_id_payment",
                table: "refunds",
                column: "fk_id_payment");

            migrationBuilder.CreateIndex(
                name: "i_x_reservations_fk_id_trip",
                table: "reservations",
                column: "fk_id_trip");

            migrationBuilder.CreateIndex(
                name: "i_x_route_stops_f_k_route_id",
                table: "route_stops",
                column: "f_k_route_id");

            migrationBuilder.CreateIndex(
                name: "i_x_route_stops_fk_stop_id",
                table: "route_stops",
                column: "fk_stop_id");

            migrationBuilder.CreateIndex(
                name: "i_x_schedules_route_id",
                table: "schedules",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "i_x_stops_fk_id_district",
                table: "stops",
                column: "fk_id_district");

            migrationBuilder.CreateIndex(
                name: "i_x_stops_fk_id_driver",
                table: "stops",
                column: "fk_id_driver");

            migrationBuilder.CreateIndex(
                name: "i_x_trips_fk_id_destination_stop",
                table: "trips",
                column: "fk_id_destination_stop");

            migrationBuilder.CreateIndex(
                name: "i_x_trips_fk_id_driver",
                table: "trips",
                column: "fk_id_driver");

            migrationBuilder.CreateIndex(
                name: "i_x_trips_fk_id_origin_stop",
                table: "trips",
                column: "fk_id_origin_stop");

            migrationBuilder.CreateIndex(
                name: "i_x_trips_fk_id_route",
                table: "trips",
                column: "fk_id_route");

            migrationBuilder.CreateIndex(
                name: "i_x_trips_fk_id_user",
                table: "trips",
                column: "fk_id_user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_items");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "route_durations");

            migrationBuilder.DropTable(
                name: "route_stops");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "tariffs");

            migrationBuilder.DropTable(
                name: "collections");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "stops");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "districts");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.DropTable(
                name: "provinces");

            migrationBuilder.DropTable(
                name: "regions");
        }
    }
}
