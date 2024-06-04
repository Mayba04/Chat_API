using Core.DTO.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Validation.User
{
    public class EditPasswordValidation : AbstractValidator<EditUserPasswordDTO>
    {
        public EditPasswordValidation()
        {
            RuleFor(r => r.CurrentPassword).NotEmpty().MinimumLength(6);
            RuleFor(r => r.NewPassword).NotEmpty().MinimumLength(6);
            RuleFor(r => r.ConfirmPassword).NotEmpty().MinimumLength(6).Equal(r => r.NewPassword);
        }
    }
}
