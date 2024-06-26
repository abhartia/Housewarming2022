﻿using LifxCloud.NET.Infrastructure;
using LifxCloud.NET.Models;
using Newtonsoft.Json;
using System;
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

        internal const float DEFAULT_DURATION = 1f;
        internal const PowerState DEFAULT_POWER_ON = PowerState.On;

        private static HttpClient Client { get; set; }

        private const string Url = "https://api.lifx.com/v1/";

        private readonly string LightEndPoint = $"{Url}lights/";

        private readonly string SceneEndPoint = $"{Url}scenes/";

        private readonly string ColorEndPoint = $"{Url}color/";

        private LifxCloudClient()
        {
        }

        public static Task<LifxCloudClient> CreateAsync(string appToken)
        {
            LifxCloudClient client = new LifxCloudClient();
            client.Initialize(appToken);
            return Task.FromResult(client);
        }

        public void Initialize(string appToken)
        {
            RequestFactory _factory = new RequestFactory();
            Client = _factory.CreateClient(new AuthenticationHeaderValue("Bearer", appToken));

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            appToken);
        }

        public async Task<ColorResult> ValidateColor(string color)
        {
            var response = await GetResponseData<ColorResult>($"{ColorEndPoint}?string={color}");

            if (response.GetType() == typeof(ErrorResponse))
            {
                throw new Exception(((ErrorResponse)response).Error);
            }
            else
            {
                return (ColorResult)response;
            }
        }

        internal static async Task<ApiResponse> PutResponseData<T>(string url, object data = null)
        {
            return await await ResilientCall(async () =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await Client.PutAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                ApiResponse resource;

                if (result.Contains("error"))
                {
                    resource = JsonConvert.DeserializeObject<ErrorResponse>(result);
                }
                else
                {
                    resource = JsonConvert.DeserializeObject<SuccessResponse>(result);
                }
                sw.Stop();
                Debug.WriteLine("Put request took " + sw.ElapsedMilliseconds + " ms.");
                return resource;
            });
        }

        internal static async Task<ApiResponse> PostResponseData<T>(string url, object data)
        {
            return await await ResilientCall(async () =>
            {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await Client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                ApiResponse resource;

                if (result.Contains("error"))
                {
                    resource = JsonConvert.DeserializeObject<ErrorResponse>(result);
                }
                else
                {
                    resource = JsonConvert.DeserializeObject<SuccessResponse>(result);
                }

                return resource;
            });
        }

        internal static async Task<object> GetResponseData<T>(String url)
        {
            Func<Task<object>> block = async () =>
                                    {
                                        var response = await Client.GetAsync(url);

                                        var result = await response.Content.ReadAsStringAsync();
                                        object resource;

                                        if (result.Contains("error"))
                                        {
                                            resource = JsonConvert.DeserializeObject<ErrorResponse>(result);
                                        }
                                        else
                                        {
                                            resource = JsonConvert.DeserializeObject<T>(result);
                                        }

                                        return resource;
                                    };
            Task<object> task = await ResilientCall(block);
            return await task;
        }

        async static Task<T> ResilientCall<T>(Func<T> block)
        {
            int currentRetry = 0;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            for (; ; )
            {
                try
                {
                    return block();
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Operation Exception");

                    currentRetry++;

                    // Check if the exception thrown was a transient exception
                    // based on the logic in the error detection strategy.
                    // Determine whether to retry the operation, as well as how
                    // long to wait, based on the retry strategy.
                    if (currentRetry > 3 || !IsTransient(ex))
                    {
                        // If this isn't a transient error or we shouldn't retry, 
                        // rethrow the exception.
                        throw;
                    }
                }

                // Wait to retry the operation.
                // Consider calculating an exponential delay here and
                // using a strategy best suited for the operation and fault.
                await Task.Delay(delay);
            }
        }

        private static bool IsTransient(Exception ex)
        {
            if (ex is WebException webException)
            {
                // If the web exception contains one of the following status values
                // it might be transient.
                return new[] {WebExceptionStatus.ConnectionClosed,
                  WebExceptionStatus.Timeout,
                  WebExceptionStatus.RequestCanceled }.
                        Contains(webException.Status);
            }

            // Additional exception checking logic goes here.
            return false;
        }
    }
}
