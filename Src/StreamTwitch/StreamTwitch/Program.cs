﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace StreamTwitch
{
    class Program
    {
        static void OnKeyDown(Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                const string message = "• Author: Michaël André Franiatte.\n\r\n\r• Contact: michael.franiatte@gmail.com.\n\r\n\r• Publisher: https://github.com/michaelandrefraniatte.\n\r\n\r• Copyrights: All rights reserved, no permissions granted.\n\r\n\r• License: Not open source, not free of charge to use.";
                const string caption = "About";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        [DllImport("advapi32.dll")]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] Int32 nCmdShow);
        const Int32 SW_MINIMIZE = 6;
        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        private static string audiodevice, streamurl, BITRATE, BITRATEx2, cpuorgpu;
        private static bool Getstate;
        private static Process process;
        private static ProcessStartInfo startInfo;
        public static ThreadStart threadstart;
        public static Thread thread;
        public static uint CurrentResolution = 0;
        public static bool echoboostenable = false;
        public static int[] wd = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static int[] wu = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        public static bool[] ws = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        static void valchanged(int n, bool val)
        {
            if (val)
            {
                if (wd[n] <= 1)
                {
                    wd[n] = wd[n] + 1;
                }
                wu[n] = 0;
            }
            else
            {
                if (wu[n] <= 1)
                {
                    wu[n] = wu[n] + 1;
                }
                wd[n] = 0;
            }
            ws[n] = val;
        }
        static void Main()
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            MinimizeConsoleWindow();
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            if (!AlreadyRunning())
            {
                using (System.IO.StreamReader createdfile = new System.IO.StreamReader("params.txt"))
                {
                    createdfile.ReadLine();
                    cpuorgpu = createdfile.ReadLine();
                    createdfile.ReadLine();
                    audiodevice = createdfile.ReadLine();
                    createdfile.ReadLine();
                    streamurl = createdfile.ReadLine();
                    createdfile.ReadLine();
                    BITRATE = createdfile.ReadLine();
                    createdfile.ReadLine();
                    BITRATEx2 = createdfile.ReadLine();
                    createdfile.ReadLine();
                    echoboostenable = bool.Parse(createdfile.ReadLine());
                }
                if (echoboostenable)
                    Process.Start("EchoBoost.exe");
                Task.Run(() => Start());
                Console.ReadLine();
            }
        }
        public static bool AlreadyRunning()
        {
            Process[] processes = Process.GetProcessesByName("GameRecorder");
            if (processes.Length > 1)
                return true;
            else
                return false;
        }
        public static void Start()
        {
            for (; ; )
            {
                valchanged(0, GetAsyncKeyState(Keys.NumPad0));
                valchanged(1, GetAsyncKeyState(Keys.Decimal));
                if (wd[1] == 1 & !Getstate)
                {
                    Getstate = true;
                    startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.FileName = "ffmpeg.exe";
                    if (cpuorgpu == "CPU")
                        startInfo.Arguments = @"-f gdigrab -thread_queue_size 5096 -i desktop -f dshow -thread_queue_size 5096 -i audio=" + audiodevice + " -c:v libx264 -profile:v high -bf 2 -g 30 -crf 18 -pix_fmt yuv420p -c:a aac -profile:a aac_low -b:a 384k -f flv " + streamurl;
                    if (cpuorgpu == "GPU")
                        startInfo.Arguments = @"-f gdigrab -i desktop -f dshow -i audio=" + audiodevice + " -c:v h264_nvenc -preset p7 -r 15 -b:v " + BITRATE + " -c:a aac -b:a 256k -bufsize " + BITRATEx2 + " -f flv " + streamurl;
                    try
                    {
                        process = Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    if (wd[0] == 1 & Getstate)
                    {
                        Getstate = false;
                        try
                        {
                            process.StandardInput.WriteLine('q');
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                Thread.Sleep(70);
            }
        }
        private static void MinimizeConsoleWindow()
        {
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, SW_MINIMIZE);
        }
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                threadstart = new ThreadStart(FormClose);
                thread = new Thread(threadstart);
                thread.Start();
                Thread.Sleep(2000);
            }
            return false;
        }
        private static void FormClose()
        {
            if (Getstate)
            {
                try
                {
                    process.StandardInput.WriteLine('q');
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            if (echoboostenable)
            {
                var proc = Process.GetProcessesByName("EchoBoost");
                if (proc.Length > 0)
                    proc[0].Kill();
            }
            TimeEndPeriod(1);
        }
    }
}