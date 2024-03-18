
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
        otr_api = new API();
    }

    [Test]
    public async Task APIHealth()
    {
        HttpResponseMessage response = await otr_api.APIHealthCheck();
        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        var content = await response.Content.ReadAsStringAsync();
        dynamic contentJson = JsonConvert.DeserializeObject(content);
        Assert.That((string)contentJson.message, Does.Contain("Hello, welcome to Otterli API!"));
    }

}