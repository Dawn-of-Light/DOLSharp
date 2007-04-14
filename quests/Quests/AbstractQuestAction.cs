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
using System.Text;
using DOL.Events;
using DOL.GS.Quests.Attributes;
using System.Reflection;
using log4net;

namespace DOL.GS.Quests
{
    /// <summary>
    /// If one trigger and all requirements are fulfilled the corresponding actions of
    /// a QuestAction will we executed one after another. Actions can be more or less anything:
    /// at the moment there are: GiveItem, TakeItem, Talk, Give Quest, Increase Quest Step, FinishQuest,
    /// etc....
    /// </summary>
    public abstract class AbstractQuestAction<TypeP,TypeQ> : IQuestAction
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private eActionType actionType;
        private TypeQ q;
        private TypeP p;
		private GameNPC defaultNPC;
		private Type questType;
        
        /// <summary>
        /// The action type
        /// </summary>
        public eActionType ActionType
        {
            get { return actionType; }
        }
        /// <summary>
        /// First Action Variable
        /// </summary>
        public TypeP P
        {
            get { return p; }
			set { p = value; }
        }
        /// <summary>
        /// Second Action Variable
        /// </summary>
        public TypeQ Q
        {
            get { return q; }
			set {q = value;}
        }

		/// <summary>
		/// returns the NPC of the action
		/// </summary>
		public GameNPC NPC
		{
			get { return defaultNPC; }
		}

		/// <summary>
		/// Gets the type of the quest.
		/// </summary>
		/// <value>The type of the quest.</value>
		public Type QuestType
		{
			get { return questType; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractQuestAction"/> class.
		/// </summary>
		/// <param name="questPart">The quest part.</param>
		/// <param name="actionType">Type of the action.</param>
		/// <param name="p">The parameter p.</param>
		/// <param name="q">The parameter q.</param>
        public AbstractQuestAction(BaseQuestPart questPart, eActionType actionType, Object p, Object q)
        :this (questPart.NPC,questPart.QuestType,actionType,p,q) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractQuestAction"/> class.
		/// </summary>
		/// <param name="npc">The default NPC.</param>
		/// <param name="questType">Default Type of the quest.</param>
		/// <param name="actionType">Type of the action.</param>
		/// <param name="p">The parameter p.</param>
		/// <param name="q">The parameter q.</param>
		public AbstractQuestAction(GameNPC npc,Type questType, eActionType actionType, Object p, Object q)
		{
			this.defaultNPC = npc;
			this.questType = questType;
			this.actionType = actionType;

            QuestActionAttribute attr = QuestMgr.GetQuestActionAttribute(this.GetType());

            // handle parameter P
            object defaultValueP = null;
            if (attr.DefaultValueP != null)
            {
                if (attr.DefaultValueP is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)attr.DefaultValueP)
                    {
                        case eDefaultValueConstants.QuestType:
                            defaultValueP = QuestType;
                            break;
                        case eDefaultValueConstants.NPC:
                            defaultValueP = NPC;
                            break;
                    }
                }
                else
                {
                    defaultValueP = attr.DefaultValueP;
                }
            }
            this.p = (TypeP)QuestPartUtils.ConvertObject(p, defaultValueP, typeof(TypeP));

            if (typeof(TypeP) == typeof(Unused))
            {
                if (this.p != null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Parameter P is not used for ActionType=" + attr.ActionType + ".\n The recieved parameter " + p + " will not be used for anthing. Check your quest code for inproper usage of parameters!");
                }
            }
            else
            {
                if (!attr.IsNullableP && this.p == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Not nullable parameter P was null, expected type is " + typeof(TypeP).Name + "for ActionType=" + attr.ActionType + ".\nRecived parameter was " + p + " and DefaultValue for this parameter was " + attr.DefaultValueP);
                }
                if (this.p != null && !(this.p is TypeP))
                {
                    if (log.IsErrorEnabled)
                        log.Error("Parameter P was not of expected type, expected type is " + typeof(TypeP).Name + "for ActionType=" + attr.ActionType + ".\nRecived parameter was " + p + " and DefaultValue for this parameter was " + attr.DefaultValueP);
                }
            }


            // handle parameter Q
            object defaultValueQ = null;
            if (attr.DefaultValueQ != null)
            {
                if (attr.DefaultValueQ is eDefaultValueConstants)
                {
                    switch ((eDefaultValueConstants)attr.DefaultValueQ)
                    {
                        case eDefaultValueConstants.QuestType:
                            defaultValueQ = QuestType;
                            break;
                        case eDefaultValueConstants.NPC:
                            defaultValueQ = NPC;
                            break;
                    }
                }
                else
                {
                    defaultValueQ = attr.DefaultValueQ;
                }
            }
            this.q = (TypeQ)QuestPartUtils.ConvertObject(q, defaultValueQ, typeof(TypeQ));

            if (typeof(TypeQ) == typeof(Unused))
            {
                if (this.q != null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Parameter Q is not used for ActionType=" + attr.ActionType + ".\n The recieved parameter " + q + " will not be used for anthing. Check your quest code for inproper usage of parameters!");
                }
            }
            else
            {
                if (!attr.IsNullableQ && this.q == null)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Not nullable parameter Q was null, expected type is " + typeof(TypeQ).Name + "for ActionType=" + attr.ActionType + ".\nRecived parameter was " + q + " and DefaultValue for this parameter was " + attr.DefaultValueQ);
                }
                if (this.q != null && !(this.q is TypeQ))
                {
                    if (log.IsErrorEnabled)
                        log.Error("Parameter Q was not of expected type, expected type is " + typeof(TypeQ).Name + "for ActionType=" + attr.ActionType + ".\nRecived parameter was " + q + " and DefaultValue for this parameter was " + attr.DefaultValueQ);
                }
            }
		}

        /// <summary>
        /// Action performed 
        /// Can be used in subclasses to define special behaviour of actions
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>        
        public abstract void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player);
        
    }
}
