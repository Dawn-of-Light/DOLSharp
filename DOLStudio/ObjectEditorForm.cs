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
        DatabaseObject m_currentobject;
        Type CurrentType;
        public ObjectEditorForm()
        {
            InitializeComponent();
        }

        private void ObjectEditorForm_Load(object sender, EventArgs e)
        {
        }

        public void ReloadTypes()
        {
            Cursor = Cursors.WaitCursor;
            List<Type> Orphans = new List<Type>(); // Parent <-> Type
            Dictionary<string, TreeNode> TypeCache = new Dictionary<string,TreeNode>();
            foreach (Type t in m_parentform.TypeBrowser)
            {
                if (t.BaseType == typeof(DatabaseObject))
                {
                    TreeNode temp = treeView1.Nodes.Add(t.AssemblyQualifiedName, t.Name);
                    TypeCache.Add(t.AssemblyQualifiedName, temp);
                    /*
                    Type type = t;
                    while (1)
                    {
                        foreach (Type orphan in Orphans)
                        {
                            if (orphan.BaseType == type)
                                TypeCache.Add(orphan.AssemblyQualifiedName, TypeCache[orphan.BaseType.AssemblyQualifiedName].Nodes.Add(orphan.AssemblyQualifiedName, orphan.Name));
                        }
                    }
                     */
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
        }

        private void ObjectEditorForm_Shown(object sender, EventArgs e)
        {
            m_parentform = (ParentForm)MdiParent;
            ReloadTypes();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Fieldchooser.Items.Clear();
            Type t = Type.GetType(treeView1.SelectedNode.Name as string);
            foreach (MemberInfo info in t.GetMembers())
            {
                if (!(info is PropertyInfo || info is FieldInfo))
                    continue;       
                Fieldchooser.Items.Add(info);
            }
            ResultView.Items.Clear();
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
            m_currentobject = DatabaseLayer.Instance.GetDatabaseObjectFromID(UInt64.Parse(IDfield.Value.ToString()));
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
            Type t = Type.GetType(treeView1.SelectedNode.Name as string);
            ConstructorInfo construct =  t.GetConstructor(Type.EmptyTypes);
            ResultView.Clear();
            ResultView.Items.Add(construct.Invoke(null).ToString());

        }

    }
}
