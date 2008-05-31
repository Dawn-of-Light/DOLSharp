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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using DOL.Database2;
using DOL;

namespace DOLStudio
{
    public partial class ObjectEditorForm : Form
    {
        ParentForm m_parentform;
        MemberInfo m_currentInfo = null;
        DatabaseObject m_currentobject
        {
            get { return propertyGrid.SelectedObject as DatabaseObject; }
            set { propertyGrid.SelectedObject = value; }
        }
        public ObjectEditorForm()
        {
            InitializeComponent();
            ResultView.MultiSelect = false;
        }

        private void ObjectEditorForm_Load(object sender, EventArgs e)
        {
        }

        public void ReloadTypes()
        {
            Cursor = Cursors.WaitCursor;
            StatusLabel.Text = "Loading types";
            List<Type> Orphans = new List<Type>(); // Parent <-> Type
            Dictionary<string, TreeNode> TypeCache = new Dictionary<string,TreeNode>();
            foreach (Type t in m_parentform.TypeBrowser)
            {
                if (t.BaseType == typeof(DatabaseObject))
                {
                    TreeNode temp = treeView1.Nodes.Add(t.AssemblyQualifiedName, t.Name);
                    TypeCache.Add(t.AssemblyQualifiedName, temp);
                    
                }
                else if (TypeCache.ContainsKey(t.BaseType.AssemblyQualifiedName))
                {
                    TreeNode temp = TypeCache[t.BaseType.AssemblyQualifiedName].Nodes.Add(t.AssemblyQualifiedName, t.Name);
                    TypeCache.Add(t.AssemblyQualifiedName, temp);
                }
                else
                    Orphans.Add(t);                
            }
            while (Orphans.Count > 0)
            {
                foreach (Type t in Orphans)
                {
                    if (TypeCache.ContainsKey(t.BaseType.AssemblyQualifiedName))
                    {
                        TreeNode temp = TypeCache[t.BaseType.AssemblyQualifiedName].Nodes.Add(t.AssemblyQualifiedName, t.Name);
                        TypeCache.Add(t.AssemblyQualifiedName, temp);
                        Orphans.Remove(t);
                        break;
                    }
                }
            }
            Cursor = Cursors.Default;
            treeView1.Refresh();
            StatusLabel.Text = "Types loaded !Ready.";
        }

        private void ObjectEditorForm_Shown(object sender, EventArgs e)
        {
            m_parentform = (ParentForm)MdiParent;

            ReloadTypes();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Fieldchooser.Items.Clear();
            ResultView.Items.Clear();
            Type t = Type.GetType(treeView1.SelectedNode.Name as string);
            foreach (MemberInfo info in t.GetMembers())
            {
                if (!(info is PropertyInfo || info is FieldInfo))
                    continue;       
                Fieldchooser.Items.Add(info);
            }
            foreach(DatabaseObject dbo in (from s in DatabaseLayer.Instance
                                           where t.IsAssignableFrom(s.GetType())
                                        select s))
            {
                ResultView.Items.Add(dbo.ID.ToString(), dbo.ID.ToString(), null);
            }
        }

        private void IDnbquery_Click(object sender, EventArgs e)
        {
            Type t = Type.GetType(treeView1.SelectedNode.Name as string);
            m_currentobject = DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(t,Idnb.Text);
            ResultView.Items.Clear();
            ResultView.Items.Add(m_currentobject.ID.ToString(), m_currentobject.ID.ToString(), null);
        }

        private void IDbutton_Click(object sender, EventArgs e)
        {
            ResultView.Items.Clear();
            m_currentobject = DatabaseLayer.Instance.GetDatabaseObjectFromID(UInt64.Parse(IDfield.Value.ToString()));
            ResultView.Items.Add(m_currentobject.ID.ToString(), m_currentobject.ID.ToString(), null);
        }

        private void QueryBtn_Click(object sender, EventArgs e)
        {
            if (Fieldchooser.Text.Length == 0)
            {
                Type t = Type.GetType(treeView1.SelectedNode.Name as string);
                foreach (DatabaseObject dbo in (from s in DatabaseLayer.Instance
                                                where s.GetType().IsAssignableFrom(t)
                                                select s))
                {
                    ResultView.Items.Add(dbo.ID.ToString(), dbo.ID.ToString(), null);
                }
            }
        }

        private void createNewItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode == null)
            {
                MessageBox.Show("Please select a type below");
                return;
            }
            Type t = Type.GetType(treeView1.SelectedNode.Name as string);
            ConstructorInfo construct =  t.GetConstructor(Type.EmptyTypes);
            if (construct == null)
            {
                MessageBox.Show("No constructors found without parameters");
                return;
            }
            DatabaseObject dbo = (DatabaseObject)construct.Invoke(null);
            ResultView.Items.Add(dbo.ID.ToString(),dbo.ID.ToString(),0);
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {

        }

        private void ResultView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ResultView.SelectedItems.Count > 0)
            {
                UInt64 ID = UInt64.Parse(ResultView.SelectedItems[0].Name as string);
                m_currentobject = DatabaseLayer.Instance.GetDatabaseObjectFromID(ID);
                StatusLabel.Text = m_currentobject.GetType().Name;
            }
            else
            {
                m_currentobject = null;
                StatusLabel.Text = "Ready";
            }
        }

    }
}
