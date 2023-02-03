
using DOL.AI.Brain;
using DOL.Events;

namespace DOL.GS
{
    public class AnimistClassBehavior : DefaultClassBehavior
	{
        public AnimistClassBehavior(GamePlayer player) : base(player) { }

		public override void CommandNpcRelease()
		{
			TurretPet turretFnF = Player.TargetObject as TurretPet;
			if (turretFnF != null && turretFnF.Brain is TurretFNFBrain && Player.IsControlledNPC(turretFnF))
			{
				Player.Notify(GameLivingEvent.PetReleased, turretFnF);
				return;
			}

			base.CommandNpcRelease();
		}
	}
}