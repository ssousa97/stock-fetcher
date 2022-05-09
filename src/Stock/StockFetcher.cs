using System.Text.Json;
using System.Text.Json.Nodes;


class StockFetcher {
    public string Stock {get; set;}
    public float SellPrice {get; set;}
    public float BuyPrice {get; set;}
    public float LastPrice {get; set;}
    private readonly IMailService MailService;

    public StockFetcher(string stock, float sellPrice, float buyPrice, IMailService mailService) {
        Stock = stock;
        SellPrice = sellPrice;
        BuyPrice = buyPrice;
        MailService = mailService;
        LastPrice = 0f;
    }

    public async Task Run() {
        
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(5)); 
        var running = true;

        while(await periodicTimer.WaitForNextTickAsync() && running){
            Console.WriteLine("\nBusca de preços iniciada...");
            try {

                var stockPrice = await FetchStockPrice();
                SendMail(stockPrice);
            
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

    public void SendMail(StockPrice stockPrice){
        
        if(stockPrice.Price != LastPrice){

            LastPrice = stockPrice.Price;
            Console.WriteLine($"-> Preço atual : {stockPrice.Price}");
            
            if(stockPrice.Price > SellPrice) {
            
                MailService.SendMail(Stock, stockPrice.Price, true);

            } else if(stockPrice.Price < BuyPrice) {
            
                MailService.SendMail(Stock, stockPrice.Price, false);
            }

        }else {

            Console.WriteLine("-> Nenhuma atualização recente desde a ultima busca. Nenhum email enviado.");
        
        }
        
    }


}

