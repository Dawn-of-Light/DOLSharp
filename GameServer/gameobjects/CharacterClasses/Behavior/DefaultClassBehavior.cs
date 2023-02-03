using System;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
    public class DefaultClassBehavior
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GamePlayer Player { get; private set; }

        protected DefaultClassBehavior(GamePlayer player)
        {
            Player = player;
            if (Player != null) Init();
        }

        public static DefaultClassBehavior Create(GamePlayer player, int classID)
        {
            DefaultClassBehavior behavior;
            switch ((eCharacterClass)classID)
            {
                case eCharacterClass.Necromancer: behavior = new NecromancerClassBehavior(player); break;
                case eCharacterClass.Bonedancer: behavior = new BonedancerClassBehavior(player); break;
                case eCharacterClass.Warlock: behavior = new WarlockClassBehavior(player); break;
                case eCharacterClass.Bainshee: behavior = new BainsheeClassBehavior(player); break;
                default: behavior = new DefaultClassBehavior(player); break;
            }
            return behavior;
        }

        public virtual void Init() { }

        /// <summary>
        /// Add all spell-lines and other things that are new when this skill is trained
        /// </summary>
        public virtual void OnSkillTrained(GamePlayer player, Specialization skill)
        {
        }

        public virtual void SetControlledBrain(IControlledBrain controlledBrain)
        {
            if (controlledBrain == Player.ControlledBrain) return;
            if (controlledBrain == null)
            {
                Player.Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
                Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget2", Player.ControlledBrain.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
            else
            {
                if (controlledBrain.Owner != Player)
                    throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Player.Name + ", owner=" + controlledBrain.Owner.Name + ")", "controlledNpc");
                if (Player.ControlledBrain == null)
                    Player.InitControlledBrainArray(1);
                Player.Out.SendPetWindow(controlledBrain.Body, ePetWindowAction.Open, controlledBrain.AggressionState, controlledBrain.WalkState);
                if (controlledBrain.Body != null)
                {
                    Player.Out.SendNPCCreate(controlledBrain.Body); // after open pet window again send creation NPC packet
                    if (controlledBrain.Body.Inventory != null)
                        Player.Out.SendLivingEquipmentUpdate(controlledBrain.Body);
                }
            }

            Player.ControlledBrain = controlledBrain;
        }

        /// <summary>
        /// Return the health percent of this character
        /// </summary>
        public virtual byte HealthPercentGroupWindow
        {
            get
            {
                return Player.HealthPercent;
            }
        }

        public virtual void Notify(DOLEvent e, object sender, EventArgs args)
        {
        }

        public virtual bool CanChangeCastingSpeed(SpellLine line, Spell spell)
        {
            return true;
        }
    }
}