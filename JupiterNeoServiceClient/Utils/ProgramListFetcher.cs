using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class ProgramListFetcher
{
    private readonly List<string> installs = new();
    private readonly string[] officeAppNames = { "Access", "Excel", "OneNote", "Outlook", "PowerPoint", "Publisher", "Word" };

    private readonly List<string> keys = new()
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    private void FindInstalls(RegistryKey regKey, List<string> keys, List<string> installed)
    {
        foreach (string key in keys)
        {
            using RegistryKey? rk = regKey.OpenSubKey(key);
            if (rk == null) continue;

            foreach (string skName in rk.GetSubKeyNames())
            {
                using RegistryKey? sk = rk.OpenSubKey(skName);
                if (sk != null)
                {
                    try
                    {
                        string? displayName = sk.GetValue("DisplayName")?.ToString();
                        if (!string.IsNullOrEmpty(displayName))
                        {
                            installed.Add(displayName);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignorar errores en la lectura del registro
                    }
                }
            }
        }
    }

    private string? GetProgramFilesPath()
    {
        using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        return key?.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion")?.GetValue("ProgramFilesDir")?.ToString();
    }

    private string? GetProgramFilesX86Path()
    {
        using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        return key?.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion")?.GetValue("ProgramFilesDir (x86)")?.ToString();
    }

    private string? FindOfficeRootPath(string? programFilesPath, string? programFilesX86Path)
    {
        if (programFilesPath != null)
        {
            string officeRootPath = Path.Combine(programFilesPath, "Microsoft Office", "root", "Office16");
            if (Directory.Exists(officeRootPath))
            {
                return officeRootPath;
            }
        }

        if (programFilesX86Path != null)
        {
            string officeRootPath = Path.Combine(programFilesX86Path, "Microsoft Office", "root", "Office16");
            if (Directory.Exists(officeRootPath))
            {
                return officeRootPath;
            }
        }

        return null; // No se encontró la instalación de Office
    }

    public List<string> GetInstalledProgramsList()
    {
        FindInstalls(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64), keys, installs);
        FindInstalls(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64), keys, installs);

        string? programFilesPath = GetProgramFilesPath();
        string? programFilesX86Path = GetProgramFilesX86Path();

        string? officeRootPath = FindOfficeRootPath(programFilesPath, programFilesX86Path);
        if (!string.IsNullOrEmpty(officeRootPath))
        {
            foreach (string appName in officeAppNames)
            {
                if (IsOfficeAppInstalled(appName, officeRootPath))
                {
                    installs.Add($"Microsoft {appName}");
                }
            }
        }

        AddAppxPackagesToInstalls();

        installs.RemoveAll(string.IsNullOrEmpty);
        return installs;
    }

    private void AddAppxPackagesToInstalls()
    {
        const string script = "Get-AppxPackage | where {$_.IsFramework -eq $false} | select -ExpandProperty Name";
        using Process powerShellProcess = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            powerShellProcess.Start();
            while (!powerShellProcess.StandardOutput.EndOfStream)
            {
                string? line = powerShellProcess.StandardOutput.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    installs.Add(line.Trim());
                }
            }
        }
        catch (Exception)
        {
            // Ignorar errores al ejecutar PowerShell
        }
    }

    private bool IsOfficeAppInstalled(string appName, string officeRootPath)
    {
        foreach (string appExe in GetOfficeAppExecutableNames(appName))
        {
            string appExePath = Path.Combine(officeRootPath, appExe);
            if (File.Exists(appExePath))
            {
                return true;
            }
        }
        return false;
    }

    private string[] GetOfficeAppExecutableNames(string appName) =>
        appName switch
        {
            "Access" => new[] { "MSACCESS.EXE" },
            "Excel" => new[] { "EXCEL.EXE", "XCEL.EXE" },
            "Outlook" => new[] { "OUTLOOK.EXE" },
            "PowerPoint" => new[] { "POWERPNT.EXE" },
            "Publisher" => new[] { "MSPUB.EXE" },
            "Word" => new[] { "WINWORD.EXE", "WINWRD.EXE" },
            _ => Array.Empty<string>()
        };
}

