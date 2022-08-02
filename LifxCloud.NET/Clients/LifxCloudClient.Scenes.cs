using LifxCloud.NET.Infrastructure;
using LifxCloud.NET.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LifxCloud.NET
{
    public partial class LifxCloudClient
    {
        public async Task<List<SceneResponse>> ListScenes()
        {
            var response = await GetResponseData<List<SceneResponse>>($"{SceneEndPoint}");

            if (response.GetType() == typeof(ErrorResponse))
            {
                throw new Exception(((ErrorResponse)response).Error);
            }
            else
            {
                return (List<SceneResponse>)response;
            }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Account
        {
            public string uuid { get; set; }
        }

        public class Color
        {
            public double hue { get; set; }
            public double saturation { get; set; }
            public double kelvin { get; set; }
        }

        public class SceneResponse
        {
            public string uuid { get; set; }
            public string name { get; set; }
            public Account account { get; set; }
            public List<State> states { get; set; }
            public int created_at { get; set; }
            public int updated_at { get; set; }
        }


        public class State
        {
            public string selector { get; set; }
            public string power { get; set; }
            public double brightness { get; set; }
            public Color color { get; set; }
            public List<Zone> zones { get; set; }
        }

        public class State2
        {
            public string serial_number { get; set; }
            public double brightness { get; set; }
            public Color color { get; set; }
        }

        public class Zone
        {
            public int start_index { get; set; }
            public int end_index { get; set; }
            public State state { get; set; }
        }



        public async Task<Scene> GetScene(string selector)
        {
            var response = await GetResponseData<ApiResponse>($"{SceneEndPoint}{selector}");

            if (response.GetType() == typeof(ErrorResponse))
            {
                throw new Exception(((ErrorResponse)response).Error);
            }
            else
            {
                return (Scene)response;
            }
        }

        public async Task<ApiResponse> ActivateScene(string scene_uuid, SetStateRequest request = null)
        {
            return await PutResponseData<ApiResponse>($"{SceneEndPoint}scene_id:{scene_uuid}/activate", request);
        }
    }
}