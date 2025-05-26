using System;
using System.Management;
using System.Runtime.InteropServices;
using DirectShowLib;
using OpenCvSharp;
using TestOpenCVSharp4.Data.DO;

namespace TestOpenCVSharp4.Core
{
    public static class Utils
    {
        public static List<CamInfoDO> ListCameras()
        {
            List<CamInfoDO> lst = [];

            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < devices.Length; i++)
            {
                var d = devices[i];
                
                var cam = new CamInfoDO(i, d.Name, "");
                lst.Add(cam);
                
                var resolutions = GetAllAvailableResolution(d);
                resolutions.Sort();
                if (resolutions != null)
                {
                    foreach (var r in resolutions) { 
                        cam.Resolutions.Add(r);
                    }
                }
            }
            
            return lst;
        }

        private static List<CamResolutionDO> GetAllAvailableResolution(DsDevice vidDev)
        {
            var AvailableResolutions = new List<CamResolutionDO>();
            
            try
            {
                if (new FilterGraph() is IFilterGraph2 graph)
                {
                    // Add the camera filter to the graph
                    graph.AddSourceFilterForMoniker(vidDev.Mon, null, vidDev.Name, out IBaseFilter? capFilter);

                    var captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                    captureGraphBuilder.SetFiltergraph(graph);

                    // Query IAMStreamConfig
                    captureGraphBuilder.FindInterface(
                        PinCategory.Capture,
                        MediaType.Video,
                        capFilter,
                        typeof(IAMStreamConfig).GUID,
                        out object configObj);

                    if (configObj is IAMStreamConfig streamConfig)
                    {
                        streamConfig.GetNumberOfCapabilities(out int count, out int size);
                        IntPtr taskMemPointer = Marshal.AllocCoTaskMem(size);

                        for (int i = 0; i < count; i++)
                        {
                            streamConfig.GetStreamCaps(i, out AMMediaType mediaType, taskMemPointer);
                            VideoStreamConfigCaps? caps = (VideoStreamConfigCaps?)Marshal.PtrToStructure(taskMemPointer, typeof(VideoStreamConfigCaps));
                            if (caps == null) continue;

                            VideoInfoHeader? videoInfoHeader = (VideoInfoHeader?)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
                            if (videoInfoHeader == null) continue;

                            int width = videoInfoHeader.BmiHeader.Width;
                            int height = videoInfoHeader.BmiHeader.Height;

                            // Frame intervals in 100ns units
                            long minInterval = caps.MinFrameInterval;
                            long maxInterval = caps.MaxFrameInterval;

                            double maxFps = 10_000_000.0 / minInterval;
                            double minFps = 10_000_000.0 / maxInterval;

                            if (width > 0 && height > 0)
                            {
                                AvailableResolutions.Add(new CamResolutionDO() { W = width, H = height, FPS = (int)maxFps });
                            }

                            DsUtils.FreeAMMediaType(mediaType);
                        }

                        Marshal.FreeCoTaskMem(taskMemPointer);
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return AvailableResolutions;
        }

        public static List<CamInfoDO> ListCameras2()
        {
            List<CamInfoDO> lst = [];

            var cameraNames = new List<string?>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraNames.Add(device["Caption"].ToString());
                }
            }

            for (int i = 0; i < cameraNames.Count; i++)
            {
                try
                {
                    using var capture = new VideoCapture(i);
                    if (capture.IsOpened())
                    {
                        System.Diagnostics.Debug.WriteLine($"✔ OpenCV index {i} opened successfully.");

                        // Optional: show resolution or grab a test frame
                        int width = (int)capture.Get(VideoCaptureProperties.FrameWidth);
                        int height = (int)capture.Get(VideoCaptureProperties.FrameHeight);
                        int fps = (int)capture.Get(VideoCaptureProperties.Fps);

                        lst.Add(new CamInfoDO(i, capture.GetBackendName(), cameraNames[i]));
                    }
                }
                catch { }
            }

            return lst;
        }
    }
}
