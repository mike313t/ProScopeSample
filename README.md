# ProScopeSample
Sample project to re-create connection issues to ProScope HR5

* This solution must be opened with **Visual Studio 2015** that has appropriate tooling installed for UWP development.
* If you look at the **MainPage.xaml.cs**, an exception occurs when trying to obtain a **UsbDevice** in **Main_Loaded** function.
  * If you’d like to run the app to just view the video stream from the ProScope, comment out the line that sets **mydevice** variable in **Main_Loaded** function. 
* For UWP apps, in order to interact with USB device, it must be specified in the Capabilities of the application.  These capabilities are managed in **Package.appxmanifest** file.  For usb, the file must be edited manually (not through UI provided).  To edit, right click the file and select **View Code**.
* Following UWP documentation has a step by step overview of communicating with a USB device:  https://docs.microsoft.com/en-us/windows-hardware/drivers/usbcon/talking-to-usb-devices-start-to-finish
