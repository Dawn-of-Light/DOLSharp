using System;
using DOL.Language;
using DOL.Events;
using DOL.GS.PlayerTitles;

namespace DOL.GS.Scripts
{
	#region RealmRanks Titles
	#region Albion
	public class AlbionRR4 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Gryphon Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=30&&player.RealmLevel<40); }
	}
	public class AlbionRR5 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Eagle Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=40&&player.RealmLevel<50); }
	}
	public class AlbionRR6 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Pheonix Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=50&&player.RealmLevel<60); }
	}
	public class AlbionRR7 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Alerion Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=60&&player.RealmLevel<70); }
	}
	public class AlbionRR8 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Unicorn Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=70&&player.RealmLevel<80); }
	}
	public class AlbionRR9 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Lion Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=80&&player.RealmLevel<90); }
	}
	public class AlbionRR10 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Dragon Knight"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=90&&player.RealmLevel<100); }
	}
	public class AlbionRR11M : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Lord"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=100&&player.RealmLevel<110&&player.PlayerCharacter.Gender==0); }
	}
	public class AlbionRR11F : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Lady"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=100&&player.RealmLevel<110&&player.PlayerCharacter.Gender==1); }
	}
	public class AlbionRR12M : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Baronet"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=110&&player.RealmLevel<120&&player.PlayerCharacter.Gender==0); }
	}
	public class AlbionRR12F : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Baronetess"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Albion&&player.RealmLevel>=110&&player.RealmLevel<120&&player.PlayerCharacter.Gender==1); }
	}
	#endregion Ablion
	#region Hibernia
	public class HiberniaRR4 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Grove Protector"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=30&&player.RealmLevel<40); }
	}
	public class HiberniaRR5 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Raven Ardent"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=40&&player.RealmLevel<50); }
	}
	public class HiberniaRR6 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Silver Hand"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=50&&player.RealmLevel<60); }
	}
	public class HiberniaRR7 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Thunderer"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=60&&player.RealmLevel<70); }
	}
	public class HiberniaRR8 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Gilded Spear"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=70&&player.RealmLevel<80); }
	}
	public class HiberniaRR9 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Tiarna"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=80&&player.RealmLevel<90); }
	}
	public class HiberniaRR10 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Emerald Ridere"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=90&&player.RealmLevel<100); }
	}
	public class HiberniaRR11M : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Barun"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=100&&player.RealmLevel<110&&player.PlayerCharacter.Gender==0); }
	}
	public class HiberniaRR11F : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Banbharun"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=100&&player.RealmLevel<110&&player.PlayerCharacter.Gender==1); }
	}
	public class HiberniaRR12M : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Ard Tiarna"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=110&&player.RealmLevel<120&&player.PlayerCharacter.Gender==0); }
	}
	public class HiberniaRR12F : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Ard Bantiarna"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Hibernia&&player.RealmLevel>=110&&player.RealmLevel<120&&player.PlayerCharacter.Gender==1); }
	}
	#endregion Hibernia
	#region Midgard
	public class MidgardRR4 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Elding Vakten"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=30&&player.RealmLevel<40); }
	}
	public class MidgardRR5 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Stormur Vakten"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=40&&player.RealmLevel<50); }
	}
	public class MidgardRR6 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Isen Herra"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=50&&player.RealmLevel<60); }
	}
	public class MidgardRR7 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Flammen Herra"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=60&&player.RealmLevel<70); }
	}
	public class MidgardRR8 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Elding Herra"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=70&&player.RealmLevel<80); }
	}
	public class MidgardRR9 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Stormur Herra"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=80&&player.RealmLevel<90); }
	}
	public class MidgardRR10 : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Einherjar"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=90&&player.RealmLevel<100); }
	}
	public class MidgardRR11M : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Herra"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=100&&player.RealmLevel<110&&player.PlayerCharacter.Gender==0); }
	}
	public class MidgardRR11F : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Fru"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=100&&player.RealmLevel<110&&player.PlayerCharacter.Gender==1); }
	}
	public class MidgardRR12M : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Hersir"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=110&&player.RealmLevel<120&&player.PlayerCharacter.Gender==0); }
	}
	public class MidgardRR12F : SimplePlayerTitle
	{
		public override string GetDescription(GamePlayer player) { return "Realm Rank"; }
		public override string GetValue(GamePlayer player) { return "Baronsfru"; }
		public override bool IsSuitable(GamePlayer player) { return (player.Realm==eRealm.Midgard&&player.RealmLevel>=110&&player.RealmLevel<120&&player.PlayerCharacter.Gender==1); }
	}
	#endregion Midgard
	#endregion RealmRank Titles
}