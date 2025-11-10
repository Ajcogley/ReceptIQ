using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ReciptIQ.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiProcessingLog> AiProcessingLogs { get; set; }

    public virtual DbSet<Approval> Approvals { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<DashboardStat> DashboardStats { get; set; }

    public virtual DbSet<MonthlyExpensesByCategory> MonthlyExpensesByCategories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<ReceiptItem> ReceiptItems { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<AiProcessingLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_processing_logs_pkey");

            entity.ToTable("ai_processing_logs", tb => tb.HasComment("Logs de todas las llamadas a APIs de IA para debugging y auditoría"));

            entity.HasIndex(e => e.CreatedAt, "idx_ai_logs_created").IsDescending();

            entity.HasIndex(e => e.Provider, "idx_ai_logs_provider");

            entity.HasIndex(e => e.ReceiptId, "idx_ai_logs_receipt");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ExtractionSuccessful).HasColumnName("extraction_successful");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
            entity.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasColumnName("provider");
            entity.Property(e => e.RawResponse)
                .HasColumnType("jsonb")
                .HasColumnName("raw_response");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.TokensUsed).HasColumnName("tokens_used");

            entity.HasOne(d => d.Receipt).WithMany(p => p.AiProcessingLogs)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("ai_processing_logs_receipt_id_fkey");
        });

        modelBuilder.Entity<Approval>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("approvals_pkey");

            entity.ToTable("approvals", tb => tb.HasComment("Historial completo de aprobaciones y rechazos con comentarios"));

            entity.HasIndex(e => e.Action, "idx_approvals_action");

            entity.HasIndex(e => e.ApproverId, "idx_approvals_approver");

            entity.HasIndex(e => e.CreatedAt, "idx_approvals_created").IsDescending();

            entity.HasIndex(e => e.ReceiptId, "idx_approvals_receipt");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .HasColumnName("action");
            entity.Property(e => e.ApproverId).HasColumnName("approver_id");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");

            entity.HasOne(d => d.Approver).WithMany(p => p.Approvals)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("approvals_approver_id_fkey");

            entity.HasOne(d => d.Receipt).WithMany(p => p.Approvals)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("approvals_receipt_id_fkey");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs", tb => tb.HasComment("Auditoría completa de todas las operaciones críticas"));

            entity.HasIndex(e => e.CreatedAt, "idx_audit_logs_created").IsDescending();

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "idx_audit_logs_entity");

            entity.HasIndex(e => e.UserId, "idx_audit_logs_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.NewValues)
                .HasColumnType("jsonb")
                .HasColumnName("new_values");
            entity.Property(e => e.OldValues)
                .HasColumnType("jsonb")
                .HasColumnName("old_values");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("audit_logs_user_id_fkey");
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("budgets_pkey");

            entity.ToTable("budgets", tb => tb.HasComment("Presupuestos mensuales por categoría y/o usuario"));

            entity.HasIndex(e => new { e.CompanyId, e.CategoryId, e.UserId, e.Month, e.Year }, "budgets_company_id_category_id_user_id_month_year_key").IsUnique();

            entity.HasIndex(e => e.AlertSent, "idx_budgets_alert").HasFilter("(alert_sent = false)");

            entity.HasIndex(e => e.CategoryId, "idx_budgets_category");

            entity.HasIndex(e => e.CompanyId, "idx_budgets_company");

            entity.HasIndex(e => new { e.Year, e.Month }, "idx_budgets_period").IsDescending();

            entity.HasIndex(e => e.UserId, "idx_budgets_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AlertSent)
                .HasDefaultValue(false)
                .HasColumnName("alert_sent");
            entity.Property(e => e.AlertThreshold)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.80")
                .HasColumnName("alert_threshold");
            entity.Property(e => e.BudgetAmount)
                .HasPrecision(12, 2)
                .HasColumnName("budget_amount");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.RemainingAmount)
                .HasPrecision(12, 2)
                .HasComputedColumnSql("(budget_amount - spent_amount)", true)
                .HasColumnName("remaining_amount");
            entity.Property(e => e.SpentAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("spent_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasComment("NULL = presupuesto de empresa completa, UUID = presupuesto individual")
                .HasColumnName("user_id");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Category).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("budgets_category_id_fkey");

            entity.HasOne(d => d.Company).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("budgets_company_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("budgets_user_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories", tb => tb.HasComment("Categorías de gastos personalizables por empresa"));

            entity.HasIndex(e => new { e.CompanyId, e.Name }, "categories_company_id_name_key").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_categories_active").HasFilter("(is_active = true)");

            entity.HasIndex(e => e.CompanyId, "idx_categories_company");

            entity.HasIndex(e => e.DisplayOrder, "idx_categories_order");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.BudgetMonthly)
                .HasPrecision(12, 2)
                .HasColumnName("budget_monthly");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValueSql("'#3B82F6'::character varying")
                .HasColumnName("color");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasDefaultValueSql("'receipt'::character varying")
                .HasColumnName("icon");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasComment("true para categorías del sistema (Comida, Transporte, etc.)")
                .HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany(p => p.Categories)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("categories_company_id_fkey");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("companies_pkey");

            entity.ToTable("companies", tb => tb.HasComment("Empresas u organizaciones que usan la plataforma"));

            entity.HasIndex(e => e.Slug, "companies_slug_key").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_companies_active").HasFilter("(is_active = true)");

            entity.HasIndex(e => e.Slug, "idx_companies_slug");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Industry)
                .HasMaxLength(100)
                .HasColumnName("industry");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Settings)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasComment("Configuraciones JSON: políticas de gastos, límites, notificaciones")
                .HasColumnType("jsonb")
                .HasColumnName("settings");
            entity.Property(e => e.Slug)
                .HasMaxLength(100)
                .HasColumnName("slug");
            entity.Property(e => e.TaxId)
                .HasMaxLength(50)
                .HasColumnName("tax_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<DashboardStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("dashboard_stats");

            entity.Property(e => e.ApprovedReceipts).HasColumnName("approved_receipts");
            entity.Property(e => e.ApprovedSpent).HasColumnName("approved_spent");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.PendingReceipts).HasColumnName("pending_receipts");
            entity.Property(e => e.TotalReceipts).HasColumnName("total_receipts");
            entity.Property(e => e.TotalSpent).HasColumnName("total_spent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<MonthlyExpensesByCategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("monthly_expenses_by_category");

            entity.Property(e => e.AvgAmount).HasColumnName("avg_amount");
            entity.Property(e => e.CategoryColor)
                .HasMaxLength(7)
                .HasColumnName("category_color");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ReceiptCount).HasColumnName("receipt_count");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications", tb => tb.HasComment("Notificaciones en la app para usuarios"));

            entity.HasIndex(e => e.CreatedAt, "idx_notifications_created").IsDescending();

            entity.HasIndex(e => e.Type, "idx_notifications_type");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_notifications_unread").HasFilter("(is_read = false)");

            entity.HasIndex(e => e.UserId, "idx_notifications_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Link)
                .HasMaxLength(500)
                .HasColumnName("link");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_user_id_fkey");
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("receipts_pkey");

            entity.ToTable("receipts", tb => tb.HasComment("Recibos/facturas con datos extraídos por IA"));

            entity.HasIndex(e => e.TotalAmount, "idx_receipts_amount");

            entity.HasIndex(e => e.CategoryId, "idx_receipts_category");

            entity.HasIndex(e => e.CompanyId, "idx_receipts_company");

            entity.HasIndex(e => new { e.CompanyId, e.ReceiptDate, e.Status }, "idx_receipts_company_date_status").IsDescending(false, true, false);

            entity.HasIndex(e => e.CreatedAt, "idx_receipts_created").IsDescending();

            entity.HasIndex(e => e.ReceiptDate, "idx_receipts_date").IsDescending();

            entity.HasIndex(e => new { e.VendorName, e.TotalAmount, e.ReceiptDate }, "idx_receipts_duplicate_check").HasFilter("(is_duplicate = false)");

            entity.HasIndex(e => e.ExtractedData, "idx_receipts_extracted_data").HasMethod("gin");

            entity.HasIndex(e => e.Status, "idx_receipts_status");

            entity.HasIndex(e => e.Tags, "idx_receipts_tags").HasMethod("gin");

            entity.HasIndex(e => e.UserId, "idx_receipts_user");

            entity.HasIndex(e => e.VendorId, "idx_receipts_vendor");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(3, 2)
                .HasComment("Nivel de confianza de la IA (0-1). <0.7 requiere revisión manual")
                .HasColumnName("confidence_score");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'USD'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.ExtractedData)
                .HasComment("JSON con todos los campos extraídos: items, subtotales, métodos de pago, etc.")
                .HasColumnType("jsonb")
                .HasColumnName("extracted_data");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.FlagReason).HasColumnName("flag_reason");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsAiProcessed)
                .HasDefaultValue(false)
                .HasColumnName("is_ai_processed");
            entity.Property(e => e.IsDuplicate)
                .HasDefaultValue(false)
                .HasColumnName("is_duplicate");
            entity.Property(e => e.IsFlagged)
                .HasDefaultValue(false)
                .HasColumnName("is_flagged");
            entity.Property(e => e.MimeType)
                .HasMaxLength(50)
                .HasColumnName("mime_type");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OriginalFilename)
                .HasMaxLength(255)
                .HasColumnName("original_filename");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.ProcessingStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'uploaded'::character varying")
                .HasColumnName("processing_status");
            entity.Property(e => e.ReceiptDate).HasColumnName("receipt_date");
            entity.Property(e => e.ReceiptNumber)
                .HasMaxLength(100)
                .HasColumnName("receipt_number");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Tags)
                .HasComment("Tags personalizados para organización flexible")
                .HasColumnName("tags");
            entity.Property(e => e.TaxAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax_amount");
            entity.Property(e => e.TipAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tip_amount");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.VendorName)
                .HasMaxLength(255)
                .HasColumnName("vendor_name");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ReceiptApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("receipts_approved_by_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("receipts_category_id_fkey");

            entity.HasOne(d => d.Company).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("receipts_company_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ReceiptUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("receipts_user_id_fkey");

            entity.HasOne(d => d.Vendor).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("receipts_vendor_id_fkey");
        });

        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("receipt_items_pkey");

            entity.ToTable("receipt_items", tb => tb.HasComment("Items individuales extraídos de cada recibo para análisis detallado"));

            entity.HasIndex(e => e.CategoryId, "idx_receipt_items_category");

            entity.HasIndex(e => e.ReceiptId, "idx_receipt_items_receipt");

            entity.HasIndex(e => new { e.ReceiptId, e.ItemNumber }, "receipt_items_receipt_id_item_number_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ItemNumber).HasColumnName("item_number");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("1")
                .HasColumnName("quantity");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(12, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Category).WithMany(p => p.ReceiptItems)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("receipt_items_category_id_fkey");

            entity.HasOne(d => d.Receipt).WithMany(p => p.ReceiptItems)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("receipt_items_receipt_id_fkey");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("system_settings_pkey");

            entity.ToTable("system_settings", tb => tb.HasComment("Configuraciones globales del sistema (límites de API, features flags, etc.)"));

            entity.Property(e => e.Key)
                .HasMaxLength(100)
                .HasColumnName("key");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Value)
                .HasColumnType("jsonb")
                .HasColumnName("value");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SystemSettings)
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("system_settings_updated_by_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", tb => tb.HasComment("Usuarios de la plataforma con roles y permisos"));

            entity.HasIndex(e => e.IsActive, "idx_users_active").HasFilter("(is_active = true)");

            entity.HasIndex(e => e.CompanyId, "idx_users_company");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Role, "idx_users_role");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .HasColumnName("department");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("email_verified");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasComment("admin: control total, manager: aprueba gastos, employee: solo sube recibos")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany(p => p.Users)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("users_company_id_fkey");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vendors_pkey");

            entity.ToTable("vendors", tb => tb.HasComment("Proveedores/comercios extraídos automáticamente de recibos"));

            entity.HasIndex(e => e.CategoryId, "idx_vendors_category");

            entity.HasIndex(e => e.CompanyId, "idx_vendors_company");

            entity.HasIndex(e => e.NormalizedName, "idx_vendors_normalized_name");

            entity.HasIndex(e => new { e.CompanyId, e.NormalizedName }, "vendors_company_id_normalized_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NormalizedName)
                .HasMaxLength(255)
                .HasComment("Normalizado para evitar duplicados (UPPER, sin espacios extra)")
                .HasColumnName("normalized_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.TaxId)
                .HasMaxLength(50)
                .HasColumnName("tax_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Website)
                .HasMaxLength(500)
                .HasColumnName("website");

            entity.HasOne(d => d.Category).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("vendors_category_id_fkey");

            entity.HasOne(d => d.Company).WithMany(p => p.Vendors)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("vendors_company_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
