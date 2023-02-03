using System;
using System.Linq;
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

        protected DefaultClassBehavior() { }

        public static DefaultClassBehavior Create(GamePlayer player, int classID)
        {
            DefaultClassBehavior behavior;
            switch ((eCharacterClass)classID)
            {
                case eCharacterClass.Necromancer: behavior = new NecromancerClassBehavior(); break;
                case eCharacterClass.Bonedancer: behavior = new BonedancerClassBehavior(); break;
                case eCharacterClass.Warlock: behavior = new WarlockClassBehavior(); break;
                case eCharacterClass.Bainshee: behavior = new BainsheeClassBehavior(); break;
                case eCharacterClass.Animist: behavior = new AnimistClassBehavior(); break;
                default: behavior = new DefaultClassBehavior(); break;
            }
            behavior.Player = player;
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
        /// Releases controlled object
        /// </summary>
        public virtual void CommandNpcRelease()
        {
            IControlledBrain controlledBrain = Player.ControlledBrain;
            if (controlledBrain == null)
                return;

            GameNPC npc = controlledBrain.Body;
            if (npc == null)
                return;

            if (npc is GamePet pet)
                pet.StripBuffs();

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
        public virtual byte HealthPercentGroupWindow
        {
            get
            {
                return Player.HealthPercent;
            }
        }


        /// <summary>
        /// Create a shade effect for this player.
        /// </summary>
        /// <returns></returns>
        protected virtual ShadeEffect CreateShadeEffect()
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
                    Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.Shade.AlreadyShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
}