using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finote_Web.Models.Data
{
    public class FinoteDbContext : IdentityDbContext<Users> 
    {
         public FinoteDbContext(DbContextOptions<FinoteDbContext> options) : base(options)
        {

        }
        public DbSet<UserInfomation> UserInfomations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<UserWalletParticipant> UserWalletParticipants { get; set; }
        public DbSet<WalletRole> WalletRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<AiLog> AiLogs { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // === Identity default tables rename ===
            builder.Entity<Users>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // === Categories ===
            builder.Entity<Category>()
                .Property(c => c.CategoryName)
                .HasColumnType("nvarchar(256)");
            builder.Entity<Category>()
                .HasOne(c => c.TransactionType)
                .WithMany(nt => nt.Categories)
                .HasForeignKey(c => c.TransactionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Currency ===
            builder.Entity<Currency>()
                .Property(c => c.CurrencyName)
                .HasColumnType("nvarchar(128)");

            // === Notifications ===
            builder.Entity<Notification>()
                .Property(n => n.Title)
                .HasColumnType("nvarchar(256)");
            builder.Entity<Notification>()
                .Property(n => n.Message)
                .HasColumnType("nvarchar(max)");
            builder.Entity<Notification>()
                .HasOne(n => n.NotificationType)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // === NotificationType ===
            builder.Entity<NotificationType>()
                .Property(nt => nt.TypeName)
                .HasColumnType("nvarchar(256)");
            builder.Entity<NotificationType>()
                .Property(nt => nt.Description)
                .HasColumnType("nvarchar(256)");
            builder.Entity<NotificationType>()
                .Property(nt => nt.IconURL)
                .HasColumnType("varchar(128)");

            // === Permissions ===
            builder.Entity<Permission>()
                .Property(p => p.PermissionName)
                .HasColumnType("nvarchar(128)");
            builder.Entity<Permission>()
                .Property(p => p.Description)
                .HasColumnType("nvarchar(256)");

            // === RolePermission (composite key) ===
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.WalletRoleId, rp.PermissionId });
            builder.Entity<RolePermission>()
                .HasOne(rp => rp.WalletRole)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.WalletRoleId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Transactions ===
            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Transaction>()
                .Property(t => t.Note)
                .HasColumnType("nvarchar(256)");
            builder.Entity<Transaction>()
                .Property(t => t.IsDeleted)
                .HasDefaultValue(false);
            builder.Entity<Transaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Transaction>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            builder.Entity<Transaction>()
                .HasOne(t => t.DeletedByUser)
                .WithMany(u => u.DeletedTransactions)
                .HasForeignKey(t => t.DeletedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // === NoteTypes ===
            builder.Entity<TransactionType>()
                .Property(nt => nt.TransactionTypeName)
                .HasColumnType("nvarchar(128)");
            builder.Entity<TransactionType>()
                .Property(nt => nt.Description)
                .HasColumnType("nvarchar(256)");

            // === UserInfomation (1-1 with Users) ===
            builder.Entity<UserInfomation>()
                .Property(ui => ui.FullName)
                .HasColumnType("nvarchar(256)");
            builder.Entity<UserInfomation>()
                .Property(ui => ui.Gender)
                .HasColumnType("nvarchar(100)");
            builder.Entity<UserInfomation>()
                .Property(ui => ui.AvatarUrl)
                .HasColumnType("varchar(128)");
            builder.Entity<UserInfomation>()
                .HasOne(ui => ui.User)
                .WithOne(u => u.UserInfomation)
                .HasForeignKey<UserInfomation>(ui => ui.UserInfomationId)
                .OnDelete(DeleteBehavior.Cascade);

            // === UserNotification (many-to-many link table) ===
            builder.Entity<UserNotification>()
                .Property(un => un.IsRead)
                .HasDefaultValue(false);
            builder.Entity<UserNotification>()
                .Property(un => un.IsDeleted)
                .HasDefaultValue(false);
            builder.Entity<UserNotification>()
                .HasOne(un => un.User)
                .WithMany(u => u.UserNotifications)
                .HasForeignKey(un => un.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<UserNotification>()
                .HasOne(un => un.Notification)
                .WithMany(n => n.UserNotifications)
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // === UserWalletParticipant (composite key) ===
            builder.Entity<UserWalletParticipant>()
                .HasKey(uwp => new { uwp.WalletId, uwp.UserId });
            builder.Entity<UserWalletParticipant>()
                .Property(uwp => uwp.AllowNotification)
                .HasDefaultValue(true);
            builder.Entity<UserWalletParticipant>()
                .HasOne(uwp => uwp.User)
                .WithMany(u => u.UserWalletParticipants)
                .HasForeignKey(uwp => uwp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<UserWalletParticipant>()
                .HasOne(uwp => uwp.Wallet)
                .WithMany(w => w.UserWalletParticipants)
                .HasForeignKey(uwp => uwp.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserWalletParticipant>()
                .HasOne(uwp => uwp.WalletRole)
                .WithMany(r => r.UserWalletParticipants)
                .HasForeignKey(uwp => uwp.WalletRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Wallets ===
            builder.Entity<Wallet>()
                .Property(w => w.WalletName)
                .HasColumnType("nvarchar(128)");
            builder.Entity<Wallet>()
                .Property(w => w.Description)
                .HasColumnType("nvarchar(128)");
            builder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Wallet>()
                .Property(w => w.IsDeleted)
                .HasDefaultValue(false);
            builder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Wallet>()
                .HasOne(w => w.Currency)
                .WithMany(c => c.Wallets)
                .HasForeignKey(w => w.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // === WalletRoles ===
            builder.Entity<WalletRole>()
                .Property(r => r.WalletRoleName)
                .HasColumnType("nvarchar(128)");
            builder.Entity<ActivityLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs) // We need to add this collection to the Users class
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, their logs are also deleted.
            builder.Entity<AiLog>(entity =>
            {
                // Define the relationship: An AiLog has one User.
                entity.HasOne(al => al.User)
                    .WithMany() // Assuming you don't need a collection of AiLogs on the User model
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, their AI logs are also deleted.
            });

            
            builder.Entity<SystemSetting>().HasKey(s => s.SettingKey); // Define the primary key

        }
    }
}
