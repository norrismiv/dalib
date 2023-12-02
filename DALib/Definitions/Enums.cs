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

public enum MpfHeaderType
{
    Unknown = -1,
    None = 0
}

public enum MpfFormatType
{
    MultipleAttacks = -1,
    SingleAttack = 0
}