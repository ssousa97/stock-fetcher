using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

struct StockPrice {
    

    [JsonPropertyName("closPric")]
    public float Price {get; set;}

    [JsonPropertyName("prcFlcn")]
    public float PriceFluctuation {get; set;}

    [JsonPropertyName("dtTm")]
    public string Time {get; set;}
    
}
class StockFetcher {
    private string Stock;
    private float SellPrice;
    private float BuyPrice;

    public StockFetcher(string stock, float sellPrice, float buyPrice)
    {
        Stock = stock;
        SellPrice = sellPrice;
        BuyPrice = buyPrice;
    }

    public async Task Run() {
        
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        var running = true;

        while(await periodicTimer.WaitForNextTickAsync() && running){

            try {

                var lastStockPrice = await FetchStockPrice();
                SendMail(lastStockPrice);
            
            }
            catch(Exception e){
                Console.Error.WriteLine(e.Message);
                running = false;

            }           
        }
    
    }

    }


}

