using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DOLStudio
{
    public partial class Splash : Form
    {
        public Splash()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            Bitmap b = new Bitmap(BackgroundImage);
            b.MakeTransparent(b.GetPixel(1, 1));
            BackgroundImage = b;
        }

        private void statusLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
