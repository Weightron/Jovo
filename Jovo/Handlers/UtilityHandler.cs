﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Jovo
{
    public class UtilityHandler
    {
        static Process process = Process.GetCurrentProcess();
        string FullPath;
        public bool IsDevUser { get; set; }

        public UtilityHandler() => FullPath = process.MainModule.FileName;

        public void ArchiveLog()
        {
            try
            {
                if (File.Exists("log.txt"))
                {
                    DateTime now = DateTime.Now;
                    DateTime FileCreated = File.GetCreationTime("log.txt");

                    if (now > FileCreated.AddDays(7))
                    {
                        LogEvent("Log file is being archived", true, true);
                        if (!Directory.Exists("loghistory"))
                            Directory.CreateDirectory("loghistory");


                        if (!File.Exists("loghistory\\log " + FileCreated.Date.ToString("yyyy-MM-dd") + ".txt"))
                        {
                            File.Move("log.txt", "loghistory\\log " + FileCreated.Date.ToString("yyyy-MM-dd") + ".txt");
                        }
                        else
                        {
                            File.AppendAllText("loghistory\\log " + FileCreated.Date.ToString("yyyy-MM-dd") + ".txt", File.ReadAllText("log.txt"));
                        }


                        File.Create("log.txt").Close();
                        File.SetCreationTime("log.txt", DateTime.Now);
                    }
                }
            }
            catch (Exception ArchiveEx)
            {
                LogEvent(ArchiveEx.ToString());
            }
            finally
            {
                PurgeLogHistory();
            }
        }

        public void PurgeLogHistory()
        {
            try
            {
                if (Directory.Exists("loghistory"))
                {
                    foreach (string file in Directory.GetFiles("loghistory"))
                    {
                        FileInfo log = new FileInfo(file);
                        if (log.CreationTime < DateTime.Now.AddMonths(-1))
                        {
                            log.Delete();
                        }
                    }
                }
            }
            catch (Exception PurgeLogsEx)
            {
                LogEvent(PurgeLogsEx.ToString());
            }
        }

        public void LogEvent(string message, bool newLine = true, bool blankLine = false)
        {
            try
            {
                using (StreamWriter LogWriter = new StreamWriter(process.StartInfo.WorkingDirectory + "log.txt", true))
                {
                    if (LogWriter.BaseStream.Position == 0)

                        if (LogWriter.BaseStream.Position != 0 && blankLine)
                        {
                            LogWriter.WriteLine("");
                        }

                    if (!String.IsNullOrEmpty(message))
                    {
                        if (newLine)
                            LogWriter.Write(Environment.NewLine + DateTime.Now.ToString() + " - " + message);
                        else
                            LogWriter.Write(message);

                        if (blankLine)
                            LogWriter.Write(Environment.NewLine);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public bool TestConnection(string ipAddress)
        {
            IPAddress ip = null;

            try
            {
                if (ipAddress.Contains("\\"))
                    ip = Dns.GetHostEntry(ipAddress.Replace("\\", String.Empty)).AddressList[0];
                else if (ipAddress.Contains("http://"))
                    ip = Dns.GetHostAddresses(new Uri(ipAddress).Host)[0];
                else
                    ip = IPAddress.Parse(ipAddress);

                Ping p = new Ping();
                PingReply reply = p.Send(ip);

                if (reply.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Keys GetModuleKeyboardShortcut(string KeyboardShortcut, KeyboardHook hook)
        {

            ModifierKeys modifiers = new ModifierKeys();
            Keys shortcut = new Keys();
            Keys returnKeys = new Keys();

            try
            {
                foreach (string key in KeyboardShortcut.Split('+'))
                {
                    Keys ckey = (Keys)Enum.Parse(typeof(Keys), key, true);
                    switch (ckey)
                    {
                        case Keys.Alt:
                            modifiers = modifiers | ModifierKeys.Alt;
                            break;

                        case Keys.Control:
                            modifiers = modifiers | ModifierKeys.Control;
                            break;

                        case Keys.Shift:
                            modifiers = modifiers | ModifierKeys.Shift;
                            break;
                        default:
                            shortcut = shortcut | ckey;
                            break;
                    }

                    returnKeys = returnKeys | ckey;
                }

                hook.RegisterHotKey(modifiers, shortcut);
            }
            catch (Exception) { }

            return returnKeys;
        }

        public bool CheckKeyboardShortcut(ModuleData data, ModifierKeys modifier, Keys key)
        {
            if (data.KeyboardShortcut == modifier.ToString().Replace(", ", "+") + "+" + key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }

            // Possibly Deprecated
            /*
            string pressed = modifier.ToString().Replace(", ", "+") + "+" + key.ToString();
            int keysPressed = pressed.Split('+').Length;
            int shortcutKeys = data.KeyboardShortcut.Split('+').Length;
            int matchingKeys = 0;

            foreach (string k in data.KeyboardShortcut.Split('+'))
            {
                foreach (string p in pressed.Split('+'))
                {
                    if (pressed.Contains(k) && p.Length == k.Length)
                        matchingKeys++;
                }
            }

            if ((matchingKeys == shortcutKeys) && (matchingKeys == keysPressed) && (keysPressed == shortcutKeys))
                return true;
            else
                return false;
                */
        }

        public Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

    }
}
