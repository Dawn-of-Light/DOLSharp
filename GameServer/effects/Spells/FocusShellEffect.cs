using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;

namespace DOL.GS.Effects
{
	public class FocusShellEffect : GameSpellEffect
	{
		public FocusShellEffect(ISpellHandler handler, int duration, int pulseFreq, double effectiveness) : base(handler, duration, pulseFreq, effectiveness) { }

		/// <summary>
		/// There is no duration!
		/// </summary>
		public new int RemainingTime
		{
			get
			{
				return 1;
			}
		}
	}
}
