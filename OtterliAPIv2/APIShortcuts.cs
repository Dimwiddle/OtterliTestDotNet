using System.Runtime.InteropServices;
using OtterliAPI;
using Newtonsoft.Json;

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
        var response = await this.otr_api.sendGETRequest("GET", $"products?search={search}");
        return await this.otr_api.GetResponseContent();
    }

    /// <summary>
    /// Returns a list of vendors with available products to be searched.
    /// </summary>
    public async Task<ResponseContent> getActiveVendors() {
        var response = await this.otr_api.sendGETRequest("GET", "vendors", new Dictionary<string, string> {{"active", "true"}});
        return await this.otr_api.GetResponseContent();
    }

    /// <summary>
    ///  Returns a product for sample for testing purposes.
    /// </summary>
    /// <returns>object</returns>
    public async Task<ProductDetailRecord> getProductSample(){
        var response = await this.otr_api.sendGETRequest("GET", "products");
        var body = await this.otr_api.GetResponseContent();
        var product = body.results[0];
        var productResponse = await this.otr_api.sendGETRequest("GET", $"products/{product.id}");
        string productdDetail = await productResponse.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ProductDetailRecord>(productdDetail);
    }

    /// <summary>
    /// Returns a list of products from the vegan cube, for the given query parameters.
    /// </summary>
    /// <param name="parameters"></param>
    public async Task<ResponseContent> getProducts(Dictionary<string, string> parameters = null){
        await otr_api.sendGETRequest("GET", "products/", parameters);
        return await otr_api.GetResponseContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseResults"></param>
    public List<ProductRecord> serializeProductList(List<object> responseResults){
        string json = JsonConvert.SerializeObject(responseResults);
        List<ProductRecord> products = JsonConvert.DeserializeObject<List<ProductRecord>>(json);
        return products;
    }
   
}   