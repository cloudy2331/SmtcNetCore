﻿using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog;
using System;
using Windows.Media.Control;
using WindowsMediaController;
using Windows.Storage.Streams;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text;

namespace Sample.CMD
{
    class SmtcReturn
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public Windows.Storage.Streams.IRandomAccessStreamReference Thumbnail { get; set; }
    }

    class Program
    {
        static MediaManager mediaManager;
        static readonly object _writeLock = new object();

        public static void Main()
        {
            mediaManager = new MediaManager()
            {
                //Logger = BuildLogger("MediaManager"),
            };

            //mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
            //mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
            //mediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;
            //mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
            //mediaManager.OnAnyTimelinePropertyChanged += MediaManager_OnAnyTimelinePropertyChanged;

            mediaManager.Start();

            Console.ReadLine();
            mediaManager.Dispose();
        }

        private static void MediaManager_OnAnySessionOpened(MediaManager.MediaSession session)
        {
            WriteLineColor("-- New Source: " + session.Id, ConsoleColor.Green);
        }
        private static void MediaManager_OnAnySessionClosed(MediaManager.MediaSession session)
        {
            WriteLineColor("-- Removed Source: " + session.Id, ConsoleColor.Red);
        }

        private static void MediaManager_OnFocusedSessionChanged(MediaManager.MediaSession mediaSession)
        {
            WriteLineColor("== Session Focus Changed: " + mediaSession?.ControlSession?.SourceAppUserModelId, ConsoleColor.Gray);
        }

        private static void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            WriteLineColor($"{sender.Id} is now {args.PlaybackStatus}", ConsoleColor.Yellow);
        }

        private static void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            //WriteLineColor($"{sender.Id} is now playing {args.Title} {(string.IsNullOrEmpty(args.Artist) ? "" : $"by {args.Artist}")}", ConsoleColor.Cyan);
            var imageStream = args.Thumbnail.OpenReadAsync().GetAwaiter().GetResult();
            byte[] fileBytes = new byte[imageStream.Size];
            using (DataReader reader = new DataReader(imageStream))
            {
                reader.LoadAsync((uint)imageStream.Size).GetAwaiter().GetResult();
                reader.ReadBytes(fileBytes);
            }
            /*var image = new BitmapImage();
            using (var ms = new System.IO.MemoryStream(fileBytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }*/

            string image = Convert.ToBase64String(fileBytes);
            //string image = System.Text.Encoding.UTF8.GetString(fileBytes);

            /*for (int i = 0; i < fileBytes.Length; i++)
            {
                image += fileBytes[i] + ",";
            }*/
            /*foreach (int i in fileBytes)
            {
                image += i;
            }*/
            //string outputs = $@"OnAnyMediaPropertyChanged,{sender.Id},{args.Title},{(string.IsNullOrEmpty(args.Artist) ? "" : $"{args.Artist}")},{image.Trim()}";
            /*var blob = new BitmapMetadataBlob(fileBytes);
            var blobUrl = Convert.ToString(blob);*/
            string outputs = $@"OnAnyMediaPropertyChanged,{sender.Id},{args.Title.Replace(',', ' ')},{(string.IsNullOrEmpty(args.Artist) ? "" : $"{args.Artist.Replace(',', ' ')}")},{image}=";
            Console.Write(outputs);
            /*SmtcReturn smtcReturn = new SmtcReturn();
            smtcReturn.Id = sender.Id;
            smtcReturn.Title = args.Title;
            smtcReturn.Artist = args.Artist;
            smtcReturn.Thumbnail = args.Thumbnail;*/
        }

        private static void MediaManager_OnAnyTimelinePropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionTimelineProperties args)
        {
            WriteLineColor($"{sender.Id} timeline is now {args.Position}/{args.EndTime}", ConsoleColor.Magenta);
        }

        public static void WriteLineColor(object toprint, ConsoleColor color = ConsoleColor.White)
        {
            lock (_writeLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + toprint);
            }
        }

        /*private static Microsoft.Extensions.Logging.ILogger BuildLogger(string sourceContext = null)
        {
            return new LoggerFactory().AddSerilog(logger: new LoggerConfiguration()
                    .MinimumLevel.Is(LogEventLevel.Information)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u4}] " + (sourceContext ?? "{SourceContext}") + ": {Message:lj}{NewLine}{Exception}")
                    .CreateLogger())
                    .CreateLogger(string.Empty);
        }*/
    }
}