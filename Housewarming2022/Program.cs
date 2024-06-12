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
            Paging<FullPlaylist> playlists = await spotify.Playlists.CurrentUsers();

            //spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
            //{
            //    DeviceId = devices.Devices.First(s => s.Name == "BHUPENDRAJOGI").Id, //BHUPENDRAJOGI or XboxOne
            //    ContextUri = playlists.Items.First(s => s.Name == "30").Uri
            //});

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
            Dictionary<string, List<Color>> tops = new Dictionary<string, List<Color>>();
            Dictionary<string, List<Color>> bottoms = new Dictionary<string, List<Color>>();
            CurrentlyPlaying runningTrack = null;

            while (true)
            {
                //Get current track
                CurrentlyPlaying currentTrack = await spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));
                if (runningTrack != null && currentTrack != null && (runningTrack.Item as FullTrack).Id == (currentTrack.Item as FullTrack).Id)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                runningTrack = currentTrack;

                var artist = await spotify.Artists.Get((runningTrack.Item as FullTrack).Artists.First().Id);

                //Get album art
                string imageURL = (runningTrack.Item as FullTrack).Album.Images.First().Url;

                //Get album art bitmap
                Stopwatch sw = new Stopwatch();
                sw.Start();

                if (!tops.ContainsKey(imageURL))
                {
                    Bitmap albumArt = new Bitmap(await GetBitmapFromUrlAsync(imageURL), new Size(200, 200));

                    Console.WriteLine("Album art retrieved in ms: " + sw.ElapsedMilliseconds);

                    // Lounge and TV strip lights should be based on top half of album art
                    Console.WriteLine("Top: ");
                    List<Color> topColours = GetTopPrimaryColours(albumArt);
                    tops.Add(imageURL, topColours);

                    // Kitchen, bench strip and dining room lights should be based on bottom half of album art
                    Console.WriteLine("Bottom: ");
                    var bottomColours = GetBottomPrimaryColours(albumArt);
                    bottoms.Add(imageURL, bottomColours);
                }
            }
        }

        public static List<Color> GetTopPrimaryColours(Bitmap bitmap)
        {
            return GetPrimaryColours(bitmap, true);
        }

        public static List<Color> GetBottomPrimaryColours(Bitmap bitmap)
        {
            return GetPrimaryColours(bitmap, false);
        }

        private static List<Color> GetPrimaryColours(Bitmap bitmap, bool top)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Lock the bitmap's bits
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, top ? 0 : bitmap.Height / 2, bitmap.Width, bitmap.Height / 2), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int byteCount = bmpData.Stride * (bitmap.Height / 2);
            byte[] pixels = new byte[byteCount];

            // Copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, byteCount);

            // Unlock the bits
            bitmap.UnlockBits(bmpData);

            // Prepare the data for k-means
            double[][] pixelData = new double[bitmap.Width * bitmap.Height / 2][];
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
            KMeans kmeans = new KMeans(8);
            KMeansClusterCollection clusters = kmeans.Learn(pixelData);

            // Create a list to store colors and their vibrancy
            List<Tuple<Color, double>> vibrantColors = new List<Tuple<Color, double>>();

            foreach (var cluster in clusters.Where(s => s.Proportion > 0.045))
            {
                var color = Color.FromArgb(
                    (int)(cluster.Centroid[0] * 255),
                    (int)(cluster.Centroid[1] * 255),
                    (int)(cluster.Centroid[2] * 255)
                );

                double vibrancy = CalculateVibrancy(color);

                // Add the color and its vibrancy to the list
                vibrantColors.Add(Tuple.Create(color, vibrancy));
            }

            // Sort the colors by vibrancy in descending order
            vibrantColors = vibrantColors.OrderByDescending(c => c.Item2).ToList();

            // Take the top 6 most vibrant colors
            List<Color> topVibrantColors = vibrantColors.Take(7).Select(c => c.Item1).ToList();

            foreach (var color in topVibrantColors)
            {
                PrintColorSquare(color);
                Console.WriteLine();
            }

            Console.WriteLine("Found in ms: " + sw.ElapsedMilliseconds);
            Console.WriteLine();

            // Return the top 6 most vibrant colors
            return topVibrantColors;
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
            Console.Write($"\x1b[48;2;{r};{g};{b}m  \x1b[0m");
            Console.Write($"\x1b[48;2;{r};{g};{b}m  \x1b[0m");
        }
    }
}
