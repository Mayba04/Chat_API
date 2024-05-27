using Core.Entities;
using Core.Entities.Identity;
using Infrastructure.Initializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Context
{
    public class AppDbContext : IdentityDbContext<UserEntity, RoleEntity, int,
        IdentityUserClaim<int>, UserRoleEntity, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public AppDbContext() : base() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RoleEntity> RoleEntity { get; set; }
        public DbSet<UserEntity> UserEntity { get; set; }
        public DbSet<UserRoleEntity> UserRoleEntity { get; set; }
        public DbSet<ChatSession> ChatSession { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<AdminComment> AdminComment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.SeedRoles();
            modelBuilder.SeedUsers();
            modelBuilder.SeedUserRoles();

            modelBuilder.Entity<UserEntity>()
           .Property(u => u.EmailConfirmed)
           .HasColumnType("boolean");

            modelBuilder.Entity<UserRoleEntity>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRoleEntity>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<Message>()
            .HasOne(m => m.AdminCommentDetail)
            .WithOne(ac => ac.Message)
            .HasForeignKey<AdminComment>(ac => ac.MessageId);
             
            modelBuilder.Entity<ChatSession>()
           .Property(s => s.SessionVerificationByAdmin)
           .HasDefaultValue(true);
        }


    }
}
