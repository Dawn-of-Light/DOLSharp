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
[DataTable(TableName = "MobXAmbientBehaviour", PreCache = true)]
public class MobXAmbientBehaviour : DataObject
{
	private string m_source;
	private string m_trigger;
	private ushort m_emote;
	private string m_text;
	private ushort m_chance;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="name">Mob's name</param>
	/// <param name="type">The type of trigger to act on (eAmbientTrigger)</param>
	/// <param name="text">The formatted text for the trigger. You can use [targetclass],[targetname],[sourcename]
	/// and supply formatting stuff: [b] for broadcast, [y] for yell</param>
	/// <param name="action">the desired emote</param>
	public MobXAmbientBehaviour()
	{
		m_source = string.Empty;
		m_trigger =string.Empty;
		m_emote = 0;
		m_text = string.Empty;
		m_chance = 0;
	}

	public MobXAmbientBehaviour(string name, string trigger, ushort emote, string text, ushort chance)
	{
		m_source = name;
		m_trigger = trigger;
		m_emote = emote;
		m_text = text;
		m_chance = chance;
	}

	[DataElement(AllowDbNull = false)]
	public string Source
	{
		get { return m_source; }
		set { m_source = value; }
	}

	[DataElement(AllowDbNull = false)]
	public string Trigger
	{
		get { return m_trigger; }
		set { m_trigger = value; }
	}

	[DataElement(AllowDbNull = true)]
	public ushort Emote
	{
		get { return m_emote; }
		set { m_emote = value; }
	}

	[DataElement(AllowDbNull = false)]
	public string Text
	{
		get { return m_text; }
		set { m_text = value; }
	}
	
	[DataElement(AllowDbNull = false)]
	public ushort Chance
	{
		get { return m_chance; }
		set { m_chance = value; }
	}

}