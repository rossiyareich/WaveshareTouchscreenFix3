#define RECIEVERAW
#undef RECIEVERAW

using SharpLib.Hid;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace WaveshareTouchscreenFix3
{
    public partial class Form1 : Form
    {
        private HID hid;
        private Handler handler;
        public static Configuration CurrentConfiguration;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (handler != null)
            {
                handler.Dispose();
                handler = null;
            }
            notifyIcon.Visible = false;
        }

        private void Form1_Load1(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
            notifyIcon.Icon = Icon.ExtractAssociatedIcon("TrayIcon.ico"); ;
            notifyIcon.Visible = true;
            MenuItem elevateItem = new MenuItem("Elevate to Admin", Elevate);
            if (IsAdministrator())
            {
                elevateItem.Enabled = false;
            }

            MenuItem versionItem = new MenuItem("Version: beta 0.1")
            {
                Enabled = false
            };
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] 
            {
                versionItem,
                elevateItem,
                new MenuItem("Reset configuration to default", ResetConfiguration),
                new MenuItem("Exit", Exit)
            });

            Configurator configurator = new Configurator();
            if(!configurator.LoadFile())
            {
                Exit(this, EventArgs.Empty);
                return;
            }
            hid = new HID(Handle, this);
            handler = hid.RegisterDevice();
            if(!handler.IsRegistered)
            {
                Console.WriteLine("Failed to register raw input device: " + Marshal.GetLastWin32Error().ToString());
            }
            handler.OnHidEvent += hid.Handler_OnHidEvent;
        }

        private void ResetConfiguration(object sender, EventArgs e)
        {
            Configurator.CreateEmptyConfiguration();
            MessageBox.Show($"Created/Overwritten the configuration file config.json with default/empty values. Application will close once you press OK or close the dialouge box.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Exit(this, EventArgs.Empty);
        }

        protected override void WndProc(ref Message message)
        {
            switch (message.Msg)
            {
                case 0x00FF:
                    #if !RECIEVERAW
                    //Log that message
                    Console.Write("WM_INPUT: " + message.ToString() + "\r\n");
                    #endif
                    //Returning zero means we processed that message.
                    message.Result = new IntPtr(0);
                    try
                    {
                        handler.ProcessInput(ref message);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message.ToString());
                    }
                    break;
            }
            //Is that needed? Check the docs.
            base.WndProc(ref message);
        }

        public void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void Elevate(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                // Restart program and run as admin
                string exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                try
                {
                    Process.Start(startInfo);
                }
                catch 
                {
                    return;
                }
                Exit(this, EventArgs.Empty);
            }
        }

        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
