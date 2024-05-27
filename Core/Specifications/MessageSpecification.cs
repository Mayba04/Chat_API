using Ardalis.Specification;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Specifications.UserRolesSpecification;

namespace Core.Specifications
{
    public class MessageSpecification : Specification<Message>
    {
        public class GetMessagesBySessionId : Specification<Message>
        {
            public GetMessagesBySessionId(int SessionId)
            {
                Query.Where(a => a.ChatSessionId == SessionId);
                Query.OrderBy(a => a.Timestamp);
            }
        }

        public class GetPendingBotMessages : Specification<Message>
        {
            public GetPendingBotMessages()
            {
                Query.Where(m => m.Role == "bot" && !m.AdminComment);
            }
        }
    }
}
