using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using DotNetEnv;
using OtterliAPI;

namespace Feedback;

[TestFixture]
public class FeedbackTests{

    private API otrAPI;
    private string token;

    public FeedbackTests(){

        DotNetEnv.Env.TraversePath().Load();
        string token = Environment.GetEnvironmentVariable("API_TOKEN");
        this.otrAPI = new API(token);
    }

    [Test]

    public async Task invalidToken()
    {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("feedback");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }
    [Test]
    public async Task noAuthToken(){
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("feedback");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }
    [Test]
    public async Task APIVersion(){
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("feedback");
        Assert.That((int)response.StatusCode, Is.EqualTo(406));
    }


    [Test]
    public async Task feedbackEndpoint(){
        var response = await otrAPI.sendGETRequest("feedback/");
        Assert.That(response.IsSuccessStatusCode);
        ResponseContent content = await otrAPI.GetResponseContent();
        Assert.That(content.results, Is.Not.Null);
    }
    [Test]
    public async Task POSTFeedbackForm1(){
        JsonObject json = new JsonObject{
            ["useful"] = true,
            ["found_product"] = true,
            ["problem_solved"] = 1,
            ["age"] = 25,
            ["diet"] = "Vegan"
        };
        (var response, JsonObject jsonBody ) = await otrAPI.sendPOSTRequest("feedback/", json);
        Assert.That(response.IsSuccessStatusCode);
        FeedbackRecordOut feedback = JsonConvert.DeserializeObject<FeedbackRecordOut>(jsonBody.ToString());
        Assert.Pass();
    }

    [Test]
    public async Task POSTFeedbackForm2(){
        JsonObject json = new JsonObject{
            ["keep"] = "Everything you like",
            ["change"] = "Anything you want.",
            ["problem_solved"] = 5,
            ["email_address"] = "admin@admin.com"
        };
        (var response, JsonObject jsonBody ) = await otrAPI.sendPOSTRequest("feedback/", json);
        Assert.That(response.IsSuccessStatusCode);
        FeedbackRecordOut feedback = JsonConvert.DeserializeObject<FeedbackRecordOut>(jsonBody.ToString());
        Assert.Pass();
    }

    [Test]
    public async Task invalidTokenQQ()
    {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("quick_question/");
        Assert.That((int)response.StatusCode, Is.EqualTo(403));
    }
    [Test]
    public async Task noAuthTokenQQ(){
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("quick_question/");
        Assert.That((int)response.StatusCode, Is.EqualTo(403));
    }
    [Test]
    public async Task APIVersionQQ(){
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("quick_question/");
        Assert.That((int)response.StatusCode, Is.EqualTo(406));
    }

    [Test]
    public async Task QuickQuestion(){
        var quickQuestion = await otrAPI.sendGETRequest($"quick_question/");
        Assert.That(quickQuestion.IsSuccessStatusCode);
        ResponseContent content = await otrAPI.GetResponseContent();
        Assert.That(content.results, Is.Not.Null);
        if (content.results != null){
            string resultsJson = JsonConvert.SerializeObject(content.results);
            List<QuickQuestionRecord> quickQuestions = JsonConvert.DeserializeObject<List<QuickQuestionRecord>>(resultsJson);
            foreach(QuickQuestionRecord node in quickQuestions){
                if(node.Display == true){
                    Assert.That(node.DisplayFrom, Is.Not.Null);
                    Assert.That(node.DisplayTo, Is.Not.Null);
                    Assert.That(node.Question, Is.Not.Null);
                    DateTime displayFrom = DateTime.Parse(node.DisplayFrom);
                    DateTime displayTo = DateTime.Parse(node.DisplayTo);
                    DateTime now = DateTime.Now;
                    Assert.That(now, Is.GreaterThanOrEqualTo(displayFrom));
                    Assert.That(now, Is.LessThanOrEqualTo(displayTo));
                    break;
                }
            }
        }

    }
}