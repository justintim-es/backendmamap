
using System.ComponentModel.DataAnnotations;

namespace latest.Validation;

public class NotToday : ValidationAttribute {
    public override bool IsValid(object? value)
    {
        DateTime d = Convert.ToDateTime(value);
        return d <= DateTime.Now;
    }
}