﻿using AutoMapper;
using Core.Constants;
using Core.DTO.Role;
using Core.DTO.User;
using Core.Entities.Identity;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    internal class UserService : IUserService
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IMapper _mapper;
        private readonly IFilesService _filesService;
        private readonly EmailService _emailService;
        private readonly IRepository<UserRoleEntity> _userroleRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IConfiguration _configuration;

        public UserService(IRepository<UserEntity> userRepository, UserManager<UserEntity> userManager, IMapper mapper, IFilesService filesService, IRepository<UserRoleEntity> userroleRepository, IRepository<RoleEntity> roleRepository, EmailService emailService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _filesService = filesService;
            _userroleRepository = userroleRepository;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }


        public async Task<IdentityResult> CreatAsync(CreateUserDTO userDto)
        {
            var newUser = _mapper.Map<UserEntity>(userDto);
            newUser.UserName = newUser.FirstName + "_" + newUser.LastName;
            if (userDto.ImageFile != null)
            {
                newUser.Image = await _filesService.SaveImage(userDto.ImageFile);
            }
            var result = await _userManager.CreateAsync(newUser, userDto.Password);

            if (!result.Succeeded)
            {
                await _filesService.DeleteImage(newUser.Image);
                return result;
            }

            var lastuser = await _userRepository.GetItemBySpec(new UserSpecification.GetLastUser());
            if (lastuser != null)
            {
                var userRole = new UserRoleEntity
                {
                    UserId = lastuser.Id,
                    RoleId = userDto.Role,
                };
                await _userroleRepository.Insert(userRole);

                await _userroleRepository.Save();
            }

            var emailResult = await SendConfirmationEmailAsync(lastuser);
            if (!emailResult)
            {
                await DeleteUserAsync(newUser.Id); 
                return IdentityResult.Failed(new IdentityError { Description = "Failed to send confirmation email." });
            }


            return IdentityResult.Success;
        }

        public async Task<UserEntity> CreateUserAsync(CreateUserDTO userDto)
        {
            var newUser = _mapper.Map<UserEntity>(userDto);

            if (userDto.ImageFile != null)
            {
                newUser.Image = await _filesService.SaveImage(userDto.ImageFile);
            }

            await _userRepository.Insert(newUser);
            await _userRepository.Save();

            return newUser;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var entity = await _userRepository.GetByID(userId);
            if (entity != null)
            {
                var userroles = await _userroleRepository.GetItemBySpec(new UserRolesSpecification.GetByUserRoleUserId(userId));
                await _userroleRepository.Delete(userroles);
                if (entity.Image != null)
                {
                    await _filesService.DeleteImage(entity.Image);
                }
                await _userRepository.Delete(entity);
                await _userRepository.Save();
            }
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                var ss = await _userroleRepository.GetAll();
                var users = _mapper.Map<List<UserDTO>>(await _userRepository.GetAll());
                var userroles = _mapper.Map<List<UserRoleDTO>>(await _userroleRepository.GetAll());
                var roles = _mapper.Map<List<RoleDTO>>(await _roleRepository.GetAll());

                for (int i = 0; i < users.Count(); i++)
                {
                    var userrole = userroles.FirstOrDefault(a => a.UserId == users[i].Id);
                    if (userrole != null)
                    {
                        var role = roles.FirstOrDefault(a => a.Id == userrole.RoleId);
                        if (role != null)
                        {
                            users[i].Role = role.Name;
                        }
                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<UserDTO>();
            }
        }

        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByID(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var userroles = await _userroleRepository.GetItemBySpec(new UserRolesSpecification.GetByUserRoleUserId(userId));
            if (userroles == null)
            {
                throw new Exception("User_roles not found");
            }

            var role = await _roleRepository.GetByID(userroles.RoleId);
            if (role == null)
            {
                throw new Exception("Role not found");
            }

            var mapped = _mapper.Map<UserDTO>(user);
            mapped.Role = role.Name;
            return mapped;
        }

        public async Task UpdateUserAsync(UpdateUserDTO userDto)
        {
            var rolesid = new RoleEntity();
            var user = await _userManager.FindByIdAsync(userDto.Id.ToString());
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Manual mapping of properties
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.UserName = userDto.FirstName + "_" + userDto.LastName;

            // Handle image update if necessary
            if (userDto.ImageFile != null)
            {
                if (user.Image != null)
                {
                    await _filesService.DeleteImage(user.Image);
                }
                user.Image = await _filesService.SaveImage(userDto.ImageFile);
            }

            // Update the user using UserManager
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                Console.WriteLine("Failed to update user");
                throw new Exception("Failed to update user");
            }
            if (userDto.Role == null || userDto.Role == 0)
            {
                rolesid = await _roleRepository.GetItemBySpec(new RolesSpecification.GetRolesByName(userDto.RoleName));
            }
            
            var userrole = await _userroleRepository.GetItemBySpec(new UserRolesSpecification.GetByUserRoleUserId(user.Id));
            if (userrole != null)
            {
                var userRole = new UserRoleEntity
                {
                    UserId = userrole.UserId,
                    RoleId = rolesid.Id,
                };
                await _userroleRepository.Delete(userrole);
                await _userroleRepository.Insert(userRole);

                await _userroleRepository.Save();
            }
        }

        public async Task ChangePasswordInfo(EditUserPasswordDTO passwordDto)
        {
            var user = await _userManager.FindByIdAsync(passwordDto.Id.ToString());
            if (user == null)
            {
                throw new Exception("User not found");
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, passwordDto.CurrentPassword, passwordDto.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception("Password failed to update");
            }
        }

        public async Task<bool> SendConfirmationEmailAsync(UserEntity user)
        {
            try
            {
                string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                byte[] encodedToken = Encoding.UTF8.GetBytes(token);
                string validEmailToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration["HostSettings:URL"]}/confirmemail?userid={user.Id}&token={validEmailToken}";

                string emailBody = $"<h1>Confirm your email</h1> <a href='{url}'>Confirm now!</a>";
                return await _emailService.SendEmailAsync(user.Email, "Email confirmation.", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendConfirmationEmailAsync: {ex.Message}");
                return false; 
            }
        }


        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            UserEntity? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            byte[] decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, normalToken);
            return result;
        }


    }
}
