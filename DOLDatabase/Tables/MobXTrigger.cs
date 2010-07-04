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

using DOL.Database;
using DOL.Database.Attributes;

/// <summary>
/// Stores the triggers with corresponding text for a mob, for example if the mob should say something when it dies.
/// </summary>
[DataTable(TableName = "MobXTrigger", PreCache = true)]
public class DBMobXTrigger : DataObject
{
    private string m_name;
    private string m_type;
    private string m_action;
    private string m_text;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">Mob's name</param>
    /// <param name="type">The type of trigger to act on (Aggro, Die, etc.)</param>
    /// <param name="text">The text for the trigger (what the mob says when it aggro's or dies)</param>
    public DBMobXTrigger()
    {
        m_name = "";
        m_type = "";
        m_action = "";
        m_text = "";
    }

    public DBMobXTrigger(string name, string type, string action, string text)
    {
        m_name = name;
        m_type = type;
        m_action = action;
        m_text = text;
    }

    [DataElement(AllowDbNull = false)]
    public string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }

    [DataElement(AllowDbNull = false)]
    public string Type
    {
        get { return m_type; }
        set { m_type = value; }
    }

    [DataElement(AllowDbNull = false)]
    public string Action
    {
        get { return m_action; }
        set { m_action = value; }
    }

    [DataElement(AllowDbNull = false)]
    public string Text
    {
        get { return m_text; }
        set { m_text = value; }
    }

}