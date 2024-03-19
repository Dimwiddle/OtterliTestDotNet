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
public class ProductFilterParams {

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

    [Test]
    public async Task ByCategory(){
        ProductDetailRecord productSample = await shortcutsAPI.getProductSample();
        CategoryRecord expectedCategory = productSample.Categories.First();
        Dictionary<string, string> parameters = new Dictionary<string, string>{{"category", expectedCategory.Id}};
        ResponseContent responseContent = await shortcutsAPI.getProducts(parameters);
        List<ProductRecord> products = shortcutsAPI.serializeProductList(responseContent.results);

        while (responseContent.next != null){
            foreach (ProductRecord product in products){
                Assert.That(product.Categories.Contains(expectedCategory));
            }
            string nextURL = responseContent.next.Replace(otrAPI.host + "/", "");
            await otrAPI.sendGETRequest("GET", nextURL);
            responseContent = await otrAPI.GetResponseContent();
            products = shortcutsAPI.serializeProductList(responseContent.results);
        }
    }

    [Test]
    public async Task ByAvailability(){
            Dictionary<string, string> parameters = new Dictionary<string, string>{{"available", "false"}};
            ResponseContent responseContent = await shortcutsAPI.getProducts(parameters);
            List<ProductRecord> products = shortcutsAPI.serializeProductList(responseContent.results);
            while (responseContent.next != null){
                foreach (ProductRecord product in products){
                    Assert.That(product.Active, Is.False);
                }
            string nextURL = responseContent.next.Replace(otrAPI.host + "/", "");
            await otrAPI.sendGETRequest("GET", nextURL);
            responseContent = await otrAPI.GetResponseContent();
            products = shortcutsAPI.serializeProductList(responseContent.results);
        }
    }

    [Test]
    public async Task ByVendor(){
        ResponseContent response = await shortcutsAPI.getProducts();
        ProductRecord productSample = shortcutsAPI.serializeProductList(response.results).First();
        ProductVendorRecord vendor = productSample.Vendors.First();
        Dictionary<string, string> parameters = new Dictionary<string, string>{{"vendor", vendor.Name}};
        ResponseContent responseContent = await shortcutsAPI.getProducts(parameters);
        List<ProductRecord> products = shortcutsAPI.serializeProductList(responseContent.results);
        while (responseContent.next != null){
            foreach (ProductRecord product in products){
                Assert.That(product.Vendors.Contains(vendor));
            }
            string nextURL = responseContent.next.Replace(otrAPI.host + "/", "");
            await otrAPI.sendGETRequest("GET", nextURL);
            responseContent = await otrAPI.GetResponseContent();
            products = shortcutsAPI.serializeProductList(responseContent.results);
        }
    }
    
    [Test]
    public async Task BySelectProducts(){
        ResponseContent response = await shortcutsAPI.getProducts();
        List<ProductRecord> products = shortcutsAPI.serializeProductList(response.results);
        ProductRecord productSample1 = products.First();
        ProductRecord productSample2 = products.Last();
        Dictionary<string, string> parameters = new Dictionary<string, string>{{"select", $"{productSample1.Id},{productSample2.Id}"}};
        ResponseContent responseContent = await shortcutsAPI.getProducts(parameters);
        List<ProductRecord> selectedProducts = shortcutsAPI.serializeProductList(responseContent.results);
        Assert.That(selectedProducts.Count, Is.EqualTo(2));
        List<string> actualSelectedIds = selectedProducts.Select(p => p.Id).ToList();
        List<string> expectedSelectedIds = new List<string>{productSample1.Id, productSample2.Id};
        Assert.That(actualSelectedIds, Is.EquivalentTo(expectedSelectedIds));
    }

}

[TestFixture]
public class ProductOrdering {

    private API otrAPI;

    private ShortcutsAPI shortcutsAPI = new ShortcutsAPI();

    public ProductOrdering(){
        DotNetEnv.Env.TraversePath().Load();
        string token = Environment.GetEnvironmentVariable("API_TOKEN");
        this.otrAPI = new API(token);
    }

    [Test]
    public async Task ByDate(){
        List<string> dateOrder = new List<string>{"-date_added", "date_added"};
        List<(string, bool)> testResults = new List<(string, bool)>();
        bool isAscending = false;
        bool isDescending = false;
        foreach (string order in dateOrder){
            Dictionary<string, string> parameters = new Dictionary<string, string>{{"ordering", order}};
            ResponseContent response = await shortcutsAPI.getProducts(parameters);
            List<ProductRecord> products = shortcutsAPI.serializeProductList(response.results);
            List<DateTime> dates = products.Select(p => DateTime.Parse(p.DateAdded)).ToList();
            if (order == "-date_added"){
                isDescending = dates.Zip(dates.Skip(1), (a, b) => a.CompareTo(b) >= 0).All(x => x);
            } else {
                isAscending = dates.Zip(dates.Skip(1), (a, b) => a.CompareTo(b) <= 0).All(x => x);
            }
        }
        Assert.That(isAscending);
        Assert.That(isDescending);
    }

    [Test]
    public async Task ByRating(){
        List<string> ratingOrder = new List<string>{"-rating", "rating"};
        List<(string, bool)> testResults = new List<(string, bool)>();
        bool isAscending = false;
        bool isDescending = false;
        foreach (string order in ratingOrder){
            Dictionary<string, string> parameters = new Dictionary<string, string>{{"ordering", order}};
            ResponseContent response = await shortcutsAPI.getProducts(parameters);
            List<ProductRecord> products = shortcutsAPI.serializeProductList(response.results);
            List<double?> ratings = products.Select(p => p.Reviews.AvgRating).ToList();
            if (order == "-rating"){
                isDescending = ratings.Zip(ratings.Skip(1), (a, b) => (a?.CompareTo(b) ?? 1) >= 0).All(x => x);
            } else {
                isAscending = ratings.Zip(ratings.Skip(1), (a, b) => (a?.CompareTo(b) ?? -1) <= 0).All(x => x);
            }
        }
        Assert.That(isAscending);
        Assert.That(isDescending);
    }

}