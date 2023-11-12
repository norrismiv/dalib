namespace DALib.Drawing;

public sealed class PaletteTableEntry
{
    public int MaxTileNumber { get; }
    public int MinTileNumber { get; }
    public int PaletteNumber { get; }

    public PaletteTableEntry(int minTileNumber, int maxTileNumber, int paletteNumber)
    {
        MinTileNumber = minTileNumber;
        MaxTileNumber = maxTileNumber;
        PaletteNumber = paletteNumber;
    }
}