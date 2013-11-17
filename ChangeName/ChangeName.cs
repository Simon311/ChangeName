using System;
using System.Linq;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ChangeName
{
    [ApiVersion(1, 14)]

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
            get { return "Simon311"; }
        }
        public override string Description
        {
            get { return "Changing names"; }
        }

        Dictionary<string, string> oldNames = new Dictionary<string, string>();

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
            Commands.ChatCommands.Add(new Command("tshock.canchat", Chat, "chat"));
        }

        private void ChanName(CommandArgs args)
        {
            if (args.Player != null)
            {
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
                    args.Player.SendErrorMessage(string.Format("More than one ({0}) player matched!", args.Parameters.Count));
                    return;
                }

                string newName = args.Parameters[1];
				bool hidden = args.Parameters.Count > 2;

                var plr = foundplr[0];
                string oldName = plr.TPlayer.name;
                if (!hidden) TShock.Utils.Broadcast(string.Format("{0} has changed {1}'s name to {2}.", args.Player.Name, oldName, newName), Color.DeepPink);
                else args.Player.SendMessage(string.Format("You have secretly changed {0}'s name to {1}.", oldName, newName), Color.DeepPink);
                plr.TPlayer.name = newName;
				NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, plr.TPlayer.name, args.Player.Index, 0, 0, 0, 0);
                oldNames[newName] = oldName;
            }
        }

        private void SelfName(CommandArgs args)
        {
            if (args.Player != null)
            {
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

				List<TSPlayer> SameName = TShock.Players.Where(player => (player != null && player.Name == newName)).ToList();
				if (SameName.Count > 0)
				{
					args.Player.SendMessage("This name is taken by another player.", Color.DeepPink);
					return;
				}
				#endregion Checks

				string oldName = plr.TPlayer.name;
                plr.TPlayer.name = newName;
				NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, plr.TPlayer.name, args.Player.Index, 0, 0, 0, 0);
                oldNames[newName] = oldName;
                TShock.Utils.Broadcast(string.Format("{0} has changed his name to {1}.", oldName, newName), Color.DeepPink);
            }
        }

        private void OldName(CommandArgs args)
        {
            if (args.Player != null)
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /oldname [player]");
                    return;
                }
				var name = String.Join(" ", args.Parameters);
                if (oldNames.ContainsKey(name)) args.Player.SendMessage(string.Format("{0}'s old name is {1}.", name, oldNames[name]), Color.DeepPink);
				else args.Player.SendMessage(string.Format("{0}'s name has not been changed.", name), Color.DeepPink);
            }
        }

        private void Chat(CommandArgs args)
        {
            if (args.Player != null)
            {
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
}
