using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace RCReactiveFluentValidation.Validation
{
    public class FluentValidationValidator : ComponentBase, IDisposable
    {
        [CascadingParameter] EditContext CurrentEditContext { get; set; }
        [Parameter] public IValidator Validator { get; set; }
        [Parameter] public EventCallback<bool> OnValidation { get; set; }

        private IDisposable formValidationSub;
        private IDisposable fieldValidationSub;

        protected override void OnInit()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{nameof(FluentValidationValidator)} requires a cascading " +
                    $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(FluentValidationValidator)} " +
                    $"inside an {nameof(EditForm)}.");
            }

            var validator = Validator ?? GetValidatorForModel(CurrentEditContext.Model);

            if (validator == null)
            {
                throw new InvalidOperationException($"{nameof(FluentValidationValidator)} requires either a " +
                    $"parameter of type {nameof(IValidator)}, or validator for type {nameof(CurrentEditContext.Model)} " +
                    $"should be reachable via reflection from the calling assembly.");
            }

            var messages = new ValidationMessageStore(CurrentEditContext);

            // This field collects errors that are model-related but not specific to concrete property on the model
            var modelErrorField = new FieldIdentifier(CurrentEditContext.Model, "");

            // This subscription fires when Submit button is pressed, 
            // acts as a guard against not validated still-focused field
            formValidationSub = Observable.FromEventPattern<ValidationRequestedEventArgs>(
                handler => CurrentEditContext.OnValidationRequested += handler,
                handler => CurrentEditContext.OnValidationRequested -= handler)
                .Subscribe(e =>
                {
                    messages.Clear();
                    var validationResults = validator.Validate(CurrentEditContext.Model);

                    if (validationResults.IsValid)
                        OnValidation.InvokeAsync(true);
                    else
                        OnValidation.InvokeAsync(false);

                    foreach (var validationResult in validationResults.Errors)
                    {
                        messages.Add(CurrentEditContext.Field(validationResult.PropertyName), validationResult.ErrorMessage);
                    }
                    CurrentEditContext.NotifyValidationStateChanged();
                });

            // This subscription fires every time an input field loses a focus, main workhorse
            fieldValidationSub = Observable.FromEventPattern<FieldChangedEventArgs>(
                handler => CurrentEditContext.OnFieldChanged += handler,
                handler => CurrentEditContext.OnFieldChanged -= handler)
                .Subscribe(e =>
                {
                    var validationResults = validator.Validate(CurrentEditContext.Model);

                    if (validationResults.IsValid)
                        OnValidation.InvokeAsync(true);
                    else
                        OnValidation.InvokeAsync(false);

                    messages.Clear(e.EventArgs.FieldIdentifier);
                    messages.AddRange(e.EventArgs.FieldIdentifier, validationResults.Errors
                        .Where(f => f.PropertyName == e.EventArgs.FieldIdentifier.FieldName)
                        .Select(error => error.ErrorMessage));

                    // clear errors that are not specific to field, e.g. complex rules
                    messages.Clear(modelErrorField);
                    messages.AddRange(modelErrorField, validationResults.Errors
                        .Where(f => f.PropertyName == "")
                        .Select(error => error.ErrorMessage));

                    CurrentEditContext.NotifyValidationStateChanged();
                });
        }
        // If validator instance is not provided, get it via reflection
        private static IValidator GetValidatorForModel(object model)
        {
            var abstractValidatorType = typeof(AbstractValidator<>).MakeGenericType(model.GetType());
            var modelValidatorType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.IsSubclassOf(abstractValidatorType));
            var modelValidatorInstance = (IValidator)Activator.CreateInstance(modelValidatorType);

            return modelValidatorInstance;
        }

        public void Dispose()
        {
            formValidationSub.Dispose();
            fieldValidationSub.Dispose();
        }
    }
}
