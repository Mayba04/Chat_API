using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities.Identity
{
    public class UserRoleEntity : IdentityUserRole<int>
    {
        public int UserId { get; set; }
        public virtual UserEntity? User { get; set; }
        public int RoleId { get; set; }
        public virtual RoleEntity? Role { get; set; }
    }
}
