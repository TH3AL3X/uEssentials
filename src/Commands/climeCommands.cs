using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "clime",
        Aliases = new[] { "c", "clime" },
        Usage = "/c [estado]",
        Description = "Inicia una votación para cambiar el clima",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandClime : EssCommand
    {
        private bool isVotingActive = false;
        private Dictionary<ulong, float> playersVoted = new Dictionary<ulong, float>();
        private int requiredVotes = 0;
        private int currentVotes = 0;
        private float lastVoteTime = 0;
        private ulong initiatingPlayerId;
        private string isVotingestate;

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var Config = UEssentials.Config.Climeinteraction;
            var estados = "dia, noche";

            // Reseteo automático si votación vencida
            if (isVotingActive && (Time.realtimeSinceStartup - lastVoteTime) > Config.VoteDuration)
            {
                isVotingActive = false;
                playersVoted.Clear();
                currentVotes = 0;
                EssLang.Send("generalicon", src, "vote_expired");
            }

            if (args.Length < 1)
            {

              EssLang.Send("icon_error_general", src, "climate_withoutr");
              return CommandResult.Empty();
            }


            string input = args[0].ToString();
            var player = src.ToPlayer();
            ulong steamPlayerId = player.CSteamId.m_SteamID;
            string estado = null;

            if (input == "dia")
                estado = "day";
            else if (input == "noche")
                estado = "night";
            else
                estado = null;

            // Cooldown
            float tiempoRestante = UEssentials.Config.Climeinteraction.VoteCooldown - (Time.realtimeSinceStartup - lastVoteTime);
            if (tiempoRestante > 0)
                return CommandResult.LangError("icon_clime_interaction", "vote_cooldown", Mathf.CeilToInt(tiempoRestante));

            if (isVotingActive)
            {
                if (isVotingestate == estado)
                {
                    if (!playersVoted.ContainsKey(steamPlayerId))
                    {
                        playersVoted[steamPlayerId] = Time.realtimeSinceStartup;
                        currentVotes++;
                        EssLang.Send("icon_clime_interaction", src, "voted_registrer", currentVotes, requiredVotes);
                    }
                    else
                    {
                        return CommandResult.LangError("icon_error_general", "ya_votaste");
                    }
                }
                else
                {
                    return CommandResult.LangError("icon_error_general", "state_vote_invalid", isVotingestate);
                }
            }
            else if (estado == null)
            {
                EssLang.Send("icon_error_general", src, "state_invalid", estados);
                return CommandResult.Empty();
            }
            else
            {
                // Iniciar nueva votación
                isVotingestate = estado;
                isVotingActive = true;
                lastVoteTime = Time.realtimeSinceStartup;
                requiredVotes = Mathf.Max(1, Mathf.CeilToInt((float)Provider.clients.Count * UEssentials.Config.Climeinteraction.VoteThreshold));
                currentVotes = 1;
                playersVoted.Clear();
                playersVoted[steamPlayerId] = Time.realtimeSinceStartup;
                initiatingPlayerId = steamPlayerId;

                EssLang.Send("icon_clime_interaction", src, "iniciated_vote", player.DisplayName, estado, currentVotes, requiredVotes);
            }

            if (currentVotes >= requiredVotes)
            {
                EssLang.Send("icon_clime_interaction", src, "clime_CHANGED");

                // Protegemos la llamada para no pasar null al ejecutar comando
                if (!string.IsNullOrEmpty(isVotingestate))
                {
                    Commander.execute(CSteamID.Nil, isVotingestate);
                }
             

                isVotingActive = false;
                playersVoted.Clear();
                currentVotes = 0;
                return CommandResult.Success();
            }

            return CommandResult.Success();
        }
    }
}
