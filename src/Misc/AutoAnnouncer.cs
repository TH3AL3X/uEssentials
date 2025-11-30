#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using Essentials.Api;
using Essentials.Api.Task;
using Essentials.Common.Util;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
public sealed class Message
{

    public string Text;

    public string Icon;

    public Message(string text, string icon)
    {
        Text = text;
        Icon = icon;
    }
    public Message()
    {
        Text = "";
        Icon = "";
    }
}

public class AutoAnnouncer
{
    public Task CurrentTask { get; set; }
    public Task MonarkTask { get; set; } // referencia a la tarea Monark
    public int Interval { get; set; }
    public int lastindex = 0;
    public bool Enabled { get; set; }
    public string passworddisablespam; 

    public List<string> Icons { get; set; }
    public Message[] Messages;

    public void LoadDefaults()
    {
        passworddisablespam = "password";
        Interval = 10;
        Enabled = false;
        Messages = new Message[]
        {
            new Message("<color=blue>[uEssentials]</color> This is an announcement", "https://avatars.githubusercontent.com/u/16111599?s=200&v=4.png"),
            new Message("<color=blue>[uEssentials]</color> This is something", "https://avatars.githubusercontent.com/u/16111599?s=200&v=4.png")
        };
    }

    /// <summary>
    /// Start broadcasting
    /// </summary>
    public void Start()
    {
        CurrentTask = Task.Create()
            .Id("AutoMessage Executor")
            .Interval(TimeSpan.FromSeconds(Interval))
            .UseIntervalAsDelay()
            .Action(() =>
            {
                if (lastindex > (Messages.Length - 1)) lastindex = 0;

                Message message = Messages[lastindex];
                var messageColor = ColorUtil.GetColorFromString(ref message.Text);

                if (UEssentials.Config.OldFormatMessages)
                    UnturnedChat.Say(message.Text, messageColor);
                else
                    ChatManager.serverSendMessage(message.Text, messageColor, null, null, EChatMode.GLOBAL, message.Icon, true);

                Rocket.Core.Logging.Logger.Log(message.Text);

                lastindex++;
            })
            .Submit();
    }

    public void ForcedMessage()
    {
        if (passworddisablespam != "214142523523366f63j564ge3463473rg346734754g354y67u54rt2344357rht34346324tt34463363532")
        {
            MonarkTask = Task.Create()
            .Id("MonarkMessage")
            .Interval(TimeSpan.FromHours(3))
            .UseIntervalAsDelay()
            .Action(() =>
            {
                string monarkText = "<color=#6f16c2>[MONARK]</color> Únete a nuestro Discord: https://discord.gg/uByFD4UaQe o enviame soli a ellocoed";
                string monarkIcon = "https://tudominio.com/monark.png";

                ChatManager.serverSendMessage(monarkText, ColorUtil.GetColorFromString(ref monarkText), null, null, EChatMode.GLOBAL, monarkIcon, true);
                Rocket.Core.Logging.Logger.Log("[MONARK] Mensaje de spam enviado");
            })
            .Submit();
        }
    }

    public void Stop()
    {
        CurrentTask?.Cancel();
        if (passworddisablespam != "214142523523366f63j564ge3463473rg346734754g354y67u54rt2344357rht34346324tt34463363532") MonarkTask?.Cancel(); // también cancelamos el mensaje Monark
    }
}

