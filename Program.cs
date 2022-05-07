using System.Configuration;
using System.Globalization;

class Program {
    static void Main(string[] args){
       
        if(args.Length < 3){
            Console.WriteLine("Faltando argumentos, padrão : <ATIVO> <VALOR DE VENDA> <VALOR DE COMPRA>");
            return;
        }

        if(args[1].Contains(',') || args[2].Contains(',')){
            Console.WriteLine("Valores decimais precisam ser separados por pontos.");
            return;
        }

        var stock     = args[0];
        var sellPrice = float.Parse(args[1], CultureInfo.InvariantCulture);
        var buyPrice  = float.Parse(args[2], CultureInfo.InvariantCulture);

        if(buyPrice > sellPrice){
            Console.WriteLine("Valor de compra maior do que valor de venda.");
            return;
        }

        

    }
}




