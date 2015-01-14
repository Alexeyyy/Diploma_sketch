using System;
using ReformaAPITesting.ReformaAPI;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;

namespace ReformaAPITesting
{
    public static class SessionKey 
    {
        public static string LogKey { get; set; } 
    }
    
    /*
    #region Новый вариант 2
    public class InterceptorMessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref Message reply, object correlationState) 
        {
            Console.WriteLine(reply.Headers.Count);
            Console.WriteLine("!!!!!!!!!!!!!!!!");
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            throw new NotImplementedException();
        }
    }

    public class InterceptorBehaviour : IEndpointBehavior 
    {
        public void ApplyClientBehaviour(ServiceEndpoint endpoint, ClientRuntime clientRuntime) 
        {
            clientRuntime.MessageInspectors.Add(new InterceptorMessageInspector());
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            throw new NotImplementedException();
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            throw new NotImplementedException();
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Новый вариант
    public class MyBehaviour : BehaviorExtensionElement, IEndpointBehavior
    {
        private string headerKey = "authenticate";
        public MyBehaviour() { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new MyInspector());
        }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new MyInspector());
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void Validate(ServiceEndpoint endpoint) { }

        protected override object CreateBehavior()
        {
            return new MyBehaviour();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(MyBehaviour);
            }
        }
        /*public void AfterReceiveReply(ref Message reply, object correlationState) { }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            request.Headers.Add(MessageHeader.CreateHeader(headerKey, string.Empty, SessionKey.LogKey));
            return null;
        }*/
    }

    public class MyInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        private const string headerKey = "authenticate";

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            request.Headers.Add(MessageHeader.CreateHeader(headerKey, string.Empty, SessionKey.LogKey));
            return null;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
        public void BeforeSendReply(ref Message reply, object correlationState) { }
    }
    #endregion
    */
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
         * http://blogs.msdn.com/b/stcheng/archive/2009/02/21/wcf-how-to-inspect-and-modify-wcf-message-via-custom-messageinspector.aspx
         * http://msdn.microsoft.com/en-us/library/ms733786(v=vs.110).aspx
         * 
         * Custom endpoint behaviour
         * http://stackoverflow.com/questions/10448327/edit-soap-of-a-wcf-service-using-iclientmessageinspector  ???
         * http://burcakcakiroglu.com/defining-custom-wcf-endpoint-behavior/
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
                SessionKey.LogKey = response.LoginResult; 
                Console.WriteLine("After connection status: " + client.State.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("**********Login exception*************");
                Console.WriteLine(e.Message);
                Console.WriteLine("**************************************");
            }

            //client.Endpoint.Behaviors.Insert(0, new MyBehaviour());
            InterceptorBehaviour b = new InterceptorBehaviour();
            client.ChannelFactory.Endpoint.Behaviors.Insert(0, new InterceptorBehaviour());
            
            Console.WriteLine("Behaviours count = " + client.Endpoint.Behaviors.Count);
            foreach (var item in client.Endpoint.Behaviors) 
            {
                Console.WriteLine(item.GetType().Name);
                //if (item.GetType().Name.Equals("MyBehaviour"))
                    //item.
            }

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

