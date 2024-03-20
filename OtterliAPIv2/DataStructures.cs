using Newtonsoft.Json;

public record ProductRecord (
    [JsonProperty("id")] string Id,
    [JsonProperty("name")] string Name,
    [JsonProperty("vegan")] bool? Vegan,
    [JsonProperty("active")] bool Active,
    [JsonProperty("categories")] List<CategoryRecord> Categories,
    [JsonProperty("countries")] List<CountryRecord> Countries,
    [JsonProperty("vendors")] List<ProductVendorRecord> Vendors,
    [JsonProperty("date_added")] string DateAdded,
    [JsonProperty("last_updated_date")] string LastUpdatedDate,
    [JsonProperty("reviews")] ProductReviewRecord Reviews
);

public record ProductDetailRecord (
    [JsonProperty("id")] string Id,
    [JsonProperty("name")] string Name,
    [JsonProperty("vegan")] bool Vegan,
    [JsonProperty("active")] bool Active,
    [JsonProperty("categories")] List<string> Categories,
    [JsonProperty("vendors")] List<ProductVendorRecord> Vendors,
    [JsonProperty("reviews")] ProductReviewRecord Reviews,
    [JsonProperty("notes")] string Description,
    [JsonProperty("image_url")] string Image,
    [JsonProperty("gtin")] string? Gtin,
    [JsonProperty("unit")] string? Unit,
    [JsonProperty("price")] string? Price,
    [JsonProperty("last_updated_date")] string LastUpdatedDate,
    [JsonProperty("date_added")] string DateAdded
);

public record ProductVendorRecord (
    [JsonProperty("name")] string Name,
    [JsonProperty("logo")] string? Logo,
    [JsonProperty("logo_svg")] string? LogoSvg,
    [JsonProperty("price")] double Price,
    [JsonProperty("country")] string Country,
    [JsonProperty("currency")] string Currency,
    [JsonProperty("affiliate_url")] string AffiliateUrl,
    [JsonProperty("date_added")] string DateAdded,
    [JsonProperty("last_updated_date")] string LastUpdatedDate
);

public record CountryRecord (
    [JsonProperty("code")] string CountryCode
);

public record ProductReviewRecord(
    [JsonProperty("avg_rating")] double? AvgRating,
    [JsonProperty("rating_count")] int? ReviewCount
);

public record CategoryRecord (
    [JsonProperty("name")] string Name,
    [JsonProperty("otterli_id")] string Id,
    [JsonProperty("icon_svg")] string? IconSvg
);

public record VendorRecord (
    [JsonProperty("id")] string Id,
    [JsonProperty("name")] string Name,
    [JsonProperty("logo")] string? Logo,
    [JsonProperty("logo_svg")] string? LogoSvg,
    [JsonProperty("country")] string Country
);