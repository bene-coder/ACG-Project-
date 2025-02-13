using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.AspNetCore.TestBase;
using ACGProjectGoGlobal.EntityFrameworkCore;
using ACGProjectGoGlobal.Tests.TestDatas;
using ACGProjectGoGlobal.Web.Startup;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Shouldly;

namespace ACGProjectGoGlobal.Web.Tests
{
    public abstract class ACGProjectGoGlobalWebTestBase : AbpAspNetCoreIntegratedTestBase<Startup>
    {
        private readonly ACGProjectGoGlobalDbContext _dbContext;
        protected new readonly HttpClient Client;

        protected static readonly Lazy<string> ContentRootFolder =
            new(() => WebContentDirectoryFinder.CalculateContentRootFolder(), true);

        protected ACGProjectGoGlobalWebTestBase()
        {
            _dbContext = ServiceProvider.GetRequiredService<ACGProjectGoGlobalDbContext>();
            Client = Server.CreateClient();
            UsingDbContext(context => new TestDataBuilder(context).Build());
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base.CreateWebHostBuilder()
                .UseContentRoot(ContentRootFolder.Value)
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(ACGProjectGoGlobalWebModule).Assembly.FullName);
        }

        #region Get response

        protected async Task<T> GetResponseAsObjectAsync<T>(string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var strResponse = await GetResponseAsStringAsync(url, expectedStatusCode);
            if (string.IsNullOrEmpty(strResponse))
            {
                throw new Exception("API returned an empty response");
            }
            return JsonConvert.DeserializeObject<T>(strResponse, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        protected async Task<string> GetResponseAsStringAsync(string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var response = await GetResponseAsync(url, expectedStatusCode);
            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<HttpResponseMessage> GetResponseAsync(string url,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var response = await Client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

    //         if (response.StatusCode != expectedStatusCode)
    //         {
    // throw new Exception($"Expected {(int)expectedStatusCode} ({expectedStatusCode}), but got {(int)response.StatusCode} ({response.StatusCode}). Response: {content}");
    //         }
            return response;
        }

        #endregion

        #region UsingDbContext

        protected void UsingDbContext(Action<ACGProjectGoGlobalDbContext> action)
        {
            action(_dbContext);
            _dbContext.SaveChanges();
        }

        protected T UsingDbContext<T>(Func<ACGProjectGoGlobalDbContext, T> func)
        {
            var result = func(_dbContext);
            _dbContext.SaveChanges();
            return result;
        }

        protected async Task UsingDbContextAsync(Func<ACGProjectGoGlobalDbContext, Task> action)
        {
            await action(_dbContext);
            await _dbContext.SaveChangesAsync(true);
        }

        protected async Task<T> UsingDbContextAsync<T>(Func<ACGProjectGoGlobalDbContext, Task<T>> func)
        {
            var result = await func(_dbContext);
            await _dbContext.SaveChangesAsync(true);
            return result;
        }

        #endregion

        #region ParseHtml

        protected IHtmlDocument ParseHtml(string htmlString)
        {
            return new HtmlParser().ParseDocument(htmlString);
        }

        #endregion
    }
}
