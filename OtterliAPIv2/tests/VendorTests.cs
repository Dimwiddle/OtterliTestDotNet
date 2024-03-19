using OtterliAPI;

namespace Vendors;

[TestFixture]
public class VendorTests{
    
    private API otr_api;

    private string token;

    private dynamic body;

    private dynamic vendorSample;

    private HttpResponseMessage response;

    [OneTimeSetUp]
    public async Task Setup()
    {
        DotNetEnv.Env.TraversePath().Load();
        this.token = Environment.GetEnvironmentVariable("API_TOKEN");
        otr_api = new API(this.token);
        this.response = await otr_api.sendGETRequest("GET", "vendors");
        this.body = await otr_api.GetResponseContent();
        this.vendorSample = body.results[0];
    }

    /// <summary>
    /// Testing the GET /categories endpoint's authentication security with an invalidToken.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task invalidToken()
    {
        API temp_client = new API("thisisfake");
        var response = await temp_client.sendGETRequest("GET", "vendors");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Testing the GET /categories endpoint's authentication security without a token being provided.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task noAuthToken(){
        API temp_client = new API();
        var response = await temp_client.sendGETRequest("GET", "vendors");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    /// <summary>
    /// Testing the GET /categories endpoint's versioning.
    /// </summary>
    [Test]
    public async Task APIVersion(){
        API temp_client = new API(token, version: "0.1");
        var response = await temp_client.sendGETRequest("GET", "vendors");
        Assert.That((int)response.StatusCode, Is.EqualTo(406));
    }

    /// <summary>
    /// Testing the GET /vendors endpoint and checking the response contains the correct fields.
    /// </summary>
    [Test]
    public void VendorsResponseBody(){
        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(body.results, Is.Not.Null);
        Assert.That(body.results, Is.Not.Empty);
        Assert.That(body.count, Is.GreaterThan(0));
    }

    /// <summary>
    /// Testing the pagination fields of the GET /vendors endpoint response.
    /// </summary>
    [Test]
    public void VendorsPagination(){
        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        if (body.count > 25){
            Assert.That(body.next, Is.Not.Null);
            Assert.That(body.results.Count, Is.EqualTo(25));
        }
        else{
            Assert.That(body.next, Is.Null);
        }
        Assert.That(body.previous, Is.Null);
    }

    /// <summary>
    /// Testing the GET /vendors endpoint and checking the Vendor Items are as expected.
    /// </summary>
    [Test]
    public void VendorItems(){
        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(vendorSample.ContainsKey("id"));
        Assert.That(vendorSample.ContainsKey("name"));
        Assert.That(vendorSample.ContainsKey("logo"));
        Assert.That(vendorSample.ContainsKey("logo_svg"));
        Assert.That(vendorSample.ContainsKey("country"));

        foreach(var vendor in body.results){
            Assert.That(vendor.id, Is.Not.Null);
            // string id = vendor.id;
            Assert.That((string)vendor.id, Does.Contain("VEN"));
            Assert.That(vendor.name, Is.Not.Null);
            Assert.That(vendor.logo, Is.Not.Null);
            if (vendor.logo != null){
                Assert.That((string)vendor.logo, Does.Contain("https"));
                Assert.That((string)vendor.logo, Does.Contain(".png").Or.Contains(".jpg").Or.Contains(".jpeg"));
            }
            if (vendor.logo_svg != null){
                Assert.That((string)vendor.logo_svg, Does.Contain(".svg"));
                Assert.That((string)vendor.logo_svg, Does.Contain("https"));
            }
        }
        
    }
    [Ignore("This test is not yet implemented")]
    public async Task ActiveVendorItems(){
        // Get products from /products endpoint
        // aggregate unique vendor ids
        // get active vendors from /vendors endpoint
        // check that all vendor ids from products are in the active vendors
        var activeVendorsResponse = await otr_api.sendGETRequest("GET", "vendors", new Dictionary<string, string> {{"active", "true"}});
    }
}