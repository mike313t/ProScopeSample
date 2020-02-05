using ProScopeSampleApp.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ProScopeSampleApp.Helpers
{
    public class PhotoHelper
    {
        public PhotoHelper()
        {

        }
               

        public static async Task<byte[]> ConvertToByteArray(IRandomAccessStream stream)
        {
            var dr = new DataReader(stream.GetInputStreamAt(0));
            var bytes = new byte[stream.Size];
            await dr.LoadAsync((uint)stream.Size);
            dr.ReadBytes(bytes);
            return bytes;
        }

        public static SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));

            return myBrush;
        }

        public static async Task<IRandomAccessStream> RenderToRandomAccessStream(UIElement element)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            // Useful for rendering in the correct DPI
            var displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                 BitmapAlphaMode.Premultiplied,
                                 (uint)rtb.PixelWidth,
                                 (uint)rtb.PixelHeight,
                                 displayInformation.RawDpiX,
                                 displayInformation.RawDpiY,
                                 pixels);

            await encoder.FlushAsync();
            stream.Seek(0);

            return stream;
        }

        public static async Task<IRandomAccessStream> RenderToRandomAccessStream(byte[] arr)
        {
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            await randomAccessStream.WriteAsync(arr.AsBuffer());
            randomAccessStream.Seek(0); // Just to be sure.
                                        // I don't think you need to flush here, but if it doesn't work, give it a try.
            return randomAccessStream;
        }

        public static async Task<IEnumerable<LookupItemDTO>> GetAvailableCameras(bool isScopeCapture)
        {
            //sort the cameras in order that we want to cycle through them: scope (if in context of scope), default local setting (last selected camera), back camera, front camera, all cameras but scope scope (if not in context of scope), scope
            var availableCameras = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).Select(x => new LookupItemDTO()
            {
                ValueLong = x.Id               
            }).OrderBy(x => x.SortOrder).ToList();

            return availableCameras;
        }

        public static LookupItemDTO GetNextCamera(IEnumerable<LookupItemDTO> availableCameras, string currentCamera, bool saveAsDefault)
        {
            LookupItemDTO nextCamera = null;

            if (availableCameras != null && availableCameras.Any())
            {                
                nextCamera = availableCameras.First(x=> x.ValueLong.Contains("VID_19AB&PID_2000"));

                if (nextCamera == null)
                    nextCamera = availableCameras.First();                
            }

            return nextCamera;
        }
        
        public static async Task ConfigureMedia(CaptureElement captureElement, LookupItemDTO cameraDevice, bool useHighRes)
        {
            if (cameraDevice != null)
            {
                if (captureElement.Source != null)
                {
                    await captureElement.Source.StopPreviewAsync();
                    captureElement.Source = null;
                }

                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.ValueLong });

                try
                {                 
                    var resolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).Select(x => x as VideoEncodingProperties);
                    double ratio = 480.00 / 640.00;
                    VideoEncodingProperties imageResolution = null;

                    //If using High Resolution, pick the largest resolution with the same aspect ratio as the default photo size, 
                    //   if not pick the resolution with the same aspect ratio and where the width is greater than or equal to the default photo size
                    if (!useHighRes)
                        imageResolution = resolutions.Where(x => x.Width >= 640.00 && ratio == ((double)x.Height / (double)x.Width)).OrderBy(x => x.Height * x.Width).FirstOrDefault();
                    if (imageResolution == null || useHighRes)
                        imageResolution = resolutions.Where(x => ratio == ((double)x.Height / (double)x.Width)).OrderByDescending(x => x.Height * x.Width).FirstOrDefault();

                    if (imageResolution != null)
                        await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, imageResolution);
                }
                catch
                {
                    //TODO: Swallow the error for now, some devices don't support MediaStreamType.Photo and i haven't found a good solution to handle this yet
                }

                mediaCapture.VideoDeviceController.PrimaryUse = CaptureUse.Photo;
                captureElement.Source = mediaCapture;
                captureElement.Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill;

                await mediaCapture.StartPreviewAsync();
            }
        }

        public static string GetUploadSessionID()
        {
            //Use local time zone
            var dateStamp = DateTime.Now;
            return string.Format("{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", dateStamp.Year, dateStamp.Month, dateStamp.Day, dateStamp.Hour, dateStamp.Minute, dateStamp.Second);
        }
        
        
        

        //public static WriteableBitmap GetSignatureBitmapFull()
        //{
        //    var bytes = ConvertInkCanvasToByteArray();

        //    if (bytes != null)
        //    {
        //        var width = (int)signatureInkCanvas.ActualWidth;
        //        var height = (int)signatureInkCanvas.ActualHeight;

        //        var bmp = new WriteableBitmap(width, height);
        //        using (var stream = bmp.PixelBuffer.AsStream())
        //        {
        //            stream.Write(bytes, 0, bytes.Length);
        //            return bmp;
        //        }
        //    }
        //    else
        //        return null;
        //}

    }
}
