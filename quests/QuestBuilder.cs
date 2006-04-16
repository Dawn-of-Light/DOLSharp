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
using System.Reflection;
using DOL.GS.Database;

namespace DOL.GS.Quests
{
    public class QuestBuilder
    {
        private Type questType;

        private MethodInfo addActionMethod;
        private MethodInfo registerActionMethod;

        public Type QuestType
        {
            get { return questType; }
            set { questType = value; }
        }

        public QuestBuilder(Type questType)
        {
            this.questType = questType;
            this.addActionMethod = questType.GetMethod("AddQuestPart", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            this.registerActionMethod = questType.GetMethod("RegisterQuestPart", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        /// <summary>
        /// Adds an interact QuestPart that will display some sort of text, depending on texttype.
        /// </summary>
        /// <param name="questType">type of quest this interact Questpart should be added to.</param>
        /// <param name="npc">NPC player must interact with</param>
        /// <param name="step">QuestStep Player must have to fire interaction. 
        /// -1 means Player does not have quest defined by questType
        /// 0 means Player has quest defined by questType
        /// >0 meanst Player has quest defined by questType and is at the given QuestStep</param>
        /// <param name="textType"> Type of text should be displayed: Emote, Dialog, Direct Say, Brodcast, ...</param>
        /// <param name="triggerKeyword">Keyword Player must whisper to NPc to fire interaction, (player must click on word in brackets)</param>
        /// <param name="message">Actual message being displayed</param>
        /// <returns>generated BaseQuestPart be altered further via AddTrigger, AddRequirement, AddAction</returns>
        public BaseQuestPart AddInteraction(GameNPC npc, int step, eTextType textType, String triggerKeyword, String message)
        {
            return AddInteraction(npc, step, step, textType, triggerKeyword, message);            
        }

        /// <summary>
        /// Adds an interact QuestPart that will display some sort of text, depending on texttype.
        /// </summary>
        /// <param name="questType">type of quest this interact Questpart should be added to.</param>
        /// <param name="npc">NPC player must interact with</param>
        /// <param name="step">QuestStep Player must have to fire interaction. 
        /// -1 means Player does not have quest defined by questType
        /// 0 means Player has quest defined by questType
        /// >0 meanst Player has quest defined by questType and is at the given QuestStep</param>
        /// <param name="textType"> Type of text should be displayed: Emote, Dialog, Direct Say, Brodcast, ...</param>
        /// <param name="triggerKeyword">Keyword Player must whisper to NPc to fire interaction, (player must click on word in brackets)</param>
        /// <param name="message">Actual message being displayed</param>
        /// <returns>generated BaseQuestPart be altered further via AddTrigger, AddRequirement, AddAction</returns>
        public BaseQuestPart AddInteraction(GameNPC npc, int minstep, int maxstep, eTextType textType, String triggerKeyword, String message)
        {            
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);

            eTriggerType triggerType = (triggerKeyword == null) ? eTriggerType.Interact : eTriggerType.Whisper;
            questPart.AddTrigger(triggerType, triggerKeyword);            

            if (minstep != maxstep)
            {
                questPart.AddRequirement(eRequirementType.QuestStep, QuestType, minstep - 1, eComparator.Greater);
                questPart.AddRequirement(eRequirementType.QuestStep, QuestType, maxstep + 1, eComparator.Less);
            }
            else
            {
                int step = minstep;
                if (step > 0)
                    questPart.AddRequirement(eRequirementType.QuestStep, questType, step, eComparator.Equal);
                else if (step == 0)
                    questPart.AddRequirement(eRequirementType.QuestPending, questType);
                else
                {
                    questPart.AddRequirement(eRequirementType.QuestPending, questType, null, eComparator.Not);
                    questPart.AddRequirement(eRequirementType.QuestGivable, questType);
                }
            }

			questPart.AddAction(eActionType.Message, textType, message);

            AddQuestPart(questPart);
            return questPart;
        }

        public BaseQuestPart AddOnGiveItem(GameNPC npc, int minstep, int maxstep, GenericItemTemplate item, eTextType textType, String message)
        {
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);

            questPart.AddTrigger(eTriggerType.GiveItem, null, item);            

            if (minstep != maxstep)
            {
                questPart.AddRequirement(eRequirementType.QuestStep, QuestType, minstep - 1, eComparator.Greater);
                questPart.AddRequirement(eRequirementType.QuestStep, QuestType, maxstep + 1, eComparator.Less);
            }
            else
            {
                int step = minstep;
                if (step > 0)
                    questPart.AddRequirement(eRequirementType.QuestStep, questType, step, eComparator.Equal);
                else if (step == 0)
                    questPart.AddRequirement(eRequirementType.QuestPending, questType);
                else
                {
                    questPart.AddRequirement(eRequirementType.QuestPending, questType, null, eComparator.Not);
                    questPart.AddRequirement(eRequirementType.QuestGivable, questType);
                }
            }

			questPart.AddAction(eActionType.Message, textType, message);

            AddQuestPart(questPart);
            return questPart;
        }

        public BaseQuestPart AddOnGiveItem(GameNPC npc, int step, GenericItemTemplate item, eTextType textType,  String message)
        {                        
            return AddOnGiveItem(npc, step, step, item, textType, message);
        }

        public BaseQuestPart AddOnQuestContinue(GameNPC npc, eTextType textType, String message)
        {           
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);

            questPart.AddTrigger(eTriggerType.ContinueQuest, null, QuestType);
            questPart.AddRequirement(eRequirementType.QuestPending, QuestType);
			questPart.AddAction(eActionType.Message, textType, message);
            AddQuestPart(questPart);
            return questPart;
        }

        public BaseQuestPart AddOnQuestAbort(GameNPC npc, eTextType textType, String message)
        {
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);

            questPart.AddTrigger(eTriggerType.AbortQuest, null, QuestType);
            questPart.AddRequirement(eRequirementType.QuestPending, QuestType);            
            questPart.AddAction(eActionType.AbortQuest, QuestType);
			questPart.AddAction(eActionType.Message, textType, message);

            AddQuestPart(questPart);
            return questPart;
        }

        public BaseQuestPart AddOnQuestAccept(GameNPC npc, eTextType textType, String message)
        {
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);

            questPart.AddRequirement(eRequirementType.QuestPending, QuestType, eComparator.Not);
            questPart.AddRequirement(eRequirementType.QuestGivable, QuestType);
            questPart.AddTrigger(eTriggerType.AcceptQuest, null, QuestType);
            questPart.AddAction(eActionType.GiveQuest, QuestType);
			questPart.AddAction(eActionType.Message, textType, message);

            AddQuestPart(questPart);
            return questPart;
        }

        public BaseQuestPart AddOnQuestDecline(GameNPC npc, eTextType textType, String message)
        {
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);

            questPart.AddRequirement(eRequirementType.QuestPending, QuestType, eComparator.Not);
            questPart.AddTrigger(eTriggerType.DeclineQuest, null, QuestType);
			questPart.AddAction(eActionType.Message, textType, message);

            AddQuestPart(questPart);
            return questPart;
        }
        

        public void AddQuestPart(BaseQuestPart questPart)
        {            
            addActionMethod.Invoke(null, new object[] { questPart });
        }

        public void RegisterQuestPart(BaseQuestPart questPart)
        {
            registerActionMethod.Invoke(null, new object[] { questPart });
        }

        public BaseQuestPart CreateQuestPart(GameNPC npc)
        {
            BaseQuestPart questPart =  new BaseQuestPart(questType, npc);            
            return questPart;
        }

        public BaseQuestPart CreateQuestPart(GameNPC npc, eTextType textType, string message)
        {
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);
			questPart.AddAction(eActionType.Message, textType, message);
            return questPart;
        }

        public BaseQuestPart CreateQuestPart(GameNPC npc, int step, eTextType textType, string message)
        {
            BaseQuestPart questPart = new BaseQuestPart(questType, npc);                        

            if (step > 0)            
                questPart.AddRequirement(eRequirementType.QuestStep, questType, step, eComparator.Equal);            
            else if (step == 0)
                questPart.AddRequirement(eRequirementType.QuestPending, questType);            
            else
            {
                questPart.AddRequirement(eRequirementType.QuestPending, questType, null, eComparator.Not);
                questPart.AddRequirement(eRequirementType.QuestGivable, questType);
            }
			questPart.AddAction(eActionType.Message, textType, message);

            return questPart;
        }
    }
}
