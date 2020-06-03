﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Aptacode.CSharp.Core.Http;
using Aptacode.CSharp.Core.Services.Interfaces;
using Aptacode.CSharp.Utilities.Persistence;
using Newtonsoft.Json;

namespace Aptacode.CSharp.Core.Services
{
    public abstract class GenericHttpApiServiceClient<TEntity> : GenericHttpApiServiceClient<TEntity, TEntity>
        where TEntity : IEntity
    {
        protected GenericHttpApiServiceClient(IAccessTokenService authService, ServerAddress serverAddress,
            string apiRoute, string controllerRoute) : base(authService, serverAddress, apiRoute, controllerRoute)
        {
        }
    }

    public abstract class GenericHttpApiServiceClient<TGetViewModel, TPutViewModel>
    {
        protected static HttpClient HttpClient = new HttpClient();
        protected readonly HttpRouteBuilder ApiRouteBuilder;
        protected IAccessTokenService AuthService;

        protected GenericHttpApiServiceClient(IAccessTokenService authService, ServerAddress serverAddress,
            string apiRoute,
            string controllerRoute)
        {
            ApiRouteBuilder = new HttpRouteBuilder(serverAddress, apiRoute, controllerRoute);
            AuthService = authService;
        }

        protected async Task<IEnumerable<TGetViewModel>> Get()
        {
            var response = await HttpClient.SendAsync(GetRequestTemplate(HttpMethod.Get, ApiRouteBuilder.GetRoute()));
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<TGetViewModel>>(body);
            }

            return null;
        }

        protected async Task<TGetViewModel> Get(int id)
        {
            var response =
                await HttpClient.SendAsync(GetRequestTemplate(HttpMethod.Get, ApiRouteBuilder.GetRoute(id.ToString())));

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TGetViewModel>(body);
            }

            return default;
        }

        protected async Task<TGetViewModel> Put(TPutViewModel entity)
        {
            var req = GetRequestTemplate(HttpMethod.Put, ApiRouteBuilder.GetRoute());
            req.Content = new StringContent(JsonConvert.SerializeObject(entity));
            var response = await HttpClient.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TGetViewModel>(body);
            }

            return default;
        }

        protected async Task<TGetViewModel> Push(int id, TPutViewModel entity)
        {
            var req = GetRequestTemplate(HttpMethod.Put, ApiRouteBuilder.GetRoute(id.ToString()));
            req.Content = new StringContent(JsonConvert.SerializeObject(entity));
            var response = await HttpClient.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TGetViewModel>(body);
            }

            return default;
        }

        protected async Task<bool> Delete(int id)
        {
            var req = GetRequestTemplate(HttpMethod.Delete, ApiRouteBuilder.GetRoute(id.ToString()));
            var response = await HttpClient.SendAsync(req);
            return response.IsSuccessStatusCode;
        }

        protected HttpRequestMessage GetRequestTemplate(HttpMethod method, string endpoint)
        {
            var accessToken = AuthService.GetAccessToken();
            if (accessToken == null) throw new Exception("You are not authorized to view this content");

            var req = new HttpRequestMessage {Method = method, RequestUri = new Uri(endpoint)};
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return req;
        }
    }
}