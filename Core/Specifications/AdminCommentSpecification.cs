using Ardalis.Specification;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Core.Specifications
{
    public class AdminCommentSpecification : Specification<AdminComment>
    {
        public class GetCommentsByMessageId : Specification<AdminComment>
        {
            public GetCommentsByMessageId(int messageId)
            {
                Query.Where(c => c.MessageId == messageId);
            }
        }

    }

}
