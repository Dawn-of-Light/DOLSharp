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
		static List<ServerProperty> spList = new List<ServerProperty>();
		static List<ServerPropertyCategory> scList = new List<ServerPropertyCategory>();

		private void LoadServerProperties()
		{
			// open data connection
			ServerProperty[] sp;
			ServerPropertyCategory[] sc;
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
			
			// bufferisation of the datas
			spList.Clear();
			scList.Clear();
			foreach(var current in sp)
				spList.Add(current);
			foreach(var current in sc)
				scList.Add(current);
			
			// creation of the nodemap
			foreach(var current in scList)
			{
				if (string.IsNullOrEmpty(current.ParentCategory) && !string.IsNullOrEmpty(current.BaseCategory))
					CreateMap(tv_spShow.Nodes, current.BaseCategory, current.ObjectId);
			}
			
			// filling the nodemap
			FillMap();
			
			// how many SP we have ? 1.6millions ? :D
			toolstrip_status_label.Text ="Loaded: " + spList.Count() + " server properties."; //result.Count() + " properties on " + (from i in sp select i).Count() + " total.";
		}

		private void CreateMap(TreeNodeCollection nodes, string parent, string id)
		{
			// create root node
			foreach (var current in scList)
			{
				if (current.ParentCategory == parent && current.ObjectId != id)
				{
					nodes.Add(current.BaseCategory);
					nodes[nodes.Count - 1].ForeColor = Color.Green;
					CreateMap(nodes, current.ParentCategory, current.ObjectId);
				}
			}
		}
		
		private void FillMap()
		{
			// attach sps to root node
			foreach (var current in spList)
			{
				if (string.IsNullOrEmpty(current.Category))
				{
					tv_spShow.Nodes.Add(current.Key);
					tv_spShow.Nodes[tv_spShow.Nodes.Count - 1].ForeColor = Color.Blue;
				}
			}
		}
		
		private void SelectProperty(string spkey)
		{
			// find the SP
			var target = (from i in spList where i.Key == spkey select i).Single();
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
			if ((sender as TreeView).SelectedNode.ForeColor == Color.Blue)
				SelectProperty( (sender as TreeView).SelectedNode.Text);
		}
		#endregion
	}
}
