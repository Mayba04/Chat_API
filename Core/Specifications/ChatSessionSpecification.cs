using Ardalis.Specification;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Core.Specifications
{
    public class ChatSessionSpecification : Specification<ChatSession>
    {
        public class GetLastChatSession : Specification<ChatSession>
        {
            public GetLastChatSession()
            {
                Query.OrderByDescending(a => a.Id);
            }
        }

        public class GetChatSesionUserId : Specification<ChatSession>
        {
            public GetChatSesionUserId(int userId)
            {
                Query.Where(a => a.UserId == userId );
            }
        }

    }
}
