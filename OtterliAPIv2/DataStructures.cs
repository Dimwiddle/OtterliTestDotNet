using Newtonsoft.Json;

public record ProductRecord (
    [JsonProperty("id")] string Id,
    [JsonProperty("name")] string Name,
    [JsonProperty("vegan")] bool Vegan,
    [JsonProperty("active")] bool Active,
    [JsonProperty("categories")] List<object> Categories,
    [JsonProperty("countries")] List<object> Countries,
    [JsonProperty("vendors")] List<object> Vendors,
    [JsonProperty("date_added")] string DateAdded,
    [JsonProperty("last_updated_date")] string LastUpdatedDate,
    [JsonProperty("reviews")] object Reviews
);

public record ProductDetailRecord (
    [JsonProperty("id")] string Id,
    [JsonProperty("name")] string Name,
    [JsonProperty("vegan")] bool Vegan,
    [JsonProperty("active")] bool Active,
    [JsonProperty("categories")] object Categories,
    [JsonProperty("countries")] object Countries,
    [JsonProperty("vendors")] List<ProductVendorRecord> Vendors,
    [JsonProperty("reviews")] object Reviews,
    [JsonProperty("description")] string Description,
    [JsonProperty("image")] string Image,
    [JsonProperty("gtin")] string Gtin,
    [JsonProperty("unit")] string Unit,
    [JsonProperty("price")] string Price,
    [JsonProperty("last_updated_date")] string LastUpdatedDate,
    [JsonProperty("date_added")] string DateAdded
);

public record ProductVendorRecord (
    [JsonProperty("name")] string Name,
    [JsonProperty("logo")] string Logo,
    [JsonProperty("logo_svg")] string LogoSvg,
    [JsonProperty("price")] double Price,
    [JsonProperty("country")] string Country,
    [JsonProperty("currency")] string Currency,
    [JsonProperty("affiliate_url")] string AffiliateUrl,
    [JsonProperty("date_added")] string DateAdded,
    [JsonProperty("last_updated_date")] string LastUpdatedDate
);

public enum CategoryRecord {
    id,
    name,
    icon_svg,
    date_added,
    last_updated_date
}

public enum VendorEnum {
    id,
    name,
    logo,
    logo_svg,
    country,
    date_added,
    last_updated_date
}