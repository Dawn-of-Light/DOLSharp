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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using DOL.Database;
using DOL.Database.Connection;

namespace DOLConfig
{
	public partial class DolConfig : Form
	{
		List<ServerProperty> sp = new List<ServerProperty>();
		List<ServerPropertyCategory> sc = new List<ServerPropertyCategory>();
		Dictionary<string, ServerProperty> modifyed_sp = new Dictionary<string, ServerProperty>();
		ObjectDatabase db = null;
		string bu_save_text = null;
		string bu_reset_text = null;
		bool sp_saved = true;
		bool sp_tab_selected = false;
		const char Cte_sep=':';
		TreeNode selected_node = null;
		ServerProperty selected_sp = new ServerProperty();
		
		private void LoadServerProperties()
		{
			// open data connection and grab datas
			sp.Clear();
			sc.Clear();
			try
			{
				DataConnection dc = new DataConnection(currentConfig.DBType, currentConfig.DBConnectionString);
				db = new ObjectDatabase(dc);
				ServerProperty[] sptmp = (ServerProperty[])db.SelectAllObjects(typeof(ServerProperty));
				ServerPropertyCategory[] sctmp = (ServerPropertyCategory[])db.SelectAllObjects(typeof(ServerPropertyCategory));
				sp = sptmp.ToList();
				sc = sctmp.ToList();
			}
			catch
			{
				toolstrip_status_label.Text="Unable to connect/read SQL database !";
				return;
			}
			
			// reset display
			tv_spShow.Nodes.Clear();
			
			// creation of the SP map
			CreateSPMap(null, tv_spShow.Nodes,0);
			
			// how many SP we have ? 1.6millions ? :D
			toolstrip_status_label.Text ="Loaded: " + sp.Count() + " server properties.";
		}
		
		/// <summary>
		/// convert serverproperty / serverproperty_category table to tree view
		/// </summary>
		/// <param name="category"></param>
		/// <param name="tn"></param>
		private void CreateSPMap(string category, TreeNodeCollection tn, int level)
		{
			// serverproperty_category
			foreach (var current in sc)
			{
				if (current.ParentCategory == category)
				{
					tn.Add(current.DisplayName);
					tn[tn.Count -1].ForeColor = Color.Green;
					CreateSPMap(current.BaseCategory,tn[tn.Count-1].Nodes, ++level);
					level--;
				}
			}
			
			// serverproperty
			List<ServerProperty> resultSet;
			if (string.IsNullOrEmpty(category))
			{
				resultSet = (from i in sp
				             where string.IsNullOrEmpty(i.Category)
				             select i).ToList();
			}
			else
			{
				resultSet = (from i in sp
				             where i.Category == category
				             select i).ToList();
			}
			
			foreach (var current in resultSet)
			{
				tn.Add(FormatNodeText(level, current.Key, current.Value));
				tn[tn.Count-1].ForeColor = Color.Blue;
			}
			
		}
		
		/// <summary>
		/// Formatting the node string
		/// </summary>
		/// <param name="k"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string FormatNodeText(int level, string k, string v)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(k);
			sb.Append(Cte_sep);
			//System.Diagnostics.Debug.Print("key = " + k + " - level = " + level + " - length = " + k.Length);
			sb.Append(' ', 42 - k.Length - level*3);
			sb.Append(v);
			return sb.ToString();
		}
		
		/// <summary>
		/// Display the selected property
		/// </summary>
		/// <param name="spkey"></param>
		private void SelectProperty(string spkey)
		{
			// find the SP
			selected_sp = (ServerProperty)(from i in sp where i.Key == spkey.Split(Cte_sep)[0] select i).Single().Clone();
			
			// combobox or textbox ?
			cb_spCurrentValue.Text = "False";
			tb_spCurrentValue.Visible = false;
			cb_spCurrentValue.Visible = false;
			
			if (!IsText(selected_sp))
			{
				if (selected_sp.Value.Trim().ToLower() == "true")
					cb_spCurrentValue.Text = "True";
				cb_spCurrentValue.Visible = true;
			}
			else
			{
				tb_spCurrentValue.Text = selected_sp.Value;
				tb_spCurrentValue.Visible = true;
			}
			
			lbl_spName.Text = selected_sp.Key.ToUpper();
			tb_spDesc.Text = selected_sp.Description;
			tb_spDefaultValue.Text = selected_sp.DefaultValue;
			selected_node = tv_spShow.SelectedNode;
		}
		
		/// <summary>
		/// save/reset the modified SPs in the database
		/// </summary>
		private void SaveSP()
		{
			foreach (var current in modifyed_sp)
				db.SaveObject((ServerProperty)current.Value);
			
			sp_saved = true;
			LoadServerProperties();
		}
		private void ResetSP()
		{
			sp_saved = true;
			LoadServerProperties();
		}
		
		/// <summary>
		/// For now, we differentiate only boolean and string SPs
		/// </summary>
		/// <param name="this_sp"></param>
		/// <returns></returns>
		bool IsText(ServerProperty this_sp)
		{
			if (this_sp.DefaultValue.Trim().ToLower() == "true" || this_sp.DefaultValue.Trim().ToLower() == "false") return false;
			return true;
		}
		
		/// <summary>
		/// Event on sp_tab enabled, in view of loading the SP
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void TabControl1SelectedIndexChanged(object sender, EventArgs e)
		{
			// do not change modified SPs
			if (sp_tab_selected)
			{
				sp_tab_selected = false;
				return;
			}
			
			// cache buttons text
			if (string.IsNullOrEmpty (bu_save_text))
				bu_save_text = save_config_button.Text;
			if (string.IsNullOrEmpty (bu_reset_text))
				bu_reset_text = set_default_values_button.Text;
			save_config_button.Text = bu_save_text;
			set_default_values_button.Text = bu_reset_text;
					
			// check if we saved modified SPs
			if (!sp_saved && (sender as TabControl).SelectedTab != sp_tab)
			{
				if (MessageBox.Show("Your changes will be lost. Continue ?","Warning",MessageBoxButtons.OKCancel,MessageBoxIcon.Asterisk,MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
				{
					sp_tab_selected = true;
					(sender as TabControl).SelectedTab = sp_tab;
					return;
				}
				sp_tab_selected = false;
				sp_saved = true;
			}

			// fill the treeview if suitable ( = no xml db selected)
			if (sp_tab.Enabled && (sender as TabControl).SelectedTab == sp_tab)
			{
				save_config_button.Text = "Save all properties";
				set_default_values_button.Text = "Cancel all changes";
				LoadServerProperties ();
			}
		}
		
		/// <summary>
		/// Click on SP node
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Tv_spShowAfterSelect(object sender, TreeViewEventArgs e)
		{
			if ((sender as TreeView).SelectedNode.ForeColor != Color.Green)
				SelectProperty( (sender as TreeView).SelectedNode.Text);
		}
		
		/// <summary>
		/// Click on Update SP
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Bu_spChangeClick(object sender, EventArgs e)
		{
			if (IsText(selected_sp))
				selected_sp.Value = tb_spCurrentValue.Text;
			else
				selected_sp.Value = cb_spCurrentValue.Text;
			selected_sp.Description = tb_spDesc.Text;
			selected_sp.DefaultValue = tb_spDefaultValue.Text;
			
			// get base sp for comparisons
			var original_sp = (from i in sp where i.Key == selected_sp.Key select i).Single();
			
			// update colors related to changes to be applyed
			if (selected_sp.Description != original_sp.Description)
			{
				original_sp.Description = selected_sp.Description;
				selected_node.ForeColor = Color.Orange;
				sp_saved = false;
			}
			if (selected_sp.Value.Trim() != original_sp.Value.Trim())
			{
				original_sp.Value = selected_sp.Value.Trim();
				selected_node.ForeColor = Color.OrangeRed;
				sp_saved = false;
			}
			
			// update display
			selected_node.Text = FormatNodeText(selected_node.Level, selected_sp.Key, selected_sp.Value);
			
			// create a list of modifyed SPs
			if (!sp_saved)
			{
				if (modifyed_sp.Keys.Contains(original_sp.Key))
					modifyed_sp[original_sp.Key] = original_sp;
				else
					modifyed_sp.Add(original_sp.Key, original_sp);
			}
		}
	}
}
