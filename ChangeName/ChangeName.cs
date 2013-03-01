using System;
using TShockAPI;
using Terraria;
using System.Collections.Generic;
using Hooks;
using System.ComponentModel;

namespace ChangeName
{
    [APIVersion(1, 12)]

    public class ChangeName : TerrariaPlugin
    {
        public override Version Version
        {
            get { return new Version("1.0.0.1"); }
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
            Order = -2;
        }
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("changenames", ChanName, "chname"));
            Commands.ChatCommands.Add(new Command("oldnames", OldName, "oldname"));
            Commands.ChatCommands.Add(new Command("selfname", SelfName, "selfname"));
            Commands.ChatCommands.Add(new Command("", Chat, "chat"));
            ServerHooks.Chat += OnChat;
            // TShock.Config.EnableChatAboveHeads = false;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Hooks.ServerHooks.Chat -= OnChat;
            }
            base.Dispose(disposing);
        }
        private void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
            if (e.Handled)
                return;

            var tsplr = TShock.Players[msg.whoAmI];
            if (tsplr == null)
            {
                e.Handled = true;
                return;
            }
            if (text.StartsWith("/"))
            {
                return;
            }
            else if (!tsplr.mute && TShock.Config.EnableChatAboveHeads)
            {
                Broadcast(ply, String.Format(TShock.Config.ChatAboveHeadsFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix, text), tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);

                e.Handled = true;
            }
        }
        private void ChanName(CommandArgs args)
        {
            if( args.Player != null )
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
                bool hidden;
                if (args.Parameters.Count<3) { hidden = false; } else { hidden = true; }
                var plr = foundplr[0];
                string oldName = plr.TPlayer.name;
                if (!hidden)
                {
                    TShock.Utils.Broadcast(string.Format("{0} has changed {1}'s name to {2}.", args.Player.Name, oldName, newName), Color.DeepPink);
                    if (TShock.Config.EnableChatAboveHeads) { TShock.Utils.Broadcast("It will take a while before new name appears", Color.DeepPink); }
                }
                else
                {
                    args.Player.SendMessage(string.Format("You have secretly changed {0}'s name to {1}.", oldName, newName), Color.DeepPink);
                    if (TShock.Config.EnableChatAboveHeads) { args.Player.SendMessage("It will take a while before new name appears", Color.DeepPink); }
                }
                plr.TPlayer.name = newName;
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
                string newName = args.Parameters[0];
                bool hidden;
                if (args.Parameters.Count < 2) { hidden = false; } else { hidden = true; }
                string oldName = plr.TPlayer.name;
                plr.TPlayer.name = newName;
                oldNames[newName] = oldName;
                if (!hidden)
                {
                    TShock.Utils.Broadcast(string.Format("{0} has changed his name to {1}.", oldName, newName), Color.DeepPink);
                    if (TShock.Config.EnableChatAboveHeads) { args.Player.SendMessage("It will take a while before new name appears", Color.DeepPink); }
                }
                else
                {
                    args.Player.SendMessage(string.Format("You have secretly changed your name to {0}.", newName), Color.DeepPink);
                    if (TShock.Config.EnableChatAboveHeads) { args.Player.SendMessage("It will take a while before new name appears", Color.DeepPink); }
                }
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
                if(oldNames.ContainsKey(args.Parameters[0])){
                    args.Player.SendMessage(string.Format("{0}'s old name is {1}.", args.Parameters[0], oldNames[args.Parameters[0]]), Color.DeepPink);
                } else {
                    args.Player.SendMessage(string.Format("{0}'s name has not been changed.", args.Parameters[0]), Color.DeepPink);
                }
            }
        }
        private void Broadcast(int ply, string msg, byte red, byte green, byte blue)
        {
            foreach (TSPlayer pla in TShock.Players)
            {
                if (pla == TShock.Players[ply])
                {
                    pla.SendMessage(string.Format("<{0}> {1}", TShock.Players[ply].Name, msg), red, green, blue);
                }
                else
                {
                    pla.SendMessageFromPlayer(msg, red, green, blue, ply);
                }
            }
            TSPlayer.Server.SendMessage(TShock.Players[ply].Name + ": " + msg, red, green, blue);
            Log.Info(string.Format("Broadcast: {0}", TShock.Players[ply].Name + ": " + msg));
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
