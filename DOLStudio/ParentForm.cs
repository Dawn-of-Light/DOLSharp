/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DOL.GS;
using DOL.Database2;
namespace DOLStudio
{
    public partial class ParentForm : Form
    {
        static ParentForm m_Instance = null;  // Pseudo-Singleton... make a Singleton?
        List<ObjectEditorForm> m_ObjectEditors = new List<ObjectEditorForm>();
        TypeBrowserForm m_TypeBrowserForm = null;
        TypeBrowser m_TypeBrowser;
        GameServerConfiguration m_config;
        Splash m_splash = new Splash();
        public ParentForm()
        {
            if (m_Instance != null)
                throw new Exception("Multiple ParentForms");
            
  
            //TODO: load scripts... , so dynamic types are supported
            m_Instance = this;
            InitializeComponent();
            Thread th = new Thread(new ThreadStart(DoSplash));
            th.Start();
            Thread.Sleep(3000);
            m_config = new GameServerConfiguration();
            m_splash.statusLabel.Text = "Loading Database Provider";
            LoadSettings();
            LoadDatabaseProvider();
            th.Abort();
            m_TypeBrowser = new TypeBrowser();
            m_TypeBrowserForm = new TypeBrowserForm();
            m_TypeBrowserForm.MdiParent = this;
            m_TypeBrowserForm.Show();
            ObjectEditorForm temp =  new ObjectEditorForm();
            temp.MdiParent = this;
            temp.Show();
            m_ObjectEditors.Add(temp);
           
        }
        void DoSplash()
        {
            m_splash.ShowDialog();
        }
        protected void LoadSettings()
        {
            if(File.Exists(Application.StartupPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "serverconfig.xml"))
            {
				FileInfo ConfigFile = new FileInfo(Application.StartupPath + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "serverconfig.xml");
                m_config.LoadFromXMLFile(ConfigFile);
            }
        }
        protected void LoadDatabaseProvider()
        {
            Assembly dbAssembly;
            Type dbProvider = typeof(DOL.Database2.Providers.NullDatabaseProvider);
            if (m_config.DBProviderAssembly.Length > 0)
            {
                try
                {
                    dbAssembly = Assembly.LoadFile(m_config.DBProviderAssembly);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not load Database Provider assembly!", e);
                }
                    /*
                catch (FileLoadException e) { log.Error("Could not load database assembly " + Configuration.DBProviderAssembly + " check permissions", e); }
                catch (FileNotFoundException e) { log.Error("Could not load database assembly " + Configuration.DBProviderAssembly + " : Check if file exists and spelling", e); }
                catch (BadImageFormatException e)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Could not load database assembly " + Configuration.DBProviderAssembly + "-not a valid assembly. Check compile Target ( same version of NET or older than DOL) ", e);
                }  */
            }
            if (m_config.DBProviderType.Length != 0)
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type temp = asm.GetType(m_config.DBProviderType, false, true);
                    if (temp != null)
                    {
                        dbProvider = temp;
                        break;
                    }
                }

            DatabaseLayer.Instance.SetProvider((DatabaseProvider)Activator.CreateInstance(dbProvider), m_config.DBConnectionString);
            m_splash.statusLabel.Text = "Loading database";
            DatabaseLayer.Instance.RestoreWorldState();
            m_splash.statusLabel.Text = "Finalizing";
        }
        public static ParentForm Instance
        {
            get { return m_Instance; }
        }
        public TypeBrowserForm TypeBrowserForm
        {
            get { return m_TypeBrowserForm; }
        }
        public TypeBrowser TypeBrowser
        {
            get { return m_TypeBrowser; }
        }
        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void objectWindowToolStripMenuItem_Click(object sender, EventArgs e)        
        {
            ObjectEditorForm window = new ObjectEditorForm();
            window.MdiParent = this;
            window.Show();
            ToolStripItem toolstrip;
        }

        private void typeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TypeBrowserForm == null)
            {
                TypeBrowserForm temp = new TypeBrowserForm();
                temp.MdiParent = this;
                temp.Show();
            }
            else if (TypeBrowserForm.Visible == false)
            {
                TypeBrowserForm.Visible = true;
                typeWindowToolStripMenuItem.Checked = true;
            }
            else if (TypeBrowserForm.Visible == true)
            {
                TypeBrowserForm.Visible = false;
                typeWindowToolStripMenuItem.Checked = false;
            }
            else
                TypeBrowserForm.Show();
        }

        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
        }

        private void toolsMenu_Click(object sender, EventArgs e)
        {

        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Conversion.ConversionAssistant assist = new DOLStudio.Conversion.ConversionAssistant();
            assist.ShowDialog();
        }
    }
}
