using System.Text.RegularExpressions;

namespace OrgTree.Core;

public partial class Department
{
    [GeneratedRegex("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorPattern();

    public static bool IsValidColorHex(string? value)
    {
        return value is null || HexColorPattern().IsMatch(value);
    }

    public int Id { get; set; }
    required public string Name { get; set; }
    public int? LeaderId { get; set; }
    public int? ParentDepartmentId { get; set; }

    private string? _colorHex;

    public string? ColorHex
    {
        get => _colorHex;
        set
        {
            if (!IsValidColorHex(value))
            {
                throw new ArgumentException(
                    $"'{value}' ist keine gueltige Hex-Farbe. Erwartet: #RGB, #RRGGBB oder #AARRGGBB.",
                    nameof(value));
            }

            _colorHex = value;
        }
    }
}
