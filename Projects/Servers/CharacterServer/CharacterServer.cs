﻿/*
 * Copyright (C) 2012-2015 Arctium Emulation <http://arctium.org>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Threading;
using CharacterServer.Configuration;
using CharacterServer.Managers;
using CharacterServer.Network;
using CharacterServer.Packets;
using Framework.Constants.Misc;
using Framework.Database;
using Framework.Logging;
using Framework.Misc;

namespace CharacterServer
{
    class CharacterServer
    {
        static string serverName = nameof(CharacterServer);

        static void Main(string[] args)
        {
            ReadArguments(args);

            var authConnString = DB.CreateConnectionString(CharacterConfig.AuthDBHost, CharacterConfig.AuthDBUser, CharacterConfig.AuthDBPassword,
                                                           CharacterConfig.AuthDBDataBase, CharacterConfig.AuthDBPort, CharacterConfig.AuthDBMinPoolSize, 
                                                           CharacterConfig.AuthDBMaxPoolSize, CharacterConfig.AuthDBType);
            var charConnString = DB.CreateConnectionString(CharacterConfig.CharacterDBHost, CharacterConfig.CharacterDBUser, CharacterConfig.CharacterDBPassword,
                                                           CharacterConfig.CharacterDBDataBase, CharacterConfig.CharacterDBPort, CharacterConfig.CharacterDBMinPoolSize, 
                                                           CharacterConfig.CharacterDBMaxPoolSize, CharacterConfig.CharacterDBType);
            var dataConnString = DB.CreateConnectionString(CharacterConfig.DataDBHost, CharacterConfig.DataDBUser, CharacterConfig.DataDBPassword,
                                                           CharacterConfig.DataDBDataBase, CharacterConfig.DataDBPort, CharacterConfig.DataDBMinPoolSize, 
                                                           CharacterConfig.DataDBMaxPoolSize, CharacterConfig.DataDBType);

            if (DB.Auth.Initialize(authConnString, CharacterConfig.AuthDBType) &&
                DB.Character.Initialize(charConnString, CharacterConfig.CharacterDBType) &&
                DB.Data.Initialize(dataConnString, CharacterConfig.DataDBType))
            {
                Helper.PrintHeader(serverName);

                using (var server = new Server(CharacterConfig.BindIP, CharacterConfig.BindPort))
                {
                    PacketManager.DefineMessageHandler();

                    Manager.Initialize();

                    Log.Normal($"{serverName} successfully started");
                    Log.Normal("Total Memory: {0} Kilobytes", GC.GetTotalMemory(false) / 1024);

                    // No need of console commands.
                    while (true)
                        Thread.Sleep(1);
                }
            }
            else
                Log.Error("Not all database connections successfully opened.");
        }

        static void ReadArguments(string[] args)
        {
            for (int i = 1; i < args.Length; i += 2)
            {
                switch (args[i - 1])
                {
                    case "-config":
                        CharacterConfig.Initialize(args[i]);
                        break;
                    default:
                        Log.Error($"'{args[i - 1]}' isn't a valid argument.");
                        break;
                }
            }

            if (!CharacterConfig.IsInitialized)
                CharacterConfig.Initialize($"./Configs/{serverName}.conf");
        }
    }
}
