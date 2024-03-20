using OtterliAPI;
using DotNetEnv;
using APIShortcuts;
using System.Diagnostics.CodeAnalysis;
namespace Categories;



[TestFixture]
public class CategoryTests
{

    
    private API otr_api;
    private string token;
    private dynamic body;

    private dynamic categorySample;

    private HttpResponseMessage response;

    private ShortcutsAPI shortcutsAPI = new ShortcutsAPI();

    [OneTimeSetUp]
    public async Task Setup()
    {
        DotNetEnv.Env.TraversePath().Load();
        this.token = Environment.GetEnvironmentVariable("API_TOKEN");
        otr_api = new API(this.token);
        this.response = await otr_api.sendGETRequest("categories");
        this.body = await otr_api.GetResponseContent();
        this.categorySample = body.results[0];
    }

    /// <summary>
    /// Testing the GET /categories endpoint's authentication security with an invalidToken.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task invalidToken()
    {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("categories");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Testing the GET /categories endpoint's authentication security without a token being provided.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task noAuthToken(){
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("categories");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Testing the GET /categories endpoint's versioning.
    /// </summary>
    [Test]
    public async Task APIVersion(){
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("categories");
        Assert.That((int)response.StatusCode, Is.EqualTo(406));
    }

    /// <summary>
    /// Testing the GET /categories endpoint and checking the response as expected.
    /// The content of the response is also checked to ensure it is a JSON object with correct fields.
    /// </summary>
    [Test]
    public void CategoriesResponseBody()
    {
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.That(body.count, Is.InstanceOf<int>());
        Assert.That(body.count, Is.GreaterThan(0));
        Assert.That(body.previous, Is.Null);
        Assert.That(body.next, Is.Not.Null);
        Assert.That(body.results, Is.Not.Null);
        Assert.AreEqual(body.results.Count, 25);
        
    }
    /// <summary>
    /// Checking the response of the GET /categories endpoint to ensure the category items are as expected.
    /// </summary>
    [Test]
    public void CategoryItems(){
        Assert.That(categorySample.ContainsKey("name"));
        Assert.That(categorySample.name, Is.Not.Null);
        Assert.That(categorySample.ContainsKey("id"));
        Assert.That(categorySample.id, Is.Not.Null);
        Assert.That(categorySample.ContainsKey("icon_svg"));
    }

    /// <summary>
    /// Testing the GET /categories endpoint with the pagination query parameter.
    /// </summary>
    [Test]
    public async Task CategoryPagination(){
        string nextPage = body.next.Replace(otr_api.host + "/", "");
        var page2response = await otr_api.sendGETRequest(nextPage);
        var page2body = await otr_api.GetResponseContent();
        string expectedPrevious = otr_api.host + "/categories/";
        Assert.IsTrue(page2response.IsSuccessStatusCode);
        Assert.That(page2body.count, Is.InstanceOf<int>());
        Assert.That(page2body.previous, Is.EqualTo(expectedPrevious));
        Assert.That(page2body.results, Is.Not.Null);
        int itemsRemaining = body.count - body.results.Count;
        Assert.That(page2body.results.Count, Is.EqualTo(itemsRemaining));
    }

    /// <summary>
    /// Testing the GET /categories endpoint with the icons query parameter.
    /// </summary>
    [Test]
    public async Task CategoryIcons(){
        List<CategoryRecord> categories = await shortcutsAPI.getCategories(new Dictionary<string, string> {{"icons", "true"}});
        foreach(CategoryRecord category in categories){
            Assert.That(category.IconSvg, Does.Contain(".svg"), "'.svg' was not found");
        }
    }

}
