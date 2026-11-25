using PhoneNumbers;

namespace OrgTree.Core;

public class Employee
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    private string? _phoneNumber;

    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public int? DepartmentId { get; set; }

    public string? PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (!TryFormatPhoneNumber(value, out var formatted))
            {
                throw new ArgumentException(
                    $"'{value}' ist keine gültige Telefonnummer. Erwartet: internationales Format, z. B. +49 172 12345678.",
                    nameof(value));
            }

            _phoneNumber = formatted;
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public string DisplayName => $"{LastName}, {FirstName}";

    public static bool IsValidPhoneNumber(string? input)
    {
        return TryFormatPhoneNumber(input, out _);
    }

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
}
