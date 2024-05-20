using Ardalis.Specification;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications
{
    public class UserSpecification : Specification<UserEntity>
    {
        public class GetLastUser : Specification<UserEntity>
        {
            public GetLastUser()
            {
                Query.OrderByDescending(a=>a.Id);
            }
        }

       
    }
}
