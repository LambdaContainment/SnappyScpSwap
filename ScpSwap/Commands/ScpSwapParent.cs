// -----------------------------------------------------------------------
// <copyright file="ScpSwapParent.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.Permissions.Extensions;

namespace ScpSwap.Commands
{
    using System;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using PlayerRoles;
    using ScpSwap.Configs;
    using ScpSwap.Models;

    /// <summary>
    /// The base command for ScpSwapParent.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ScpSwapParent : ParentCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScpSwapParent"/> class.
        /// </summary>
        public ScpSwapParent() => LoadGeneratedCommands();

        /// <inheritdoc />
        public override string Command => "scpswap";

        /// <inheritdoc />
        public override string[] Aliases { get; } = { "swap" };

        /// <inheritdoc />
        public override string Description => "Base command for ScpSwap.";

        /// <inheritdoc />
        public sealed override void LoadGeneratedCommands()
        {
            CommandTranslations commandTranslations = Plugin.Instance.Translation.CommandTranslations;

            RegisterCommand(commandTranslations.Accept);
            RegisterCommand(commandTranslations.Cancel);
            RegisterCommand(commandTranslations.Decline);
            RegisterCommand(commandTranslations.List);
        }

        /// <inheritdoc />
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            if (playerSender == null)
            {
                response = Plugin.Instance.Translation.ExecutorIsntPlayer;
                return false;
            }

            if (!Round.IsStarted)
            {
                response = Plugin.Instance.Translation.RoundIsntStarted;
                return false;
            }

            if (Round.ElapsedTime.TotalSeconds > Plugin.Instance.Config.SwapTimeout)
            {
                response = Plugin.Instance.Translation.SwapPeriodEnded;
                return false;
            }

            if (arguments.IsEmpty())
            {
                response = $"Usage: .{Command} ScpNumber";
                return false;
            }

            if (Plugin.Instance.Config.AllowUserSwapByPermission)
            {
                if (!playerSender.CheckPermission("scpswap.allowed"))
                {
                    response = Plugin.Instance.Translation.AllowUserSwapByPermission;
                    return false;
                }
            }

            if (!playerSender.IsScp && ValidSwaps.GetCustom(playerSender) == null)
            {
                response = Plugin.Instance.Translation.NotAnScp;
                return false;
            }

            if (Swap.FromSender(playerSender) != null)
            {
                response = Plugin.Instance.Translation.AlreadyHasPendingRequest;
                return false;
            }

            Player receiver = GetReceiver(arguments.At(0), out Action<Player> spawnMethod);
            if (playerSender == receiver)
            {
                response = Plugin.Instance.Translation.CannotSwapWithYourself;
                return false;
            }

            if (Plugin.Instance.Config.BlacklistedSwapFromScps.Contains(playerSender.Role.Type))
            {
                response = Plugin.Instance.Translation.CannotSwapOffThisScp;
                return false;
            }

            if (receiver != null)
            {
                Swap.Send(playerSender, receiver);
                response = Plugin.Instance.Translation.RequestSent;
                return true;
            }

            if (spawnMethod == null)
            {
                response = Plugin.Instance.Translation.CannotFindRole;
                return false;
            }

            if (Plugin.Instance.Config.AllowNewScps)
            {
                spawnMethod(playerSender);
                response = Plugin.Instance.Translation.SuccessfulSwap;
                return true;
            }

            response = Plugin.Instance.Translation.CannotFindPlayerWithRole;
            return false;
        }

        private static Player GetReceiver(string request, out Action<Player> spawnMethod)
        {
            CustomSwap customSwap = ValidSwaps.GetCustom(request);
            if (customSwap != null)
            {
                spawnMethod = customSwap.SpawnMethod;
                return Player.List.FirstOrDefault(player => customSwap.VerificationMethod(player));
            }

            RoleTypeId roleSwap = ValidSwaps.Get(request);
            if (roleSwap != RoleTypeId.None)
            {
                spawnMethod = player => player.Role.Set(roleSwap);
                return Player.List.FirstOrDefault(player => player.Role == roleSwap);
            }

            spawnMethod = null;
            return null;
        }
    }
}