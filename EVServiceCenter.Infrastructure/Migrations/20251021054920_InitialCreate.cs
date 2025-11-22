using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVServiceCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentStatus",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Appointm__C8EE204358988DD1", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "CarBrands",
                columns: table => new
                {
                    BrandID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CarBrand__DAD4F3BEFB91F09A", x => x.BrandID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__516F03952CD0AA46", x => x.TypeID);
                });

            migrationBuilder.CreateTable(
                name: "KPIMetrics",
                columns: table => new
                {
                    MetricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CalculationFormula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetValue = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KPIMetri__561056459F678B59", x => x.MetricID);
                });

            migrationBuilder.CreateTable(
                name: "MaintenancePackages",
                columns: table => new
                {
                    PackageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValidityPeriod = table.Column<int>(type: "int", nullable: true),
                    ValidityMileage = table.Column<int>(type: "int", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPopular = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Maintena__322035EC3FB048A9", x => x.PackageID);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultEnabled = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__516F03953D44EDA0", x => x.TypeID);
                });

            migrationBuilder.CreateTable(
                name: "PartCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentCategoryID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PartCate__19093A2B4A988338", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK__PartCateg__Paren__18B6AB08",
                        column: x => x.ParentCategoryID,
                        principalTable: "PartCategories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    MethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MethodCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MethodName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GatewayProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProviderConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingFee = table.Column<decimal>(type: "decimal(5,4)", nullable: true, defaultValue: 0m),
                    FixedFee = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    IsOnline = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentM__FC681FB15BC7676A", x => x.MethodID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ServiceC__19093A2BFAC23C48", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    Rating = table.Column<int>(type: "int", nullable: true, defaultValue: 3),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Supplier__4BE666946869E259", x => x.SupplierID);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRole__8AFACE3A7DD86565", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyTypes",
                columns: table => new
                {
                    WarrantyTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultPeriod = table.Column<int>(type: "int", nullable: true, defaultValue: 12),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Warranty__EDA140F3DF34F290", x => x.WarrantyTypeID);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderStatus",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AllowEdit = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    RequireApproval = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WorkOrde__C8EE20434E20B889", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "CarModels",
                columns: table => new
                {
                    ModelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandID = table.Column<int>(type: "int", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    BatteryCapacity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxRange = table.Column<int>(type: "int", nullable: true),
                    ChargingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotorPower = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AccelerationTime = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    TopSpeed = table.Column<int>(type: "int", nullable: true),
                    ServiceInterval = table.Column<int>(type: "int", nullable: true, defaultValue: 10000),
                    ServiceIntervalMonths = table.Column<int>(type: "int", nullable: true, defaultValue: 6),
                    WarrantyPeriod = table.Column<int>(type: "int", nullable: true, defaultValue: 24),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CarModel__E8D7A1CC320085B4", x => x.ModelID);
                    table.ForeignKey(
                        name: "FK__CarModels__Brand__00200768",
                        column: x => x.BrandID,
                        principalTable: "CarBrands",
                        principalColumn: "BrandID");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceServices",
                columns: table => new
                {
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    ServiceCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StandardTime = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    SkillLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiredCertification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsWarrantyService = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    WarrantyPeriod = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Maintena__C51BB0EA01301206", x => x.ServiceID);
                    table.ForeignKey(
                        name: "FK__Maintenan__Categ__17036CC0",
                        column: x => x.CategoryID,
                        principalTable: "ServiceCategories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(64)", maxLength: 64, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    PasswordExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ResetToken = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerificationToken = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EmailVerificationExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountLockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAccountLocked = table.Column<bool>(type: "bit", nullable: false),
                    LastFailedLoginAttempt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockoutReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnlockAttempts = table.Column<int>(type: "int", nullable: true),
                    ExternalProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalProviderId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC6BBF0671", x => x.UserID);
                    table.ForeignKey(
                        name: "FK__Users__CreatedBy__5441852A",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Users__RoleID__534D60F1",
                        column: x => x.RoleID,
                        principalTable: "UserRoles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "ModelServicePricing",
                columns: table => new
                {
                    PricingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    CustomTime = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(CONVERT([date],getdate()))"),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ModelSer__EC306B7289B90AA9", x => x.PricingID);
                    table.ForeignKey(
                        name: "FK__ModelServ__Model__282DF8C2",
                        column: x => x.ModelID,
                        principalTable: "CarModels",
                        principalColumn: "ModelID");
                    table.ForeignKey(
                        name: "FK__ModelServ__Servi__29221CFB",
                        column: x => x.ServiceID,
                        principalTable: "MaintenanceServices",
                        principalColumn: "ServiceID");
                });

            migrationBuilder.CreateTable(
                name: "PackageServices",
                columns: table => new
                {
                    PackageServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    IncludedInPackage = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    AdditionalCost = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PackageS__5EAFC2105FEBAB58", x => x.PackageServiceID);
                    table.ForeignKey(
                        name: "FK__PackageSe__Packa__22751F6C",
                        column: x => x.PackageID,
                        principalTable: "MaintenancePackages",
                        principalColumn: "PackageID");
                    table.ForeignKey(
                        name: "FK__PackageSe__Servi__236943A5",
                        column: x => x.ServiceID,
                        principalTable: "MaintenanceServices",
                        principalColumn: "ServiceID");
                });

            migrationBuilder.CreateTable(
                name: "APIKeys",
                columns: table => new
                {
                    KeyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    APIKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    KeyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Internal"),
                    AllowedIPs = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RateLimit = table.Column<int>(type: "int", nullable: true, defaultValue: 1000),
                    CurrentUsage = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__APIKeys__21F5BE2711270D11", x => x.KeyID);
                    table.ForeignKey(
                        name: "FK__APIKeys__Created__0504B816",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "BusinessRules",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RuleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(CONVERT([date],getdate()))"),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Business__110458C2A7189762", x => x.RuleID);
                    table.ForeignKey(
                        name: "FK__BusinessR__Creat__5EDF0F2E",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    CertificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CertificationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CertificationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Issuer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active"),
                    RenewalRequired = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    RenewalReminderSent = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Certific__1237E5AA18E73C54", x => x.CertificationID);
                    table.ForeignKey(
                        name: "FK__Certifica__Creat__3C89F72A",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Certifica__UserI__3B95D2F1",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "ChatQuickReplies",
                columns: table => new
                {
                    QuickReplyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UseCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChatQuic__C682A55EBE5F3B85", x => x.QuickReplyID);
                    table.ForeignKey(
                        name: "FK__ChatQuick__Creat__1293BD5E",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "ChecklistTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: true),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Checklis__F87ADD07FE6D782B", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK__Checklist__Categ__0C50D423",
                        column: x => x.CategoryID,
                        principalTable: "ServiceCategories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK__Checklist__Creat__0D44F85C",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Checklist__Servi__0B5CAFEA",
                        column: x => x.ServiceID,
                        principalTable: "MaintenanceServices",
                        principalColumn: "ServiceID");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IdentityNumber = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: true),
                    TypeID = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "vi-VN"),
                    MarketingOptIn = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    LoyaltyPoints = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    TotalSpent = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    LastVisitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__A4AE64B8632A69D9", x => x.CustomerID);
                    table.ForeignKey(
                        name: "FK_Customers_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__Customers__Creat__6B24EA82",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Customers__TypeI__6A30C649",
                        column: x => x.TypeID,
                        principalTable: "CustomerTypes",
                        principalColumn: "TypeID");
                    table.ForeignKey(
                        name: "FK__Customers__Updat__6C190EBB",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "DataRetentionPolicies",
                columns: table => new
                {
                    PolicyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RetentionPeriodMonths = table.Column<int>(type: "int", nullable: false),
                    ArchiveTableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteAfterArchive = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    RetentionCondition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastExecuted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextExecution = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DataRete__2E133944D99AE100", x => x.PolicyID);
                    table.ForeignKey(
                        name: "FK__DataReten__Creat__7C6F7215",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSkills",
                columns: table => new
                {
                    SkillID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SkillName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SkillLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CertificationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CertifyingBody = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CertificationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    VerifiedBy = table.Column<int>(type: "int", nullable: true),
                    VerifiedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Employee__DFA091E7A81A406B", x => x.SkillID);
                    table.ForeignKey(
                        name: "FK__EmployeeS__UserI__1C1D2798",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__EmployeeS__Verif__1D114BD1",
                        column: x => x.VerifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPrograms",
                columns: table => new
                {
                    ProgramID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PointsPerDollar = table.Column<decimal>(type: "decimal(10,4)", nullable: true, defaultValue: 1.0m),
                    WelcomeBonus = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ReferralBonus = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    BirthdayBonus = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    MinimumRedemption = table.Column<int>(type: "int", nullable: true, defaultValue: 100),
                    PointsExpiryDays = table.Column<int>(type: "int", nullable: true, defaultValue: 365),
                    TierThresholds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(CONVERT([date],getdate()))"),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoyaltyP__752560385C07EBF9", x => x.ProgramID);
                    table.ForeignKey(
                        name: "FK__LoyaltyPr__Creat__6A50C1DA",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypeID = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriggerEvent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TriggerCondition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SendDelay = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__F87ADD07A2668407", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK__Notificat__Creat__53A266AC",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Notificat__TypeI__52AE4273",
                        column: x => x.TypeID,
                        principalTable: "NotificationTypes",
                        principalColumn: "TypeID");
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    PartID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BarCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    BrandID = table.Column<int>(type: "int", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "PCS"),
                    CostPrice = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    SellingPrice = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MinStock = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CurrentStock = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ReorderLevel = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    MaxStock = table.Column<int>(type: "int", nullable: true, defaultValue: 1000),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    Dimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WarrantyPeriod = table.Column<int>(type: "int", nullable: true, defaultValue: 12),
                    PartCondition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "New"),
                    IsConsumable = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TechnicalSpecs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompatibleModels = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    AlternativePartIDs = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastStockUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    LastCostUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Parts__7C3F0D3053AF215E", x => x.PartID);
                    table.ForeignKey(
                        name: "FK__Parts__BrandID__2F9A1060",
                        column: x => x.BrandID,
                        principalTable: "CarBrands",
                        principalColumn: "BrandID");
                    table.ForeignKey(
                        name: "FK__Parts__CategoryI__2EA5EC27",
                        column: x => x.CategoryID,
                        principalTable: "PartCategories",
                        principalColumn: "CategoryID");
                    table.ForeignKey(
                        name: "FK__Parts__CreatedBy__318258D2",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Parts__SupplierI__308E3499",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID");
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    MetricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MetricDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkOrdersCompleted = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ServicesCompleted = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AverageServiceTime = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    EfficiencyRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    CustomerRatingAvg = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    ReworkCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ComplaintCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    HoursWorked = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    OvertimeHours = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    LateCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AbsentCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    RevenueGenerated = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    TrainingHoursCompleted = table.Column<decimal>(type: "decimal(8,2)", nullable: true, defaultValue: 0m),
                    CertificationsEarned = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Performa__561056451E964448", x => x.MetricID);
                    table.ForeignKey(
                        name: "FK__Performan__UserI__33F4B129",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    PromotionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PromotionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PromotionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    MaximumDiscount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    ApplicableServices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicableCategories = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerTypes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Promotio__52C42F2F955E70A8", x => x.PromotionID);
                    table.ForeignKey(
                        name: "FK__Promotion__Creat__76B698BF",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "RevokedTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RevokeReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RevokedTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK__RevokedTokens__Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EventDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Low"),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    BlockReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResolvedBy = table.Column<int>(type: "int", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Security__7944C870A4DE4C35", x => x.EventID);
                    table.ForeignKey(
                        name: "FK__SecurityE__Resol__49E3F248",
                        column: x => x.ResolvedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__SecurityE__UserI__48EFCE0F",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "ServiceCenters",
                columns: table => new
                {
                    CenterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CenterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ManagerID = table.Column<int>(type: "int", nullable: true),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true, defaultValue: 10),
                    Facilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ServiceC__398FC7D75D87FF4A", x => x.CenterID);
                    table.ForeignKey(
                        name: "FK__ServiceCe__Manag__2FCF1A8A",
                        column: x => x.ManagerID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    ConfigID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "String"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEditable = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    RequiresRestart = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SystemCo__C3BC333CA8449B51", x => x.ConfigID);
                    table.ForeignKey(
                        name: "FK__SystemCon__Updat__5832119F",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SessionToken = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    LastActivityTime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserSess__C9F49270CCD65102", x => x.SessionID);
                    table.ForeignKey(
                        name: "FK__UserSessi__UserI__5AEE82B9",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "APIRequestLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyID = table.Column<int>(type: "int", nullable: true),
                    RequestMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RequestURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseStatus = table.Column<int>(type: "int", nullable: true),
                    ResponseHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseTime = table.Column<int>(type: "int", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__APIReque__5E5499A8CDACA85C", x => x.LogID);
                    table.ForeignKey(
                        name: "FK__APIReques__KeyID__08D548FA",
                        column: x => x.KeyID,
                        principalTable: "APIKeys",
                        principalColumn: "KeyID");
                });

            migrationBuilder.CreateTable(
                name: "ChatChannels",
                columns: table => new
                {
                    ChannelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChannelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Support"),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    AssignedUserID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active"),
                    Priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Normal"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    LastMessageDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedBy = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChatChan__38C3E8F48AF9D1A3", x => x.ChannelID);
                    table.ForeignKey(
                        name: "FK__ChatChann__Assig__00750D23",
                        column: x => x.AssignedUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__ChatChann__Close__0169315C",
                        column: x => x.ClosedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__ChatChann__Custo__7F80E8EA",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID");
                });

            migrationBuilder.CreateTable(
                name: "CustomerCommunicationPreferences",
                columns: table => new
                {
                    PreferenceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    SMSNotifications = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    PushNotifications = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    MarketingCommunications = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    ServiceReminders = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    PromotionalOffers = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__E228490FEC3F25E4", x => x.PreferenceID);
                    table.ForeignKey(
                        name: "FK__CustomerC__Custo__75A278F5",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID");
                });

            migrationBuilder.CreateTable(
                name: "CustomerVehicles",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    ModelID = table.Column<int>(type: "int", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PurchaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Mileage = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    LastMaintenanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NextMaintenanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastMaintenanceMileage = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    NextMaintenanceMileage = table.Column<int>(type: "int", nullable: true),
                    BatteryHealthPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 100m),
                    VehicleCondition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Good"),
                    InsuranceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InsuranceExpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistrationExpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__476B54B26D8CEE26", x => x.VehicleID);
                    table.ForeignKey(
                        name: "FK__CustomerV__Custo__0A9D95DB",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK__CustomerV__Model__0B91BA14",
                        column: x => x.ModelID,
                        principalTable: "CarModels",
                        principalColumn: "ModelID");
                    table.ForeignKey(
                        name: "FK__CustomerV__Updat__0C85DE4D",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    ProgramID = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoyaltyT__55433A4B990C52E0", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK__LoyaltyTr__Creat__70099B30",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__LoyaltyTr__Custo__6E2152BE",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK__LoyaltyTr__Progr__6F1576F7",
                        column: x => x.ProgramID,
                        principalTable: "LoyaltyPrograms",
                        principalColumn: "ProgramID");
                });

            migrationBuilder.CreateTable(
                name: "AutoNotificationRules",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    TriggerTable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TriggerEvent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TriggerCondition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Normal"),
                    MaxRetries = table.Column<int>(type: "int", nullable: true, defaultValue: 3),
                    RetryInterval = table.Column<int>(type: "int", nullable: true, defaultValue: 60),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AutoNoti__110458C29FBAE080", x => x.RuleID);
                    table.ForeignKey(
                        name: "FK__AutoNotif__Creat__5C37ACAD",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__AutoNotif__Templ__5B438874",
                        column: x => x.TemplateID,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateID");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TemplateID = table.Column<int>(type: "int", nullable: true),
                    RecipientType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Normal"),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Pending"),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    RelatedType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedID = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E321CBA13BB", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK__Notificat__Creat__740F363E",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Notificat__Custo__731B1205",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK__Notificat__Templ__7132C993",
                        column: x => x.TemplateID,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateID");
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__7226EDCC",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "DailyMetrics",
                columns: table => new
                {
                    DailyMetricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterID = table.Column<int>(type: "int", nullable: true),
                    MetricDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AppointmentsScheduled = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AppointmentsCompleted = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AppointmentsCancelled = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    WorkOrdersCreated = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    WorkOrdersCompleted = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DailyRevenue = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    ServiceRevenue = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    PartsRevenue = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    NewCustomers = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CustomerSatisfactionAvg = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    AverageServiceTime = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    TechnicianUtilization = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ReworkCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    WarrantyClaimsCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyMet__D09962A318FC9D36", x => x.DailyMetricID);
                    table.ForeignKey(
                        name: "FK__DailyMetr__Cente__2F650636",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ManagerID = table.Column<int>(type: "int", nullable: true),
                    CenterID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__B2079BCD7A1C4AC9", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK__Departmen__Cente__184C96B4",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__Departmen__Manag__1758727B",
                        column: x => x.ManagerID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "PartInventory",
                columns: table => new
                {
                    InventoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartID = table.Column<int>(type: "int", nullable: false),
                    CenterID = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ReservedStock = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AvailableStock = table.Column<int>(type: "int", nullable: true, computedColumnSql: "([CurrentStock]-[ReservedStock])", stored: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastStockTakeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PartInve__F5FDE6D3649D35C9", x => x.InventoryID);
                    table.ForeignKey(
                        name: "FK__PartInven__Cente__382F5661",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__PartInven__PartI__373B3228",
                        column: x => x.PartID,
                        principalTable: "Parts",
                        principalColumn: "PartID");
                    table.ForeignKey(
                        name: "FK__PartInven__Updat__39237A9A",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    PurchaseOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierID = table.Column<int>(type: "int", nullable: false),
                    CenterID = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(CONVERT([date],getdate()))"),
                    RequiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Pending"),
                    SubTotal = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    ShippingCost = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "decimal(17,2)", nullable: true, computedColumnSql: "(([SubTotal]+[TaxAmount])+[ShippingCost])", stored: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Purchase__036BAC449534989B", x => x.PurchaseOrderID);
                    table.ForeignKey(
                        name: "FK__PurchaseO__Appro__52E34C9D",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__PurchaseO__Cente__51EF2864",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__PurchaseO__Creat__53D770D6",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__PurchaseO__Suppl__50FB042B",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID");
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReportCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CenterID = table.Column<int>(type: "int", nullable: true),
                    TotalRecords = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    ProfitAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    KeyInsights = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FileSize = table.Column<int>(type: "int", nullable: true),
                    GeneratedBy = table.Column<int>(type: "int", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    LastAccessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessCount = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    ScheduleFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NextRunDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reports__D5BD48E5F07D7347", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK__Reports__CenterI__45544755",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__Reports__Generat__46486B8E",
                        column: x => x.GeneratedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CenterID = table.Column<int>(type: "int", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    BreakEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ShiftType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Regular"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Scheduled"),
                    ActualStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ActualEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    WorkedHours = table.Column<decimal>(type: "numeric(17,6)", nullable: true, computedColumnSql: "(case when [ActualStartTime] IS NOT NULL AND [ActualEndTime] IS NOT NULL then datediff(minute,[ActualStartTime],[ActualEndTime])/(60.0)  end)", stored: true),
                    ActualBreakStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    ActualBreakEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    BreakMinutes = table.Column<int>(type: "int", nullable: true, computedColumnSql: "(case when [ActualBreakStart] IS NOT NULL AND [ActualBreakEnd] IS NOT NULL then datediff(minute,[ActualBreakStart],[ActualBreakEnd]) else (0) end)", stored: true),
                    NetWorkingHours = table.Column<decimal>(type: "numeric(18,6)", nullable: true, computedColumnSql: "(case when [ActualStartTime] IS NOT NULL AND [ActualEndTime] IS NOT NULL then datediff(minute,[ActualStartTime],[ActualEndTime])/(60.0)-case when [ActualBreakStart] IS NOT NULL AND [ActualBreakEnd] IS NOT NULL then datediff(minute,[ActualBreakStart],[ActualBreakEnd])/(60.0) else (0) end  end)", stored: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLate = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsEarlyLeave = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    AbsentReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Shifts__C0A838E1D7590458", x => x.ShiftID);
                    table.ForeignKey(
                        name: "FK__Shifts__Approved__269AB60B",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Shifts__CenterID__25A691D2",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__Shifts__CreatedB__278EDA44",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Shifts__UserID__24B26D99",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartID = table.Column<int>(type: "int", nullable: false),
                    CenterID = table.Column<int>(type: "int", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(26,2)", nullable: true, computedColumnSql: "([Quantity]*[UnitCost])", stored: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceID = table.Column<int>(type: "int", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StockTra__55433A4B44E46CCE", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK__StockTran__Cente__4589517F",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__StockTran__Creat__477199F1",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__StockTran__PartI__44952D46",
                        column: x => x.PartID,
                        principalTable: "Parts",
                        principalColumn: "PartID");
                    table.ForeignKey(
                        name: "FK__StockTran__Suppl__467D75B8",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierID");
                });

            migrationBuilder.CreateTable(
                name: "TechnicianSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TechnicianID = table.Column<int>(type: "int", nullable: false),
                    CenterID = table.Column<int>(type: "int", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    BreakEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    MaxCapacityMinutes = table.Column<int>(type: "int", nullable: true, defaultValue: 480),
                    BookedMinutes = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    AvailableMinutes = table.Column<int>(type: "int", nullable: true, computedColumnSql: "([MaxCapacityMinutes]-[BookedMinutes])", stored: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ShiftType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Regular"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Technici__9C8A5B6984F36596", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK__Technicia__Cente__3864608B",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                    table.ForeignKey(
                        name: "FK__Technicia__Techn__37703C52",
                        column: x => x.TechnicianID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    SlotID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterID = table.Column<int>(type: "int", nullable: false),
                    SlotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    MaxBookings = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SlotType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Regular"),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TimeSlot__0A124A4F359B1C4C", x => x.SlotID);
                    table.ForeignKey(
                        name: "FK__TimeSlots__Cente__40058253",
                        column: x => x.CenterID,
                        principalTable: "ServiceCenters",
                        principalColumn: "CenterID");
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SessionID = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecordID = table.Column<int>(type: "int", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "Info"),
                    Success = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExecutionTime = table.Column<int>(type: "int", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Activity__5E5499A89BA8FBB4", x => x.LogID);
                    table.ForeignKey(
                        name: "FK__ActivityL__Sessi__4336F4B9",
                        column: x => x.SessionID,
                        principalTable: "UserSessions",
                        principalColumn: "SessionID");
                    table.ForeignKey(
                        name: "FK__ActivityL__UserI__4242D080",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");
        }
    }
}
