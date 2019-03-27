using System;
using FluentValidation;

namespace RCReactiveFluentValidation.Validation
{
    public class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("You must enter a name");
            RuleFor(p => p.Name).MaximumLength(10).WithMessage("Name cannot be longer than 10 characters");
            RuleFor(p => p.Age).NotEmpty().WithMessage("Age must be greater than 0");
            RuleFor(p => p.Age).LessThan(150).WithMessage("Age cannot be greater than 150");
            RuleFor(p => p.Age).NotEqual(600).WithMessage("Age cannot be 600!");
            RuleFor(p => p.EmailAddress).NotEmpty().WithMessage("You must enter a email address");
            RuleFor(p => p.EmailAddress).EmailAddress().WithMessage("You must provide a valid email address");

            RuleFor(p => p).Must(NotBeAlex33years).WithMessage("Person Alex who is 33 years old already exists");
        }

        private bool NotBeAlex33years(Person arg)
        {
            return !(arg.Name == "alex" && arg.Age == 33);
        }
    }
}
