using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EXILED;
using EXILED.Extensions;
using EXILED.Patches;
using Grenades;
using MEC;
using static EXILED.Events;

namespace PlayerVote
{
	public class EventHandlers
	{
		public Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnConsoleCommand(ConsoleCommandEvent ev)
		{
			string command = ev.Command.Split(' ')[0];
			string[] args = ev.Command.Split(' ');
			ReferenceHub sender = ev.Player;

			if (int.TryParse(command, out int option))
			{
				if (plugin.Voting())
				{

					ev.ReturnMessage = (plugin.VoteHandler(sender, option));
				}
				else
				{
					ev.ReturnMessage = ("No vote is in progress.");
				}
			}
			else
			{
				switch (command)
				{
					case "callvote":
						string[] quotedArgs = Regex.Matches(string.Join(" ", ev.Command), "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
							.Cast<Match>()
							.Select(m => m.Value)
							.ToArray()
							.Skip(1)
							.ToArray();
						ev.ReturnMessage = (plugin.CallvoteHandler(sender, quotedArgs));

						for (int i = 0; i < quotedArgs.Length; i++)
						{
							Log.Debug("\t" + i + ": " + quotedArgs[i]);
						}
						if (args.Length == 0)
						{
							ev.ReturnMessage = ("callvote Kick/Kill/<custom> <player>/[options]");
						}
						break;
					case "stopvote":
						ev.ReturnMessage = (plugin.StopvoteHandler(sender));
						break;
					case "yes":
						ev.ReturnMessage = (plugin.VoteHandler(sender, 1));
						break;
					case "no":
						ev.ReturnMessage = (plugin.VoteHandler(sender, 2));
						break;
				}
			}
		}

		public void OnRACommand(ref RACommandEvent ev)
		{
			string command = ev.Command.Split(' ')[0];
			string[] args = ev.Command.Split(' ');
			ReferenceHub sender = Player.GetPlayer(ev.Sender.SenderId);

			if (int.TryParse(command, out int option))
			{
				if (plugin.Voting())
				{

					ev.Sender.RAMessage(plugin.VoteHandler(sender, option));
				}
				else
				{
					ev.Sender.RAMessage("No vote is in progress.");
				}
			}
			else
			{
				switch (command)
				{
					case "callvote":
						ev.Allow = false;
						string[] quotedArgs = Regex.Matches(string.Join(" ", ev.Command), "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
							.Cast<Match>()
							.Select(m => m.Value)
							.ToArray()
							.Skip(1)
							.ToArray();
						ev.Sender.RAMessage(plugin.CallvoteHandler(sender, quotedArgs));

						for (int i = 0; i < quotedArgs.Length; i++)
						{
							Log.Debug("\t" + i + ": " + quotedArgs[i]);
						}
						if (args.Length == 0)
						{
							ev.Sender.RAMessage("callvote Kick/Kill/<custom> <player>/[options]");
						}
						break;
					case "stopvote":
						ev.Allow = false;
						ev.Sender.RAMessage(plugin.StopvoteHandler(sender));
						break;
					case "yes":
						ev.Allow = false;
						ev.Sender.RAMessage(plugin.VoteHandler(sender, 1));
						break;
					case "no":
						ev.Allow = false;
						ev.Sender.RAMessage(plugin.VoteHandler(sender, 2));
						break;
				}
			}
		}

		public void OnWaitingForPlayers()
		{
			plugin.ReloadConfig();
			if (this.plugin.CurrentVote != null && this.plugin.CurrentVote.Timer != null)
			{
				this.plugin.CurrentVote.Timer.Stop();
				this.plugin.CurrentVote.Timer.Dispose();
			}
			this.plugin.CurrentVote = null;
		}
	}
}