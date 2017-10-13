using System.IO;

namespace DALib.Data
{
    public class MapFile
    {
        private MapTile[,] _tiles;

        public MapFile(Stream stream, int width, int height)
        {
            Width = width;
            Height = height;
            Init(stream);
        }

        public int Width { get; }

        public int Height { get; }

        public MapTile this[int x, int y] => _tiles[x, y];

        private void Init(Stream stream)
        {
            _tiles = new MapTile[Width, Height];

            using (var reader = new BinaryReader(stream))
            {
                stream.Seek(0, SeekOrigin.Begin);

                for (var y = 0; y < Height; ++y)
                {
                    for (var x = 0; x < Width; ++x)
                    {
                        var groundTile = reader.ReadByte() << 8 | reader.ReadByte();
                        var leftStaticObjectTile = reader.ReadByte() << 8 | reader.ReadByte();
                        var rightStaticObjectTile = reader.ReadByte() << 8 | reader.ReadByte();
                        _tiles[x, y] = new MapTile(groundTile, leftStaticObjectTile, rightStaticObjectTile);
                    }
                }
            }
        }
    }

    public class MapTile
    {
        public MapTile(int groundTile, int leftStaticObjectTile, int rightStaticObjectTile)
        {
            GroundTile = groundTile;
            LeftStaticObjectTile = leftStaticObjectTile;
            RightStaticObjectTile = rightStaticObjectTile;
        }

        public int GroundTile { get; }

        public int LeftStaticObjectTile { get; }

        public int RightStaticObjectTile { get; }
    }
}
