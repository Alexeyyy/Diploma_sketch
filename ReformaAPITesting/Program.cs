using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReformaAPITesting.ReformaAPI;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.IdentityModel.Selectors;
using Microsoft.ServiceBus;

namespace ReformaAPITesting
{
    public class CustomToken 
    {
        public string SecurityKey { get; set; }

        public CustomToken(string key) {
            SecurityKey = key;
        }
    }

    public class AuthHeaderBehaviour : IEndpointBehavior, IClientMessageInspector
    {
        private readonly CustomToken tokenProvider;

        public AuthHeaderBehaviour(CustomToken tokenProvider) 
        {
            this.tokenProvider = tokenProvider;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }
        
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        public object BeforeSendRequest(ref Message request, IClientChannel clientChannel) {
            request.Headers.Add(MessageHeader.CreateHeader("authenticate", string.Empty, this.tokenProvider.SecurityKey));
            return null;
        }
        public void Validate(ServiceEndpoint endpoint)
        {

        }
    }

    public class Program
    {
        /*
         1. Метод логин (Login)
         2. SetRequestForSubmit - подача на раскрытие информации управляющих организаций.
         3. GetRequestList - список запросов подписки на управляющую организацию
         4. GetReportingPeriodList - отчетные периоды системы
         5. SetCompanyProfile - изменяет данные по текущей/архивной анкете УО с соотв. ИНН за указанный
                                отчетный период. Внешняя система может обновлять анкеты только тех организаций, 
                                по которым разрешена подписка 
         6. GetHouseList - список домов в управлении конкретной организации
         7. GetHouseInfo - данные по дому
         * 
         * http://stackoverflow.com/questions/14621544/how-can-i-add-authorization-header-to-the-request-in-wcf
         * http://stackoverflow.com/questions/3879199/intercept-soap-messages-from-and-to-a-web-service-at-the-client
        */
        public static string LOGIN = "a.zhelepov";
        public static string PASSWORD = "!ALEX!()$!(($!";
        public static string endpointAddress = "http://api-beta.reformagkh.ru/api_document_literal";

        static void Main(string[] args)
        {
            //ApiSoapPortClient client = new ApiSoapPortClient(new BasicHttpBinding("ApiSoapBinding", new EndpointAddress());
            LoginResponse response = new LoginResponse();
            LoginRequest request = new LoginRequest(LOGIN, PASSWORD);
            SetRequestForSubmitInnStatus[] statuses;
            RequestState[] states;
            FiasAddress address = new ReformaAPI.FiasAddress();
            ApiSoapPortClient client = new ApiSoapPortClient(new BasicHttpBinding("ApiSoapBinding"), new EndpointAddress(endpointAddress));

            try
            {
                Console.WriteLine("Before connection status: " + client.State.ToString());
                response.LoginResult = client.Login(LOGIN, PASSWORD);
                Console.WriteLine("After connection status: " + client.State.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("***********************");
                Console.WriteLine(client.InnerChannel.LocalAddress.Uri.Host);
                Console.WriteLine(e.Message);
                Console.WriteLine("***********************");
            }

            CustomToken token = new CustomToken(response.LoginResult);
            AuthHeaderBehaviour behaviour = new AuthHeaderBehaviour(token);
            //behaviour.BeforeSendRequest(client.);


            Console.WriteLine("Login result: " + response.LoginResult);
            Console.WriteLine("URI - " + client.Endpoint.Address.Uri.AbsoluteUri);
            var cridential = new System.Net.NetworkCredential(request.login, request.password);
            
            Console.WriteLine("===================================");
            foreach(var item in client.Endpoint.Behaviors) {
                Console.WriteLine(item.ToString());
            }

            //client.Endpoint.Behaviors.Add();
            //client.Endpoint.Behaviors.Add(new AuthHeaderBehaviour(TokenProvider.CreateOAuthTokenProvider(new [] { new Uri(endpointAddress) }, cridential)));

            

            /*
            foreach (var item in client.Endpoint.Contract.Operations)
            {
                Console.WriteLine(item.Messages[0].Headers.ToString());
            }
            */
            try
            {
                statuses = client.SetRequestForSubmit(new string[] { "7329012644" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            /*try
            {
                client.Logout();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }*/

            Console.ReadLine();
        }

        public static void l_CustomDrawingPart()
        {
            Console.WriteLine("Custom part!");
        }
    }
}
