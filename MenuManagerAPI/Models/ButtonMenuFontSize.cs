//
using System.ComponentModel;

// Declare namespace
namespace MenuManagerAPI.Models
{
    // Define enum
    public enum ButtonMenuFontSize
    {
        // Name = EstimatedPixelHeight
        // Description = CSS_Class_String

        [Description("fontSize-xs")]
        XS = 8,

        [Description("fontSize-s")]
        S = 12,

        [Description("fontSize-sm")]
        SM = 16,

        [Description("fontSize-m")]
        M = 18,

        [Description("fontSize-ml")]
        ML = 20,

        [Description("fontSize-l")]
        L = 24
    }
}