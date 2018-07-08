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
using System.Text;

using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// base class for skills
    /// </summary>
    public abstract class Skill
    {
        protected int m_id;
        protected string m_name;
        protected int m_level;
        protected ushort m_icon;
        protected int m_internalID;

        /// <summary>
        /// Construct a Skill from the name, an id, and a level
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="level"></param>
        public Skill(string name, int id, ushort icon, int level, int internalID)
        {
            m_id = id;
            m_name = name;
            m_level = level;
            m_icon = icon;
            m_internalID = internalID;
        }

        /// <summary>
        /// in most cases it is icon id or other specifiing id for client
        /// like spell id or style id in spells
        /// </summary>
        public virtual int ID
        {
            get { return m_id; }
        }

        /// <summary>
        /// The Skill Name
        /// </summary>
        public virtual string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// The Skill Level
        /// </summary>
        public virtual int Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        /// <summary>
        /// The Skill Icon
        /// </summary>
        public virtual ushort Icon
        {
            get { return m_icon; }
        }

        /// <summary>
        /// Internal ID is used for Hardcoded Tooltip.
        /// </summary>
        public virtual int InternalID {
            get { return m_internalID; }
            set { m_internalID = value; }
        }

        /// <summary>
        /// the type of the skill
        /// </summary>
        public virtual eSkillPage SkillType
        {
            get { return eSkillPage.Abilities; }
        }

        /// <summary>
        /// Clone a skill
        /// </summary>
        /// <returns></returns>
        public virtual Skill Clone()
        {
            return (Skill)MemberwiseClone();
        }
    }

    /// <summary>
    /// the named skill is used for identification purposes
    /// the name is strong and must be unique for one type of skill page
    /// so better make the name real unique
    /// </summary>
    public class NamedSkill : Skill
    {
        private string m_keyName;

        /// <summary>
        /// Construct a named skill from the keyname, name, id and level
        /// </summary>
        /// <param name="keyName">The keyname</param>
        /// <param name="name">The name</param>
        /// <param name="id">The ID</param>
        /// <param name="level">The level</param>
        public NamedSkill(string keyName, string name, int id, ushort icon, int level, int internalID)
            : base(name, id, icon, level, internalID)
        {
            m_keyName = keyName;
        }

        /// <summary>
        /// Returns the string representation of the Skill
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(32)
                .Append("KeyName=").Append(KeyName)
                .Append(", ID=").Append(ID)
                .Append(", Icon=").Append(Icon)
                .Append(", Level=").Append(Level)
                .ToString();
        }

        /// <summary>
        /// strong identification name
        /// </summary>
        public virtual string KeyName
        {
            get { return m_keyName; }
        }
    }

    public class Song : Spell
    {
        public Song(DBSpell spell, int requiredLevel)
            : base(spell, requiredLevel)
        {
        }

        public override eSkillPage SkillType
        {
            get { return eSkillPage.Songs; }
        }

        public override Skill Clone()
        {
            return (Song)MemberwiseClone();
        }
    }

    public class SpellLine : NamedSkill
    {
        protected bool m_isBaseLine;
        protected string m_spec;

        public SpellLine(string keyname, string name, string spec, bool baseline)
            : base(keyname, name, 0, 0, 1, 0)
        {
            m_isBaseLine = baseline;
            m_spec = spec;
        }

        public string Spec
        {
            get { return m_spec; }
        }

        public bool IsBaseLine
        {
            get { return m_isBaseLine; }
        }

        public override eSkillPage SkillType
        {
            get { return eSkillPage.Spells; }
        }

        public override Skill Clone()
        {
            return (SpellLine)MemberwiseClone();
        }
    }

    public enum eSkillPage
    {
        Specialization = 0x00,
        Abilities = 0x01,
        Styles = 0x02,
        Spells = 0x03,
        Songs = 0x04,
        AbilitiesSpell = 0x05,
        RealmAbilities = 0x06,
    }
}
