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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS
{
    /// <summary>
    /// The Base class for all Character Classes in DOL
    /// </summary>
    public abstract class CharacterClassBase : ICharacterClass
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Name of class
        /// </summary>
        private string _name;

        /// <summary>
        /// Profession of character, e.g. Defenders of Albion
        /// </summary>
        private string _profession;

        /// <summary>
        /// The GamePlayer for this character
        /// </summary>
        public GamePlayer Player { get; private set; }

        private static readonly string[] AutotrainableSkills = new string[0];

        protected CharacterClassBase()
        {
            ID = 0;
            _name = "Unknown Class";
            BaseName = "Unknown Base Class";
            _profession = string.Empty;

            // initialize members from attributes
            Attribute[] attrs = Attribute.GetCustomAttributes(GetType(), typeof(CharacterClassAttribute));
            foreach (Attribute attr in attrs)
            {
                if (attr is CharacterClassAttribute attribute)
                {
                    ID = attribute.ID;
                    _name = attribute.Name;
                    BaseName = attribute.BaseName;
                    if (Util.IsEmpty(attribute.FemaleName) == false)
                    {
                        FemaleName = attribute.FemaleName;
                    }

                    break;
                }
            }
        }

        public virtual void Init(GamePlayer player)
        {
            // TODO : Should Throw Exception Here.
            if (Player != null && Log.IsWarnEnabled)
            {
                Log.Warn($"Character Class initializing Player when it was already initialized ! Old Player : {Player} New Player : {player}");
            }

            Player = player;
        }

        public string FemaleName { get; }

        public int BaseHP { get; protected set; } = 600;

        public int ID { get; protected set; }

        public string Name
        {
            get => Player != null && Player.Gender == eGender.Female && !Util.IsEmpty(FemaleName) ? FemaleName : _name;
            protected set => _name = value;
        }

        public string BaseName { get; protected set; }

        /// <summary>
        /// Return Translated Profession
        /// </summary>
        public string Profession
        {
            get => Player.TryTranslateOrDefault(_profession, _profession);
            protected set => _profession = value;
        }

        public int SpecPointsMultiplier { get; protected set; } = 10;

        /// <summary>
        /// This is specifically used for adjusting spec points as needed for new training window
        /// For standard DOL classes this will simply return the standard spec multiplier
        /// </summary>
        public int AdjustedSpecPointsMultiplier => SpecPointsMultiplier;

        public eStat PrimaryStat { get; protected set; } = eStat.UNDEFINED;

        public eStat SecondaryStat { get; protected set; } = eStat.UNDEFINED;

        public eStat TertiaryStat { get; protected set; } = eStat.UNDEFINED;

        public eStat ManaStat { get; protected set; } = eStat.UNDEFINED;

        public int WeaponSkillBase { get; protected set; } = 400;

        public int WeaponSkillRangedBase { get; } = 440;

        /// <summary>
        /// Maximum number of pulsing spells that can be active simultaneously
        /// </summary>
        public virtual ushort MaxPulsingSpells => 1;

        public virtual string GetTitle(GamePlayer player, int level)
        {

            // Clamp level in 5 by 5 steps - 50 is the max available translation for now
            int clamplevel = Math.Min(50, (level / 5) * 5);

            string none = player.TryTranslateOrDefault("!None!", "PlayerClass.GetTitle.none");

            if (clamplevel > 0)
            {
                return player.TryTranslateOrDefault($"!{_name}!", $"PlayerClass.{_name}.GetTitle.{clamplevel}");
            }

            return none;
        }

        public virtual eClassType ClassType => eClassType.ListCaster;

        /// <summary>
        /// Return the base list of Realm abilities that the class
        /// can train in.  Added by Echostorm for RAs
        /// </summary>
        /// <returns></returns>
        public virtual IList<string> GetAutotrainableSkills()
        {
            return AutotrainableSkills;
        }

        /// <summary>
        /// What Champion trainer does this class use?
        /// </summary>
        /// <returns></returns>
        public virtual GameTrainer.eChampionTrainerType ChampionTrainerType()
        {
            return GameTrainer.eChampionTrainerType.None;
        }

        /// <summary>
        /// Add things that are required for current level
        /// Skills and other things are handled through player specs... (on Refresh Specs)
        /// </summary>
        /// <param name="player">player to modify</param>
        /// <param name="previousLevel">the previous level of the player</param>
        public virtual void OnLevelUp(GamePlayer player, int previousLevel)
        {
        }

        /// <summary>
        /// Add various skills as the player levels his realm rank up
        /// </summary>
        /// <param name="player">player to modify</param>
        public virtual void OnRealmLevelUp(GamePlayer player)
        {
            // we dont want to add things when players arent using their advanced class
            if (player.CharacterClass.BaseName == player.CharacterClass.Name)
            {
            }
        }

        /// <summary>
        /// Add all spell-lines and other things that are new when this skill is trained
        /// </summary>
        /// <param name="player">player to modify</param>
        /// <param name="skill">The skill that is trained</param>
        public virtual void OnSkillTrained(GamePlayer player, Specialization skill)
        {
        }

        /// <summary>
        /// Checks whether player has ability to use lefthanded weapons
        /// </summary>
        public virtual bool CanUseLefthandedWeapon => false;

        public virtual bool HasAdvancedFromBaseClass()
        {
            return true;
        }

        public virtual void SetControlledBrain(IControlledBrain controlledBrain)
        {
            if (controlledBrain == Player.ControlledBrain)
            {
                return;
            }

            if (controlledBrain == null)
            {
                Player.Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
                Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget2", Player.ControlledBrain.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                if (controlledBrain.Owner != Player)
                {
                    throw new ArgumentException($"ControlledNpc with wrong owner is set (player={Player.Name}, owner={controlledBrain.Owner.Name})", nameof(controlledBrain));
                }

                if (Player.ControlledBrain == null)
                {
                    Player.InitControlledBrainArray(1);
                }

                Player.Out.SendPetWindow(controlledBrain.Body, ePetWindowAction.Open, controlledBrain.AggressionState, controlledBrain.WalkState);
                if (controlledBrain.Body != null)
                {
                    Player.Out.SendNPCCreate(controlledBrain.Body); // after open pet window again send creation NPC packet
                    if (controlledBrain.Body.Inventory != null)
                    {
                        Player.Out.SendLivingEquipmentUpdate(controlledBrain.Body);
                    }
                }
            }

            Player.ControlledBrain = controlledBrain;
        }

        /// <summary>
        /// Releases controlled object
        /// </summary>
        public virtual void CommandNpcRelease()
        {
            IControlledBrain controlledBrain = Player.ControlledBrain;

            GameNPC npc = controlledBrain?.Body;
            if (npc == null)
            {
                return;
            }

            Player.Notify(GameLivingEvent.PetReleased, npc);
        }

        /// <summary>
        /// Invoked when pet is released.
        /// </summary>
        public virtual void OnPetReleased()
        {
        }

        /// <summary>
        /// Can this character start an attack?
        /// </summary>
        /// <param name="attackTarget"></param>
        /// <returns></returns>
        public virtual bool StartAttack(GameObject attackTarget)
        {
            return true;
        }

        /// <summary>
        /// Return the health percent of this character
        /// </summary>
        public virtual byte HealthPercentGroupWindow => Player.HealthPercent;

        /// <summary>
        /// Create a shade effect for this player.
        /// </summary>
        /// <returns></returns>
        public virtual ShadeEffect CreateShadeEffect()
        {
            return new ShadeEffect();
        }

        /// <summary>
        /// Changes shade state of the player.
        /// </summary>
        /// <param name="state">The new state.</param>
        public virtual void Shade(bool makeShade)
        {
            if (Player.IsShade == makeShade)
            {
                if (makeShade && (Player.ObjectState == GameObject.eObjectState.Active))
                {
                    Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.Shade.AlreadyShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                return;
            }

            if (makeShade)
            {
                // Turn into a shade.
                Player.Model = Player.ShadeModel;
                Player.ShadeEffect = CreateShadeEffect();
                Player.ShadeEffect.Start(Player);
            }
            else
            {
                if (Player.ShadeEffect != null)
                {
                    // Drop shade form.
                    Player.ShadeEffect.Stop();
                    Player.ShadeEffect = null;
                }

                // Drop shade form.
                Player.Model = Player.CreationModel;
                Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.Shade.NoLongerShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }

        /// <summary>
        /// Called when player is removed from world.
        /// </summary>
        /// <returns></returns>
        public virtual bool RemoveFromWorld()
        {
            return true;
        }

        /// <summary>
        /// What to do when this character dies
        /// </summary>
        /// <param name="killer"></param>
        public virtual void Die(GameObject killer)
        {
        }

        public virtual void Notify(DOLEvent e, object sender, EventArgs args)
        {
        }

        public virtual bool CanChangeCastingSpeed(SpellLine line, Spell spell)
        {
            return true;
        }
    }

    /// <summary>
    /// Usable default Character Class, if not other can be found or used
    /// just for getting things valid in problematic situations
    /// </summary>
    public class DefaultCharacterClass : CharacterClassBase
    {
        public DefaultCharacterClass()
        {
            ID = 0;
            Name = "Unknown";
            BaseName = "Unknown Class";
            Profession = "None";
        }
    }
}
