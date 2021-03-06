﻿using EZBlocker3.Logging;
using EZBlocker3.Utils;
using Lazy;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace EZBlocker3.Settings {
    public static class StartWithSpotify {

        [Lazy]
        private static string SpotifyPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\Spotify.exe";
        [Lazy]
        private static string RealSpotifyPath => Path.ChangeExtension(SpotifyPath, "real.exe");
        [Lazy]
        private static string ProxyTempPath => Path.ChangeExtension(SpotifyPath, "proxy.exe");

        public static void SetEnabled(bool enabled) {
            if (IsProxyInstalled()) {
                if (!enabled)
                    Disable();
            } else {
                if (enabled)
                    Enable();
            }
        }
        public static void Enable() => InstallProxy();
        public static void Disable() => UninstallProxy();

        public static bool IsProxyInstalled() => File.Exists(RealSpotifyPath);
        public static void InstallProxy() {
            var tempIconFilePath = Path.ChangeExtension(SpotifyPath, ".ico.temp");
            try {
                using var spotifyIcon = Icon.ExtractAssociatedIcon(SpotifyPath);
                using var spotifyIconBitmap = spotifyIcon.ToBitmap();
                BitmapUtils.SaveAsIcon(spotifyIconBitmap, tempIconFilePath);

                File.Delete(ProxyTempPath);
                if (GenerateProxy(ProxyTempPath, tempIconFilePath)) {
                    Logger.LogInfo("Settings: Successfully generated proxy executable");
                    File.Move(SpotifyPath, RealSpotifyPath);
                    File.Move(ProxyTempPath, SpotifyPath);
                } else {
                    Logger.LogError("Settings: Failed to generate proxy executable");
                }
            } catch {
                File.Delete(ProxyTempPath);
                throw;
            } finally {
                File.Delete(tempIconFilePath);
            }
        }
        public static void UninstallProxy() {
            if (File.Exists(RealSpotifyPath)) {
                File.Delete(SpotifyPath);
                File.Move(RealSpotifyPath, SpotifyPath);
            } else {
                File.Delete(ProxyTempPath);
            }
        }
        public static bool GenerateProxy(string executablePath, string iconPath) {
            var parameters = new CompilerParameters {
                GenerateExecutable = true,
                OutputAssembly = executablePath,
                GenerateInMemory = false
            };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.CompilerOptions = $"/target:winexe \"/win32icon:{iconPath}\"";

            var provider = CodeDomProvider.CreateProvider("CSharp");
            var code = GetProxyCode(App.Location, CliArgs.ProxyStartOption);
            var result = provider.CompileAssemblyFromSource(parameters, code);

            foreach (CompilerError error in result.Errors)
                Logger.LogWarning($"Settings: Redirection executable generation {(error.IsWarning ? "warning" : "error")}:\n{error.ErrorText}");

            return result.Errors.Count == 0;
        }

        public static void HandleProxiedStart() {
            Logger.LogInfo("Started through proxy executable");

            if (!File.Exists(RealSpotifyPath)) {
                Logger.LogWarning("Started through proxy executable when no proxy is present");
                return;
            } 

            File.Delete(ProxyTempPath);
            File.Move(SpotifyPath, ProxyTempPath);
            File.Move(RealSpotifyPath, SpotifyPath);

            Process.Start(SpotifyPath).Dispose();
        }
        public static void HandleProxiedExit() {
            Logger.LogInfo("Reset proxy executable");

            if (!File.Exists(ProxyTempPath)) {
                Logger.LogWarning("Failed to reset proxy as no proxy is present");
                return;
            }

            File.Move(SpotifyPath, RealSpotifyPath);
            File.Move(ProxyTempPath, SpotifyPath);
        }

        private static string GetProxyCode(string appPath, string appArgs) => @"
using System.Diagnostics;

public static class Proxy {
    public static void Main() {
        var appPath = @""" + appPath + @""";
        var appArgs = @""" + appArgs + @""";

        Process.Start(appPath, appArgs);
    }
}";
    }
}

