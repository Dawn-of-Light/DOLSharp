using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
namespace DOLStudio.Conversion
{
    public partial class ConversionAssistant : Form
    {
        public ConversionAssistant()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.OpenFile());
                LegacyImporter.PerformConversion(doc,progressBar1);
            }
        }
    }
}
