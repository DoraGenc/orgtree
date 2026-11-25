using System.Text.RegularExpressions;

namespace OrgTree.Core;

public partial class Department
{
    private string? _colorHex;

    public int Id { get; set; }

    public required string Name { get; set; }

    public int? LeaderId { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? ColorHex
    {
        get => _colorHex;
        set
        {
            if (!IsValidColorHex(value))
            {
                throw new ArgumentException(
                    $"'{value}' ist keine gültige Hex-Farbe. Erwartet: #RGB, #RRGGBB oder #AARRGGBB.",
                    nameof(value));
            }

            _colorHex = value;
        }
    }

    public static bool IsValidColorHex(string? value)
    {
        return value is null || HexColorPattern().IsMatch(value);
    }

    [GeneratedRegex("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorPattern();
}
