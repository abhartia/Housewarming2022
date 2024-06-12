using Accord.MachineLearning;
using LifxCloud.NET;
using LifxCloud.NET.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static LifxCloud.NET.LifxCloudClient;
using Color = System.Drawing.Color;

namespace Housewarming2022
{
    class Program
    {
        private static EmbedIOAuthServer _server;

        static string LifxAppToken = "";
        static string SpotifyClientID = "";
        static string SpotifySecret = "";

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appSettings.json")
                            .AddEnvironmentVariables();
            var configurationRoot = builder.Build();
            LifxAppToken = configurationRoot["LifxAppToken"];
            SpotifyClientID = configurationRoot["SpotifyClientID"];
            SpotifySecret = configurationRoot["SpotifySecret"];

            bool activated = true;
            while (true)
            {
                //if (DateTime.Now.Hour == 16 && DateTime.Now.Minute == 0)
                //{
                //    activated = true;
                //}
                if (activated)
                {
                    _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
                    await _server.Start();

                    _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

                    var request = new LoginRequest(_server.BaseUri, SpotifyClientID, LoginRequest.ResponseType.Code)
                    {
                        Scope = new List<string> {
                            Scopes.UgcImageUpload,
                            Scopes.UserReadRecentlyPlayed ,
                            Scopes.UserReadPlaybackPosition,
                            Scopes.UserTopRead ,
                            Scopes.UserLibraryRead ,
                            Scopes.UserLibraryModify,
                            Scopes.PlaylistModifyPrivate ,
                            Scopes.PlaylistReadPrivate ,
                            Scopes.UserFollowRead ,
                            Scopes.PlaylistModifyPublic ,
                            Scopes.UserReadPrivate ,
                            Scopes.UserReadEmail ,
                            Scopes.AppRemoteControl ,
                            Scopes.Streaming ,
                            Scopes.UserReadCurrentlyPlaying ,
                            Scopes.UserModifyPlaybackState ,
                            Scopes.UserReadPlaybackState ,
                            Scopes.PlaylistReadCollaborative ,
                            Scopes.UserFollowModify
                        }
                    };
                    var uri = request.ToUri();
                    BrowserUtil.Open(uri);

                    Thread.Sleep(99930000);
                }
                Thread.Sleep(60000);
            }
        }

        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            LifxCloudClient lifxClient = await LifxCloudClient.CreateAsync(LifxAppToken);
            //List<Light> lights = await client.ListLights();

            await _server.Stop();

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
              new AuthorizationCodeTokenRequest(
                SpotifyClientID, SpotifySecret, response.Code, new Uri("http://localhost:5000/callback")
              )
            );

            SpotifyClient spotify = new SpotifyClient(tokenResponse.AccessToken);
            DeviceResponse devices = await spotify.Player.GetAvailableDevices();
            Paging<SimplePlaylist> playlists = await spotify.Playlists.CurrentUsers();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
            //{
            //    DeviceId = devices.Devices.First(s => s.Name == "BHUPENDRAJOGI").Id, //BHUPENDRAJOGI or XboxOne
            //    ContextUri = playlists.Items.First(s => s.Name == "30").Uri
            //});
            sw.Stop();
            Debug.WriteLine("SW: " + sw.ElapsedMilliseconds + " ms.");

            // do calls with Spotify and save token?
            SyncLights(lifxClient, spotify);

        }

        public static async Task<Bitmap> GetBitmapFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                // Download the image data as a byte array
                byte[] imageData = await client.GetByteArrayAsync(url);

                // Create a memory stream from the downloaded data
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    // Create and return a Bitmap from the memory stream
                    return new Bitmap(ms);
                }
            }
        }

        static async void SyncLights(LifxCloudClient lifxClient, SpotifyClient spotify)
        {
            Dictionary<string, Bitmap> imageLibrary = new Dictionary<string, Bitmap>();
            CurrentlyPlaying currentTrack = null;

            while (true)
            {
                //Get current track
                currentTrack = await spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));

                //Get album art
                string albumArtURL = (currentTrack.Item as FullTrack).Album.Images.First().Url;

                //Get album art bitmap
                Stopwatch sw = new Stopwatch();
                sw.Start();

                if (!imageLibrary.ContainsKey(albumArtURL))
                {
                    imageLibrary.Add(albumArtURL, new Bitmap(await GetBitmapFromUrlAsync(albumArtURL), new Size(200, 200)));
                }
                Bitmap albumArt = imageLibrary[albumArtURL];

                Console.WriteLine("Album art retrieved in ms: " + sw.ElapsedMilliseconds);
                var color = GetPrimaryColor(albumArt);

                Thread.Sleep(1000);
            }
            return;
        }

        public static Color GetPrimaryColor(Bitmap bitmap)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Lock the bitmap's bits
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height / 2), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int byteCount = bmpData.Stride * (bitmap.Height / 2);
            byte[] pixels = new byte[byteCount];

            // Copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

            // Unlock the bits
            bitmap.UnlockBits(bmpData);

            // Prepare the data for k-means
            double[][] pixelData = new double[(bitmap.Width * bitmap.Height) / 2][];
            int index = 0;
            for (int y = 0; y < bitmap.Height / 2; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int i = (y * bmpData.Stride) + (x * bytesPerPixel);
                    byte b = pixels[i];
                    byte g = pixels[i + 1];
                    byte r = pixels[i + 2];

                    pixelData[index++] = [r / 255.0, g / 255.0, b / 255.0];
                }
            }

            // Apply k-means clustering
            KMeans kmeans = new KMeans(6);
            KMeansClusterCollection clusters = kmeans.Learn(pixelData);

            // Find the most vibrant cluster centroid
            double maxVibrancy = 0;
            double[] mostVibrantColor = clusters.OrderBy(s => s.Centroid.First()).Last().Centroid;
            foreach (var cluster in clusters)
            {
                var color = Color.FromArgb(
                    (int)(cluster.Centroid[0] * 255),
                    (int)(cluster.Centroid[1] * 255),
                    (int)(cluster.Centroid[2] * 255)
                );

                double vibrancy = CalculateVibrancy(color);

                if (vibrancy > maxVibrancy && cluster.Proportion > 0.05)
                {
                    maxVibrancy = vibrancy;
                    mostVibrantColor = cluster.Centroid;
                }
            }

            // Convert the most vibrant color to RGB values
            int rValue = (int)(mostVibrantColor[0] * 255);
            int gValue = (int)(mostVibrantColor[1] * 255);
            int bValue = (int)(mostVibrantColor[2] * 255);

            foreach (var cluster in clusters)
            {
                Console.Write("Cluster " + cluster.Index + ": ");
                PrintColorSquare(Color.FromArgb((int)(cluster.Centroid[0] * 255), (int)(cluster.Centroid[1] * 255), (int)(cluster.Centroid[2] * 255)));
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Most prominent: ");
            PrintColorSquare(Color.FromArgb(rValue, gValue, bValue));
            Console.WriteLine();
            Console.WriteLine("Found in ms: " + sw.ElapsedMilliseconds);
            Console.WriteLine();

            // Return the primary color as a Color object
            return Color.FromArgb(rValue, gValue, bValue);
        }

        public static double CalculateVibrancy(Color color)
        {
            // Vibrancy is influenced by the saturation and brightness of the color
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double chroma = max - min;

            // Return the chroma (a measure of color intensity) as a proxy for vibrancy
            return chroma;
        }

        public static void PrintColorSquare(Color color)
        {
            // Convert color to RGB
            int r = color.R;
            int g = color.G;
            int b = color.B;

            // Use ANSI escape codes to set the background color
            Console.Write($"\x1b[48;2;{r};{g};{b}m  \x1b[0m");
        }
    }
}
