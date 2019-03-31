using Prism.Events;

namespace RCReactiveFluentValidation.Validation
{
    public class ValidationEventArgs
    {
        public bool Valid { get; private set; }
        public string Model { get; private set; }

        public ValidationEventArgs(bool valid, string model)
        {
            Valid = valid;
            Model = model;
        }
    }

    public class ValidationEvent : PubSubEvent<ValidationEventArgs> {}
}
