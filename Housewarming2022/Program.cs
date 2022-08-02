using LifxCloud.NET;
using LifxCloud.NET.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            
            //spotify client id: f5f7da86cb004ce6bf9c3f4ff4f1ac92
            // spotify secret: bc81c5c4e6cb4fc3ae95750419cbff56
            bool activated = false;
            //while (true)
            //{
            //    if (DateTime.Now.Hour == 17 && DateTime.Now.Minute == 3)
            //    {
            activated = true;
            //}
            if (activated)
            {

                // Make sure "http://localhost:5000/callback" is in your spotify application as redirect uri!
                _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
                await _server.Start();

                _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

                var request = new LoginRequest(_server.BaseUri, "f5f7da86cb004ce6bf9c3f4ff4f1ac92", LoginRequest.ResponseType.Code)
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
            //}
        }

        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            LifxCloudClient client = await LifxCloudClient.CreateAsync(LifxAppToken);
            //List<Light> lights = await client.ListLights();

            await _server.Stop();

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
              new AuthorizationCodeTokenRequest(
                SpotifyClientID, SpotifySecret, response.Code, new Uri("http://localhost:5000/callback")
              )
            );

            var spotify = new SpotifyClient(tokenResponse.AccessToken);
            DeviceResponse devices = await spotify.Player.GetAvailableDevices();
            Paging<SimplePlaylist> playlists = await spotify.Playlists.CurrentUsers();

            //await spotify.Player.TransferPlayback(new PlayerTransferPlaybackRequest(new List<string>() { devices.Devices.First(s => s.Name == "ABU-SB2").Id })
            //{ 
            //    Play = true
            //});
            Stopwatch sw = new Stopwatch();
            sw.Start();
            spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
            {
                DeviceId = devices.Devices.First(s => s.Name == "XboxOne").Id, //ABU-SB2
                ContextUri = playlists.Items.First(s => s.Name == "Housewarming 2022").Uri
            });
            sw.Stop();
            Debug.WriteLine("SW: " + sw.ElapsedMilliseconds + " ms.");

            // do calls with Spotify and save token?
            LightSequence(client);

        }

        static async void LightSequence(LifxCloudClient client)
        {
            //ACTUAL PLAYLIST BELOW
            List<LifxCloudClient.SceneResponse> scenes = await client.ListScenes();

            new Task(() =>
            {
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022-1").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Green & Gold").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Why'd You Only Call Me When You're High
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Borderline
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Do I Wanna Know?
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Lady - Hear My Tonight
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Arabella
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Genghis Khan
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Strange Effect
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(10000);
                //Red Lights Part 1
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(20000);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Red Lights").uuid, new SetStateRequest()
                {
                    Duration = 1,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(30000);
                //Where You Belong and Best I Ever Had
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022-4").uuid, new SetStateRequest()
                {
                    Duration = 1,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(40000);
                //Blinding Lights
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022-5").uuid, new SetStateRequest()
                {
                    Duration = 1,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(50000);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022-6").uuid, new SetStateRequest()
                {
                    Duration = 1,
                    Fast = true
                });
            }).Start();

        }
    }
}
