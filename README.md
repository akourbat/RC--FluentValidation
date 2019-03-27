# RC--FluentValidation
Small example of using Razor Components with FluentValidation


Requires at least VS 2019 preview 3 and asp.net core 3 preview 3

Based on article https://chrissainty.com/using-fluentvalidation-for-forms-validation-in-razor-components/ by Chris Sainty and his repo. I used `Observable.FromEventPattern<>` in this example rather than hooking up directly to event handlers.  I also added an optional IValidator parameter to `FluentValidationValidator` to be able to avoid a refelection call.

![](https://github.com/akourbat/RC--FluentValidation/blob/master/Validation.gif)
