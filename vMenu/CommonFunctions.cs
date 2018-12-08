using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using Newtonsoft.Json;

namespace vMenuClient
{
    public class CommonFunctions : BaseScript
    {
        #region Variables
        private string currentScenario = "";
        private Vehicle previousVehicle;
        private StorageManager sm = new StorageManager();
        public MpPedDataManager MpPedDataManager = new MpPedDataManager();

        public bool driveToWpTaskActive = false;
        public bool driveWanderTaskActive = false;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public CommonFunctions() { }

        #region Get Localized Label Text
        /// <summary>
        /// Get the localized name from a text label (for classes that don't have BaseScript)
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string GetLocalizedName(string label) => GetLabelText(label);
        #endregion

        #region Get Localized Vehicle Display Name
        /// <summary>
        /// Get the localized model name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetVehDisplayNameFromModel(string name) => GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
        #endregion

        #region GetHashKey for other classes
        /// <summary>
        /// Get the hash key for the specified string.
        /// </summary>
        /// <param name="input">String to convert into a hash.</param>
        /// <returns>The has value of the input string.</returns>
        public uint GetHash(string input) => (uint)GetHashKey(input);
        #endregion

        #region DoesModelExist
        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelName">The model name</param>
        /// <returns></returns>
        public bool DoesModelExist(string modelName) => DoesModelExist((uint)GetHashKey(modelName));
        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelHash">The model hash</param>
        /// <returns></returns>
        public bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
        #endregion

        #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
        /// <summary>
        /// Get the vehicle from the specified player. If no player specified, then return the vehicle of the current player.
        /// Optionally specify the 'lastVehicle' (bool) parameter to return the last vehicle the player was in.
        /// </summary>
        /// <param name="ped">Get the vehicle for this player.</param>
        /// <param name="lastVehicle">If true, return the last vehicle, if false (default) return the current vehicle.</param>
        /// <returns>Returns a vehicle (int).</returns>
        //public int GetVehicle(int player = -1, bool lastVehicle = false)
        //{

        //};

        public Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }

        public Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return ped.LastVehicle;
            }
            else
            {
                if (ped.IsInVehicle())
                {
                    return ped.CurrentVehicle;
                }
            }
            return null;
        }

        public Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
        }
        #endregion

        #region GetVehicleModel (uint)(hash) from Entity/Vehicle (int)
        /// <summary>
        /// Get the vehicle model hash (as uint) from the specified (int) entity/vehicle.
        /// </summary>
        /// <param name="vehicle">Entity/vehicle.</param>
        /// <returns>Returns the (uint) model hash from a (vehicle) entity.</returns>
        public uint GetVehicleModel(int vehicle) => (uint)GetHashKey(GetEntityModel(vehicle).ToString());
        #endregion

        #region Drive Tasks (WIP)
        /// <summary>
        /// Todo
        /// </summary>
        public void DriveToWp(int style = 0)
        {

            ClearPedTasks(Game.PlayerPed.Handle);
            driveWanderTaskActive = false;
            driveToWpTaskActive = true;

            Vector3 waypoint = World.WaypointPosition;

            Vehicle veh = GetVehicle();
            uint model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveToCoordLongrange(Game.PlayerPed.Handle, veh.Handle, waypoint.X, waypoint.Y, waypoint.Z, GetVehicleModelMaxSpeed(model), style, 10f);
        }

        /// <summary>
        /// Todo
        /// </summary>
        public void DriveWander(int style = 0)
        {
            ClearPedTasks(Game.PlayerPed.Handle);
            driveWanderTaskActive = true;
            driveToWpTaskActive = false;

            Vehicle veh = GetVehicle();
            uint model = (uint)veh.Model.Hash;

            SetDriverAbility(Game.PlayerPed.Handle, 1f);
            SetDriverAggressiveness(Game.PlayerPed.Handle, 0f);

            TaskVehicleDriveWander(Game.PlayerPed.Handle, veh.Handle, GetVehicleModelMaxSpeed(model), style);
        }
        #endregion

        #region Quit session & Quit game
        /// <summary>
        /// Quit the current network session, but leaves you connected to the server so addons/resources are still streamed.
        /// </summary>
        public void QuitSession() => NetworkSessionEnd(true, true);

        /// <summary>
        /// Quit the game after 5 seconds.
        /// </summary>
        public async void QuitGame()
        {
            Notify.Info("The game will exit in 5 seconds.", true, true);
            Debug.WriteLine("Game will be terminated in 5 seconds, because the player used the Quit Game option in vMenu.");
            await Delay(5000);
            ForceSocialClubUpdate(); // bye bye
        }
        #endregion

        #region Teleport to player (or teleport into the player's vehicle)
        /// <summary>
        /// Teleport to the specified player id. (Optionally teleport into their vehicle).
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inVehicle"></param>
        public async void TeleportToPlayerAsync(int playerId, bool inVehicle = false)
        {
            // If the player exists.
            if (NetworkIsPlayerActive(playerId))
            {
                int playerPed = GetPlayerPed(playerId);
                if (Game.PlayerPed.Handle == playerPed)
                {
                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~teleport to yourself!");
                    return;
                }

                // Get the coords of the other player.
                Vector3 playerPos = GetEntityCoords(playerPed, true);

                // Then await the proper loading/teleporting.
                await TeleportToCoords(playerPos);

                // If the player should be teleported inside the other player's vehcile.
                if (inVehicle)
                {
                    // Is the other player inside a vehicle?
                    if (IsPedInAnyVehicle(playerPed, false))
                    {
                        // Get the vehicle of the specified player.
                        Vehicle vehicle = GetVehicle(Players[playerId]);
                        if (vehicle != null)
                        {
                            int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle.Handle));

                            // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                            if (vehicle.Exists() && !vehicle.IsDead && IsAnyVehicleSeatEmpty(vehicle.Handle))
                            {
                                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, (int)VehicleSeat.Any);
                                Notify.Success("Teleported into ~g~<C>" + GetPlayerName(playerId) + "</C>'s ~s~vehicle.");
                            }
                            // If there are not enough empty vehicle seats or the vehicle doesn't exist/is dead then notify the user.
                            else
                            {
                                // If there's only one seat on this vehicle, tell them that it's a one-seater.
                                if (totalVehicleSeats == 1)
                                {
                                    Notify.Error("This vehicle only has room for 1 player!");
                                }
                                // Otherwise, tell them there's not enough empty seats remaining.
                                else
                                {
                                    Notify.Error("Not enough empty vehicle seats remaining!");
                                }
                            }
                        }
                    }
                }
                // The player is not being teleported into the vehicle, so the teleporting is successfull.
                // Notify the user.
                else
                {
                    Notify.Success("Teleported to ~y~<C>" + GetPlayerName(playerId) + "</C>~s~.");
                }
            }
            // The specified playerId does not exist, notify the user of the error.
            else
            {
                Notify.Error(CommonErrors.PlayerNotFound, placeholderValue: "So the teleport has been cancelled."); ;
                return;
            }
        }
        #endregion

        #region Teleport To Coords
        /// <summary>
        /// Teleport the player to a specific location.
        /// </summary>
        /// <param name="pos">These are the target coordinates to teleport to.</param>
        public async Task TeleportToCoords(Vector3 pos, bool safeModeDisabled = false)
        {
            if (!safeModeDisabled)
            {
                RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);
                bool inCar = Game.PlayerPed.IsInVehicle() && GetVehicle().Driver == Game.PlayerPed;
                if (inCar)
                {
                    SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z);
                    FreezeEntityPosition(GetVehiclePedIsIn(Game.PlayerPed.Handle, false), true);
                }
                else
                {
                    SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                    FreezeEntityPosition(Game.PlayerPed.Handle, true);
                }

                int timer = GetGameTimer();
                bool failed = true;
                float outputZ = pos.Z;
                await Delay(100);
                var z = 0f;
                while (GetGameTimer() - timer < 5000)
                {
                    await Delay(0);
                    if (GetGroundZFor_3dCoord(pos.X, pos.Y, z, ref outputZ, true))
                    {
                        failed = false;
                        break;
                    }
                    //Debug.WriteLine(z.ToString());
                    z = z + 10f;
                    if (z > 900)
                    {
                        z = 5;
                    }
                }
                if (!failed)
                {
                    if (inCar)
                        SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, pos.X, pos.Y, outputZ);
                    else
                        SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, outputZ, false, false, false, true);
                }
                await Delay(200);
                failed = (IsEntityInWater(Game.PlayerPed.Handle) || GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 50f) ? true : failed;
                if (failed)
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, 1, false, true);
                    Vector3 safePos = pos;
                    safePos.Z = 810f;
                    var foundSafeSpot = GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 0, ref safePos, 0, 0, 0);
                    if (foundSafeSpot)
                    {
                        Notify.Alert("No suitable location found near target coordinates. Teleporting to the nearest suitable spawn location as a backup method.", true);
                        if (inCar)
                            SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, safePos.X, safePos.Y, safePos.Z);
                        else
                            SetEntityCoords(Game.PlayerPed.Handle, safePos.X, safePos.Y, safePos.Z, false, false, false, true);
                    }
                    else
                    {
                        Notify.Alert("Failed to find a suitable location, backup method #1 failed, only backup method #2 remains: Open your parachute!", true);
                        if (inCar)
                            SetPedCoordsKeepVehicle(Game.PlayerPed.Handle, pos.X, pos.Y, 810f);
                        else
                            SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, 810f, false, false, false, true);
                    }
                }
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle() && GetVehicle().Driver == Game.PlayerPed)
                {
                    SetEntityCoords(GetVehicle().Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                }
                else
                {
                    SetEntityCoords(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, false, false, false, true);
                }
            }
            if (Game.PlayerPed.IsInVehicle())
            {
                FreezeEntityPosition(GetVehiclePedIsIn(Game.PlayerPed.Handle, false), false);
            }
            else
            {
                Game.PlayerPed.IsPositionFrozen = false;
            }
        }

        /// <summary>
        /// Teleports to the player's waypoint. If no waypoint is set, notify the user.
        /// </summary>
        public async void TeleportToWp()
        {
            if (Game.IsWaypointActive)
            {
                var pos = World.WaypointPosition;
                pos.Z = Game.PlayerPed.Position.Z;
                await TeleportToCoords(pos);
            }
            else
            {
                Notify.Error("You need to set a waypoint first!");
            }
        }
        #endregion

        #region Kick Player
        /// <summary>
        /// Kick a user with the provided kick reason. Or ask the current player to provide a reason.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="askUserForReason"></param>
        /// <param name="reason"></param>
        public async void KickPlayer(Player player, bool askUserForReason, string providedReason = "You have been kicked.")
        {
            if (player != null)
            {
                // Default kick reason.
                string defaultReason = "You have been kicked.";
                bool cancel = false;
                // If we need to ask for the user's input and the default reason is the same as the provided reason, get the user input..
                if (askUserForReason && providedReason == defaultReason)
                {
                    string userInput = await GetUserInput(windowTitle: "Enter Kick Message", maxInputLength: 100);
                    // If the input is not invalid, set the kick reason to the user's custom message.
                    if (!string.IsNullOrEmpty(userInput))
                    {
                        defaultReason += $" Reason: {userInput}";
                    }
                    else
                    {
                        cancel = true;
                        Notify.Error("An invalid kick reason was provided. Action cancelled.", true, true);
                        return;
                    }
                }
                // If the provided reason is not the same as the default reason, set the kick reason to the provided reason.
                else if (providedReason != defaultReason)
                {
                    defaultReason = providedReason;
                }

                // Kick the player using the specified reason.
                if (!cancel)
                {
                    TriggerServerEvent("vMenu:KickPlayer", player.ServerId, defaultReason);
                    Log($"Attempting to kick player {player.Name} (server id: {player.ServerId}, client id: {player.Handle}).");
                }
                else
                {
                    Notify.Error("The kick action was cancelled because the kick reason was invalid.", true, true);
                }
            }
            else
            {
                Notify.Error("The selected player is somehow invalid, action aborted.", true, true);
            }
        }
        #endregion

        #region (Temp) Ban Player
        /// <summary>
        /// Bans the specified player.
        /// </summary>
        /// <param name="player">Player to ban.</param>
        /// <param name="forever">Ban forever or ban temporarily.</param>
        public async void BanPlayer(Player player, bool forever)
        {
            string banReason = await GetUserInput(windowTitle: "Enter Ban Reason", defaultText: "Banned by staff.", maxInputLength: 200);
            if (!string.IsNullOrEmpty(banReason) && banReason.Length > 1)
            {
                if (forever)
                {
                    TriggerServerEvent("vMenu:PermBanPlayer", player.ServerId, banReason);
                }
                else
                {
                    string banDurationHours = await GetUserInput(windowTitle: "Ban Duration (in hours) - Max: 720 (1 month)", defaultText: "1.5", maxInputLength: 10);
                    if (!string.IsNullOrEmpty(banDurationHours))
                    {
                        if (double.TryParse(banDurationHours, out double banHours))
                        {
                            TriggerServerEvent("vMenu:TempBanPlayer", player.ServerId, banHours, banReason);
                        }
                        else
                        {
                            if (int.TryParse(banDurationHours, out int banHoursInt))
                            {
                                TriggerServerEvent("vMenu:TempBanPlayer", player.ServerId, (double)banHoursInt, banReason);
                            }
                            else
                            {
                                Notify.Error(CommonErrors.InvalidInput);
                                TriggerEvent("chatMessage", $"[vMenu] The input is invalid or you cancelled the action, please try again.");
                            }
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        TriggerEvent("chatMessage", $"[vMenu] The input is invalid or you cancelled the action, please try again.");
                    }

                }
            }
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
                TriggerEvent("chatMessage", $"[vMenu] The input is invalid or you cancelled the action, please try again.");
            }
        }
        #endregion

        #region Kill Player/commit suicide options
        /// <summary>
        /// Kill player
        /// </summary>
        /// <param name="player"></param>
        public void KillPlayer(Player player) => TriggerServerEvent("vMenu:KillPlayer", player.ServerId);

        /// <summary>
        /// Kill yourself.
        /// </summary>
        public async void CommitSuicide()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                // Get the suicide animations ready.
                RequestAnimDict("mp_suicide");
                while (!HasAnimDictLoaded("mp_suicide"))
                {
                    await Delay(0);
                }
                // Decide if the death should be using a pill or a gun (randomly).
                uint? weaponHash = null;
                bool takePill = false;


                if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.PistolMk2))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_PISTOL_MK2");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.CombatPistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_COMBATPISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Pistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_PISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon((WeaponHash)(uint)GetHashKey("WEAPON_SNSPISTOL_MK2")))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_SNSPISTOL_MK2");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.SNSPistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_SNSPISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Pistol50))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_PISTOL50");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.HeavyPistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_HEAVYPISTOL");
                }
                else if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.VintagePistol))
                {
                    weaponHash = (uint)GetHashKey("WEAPON_VINTAGEPISTOL");
                }
                else
                {
                    takePill = true;
                }


                // If we take the pill, remove any weapons in our hands.
                if (takePill)
                {
                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_unarmed"), true);
                }
                // Otherwise, give the ped a gun.
                else if (weaponHash != null)
                {
                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)weaponHash, true);
                    SetPedDropsWeaponsWhenDead(Game.PlayerPed.Handle, true);
                }
                else
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_pistol_mk2"), 1, false, true);
                    SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("weapon_pistol_mk2"), true);
                    SetPedDropsWeaponsWhenDead(Game.PlayerPed.Handle, true);
                }

                // Play the animation for the pill or pistol suicide type. Pistol oddly enough does not have any sounds. Needs research.
                ClearPedTasks(Game.PlayerPed.Handle);
                TaskPlayAnim(Game.PlayerPed.Handle, "MP_SUICIDE", (takePill ? "pill" : "pistol"), 8f, -8f, -1, 270540800, 0, false, false, false);

                bool shot = false;
                while (true)
                {
                    float time = GetEntityAnimCurrentTime(Game.PlayerPed.Handle, "MP_SUICIDE", (takePill ? "pill" : "pistol"));
                    if (HasAnimEventFired(Game.PlayerPed.Handle, (uint)GetHashKey("Fire")) && !shot) // shoot the gun if the animation event is triggered.
                    {
                        ClearEntityLastDamageEntity(Game.PlayerPed.Handle);
                        SetPedShootsAtCoord(Game.PlayerPed.Handle, 0f, 0f, 0f, false);
                        shot = true;
                    }
                    if (time > (takePill ? 0.536f : 0.365f))
                    {
                        // Kill the player.
                        ClearEntityLastDamageEntity(Game.PlayerPed.Handle);
                        SetEntityHealth(Game.PlayerPed.Handle, 0);
                        break;
                    }
                    await Delay(0);
                }
                RemoveAnimDict("mp_suicide");
            }
            else
            {
                SetEntityHealth(Game.PlayerPed.Handle, 0);
            }


        }
        #endregion

        #region Summon Player
        /// <summary>
        /// Summon player.
        /// </summary>
        /// <param name="player"></param>
        public void SummonPlayer(Player player) => TriggerServerEvent("vMenu:SummonPlayer", player.ServerId);
        #endregion

        #region Spectate function
        public async void SpectatePlayer(Player player, bool forceDisable = false)
        {
            if (forceDisable)
            {
                NetworkSetInSpectatorMode(false, 0); // disable spectating.
            }
            else
            {
                if (player.Handle == Game.Player.Handle)
                {
                    if (NetworkIsInSpectatorMode())
                    {
                        DoScreenFadeOut(500);
                        while (IsScreenFadingOut()) await Delay(0);
                        NetworkSetInSpectatorMode(false, 0); // disable spectating.
                        DoScreenFadeIn(500);
                        Notify.Success("Stopped spectating.", false, true);
                    }
                    else
                    {
                        Notify.Alert("You can't spectate yourself.", false, true);
                    }
                }
                else
                {
                    DoScreenFadeOut(500);
                    while (IsScreenFadingOut()) await Delay(0);
                    NetworkSetInSpectatorMode(false, 0);
                    NetworkSetInSpectatorMode(true, player.Character.Handle);
                    DoScreenFadeIn(500);
                    Notify.Success($"You are now spectating ~g~<C>{GetSafePlayerName(player.Name)}</C>~s~.", false, true);
                }
            }

        }

        ///// <summary>
        ///// Toggle spectating for the specified player Id. Leave the player ID empty (or -1) to disable spectating.
        ///// </summary>
        ///// <param name="playerId"></param>
        //public async void SpectateAsync(int playerId = -1)
        //{
        //    // Stop spectating.
        //    if (NetworkIsInSpectatorMode() || playerId == -1)
        //    {
        //        //spectating = false;
        //        DoScreenFadeOut(100);
        //        await Delay(100);
        //        Notify.Info("Stopped spectating.", false, false);
        //        NetworkSetInSpectatorMode(false, Game.PlayerPed.Handle);
        //        DoScreenFadeIn(100);
        //        await Delay(100);
        //    }
        //    // Start spectating for the first time.
        //    else
        //    {
        //        //spectating = true;
        //        DoScreenFadeOut(100);
        //        await Delay(100);
        //        Notify.Info($"Spectating ~r~{GetPlayerName(playerId)}</C>~s~.", false, false);
        //        NetworkSetInSpectatorMode(true, GetPlayerPed(playerId));
        //        DoScreenFadeIn(100);
        //        await Delay(100);
        //    }
        //}
        #endregion

        #region Cycle Through Vehicle Seats
        /// <summary>
        /// Cycle to the next available seat.
        /// </summary>
        public void CycleThroughSeats()
        {

            // Create a new vehicle.
            Vehicle vehicle = GetVehicle();

            // If there are enough empty seats, continue.
            if (AreAnyVehicleSeatsFree(vehicle.Handle))
            {
                // Get the total seats for this vehicle.
                var maxSeats = GetVehicleModelNumberOfSeats((uint)GetEntityModel(vehicle.Handle));

                // If the player is currently in the "last" seat, start from the driver's position and loop through the seats.
                if (GetPedInVehicleSeat(vehicle.Handle, maxSeats - 2) == Game.PlayerPed.Handle)
                {
                    // Loop through all seats.
                    for (var seat = -1; seat < maxSeats - 2; seat++)
                    {
                        // If the seat is free, get in it and stop the loop.
                        if (vehicle.IsSeatFree((VehicleSeat)seat))
                        {
                            TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                            break;
                        }
                    }
                }
                // If the player is not in the "last" seat, loop through all the seats starrting from the driver's position.
                else
                {
                    var switchedPlace = false;
                    var passedCurrentSeat = false;
                    // Loop through all the seats.
                    for (var seat = -1; seat < maxSeats - 1; seat++)
                    {
                        // If this seat is the one the player is sitting on, set passedCurrentSeat to true.
                        // This way we won't just keep placing the ped in the 1st available seat, but actually the first "next" available seat.
                        if (!passedCurrentSeat && GetPedInVehicleSeat(vehicle.Handle, seat) == Game.PlayerPed.Handle)
                        {
                            passedCurrentSeat = true;
                        }

                        // Only if the current seat has been passed, check if the seat is empty and if so teleport into it and stop the loop.
                        if (passedCurrentSeat && IsVehicleSeatFree(vehicle.Handle, seat))
                        {
                            switchedPlace = true;
                            TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                            break;
                        }
                    }
                    // If the player was not switched, then that means there are not enough empty vehicle seats "after" the player, and the player was not sitting in the "last" seat.
                    // To fix this, loop through the entire vehicle again and place them in the first available seat.
                    if (!switchedPlace)
                    {
                        // Loop through all seats, starting at the drivers seat (-1), then moving up.
                        for (var seat = -1; seat < maxSeats - 1; seat++)
                        {
                            // If the seat is free, take it and break the loop.
                            if (IsVehicleSeatFree(vehicle.Handle, seat))
                            {
                                TaskWarpPedIntoVehicle(Game.PlayerPed.Handle, vehicle.Handle, seat);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                Notify.Alert("There are no more available seats to cycle through.");
            }
        }
        #endregion

        #region Spawn Vehicle
        #region Overload Spawn Vehicle Function
        /// <summary>
        /// Simple custom vehicle spawn function.
        /// </summary>
        /// <param name="vehicleName">Vehicle model name. If "custom" the user will be asked to enter a model name.</param>
        /// <param name="spawnInside">Warp the player inside the vehicle after spawning.</param>
        /// <param name="replacePrevious">Replace the previous vehicle of the player.</param>
        public async void SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
        {
            if (vehicleName == "custom")
            {
                // Get the result.
                string result = await GetUserInput(windowTitle: "Enter Vehicle Name");
                // If the result was not invalid.
                if (!string.IsNullOrEmpty(result))
                {
                    // Convert it into a model hash.
                    uint model = (uint)GetHashKey(result);
                    SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false, vehicleInfo: new VehicleInfo(),
                        saveName: null);
                }
                // Result was invalid.
                else
                {
                    Notify.Error(CommonErrors.InvalidInput);
                }
            }
            // Spawn the specified vehicle.
            else
            {
                SpawnVehicle(vehicleHash: (uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false,
                    vehicleInfo: new VehicleInfo(), saveName: null);
            }
        }
        #endregion

        #region Main Spawn Vehicle Function
        /// <summary>
        /// Spawns a vehicle.
        /// </summary>
        /// <param name="vehicleHash">Model hash of the vehicle to spawn.</param>
        /// <param name="spawnInside">Teleports the player into the vehicle after spawning.</param>
        /// <param name="replacePrevious">Replaces the previous vehicle of the player with the new one.</param>
        /// <param name="skipLoad">Does not attempt to load the vehicle, but will spawn it right a way.</param>
        /// <param name="vehicleInfo">All information needed for a saved vehicle to re-apply all mods.</param>
        /// <param name="saveName">Used to get/set info about the saved vehicle data.</param>
        public async void SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad, VehicleInfo vehicleInfo, string saveName = null)
        {
            float speed = 0f;
            float rpm = 0f;
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle tmpOldVehicle = GetVehicle();
                speed = GetEntitySpeedVector(tmpOldVehicle.Handle, true).Y; // get forward/backward speed only
                rpm = tmpOldVehicle.CurrentRPM;
                tmpOldVehicle = null;
            }


            var vehClass = GetVehicleClassFromName(vehicleHash);
            int modelClass = GetVehicleClassFromName(vehicleHash);
            if (!VehicleSpawner.allowedCategories[modelClass])
            {
                Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
                return;
            }

            if (!skipLoad)
            {
                bool successFull = await LoadModel(vehicleHash);
                if (!successFull || !IsModelAVehicle(vehicleHash))
                {
                    // Vehicle model is invalid.
                    Notify.Error(CommonErrors.InvalidModel);
                    return;
                }
            }

            Log("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");

            // Get the heading & position for where the vehicle should be spawned.
            Vector3 pos = (spawnInside) ? GetEntityCoords(Game.PlayerPed.Handle, true) : GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
            float heading = GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f);

            // If the previous vehicle exists...
            if (previousVehicle != null)
            {
                // And it's actually a vehicle (rather than another random entity type)
                if (previousVehicle.Exists() && previousVehicle.PreviouslyOwnedByPlayer &&
                    (previousVehicle.Occupants.Count() == 0 || previousVehicle.Driver.Handle == Game.PlayerPed.Handle))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious || !PermissionsManager.IsAllowed(Permission.VSDisableReplacePrevious))
                    {
                        // Delete it.
                        previousVehicle.PreviouslyOwnedByPlayer = false;
                        SetEntityAsMissionEntity(previousVehicle.Handle, true, true);
                        previousVehicle.Delete();
                    }
                    // Otherwise
                    else
                    {
                        if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_keep_spawned_vehicles_persistent))
                        {
                            // Set the vehicle to be no longer needed. This will make the game engine decide when it should be removed (when all players get too far away).
                            previousVehicle.IsPersistent = false;
                            previousVehicle.PreviouslyOwnedByPlayer = false;
                            previousVehicle.MarkAsNoLongerNeeded();
                        }
                    }
                    previousVehicle = null;
                }
            }

            if (Game.PlayerPed.IsInVehicle() && (replacePrevious || !PermissionsManager.IsAllowed(Permission.VSDisableReplacePrevious)))
            {
                if (GetVehicle().Driver == Game.PlayerPed)// && IsVehiclePreviouslyOwnedByPlayer(GetVehicle()))
                {
                    var tmpveh = GetVehicle();
                    SetVehicleHasBeenOwnedByPlayer(tmpveh.Handle, false);
                    SetEntityAsMissionEntity(tmpveh.Handle, true, true);

                    if (previousVehicle != null)
                    {
                        if (previousVehicle.Handle == tmpveh.Handle)
                        {
                            previousVehicle = null;
                        }
                    }
                    tmpveh.Delete();
                    Notify.Info("Your old car was removed to prevent your new car from glitching inside it. Next time, get out of your vehicle before spawning a new one if you want to keep your old one.");
                }
            }

            if (previousVehicle != null)
                previousVehicle.PreviouslyOwnedByPlayer = false;

            if (Game.PlayerPed.IsInVehicle())
                pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0, 8f, 0.1f);

            // Create the new vehicle and remove the need to hotwire the car.
            Vehicle vehicle = new Vehicle(CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z + 1f, heading, true, false))
            {
                NeedsToBeHotwired = false,
                PreviouslyOwnedByPlayer = true,
                IsPersistent = true
            };

            Log($"New vehicle, hash:{vehicleHash}, handle:{vehicle.Handle}, force-re-save-name:{(saveName ?? "NONE")}, created at x:{pos.X} y:{pos.Y} z:{(pos.Z + 1f)} " +
                $"heading:{heading}");

            // If spawnInside is true
            if (spawnInside)
            {
                // Set the vehicle's engine to be running.
                vehicle.IsEngineRunning = true;

                // Set the ped into the vehicle.
                new Ped(Game.PlayerPed.Handle).SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // If the vehicle is a helicopter and the player is in the air, set the blades to be full speed.
                if (vehicle.ClassType == VehicleClass.Helicopters && GetEntityHeightAboveGround(Game.PlayerPed.Handle) > 10.0f)
                {
                    SetHeliBladesFullSpeed(vehicle.Handle);
                }
                // If it's not a helicopter or the player is not in the air, set the vehicle on the ground properly.
                else
                {
                    vehicle.PlaceOnGround();
                }
            }

            // If mod info about the vehicle was specified, check if it's not null.
            if (saveName != null)
            {
                // Set the modkit so we can modify the car.
                SetVehicleModKit(vehicle.Handle, 0);

                // set the extras
                foreach (var extra in vehicleInfo.extras)
                {
                    if (DoesExtraExist(vehicle.Handle, extra.Key))
                        vehicle.ToggleExtra(extra.Key, extra.Value);
                }

                SetVehicleWheelType(vehicle.Handle, vehicleInfo.wheelType);
                SetVehicleMod(vehicle.Handle, 23, 0, vehicleInfo.customWheels);
                if (vehicle.Model.IsBike)
                {
                    SetVehicleMod(vehicle.Handle, 24, 0, vehicleInfo.customWheels);
                }
                ToggleVehicleMod(vehicle.Handle, 18, vehicleInfo.turbo);
                SetVehicleTyreSmokeColor(vehicle.Handle, vehicleInfo.colors["tyresmokeR"], vehicleInfo.colors["tyresmokeG"], vehicleInfo.colors["tyresmokeB"]);
                ToggleVehicleMod(vehicle.Handle, 20, vehicleInfo.tyreSmoke);
                ToggleVehicleMod(vehicle.Handle, 22, vehicleInfo.xenonHeadlights);
                SetVehicleLivery(vehicle.Handle, vehicleInfo.livery);

                SetVehicleColours(vehicle.Handle, vehicleInfo.colors["primary"], vehicleInfo.colors["secondary"]);
                SetVehicleInteriorColour(vehicle.Handle, vehicleInfo.colors["trim"]);
                SetVehicleDashboardColour(vehicle.Handle, vehicleInfo.colors["dash"]);

                SetVehicleExtraColours(vehicle.Handle, vehicleInfo.colors["pearlescent"], vehicleInfo.colors["wheels"]);

                SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo.plateText);
                SetVehicleNumberPlateTextIndex(vehicle.Handle, vehicleInfo.plateStyle);

                SetVehicleWindowTint(vehicle.Handle, vehicleInfo.windowTint);

                foreach (var mod in vehicleInfo.mods)
                {
                    SetVehicleMod(vehicle.Handle, mod.Key, mod.Value, vehicleInfo.customWheels);
                }
                vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(red: vehicleInfo.colors["neonR"], green: vehicleInfo.colors["neonG"], blue: vehicleInfo.colors["neonB"]);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vehicleInfo.neonLeft);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vehicleInfo.neonRight);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vehicleInfo.neonFront);
                vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vehicleInfo.neonBack);
            }

            // Set the previous vehicle to the new vehicle.
            previousVehicle = vehicle;
            //vehicle.Speed = speed; // retarded feature that randomly breaks for no fucking reason
            if (!vehicle.Model.IsTrain) // to be extra fucking safe
            {
                // workaround of retarded feature above:
                SetVehicleForwardSpeed(vehicle.Handle, speed);
            }
            vehicle.CurrentRPM = rpm;

            // Discard the model.
            SetModelAsNoLongerNeeded(vehicleHash);
        }
        #endregion
        #endregion

        #region VehicleInfo struct
        /// <summary>
        /// Contains all information for a saved vehicle.
        /// </summary>
        public struct VehicleInfo
        {
            public Dictionary<string, int> colors;
            public bool customWheels;
            public Dictionary<int, bool> extras;
            public int livery;
            public uint model;
            public Dictionary<int, int> mods;
            public string name;
            public bool neonBack;
            public bool neonFront;
            public bool neonLeft;
            public bool neonRight;
            public string plateText;
            public int plateStyle;
            public bool turbo;
            public bool tyreSmoke;
            public int version;
            public int wheelType;
            public int windowTint;
            public bool xenonHeadlights;
        };
        #endregion

        #region Save Vehicle
        /// <summary>
        /// Saves the vehicle the player is currently in to the client's kvp storage.
        /// </summary>
        public async void SaveVehicle(string updateExistingSavedVehicleName = null)
        {
            // Only continue if the player is in a vehicle.
            if (Game.PlayerPed.IsInVehicle())
            {
                // Get the vehicle.
                Vehicle veh = GetVehicle();
                // Make sure the entity is actually a vehicle and it still exists, and it's not dead.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.IsDriveable)
                {
                    #region new saving method
                    Dictionary<int, int> mods = new Dictionary<int, int>();

                    foreach (var mod in veh.Mods.GetAllMods())
                    {
                        mods.Add((int)mod.ModType, mod.Index);
                    }

                    #region colors
                    var colors = new Dictionary<string, int>();
                    int primaryColor = 0;
                    int secondaryColor = 0;
                    int pearlescentColor = 0;
                    int wheelColor = 0;
                    int dashColor = 0;
                    int trimColor = 0;
                    GetVehicleExtraColours(veh.Handle, ref pearlescentColor, ref wheelColor);
                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref trimColor);
                    colors.Add("primary", primaryColor);
                    colors.Add("secondary", secondaryColor);
                    colors.Add("pearlescent", pearlescentColor);
                    colors.Add("wheels", wheelColor);
                    colors.Add("dash", dashColor);
                    colors.Add("trim", trimColor);
                    int neonR = 255;
                    int neonG = 255;
                    int neonB = 255;
                    if (veh.Mods.HasNeonLights)
                    {
                        GetVehicleNeonLightsColour(veh.Handle, ref neonR, ref neonG, ref neonB);
                    }
                    colors.Add("neonR", neonR);
                    colors.Add("neonG", neonG);
                    colors.Add("neonB", neonB);
                    int tyresmokeR = 0;
                    int tyresmokeG = 0;
                    int tyresmokeB = 0;
                    GetVehicleTyreSmokeColor(veh.Handle, ref tyresmokeR, ref tyresmokeG, ref tyresmokeB);
                    colors.Add("tyresmokeR", tyresmokeR);
                    colors.Add("tyresmokeG", tyresmokeG);
                    colors.Add("tyresmokeB", tyresmokeB);
                    #endregion

                    var extras = new Dictionary<int, bool>();
                    for (int i = 0; i < 20; i++)
                    {
                        if (veh.ExtraExists(i))
                        {
                            extras.Add(i, veh.IsExtraOn(i));
                        }
                    }

                    VehicleInfo vi = new VehicleInfo()
                    {
                        colors = colors,
                        customWheels = GetVehicleModVariation(veh.Handle, 23),
                        extras = extras,
                        livery = GetVehicleLivery(veh.Handle),
                        model = (uint)GetEntityModel(veh.Handle),
                        mods = mods,
                        name = GetLabelText(GetDisplayNameFromVehicleModel((uint)GetEntityModel(veh.Handle))),
                        neonBack = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back),
                        neonFront = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front),
                        neonLeft = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left),
                        neonRight = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right),
                        plateText = veh.Mods.LicensePlate,
                        plateStyle = (int)veh.Mods.LicensePlateStyle,
                        turbo = IsToggleModOn(veh.Handle, 18),
                        tyreSmoke = IsToggleModOn(veh.Handle, 20),
                        version = 1,
                        wheelType = GetVehicleWheelType(veh.Handle),
                        windowTint = (int)veh.Mods.WindowTint,
                        xenonHeadlights = IsToggleModOn(veh.Handle, 22)
                    };

                    #endregion

                    if (updateExistingSavedVehicleName == null)
                    {
                        // Ask the user for a save name (will be displayed to the user and will be used as unique identifier for this vehicle)
                        var saveName = await GetUserInput(windowTitle: "Enter a save name", maxInputLength: 30);
                        // If the name is not invalid.
                        if (saveName != "NULL")
                        {
                            // Save everything from the dictionary into the client's kvp storage.
                            // If the save was successfull:
                            if (StorageManager.SaveVehicleInfo("veh_" + saveName, vi, false))
                            {
                                Notify.Success($"Vehicle {saveName} saved.");
                            }
                            // If the save was not successfull:
                            else
                            {
                                Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: "(" + saveName + ")");
                            }
                        }
                        // The user did not enter a valid name to use as a save name for this vehicle.
                        else
                        {
                            Notify.Error(CommonErrors.InvalidSaveName);
                        }
                    }
                    // We need to update an existing slot.
                    else
                    {
                        StorageManager.SaveVehicleInfo("veh_" + updateExistingSavedVehicleName, vi, true);
                    }

                }
                // The player is not inside a vehicle, or the vehicle is dead/not existing so we won't do anything. Only alert the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle, placeholderValue: "to save it");
                }
            }
            // The player is not inside a vehicle.
            else
            {
                Notify.Error(CommonErrors.NoVehicle);
            }

            // update the saved vehicles menu list to reflect the new saved car.
            if (MainMenu.SavedVehiclesMenu != null)
            {
                MainMenu.SavedVehiclesMenu.UpdateMenuAvailableCategories();
            }

        }
        #endregion

        #region Loading Saved Vehicle
        /// <summary>
        /// New updated function to get the saved vehicle info from storage.
        /// </summary>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public VehicleInfo GetSavedVehicleInfo(string saveName) => sm.GetSavedVehicleInfo(saveName);
        #endregion

        #region Get Saved Vehicles Dictionary
        /// <summary>
        /// Returns a collection of all saved vehicles, with their save name and saved vehicle info struct.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, VehicleInfo> GetSavedVehicles()
        {
            // Create a list to store all saved vehicle names in.
            var savedVehicleNames = new List<string>();
            // Start looking for kvps starting with veh_
            var findHandle = StartFindKvp("veh_");
            // Keep looking...
            while (true)
            {
                // Get the kvp string key.
                var vehString = FindKvp(findHandle);

                // If it exists then the key to the list.
                if (vehString != "" && vehString != null && vehString != "NULL")
                {
                    //Debug.WriteLine(vehString);
                    savedVehicleNames.Add(vehString);
                }
                // Otherwise stop.
                else
                {
                    EndFindKvp(findHandle);
                    break;
                }
            }

            // Create a Dictionary to store all vehicle information in.
            //var vehiclesList = new Dictionary<string, Dictionary<string, string>>();
            var vehiclesList = new Dictionary<string, VehicleInfo>();
            // Loop through all save names (keys) from the list above, convert the string into a dictionary 
            // and add it to the dictionary above, with the vehicle save name as the key.
            foreach (var saveName in savedVehicleNames)
            {
                vehiclesList.Add(saveName, sm.GetSavedVehicleInfo(saveName));
            }
            // Return the vehicle dictionary containing all vehicle save names (keys) linked to the correct vehicle
            // including all vehicle mods/customization parts.
            return vehiclesList;
        }
        #endregion

        #region Load Model
        /// <summary>
        /// Check and load a model.
        /// </summary>
        /// <param name="modelHash"></param>
        /// <returns>True if model is valid & loaded, false if model is invalid.</returns>
        private async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (IsModelInCdimage(modelHash))
            {
                // Load the model.
                RequestModel(modelHash);
                // Wait until it's loaded.
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }
                // Model is loaded, return true.
                return true;
            }
            // Model is not valid or is not loaded correctly.
            else
            {
                // Return false.
                return false;
            }
        }
        #endregion

        #region GetUserInput
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        public async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
        /// <summary>
        /// Get a user input text string.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <param name="maxInputLength"></param>
        /// <returns></returns>
        public async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            var currentMenu = GetOpenMenu();
            if (currentMenu != null)
            {
                MainMenu.Mp.CloseAllMenus();
            }
            MainMenu.DisableControls = true;
            MainMenu.DontOpenMenus = true;

            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength.ToString()} Characters)");

            async void ReopenMenuDelayed(UIMenu menu)
            {
                MainMenu.DontOpenMenus = false;
                await Delay(100);
                if (menu != null)
                {
                    menu.Visible = true;
                }
                await Delay(50);
                MainMenu.DisableControls = false;
            }

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        ReopenMenuDelayed(currentMenu);
                        return null;
                    case 1: // finished editing
                        ReopenMenuDelayed(currentMenu);
                        return GetOnscreenKeyboardResult();
                    default:
                        await Delay(0);
                        break;
                }
            }
        }
        #endregion

        #region Set License Plate Text
        /// <summary>
        /// Set the license plate text using the player's custom input.
        /// </summary>
        public async void SetLicensePlateTextAsync()
        {
            // Get the input.
            var text = await GetUserInput(windowTitle: "Enter License Plate", maxInputLength: 8);
            // If the input is valid.
            if (!string.IsNullOrEmpty(text))
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // If it exists.
                if (veh != null && veh.Exists())
                {
                    // Set the license plate.
                    SetVehicleNumberPlateText(veh.Handle, text);
                }
                // If it doesn't exist, notify the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                    //Notification.Error("You're not inside a vehicle!");
                }
            }
            // No valid text was given.
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
                //Notification.Error($"License plate text ~r~{(text == "NULL" ? "(empty input)" : text)} ~s~can not be used on a license plate!");
            }

        }
        #endregion

        #region ToProperString()
        /// <summary>
        /// Converts a PascalCaseString to a Propper Case String.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Input string converted to a normal sentence.</returns>
        public string ToProperString(string inputString)
        {
            var outputString = "";
            var prevUpper = true;
            foreach (char c in inputString)
            {
                if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
                {
                    if (prevUpper)
                    {
                        outputString += $"{c.ToString()}";
                    }
                    else
                    {
                        outputString += $" {c.ToString()}";
                    }
                    prevUpper = true;
                }
                else
                {
                    prevUpper = false;
                    outputString += c.ToString();
                }
            }
            while (outputString.IndexOf("  ") != -1)
            {
                outputString = outputString.Replace("  ", " ");
            }
            return outputString;
        }

        #endregion

        #region Permissions
        /// <summary>
        /// Checks if the specified permission is granted for this user.
        /// Also checks parent/inherited/wildcard permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        //public bool IsAllowed(Permission permission) => true;
        public bool IsAllowed(Permission permission) => PermissionsManager.IsAllowed(permission);
        #endregion

        #region Play Scenarios
        /// <summary>
        /// Play the specified scenario name.
        /// If "forcestop" is specified, any currrently playing scenarios will be forcefully stopped.
        /// </summary>
        /// <param name="scenarioName"></param>
        public void PlayScenario(string scenarioName)
        {
            // If there's currently no scenario playing, or the current scenario is not the same as the new scenario, then..
            if (currentScenario == "" || currentScenario != scenarioName)
            {
                // Set the current scenario.
                currentScenario = scenarioName;
                // Clear all tasks to make sure the player is ready to play the scenario.
                ClearPedTasks(Game.PlayerPed.Handle);

                var canPlay = true;
                // Check if the player CAN play a scenario... 
                //if (IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
                //{
                //    Notification.Alert("You can't start a scenario when you are inside a vehicle.", true, false);
                //    canPlay = false;
                //}
                if (IsPedRunning(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are running.", true, false);
                    canPlay = false;
                }
                if (IsEntityDead(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are dead.", true, false);
                    canPlay = false;
                }
                if (IsPlayerInCutscene(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are in a cutscene.", true, false);
                    canPlay = false;
                }
                if (IsPedFalling(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are falling.", true, false);
                    canPlay = false;
                }
                if (IsPedRagdoll(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You can't start a scenario when you are currently in a ragdoll state.", true, false);
                    canPlay = false;
                }
                if (!IsPedOnFoot(Game.PlayerPed.Handle))
                {
                    Notify.Alert("You must be on foot to start a scenario.", true, false);
                    canPlay = false;
                }
                if (NetworkIsInSpectatorMode())
                {
                    Notify.Alert("You can't start a scenario when you are currently spectating.", true, false);
                    canPlay = false;
                }
                if (GetEntitySpeed(Game.PlayerPed.Handle) > 5.0f)
                {
                    Notify.Alert("You can't start a scenario when you are moving too fast.", true, false);
                    canPlay = false;
                }

                if (canPlay)
                {
                    // If the scenario is a "sit" scenario, then the scenario needs to be played at a specific location.
                    if (PedScenarios.PositionBasedScenarios.Contains(scenarioName))
                    {
                        // Get the offset-position from the player. (0.5m behind the player, and 0.5m below the player seems fine for most scenarios)
                        var pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, -0.5f, -0.5f);
                        var heading = GetEntityHeading(Game.PlayerPed.Handle);
                        // Play the scenario at the specified location.
                        TaskStartScenarioAtPosition(Game.PlayerPed.Handle, scenarioName, pos.X, pos.Y, pos.Z, heading, -1, true, false);
                    }
                    // If it's not a sit scenario (or maybe it is, but using the above native causes other
                    // issues for some sit scenarios so those are not registered as "sit" scenarios), then play it at the current player's position.
                    else
                    {
                        TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenarioName, 0, true);
                    }
                }
            }
            // If the new scenario is the same as the currently playing one, cancel the current scenario.
            else
            {
                currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedSecondaryTask(Game.PlayerPed.Handle);
            }

            // If the scenario name to play is called "forcestop" then clear the current scenario and force any tasks to be cleared.
            if (scenarioName == "forcestop")
            {
                currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedTasksImmediately(Game.PlayerPed.Handle);
            }

        }
        #endregion

        #region Data parsing functions
        /// <summary>
        /// New updated method for converting a KVP (string) json object (vehicle info data) to VehicleInfo struct.
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <param name="version"></param>
        /// <param name="saveName">Name used to re-save the info if necessary.</param>
        /// <returns></returns>
        public VehicleInfo JsonToVehicleInfo(dynamic jsonObject, int version, string saveName)
        {
            VehicleInfo vi = new VehicleInfo();
            if (version == 1)
            {
                var colors = new Dictionary<string, int>
                {
                    ["primary"] = jsonObject.colors.primary,
                    ["secondary"] = jsonObject.colors.secondary,
                    ["pearlescent"] = jsonObject.colors.pearlescent,
                    ["wheels"] = jsonObject.colors.wheels,
                    ["dash"] = jsonObject.colors.dash,
                    ["trim"] = jsonObject.colors.trim,
                    ["neonR"] = jsonObject.colors.neonR,
                    ["neonG"] = jsonObject.colors.neonG,
                    ["neonB"] = jsonObject.colors.neonB,
                    ["tyresmokeR"] = jsonObject.colors.tyresmokeR,
                    ["tyresmokeG"] = jsonObject.colors.tyresmokeG,
                    ["tyresmokeB"] = jsonObject.colors.tyresmokeB,
                };
                vi.colors = colors;

                vi.customWheels = jsonObject.customWheels;

                var extras = new Dictionary<int, bool>();
                foreach (KeyValuePair<int, bool> e in jsonObject.extras)
                {
                    extras.Add(e.Key, e.Value);
                }
                vi.extras = extras;

                vi.livery = jsonObject.livery;
                vi.model = jsonObject.model;

                var mods = new Dictionary<int, int>();
                foreach (KeyValuePair<int, int> m in jsonObject.mods)
                {
                    mods.Add(m.Key, m.Value);
                }
                vi.mods = mods;

                vi.name = jsonObject.name;
                vi.neonBack = jsonObject.neonBack;
                vi.neonFront = jsonObject.neonFront;
                vi.neonLeft = jsonObject.neonLeft;
                vi.neonRight = jsonObject.neonRight;
                vi.plateStyle = jsonObject.plateStyle;
                vi.plateText = jsonObject.plateText;
                vi.turbo = jsonObject.turbo;
                vi.tyreSmoke = jsonObject.tyreSmoke;
                vi.version = jsonObject.version;
                vi.wheelType = jsonObject.wheelType;
                vi.windowTint = jsonObject.windowTint;
                vi.xenonHeadlights = jsonObject.xenonHeadlights;
            }
            else if (version == 0)
            {
                var dict = JsonToDictionary(JsonConvert.SerializeObject(jsonObject));
                var colors = new Dictionary<string, int>()
                {
                    ["primary"] = int.Parse(dict["primaryColor"]),
                    ["secondary"] = int.Parse(dict["secondaryColor"]),
                    ["pearlescent"] = int.Parse(dict["pearlescentColor"]),
                    ["wheels"] = int.Parse(dict["wheelsColor"]),
                    ["dash"] = int.Parse(dict["dashboardColor"]),
                    ["trim"] = int.Parse(dict["interiorColor"]),
                    ["neonR"] = 255,
                    ["neonG"] = 255,
                    ["neonB"] = 255,
                    ["tyresmokeR"] = int.Parse(dict["tireSmokeR"]),
                    ["tyresmokeG"] = int.Parse(dict["tireSmokeG"]),
                    ["tyresmokeB"] = int.Parse(dict["tireSmokeB"]),
                };
                var extras = new Dictionary<int, bool>();
                for (int i = 0; i < 15; i++)
                {
                    if (dict["extra" + i] == "true")
                    {
                        extras.Add(i, true);
                    }
                    else
                    {
                        extras.Add(i, false);
                    }
                }

                var mods = new Dictionary<int, int>();
                int skip = 8 + 24 + 2 + 1;
                foreach (var mod in dict)
                {
                    skip--;
                    if (skip < 0)
                    {
                        var key = int.Parse(mod.Key);
                        var val = int.Parse(mod.Value);
                        mods.Add(key, val);
                    }
                }

                vi.colors = colors;
                vi.customWheels = dict["customWheels"] == "true";
                vi.extras = extras;
                vi.livery = int.Parse(dict["oldLivery"]);
                vi.model = (uint)Int64.Parse(dict["model"]);
                vi.mods = mods;
                vi.name = dict["name"];
                vi.neonBack = false;
                vi.neonFront = false;
                vi.neonLeft = false;
                vi.neonRight = false;
                vi.plateStyle = int.Parse(dict["plateStyle"]);
                vi.plateText = dict["plate"];
                vi.turbo = dict["turbo"] == "true";
                vi.tyreSmoke = dict["tireSmoke"] == "true";
                vi.version = 1;
                vi.wheelType = int.Parse(dict["wheelType"]);
                vi.windowTint = int.Parse(dict["windowTint"]);
                vi.xenonHeadlights = dict["xenonHeadlights"] == "true";

                StorageManager.SaveVehicleInfo(saveName, vi, true);
            }
            else
            {
                Notify.Error("An error occurred while converting a saved vehicle record. Unknown version to convert from.");
            }

            return vi;
        }

        /// <summary>
        /// Converts a simple json string (only containing (string) key : (string) value).
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Dictionary<string, string> JsonToDictionary(string json)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dynamic obj = JsonConvert.DeserializeObject(json);
            MainMenu.Cf.Log(obj.ToString());
            foreach (Newtonsoft.Json.Linq.JProperty item in obj)
            {
                dict.Add(item.Name, item.Value.ToString());
            }
            return dict;
        }
        #endregion

        #region Weather Sync
        /// <summary>
        /// Update the server with the new weather type, blackout status and dynamic weather changes enabled status.
        /// </summary>
        /// <param name="newWeather">The new weather type.</param>
        /// <param name="blackout">Manual blackout mode enabled/disabled.</param>
        /// <param name="dynamicChanges">Dynamic weather changes enabled/disabled.</param>
        public void UpdateServerWeather(string newWeather, bool blackout, bool dynamicChanges) => TriggerServerEvent("vMenu:UpdateServerWeather", newWeather, blackout, dynamicChanges);

        /// <summary>
        /// Modify the clouds for everyone. If removeClouds is true, then remove all clouds. If it's false, then randomize the clouds.
        /// </summary>
        /// <param name="removeClouds">Removes the clouds from the sky if true, otherwise randomizes the clouds type for all players.</param>
        public void ModifyClouds(bool removeClouds) => TriggerServerEvent("vMenu:UpdateServerWeatherCloudsType", removeClouds);
        #endregion

        #region Time Sync
        /// <summary>
        /// Update the server with the new time. If an invalid time is provided, then the time will be set to midnight (00:00);
        /// </summary>
        /// <param name="hours">Hours (0-23)</param>
        /// <param name="minutes">Minutes (0-59)</param>
        /// <param name="freezeTime">Should the time be frozen?</param>
        public void UpdateServerTime(int hours, int minutes, bool freezeTime)
        {
            var realHours = hours;
            var realMinutes = minutes;
            if (hours > 23 || hours < 0)
            {
                realHours = 0;
            }
            if (minutes > 59 || minutes < 0)
            {
                realMinutes = 0;
            }
            TriggerServerEvent("vMenu:UpdateServerTime", realHours, realMinutes, freezeTime);
        }


        #endregion

        #region StringToStringArray
        /// <summary>
        /// Converts the inputString into 1, 2 or 3 strings in a string[] (array).
        /// Each string in the array is up to 99 characters long at max.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>String[] containing 1, 2 or 3 strings.</returns>
        public string[] StringToArray(string inputString)
        {
            string[] outputString = new string[3];

            var lastSpaceIndex = 0;
            var newStartIndex = 0;
            outputString[0] = inputString;

            if (inputString.Length > 99)
            {
                for (int i = 0; i < inputString.Length; i++)
                {
                    if (inputString.Substring(i, 1) == " ")
                    {
                        lastSpaceIndex = i;
                    }

                    if (inputString.Length > 99 && i >= 98)
                    {
                        if (i == 98)
                        {
                            outputString[0] = inputString.Substring(0, lastSpaceIndex);
                            newStartIndex = lastSpaceIndex + 1;
                        }
                        if (i > 98 && i < 198)
                        {
                            if (i == 197)
                            {
                                outputString[1] = inputString.Substring(newStartIndex, (lastSpaceIndex - (outputString[0].Length - 1)) - (inputString.Length - 1 > 197 ? 1 : -1));
                                newStartIndex = lastSpaceIndex + 1;
                            }
                            else if (i == inputString.Length - 1 && inputString.Length < 198)
                            {
                                outputString[1] = inputString.Substring(newStartIndex, ((inputString.Length - 1) - outputString[0].Length));
                                newStartIndex = lastSpaceIndex + 1;
                            }
                        }
                        if (i > 197)
                        {
                            if (i == inputString.Length - 1 || i == 296)
                            {
                                outputString[2] = inputString.Substring(newStartIndex, ((inputString.Length - 1) - outputString[0].Length) - outputString[1].Length);
                            }
                        }
                    }
                }
            }

            return outputString;
        }
        #endregion

        #region Hud Functions

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        /// <param name="disableTextOutline">Disables the default text outline.</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (IsHudPreferenceSwitchedOn() && CitizenFX.Core.UI.Screen.Hud.IsVisible && !MainMenu.MiscSettingsMenu.HideHud && !IsPlayerSwitchInProgress() && IsScreenFadedIn() && !IsPauseMenuActive() && !IsFrontendFading() && !IsPauseMenuRestarting() && !IsHudHidden())
            {
                SetTextFont(font);
                SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    SetTextWrap(0f, xPosition);
                }
                SetTextJustification((int)justification);
                if (!disableTextOutline) { SetTextOutline(); }
                BeginTextCommandDisplayText("STRING");
                AddTextComponentSubstringPlayerName(text);
                EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
        #endregion

        #region ped (& ped mp info) info struct
        public struct MultiplayerPedInfo
        {
            // todo
        };

        public struct PedInfo
        {
            public int version;
            public uint model;
            public bool isMpPed;
            public MultiplayerPedInfo mpPedInfo;
            public Dictionary<int, int> props;
            public Dictionary<int, int> propTextures;
            public Dictionary<int, int> drawableVariations;
            public Dictionary<int, int> drawableVariationTextures;
        };
        #endregion

        #region Set Player Skin
        /// <summary>
        /// Sets the player's model to the provided modelName.
        /// </summary>
        /// <param name="modelName">The model name.</param>
        public async Task SetPlayerSkin(string modelName, PedInfo pedCustomizationOptions, bool keepWeapons = true) => await SetPlayerSkin((uint)GetHashKey(modelName), pedCustomizationOptions, keepWeapons);

        /// <summary>
        /// Sets the player's model to the provided modelHash.
        /// </summary>
        /// <param name="modelHash">The model hash.</param>
        public async Task SetPlayerSkin(uint modelHash, PedInfo pedCustomizationOptions, bool keepWeapons = true)
        {
            //uint model = modelHash;
            //Debug.Write(modelHash.ToString() + "\n");
            if (IsModelInCdimage(modelHash))
            {
                if (keepWeapons)
                {
                    await SaveWeaponLoadout();
                }
                RequestModel(modelHash);
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }

                if ((uint)GetEntityModel(Game.PlayerPed.Handle) != modelHash) // only change skins if the player is not yet using the new skin.
                {
                    SetPlayerModel(Game.Player.Handle, modelHash);
                }
                SetPedDefaultComponentVariation(Game.PlayerPed.Handle);

                if (pedCustomizationOptions.version == 1)
                {
                    var ped = Game.PlayerPed.Handle;
                    for (var drawable = 0; drawable < 21; drawable++)
                    {
                        SetPedComponentVariation(ped, drawable, pedCustomizationOptions.drawableVariations[drawable],
                            pedCustomizationOptions.drawableVariationTextures[drawable], 1);
                    }

                    for (var i = 0; i < 21; i++)
                    {
                        int prop = pedCustomizationOptions.props[i];
                        int propTexture = pedCustomizationOptions.propTextures[i];
                        if (prop == -1 || propTexture == -1)
                        {
                            ClearPedProp(ped, i);
                        }
                        else
                        {
                            SetPedPropIndex(ped, i, prop, propTexture, true);
                        }
                    }
                }
                else if (pedCustomizationOptions.version == -1)
                {
                    // do nothing.
                }
                else
                {
                    // notify user of unsupported version
                    Notify.Error("This is an unsupported saved ped version. Cannot restore appearance. :(");
                }
                if (keepWeapons)
                {
                    RestoreWeaponLoadout();
                }
                if (modelHash == (uint)GetHashKey("mp_f_freemode_01") || modelHash == (uint)GetHashKey("mp_m_freemode_01"))
                {
                    var headBlendData = Game.PlayerPed.GetHeadBlendData();
                    if (pedCustomizationOptions.version == -1)
                    {
                        SetPedHeadBlendData(Game.PlayerPed.Handle, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                        while (!HasPedHeadBlendFinished(Game.PlayerPed.Handle))
                        {
                            await Delay(0);
                        }
                    }
                }
                SetModelAsNoLongerNeeded(modelHash);
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }

        /// <summary>
        /// Set the player model by asking for user input.
        /// </summary>
        public async void SpawnPedByName()
        {
            string input = await GetUserInput(windowTitle: "Enter Ped Model Name", maxInputLength: 30);
            if (!string.IsNullOrEmpty(input))
            {
                await SetPlayerSkin((uint)GetHashKey(input), new PedInfo() { version = -1 });
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }
        #endregion

        #region Save Ped Model + Customizations
        /// <summary>
        /// Saves the current player ped.
        /// </summary>
        public async void SavePed(string forceName = null)
        {
            string name = forceName;
            if (string.IsNullOrEmpty(name))
            {
                // Get the save name.
                name = await GetUserInput(windowTitle: "Enter a ped save name", maxInputLength: 30);
            }

            // If the save name is not invalid.
            if (!string.IsNullOrEmpty(name))
            {
                // Create a dictionary to store all data in.
                //Dictionary<string, string> pedData = new Dictionary<string, string>();
                PedInfo data = new PedInfo();

                // Get the ped.
                int ped = Game.PlayerPed.Handle;

                data.version = 1;
                // Get the ped model hash & add it to the dictionary.
                uint model = (uint)GetEntityModel(ped);
                data.model = model;

                // Loop through all drawable variations.
                var drawables = new Dictionary<int, int>();
                var drawableTextures = new Dictionary<int, int>();
                for (var i = 0; i < 21; i++)
                {
                    int drawable = GetPedDrawableVariation(ped, i);
                    int textureVariation = GetPedTextureVariation(ped, i);
                    drawables.Add(i, drawable);
                    drawableTextures.Add(i, textureVariation);
                }
                data.drawableVariations = drawables;
                data.drawableVariationTextures = drawableTextures;

                var props = new Dictionary<int, int>();
                var propTextures = new Dictionary<int, int>();
                // Loop through all prop variations.
                for (var i = 0; i < 21; i++)
                {
                    int prop = GetPedPropIndex(ped, i);
                    int propTexture = GetPedPropTextureIndex(ped, i);
                    props.Add(i, prop);
                    propTextures.Add(i, propTexture);
                }
                data.props = props;
                data.propTextures = propTextures;

                data.isMpPed = (model == (uint)GetHashKey("mp_f_freemode_01") || model == (uint)GetHashKey("mp_m_freemode_01"));
                data.mpPedInfo = new MultiplayerPedInfo();

                // Try to save the data, and save the result in a variable.
                //bool saveSuccessful = sm.SaveDictionary("ped_" + name, pedData, false);
                bool saveSuccessful = false;
                if (name == "vMenu_tmp_saved_ped")
                {
                    saveSuccessful = StorageManager.SavePedInfo(name, data, true);
                }
                else
                {
                    saveSuccessful = StorageManager.SavePedInfo("ped_" + name, data, false);
                }


                if (name != "vMenu_tmp_saved_ped") // only send a notification if the save wasn't triggered because the player died.
                {
                    // If the save was successfull.
                    if (saveSuccessful)
                    {
                        Notify.Success("Ped saved.");
                    }
                    // Save was not successfull.
                    else
                    {
                        Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: name);
                    }
                }

            }
            // User cancelled the saving or they did not enter a valid name.
            else
            {
                Notify.Error(CommonErrors.InvalidSaveName);
            }
        }
        #endregion

        #region Load Saved Ped
        /// <summary>
        /// Load the saved ped and spawn it.
        /// </summary>
        /// <param name="savedName">The ped saved name</param>
        public async void LoadSavedPed(string savedName, bool restoreWeapons)
        {
            if (savedName != "vMenu_tmp_saved_ped")
            {
                PedInfo pi = sm.GetSavedPedInfo("ped_" + savedName);
                Log(JsonConvert.SerializeObject(pi));
                await SetPlayerSkin(pi.model, pi, restoreWeapons);
            }
            else
            {
                PedInfo pi = sm.GetSavedPedInfo(savedName);
                Log(JsonConvert.SerializeObject(pi));
                await SetPlayerSkin(pi.model, pi, restoreWeapons);
                DeleteResourceKvp("vMenu_tmp_saved_ped");
            }

        }


        public bool GetPedInfoFromBeforeDeath()
        {
            if (!string.IsNullOrEmpty(GetResourceKvpString("vMenu_tmp_saved_ped")))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region saved ped json string to ped info
        /// <summary>
        /// Load and convert json ped info into PedInfo struct.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="saveName"></param>
        /// <returns></returns>
        public PedInfo JsonToPedInfo(string json, string saveName)
        {
            var pi = new PedInfo() { version = -1 };
            dynamic obj = JsonConvert.DeserializeObject(json);
            if (json.Contains("version")) // new ped save
            {
                pi.model = (uint)obj["model"];
                Dictionary<int, int> drawables = new Dictionary<int, int>();
                Dictionary<int, int> drawableTextures = new Dictionary<int, int>();
                Dictionary<int, int> props = new Dictionary<int, int>();
                Dictionary<int, int> propTextures = new Dictionary<int, int>();
                foreach (Newtonsoft.Json.Linq.JProperty drawable in obj["drawableVariations"])
                {
                    drawables.Add(int.Parse(drawable.Name.ToString()), (int)drawable.Value);
                }
                foreach (Newtonsoft.Json.Linq.JProperty drawableVar in obj["drawableVariationTextures"])
                {
                    drawableTextures.Add(int.Parse(drawableVar.Name.ToString()), (int)drawableVar.Value);
                }
                foreach (Newtonsoft.Json.Linq.JProperty prop in obj["props"])
                {
                    props.Add(int.Parse(prop.Name.ToString()), (int)prop.Value);
                }
                foreach (Newtonsoft.Json.Linq.JProperty propVar in obj["propTextures"])
                {
                    propTextures.Add(int.Parse(propVar.Name.ToString()), (int)propVar.Value);
                }
                pi.drawableVariations = drawables;
                pi.drawableVariationTextures = drawableTextures;
                pi.props = props;
                pi.propTextures = propTextures;
                pi.isMpPed = (bool)obj["isMpPed"];
                pi.mpPedInfo = new MultiplayerPedInfo();
                pi.version = (int)obj["version"];
            }
            else // old ped save
            {
                Dictionary<int, int> drawables = new Dictionary<int, int>();
                Dictionary<int, int> drawableTextures = new Dictionary<int, int>();
                Dictionary<int, int> props = new Dictionary<int, int>();
                Dictionary<int, int> propTextures = new Dictionary<int, int>();
                uint model = (uint)Int64.Parse(obj["modelHash"].ToString());
                foreach (Newtonsoft.Json.Linq.JProperty i in obj)
                {
                    string key = i.Name.ToString();
                    if (key.Contains("drawable_variation"))
                    {
                        drawables.Add(int.Parse(key.Substring(19)), (int)i.Value);
                    }
                    else if (key.Contains("drawable_texture"))
                    {
                        drawableTextures.Add(int.Parse(key.Substring(17)), (int)i.Value);
                    }
                    else if (key.Contains("prop_texture"))
                    {
                        propTextures.Add(int.Parse(key.Substring(13)), (int)i.Value);
                    }
                    else if (key.Contains("prop"))
                    {
                        props.Add(int.Parse(key.Split('_')[1]), (int)i.Value);
                    }
                }
                pi.drawableVariations = drawables;
                pi.model = model;
                pi.drawableVariationTextures = drawableTextures;
                pi.mpPedInfo = new MultiplayerPedInfo() { };
                pi.isMpPed = (model == (uint)GetHashKey("mp_f_freemode_01") || model == (uint)GetHashKey("mp_m_freemode_01"));
                pi.props = props;
                pi.propTextures = propTextures;
                pi.version = 1;
                if (StorageManager.SavePedInfo(saveName, pi, true))
                {
                    Notify.Success("Converted saved ped successfully.");
                }
                else
                {
                    Notify.Error("Could not convert saved ped! Reason: unknown.");
                }
            }
            return pi;
        }
        #endregion

        #region Save and restore weapon loadouts when changing models
        private struct WeaponInfo
        {
            public int Ammo;
            public uint Hash;
            public List<uint> Components;
            public int Tint;
        }

        private List<WeaponInfo> weaponsList = new List<WeaponInfo>();

        /// <summary>
        /// Saves all current weapons and components.
        /// </summary>
        public async Task SaveWeaponLoadout()
        {
            //await Delay(1);
            weaponsList.Clear();
            //await Delay(1);
            foreach (var vw in ValidWeapons.Weapons)
            {
                if (HasPedGotWeapon(Game.PlayerPed.Handle, vw.Value, false))
                {
                    List<uint> components = new List<uint>();
                    foreach (var wc in ValidWeapons.weaponComponents)
                    {
                        if (DoesWeaponTakeWeaponComponent(vw.Value, wc.Value))
                        {
                            if (HasPedGotWeaponComponent(Game.PlayerPed.Handle, vw.Value, wc.Value))
                            {
                                components.Add(wc.Value);
                            }
                        }
                    }
                    weaponsList.Add(new WeaponInfo()
                    {
                        Ammo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, vw.Value),
                        Hash = vw.Value,
                        Components = components,
                        Tint = GetPedWeaponTintIndex(Game.PlayerPed.Handle, vw.Value)
                    });
                }
            }
            await Delay(0);
        }

        /// <summary>
        /// Restores all weapons and components
        /// </summary>
        public async void RestoreWeaponLoadout()
        {
            await Delay(0);
            if (weaponsList.Count > 0)
            {
                foreach (WeaponInfo wi in weaponsList)
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, wi.Hash, wi.Ammo, false, false);
                    if (wi.Components.Count > 0)
                    {
                        foreach (var wc in wi.Components)
                        {
                            GiveWeaponComponentToPed(Game.PlayerPed.Handle, wi.Hash, wc);
                        }
                    }
                    SetPedWeaponTintIndex(Game.PlayerPed.Handle, wi.Hash, wi.Tint);
                }
            }
        }
        #endregion

        #region Get "Header" Menu Item
        /// <summary>
        /// Get a header menu item (text-centered, disabled UIMenuItem)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public UIMenuItem GetSpacerMenuItem(string title, string description = null)
        {
            string output = "~h~";
            int length = title.Length;
            int totalSize = 80 - length;

            for (var i = 0; i < totalSize / 2 - (length / 2); i++)
            {
                output += " ";
            }
            output += title;
            UIMenuItem item = new UIMenuItem(output, description ?? "")
            {
                Enabled = false
            };
            return item;
        }
        #endregion

        #region Log Function
        /// <summary>
        /// Print data to the console and save it to the CitizenFX.log file. Only when vMenu debugging mode is enabled.
        /// </summary>
        /// <param name="data"></param>
        public void Log(string data)
        {
            if (MainMenu.DebugMode)
                Debug.Write(data + "\n");
            //Debug.WriteLine(data.Replace("{", "{{").Replace("}", "}}"), "");
        }
        #endregion

        #region Get Currently Opened Menu
        /// <summary>
        /// Returns the currently opened menu, if no menu is open, it'll return null.
        /// </summary>
        /// <returns></returns>
        public UIMenu GetOpenMenu()
        {
            if (MainMenu.Mp.IsAnyMenuOpen())
            {
                foreach (UIMenu m in MainMenu.Mp.ToList())
                {
                    if (m.Visible)
                    {
                        return m;
                    }
                }
            }
            return null;

        }
        #endregion

        #region Weapon Options
        /// <summary>
        /// Set the ammo for all weapons in inventory to the custom amount entered by the user.
        /// </summary>
        public async void SetAllWeaponsAmmo()
        {
            int ammo = 100;
            string inputAmmo = await GetUserInput(windowTitle: "Enter Ammo Amount", defaultText: "100");

            if (!string.IsNullOrEmpty(inputAmmo))
            {
                ammo = int.Parse(inputAmmo);
                Ped ped = Game.PlayerPed;
                foreach (var wp in ValidWeapons.Weapons)
                {
                    if (ped.Weapons.HasWeapon((WeaponHash)wp.Value))
                    {
                        SetPedAmmo(ped.Handle, wp.Value, ammo);
                    }
                }
            }
            else
            {
                Notify.Error("You did not enter a valid ammo count.");
            }
        }

        /// <summary>
        /// Spawn a weapon by asking the player for the weapon name.
        /// </summary>
        public async void SpawnCustomWeapon()
        {
            int ammo = 900;
            string inputName = await GetUserInput(windowTitle: "Enter Weapon Model Name", maxInputLength: 30);
            if (!string.IsNullOrEmpty(inputName))
            {
                var model = (uint)GetHashKey(inputName.ToUpper());

                if (IsWeaponValid(model))
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, model, ammo, false, true);
                    Notify.Success("Added weapon to inventory.");
                }
                else
                {
                    Notify.Error($"This ({inputName.ToString()}) is not a valid weapon model name, or the model hash ({model.ToString()}) could not be found in the game files.");
                }
            }
            else
            {
                Notify.Error($"This ({inputName.ToString()}) is not a valid weapon model name.");
            }
        }
        #endregion

        #region Set Player Walking Style
        public async void SetWalkingStyle(string walkingStyle)
        {
            if (IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_f_freemode_01")) || IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01")))
            {
                bool isPedMale = IsPedModel(Game.PlayerPed.Handle, (uint)GetHashKey("mp_m_freemode_01"));
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, 1f);
                ClearPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, 1f);
                ClearPedAlternateWalkAnim(Game.PlayerPed.Handle, 1f);
                string animDict = null;
                if (walkingStyle == "Injured")
                {
                    animDict = isPedMale ? "move_m@injured" : "move_f@injured";
                }
                else if (walkingStyle == "Tough Guy")
                {
                    animDict = isPedMale ? "move_m@tough_guy@" : "move_f@tough_guy@";
                }
                else if (walkingStyle == "Femme")
                {
                    animDict = isPedMale ? "move_m@femme@" : "move_f@femme@";
                }
                else if (walkingStyle == "Gangster")
                {
                    animDict = isPedMale ? "move_m@gangster@a" : "move_f@gangster@ng";
                }
                else if (walkingStyle == "Posh")
                {
                    animDict = isPedMale ? "move_m@posh@" : "move_f@posh@";
                }
                else if (walkingStyle == "Sexy")
                {
                    animDict = isPedMale ? null : "move_f@sexy@a";
                }
                else if (walkingStyle == "Business")
                {
                    animDict = isPedMale ? null : "move_f@business@a";
                }
                else if (walkingStyle == "Drunk")
                {
                    animDict = isPedMale ? "move_m@drunk@a" : "move_f@drunk@a";
                }
                else if (walkingStyle == "Hipster")
                {
                    animDict = isPedMale ? "move_m@hipster@a" : null;
                }
                if (animDict != null)
                {
                    if (!HasAnimDictLoaded(animDict))
                    {
                        RequestAnimDict(animDict);
                        while (!HasAnimDictLoaded(animDict))
                        {
                            await Delay(0);
                        }
                    }
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 0, animDict, "idle", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 1, animDict, "walk", 1f, true);
                    SetPedAlternateMovementAnim(Game.PlayerPed.Handle, 2, animDict, "run", 1f, true);
                }
                else if (walkingStyle != "Normal")
                {
                    if (isPedMale)
                    {
                        Notify.Error(CommonErrors.WalkingStyleNotForMale);
                    }
                    else
                    {
                        Notify.Error(CommonErrors.WalkingStyleNotForFemale);
                    }
                }
            }
            else
            {
                Notify.Error("This feature only supports the multiplayer freemode male/female ped models.");
            }
        }
        #endregion

        #region Disable Movement Controls
        public void DisableMovementControlsThisFrame(bool disableMovement, bool disableCameraMovement)
        {
            if (disableMovement)
            {
                Game.DisableControlThisFrame(0, Control.MoveDown);
                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeft);
                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
                Game.DisableControlThisFrame(0, Control.MoveRight);
                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
                Game.DisableControlThisFrame(0, Control.MoveUp);
                Game.DisableControlThisFrame(0, Control.MoveUpDown);
                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleFlyMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDown);
                Game.DisableControlThisFrame(0, Control.VehicleMoveDownOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeft);
                Game.DisableControlThisFrame(0, Control.VehicleMoveLeftRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRight);
                Game.DisableControlThisFrame(0, Control.VehicleMoveRightOnly);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUp);
                Game.DisableControlThisFrame(0, Control.VehicleMoveUpDown);
                Game.DisableControlThisFrame(0, Control.VehicleSubMouseControlOverride);
                Game.DisableControlThisFrame(0, Control.Duck);
                Game.DisableControlThisFrame(0, Control.SelectWeapon);
            }
            if (disableCameraMovement)
            {
                Game.DisableControlThisFrame(0, Control.LookBehind);
                Game.DisableControlThisFrame(0, Control.LookDown);
                Game.DisableControlThisFrame(0, Control.LookDownOnly);
                Game.DisableControlThisFrame(0, Control.LookLeft);
                Game.DisableControlThisFrame(0, Control.LookLeftOnly);
                Game.DisableControlThisFrame(0, Control.LookLeftRight);
                Game.DisableControlThisFrame(0, Control.LookRight);
                Game.DisableControlThisFrame(0, Control.LookRightOnly);
                Game.DisableControlThisFrame(0, Control.LookUp);
                Game.DisableControlThisFrame(0, Control.LookUpDown);
                Game.DisableControlThisFrame(0, Control.LookUpOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookDownOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftOnly);
                Game.DisableControlThisFrame(0, Control.ScaledLookLeftRight);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpDown);
                Game.DisableControlThisFrame(0, Control.ScaledLookUpOnly);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook);
                Game.DisableControlThisFrame(0, Control.VehicleDriveLook2);
                Game.DisableControlThisFrame(0, Control.VehicleLookBehind);
                Game.DisableControlThisFrame(0, Control.VehicleLookLeft);
                Game.DisableControlThisFrame(0, Control.VehicleLookRight);
                Game.DisableControlThisFrame(0, Control.NextCamera);
                Game.DisableControlThisFrame(0, Control.VehicleFlyAttackCamera);
                Game.DisableControlThisFrame(0, Control.VehicleCinCam);
            }
        }
        #endregion

        #region Multiplayer Characters

        #region Save MP Character
        //public async void SaveMultiplayerCharacter(PlayerAppearance.MpCharacterStyle currentCharacter)
        //{
        //    string character = JsonConvert.SerializeObject(currentCharacter);
        //    string saveName = await GetUserInput("Enter a save name", "", 20) ?? "NULL";
        //    if (!string.IsNullOrEmpty(saveName) && saveName != "NULL")
        //    {
        //        if (!sm.SaveJsonData("mp_char_" + saveName, character, false))
        //        {
        //            Notify.Error("Your character could not be saved, is the save name already taken?", true, true);
        //        }
        //        else
        //        {
        //            Notify.Success($"Your character is now saved as <C>{saveName}</C>!");
        //        }
        //    }
        //    else
        //    {
        //        Notify.Error("Your character could not be saved, the provided save name is invalid.", true, true);
        //    }
        //}

        //public PlayerAppearance.MpCharacterStyle LoadMultiplayerCharacter(string saveName)
        //{
        //    if (!string.IsNullOrEmpty(saveName))
        //    {
        //        //Debug.WriteLine(saveName);
        //        string characterData = sm.GetJsonData("mp_char_" + saveName);
        //        //Debug.Write(characterData.ToString() + "\n");
        //        if (!string.IsNullOrEmpty(characterData))
        //        {
        //            dynamic characterObj = JsonConvert.DeserializeObject(characterData);
        //            //Debug.Write(characterObj.ToString() + "\n");
        //            if (characterObj != null)
        //            {

        //                Debug.Write(characterObj.ToString() + "\n");

        //                bool IsMale = (bool)characterObj["IsMale"];

        //                int HairStyle = (int)characterObj["HairStyle"];
        //                int HairColor = (int)characterObj["HairColor"];
        //                int HairHighlightColor = (int)characterObj["HairHighlightColor"];

        //                int EyeColor = (int)characterObj["EyeColor"];

        //                float NoseWidth = (float)characterObj["NoseWidth"];
        //                float NosePeakHeight = (float)characterObj["NosePeakHeight"];
        //                float NosePeakLength = (float)characterObj["NosePeakLength"];
        //                float NosePeakLowering = (float)characterObj["NosePeakLowering"];
        //                float NoseBoneTwist = (float)characterObj["NoseBoneTwist"];

        //                float EyebrowHeight = (float)characterObj["EyebrowHeight"];
        //                float EyebrowForward = (float)characterObj["EyebrowForward"];

        //                float CheeksBoneHeight = (float)characterObj["CheeksBoneHeight"];
        //                float CheeksBoneWidth = (float)characterObj["CheeksBoneWidth"];
        //                float CheeksWidth = (float)characterObj["CheeksWidth"];

        //                float EyesOpening = (float)characterObj["EyesOpening"];

        //                float LipsThickness = (float)characterObj["LipsThickness"];

        //                float JawBoneWidth = (float)characterObj["JawBoneWidth"];
        //                float JawBoneBackLength = (float)characterObj["JawBoneBackLength"];

        //                float ChinBoneLength = (float)characterObj["ChinBoneLength"];
        //                float ChinBoneWidth = (float)characterObj["ChinBoneWidth"];
        //                float ChinBoneLowering = (float)characterObj["ChinBoneLowering"];
        //                float ChinHole = (float)characterObj["ChinHole"];

        //                float NeckThickness = (float)characterObj["NeckThickness"];

        //                int Mom = (int)characterObj["Mom"];
        //                int Dad = (int)characterObj["Dad"];

        //                float Resemblance = (float)characterObj["Resemblance"];
        //                float SkinTone = (float)characterObj["SkinTone"];

        //                return new PlayerAppearance.MpCharacterStyle(IsMale, true, HairStyle, HairColor, HairHighlightColor, EyeColor, NoseWidth, NosePeakHeight, NosePeakLength, NosePeakLowering, NoseBoneTwist, EyebrowHeight, EyebrowForward, CheeksBoneHeight, CheeksBoneWidth, CheeksWidth, EyesOpening, LipsThickness, JawBoneWidth, JawBoneBackLength, ChinBoneLowering, ChinBoneLength, ChinBoneWidth, ChinHole, NeckThickness, Mom, Dad, Resemblance, SkinTone);
        //            }
        //        }
        //    }
        //    // in case of an error, return an invalid character struct.
        //    Debug.WriteLine("error");
        //    return new PlayerAppearance.MpCharacterStyle(false, false);
        //}
        #endregion
        public async void SaveMpPed(bool isMale)
        {
            await Delay(0);
        }

        #endregion

        #region Set Correct Blip
        public void SetCorrectBlipSprite(int ped, int blip)
        {
            if (IsPedInAnyVehicle(ped, false))
            {
                int vehicle = GetVehiclePedIsIn(ped, false);
                int blipSprite = BlipInfo.GetBlipSpriteForVehicle(vehicle);
                if (GetBlipSprite(blip) != blipSprite)
                {
                    SetBlipSprite(blip, blipSprite);
                }

            }
            else
            {
                SetBlipSprite(blip, 1);
            }
        }
        #endregion

        public string GetSafePlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            return name.Replace("^", @"\^").Replace("~", @"\~").Replace("<", "«").Replace(">", "»");
        }
    }
}
