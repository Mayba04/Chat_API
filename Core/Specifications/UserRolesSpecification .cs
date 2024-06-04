using Ardalis.Specification;
using Core.DTO.User;
using Core.Entities.Identity;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications
{
    public class UserRolesSpecification : Specification<UserRoleEntity>
    {
        public class GetByUserRoleUserId : Specification<UserRoleEntity>
        {
            public GetByUserRoleUserId(int userid)
            {
                Query.Where(a => a.UserId == userid);
            }
        }

    }
}
