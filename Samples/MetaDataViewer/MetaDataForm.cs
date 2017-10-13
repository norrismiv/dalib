using DALib.Data;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace MetaDataViewer
{
    public partial class MetaDataForm : Form
    {
        public MetaDataForm() => InitializeComponent();

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    OpenMetaDataFile(dialog.FileName);
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenMetaDataFile(string fileName)
        {
            MetaData metaData;

            using (var metaDataStream = File.OpenRead(fileName))
            {
                if (metaDataStream.ReadByte() == 0x78 && metaDataStream.ReadByte() == 0x9C)
                {
                    using (var decompressedMetaDataStream = new MemoryStream())
                    using (var decompressionStream = new DeflateStream(metaDataStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedMetaDataStream);
                        decompressedMetaDataStream.Seek(0, SeekOrigin.Begin);
                        metaData = new MetaData(decompressedMetaDataStream);
                    }
                }
                else
                {
                    metaDataStream.Seek(-2, SeekOrigin.Current);
                    metaData = new MetaData(metaDataStream);
                }
            }

            var entryNodes = new TreeNode[metaData.Entries.Count];
            for (var i = 0; i < metaData.Entries.Count; ++i)
            {
                var entry = metaData[i];
                var valueNodes = new TreeNode[entry.Values.Count];
                for (var j = 0; j < entry.Values.Count; ++j)
                {
                    valueNodes[j] = new TreeNode(entry[j]);
                }
                entryNodes[i] = new TreeNode(entry.Key, valueNodes);
            }

            Text = $"{Path.GetFileName(fileName)} - Meta Data Viewer";
            entriesList.Nodes.Clear();
            entriesList.Nodes.AddRange(entryNodes);
        }
    }
}
