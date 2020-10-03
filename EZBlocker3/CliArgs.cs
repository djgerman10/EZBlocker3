﻿using EZBlocker3.Logging;

namespace EZBlocker3 {
    public record CliArgs(bool ForceDebugMode = false, bool IsUpdateRestart = false, bool IsRedirectedSpotifyStart = false, bool IsAutomaticStart = false) {

        public const string ForceDebugOption = "/debug";
        public const string UpdateRestartOption = "/updateRestart";
        public const string RedirectedSpotifyStartOption = "/redirectStart";
        public const string AutomaticStartOption = "/autostart";

        public static CliArgs Parse(string[] args) {
            var forceDebugMode = false;
            var isUpdateRestart = false;
            var isRedirectedSpotifyStart = false;
            var isAutomaticStart = false;

            foreach (var arg in args) {
                switch (arg) {
                    case ForceDebugOption:
                        forceDebugMode = true;
                        break;
                    case UpdateRestartOption:
                        isUpdateRestart = true;
                        break;
                    case RedirectedSpotifyStartOption:
                        isRedirectedSpotifyStart = true;
                        break;
                    case AutomaticStartOption:
                        isAutomaticStart = true;
                        break;
                    default:
                        Logger.LogInfo("Unrecognised cli arg:" + arg);
                        break;
                }
            }

            return new CliArgs() {
                ForceDebugMode = forceDebugMode,
                IsUpdateRestart = isUpdateRestart,
                IsRedirectedSpotifyStart = isRedirectedSpotifyStart,
                IsAutomaticStart = isAutomaticStart
            };
        }


    }
}