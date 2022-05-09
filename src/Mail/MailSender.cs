using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;


struct User {
    public string Name  {get; set;}
    public string Email {get;set;}
}

class MailSender : IMailService {

    private string SMTPServer;
    private string FromMail;
    private string FromName;
    private string Password;
    private List<User> UserList;

    private readonly IConfiguration ConfigurationService;

    public MailSender(){

        ConfigurationService = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.json")
            .Build();

        var smtpConfig = ConfigurationService.GetSection("smtpConfig");
        var emailsConfig = ConfigurationService.GetSection("emails");

        SMTPServer     = smtpConfig.GetSection("server").Value;
        FromMail       = smtpConfig.GetSection("usermail").Value;
        FromName       = smtpConfig.GetSection("username").Value;
        Password       = smtpConfig.GetSection("password").Value;
        UserList       = emailsConfig.Get<List<User>>();

    }

    public void SendMail(string stock, float price, bool sell) {
        
        Console.WriteLine("-> Informando usuários sobre alteração de preço.");

        using(var smtpClient = new SmtpClient()){
            
            smtpClient.Connect(SMTPServer, 465, true);
            smtpClient.Authenticate(FromMail, Password);

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

                var ret = smtpClient.Send(message);

                Console.WriteLine(ret);
            }
            Console.WriteLine("-> Usuários notificados sobre a ultima atualização de preço.");
            smtpClient.Disconnect(true);
        }

    }

}