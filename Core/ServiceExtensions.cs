using Core.Entities.Identity;
using Core.Interfaces;
using Core.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class ServiceExtension
    {
        public static void AddCustomService(this IServiceCollection service)
        {
            
            service.AddScoped<IUserService, UserService>();
            service.AddTransient<IJwtTokenService, JwtTokenService>();
            service.AddScoped<IFilesService, FilesService>();
            service.AddScoped<IChatService, ChatService>();
            service.AddScoped<IMessageService, MessageService>();
            service.AddScoped<IAdminCommentService, AdminCommentService>();
            
        }
        public static void AddValidator(this IServiceCollection service)
        {
            service.AddFluentValidationAutoValidation();

            service.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        }
        public static void AddAutoMapper(this IServiceCollection service)
        {
            service.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
