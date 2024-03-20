
using OtterliAPI;
using System;
using Newtonsoft.Json;


namespace APIClientv2;

[TestFixture]
public class ClientTests
{
    private API otr_api;

    public ClientTests()
    {
        DotNetEnv.Env.TraversePath().Load();
        string token = Environment.GetEnvironmentVariable("API_TOKEN");
        otr_api = new API(token);
    }

    [Test]
    public async Task APIHealth()
    {   
        API api = new API();
        HttpResponseMessage response = await api.APIHealthCheck();
        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        var content = await response.Content.ReadAsStringAsync();
        dynamic contentJson = JsonConvert.DeserializeObject(content);
        Assert.That((string)contentJson.message, Does.Contain("Hello, welcome to Otterli API!"));
    }

    [Test]
    public async Task APIVersionSupport(){
        HttpResponseMessage response = await otr_api.sendGETRequest("api_versions/");
        Assert.That(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        dynamic contentJson = JsonConvert.DeserializeObject(content);
        Assert.That(contentJson.count, Is.GreaterThan(0));
        Assert.That(contentJson.results, Is.Not.Empty);
    }

    [Test]
    public async Task AppSupport(){
        HttpResponseMessage response = await otr_api.sendGETRequest("app_support/");
        Assert.That(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        dynamic contentJson = JsonConvert.DeserializeObject(content);
        Assert.That(contentJson.count, Is.GreaterThan(0));
        Assert.That(contentJson.results, Is.Not.Empty);
    }

}