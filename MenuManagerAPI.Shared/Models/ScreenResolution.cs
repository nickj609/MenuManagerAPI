// Included libraries
using System.ComponentModel;

// Declare namespace
namespace MenuManagerAPI.Shared.Models
{
    // Define enum
    public enum ScreenResolution
    {
        [Description("1920x1080 (16:9)")]
        R1920x1080,

        [Description("1600x900 (16:9)")]
        R1600x900, 

        [Description("1440x1080 (4:3)")]
        R1440x1080, 

        [Description("1280x960 (4:3)")]
        R1280x960, 

        [Description("1280x1024 (5:4)")]
        R1280x1024
    }
}