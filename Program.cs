using System.Configuration;
using System.Globalization;

class Program {
    static void Main(string[] args)
    {

        try {

            if(args.Length < 3){
                throw new Exception("Faltando argumentos, padrão : <ATIVO> <VALOR DE VENDA> <VALOR DE COMPRA>");
            }

            if(args[1].Contains(',') || args[2].Contains(',')){
                throw new Exception("Valores decimais precisam ser separados por pontos.");
            }

            var stock     = args[0];
            var sellPrice = float.Parse(args[1], CultureInfo.InvariantCulture.NumberFormat);
            var buyPrice  = float.Parse(args[2], CultureInfo.InvariantCulture.NumberFormat);

            if(buyPrice > sellPrice){
                throw new Exception("Valor de compra maior do que valor de venda.");
            }

            var configs = ConfigurationManager.AppSettings;
            Console.WriteLine(configs["Key"]);
            var stockFetcher = new StockFetcher(stock, sellPrice, buyPrice);

        } catch(Exception e){
            Console.Error.WriteLine(e.Message);
        } 

    }
}




