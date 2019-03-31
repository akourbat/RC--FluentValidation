using Prism.Events;

namespace RCReactiveFluentValidation.Validation
{
    public class ValidationTarget
    {
        public string Model { get; set; }
        public bool Valid { get; set; }
    }

    public class ValidationEvent : PubSubEvent<ValidationTarget> {}
}
