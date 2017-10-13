using DALib.Data;
using DALib.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ItemViewer
{
    public partial class ItemViewerForm : Form
    {
        private const int Columns = 19;
        private const int Rows = 14;
        private const int TilesPerFile = 266;

        private DataFile _legend;
        private ColorTable _colorTable;
        private PaletteTable _paletteTable;
        private List<FileSelectItem> _fileSelectItems;
        private List<ColorSelectItem> _colorSelectItems;
        private Dictionary<int, Palette> _palettes;

        public ItemViewerForm()
        {
            InitializeComponent();

            fileSelect.ComboBox.DisplayMember = "FileNumber";
            fileSelect.ComboBox.ValueMember = "File";
            _fileSelectItems = new List<FileSelectItem>();

            colorSelect.ComboBox.DisplayMember = "ColorNumber";
            colorSelect.ComboBox.ValueMember = "Entry";
            _colorSelectItems = new List<ColorSelectItem>();

            _palettes = new Dictionary<int, Palette>();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Legend.dat|Legend.dat";
                if (dialog.ShowDialog() == DialogResult.OK)
                    OpenLegendDataFile(dialog.FileName);
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void FileSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fileSelect.SelectedItem == null || colorSelect.SelectedItem == null)
                return;
            itemsPanel.Refresh();
        }

        private void ColorSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fileSelect.SelectedItem == null || colorSelect.SelectedItem == null)
                return;
            itemsPanel.Refresh();
        }

        private void ItemsPanel_Paint(object sender, PaintEventArgs e)
        {
            var selectedFileItem = (FileSelectItem)fileSelect.SelectedItem;
            var selectedColorItem = (ColorSelectItem)colorSelect.SelectedItem;

            if (selectedFileItem == null || selectedColorItem == null)
                return;

            var epf = selectedFileItem.File;

            using (var fileImage = new Bitmap(32 * Columns, 32 * Rows))
            using (var g = Graphics.FromImage(fileImage))
            {
                g.Clear(Color.Teal);
                for (var row = 0; row < Rows; ++row)
                {
                    for (var col = 0; col < Columns; ++col)
                    {
                        var frameIndex = row * Columns + col;
                        if (frameIndex >= epf.Frames.Count)
                            continue;
                        var frame = epf[frameIndex];
                        var tileNumber = GetTileNumber(selectedFileItem.FileNumber, frameIndex);
                        var paletteNumber = _paletteTable.GetPaletteNumber(tileNumber);
                        var palette = _palettes[paletteNumber];
                        if (selectedColorItem.ColorNumber > 0)
                            palette = palette.Dye(selectedColorItem.Entry);
                        using (var itemImage = frame.Render(palette))
                        {
                            g.DrawImage(itemImage, 32 * col + frame.Left, 32 * row + frame.Top);
                        }
                    }
                }
                e.Graphics.DrawImage(fileImage, 0, 0);
            }
        }

        private void OpenLegendDataFile(string fileName)
        {
            try
            {
                _legend = DataFile.Open(fileName);
            }
            catch
            {
                MessageBox.Show("Unable to open data file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _colorTable = new ColorTable(_legend.GetEntry("color0.tbl"));
            _paletteTable = new PaletteTable(_legend.GetEntry("itempal.tbl"));

            _fileSelectItems.Clear();
            foreach (var entry in _legend)
            {
                if (entry.EntryName.Length == 11 &&
                    entry.EntryName.StartsWith("item", StringComparison.CurrentCultureIgnoreCase) &&
                    int.TryParse(entry.EntryName.Substring(4, 3), out int fileNumber))
                {
                    if (entry.EntryName.EndsWith(".epf", StringComparison.CurrentCultureIgnoreCase))
                        _fileSelectItems.Add(new FileSelectItem(new EPFFile(entry.Open()), fileNumber));
                    else if (entry.EntryName.EndsWith(".pal", StringComparison.CurrentCultureIgnoreCase))
                        _palettes.Add(fileNumber, new Palette(entry));
                }
            }
            fileSelect.ComboBox.DataSource = _fileSelectItems;
            fileSelect.Enabled = true;

            _colorSelectItems.Clear();
            foreach (var entry in _colorTable.Entries)
            {
                _colorSelectItems.Add(new ColorSelectItem(entry.Value, entry.Key));
            }
            colorSelect.ComboBox.DataSource = _colorSelectItems;
            colorSelect.Enabled = true;
        }

        private int GetTileNumber(int fileNumber, int frameIndex) => TilesPerFile * (fileNumber - 1) + frameIndex + 1;

        private class FileSelectItem
        {
            public FileSelectItem(EPFFile epfFile, int fileNumber)
            {
                File = epfFile;
                FileNumber = fileNumber;
            }

            public EPFFile File { get; }

            public int FileNumber { get; }
        }

        private class ColorSelectItem
        {
            public ColorSelectItem(ColorTableEntry entry, int colorNumber)
            {
                Entry = entry;
                ColorNumber = colorNumber;
            }

            public ColorTableEntry Entry { get; }

            public int ColorNumber { get; }
        }
    }
}
