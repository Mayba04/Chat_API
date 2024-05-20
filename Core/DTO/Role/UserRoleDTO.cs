using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Role
{
    public class UserRoleDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }  
        public int RoleId { get; set; }
    }
}
