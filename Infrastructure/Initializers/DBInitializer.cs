﻿using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Initializers
{
    internal static class DBInitializer
    {
        public static void SeedRoles(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleEntity>().HasData(
                new RoleEntity { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new RoleEntity { Id = 2, Name = "User", NormalizedName = "USER" }
            );
        }

        public static void SeedUsers(this ModelBuilder modelBuilder)
        {
            var hasher = new PasswordHasher<UserEntity>();

            var adminUser = new UserEntity
            {
                Id = 1,
                UserName = "admin",
                FirstName = "Pavlo",
                LastName = "Mayba",
                NormalizedUserName = "Pavlo Mayba",
                PhoneNumber = "0987654321",
                Email = "admin@email.com"
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "123456");

            var normalUser = new UserEntity
            {
                Id = 2,
                UserName = "user@email.com",
                FirstName = "Oleg",
                LastName = "Dobrov",
                NormalizedUserName = "Oleg Dobrov",
                PhoneNumber = "1234567890",
                Email = "user@email.com"
            };
            normalUser.PasswordHash = hasher.HashPassword(normalUser, "123456");
            modelBuilder.Entity<UserEntity>().HasData(adminUser, normalUser);
        }

        public static void SeedUserRoles(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRoleEntity>().HasData(
                new UserRoleEntity { UserId = 1, RoleId = 1, },
                new UserRoleEntity {  UserId = 2, RoleId = 2 }
            );
        }
    }
}