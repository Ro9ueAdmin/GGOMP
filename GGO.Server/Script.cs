﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using GGO.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GGO.Server
{
    public class ScriptServer : BaseScript
    {
        /// <summary>
        /// The current game status.
        /// </summary>
        public static GameStatus Status = GameStatus.WaitingForPlayers;
        /// <summary>
        /// The list of players ready.
        /// </summary>
        public static List<Player> Ready = new List<Player>();

        public ScriptServer()
        {
            // Register our events, all of them
            Tick += OnTickCheckStart;
            EventHandlers.Add("ggo:onPlayerSpawnHUB", new Action<Player>(OnPlayerSpawnHUB));
            EventHandlers.Add("playerDropped", new Action<Player, string>(OnPlayerDropped));
        }

        private async Task OnTickCheckStart()
        {
            // Store our wait time
            int WaitTime = API.GetConvarInt("ggo_gamestart", 1);

            // Create a list of players and store the player count and minimum for the game
            int MinimumPlayers = API.GetConvarInt("ggo_minplayers", 1);
            
            // If the player count is higher or equal to the minimum and the current status is "waiting for players"
            if (Ready.Count >= MinimumPlayers && Status == GameStatus.WaitingForPlayers)
            {
                // Store the log message
                string Message = string.Format("", API.GetConvarInt("ggo_gamestart", 1));
                // Write a note into the server console
                Debug.WriteLine(Message.Replace("~n~", " "));
                // Store the status on a variable
                Status = GameStatus.EnoughPlayers;
                // And notify all of the players
                NotifyPlayers(Message, false);
            }
            // If the player count is lower than the required one
            else if (Ready.Count < MinimumPlayers)
            {
                // Store the log message
                string Message = string.Format("", MinimumPlayers - Ready.Count, API.GetConvarInt("ggo_gamestart", 1));
                // Write a note into the server console
                Debug.WriteLine(Message.Replace("~n~", " "));
                // Store the status on a variable
                Status = GameStatus.WaitingForPlayers;
                // And notify all of the players
                NotifyPlayers(Message, false);
            }
            // If none of the later match
            else if (Status == GameStatus.EnoughPlayers)
            {
                // Write a note into the server console
                Debug.WriteLine("".Replace("~n~", " "));
                // Store the status on a variable
                Status = GameStatus.GameRunning;
                // Run all of the shit that we need
                StartMatch();
                // And notify all of the players
                NotifyPlayers("");
            }

            // Try again in the specified number of minutes (default: 1)
            await Delay(API.GetConvarInt("ggo_gamestart", 1) * 60 * 1000);
        }

        private void OnPlayerSpawnHUB([FromSource]Player player)
        {
            // Add the player on the ready array
            Ready[Ready.Count] = player;
        }

        private void OnPlayerDropped([FromSource]Player player, string reason)
        {
            // Remove the player from the ready array if is there
            if (Ready.Contains(player))
            {
                Ready.Remove(player);
            }
        }

        public void NotifyPlayers(string Message, bool Started = false)
        {
            // Notify all of the players
            TriggerClientEvent("ggo:onPlayerNotification", Message);
        }

        public void StartMatch()
        {

        }
    }
}
