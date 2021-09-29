# WaveshareTouchscreenFix3

A system tray application for fixing older Waveshare Touchscreen models with malformed HID reports.

## Why?
I bought a Waveshare 10.1inch HDMI LCD (B) display, and it comes a capacitive digitizer that hooks up via USB. The digitizer has an STM32 chip between it and the micro USB port on the device. Suffice to say, upon plugging the touchscreen to a Windows 10 PC, it did not function as intended.
  
Later on I came across this MSDN thread (https://social.msdn.microsoft.com/Forums/en-US/e7f030e9-d22d-4972-9b2f-49bf4e090db9/waveshare-touch-problem?forum=WindowsIoT) and found out that the problem had to do with the touch controller's firmware (or rather, the STM32's firmware). I've contacted Waveshare about this exact issue, but it was clear that they weren't going to fix it for my case. And so, I decided to create this application to at least try and get the touch functionality working. The application simply reads the raw HID reports from WM_INPUT and injects(simulates) a second touch input over the touchscreen's touch input via Win32's InjectTouchInput function. 
 
In hindsight, I should've properly created a new driver or port an existing fix (https://github.com/derekhe/waveshare-7inch-touchscreen-driver, https://github.com/110yd/wshare-touchscreen), but as I did not need full functionality of the touchscreen, I decided to create a crude solution.

## Install
> The application uses .NET Framework 4.7.2 and depends on Win32 API calls, so only works on Windows.
1. Download and extract the latest release
2. Run the executable
3. Edit the configuration file created in the same folder (config.json)

## Configure
The config.json contains some values that must be first edited for the application to work correctly. The fields to edit are:
| Field         | Value                                                                                                           |
|---------------|-----------------------------------------------------------------------------------------------------------------|
| MapDisplay    | true: The display and the digitizer are seperate, false: The display and digitizer are the same                 |
| DisplaySize   | Resolution of the display. X for width, Y for height                                                            |
| DigitizerSize | Resolution of the digitizer. X for width, Y for height                                                          |
| DeviceName    | The name for the touchscreen's digitizer that shows up in  Settings > Devices > Mouse, keyboard & pen           |
| HoldMs        | Length of time for Windows to recognize a Hold event in milliseconds (Hold functionality currently not working) |

*If MapDisplay is set to false, DisplaySize and DigitizerSize will be ignored*

## Usage
Once ran, the application will reside in the system tray. From there, right clicking the icon will reveal a menu. 
1. Version: the current version number
2. Elevate to Admin: elevates the application to have administrator privileges, this is needed in the case that the touch inputs are directed to other applications/services with Admin rights. By default, the application starts with user privileges.
3. Reset configuration to default: Overwrites or creates a new config.json file with the required fields.
4. Exit: closes the application

## Restrictions
- Multitouch is not supported
- Press & Hold for Right click does not work
- Some applications (i.e. Chrome) process the original touch input correctly, causing them to recieve the equivalent of 2 touch inputs in consecutive order (one from the digitizer, one from InjectTouchInput)
