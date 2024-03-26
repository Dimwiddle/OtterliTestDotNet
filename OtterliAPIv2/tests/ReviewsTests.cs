using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using DotNetEnv;
using OtterliAPI;
using APIShortcuts;

namespace Reviews;

[TestFixture]
public class ReviewsTests{

    private API otrAPI;

    private ShortcutsAPI shortcutsAPI;
    private string token;

    private ProductDetailRecord productSample;
    private ReviewRecordOut reviewObj;
    private JsonObject reviewJson;

    public ReviewsTests(){

        DotNetEnv.Env.TraversePath().Load();
        string token = Environment.GetEnvironmentVariable("API_TOKEN");
        this.otrAPI = new API(token);
        this.shortcutsAPI = new ShortcutsAPI();
    }

    [Test]

    public async Task invalidToken()
    {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("reviews");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }
    [Test]
    public async Task noAuthToken(){
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("reviews");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }
    [Test]
    public async Task APIVersion(){
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("reviews");
        Assert.That((int)response.StatusCode, Is.EqualTo(406));
    }
    [OneTimeSetUp]
    public async Task Setup(){
        ProductDetailRecord productSample = await shortcutsAPI.getProductSample();
        this.productSample = productSample;
        int rating = 5;
        string review = "This is a test review";
        string username = "testuser";
        string reviewer_ref = "test_9238382575325";
        (HttpResponseMessage createResponse, JsonObject reviewJsonOut) = await otrAPI.sendPOSTRequest("ratings/", new JsonObject{
            ["product"] = productSample.Id,
            ["rating"] = rating,
            ["review"] = review,
            ["username"] = username,
            ["reviewer_ref"] = reviewer_ref
        });
        this.reviewJson = reviewJsonOut;
        Assert.That((int)createResponse.StatusCode, Is.EqualTo(201));
        ReviewRecordOut reviewObjOut = JsonConvert.DeserializeObject<ReviewRecordOut>(reviewJson.ToString());
        this.reviewObj = reviewObjOut;
        Assert.That(reviewObj.Product, Is.EqualTo(productSample.Id), "Product ID is not as expected");
        Assert.That(reviewObj.Rating, Is.EqualTo(rating), "Rating is not as expected");
        Assert.That(reviewObj.Review, Is.EqualTo(review), "Review is not as expected");
        Assert.That(reviewObj.Username, Is.EqualTo(username), "Username is not as expected");
        Assert.That(reviewObj.ReviewerRef, Does.Contain(reviewer_ref), "Reviewer Ref is not as expected");

    }

    [Test]
    public async Task reviewsSuccessFlowV1(){
        HttpResponseMessage productRatingsResponse = await otrAPI.sendGETRequest($"ratings/?product={productSample.Id}");
        Assert.That(productRatingsResponse.IsSuccessStatusCode);
        JsonObject bodyJson = await productRatingsResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.That((int)bodyJson["count"], Is.GreaterThanOrEqualTo(1));
        JsonArray results = (JsonArray)bodyJson["results"];
        Assert.That(results, Is.Not.Null);
        bool containsReviewJson = results.Any(r => r.ToString() == reviewJson.ToString());
        Assert.That(containsReviewJson, Is.True);
    }

    [Test]
    public async Task reviewsSuccessFlowV2(){
        HttpResponseMessage productRatingsResponse = await otrAPI.sendGETRequest($"products/{productSample.Id}/ratings");
        Assert.That(productRatingsResponse.IsSuccessStatusCode);
        JsonObject bodyJson = await productRatingsResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.That((int)bodyJson["count"], Is.GreaterThanOrEqualTo(1));
        JsonArray results = (JsonArray)bodyJson["results"];
        Assert.That(results, Is.Not.Null);
        bool containsReviewJson = results.Any(r => r.ToString() == reviewJson.ToString());
        Assert.That(containsReviewJson, Is.True);
    }

    [Test]
    public async Task reviewRatingValidation(){
        int rating = 2;
        string username = "testuser";
        string reviewer_ref = "test_9238382575325";
        (HttpResponseMessage createResponse, JsonObject reviewJsonOut) = await otrAPI.sendPOSTRequest("ratings/", new JsonObject{
            ["product"] = productSample.Id,
            ["rating"] = rating,
            ["username"] = username,
            ["reviewer_ref"] = reviewer_ref
        });
        Assert.That((int)createResponse.StatusCode, Is.EqualTo(400));
        
    }

    [Test]
    public async Task reviewProfanityValidation(){
        int rating = 5;
        string username = "testuser";
        string review = "this is shit.";
        string reviewer_ref = "test_9238382575325";
        (HttpResponseMessage createResponse, JsonObject reviewJsonOut) = await otrAPI.sendPOSTRequest("ratings/", new JsonObject{
            ["rating"] = rating,
            ["username"] = username,
            ["reviewer_ref"] = reviewer_ref,
            ["review"] = review,
            ["product"] = productSample.Id,
        });
        Assert.That((int)createResponse.StatusCode, Is.EqualTo(400), "Profanity was added unexpectedly.");
        
    }
}