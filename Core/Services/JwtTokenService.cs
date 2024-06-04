using Core.DTO.User;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<UserEntity> _userManager;

        public JwtTokenService(IConfiguration config, UserManager<UserEntity> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<string> CreateToken(UserEntity user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var idroles = await _userManager.GetUsersInRoleAsync(roles.ToString());
            List<Claim> claims = new()
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("FirstName", $"{user.FirstName}"),
                new Claim("LastName", $"{user.LastName}"),
                new Claim("image", user.Image ?? string.Empty),
                new Claim("Phone", user.PhoneNumber),
            };

            foreach (var role in roles)
                claims.Add(new Claim("roles", role));

            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<String>("JwtSecretKey")));
            var signinCredentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                signingCredentials: signinCredentials,
                expires: DateTime.Now.AddDays(10),
                claims: claims
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task<UserDTO> GetTokenDes(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

            var  userDto = new UserDTO
            {
                Id = int.Parse(claims["Id"]),
                Email = claims["email"],
                FirstName = claims["FirstName"],
                LastName = claims["LastName"],
                Role = claims["roles"]
            };

            return await Task.Run(() => userDto);
        }
    }

    
}
