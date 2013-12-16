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
using System.Text.RegularExpressions;

namespace RegExControls
{
    public partial class RegExTextBox : TextBox 
    {
        private string mRegularExpression;

        public string Regular_Expression
        {
            get
            {
                return mRegularExpression;
            }
            set
            {
                mRegularExpression = value;
            }
        }

        public RegExTextBox()
        {
            InitializeComponent();
        }

        public bool ValidateControl(string text)
        {
            string TextToValidate;
            Regex expression;

            try
            {
                TextToValidate = text;
                expression = new Regex(Regular_Expression);
            }
            catch
            {
                MessageBox.Show("Regex invalid!");
                return false;
            }

            // test text with expression
            if (expression.IsMatch(TextToValidate))
            {
                return true;
            }
            else
            {
                // no match
                return false;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //On key press
            if (Char.IsControl(e.KeyChar) == false)
            {
                //If it is a character
                string newText = base.Text.Substring(0, base.SelectionStart) + e.KeyChar.ToString() + base.Text.Substring(base.SelectionStart + base.SelectionLength);
                if (newText != "")
                {
                    bool validateCheck = this.ValidateControl(newText);
                    if (validateCheck == false)
                    {
                        //Not allowed character, do not print it
                        e.Handled = true;
                    }
                }                 
            }
            
            base.OnKeyPress(e);
        }
    }
}
