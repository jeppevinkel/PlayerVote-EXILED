using System;
using System.Collections.Generic;
using System.Timers;
using EXILED;
using EXILED.Extensions;
using EXILED.Patches;

namespace PlayerVote
{
	public class Plugin : EXILED.Plugin
	{
		//Instance variable for eventhandlers
		public EventHandlers EventHandlers;

		public bool enabled = true;

		internal Vote CurrentVote = null;

		internal int VoteDuration = 30;

		internal bool EnableKick = false;
		internal bool EnableKill = false;
		internal bool EnableNuke = false;
		internal bool EnableRespawnWave = false;
		internal bool EnableRestartRound = false;

		internal int ThresholdKick = 80;
		internal int ThresholdKill = 80;
		internal int ThresholdNuke = 80;
		internal int ThresholdRespawnWave = 80;
		internal int ThresholdRestartRound = 80;

		public override void OnEnable()
		{
			enabled = Config.GetBool("playervote_enable", false);
			if (!enabled)
			{
				return;
			}
			Log.Debug("Initializing event handlers..");

			EventHandlers = new EventHandlers(this);

			Events.ConsoleCommandEvent += EventHandlers.OnConsoleCommand;
			Events.RemoteAdminCommandEvent += EventHandlers.OnRACommand;
			Events.WaitingForPlayersEvent += EventHandlers.OnWaitingForPlayers;

			Log.Info($"PlayerVote has been loaded.");
		}

		public override void OnDisable()
		{
			Events.ConsoleCommandEvent -= EventHandlers.OnConsoleCommand;
			Events.RemoteAdminCommandEvent -= EventHandlers.OnRACommand;
			Events.WaitingForPlayersEvent -= EventHandlers.OnWaitingForPlayers;

			EventHandlers = null;
		}

		public void ReloadConfig()
		{
			enabled = Config.GetBool("playervote_enable", true);

			VoteDuration = Config.GetInt("playervote_vote_duration", 30);

			//EnableKick = Config.GetBool("playervote_enable_kick");
			//EnableKill = Config.GetBool("playervote_enable_kill");
			//EnableNuke = Config.GetBool("playervote_enable_nuke");
			//EnableRespawnWave = Config.GetBool("playervote_enable_respawnwave");
			//EnableRestartRound = Config.GetBool("playervote_enable_restartround");

			ThresholdKick = Config.GetInt("playervote_threshold_kick", 80);
			ThresholdKill = Config.GetInt("playervote_threshold_kill", 80);
			ThresholdNuke = Config.GetInt("playervote_threshold_nuke", 80);
			ThresholdRespawnWave = Config.GetInt("playervote_threshold_respawnwave", 80);
			ThresholdRestartRound = Config.GetInt("playervote_threshold_restartround", 80);
		}

		public override void OnReload()
		{
			//This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
		}

		public string CallvoteHandler(ReferenceHub player, string[] args) // lowercase to match command
		{
			Log.Info(player.nicknameSync.MyNick + " called vote with arguments: ");
			for (int i = 0; i < args.Length; i++)
			{
				Log.Info("\t" + i + ": " + args[i]);
			}
			if (args.Length == 0)
			{
				//return new string[] { "callvote RestartRound", "callvote Kick <player>", "callvote <custom> [options]" };
				return "callvote Kick/Kill/<custom> <player>/[options]";
			}
			else
			{
				if (CurrentVote != null)
				{
					//return new string[] { "A vote is currently in progress." };
					return "A vote is currently in progress.";
				}
				else
				{
					Dictionary<int, string> options = new Dictionary<int, string>();
					switch (args[0].ToLower())
					{
						case "kick":
							if (this.EnableKick)
							{
								if (!player.CheckPermission("playervote.kick"))
								{
									return "You don't have the required permission to call kick votes.";
								}

								if (args.Length == 1)
								{
									return "callvote Kick <player>";
								}
								else
								{
									Log.Info("Vote called by " + player.nicknameSync.MyNick + " to " + args[0] + " player " + args[1]);

									ReferenceHub locatedPlayer = Player.GetPlayer(args[1]);
									if (locatedPlayer != null)
									{
										options[1] = "Yes";
										options[2] = "No";

										StartVote(new Vote(player.nicknameSync.MyNick + " asks: Kick " + locatedPlayer.nicknameSync.MyNick + "?", options), delegate (Vote vote)
										{
											int votePercent = (int)((float)vote.Counter[1] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
											if (votePercent >= this.ThresholdKick)
											{
												Extensions.BC(5, votePercent + "% voted yes. Kicking player " + locatedPlayer.nicknameSync.MyNick + ".");
												GameCore.Console.singleton.TypeCommand($"/ban " + locatedPlayer.queryProcessor.PlayerId + " 0", new PVoteSender());
											}
											else
											{
												Extensions.BC(5, "Only " + votePercent + "% voted yes. " + this.ThresholdKick + "% was required to kick " + locatedPlayer.nicknameSync.MyNick + ".");
											}
										});

										break;
									}
									else
									{
										return "Did not find any players with the name or partial name of " + args[1];
									}
								}
							}
							else
							{
								return "callvote Kick is not enabled.";
							}

						case "kill":
							if (this.EnableKill)
							{
								if (!player.CheckPermission("playervote.kill"))
								{
									return "You don't have the required permission to call kill votes.";
								}

								if (args.Length == 1)
								{
									return "callvote Kill <player>";
								}
								else
								{
									Log.Info("Vote called by " + player.nicknameSync.MyNick + " to " + args[0] + " player " + args[1]);

									ReferenceHub locatedPlayer = Player.GetPlayer(args[1]);
									if (locatedPlayer != null)
									{
										options[1] = "Yes";
										options[2] = "No";

										StartVote(new Vote(player.nicknameSync.MyNick + " asks: Kill " + locatedPlayer.nicknameSync.MyNick + "?", options), delegate (Vote vote)
										{
											int votePercent = (int)((float)vote.Counter[1] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
											if (votePercent >= this.ThresholdKill)
											{
												Extensions.BC(5, votePercent + "% voted yes. Killing player " + locatedPlayer.nicknameSync.MyNick + ".");
												locatedPlayer.playerStats.SetHPAmount(0);
												locatedPlayer.playerStats._hpDirty = true;
											}
											else
											{
												Extensions.BC(5, "Only " + votePercent + "% voted yes. " + this.ThresholdKill + "% was required to kill " + locatedPlayer.nicknameSync.MyNick + ".");
											}
										});

										break;
									}
									else
									{
										return "Did not find any players with the name or partial name of " + args[1];
									}
								}
							}
							else
							{
								return "callvote Kill is not enabled.";
							}


						case "nuke":
							if (EnableNuke)
							{
								if (!player.CheckPermission("playervote.nuke"))
								{
									return "You don't have the required permission to call nuke votes.";
								}

								Log.Info("Vote called by " + player.nicknameSync.MyNick + " to " + args[0]);
								//return new string[] { "To be implemented." };

								options[1] = "Yes";
								options[2] = "No";

								StartVote(new Vote(player.nicknameSync.MyNick + " asks: NUKE THE FACILITY?!?", options), delegate (Vote vote)
								{
									int votePercent = (int)((float)vote.Counter[1] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
									if (votePercent >= this.ThresholdNuke)
									{
										Extensions.BC(5, votePercent + "% voted yes. Nuking the facility...");
										EnableNuke = true;
									}
									else
									{
										Extensions.BC(5, "Only " + votePercent + "% voted yes. " + this.ThresholdNuke + "% was required to nuke the facility.");
									}
								});
								break;
							}
							else
							{
								return "callvote Nuke is not enabled.";
							}

						case "respawnwave":
							if (EnableRespawnWave)
							{
								if (!player.CheckPermission("playervote.respawnwave"))
								{
									return "You don't have the required permission to call respawnwave votes.";
								}

								Log.Info("Vote called by " + player.nicknameSync.MyNick + " to " + args[0]);
								//return new string[] { "To be implemented." };

								options[1] = "No";
								options[2] = "MTF";
								options[3] = "CI";

								StartVote(new Vote(player.nicknameSync.MyNick + " asks: Respawn the next wave?", options), delegate (Vote vote)
								{
									int votePercent = (int)((float)vote.Counter[1] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
									int mtfVotePercent = (int)((float)vote.Counter[2] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
									int ciVotePercent = (int)((float)vote.Counter[3] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
									if (mtfVotePercent >= this.ThresholdRespawnWave)
									{
										Extensions.BC(5, mtfVotePercent + "% voted yes. Respawning a wave of Nine-Tailed Fox...");

										MTFRespawn _MTFRespawn = PlayerManager.localPlayer.GetComponent<MTFRespawn>();

										bool oldNextRespawn = _MTFRespawn.nextWaveIsCI;
										_MTFRespawn.nextWaveIsCI = false;
										_MTFRespawn.RespawnDeadPlayers();
										_MTFRespawn.nextWaveIsCI = oldNextRespawn;
									}
									else if (ciVotePercent >= this.ThresholdRespawnWave)
									{
										Extensions.BC(5, ciVotePercent + "% voted yes. Respawning a wave of Chaos Insurgency...");

										MTFRespawn _MTFRespawn = PlayerManager.localPlayer.GetComponent<MTFRespawn>();

										bool oldNextRespawn = _MTFRespawn.nextWaveIsCI;
										_MTFRespawn.nextWaveIsCI = true;
										_MTFRespawn.RespawnDeadPlayers();
										_MTFRespawn.nextWaveIsCI = oldNextRespawn;
									}
									else
									{
										Extensions.BC(5, votePercent + "% voted no. " + this.ThresholdRespawnWave + "% was required to respawn the next wave.");
									}
								});
								break;
							}
							else
							{
								return "callvote RespawnWave is not enabled.";
							}

						case "restartround":
							if (EnableRestartRound)
							{
								if (!player.CheckPermission("playervote.restartround"))
								{
									return "You don't have the required permission to call restartround votes.";
								}

								Log.Info("Vote called by " + player.nicknameSync.MyNick + " to " + args[0]);
								//return new string[] { "To be implemented." };

								options[1] = "Yes";
								options[2] = "No";

								StartVote(new Vote(player.nicknameSync.MyNick + " asks: Restart the round?", options), delegate (Vote vote)
								{
									int votePercent = (int)((float)vote.Counter[1] / (float)(ServerConsole.PlayersAmount - 1) * 100f);
									if (votePercent >= this.ThresholdRestartRound)
									{
										Extensions.BC(5, votePercent + "% voted yes. Restarting the round...");
										PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
									}
									else
									{
										Extensions.BC(5, "Only " + votePercent + "% voted yes. " + this.ThresholdRestartRound + "% was required to restart the round.");
									}
								});
								break;
							}
							else
							{
								return "callvote RestartRound is not enabled.";
							}

						default:
							if (!player.CheckPermission("playervote.custom"))
							{
								return "You don't have the required permission to call custom votes.";
							}

							if (args.Length == 1)
							{
								Log.Info("Binary vote called by " + player.nicknameSync.MyNick + ": " + string.Join(" ", args));
								options[1] = "Yes";
								options[2] = "No";
							}
							else
							{
								Log.Info("Multiple-choice vote called by " + player.nicknameSync.MyNick + ": " + string.Join(" ", args));
								for (int i = 1; i < args.Length; i++)
								{
									options[i] = args[i];
								}
							}
							StartVote(new Vote(player.nicknameSync.MyNick + " asks: " + args[0], options), null);
							break;
					}
					return "Vote has been started!";
				}
			}
		}

		public bool Voting()
		{
			if (CurrentVote != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool StartVote(Vote newVote, CallvoteFunction callback)
		{
			if (CurrentVote != null)
			{
				return false;
			}

			CurrentVote = newVote;
			CurrentVote.Callback = callback;
			string firstBroadcast = CurrentVote.Question + " Press ~ and type ";
			int counter = 0;
			foreach (KeyValuePair<int, string> kv in CurrentVote.Options)
			{
				if (counter == CurrentVote.Options.Count - 1)
				{
					firstBroadcast += "or ." + kv.Key + " for " + kv.Value + " ";
				}
				else
				{
					firstBroadcast += "." + kv.Key + " for " + kv.Value + ", ";
				}
				counter++;
			}
			Extensions.BC(5, firstBroadcast);

			int timerCounter = 0;
			CurrentVote.Timer = new Timer
			{
				Interval = 5000,
				Enabled = true,
				AutoReset = true
			};
			CurrentVote.Timer.Elapsed += delegate
			{
				if (CurrentVote.Timer.Interval == 5000)
				{
					CurrentVote.Timer.Interval = 1000;
				}

				if (timerCounter >= this.VoteDuration + 1)
				{
					if (CurrentVote.Callback == null)
					{
						string timerBroadcast = "Final results:\n";
						foreach (KeyValuePair<int, string> kv in CurrentVote.Options)
						{
							timerBroadcast += CurrentVote.Options[kv.Key] + " (" + CurrentVote.Counter[kv.Key] + ") ";
						}
						Extensions.BC(5, timerBroadcast);
					}
					else
					{
						CurrentVote.Callback.Invoke(CurrentVote);
					}

					CurrentVote.Timer.Enabled = false;
					CurrentVote = null;
				}
				else
				{
					string timerBroadcast = firstBroadcast + "\n";
					foreach (KeyValuePair<int, string> kv in CurrentVote.Options)
					{
						timerBroadcast += CurrentVote.Options[kv.Key] + " (" + CurrentVote.Counter[kv.Key] + ") ";
					}
					Extensions.BC(1, timerBroadcast);
				}
				timerCounter++;
			};
			return true;
		}

		public string StopvoteHandler(ReferenceHub player)
		{
			if (!player.CheckPermission("playervote.stopvote"))
			{
				return "You don't have the required permission to stop votes.";
			}

			if (this.StopVote())
			{
				return "Vote stopped.";
			}
			else
			{
				return "There is not a vote in progress.";
			}
		}

		public bool StopVote()
		{
			if (this.CurrentVote != null)
			{
				if (this.CurrentVote.Timer != null)
				{
					this.CurrentVote.Timer.Stop();
				}
				this.CurrentVote = null;
				return true;
			}
			else
			{
				return false;
			}
		}

		public string VoteHandler(ReferenceHub player, int option)
		{
			if (CurrentVote != null)
			{
				if (!player.CheckPermission("playervote.vote"))
				{
					return "You don't have the required permission to vote.";
				}
				if (!CurrentVote.Votes.Contains(player.characterClassManager.UserId))
				{
					if (CurrentVote.Options.ContainsKey(option))
					{
						CurrentVote.Counter[option]++;
						CurrentVote.Votes.Add(player.characterClassManager.UserId);
						Log.Info("Player " + player.nicknameSync.MyNick + " voted " + CurrentVote.Options[option] + " bringing the counter to " + CurrentVote.Counter[option]);
						return "Vote accepted!";
					}
					else
					{
						return "Vote does not have an option " + option + ".";
					}
				}
				else
				{
					return "You've already voted.";
				}
			}
			else
			{
				return "There is no vote in progress.";
			}
		}

		public override string getName { get; } = "PlayerVote";
	}

	public delegate void CallvoteFunction(Vote vote);

	public class Vote
	{
		public string Question;
		public Dictionary<int, string> Options;
		public HashSet<string> Votes;
		public Dictionary<int, int> Counter;
		public Timer Timer;
		public CallvoteFunction Callback;

		public Vote(string question, Dictionary<int, string> options)
		{
			this.Question = question;
			this.Options = options;
			this.Votes = new HashSet<string>();
			this.Counter = new Dictionary<int, int>();
			foreach (int option in options.Keys)
			{
				Counter[option] = 0;
			}
		}

		// Allow Votes and Counter to be passed in and saved by reference for Event code
		public Vote(string question, Dictionary<int, string> options, HashSet<string> votes, Dictionary<int, int> counter)
		{
			this.Question = question;
			this.Options = options;
			this.Votes = votes;
			this.Counter = counter;
			foreach (int option in options.Keys)
			{
				Counter[option] = 0;
			}
		}
	}
}