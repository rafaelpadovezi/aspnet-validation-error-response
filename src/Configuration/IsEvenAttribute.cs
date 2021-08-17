using System;
using System.ComponentModel.DataAnnotations;

namespace ExampleApi.Configuration
{
    public class IsEvenAttribute : ValidationAttribute
    {
        public IsEvenAttribute() : base ("Value is not an even number")
        {
        }

        public override bool IsValid(object value)
        {
            var intValue = Convert.ToInt32(value);
            return intValue % 2 == 0;
        }
    }
}