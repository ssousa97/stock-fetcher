using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

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

        int successSents = await CreateMessageAndSendMail(stock, sell);
    
        if(successSents == UserList.Count){

            Console.WriteLine("-> Todos os usuários notificados sobre a ultima atualização de preço.");

        } else if (successSents < UserList.Count && successSents > 0){

            Console.WriteLine($"-> Alguns usuários não foram notificados sobre a atualização de preço. Envios sucedidos : {successSents}/{UserList.Count} ");

        } else {

            Console.WriteLine("-> Nenhum usuário notificado pela atualização de preço.");

        }
        
    }

    private async Task<int> CreateMessageAndSendMail(string stock, bool sell){

        var messagesToSent = new List<Task<MailStatus>>();

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

            messagesToSent.Add(Task.Run(()=> ConnectAndSendMessage(message, user)));
        }

        var results = await Task.WhenAll(messagesToSent.ToArray());
        var failedMessages = results.Where(mailStatus => !mailStatus.Sent);

        foreach(var failed in failedMessages){
            Console.WriteLine($"--> Error ao enviar para {failed.User.Email}. Código de erro do servidor SMTP {SMTPServer} : {failed.Code}");
        }

        var successMessages = messagesToSent.Count() - failedMessages.Count();

        return successMessages;
    }

    private MailStatus ConnectAndSendMessage(MimeMessage message, User user){
        using(var smtpClient = new SmtpClient()){

            try { 
                smtpClient.Connect(SMTPServer, 465, true);
                smtpClient.Authenticate(FromMail, Password);

                var response = smtpClient.Send(message);

                smtpClient.Disconnect(true);

                return new MailStatus(){
                    Code = response.Split(' ')[0],
                    Sent = true,
                    User = user
                };

            }catch(Exception e){

                return new MailStatus(){
                    Code = e.Message.Split(' ')[0],
                    Sent = false,
                    User = user
                };
            
            }
        }
    }

}