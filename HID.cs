#define RECIEVERAW
#undef RECIEVERAW

using SharpLib.Hid;
using SharpLib.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace WaveshareTouchscreenFix3
{
    public class HID
    {
        private readonly IntPtr Handle;
        private readonly Control Control;
        private TouchInject touchInject;
        public Point LastPoint
        {
            get;
            private set;
        }

        private int count1 = 0;
        private uint lastHeader;
        private bool moved = false;
        private bool held = false;
        public delegate void OnHidEventDelegate(object aSender, Event aHidEvent);
        public delegate void TouchReleaseHandler(object sender, Point point);

        private event TouchReleaseHandler OnTouchRelease;

        private Timer holdTimer;

        public HID(IntPtr handle, Control control)
        {
            Handle = handle;
            Control = control;
            OnTouchRelease += HID_OnTouchRelease;
        }

        public Handler RegisterDevice()
        {
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[]
            {
                ChooseDevice()
            };
            Handler handler = new Handler(devices);
            return handler;
        }

        private RAWINPUTDEVICE ChooseDevice()
        {
            RAWINPUTDEVICE device = new RAWINPUTDEVICE()
            {
                usUsage = 0x0004,
                usUsagePage = (ushort)UsagePage.Digitiser,
                dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK,
                hwndTarget = Handle
            };
            return device;
        }
        public void Handler_OnHidEvent(object aSender, Event aHidEvent)
        {
            if (aHidEvent.IsStray)
            {
                //Stray event just ignore it
                return;
            }

            if (Control.InvokeRequired)
            {
                //Not in the proper thread, invoke ourselves.
                //Repeat events usually come from another thread.
                OnHidEventDelegate d = new OnHidEventDelegate(Handler_OnHidEvent);
                Control.Invoke(d, new object[] { aSender, aHidEvent });
            }
            else
            {
                Dictionary<HIDP_VALUE_CAPS, uint>.ValueCollection values = aHidEvent.UsageValues.Values;
                if (aHidEvent.Device.FriendlyName == $"{Form1.CurrentConfiguration.DeviceName} ( Digitiser, 0x0004 )")
                {
                    #if !RECIEVERAW
                    switch (values.First())
                    {
                        case 1:
                            if(Form1.CurrentConfiguration.MapDisplay)
                            {
                                int xUnscaled = (int)values.ElementAt(1);
                                int yUnscaled = (int)values.ElementAt(2);
                                LastPoint = new Point((int)map(xUnscaled, 0, Form1.CurrentConfiguration.DigitizerSize.X, 0, Form1.CurrentConfiguration.DisplaySize.X), (int)map(yUnscaled, 0, Form1.CurrentConfiguration.DigitizerSize.Y, 0, Form1.CurrentConfiguration.DisplaySize.Y));
                            }
                            else
                                LastPoint = new Point((int)values.ElementAt(1), (int)values.ElementAt(2));
                            Console.WriteLine(LastPoint.ToString());
                            if (count1 < 1)
                            {
                                count1++;
                                touchInject = new TouchInject(2);
                                touchInject.TouchDown(LastPoint.X, LastPoint.Y);
                                moved = false;
                                CancelTimer();
                                holdTimer = new Timer
                                {
                                    Interval = Form1.CurrentConfiguration.HoldMs
                                };
                                holdTimer.Tick += HoldTimer_Tick;
                                holdTimer.Start();
                            }
                            else if (count1 == 1)
                            {
                                if(touchInject.contact.pointerInfo.ptPixelLocation.x != LastPoint.X || touchInject.contact.pointerInfo.ptPixelLocation.y != LastPoint.Y)
                                {
                                    CancelTimer();
                                    touchInject.TouchDrag(LastPoint.X, LastPoint.Y);
                                    moved = true;
                                }
                            }
                            break;
                        case 9:
                            if(lastHeader == 9)
                            {
                                OnTouchRelease?.Invoke(this, LastPoint);
                            }

                            break;
                    }
                    lastHeader = values.First();
                    #endif
                }
                #if RECIEVERAW
                foreach (var value in values)
                {
                    Console.Write(value + ", ");
                }
                Console.Write("\n");
                #endif
            }
        }

        private long map(long x, long in_min, long in_max, long out_min, long out_max)
            {
                return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
            }

        private void HID_OnTouchRelease(object sender, Point point)
        {
            Console.WriteLine($@"Released at {{{point.X}, {point.Y}}}");
            count1 = 0;
            touchInject?.TouchUp();
            if(!moved)
            {
                touchInject.TouchDown(LastPoint.X, LastPoint.Y);
                touchInject.TouchUp();
                if (held)
                {
                    CancelTimer();
                    //touchInject.TouchHold();
                    //touchInject.TouchUp();
                }
            }
            touchInject = null;
        }

        private void HoldTimer_Tick(object sender, EventArgs e)
        {
            CancelTimer();
            held = true;
        }

        private void CancelTimer()
        {
            held = false;
            holdTimer?.Stop();
            holdTimer?.Dispose();
            holdTimer = null;
        }
    }
}
