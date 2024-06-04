using Ardalis.Specification;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications
{
    public class RolesSpecification : Specification<RoleEntity>
    {
        public class GetRolesByName : Specification<RoleEntity>
        {
            public GetRolesByName(string RolesName)
            {
                Query.Where(a => a.Name == RolesName);
            }
        }
    }
}
