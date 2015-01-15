using System;
using System.Linq;
using ReformaAPITesting.ReformaAPI;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;

namespace ReformaAPITesting
{
    public class APIProvider
    {
        private static string login = "a.zhelepov";
        private static string password = "!ALEX!()$!(($!";
        private LoginResponse loginResponse;
        private ApiSoapPortClient client;

        public APIProvider(ApiSoapPortClient client)
        {
            this.client = client;
            this.loginResponse = new LoginResponse();
        }

        #region Helpers

        /// <summary>
        /// Создать объект FiasAddress 
        /// </summary>
        /// <param name="cityId">Id города</param>
        /// <param name="streetId">Id улицы</param>
        /// <param name="building">Здание</param>
        /// <param name="houseNumber">Номер дома</param>
        /// <param name="block">Блок/корпус</param>
        /// <param name="roomNumber">Номер квартиры/комнаты</param>
        /// <returns></returns>
        public FiasAddress FillAddress(string cityId, string streetId, string building,
                                      string houseNumber, string block, string roomNumber)
        {
            return new FiasAddress()
            {
                city_id = cityId,
                street_id = streetId,
                building = building,
                house_number = houseNumber,
                block = block,
                room_number = roomNumber
            };
        }

        public CountDismissed FillCountDismissed(int? countDismissed, int? countDismissedAdmins,
                                                 int? countDismissedEngineers, int? countDismissedWorkers)
        {
            return new CountDismissed()
            {
                count_dismissed = countDismissed,
                count_dismissed_admins = countDismissedAdmins,
                count_dismissed_engineers = countDismissedEngineers,
                count_dismissed_workers = countDismissedWorkers
            };
        }

        #endregion

        #region Обеспечение доступа

        /// <summary>
        /// Метод выполняет авторизацию внешней системы и открывает сеанс работы.
        /// </summary>
        public void Login()
        {
            this.client.Endpoint.Behaviors.Add(new AuthHeaderBehavior(new TokenProvider(string.Empty)));
            this.loginResponse.LoginResult = this.client.Login(login, password);
            //для перехвата xml-сообщения
            (this.client.Endpoint.Behaviors.First(i => i.GetType() == typeof(AuthHeaderBehavior)) as AuthHeaderBehavior).TokenProvider.LogKey = loginResponse.LoginResult;
        }

        /// <summary>
        /// Метод завершает авторизованный сеанс работы внешней системы
        /// </summary>
        public void Logout()
        {
            this.client.Logout();
        }

        #endregion

        #region Получить информацию с реформы

        /// <summary>
        /// Метод возвращает список запросов подписки на управляющую организацию, поданных внешней системой (с детализацией статуса запроса).
        /// </summary>
        /// <returns></returns>
        public RequestState[] GetRequestList()
        {
            return this.client.GetRequestList();
        }

        /// <summary>
        /// Метод возвращает список отчетных периодов системы
        /// </summary>
        /// <returns></returns>
        public ReportingPeriod[] GetReportingPeriodList()
        {
            return client.GetReportingPeriodList();
        }

        #endregion

        #region Поставить информацию реформе

        /// <summary>
        /// Метод подачи запроса на раскрытие данных. Внешняя система подает на вход список ИНН управляющих организаций, по которым собирается раскрывать данные 
        /// </summary>
        /// <param name="inns">ИНН организаций</param>
        /// <returns></returns>
        public SetRequestForSubmitInnStatus[] SetRequestForSubmit(string[] inns)
        {
            return this.client.SetRequestForSubmit(inns);
        }

        public void SetCompanyProfile(string inn, int reportingPeriodId)
        {
            this.client.SetCompanyProfile(inn, reportingPeriodId, new CompanyProfileData());
        }

        /// <summary>
        /// Метод изменяет данные по текущей/архивной анкете управляющей организации с соответствующим ИНН за указанный отчетный период. Внешняя система может обновлять анкеты только тех организаций, по которым разрешена подписка.
        /// </summary>
        /// <returns></returns>
        public CompanyProfileData SetCompanyProfile(string fullName, string shortName, int okopf, string surname,
                                                    string middleName, string firstName, string position, string ogrn,
                                                    DateTime assignmentOgrnDate, string authorityNameAssigningOgrn,
                                                    FiasAddress legalAddress, FiasAddress actualAddress, FiasAddress postAddress,
                                                    string phone, string email, string site, float? proportion_sf,
                                                    float? proportion_mo, string additionallInfoFreeForm, string associationParticipations,
                                                    int srfCount, int moCount, int officesCount, int staffRegularTotal, int staffRegularAdministrative,
                                                    int staffRegularEngineers, int staffRegularLabour, CountDismissed countDismissed,
                                                    int? accidentsCount, int? prosecuteCount, string prosecuteDocumentsCount, string tsgManagementManagers,
                                                    string auditCommisionMembers, string additionalInfoFreeForm,
                                                    string workTime = ""
                                                    )
        {
            //какой-то доп. код, например, проверки различные

            return new CompanyProfileData()
            {
                name_full = fullName,
                name_short = shortName,
                okopf = okopf,
                surname = surname,
                middlename = middleName,
                firstname = firstName,
                position = position,
                ogrn = ogrn,
                date_assignment_ogrn = assignmentOgrnDate,
                name_authority_assigning_ogrn = authorityNameAssigningOgrn,
                legal_address = legalAddress,
                actual_address = actualAddress,
                post_address = postAddress,
                work_time = workTime,
                phone = phone,
                email = email,
                site = site,
                proportion_sf = proportion_sf,
                proportion_mo = proportion_mo,
                additional_info_freeform = additionallInfoFreeForm,
                participation_in_associations = associationParticipations,
                srf_count = srfCount,
                mo_count = moCount,
                offices_count = officesCount,
                staff_regular_total = staffRegularTotal,
                staff_regular_administrative = staffRegularAdministrative,
                staff_regular_engineers = staffRegularEngineers,
                staff_regular_labor = staffRegularLabour,
                count_dismissed = countDismissed,
                accidents_count = accidentsCount,
                prosecute_count = prosecuteCount,
                prosecute_copies_of_documents = prosecuteDocumentsCount,
                tsg_management_members = tsgManagementManagers,
                audit_commision_members = auditCommisionMembers,
            };
        }

        //Метод возвращает 
        #endregion
    }

    //=====================================================
    public class TokenProvider
    {
        public string LogKey { get; set; }
        public TokenProvider(string logKey)
        {
            LogKey = logKey;
        }
    }

    public class AuthHeaderBehavior : IEndpointBehavior, IClientMessageInspector
    {
        public TokenProvider TokenProvider { get; set; }

        public AuthHeaderBehavior(TokenProvider tokenProvider)
        {
            this.TokenProvider = tokenProvider;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (!String.IsNullOrEmpty(this.TokenProvider.LogKey))
                request.Headers.Add(MessageHeader.CreateHeader("authenticate", string.Empty, this.TokenProvider.LogKey));
            return null;
        }

        public void Validate(ServiceEndpoint endpoint) { }
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
        * http://blogs.msdn.com/b/stcheng/archive/2009/02/21/wcf-how-to-inspect-and-modify-wcf-message-via-custom-messageinspector.aspx
        * http://msdn.microsoft.com/en-us/library/ms733786(v=vs.110).aspx
        * 
        * Custom endpoint behaviour
        * http://stackoverflow.com/questions/10448327/edit-soap-of-a-wcf-service-using-iclientmessageinspector  ???
        * http://burcakcakiroglu.com/defining-custom-wcf-endpoint-behavior/
        *
         * http://stackoverflow.com/questions/643241/problem-with-wcf-client-calling-one-way-operation - logout problem
         * http://go4answers.webhost4life.com/Example/exception-while-making-way-call-wcf-56416.aspx
         * 
         */
        public static string LOGIN = "a.zhelepov";
        public static string PASSWORD = "!ALEX!()$!(($!";
        public static string endpointAddress = "http://api-beta.reformagkh.ru/api_document_literal";

        static void Main(string[] args)
        {
            APIProvider provider = new APIProvider(new ApiSoapPortClient());
            provider.Login();
            RequestState[] states = provider.GetRequestList();
            provider.Logout();

            LoginResponse response = new LoginResponse();
            LoginRequest request = new LoginRequest(LOGIN, PASSWORD);
            SetRequestForSubmitInnStatus[] statuses;
            FiasAddress address = new ReformaAPI.FiasAddress();
            ApiSoapPortClient client = new ApiSoapPortClient(new BasicHttpBinding("ApiSoapBinding"), new EndpointAddress(endpointAddress));
            TokenProvider token = new TokenProvider("");
            AuthHeaderBehavior behaviour = new AuthHeaderBehavior(token);
            client.Endpoint.Behaviors.Add(new AuthHeaderBehavior(token));

            try
            {
                Console.WriteLine("Before connection status: " + client.State.ToString());
                response.LoginResult = client.Login(LOGIN, PASSWORD);
                (client.Endpoint.Behaviors[2] as AuthHeaderBehavior).TokenProvider.LogKey = response.LoginResult;
                Console.WriteLine("After connection status: " + client.State.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error 1");
                Console.WriteLine(e.Message);
            }

            try
            {
                statuses = client.SetRequestForSubmit(new string[] { "7329012644" });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error 2");
                Console.WriteLine(e.Message);
            }

            try
            {
                client.Logout();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}

