using System.ComponentModel.DataAnnotations;

namespace latest.Validation;
public class InTheFuture : ValidationAttribute {
    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        DateTime d = Convert.ToDateTime(value);
        return d >= DateTime.Now;
    }
}