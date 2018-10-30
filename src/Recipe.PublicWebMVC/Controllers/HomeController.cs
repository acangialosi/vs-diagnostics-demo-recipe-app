﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PublicWebMVC.Models;
using RestSharp;

namespace PublicWebMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.URL = BuildAbsolute("/api/recipes/", new QueryString("?limit=16"));
            return View();
        }

        public string BuildAbsolute(PathString path, QueryString query = default(QueryString), FragmentString fragment = default(FragmentString))
        {
            var rq = HttpContext.Request;
            return Microsoft.AspNetCore.Http.Extensions.UriHelper.BuildAbsolute(rq.Scheme, rq.Host, rq.PathBase, path, query, fragment);
        }

        public async Task<IActionResult> SearchResults(string searchString)
        {
            IList<Models.Recipe> recipes = null;

            if (!String.IsNullOrEmpty(searchString)) {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(BuildAbsolute("/api/recipes/search/"));
                HttpResponseMessage response = await client.GetAsync(searchString);

                if (response.IsSuccessStatusCode)
                {
                    recipes = await response.Content.ReadAsAsync<IList<Models.Recipe>>();
                    //recipe = JsonConvert.DeserializeObject<Recipe.Service.Models.Recipe>(recipeString);
                }
                else {

                }
            }
            
            return View(recipes);
        }

        public async Task<IActionResult> Recipe(string id)
        {
            long idAsLong = 0;
            if (!long.TryParse(id, out idAsLong))
            {
                throw new Exception("Invalid id");
            }

            Models.Recipe recipe = await GetRecipeById(idAsLong);

            if (recipe == null)
            {
                throw new Exception($"Recipe not found for id {id}");
            }

            return View(recipe);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private RestClient apiClient;

        private async Task<Models.Recipe> GetRecipeById(long id)
        {
            // Note: This is all crazytown code to demonstrate snapshots on exceptions
            apiClient = new RestClient(BuildAbsolute(""));
            RestRequest request = new RestRequest();
            request.Resource = "api/recipes/{id}";
            request.AddUrlSegment("id", id);

            IRestResponse<Models.Recipe> response = null;

            try
            {
                response = await ExecuteRestSharpAsync(request);
            } 
            catch(Exception ex)
            {
                // Todo: Log errors...
                return null;
            }
            
            return response.Data;
        }

        private async Task<IRestResponse<Models.Recipe>> ExecuteRestSharpAsync(RestRequest request)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            IRestResponse<Models.Recipe> response = await apiClient.ExecuteTaskAsync<Models.Recipe>(request, cancellationTokenSource.Token);
            if(response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            return response;
        }

        private string GetRecipeJsonById(long id)
        {
            return "";
        }

    }
}
