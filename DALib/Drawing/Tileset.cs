using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DALib.Data;

namespace DALib.Drawing
{
    public class Tileset : IEnumerable<Tile>
    {
        public const int TileWidth = 56;
        public const int TileHeight = 27;
        public const int TileSize = TileWidth * TileHeight;

        private readonly List<Tile> _tiles = new List<Tile>();

        public Tile this[int index]
        {
            get { return _tiles[index]; }
            set { _tiles[index] = value; }
        }

        public int TileCount { get; private set; }

        public Tileset(DataFileEntry entry)
        {
            using (var stream = entry.Open())
            {
                Init(stream);
            }
        }
        public Tileset(Stream stream)
        {
            Init(stream);
        }

        private void Init(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                TileCount = (int)(reader.BaseStream.Length / TileSize);
                for (var i = 0; i < TileCount; i++)
                {
                    var tileData = reader.ReadBytes(TileSize);
                    _tiles.Add(new Tile(i, tileData, TileWidth, TileHeight));
                }
            }
        }

        public IEnumerator<Tile> GetEnumerator() => ((IEnumerable<Tile>) _tiles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Tile>) _tiles).GetEnumerator();
    }

    public class Tile : IRenderable
    {
        public int Id { get; }
        public int Width { get; }
        public int Height { get; }
        public int Top => 0;
        public int Left => 0;
        public byte[] Data { get; }
        public int PaletteId { get; set; }
        
        public Tile(int id, byte[] data, int width, int height)
        {
            Id = id;
            Data = data;
            Width = width;
            Height = height;
        }
    }
}
