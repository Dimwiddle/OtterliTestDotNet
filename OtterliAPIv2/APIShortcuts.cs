using System.Runtime.InteropServices;
using OtterliAPI;
using Newtonsoft.Json;
using System.ComponentModel;

namespace APIShortcuts;

public class ShortcutsAPI {
    private API otr_api;
    private string token;


    public ShortcutsAPI() {
        DotNetEnv.Env.TraversePath().Load();
        this.token = Environment.GetEnvironmentVariable("API_TOKEN");
        this.otr_api = new API(this.token);
    }

    /// <summary>
    /// Rreturns a list of all products by the given search criteria.
    /// </summary>
    /// <param name="search"></param>
    /// <returns>HttpResponse</returns>
    public async Task<ResponseContent> searchProducts(string search) {
        var response = await this.otr_api.sendGETRequest($"products?search={search}");
        return await this.otr_api.GetResponseContent();
    }

    /// <summary>
    /// Returns a list of vendors with available products to be searched.
    /// </summary>
    public async Task<ResponseContent> getActiveVendors() {
        var response = await this.otr_api.sendGETRequest("vendors", new Dictionary<string, string> {{"active", "true"}});
        return await this.otr_api.GetResponseContent();
    }

    /// <summary>
    ///  Returns a product for sample for testing purposes.
    /// </summary>
    /// <returns>object</returns>
    public async Task<ProductDetailRecord> getProductSample(){
        var response = await this.otr_api.sendGETRequest("products/");
        var body = await this.otr_api.GetResponseContent();
        var product = body.results[0];
        string productId =  product.id;
        HttpResponseMessage productResponse = await this.otr_api.sendGETRequest($"products/{productId}/");
        string productdDetail = await productResponse.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ProductDetailRecord>(productdDetail);
    }

    /// <summary>
    /// Returns a list of products from the vegan cube, for the given query parameters.
    /// </summary>
    /// <param name="parameters"></param>
    public async Task<ResponseContent> getProducts(Dictionary<string, string> parameters = null){
        // add a method to get all products from the given parameters
        var response = await otr_api.sendGETRequest("products/", parameters);
        Assert.That(response.IsSuccessStatusCode, "GET products - Response was not successful");
        return await otr_api.GetResponseContent();
    }

    /// <summary>
    /// Serialize the response results in to a ProductRecord
    /// </summary>
    /// <param name="responseResults"></param>
    public List<ProductRecord> serializeProductList(List<object> responseResults){
        string json = JsonConvert.SerializeObject(responseResults);
        List<ProductRecord> products = JsonConvert.DeserializeObject<List<ProductRecord>>(json);
        return products;
    }

    ///summary
    /// Serialize in to CategoryRecord
    /// </summary>
    /// <param name="responseResults"></param>
    /// <returns>List<CategoryRecord></returns>
    public async Task<List<CategoryRecord>> getCategories(Dictionary<string, string> parameters = null){
        var response = await otr_api.sendGETRequest("categories/", parameters);
        Assert.That(response.IsSuccessStatusCode, "GET products - Response was not successful");
        ResponseContent responseContent = await otr_api.GetResponseContent();
        string json = JsonConvert.SerializeObject(responseContent.results);
        List<CategoryRecord> categories = JsonConvert.DeserializeObject<List<CategoryRecord>>(json);
        return categories;
    }

    /// <summary>
    /// Send the request for the product details 
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>(HttpResponseMessage, ProductDetailRecord)</returns>
    public async Task<(HttpResponseMessage, ProductDetailRecord)> getProductByID (string productId, int version){
        string endpoint;
        if (version == 1){
            endpoint = $"products/{productId}/";
        }
        else {
            endpoint = $"product_details/{productId}/";
        }
        var response = await otr_api.sendGETRequest(endpoint);
        string json = await response.Content.ReadAsStringAsync();
        ProductDetailRecord productDetail = JsonConvert.DeserializeObject<ProductDetailRecord>(json);
        return (response, productDetail);
    } 
   
}   