using OtterliAPI;
using APIShortcuts;
using System.Reflection;
using Newtonsoft.Json;
using System.Security.Principal;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Products;

[TestFixture]
public class ProductResponse {
    private API otrAPI;
    private string token;
    private dynamic body;
    private dynamic productSample;
    private HttpResponseMessage response;

    [OneTimeSetUp]
    public async Task Setup() {
        DotNetEnv.Env.TraversePath().Load();
        this.token = Environment.GetEnvironmentVariable("API_TOKEN");
        otrAPI = new API(this.token);
        this.response = await otrAPI.sendGETRequest("GET", "products");
        Assert.IsTrue(response.IsSuccessStatusCode);
        this.body = await otrAPI.GetResponseContent();
        this.productSample = body.results[0];

        
    }
    [Test]
    public async Task invalidToken() {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("GET", "products");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }
    [Test]
    public async Task noAuthToken() {
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("GET", "products");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }
    [Test]
    public async Task APIVersion() {
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("GET", "products");
        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }
    [Test]
    public void ProductResponseBody() {
        Assert.That(body.count, Is.InstanceOf<int>());
        Assert.That(body.count, Is.GreaterThan(0));
        Assert.That(body.previous, Is.Null);
        Assert.That(body.next, Is.Not.Null);
        Assert.That(body.results, Is.Not.Null);
        Assert.AreEqual(body.results.Count, 25);
    }

    [Test]
    public async Task ProductPagination(){
        if (body.count > 25){
            Assert.That(body.next, Is.Not.Null);
            Assert.That(body.results.Count, Is.EqualTo(25));
        }
        else{
            Assert.That(body.next, Is.Null);
        }
        Assert.That(body.previous, Is.Null);
        string page2 = body.next.Replace(otrAPI.host + "/", "");
        var page2Response = await otrAPI.sendGETRequest("GET", $"{page2}");
        Assert.That(page2Response.IsSuccessStatusCode, Is.True);
        var page2body = await otrAPI.GetResponseContent();
        string expectedPrevious = otrAPI.host + "/products/";
        Assert.That(page2body.previous, Is.Not.Null);
        Assert.That(page2body.count, Is.InstanceOf<int>());
        Assert.That(page2body.previous, Is.EqualTo(expectedPrevious));
        Assert.That(page2body.results, Is.Not.Null);

    }

    [Test]
    public void ProductItems()
    {   
        foreach (var product in body.results)
        {
            foreach (var field in typeof(ProductRecord).GetProperties())
            
            {
                var jsonField = DataFunctions.camelToSnakeCase(field.Name);
                Assert.That(product.ContainsKey(jsonField));
                Assert.That(product[jsonField], Is.Not.Null);
            }
        }
    }
}

[TestFixture]
public class ProductSearch {
    private API otrAPI;
    private string token;
    private ShortcutsAPI shortcutApi = new ShortcutsAPI();

    [OneTimeSetUp]
    public void Setup() {
        DotNetEnv.Env.TraversePath().Load();
        this.token = Environment.GetEnvironmentVariable("API_TOKEN");
        otrAPI = new API(this.token);
    }

    [Test]
    public async Task SearchProductRanking() {
        string search = "shampoo";
        var response = await otrAPI.sendGETRequest("GET", $"products?search={search}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var body = await otrAPI.GetResponseContent();
        Assert.That(body.count, Is.GreaterThan(0));
        var productSample = body.results[0];
        Assert.That(productSample.name.ToLower().Contains(search));
    }

    [Test]
    public async Task SearchProductNotFound() {
        string search = "thisproductdoesnotexist";
        var response = await otrAPI.sendGETRequest("GET", $"products?search={search}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var body = await otrAPI.GetResponseContent();
        Assert.That(body.count, Is.EqualTo(0));
    }

    [Test]
    public async Task SearchProductbyGTIN() {
        ProductDetailRecord product = await shortcutApi.getProductSample();
        var response = await otrAPI.sendGETRequest("GET", $"products?search={product.Gtin}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var body = await otrAPI.GetResponseContent();
        Assert.That(body.count, Is.EqualTo(1));
        string foundProduct = body.results[0].id;
        Assert.That(foundProduct, Is.EqualTo(product.Id));
    }

}

[TestFixture]
public class ProductQueryParams {

    private API otrAPI;

    private ShortcutsAPI shortcutsAPI;

    [OneTimeSetUp]
    public void Setup(){
        DotNetEnv.Env.TraversePath().Load();
        string token = Environment.GetEnvironmentVariable("API_TOKEN");
        otrAPI = new API(token);
        shortcutsAPI = new ShortcutsAPI();
    }

    [Test]
    public async Task ByCountryCode(){
        Dictionary<string, string> parameters = new Dictionary<string, string>{{"country_code", "GB"}};
        ResponseContent responseContent = await shortcutsAPI.getProducts(parameters);
        List<ProductRecord> products = shortcutsAPI.serializeProductList(responseContent.results);
        CountryRecord expectedCountryCode = new CountryRecord("GB");
        while (responseContent.next != null){
            
            foreach (var product in products){
                Assert.That(product.Countries, Does.Contain(expectedCountryCode));
            }
            string nextURL = responseContent.next.Replace(otrAPI.host + "/", "");
            await otrAPI.sendGETRequest("GET", nextURL);
            responseContent = await otrAPI.GetResponseContent();
            products = shortcutsAPI.serializeProductList(responseContent.results);
        }
        
    }
    

}