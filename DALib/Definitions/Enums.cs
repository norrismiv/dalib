namespace DALib.Definitions;

/// <summary>
///     A value representing the endianness to use when reading/writing data
/// </summary>
public enum Endianness
{
    /// <summary>
    ///     Little endian is a term used to describe the byte order of multi-byte data types in computer memory.
    ///     In little endian, the least significant byte (LSB) is stored at the lowest memory address,
    ///     and the most significant byte (MSB) is stored at the highest memory address.
    ///     In other words, the bytes are ordered from the smallest to the largest, based on their significance.
    /// </summary>
    LittleEndian,

    /// <summary>
    ///     Big endian is a term used to describe the byte order of multi-byte data types in computer memory.
    ///     In big endian, the most significant byte (MSB) is stored at the lowest memory address,
    ///     and the least significant byte (LSB) is stored at the highest memory address.
    ///     In other words, the bytes are ordered from the largest to the smallest, based on their significance.
    /// </summary>
    BigEndian
}

/// <summary>
///     Represents the different types of alpha blending used by EFA files
/// </summary>
public enum EfaBlendingType : byte
{
    /// <summary>
    ///     Transparency is added by the client based on the luminance of the pixel
    /// </summary>
    Luminance = 1,

    /// <summary>
    ///     Transparency is added by the client based on the luminance of the pixel. Slightly less transparent than "Luminance"
    /// </summary>
    LessLuminance = 2,

    /// <summary>
    ///     Transparency is added by the client through some mechanism. Appears to be flood fill, but it only seems to work on
    ///     like 3 effects. Other effects that try to use this option will not render at all.
    /// </summary>
    NotSure = 3
}

/// <summary>
///     Represents the different types of MPF headers
/// </summary>
public enum MpfHeaderType
{
    /// <summary>
    ///     Not sure what this header is actually for, but it indicates that there are 4 extra bytes in the header
    /// </summary>
    Unknown = -1,

    /// <summary>
    ///     Indicates that there is no header
    /// </summary>
    None = 0
}

/// <summary>
///     Represents the different types of MPF formats
/// </summary>
public enum MpfFormatType
{
    /// <summary>
    ///     Indicates the MPF will contain multiple attack animations
    /// </summary>
    MultipleAttacks = -1,

    /// <summary>
    ///     Indicates the MPF will contain only a single attack animation
    /// </summary>
    SingleAttack = 0
}

/// <summary>
///     Represents the different types of SPF formats
/// </summary>
public enum SpfFormatType
{
    /// <summary>
    ///     Indicates the SPF will contain RGB565 and RGB555 palettes, and frame data will contain palette indexes (1 byte per
    ///     pixel)
    /// </summary>
    Palettized = 0,

    /// <summary>
    ///     Indicates the SPF will contain no palette, and frame data will be colors (2 bytes per pixel). Each frame will have
    ///     2 sets of colorized data, the first being RGB565, the second being RGB555.
    /// </summary>
    Colorized = 2
}

/// <summary>
///     Represents the type of control.
/// </summary>
public enum ControlType
{
    /// <summary>
    ///     A control with this type will represent the full bounds of the ControlFile. All other controls will be within the
    ///     rect of this control.
    /// </summary>
    Anchor = 0,

    /// <summary>
    ///     A control with this type will return a value
    /// </summary>
    ReturnsValue = 3,

    /// <summary>
    ///     A control with this type will always return 0
    /// </summary>
    Returns0 = 4,

    /// <summary>
    ///     A control with this type will never return a value, and will have un-editable text in it
    /// </summary>
    ReadonlyText = 5,

    /// <summary>
    ///     A control with this type will never return a value, and will have editable text in it
    /// </summary>
    EditableText = 6,

    /// <summary>
    ///     A control with this type will never return a value
    /// </summary>
    DoesNotReturnValue = 7
}

/// <summary>
///     Represents the type of override for a palette. When working with the KHAN archives, some items will use different
///     palettes depending on the gender they are for.
/// </summary>
public enum KhanPalOverrideType
{
    /// <summary>
    ///     Indicates that Male overrides should be used, if present
    /// </summary>
    Male = -1,

    /// <summary>
    ///     Indicates that Female overrides should be used, if present
    /// </summary>
    Female = -2,

    /// <summary>
    ///     Indicates normal overrides should be used, if present
    /// </summary>
    None = 0
}