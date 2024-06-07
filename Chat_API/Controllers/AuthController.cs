using Core.DTO.User;
using Core.Entities.Identity;
using Core.Interfaces;
using Core.Specifications;
using Core.Validation.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chat_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserService _userService;


        public AuthController(UserManager<UserEntity> userManager, IJwtTokenService jwtTokenService, IUserService userService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (user.EmailConfirmed == false)
            {
                return Unauthorized("Confirm the email.");
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized("Invalid password.");
            }

            var token = await _jwtTokenService.CreateToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] CreateUserDTO userDto)
        {
            var validator = new CreateUserDTOValidator();
            var validationResult = await validator.ValidateAsync(userDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.Errors.FirstOrDefault()?.ErrorMessage });
            }

            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already in use" });
            }

            var result = await _userService.CreatAsync(userDto);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to create user" });
            }

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromForm] string userId, [FromForm] string token)
        {
            var result = await _userService.ConfirmEmailAsync(userId, token);
            if (result.Succeeded)
            {
                return Ok(new { message = "ConfirmEmail successful" });
            }

            return BadRequest(new { message = "Email not confirmed", errors = result.Errors });
        }



    }
}
