using System.Text.Json.Serialization;

struct StockPrice {
    

    [JsonPropertyName("closPric")]
    public float Price {get; set;}

    [JsonPropertyName("prcFlcn")]
    public float PriceFluctuation {get; set;}

    [JsonPropertyName("dtTm")]
    public string Time {get; set;}
    
}