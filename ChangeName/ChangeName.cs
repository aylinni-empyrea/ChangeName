/*
 * Credit to Simon311 for original plugin.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ChangeName
{
    [ApiVersion(1, 25)]

	public class ChangeName : TerrariaPlugin
	{
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}
		public override string Name
		{
			get { return "ChangeName"; }
		}
		public override string Author
		{
			get { return "Zaicon"; }
		}
		public override string Description
		{
			get { return "Changing names"; }
		}

		public ChangeName(Main game)
			: base(game)
		{
			Order = -1;
		}

		public override void Initialize()
		{
			Commands.ChatCommands.Add(new Command("changenames", ChanName, "chname"));
			Commands.ChatCommands.Add(new Command("oldnames", OldName, "oldname"));
			Commands.ChatCommands.Add(new Command("selfname", SelfName, "selfname"));
			//Commands.ChatCommands.Add(new Command("tshock.canchat", Chat, "chat"));
		}

		private void ChanName(CommandArgs args)
		{
			if (args.Player == null)
				return;

			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /chname [player] [newname]");
				return;
			}

			var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
			if (foundplr.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
				return;
			}
			else if (foundplr.Count > 1)
			{
				TShock.Utils.SendMultipleMatchError(args.Player, foundplr.Select(p => p.Name));
				return;
			}

			string newName = args.Parameters[1];
			bool hidden = args.Parameters.Count > 2;

			#region Checks
			if (newName.Length < 2)
			{
				args.Player.SendMessage("A name must be at least 2 characters long.", Color.DeepPink);
				return;
			}

			if (newName.Length > 20)
			{
				args.Player.SendMessage("A name must not be longer than 20 characters.", Color.DeepPink);
				return;
			}

			List<TSPlayer> SameName = TShock.Players.Where(player => (player != null && player.Name == newName)).ToList();
			if (SameName.Count > 0)
			{
				args.Player.SendMessage("This name is taken by another player.", Color.DeepPink);
				return;
			}
			#endregion Checks

			var plr = foundplr[0];
			string oldName = plr.TPlayer.name;
			if (!args.Player.ContainsData("oldname"))
				args.Player.SetData("oldname", oldName);
			if (!hidden)
				TShock.Utils.Broadcast(string.Format("{0} has changed {1}'s name to {2}.", args.Player.Name, oldName, newName), Color.DeepPink);
			else
				args.Player.SendMessage(string.Format("You have secretly changed {0}'s name to {1}.", oldName, newName), Color.DeepPink);
			plr.TPlayer.name = newName;
			NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, plr.TPlayer.name, args.Player.Index, 0, 0, 0, 0);
		}

		private void SelfName(CommandArgs args)
		{
			if (args.Player == null) return;
			
			var plr = args.Player;
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /selfname [newname]");
				return;
			}
			string newName = String.Join(" ", args.Parameters).Trim();

			#region Checks
			if (newName.Length < 2)
			{
				args.Player.SendMessage("A name must be at least 2 characters long.", Color.DeepPink);
				return;
			}

			if (newName.Length > 20)
			{
				args.Player.SendMessage("A name must not be longer than 20 characters.", Color.DeepPink);
				return;
			}

			List<TSPlayer> SameName = TShock.Players.Where(player => (player != null && player.Name == newName)).ToList();
			if (SameName.Count > 0)
			{
				args.Player.SendMessage("This name is taken by another player.", Color.DeepPink);
				return;
			}
			#endregion Checks

			string oldName = plr.TPlayer.name;
			if (!args.Player.ContainsData("oldname"))
				plr.SetData("oldname", oldName);
			plr.TPlayer.name = newName;
			NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, plr.TPlayer.name, args.Player.Index, 0, 0, 0, 0);
			TShock.Utils.Broadcast(string.Format("{0} has changed his name to {1}.", oldName, newName), Color.DeepPink);
		}

		private void OldName(CommandArgs args)
		{
			if (args.Player == null) return;
			
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /oldname [player]");
				return;
			}
			var name = String.Join(" ", args.Parameters);

			var plist = TShock.Utils.FindPlayer(name);

			if (plist.Count == 0)
			{
				args.Player.SendErrorMessage("No player found by the name " + name);
				return;
			}
			else if (plist.Count > 1)
			{
				TShock.Utils.SendMultipleMatchError(args.Player, plist.Select(p => p.Name));
				return;
			}

			if (plist[0].ContainsData("oldname"))
				args.Player.SendMessage(string.Format("{0}'s old name is {1}.", name, plist[0].GetData<string>("oldname")), Color.DeepPink);
			else
				args.Player.SendMessage(string.Format("{0}'s name has not been changed.", name), Color.DeepPink);
		}

		private void Chat(CommandArgs args)
		{
			if (args.Player == null) return;
			
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /chat [message]");
				return;
			}
			string text = String.Join(" ", args.Parameters);
			var tsplr = args.Player;
			if (!tsplr.mute)
			{
				TShock.Utils.Broadcast(
					String.Format(TShock.Config.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix, text),
					tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
			}
			else
			{
				tsplr.SendErrorMessage("You are muted!");
			}
		}
	}
}
