using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using APNGLib;
using SevenZip;

namespace APNGBuilder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    editRootFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            DirectoryInfo info = new DirectoryInfo(editRootFolder.Text);
            foreach (DirectoryInfo dir in info.GetDirectories())
            {
                TraverseDirectory(info.FullName, dir);
            }
        }

        private void TraverseDirectory(string root, DirectoryInfo info)
        {
            FileInfo[] files = info.GetFiles();
            if (files.Length > 0)
            {
                FileInfo first = files[0];
                DirectoryInfo dir = first.Directory;
                GenerateAPNG(root + info.FullName.Substring(info.FullName.LastIndexOf('\\')) + ".png.7z", dir.GetFiles("*.png"));
            }
            foreach (DirectoryInfo dir in info.GetDirectories())
            {
                TraverseDirectory(root, dir);
            }
        }

        private void GenerateAPNG(string filename, IList<FileInfo> files)
        {
            APNG apng = APNGAssembler.AssembleAPNG(files, true);
            if (apng != null)
            {
                Stream pngStr = apng.ToStream();
                pngStr.Position = 0;
                using (FileStream fs = File.Create(filename))
                {
                    SevenZipCompressor compressor = new SevenZipCompressor();
                    compressor.CompressStream(pngStr, fs);
                }
                pngStr.Close();
            }
        }
    }
}
