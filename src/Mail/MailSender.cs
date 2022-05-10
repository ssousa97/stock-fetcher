using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;


struct User {
    public string Name  {get; set;}
    public string Email {get;set;}
}

struct MailStatus {
    public string Code {get; set;}
    public User User {get; set;}
    public bool Sent {get; set;}
}

class MailSender : IMailService {

    private string SMTPServer;
    private string FromMail;
    private string FromName;
    private string Password;
    private List<User> UserList;
    private List<string> FailedMails;

    private readonly IConfiguration ConfigurationService;

    public MailSender(){

        ConfigurationService = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.json")
            .Build();

        var smtpConfig = ConfigurationService.GetSection("smtpConfig");
        var emailsConfig = ConfigurationService.GetSection("emails");

        SMTPServer     = smtpConfig.GetSection("server").Value;
        FromMail       = smtpConfig.GetSection("frommail").Value;
        FromName       = smtpConfig.GetSection("fromname").Value;
        Password       = smtpConfig.GetSection("password").Value;

        UserList       = emailsConfig.Get<List<User>>();
        FailedMails    = new List<string>();

    }

    public async Task SendMail(string stock, bool sell) {
        
        Console.WriteLine("-> Informando usuários sobre alteração de preço...");

        int successSents = await CreateAndSendMail(stock, sell);
    
        if(successSents == UserList.Count){

            Console.WriteLine("-> Todos os usuários notificados sobre a ultima atualização de preço.");

        } else if (successSents < UserList.Count && successSents > 0){

            Console.WriteLine($"-> Alguns usuários não foram notificados sobre a atualização de preço. Envios sucedidos : {successSents}/{UserList.Count} ");
            LogFailedMails();

        } else {

            Console.WriteLine("-> Nenhum usuário notificado pela atualização de preço.");
            LogFailedMails();

        }
        
    }

    private async Task<int> CreateAndSendMail(string stock, bool sell){
        var successSents = 0;

        using(var smtpClient = new SmtpClient()){
            
            smtpClient.Connect(SMTPServer, 465, true);
            smtpClient.Authenticate(FromMail, Password);

            FailedMails.Clear();

            foreach(var user in UserList){
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromMail));
                message.To.Add(new MailboxAddress(user.Name, user.Email));

                message.Subject = sell ? 
                    $"Sr. {user.Name}, venda seu ativo {stock}" :
                    $"Sr. {user.Name}, compre o ativo {stock}";
                
                message.Body = sell ? 
                    new TextPart("plain") {
                        Text = @$"Sr. {user.Name}, seu ativo {stock} atingiu o preço superior ao limite estabelecido, aconselhamos a venda."
                    } :
                    new TextPart("plain") {
                        Text = @$"Sr. {user.Name}, o ativo {stock} atingiu o preço inferior ao limite estabelecido, aconselhamos a compra."
                    };
                
                try {

                    await smtpClient.SendAsync(message);
                    successSents++;
                    
                } catch(Exception e){

                    StoreFailedMail(user, e.Message);

                }
            }
            smtpClient.Disconnect(true);
        }

        return successSents;
    }

    private void StoreFailedMail(User user, string returnMsg){

        var returnCode = returnMsg.Split(' ')[0];
        var failedMsg = $"---> Envio para : {user.Email} falhou. Código de retorno : {returnCode}";

        FailedMails.Add(failedMsg);
    }

    private void LogFailedMails(){
        foreach(var log in FailedMails){
            Console.WriteLine(log);
        }
    }
}