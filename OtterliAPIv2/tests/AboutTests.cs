
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DotNetEnv;
using OtterliAPI;

namespace About;

[TestFixture]
public class AboutTests{

    private API otrAPI;
    private string token;

    public AboutTests(){

        DotNetEnv.Env.TraversePath().Load();
        string token = Environment.GetEnvironmentVariable("API_TOKEN");
        this.otrAPI = new API(token);
    }

     [Test]
    public async Task invalidToken()
    {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("about");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Testing the GET /about endpoint's authentication security without a token being provided.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task noAuthToken(){
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("about");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Testing the GET /about endpoint's versioning.
    /// </summary>
    [Test]
    public async Task APIVersion(){
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("about");
        Assert.That((int)response.StatusCode, Is.EqualTo(406));
    }

    [Test]
    public async Task aboutEndpoint(){
        var response = await otrAPI.sendGETRequest("about/");
        Assert.That(response.IsSuccessStatusCode);
        ResponseContent content = await otrAPI.GetResponseContent();
        Assert.That(content.count, Is.GreaterThan(0));
        Assert.That(content.results, Is.Not.Null);
        Assert.That(content.results, Is.Not.Empty);
        foreach (var record in content.results){
            Assert.That(record, Is.Not.Null);
            Assert.That(record, Is.Not.Empty);
            Assert.That(record, Does.ContainKey("title"));
            Assert.That(record, Does.ContainKey("body"));
            Assert.That(record.title, Is.Not.Null);
            Assert.That(record.body, Is.Not.Null);

        }
    }

    [Test]
    public async Task FAQEndpoint(){
        var response = await otrAPI.sendGETRequest("faq/");
        Assert.That(response.IsSuccessStatusCode);
        ResponseContent content = await otrAPI.GetResponseContent();
        Assert.That(content.count, Is.GreaterThan(0));
        Assert.That(content.results, Is.Not.Null);
        Assert.That(content.results, Is.Not.Empty);
        foreach (var record in content.results){
            Assert.That(record, Is.Not.Null);
            Assert.That(record, Is.Not.Empty);
            Assert.That(record, Does.ContainKey("question"));
            Assert.That(record, Does.ContainKey("answer"));
            Assert.That(record.question, Is.Not.Null);
            Assert.That(record.answer, Is.Not.Null);

        }
    }

    [Test]
    public async Task SummaryStats(){
        JsonObject content = await otrAPI.getSummaryStats();
        Assert.That(content, Is.Not.Null);
        List<string> expectedFields = ["categories", "products", "vendors", "reviews"];
        foreach (var field in expectedFields){
            Assert.That(content, Does.ContainKey(field));
            Assert.That(content[field].AsValue(), Is.Not.Null);
        }

    }
}