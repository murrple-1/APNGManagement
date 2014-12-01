using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

using APNGLib;

namespace APNGViewer
{
    public partial class Viewer : Form
    {
        private bool isStarted;

        public Viewer()
        {
            InitializeComponent();
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                string folder = fbd.SelectedPath;
                string[] files = System.IO.Directory.GetFiles(folder, "*.png");
                foreach (string file in files)
                {
                    try
                    {
                        PictureBox pb = new PictureBox();
                        Bitmap i = (Bitmap)Bitmap.FromFile(file);
                        pb.Image = i;
                        pb.Size = i.Size;
                        flowLayoutPanel2.Controls.Add(pb);
                    }
                    catch (Exception)
                    {
                        PictureBox pb = new PictureBox();
                        pb.Image = pb.ErrorImage;
                        pb.BorderStyle = BorderStyle.FixedSingle;
                        flowLayoutPanel2.Controls.Add(pb);
                    }

                    try
                    {
                        APNGLib.APNG png = new APNGLib.APNG();
                        using (Stream s = File.OpenRead(file))
                        {
                            png.Load(s);
                        }
                        APNGBox pbA = new APNGBox(png);
                        flowLayoutPanel1.Controls.Add(pbA);
                        pbA.Start();
                        isStarted = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        PictureBox pbA = new PictureBox();
                        pbA.Image = pbA.ErrorImage;
                        pbA.BorderStyle = BorderStyle.FixedSingle;
                        flowLayoutPanel1.Controls.Add(pbA);
                    }
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            foreach (Control c in flowLayoutPanel1.Controls)
            {
                if (c is APNGBox)
                {
                    (c as APNGBox).NextImage();
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (isStarted)
            {
                btnNext.Enabled = true;
                btnPLay.Text = "Play";
                isStarted = false;
                foreach (Control c in flowLayoutPanel1.Controls)
                {
                    if (c is APNGBox)
                    {
                        (c as APNGBox).Stop();
                    }
                }
            }
            else
            {
                btnNext.Enabled = false;
                btnPLay.Text = "Stop";
                isStarted = true;
                foreach (Control c in flowLayoutPanel1.Controls)
                {
                    if (c is APNGBox)
                    {
                        (c as APNGBox).Start();
                    }
                }
            }
        }
    }
}
