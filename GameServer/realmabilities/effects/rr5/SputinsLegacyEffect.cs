using System.Collections;
using DOL.Events;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Effects
{
    /// <summary>
    /// Helper for charge realm ability
    /// </summary>
    public class SputinsLegacyEffect : TimedEffect
    {
		public const string SPUTINSLEGACYHASRES = "sputinslegacyhasres";

        /// <summary>
        /// Creates a new effect
        /// </summary>
        public SputinsLegacyEffect() : base(30000) { }

        /// <summary>
        /// Start the effect on player
        /// </summary>
        /// <param name="target">The effect target</param>
        public override void Start(GameLiving target)
        {
            base.Start(target);
            GameEventMgr.AddHandler(target, GameLivingEvent.Dying, new DOLEventHandler(OnDeath));
			Owner.TempProperties.setProperty(SPUTINSLEGACYHASRES, true);
        }

        private void OnDeath(DOLEvent e, object sender, EventArgs args)
        {
            ((GamePlayer)Owner).Out.SendMessage("Sputins Legacy grants you a self resurrection!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
        }

        public override void Stop()
        {
            GameEventMgr.RemoveHandler(Owner, GameLivingEvent.Dying, new DOLEventHandler(OnDeath));
			Owner.TempProperties.removeProperty(SPUTINSLEGACYHASRES);
            base.Stop();
        }

        /// <summary>
        /// Name of the effect
        /// </summary>
        public override string Name { get { return "Sputins Legacy"; } }

        /// <summary>
        /// Icon to show on players, can be id
        /// </summary>
        public override ushort Icon { get { return 3069; } }

        /// <summary>
        /// Delve Info
        /// </summary>
        public override IList DelveInfo
        {
            get
            {
                IList list = new ArrayList();
                list.Add("Healer can insta-cast a resurrect buff on themselves. Buff lasts 30 seconds. If the healer dies while buff is up, they have the option to /resurrect themselves anytime within 10 minutes after death with 10% H/E/P. The healer must wait 10 seconds before /resurrecting themselves.");
                list.Add(base.DelveInfo);
                return list;
            }
        }
    }
}