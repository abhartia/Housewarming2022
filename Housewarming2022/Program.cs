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

            bool activated = false;
            while (true)
            {
                if (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 0)
                {
                    activated = true;
                }
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

            //Time offset in case all transitions need to be brought forward/sent backward
            int timeoffset = -500;

            new Task(() =>
            {
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Start").uuid, new SetStateRequest()
                {
                    Duration = 1,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 2311329);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Green & Gold").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 2589529);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 3168062);
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 3765945);
                    //Why'd You Only Call Me When You're High
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 3927068);
                    //Borderline
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 4164868);
                    //Do I Wanna Know?
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 4437262);
                    //Lady - Hear My Tonight
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 4744415);
                    //Arabella
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 4951771);
                    //Genghis Khan
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 5163917);
                    //Strange Effect
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Dream").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 5365378);
                    //Red Lights Part 1
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Early Warming").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 5365378 + 60500);
                    //Drop happens 60.5 seconds into the song
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Red Lights").uuid, new SetStateRequest()
                {
                    Duration = 1,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 5574832);
                    //Where You Belong and Best I Ever Had
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 6039559);
                    //Blinding Lights, Starboy
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 6470052);
                    //Lose Yourself to Dance, No diggity
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 7128545);
                    //DNA
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 7314491);
                    //Still D.R.E.
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Green").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 7585077);
                    //Backstreet Freestyle
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 7797730);
                    //Money
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 7981257);
                    //Nice For What and Black Parade
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 8473275);
                    //Vegas
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 8656181);
                    //Bad guy and goosebumps and risky business
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 9333923);
                    //Cyber sex
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Pink").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 9499656);
                    //Woman and Humble
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 9868482);
                    //Sicko mode
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 10181302);
                    //Save Your Tears
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 10372315);
                    //Don't Start Now + Tonight
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 10782418);
                    //Close Eyes
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 10914764);
                    //Animal Rights
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Green").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 11072009);
                    //Four to the floor and can't Feel My Face
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 11779914);
                    //Wild Thoughts
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 11984087);
                    //See You Again and Jubel
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 12381806);
                    //As It Was
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 12549109);
                    //Cold/mess only
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cold Mess").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 12830327);
                    //Passionfruit
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 13129267);
                    //I think i like you
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 13311925);
                    //Make Them Wheels Roll and On My Mind and Pretty Great
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 13959912);
                    //Late Night Talking
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 14137866);
                    //Gorgeous and freaky deaky
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 14562827);
                    //Little L
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 14858227);
                    //Catch 22
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 15079644);
                    //Rock DJ and Toxic
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 15537004);
                    //California Gurls
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 15771657);
                    //Shake it off and gold digger
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 16198483);
                    //Downtown and hollaback girl and don't phunk with my heart
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 16930702);
                    //Turn Down for What
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 17144435);
                    //The Real Slim Shady
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 17428635);
                    //Hey Ya!
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 17663848);
                    //Are You Gonna Be My Girl
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 17877648);
                    //Unbelievable
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Warm").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 18087461);
                    //Dance the way i feel and Funkytown
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Pink").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 18534380);
                    //A Thousand Miles
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Cool").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();

            new Task(() =>
            {
                Thread.Sleep(timeoffset + 18724985);
                    //Rolling in the Deep, Wolfie Nothing Breaks Like a Heart and Feel No Ways but would like darker
                client.ActivateScene(scenes.First(s => s.name == "Housewarming 2022 - Dark").uuid, new SetStateRequest()
                {
                    Duration = 5,
                    Fast = true
                });
            }).Start();
        }
    }
}
