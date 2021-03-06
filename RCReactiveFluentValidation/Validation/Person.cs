﻿using PropertyChanged;

namespace RCReactiveFluentValidation.Validation
{
    [AddINotifyPropertyChangedInterface]
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string EmailAddress { get; set; }
    }
}
