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

        public class GetPendingVerificationSessions : Specification<ChatSession>
        {
            public GetPendingVerificationSessions()
            {
                Query.Where(session => session.SessionVerificationByAdmin == true)
                     .Include(session => session.Messages);
            }
        }

        public class GetChatSessionsWithAdminComments : Specification<ChatSession>
        {
            public GetChatSessionsWithAdminComments()
            {
                Query.Include(cs => cs.Messages)
                     .ThenInclude(m => m.AdminCommentDetail)
                     .Where(cs => cs.Messages.Any(m => m.AdminComment != false));
            }
        }

    }
}
