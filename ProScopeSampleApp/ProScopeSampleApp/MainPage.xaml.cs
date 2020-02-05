using ProScopeSampleApp.DataObjects;
using ProScopeSampleApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ProScopeSampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        #region NotifyPropertyChanged


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
            }
        }

        #endregion

        private CaptureElement _captureElement;
        public CaptureElement CaptureElement
        {
            get
            {
                if (_captureElement == null) _captureElement = new CaptureElement();
                return _captureElement;
            }
            set
            {
                _captureElement = value;
                NotifyPropertyChanged();
            }
        }

        private LookupItemDTO _cameraDevice;
        public LookupItemDTO CameraDevice
        {
            get
            {
                return _cameraDevice;
            }
            set
            {
                _cameraDevice = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<LookupItemDTO> AvailableCameras { get; } = new ObservableCollection<LookupItemDTO>();


        private async Task ConfigureCamera()
        {
            CameraDevice = PhotoHelper.GetNextCamera(AvailableCameras, CameraDevice?.ValueLong, false);
            await PhotoHelper.ConfigureMedia(CaptureElement, CameraDevice, false);
        }

        private async void Main_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                AvailableCameras.Clear();
                var cameras = await PhotoHelper.GetAvailableCameras(true);
                foreach(var c in cameras)
                {
                    AvailableCameras.Add(c);
                }

                await ConfigureCamera();

                ushort vendorId = 0x19AB;
                ushort productId = 0x2000;
                //ushort vendorId = 0x203A;
                //ushort productId = 0xFFF9;
                // var aqs = UsbDevice.GetDeviceSelector(vendorId, productId, Guid.Parse("{ca3e7ab9-b4c3-4ae6-8251-579ef933890f}"));
                //var selector = "System.Devices.InterfaceClassGuid:=\"" + "{ca3e7ab9-b4c3-4ae6-8251-579ef933890f}"+ "\"";
                //                 + " AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
                // var myDevices = await DeviceInformation.FindAllAsync(aqs,null);

                //UsbDevice device = await UsbDevice.FromIdAsync(myDevices[0].Id);
                //var dev = interfaces.Where(x => x.Name.Contains("scope")).FirstOrDefault();

                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                var device = devices[1];


                //var mydevice = await UsbDevice.FromIdAsync(device);
                var mydevice = await UsbDevice.FromIdAsync(device.Id);




                ////If this is a scope screen, enable the button click event from the scope

                //    //Bodelin Scope - HID
                //    ushort vendorId = 0x19AB;
                //    var productIdList = new List<ushort> { 0x1000, 0x1020, 0x2000 };

                //    ushort usageId = 1;
                //    ushort usagePage = 65440;
                //    foreach (ushort productId in productIdList)
                //    {
                //        string selector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);
                //        var myDevices = await DeviceInformation.FindAllAsync(selector);

                //        if (myDevices.Any())
                //        {
                //            _proscopeHidDevice = await HidDevice.FromIdAsync(myDevices[0].Id, FileAccessMode.Read);
                //            var status = DeviceAccessInformation.CreateFromId(myDevices[0].Id).CurrentStatus;

                //            if (_proscopeHidDevice != null)
                //            {
                //                _proscopeHidDevice.InputReportReceived += ProScopeClicked;
                //                break;
                //            }
                //        }
                //    }

            }
            catch
            {
                throw;
            }
        }

    }
}
