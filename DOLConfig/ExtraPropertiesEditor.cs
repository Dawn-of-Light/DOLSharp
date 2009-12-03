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
using System.Windows.Forms;

namespace DOLConfig
{
    public partial class ExtraPropertiesEditor : Form
    {
        /// <summary>
        /// the property name
        /// </summary>
        private string _property_name = null;
        public string propertyName
        {
            get { return this._property_name; }
            set { this._property_name = value; }
        }

        /// <summary>
        /// The preferred property type
        /// </summary>
        private string _property_type = null;
        public string propertyType
        {
            get { return this._property_type; }
            set { this._property_type = value; }
        }

        /// <summary>
        /// The property value
        /// </summary>
        private object _property_value = null;
        public object propertyValue
        {
            get { return this._property_value; }
            set { this._property_value = value; }
        }

        /// <summary>
        /// The description of the property
        /// </summary>
        private string _property_description = null;
        public string propertyDescription
        {
            get { return this._property_description; }
            set { this._property_description = value; }
        }

        /// <summary>
        /// Initialized the Property-Editor
        /// </summary>
        /// <param name="property_name">Property name</param>
        /// <param name="property_type">Property type</param>
        /// <param name="property_value">Property value</param>
        /// <param name="property_description">Property description</param>
        public ExtraPropertiesEditor(string property_name, string property_type, object property_value, string property_description)
        {
            InitializeComponent();

            this.propertyName = property_name;
            this.property_name_textbox.Text = property_name;

            if (this.property_type_selectbox.Items.Contains(property_type))
            {
                this.propertyType = property_type;
                this.property_type_selectbox.SelectedItem = property_type;
            }
            else
            {
                if(property_type.Length > 0) this.edit_property_error_label.Text = "Unknown type: " + property_type;
            }

            this.propertyValue = property_value;
            this.property_value_textbox.Text = Convert.ToString(property_value);

            if (property_description.Length > 0)
            {
                this.propertyDescription = property_description;
                this.property_description_label.Text = Convert.ToString(property_description);
            }
        }

        /// <summary>
        /// Saves the edited values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_button_Click(object sender, EventArgs e)
        {
            try
            {
                switch (this.propertyType)
                {
                    case "string":
                        Convert.ToString(propertyValue);
                        break;
                    case "integer":
                        Convert.ToInt32(propertyValue);
                        break;
                    case "boolean":
                        Convert.ToBoolean(propertyValue);
                        break;
                }
            }
            catch (FormatException)
            {
                this.edit_property_error_label.Text = "The value must be a type of " + this.propertyType;
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Cancel the Editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Set the value if value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void property_name_textbox_TextChanged(object sender, EventArgs e)
        {
            this.propertyName = ((TextBox)sender).Text.Trim();
        }

        /// <summary>
        /// Set the value if value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void property_type_selectbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.propertyType = ((ComboBox)sender).SelectedItem.ToString().Trim();
        }


        /// <summary>
        /// Set the value if value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void property_value_textbox_TextChanged(object sender, EventArgs e)
        {
            this.propertyValue = ((TextBox)sender).Text.Trim();
        }
    }
}
