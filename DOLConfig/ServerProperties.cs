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
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using DOL.Database;
using DOL.Database.Connection;

namespace DOLConfig
{
	public partial class DolConfig : Form
	{
		ServerProperty[] sp;
		ServerPropertyCategory[] sc;

		private void LoadServerProperties()
		{
			// open data connection and grab datas
			try
			{
				DataConnection dc = new DataConnection(currentConfig.DBType, currentConfig.DBConnectionString);
				ObjectDatabase db = new ObjectDatabase(dc);
				sp = (ServerProperty[])db.SelectAllObjects(typeof(ServerProperty));
				sc = (ServerPropertyCategory[])db.SelectAllObjects(typeof(ServerPropertyCategory));
			}
			catch
			{
				toolstrip_status_label.Text="Unable to connect/read SQL database !";
				return;
			}
			
			// creation of the SP map
			CreateSPMap(null, tv_spShow.Nodes);
			
			// adding SP Without
			
			// how many SP we have ? 1.6millions ? :D
			toolstrip_status_label.Text ="Loaded: " + sp.Count() + " server properties.";
		}
		
		/// <summary>
		/// convert serverproperty / serverproperty_category table to tree view
		/// </summary>
		/// <param name="category"></param>
		/// <param name="tn"></param>
		private void CreateSPMap(string category, TreeNodeCollection tn)
		{
			// serverproperty_category
			foreach (var current in sc)
			{
				if (current.ParentCategory == category)
				{
					tn.Add(current.DisplayName);
					tn[tn.Count -1].ForeColor = Color.Green;
					CreateSPMap(current.BaseCategory,tn[tn.Count-1].Nodes);
				}
			}
			
			// serverproperty
			List<string> resultSet;
			if (string.IsNullOrEmpty(category))
			{
				resultSet = (from i in sp
					where string.IsNullOrEmpty(i.Category)
					select i.Key).ToList();
			}
			else
			{
				resultSet = (from i in sp
					where i.Category == category
					select i.Key).ToList();
			}
			
			foreach (var current in resultSet)
			{
				tn.Add(current);
				tn[tn.Count-1].ForeColor = Color.Blue;
			}
			
		}
		
		/// <summary>
		/// Display the selected property
		/// </summary>
		/// <param name="spkey"></param>
		private void SelectProperty(string spkey)
		{
			// find the SP
			var target = (from i in sp where i.Key == spkey select i).Single();
			lbl_spName.Text = target.Key.ToUpper();
			tb_spDesc.Text = target.Description;
			tb_spDefaultValue.Text = target.DefaultValue;
			tb_spCurrentValue.Text = target.Value;
		}
		
		#region GUI handling
		/// <summary>
		/// Event on sp_tab enabled, in view of loading the SP
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void TabControl1SelectedIndexChanged(object sender, EventArgs e)
		{
			set_default_values_button.Enabled = true;
			save_config_button.Enabled = true;
			tv_spShow.Nodes.Clear();
			
			if (sp_tab.Enabled && (sender as TabControl).SelectedTab == sp_tab)
			{
				set_default_values_button.Enabled = false;
				save_config_button.Enabled = false;
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
		#endregion
	}
}
