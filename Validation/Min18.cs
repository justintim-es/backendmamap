
using System.ComponentModel.DataAnnotations;

namespace latest.Validation;

public class Min18Attribute : ValidationAttribute {
    public override bool IsValid(object? value)
    {
        DateTime d = Convert.ToDateTime(value);
        return d <= DateTime.Now.AddYears(-18);
    }
}