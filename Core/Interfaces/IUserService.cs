using Core.DTO.User;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUserService
    {
        Task<UserEntity> CreateUserAsync(CreateUserDTO userDto);
        Task<IdentityResult> CreatAsync(CreateUserDTO userDto);
        Task<UserDTO> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task UpdateUserAsync(UpdateUserDTO userDto);
        Task DeleteUserAsync(int userId);
        Task ChangePasswordInfo(EditUserPasswordDTO passwordDto);
    }
}
