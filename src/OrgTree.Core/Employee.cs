using PhoneNumbers;

namespace OrgTree.Core;

public class Employee
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public static bool TryFormatPhoneNumber(string? input, out string? formatted)
    {
        formatted = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return true;
        }

        try
        {
            var number = PhoneUtil.Parse(input, null);
            if (!PhoneUtil.IsValidNumber(number))
            {
                return false;
            }

            formatted = PhoneUtil.Format(number, PhoneNumberFormat.INTERNATIONAL);
            return true;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumber(string? input)
    {
        return TryFormatPhoneNumber(input, out _);
    }

    public int Id { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
    public int? DepartmentId { get; set; }

    private string? _phoneNumber;

    public string? PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (!TryFormatPhoneNumber(value, out var formatted))
            {
                throw new ArgumentException(
                    $"'{value}' ist keine gueltige Telefonnummer. Erwartet: internationales Format, z. B. +49 172 12345678.",
                    nameof(value));
            }

            _phoneNumber = formatted;
        }
    }
}
