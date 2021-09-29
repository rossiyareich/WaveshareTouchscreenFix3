using System;
using System.Runtime.InteropServices;

namespace WaveshareTouchscreenFix3
{
    public class TouchInject
    {
        public POINTER_TOUCH_INFO contact;
        private readonly uint maxTouch;
        public TouchInject(uint maxTouch)
        {
            this.maxTouch = maxTouch;
            contact = new POINTER_TOUCH_INFO();
            contact.pointerInfo.pointerType = POINTER_INPUT_TYPE.PT_TOUCH;
            contact.touchFlags = TOUCH_FLAGS.TOUCH_FLAG_NONE;
            contact.orientation = 90;
            contact.touchMask = TOUCH_MASK.TOUCH_MASK_CONTACTAREA | TOUCH_MASK.TOUCH_MASK_ORIENTATION | TOUCH_MASK.TOUCH_MASK_PRESSURE;
            contact.pointerInfo.pointerId = 1;
        }

        public bool TouchDown(int x, int y)
        {
            InitializeTouchInjection(maxTouch, TOUCH_FEEDBACK_DEFAULT);

            contact.pressure = 32000;
            contact.pointerInfo.pointerFlags = POINTER_FLAGS.POINTER_FLAG_DOWN | POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT;
            contact.pointerInfo.ptPixelLocation.x = x;
            contact.pointerInfo.ptPixelLocation.y = y;

            RECT touchArea = new RECT
            {
                left = x - 2,
                right = x + 2,
                top = y - 2,
                bottom = y + 2
            };
            contact.rcContact = touchArea;

            return InjectTouchInput(1, ref contact);
        }

        public bool TouchDrag(int x, int y)
        {
            contact.pressure = 32000;
            contact.pointerInfo.pointerFlags = POINTER_FLAGS.POINTER_FLAG_UPDATE | POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT; //Setting the Pointer Flag to Drag
            contact.pointerInfo.ptPixelLocation.x = x;
            contact.pointerInfo.ptPixelLocation.y = y;

            return InjectTouchInput(1, ref contact);
        }

        public bool TouchUp()
        {
            contact.pressure = 0;
            contact.pointerInfo.pointerFlags = POINTER_FLAGS.POINTER_FLAG_UP;

            return InjectTouchInput(1, ref contact);
        }

        //public void TouchHold()
        //{
        //    contact.pressure = 32000;
        //    contact.pointerInfo.pointerFlags = POINTER_FLAGS.POINTER_FLAG_UPDATE | POINTER_FLAGS.POINTER_FLAG_INRANGE | POINTER_FLAGS.POINTER_FLAG_INCONTACT;
        //    for (int i = 0; i < Form1.CurrentConfiguration.HoldMs*100; i++)
        //    {
        //        InjectTouchInput(1, ref contact);
        //    }
        //}

        #region P/Invoke
        private const int MAX_TOUCH_COUNT = 256;
        private const int TOUCH_FEEDBACK_DEFAULT = 0x1;     //Specifies default touch visualizations.
        private const int TOUCH_FEEDBACK_INDIRECT = 0x2;    //Specifies indirect touch visualizations.
        private const int TOUCH_FEEDBACK_NONE = 0x3;        //Specifies no touch visualizations.

        [DllImport("User32.dll")]
        private static extern bool InitializeTouchInjection(uint maxCount, uint dwMode);

        [DllImport("User32.dll")]
        private static extern bool InjectTouchInput(uint count, ref POINTER_TOUCH_INFO contacts);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_TOUCH_INFO
        {
            public POINTER_INFO pointerInfo; //Contains basic pointer information common to all pointer types.
            public TOUCH_FLAGS touchFlags; //Lists the touch flags.
            public TOUCH_MASK touchMask;
            /*
             * Pointer contact area in pixel screen coordinates. 
             * By default, if the device does not report a contact area, 
             * this field defaults to a 0-by-0 rectangle centered around the pointer location.
             */
            public RECT rcContact;
            public RECT rcContactRaw;
            /*
             * A pointer orientation, with a value between 0 and 359, where 0 indicates a touch pointer 
             * aligned with the x-axis and pointing from left to right; increasing values indicate degrees
             * of rotation in the clockwise direction.
             * This field defaults to 0 if the device does not report orientation.
             */
            public uint orientation;
            public uint pressure; //Pointer pressure normalized in a range of 0 to 256.
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RECT
        {
            [FieldOffset(0)]
            public int left;
            [FieldOffset(4)]
            public int top;
            [FieldOffset(8)]
            public int right;
            [FieldOffset(12)]
            public int bottom;
        }
        public enum TOUCH_FLAGS
        {
            TOUCH_FLAG_NONE = 0x00000000 //Indicates that no flags are set. 
        }
        public enum TOUCH_MASK
        {
            TOUCH_MASK_NONE = 0x00000000, // Default - none of the optional fields are valid
            TOUCH_MASK_CONTACTAREA = 0x00000001, // The rcContact field is valid
            TOUCH_MASK_ORIENTATION = 0x00000002, // The orientation field is valid
            TOUCH_MASK_PRESSURE = 0x00000004 // The pressure field is valid
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_INFO
        {
            public POINTER_INPUT_TYPE pointerType;
            public uint pointerId;
            public uint frameId;
            public POINTER_FLAGS pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public POINT ptPixelLocation;
            public POINT ptHimetricLocation;
            public POINT ptPixelLocationRaw;
            public POINT ptHimetricLocationRaw;
            public uint dwTime;
            public uint historyCount;
            public int InputData;
            public uint dwKeyStates;
            public ulong PerformanceCount;
            public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        public enum POINTER_FLAGS
        {
            POINTER_FLAG_NONE =              0x00000000, // Default
            POINTER_FLAG_NEW =               0x00000001, // New pointer
            POINTER_FLAG_INRANGE =           0x00000002, // Pointer has not departed
            POINTER_FLAG_INCONTACT =         0x00000004, // Pointer is in contact
            POINTER_FLAG_FIRSTBUTTON =       0x00000010, // Primary action
            POINTER_FLAG_SECONDBUTTON =      0x00000020, // Secondary action
            POINTER_FLAG_THIRDBUTTON =       0x00000040, // Third button
            POINTER_FLAG_FOURTHBUTTON =      0x00000080, // Fourth button
            POINTER_FLAG_FIFTHBUTTON =       0x00000100, // Fifth button
            POINTER_FLAG_PRIMARY =           0x00002000, // Pointer is primary
            POINTER_FLAG_CONFIDENCE =        0x00004000, // Pointer is considered unlikely to be accidental
            POINTER_FLAG_CANCELED =          0x00008000, // Pointer is departing in an abnormal manner
            POINTER_FLAG_DOWN =              0x00010000, // Pointer transitioned to down state (made contact)
            POINTER_FLAG_UPDATE =            0x00020000, // Pointer update
            POINTER_FLAG_UP =                0x00040000, // Pointer transitioned from down state (broke contact)
            POINTER_FLAG_WHEEL =             0x00080000, // Vertical wheel
            POINTER_FLAG_HWHEEL =            0x00100000, // Horizontal wheel
            POINTER_FLAG_CAPTURECHANGED =    0x00200000, // Lost capture
            POINTER_FLAG_HASTRANSFORM =      0x00400000, // Input has a transform associated with it
        }

        public enum POINTER_INPUT_TYPE
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
            PT_TOUCHPAD = 0x00000005
        };

        public enum POINTER_BUTTON_CHANGE_TYPE
        {
            POINTER_CHANGE_NONE,
            POINTER_CHANGE_FIRSTBUTTON_DOWN,
            POINTER_CHANGE_FIRSTBUTTON_UP,
            POINTER_CHANGE_SECONDBUTTON_DOWN,
            POINTER_CHANGE_SECONDBUTTON_UP,
            POINTER_CHANGE_THIRDBUTTON_DOWN,
            POINTER_CHANGE_THIRDBUTTON_UP,
            POINTER_CHANGE_FOURTHBUTTON_DOWN,
            POINTER_CHANGE_FOURTHBUTTON_UP,
            POINTER_CHANGE_FIFTHBUTTON_DOWN,
            POINTER_CHANGE_FIFTHBUTTON_UP
        }

        #endregion
    }
}
