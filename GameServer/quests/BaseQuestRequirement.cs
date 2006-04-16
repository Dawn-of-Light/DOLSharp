using System;
using System.Text;
using DOL.Events;
using DOL.GS.Database;
using log4net;
using System.Reflection;
using DOL.GS.Scripts;

namespace DOL.GS.Quests
{

	/// <summary>
	/// Requirementtype defines a list of requirements to be used with questparts.
	/// Depending on requirmenttype V and N will have special
	/// meaning look at documentation of each requirementtype for details    
	/// </summary>
	///<remarks>
	/// Syntax: ... V:eEmote(eEmote.Yes) ... Parameter V must be of Type
	/// eEmote and has the default value of "eEmote.Yes" (used if no value is passed).
	/// If no default value is defined value must be passed along with action.
	/// </remarks>
	public enum eRequirementType : byte
	{
		/// <summary>
		/// No Requirement
		/// </summary>
		/// <remarks>Tested</remarks>
		None,
		/// <summary>
		/// AINV : checks inventory for V:int(1)[string] instances of item N:ItemTemplate[Item's Id_nb:string]
		/// </summary>
		/// <remarks>Tested</remarks>
		InventoryItem,
		// AREQ : checks requirement list r##n# (requirement lists described further)
		/// <summary>
		/// CLAS : checks for player class N:int[string]
		/// </summary>
		Class,
		/// <summary>
		/// COMB : checks for player combat skill N at level V:int[string]
		/// </summary>
		//CombatSkill = 0x04,
		/// <summary>
		/// CQST : checks for quest N:Type[Typename:string](Current Quest) completed V:int[string] times
		/// </summary>
		Quest,
		/// <summary>
		/// ENCU : compares player's encumbrance value with N:int[string]
		/// </summary>
		Encumbrance,
		/// <summary>
		/// MENC : compares player's maximum encumbrance value with N:int[string]
		/// </summary>
		EncumbranceMax,
		/// <summary>
		/// FACT : checks for faction N at level N
		/// </summary>
		//Faction=0x07,	
		/// <summary>
		/// FATG : compares player's fatigue value with N:int[string] using eComparator
		/// </summary>
		Endurance,
		/// <summary>
		/// MFAT : compares player's maximum fatigue value with N:int[string] using eComparator
		/// </summary>
		EnduranceMax,
		/// <summary>
		/// GLEV : compares guild level with N:int[string] (not player's guilds) using eComparator
		/// </summary>
		//GuildLevel=0x09,
		/// <summary>
		/// GNDR : compares player's gender with N:int[string] {0=male,1=female}
		/// </summary>
		Gender,
		/// <summary>
		/// GRUP : checks if player is grouped. N:int[string] is number of people in group
		/// </summary>
		GroupNumber,
		/// <summary>
		/// GPLV : checks if player is grouped. N:int[string] is sum of levels in group
		/// </summary>
		GroupLevel,
		/// <summary>
		/// GUIL : compares guildname of N:GameLiving(NPC)[NPC's ID:string] with V:string.
		/// </summary>
		Guild,
		/// <summary>
		/// Compares players gold with N:long[string] using eComparator
		/// </summary>
		Gold,
		/// <summary>
		/// HITS : compares player's current hit points with N:int[string] using eComparator
		/// </summary>
		Health,
		/// <summary>
		/// HITS : compares player's maximum hit points with N:int[string] using eComparator
		/// </summary>
		HealthMax,
		/// <summary>
		/// LEVE : compares player's level with N:int[string] using eComparator
		/// </summary>
		Level,
		/// <summary>
		/// POWR: compares player's current mana value with N:int[string] using eComparator
		/// </summary>
		Mana,
		/// <summary>
		/// MPOW: compares player's maximum mana value with N:int[string] using eComparator
		/// </summary>
		ManaMax,
		/// <summary>
		/// PQST : checks for player's pending quest N:Type[Typename:string](Current Quest)
		/// </summary>
		/// <remarks>Tested</remarks>
		QuestPending,
		/// <summary>
		/// RACE : compares player's race with N:int[string]
		/// </summary>
		Race,
		/// <summary>
		/// RAND : percent random chance indicated by N:int[string]
		/// </summary>
		Random,
		/// <summary>
		/// REAL : compares player's realm with N:int[string]
		/// </summary>
		Realm,
		/// <summary>
		/// RLEV : compares player's realm level with N:int[string] using eComparator
		/// </summary>
		RealmLevel,
		/// <summary>
		/// RPTS : compares player's realm points with N:long[string] using eComparator
		/// </summary>
		RealmPoints,
		/// <summary>
		/// REGO : compare player's zone with N:int[string] and region with V:int[string]
		/// </summary>
		Region,
		/// <summary>
		/// RINV : checks readied/worn items of player for item N:ItemTemplate[Item's Id_nb:string]
		/// </summary>
		EquippedItem,
		/*
		SHOU : checks for shout r##n# at value r##v#
		SKIL : checks for skill r##n# at level r##v#
		SONG : checks for song r##n# at value r##v#
		SPEL : checks for spell r##n# at value r##v#
		STAT : checks for stat r##n# at value r##v#
		STYL : checks for style r##n# at level r##v#
		*/
		//TIMR : checks when timer indicated by r##n# meet time r##v# (player side)
		/// <summary>
		/// QUES : checks for player's quest N:Type[Typename:string](Current Quest) at step V:int[string] using eComparator
		/// </summary>
		/// <remarks>Tested</remarks>
		QuestStep,
		/// <summary>
		/// Checks for quest N:Type[Typename:string](Current Quest) to be givable by NPC to player
		/// </summary>
		/// <remarks>Tested</remarks>
		QuestGivable,
		/// <summary>
		/// Compares distance V:int[string] between player and given GameObject N:GameLiving[GameLiving's Name:string](NPC) using Comparator
		/// </summary>
		/// <remarks>Tested</remarks>
		Distance
	}

	/// <summary>
	/// Comparator enume used within some of the requirement checks.
	/// </summary>
	public enum eComparator : byte
	{
		/// <summary>
		/// No check is done, will always return true
		/// </summary>
		None = 0,
		/// <summary>
		/// Checks wether given value1 is less than value2
		/// </summary>
		Less = 1,
		/// <summary>
		/// Checks wether given value1 is greater than value2
		/// </summary>
		Greater = 2,
		/// <summary>
		///  Checks wether given value1 is equal value2
		/// </summary>
		Equal = 3,
		/// <summary>
		/// Checks wether given value1 is not equal value2
		/// </summary>
		NotEqual = 4,
		/// <summary>
		/// Negotiation of given argument
		/// usable with QuestPending, QuestGivable
		/// </summary>
		Not = 5
	}
	 
	/// <summary>
	/// Requirements describe what must be true to allow a QuestAction to fire.
	/// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
	/// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
	/// </summary>
	public class BaseQuestRequirement : AbstractQuestRequirement
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
		/// </summary>
		/// <param name="questPart">Parent QuestPart of this Requirement</param>
		/// <param name="type">RequirementType</param>
		/// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
		/// <param name="comp">Comparator used if some values are veeing compared</param>
		public BaseQuestRequirement(BaseQuestPart questPart, eRequirementType type, Object n, Object v, eComparator comp): base(questPart, type, n, v, comp)
		{        

			switch (RequirementType)
			{
				case eRequirementType.InventoryItem:
					{
						if (N is string)
							N = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), Convert.ToString(N));
						
						if (!(N is GenericItemTemplate))
							throw new ArgumentException("Variable N must be itemTemplate for eRequirementType.InventoryItem", "N");
						break;
					}
				case eRequirementType.Class:
					{
						if (N is string)
							N = Convert.ToInt32(N);						
						
						if (!(N is int))
							throw new ArgumentException("Variable N must be charachterclassid for eRequirementType.Class", "N");
						break;
					}
				case eRequirementType.QuestStep:					
				case eRequirementType.Quest:
					{
						if (N is string)
							N = ScriptMgr.GetType(Convert.ToString(N));

						if (!(N is Type))
							N = QuestType;

						if (V is string)
							V = Convert.ToInt32(V);

						if (!(V is int))
							throw new ArgumentException("Variable V must be queststep(int) for eRequirementType.QuestStep", "V");
						break;						
					}
				case eRequirementType.QuestPending:					
				case eRequirementType.QuestGivable:
					if (N is string)
						N = ScriptMgr.GetType(Convert.ToString(N));
					
					if (!(N is Type))
						N = QuestType;

					break;
				case eRequirementType.RealmPoints:
					if (N is string)
						N = Convert.ToInt64(N);
					if (!(N is long))
						throw new ArgumentException("Variable N must be long for eRequirementType:" + RequirementType, "N");
					break;
				case eRequirementType.Realm:
				case eRequirementType.RealmLevel:
				case eRequirementType.Encumbrance:
				case eRequirementType.EncumbranceMax:
				case eRequirementType.Endurance:
				case eRequirementType.EnduranceMax:
				case eRequirementType.Mana:
				case eRequirementType.ManaMax:
					if (N is string)
						N = Convert.ToInt32(N);
					if (!(N is int))
						throw new ArgumentException("Variable N must be int for eRequirementType:" + RequirementType, "N");
					break;
				case eRequirementType.EquippedItem:
					if (N is string)
						N = (GenericItemTemplate)GameServer.Database.FindObjectByKey(typeof(GenericItemTemplate), Convert.ToString(N));
					if (!(N is GenericItemTemplate))
						throw new ArgumentException("Variable N must be ItemTemplate to look for in eRequirementType.EquippedItem", "N");
					break;
				case eRequirementType.Gender:
				case eRequirementType.GroupLevel:
				case eRequirementType.GroupNumber:
					if (N is string)
						N = Convert.ToInt32(N);
					if (!(N is int))
						throw new ArgumentException("Variable N must be int for eRequirementType.GroupNumber", "N");
					break;
				case eRequirementType.Guild:
					N = QuestMgr.ResolveNPC(N);

					if (!(V is string))
						throw new ArgumentException("Variable V must be string for requirementType " + RequirementType, "V");
					break;
				case eRequirementType.Gold:
					if (N is string)
						N = Convert.ToInt64(N);
					if (!(N is long))
						throw new ArgumentException("Variable N must be copper(long) for requirementType " + RequirementType, "N");
					break;
				case eRequirementType.Health:
				case eRequirementType.HealthMax:
				case eRequirementType.Level:
				case eRequirementType.Race:
				case eRequirementType.Random:
					if (N is string)
						N = Convert.ToInt32(N);
					if (!(N is int))
						throw new ArgumentException("Variable N must be region(int) for requirementType " + RequirementType, "N");
					break;
				case eRequirementType.Region:
					if (V is string)
						V = Convert.ToInt32(V);					
					if (!(V is int))
						throw new ArgumentException("Variable V must be region(int) for requirementType " + RequirementType, "V");
					
					if (N is string)
						N = Convert.ToInt32(N);
					if (!(N is int))
						throw new ArgumentException("Variable N must be zone(int) for requirementType " + RequirementType, "N");
					break;
				case eRequirementType.Distance:
					
					N = QuestMgr.ResolveLiving(N,NPC);									
					
					if (V is string)
						V = Convert.ToInt32(V);
					if (!(V is int))
						throw new ArgumentException("Variable V must be int for requirementType " + RequirementType, "V");
					break;
			}
		}

		/// <summary>
		/// Checks the added requirement whenever a trigger associated with this questpart fires.(returns true)
		/// </summary>        
		/// <param name="e">DolEvent of notify call</param>
		/// <param name="sender">Sender of notify call</param>
		/// <param name="args">EventArgs of notify call</param>
		/// <param name="player">GamePlayer this call is related to, can be null</param>
		/// <returns>true if all Requirements forQuestPart where fullfilled, else false</returns>
		public override bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player)
		{
			bool result = true;
	        
			switch (RequirementType)
			{
				case eRequirementType.InventoryItem:
					{
						GenericItemTemplate item = N as GenericItemTemplate;
						int number = V != null ? Convert.ToInt32(V) : 1;
						
						int count =0;
						foreach (GenericItem invItem in player.Inventory.AllItems)
						{
							if (invItem.Name == item.Name)
							{
								if (invItem is StackableItem)
								{
									count += ((StackableItem)invItem).Count;
								}
								else
								{
									count++;
								}
							}
						}

						result &= count >= number;
						break;
					}
				case eRequirementType.Class:
					{
						int characterClass = Convert.ToInt32(N);
						result &= (player.CharacterClass.ID == characterClass);
						break;
					}
				case eRequirementType.QuestStep:
					{

						Type requirementQuestType = (Type)N;
						AbstractQuest playerQuest = player.IsDoingQuest(requirementQuestType) as AbstractQuest;

						if (playerQuest != null)
						{
							result &= compare(playerQuest.Step, Convert.ToInt16(V), Comparator);
						}
						else
						{
							result = false;
						}
						break;
					}
				case eRequirementType.Quest:
					{
						Type requirementQuestType = (Type)N;
						int finishedCount = player.HasFinishedQuest(requirementQuestType);
						int maximumCount = Convert.ToInt16(V);
						result &= finishedCount < maximumCount;
						break;
					}
				case eRequirementType.QuestPending:
					{
						Type requirementQuestType = (Type)N;
						if (Comparator == eComparator.Not)
							result &= player.IsDoingQuest(requirementQuestType) == null;
						else
							result &= player.IsDoingQuest(requirementQuestType) != null;
						break;
					}
				case eRequirementType.QuestGivable:
					{
						Type requirementQuestType = (Type)N;
						if (Comparator == eComparator.Not)
							result &= QuestMgr.CanGiveQuest(requirementQuestType, player, NPC) <= 0;
						else
							result &= QuestMgr.CanGiveQuest(requirementQuestType, player, NPC) > 0;
						break;
					}
				case eRequirementType.Guild:
					{
						GameLiving living = (N is GameLiving) ? (GameLiving)N : NPC;
						result &= (living.GuildName == Convert.ToString(V));
						break;
					}
				case eRequirementType.Gold:
					result &= compare(player.Money, Convert.ToInt64(N), Comparator);
					break;
				case eRequirementType.Encumbrance:
					result &= compare(player.Encumberance, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.EncumbranceMax:
					result &= compare(player.MaxEncumberance, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.Endurance:
					result &= compare(player.EndurancePercent, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.Mana:
					result &= compare(player.Mana, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.ManaMax:
					result &= compare(player.MaxMana, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.EquippedItem:
					{
						GenericItemTemplate item = (GenericItemTemplate)N;
						result &= player.Inventory.GetFirstItemByName(item.Name, eInventorySlot.Min_Inv, eInventorySlot.FirstBackpack - 1) != null;
						break;
					}
				case eRequirementType.Gender:
					result &= compare(player.Gender, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.GroupLevel:
					PlayerGroup group = player.PlayerGroup;
					int grouplevel = 0;
					if (group != null)
					{
						foreach (GamePlayer member in group.GetPlayersInTheGroup())
						{
							grouplevel += member.Level;
						}
					}
					else
					{
						grouplevel += player.Level;
					}
					result &= compare(grouplevel, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.GroupNumber:
					group = player.PlayerGroup;
					int groupcount = 0;
					if (group != null)
					{
						groupcount = group.PlayerCount;
					}
					result &= compare(groupcount, Convert.ToInt32(N), Comparator);
					break;
				//case eRequirementType.CombatSkill:
				//TODO
				//break;
				//case eRequirementType.Faction:
				//TODO
				//break;
				case eRequirementType.Health:					
					result &= compare(player.Health, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.HealthMax:
					result &= compare(player.MaxHealth, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.Level:
					result &= compare(player.Level, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.Race:
					result &= player.Race == Convert.ToInt32(N);
					break;
				case eRequirementType.Random:
					result &= Util.Chance(Convert.ToInt32(N));
					break;
				case eRequirementType.Realm:
					result &= compare(player.Realm, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.RealmLevel:
					result &= compare(player.RealmLevel, Convert.ToInt32(N), Comparator);
					break;
				case eRequirementType.RealmPoints:
					result &= compare(player.RealmPoints, Convert.ToInt64(N), Comparator);
					break;
				case eRequirementType.Region:
					result &= (player.Region.RegionID == Convert.ToInt32(V) && player.Region.GetZone(player.Position).ZoneID == Convert.ToInt32(N));
					break;
				case eRequirementType.Distance:
					{
						GameObject obj = (GameObject)N;
                        int distance = Convert.ToInt32(V);
                        result &= compare(player.Position.GetDistance(obj.Position), distance, Comparator);
                        break;
					}
			}

			return result;
		}

		/// <summary>
		/// Compares value1 with value2 
		/// Allowed Comparators: Less,Greater,Equal, NotEqual, None
		/// </summary>
		/// <param name="value1">Value1 one to compare</param>
		/// <param name="value2">Value2 to cmopare</param>
		/// <param name="comp">Comparator to use for Comparison</param>
		/// <returns>result of comparison</returns>
		private static bool compare(long value1, long value2, eComparator comp)
		{
			switch (comp)
			{
				case eComparator.Less:
					return (value1 < value2);
				case eComparator.Greater:
					return (value1 > value2);
				case eComparator.Equal:
					return (value1 == value2);
				case eComparator.NotEqual:
					return (value1 != value2);
				case eComparator.None:
					return true;
				default:
					throw new ArgumentException("Comparator not supported:" + comp, "comp");
			}
		}

		/// <summary>
		/// Compares value1 with value2 
		/// Allowed Comparators: Less,Greater,Equal, NotEqual, None
		/// </summary>
		/// <param name="value1">Value1 one to compare</param>
		/// <param name="value2">Value2 to cmopare</param>
		/// <param name="comp">Comparator to use for Comparison</param>
		/// <returns>result of comparison</returns>
		private static bool compare(int value1, int value2, eComparator comp)
		{
			return compare((long)value1, (long)value2, comp);
		}
		/*
        private Type m_linkedQuestType;
        public override Type LinkedQuestType
        {
            get { return m_linkedQuestType; }
        }
		 */
    }
}
