// -----------------------------------------------------------------------
// <copyright file="Accept.cs" company="Build">
// Copyright (c) Build. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace ScpSwap.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using ScpSwap.Models;

    /// <summary>
    /// Accepts an active swap request.
    /// </summary>
    public class Accept : ICommand
    {
        /// <inheritdoc />
        public string Command { get; set; } = "accept";

        /// <inheritdoc />
        public string[] Aliases { get; set; } = { "yes", "y" };

        /// <inheritdoc />
        public string Description { get; set; } = "Accepts an active swap request.";

        /// <inheritdoc />
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player playerSender = Player.Get(sender);
            if (playerSender == null)
            {
                response = Plugin.Instance.Translation.ExecutorIsntPlayer;
                return false;
            }

            Swap swap = Swap.FromReceiver(playerSender);
            if (swap == null)
            {
                response = Plugin.Instance.Translation.NoPendingRequest;
                return false;
            }

            swap.Run();
            response = Plugin.Instance.Translation.SuccessfulSwap;
            return true;
        }
    }
}