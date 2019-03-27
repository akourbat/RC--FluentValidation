using FluentValidation;
using FluentValidation.Internal;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq;
using System.Reflection;
using System.Reactive.Linq;

namespace RCReactiveFluentValidation.Validation
{
    public static class EditContextFluentValidationExtensions
    {
        public static EditContext AddFluentValidation(this EditContext editContext, IValidator modelValidator=null)
        {
            if (editContext == null)
                throw new ArgumentNullException(nameof(editContext));

            var validator = modelValidator ?? GetValidatorForModel(editContext.Model);
            var messages = new ValidationMessageStore(editContext);

            // This subscription fires when Submit button is pressed
            Observable.FromEventPattern<ValidationRequestedEventArgs>(
                handler => editContext.OnValidationRequested += handler,
                handler => editContext.OnValidationRequested -= handler)
                .Subscribe( e =>
               {
                   messages.Clear();
                   var validationResults = validator.Validate(editContext.Model);
                   foreach (var validationResult in validationResults.Errors)
                   {
                       messages.Add(editContext.Field(validationResult.PropertyName), validationResult.ErrorMessage);
                   }
                   editContext.NotifyValidationStateChanged();
               });

            // This subscription fires every time an input field loses a focus
            Observable.FromEventPattern<FieldChangedEventArgs>(
                handler => editContext.OnFieldChanged += handler,
                handler => editContext.OnFieldChanged -= handler)
                .Subscribe(e =>
               {
                   var properties = new[] { e.EventArgs.FieldIdentifier.FieldName };
                   var context = new ValidationContext(e.EventArgs.FieldIdentifier.Model, new PropertyChain(), new MemberNameValidatorSelector(properties));

                   var validationResults = validator.Validate(context);

                   messages.Clear(e.EventArgs.FieldIdentifier);
                   messages.AddRange(e.EventArgs.FieldIdentifier, validationResults.Errors.Select(error => error.ErrorMessage));

                   editContext.NotifyValidationStateChanged();
               });
            return editContext;
        }

        // If validator instance is not provided, get it via reflection
        private static IValidator GetValidatorForModel(object model)
        {
            var abstractValidatorType = typeof(AbstractValidator<>).MakeGenericType(model.GetType());
            var modelValidatorType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.IsSubclassOf(abstractValidatorType));
            var modelValidatorInstance = (IValidator)Activator.CreateInstance(modelValidatorType);

            return modelValidatorInstance;
        }
    }
}
