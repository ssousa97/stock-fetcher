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
    public string Stock {get; set;}
    public float SellPrice {get; set;}
    public float BuyPrice {get; set;}

    public StockFetcher(string stock, float sellPrice, float buyPrice) {
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

    public async Task<StockPrice> FetchStockPrice() {

        using(var httpClient = new HttpClient()){

            var response = await httpClient.GetAsync($"https://cotacao.b3.com.br/mds/api/v1/DailyFluctuationHistory/{Stock}");
            
            if(!response.IsSuccessStatusCode) {
                throw new Exception($"Erro ao conectar-se com a API. Código : ${response.StatusCode}");
            }

            var responseJson = JsonArray.Parse(await response.Content.ReadAsStreamAsync());
            var stockPricesJsonArray = responseJson?["TradgFlr"]?["scty"]?["lstQtn"]?.AsArray();
            var stockPrices = stockPricesJsonArray.Deserialize<List<StockPrice>>();

            if(stockPrices is null || stockPrices.Count == 0 ){
                throw new Exception("Nenhum preço de ativo encontrado.");
            }

            return stockPrices.Last();
        }

    }

    public void SendMail(StockPrice lastStockPrice){
        if(lastStockPrice.Price > SellPrice) {
            
            // mailSender.SendSellEmail();
        
        } else if(lastStockPrice.Price < BuyPrice) {
            
            // mailSender.SendBuyEmail();
        }
    }


}

