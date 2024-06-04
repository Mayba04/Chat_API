using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.User
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public string RoleName { get; set; } = string.Empty ;
        public int Role { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}
