using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ReformaAPITesting.ReformaAPI;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;
using System.IO;
using System.Xml;
using System.Text;

namespace ReformaAPITesting
{
    public class Test
    {
        public int IntVal { get; set; }
        public string StringVal { get; set; }
        public FiasAddress AddressVal { get; set; }
        public double DoubleVal { get; set; }
    }
    public class APIProvider
    {
        public static bool isA = false;
        private static string login = "a.zhelepov";
        private static string password = "!ALEX!()$!(($!";
        private LoginResponse loginResponse;
        private ApiSoapPortClient client;
        private Dictionary<string, KeyValuePair<Type, object>> valuesDictionary;

        public APIProvider(ApiSoapPortClient client)
        {
            this.client = client;
            this.loginResponse = new LoginResponse();
            this.valuesDictionary = new Dictionary<string, KeyValuePair<Type, object>>();
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
        public FiasAddress FillAddress(string city_id, string street_id, string building,
                                      string house_number, string block, string room_number)
        {
            return new FiasAddress()
            {
                city_id = city_id,
                street_id = street_id,
                building = building,
                house_number = house_number,
                block = block,
                room_number = room_number
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countDismissed"></param>
        /// <param name="countDismissedAdmins"></param>
        /// <param name="countDismissedEngineers"></param>
        /// <param name="countDismissedWorkers"></param>
        /// <returns></returns>
        public CountDismissed FillCountDismissed(int? count_dismissed, int? count_dismissed_admins,
                                                 int? count_dismissed_engineers, int? count_dismissed_workers)
        {
            return new CountDismissed()
            {
                count_dismissed = count_dismissed,
                count_dismissed_admins = count_dismissed_admins,
                count_dismissed_engineers = count_dismissed_engineers,
                count_dismissed_workers = count_dismissed_workers
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="housesCountUnderMngReportDate"></param>
        /// <param name="servicedByCompetition"></param>
        /// <param name="servicedByOwnerUO"></param>
        /// <param name="servicedByTSG"></param>
        /// <param name="servicedByTSGUO"></param>
        /// <returns></returns>
        public CountHousesUnderMngReportDate FillCountHousesUnderMngReportDate(int count_houses_under_mng_report_date,
                                                                               int? serviced_by_competition,
                                                                               int? serviced_by_owner_uo,
                                                                               int? serviced_by_tsg,
                                                                               int? serviced_by_tsg_uo
                                                                              )
        {
            return new CountHousesUnderMngReportDate()
            {
                count_houses_under_mng_report_date = count_houses_under_mng_report_date,
                serviced_by_competition = serviced_by_competition,
                serviced_by_owner_uo = serviced_by_owner_uo,
                serviced_by_tsg = serviced_by_tsg,
                serviced_by_tsg_uo = serviced_by_tsg_uo
            };
        }

        #endregion

        #region Обеспечение доступа к удаленному серверу

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
        public CompanyProfileData SetCompanyProfile(string fullName, string shortName, Okopf okopf, string surname,
                                                    string middleName, string firstName, string position, string ogrn,
                                                    DateTime assignmentOgrnDate, string authorityNameAssigningOgrn,
                                                    FiasAddress legalAddress, FiasAddress actualAddress, FiasAddress postAddress,
                                                    string phone, string email, string site, float? proportion_sf,
                                                    float? proportion_mo, string additionallInfoFreeForm, string associationParticipations,
                                                    int srfCount, int moCount, int officesCount, int staffRegularTotal, int staffRegularAdministrative,
                                                    int staffRegularEngineers, int staffRegularLabour, CountDismissed countDismissed,
                                                    int? accidentsCount, int? prosecuteCount, string prosecuteDocumentsCount, string tsgManagementManagers,
                                                    string auditCommisionMembers, string additionalInfoFreeForm, int residentsCount,
                                                    CountHousesUnderMngReportDate housesUnderMngReportDateCount,
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
                residents_count = residentsCount,
                count_houses_under_mng_report_date = housesUnderMngReportDateCount
            };
        }

        public Test FillTest(FiasAddress AddressVal, double DoubleVal, int IntVal, string StringVal)
        {
            var hack = new { AddressVal, DoubleVal, IntVal, StringVal };
            var parameters = MethodBase.GetCurrentMethod().GetParameters();
            Test test = new Test();

            foreach (var item in test.GetType().GetProperties())
            {

            }

            //filling dictionary
            foreach (var item in test.GetType().GetProperties())
            {
                valuesDictionary.Add(item.Name, new KeyValuePair<Type, object>(hack.GetType().GetProperties().First(i => i.Name == item.Name).PropertyType,
                                                                              hack.GetType().GetProperties().First(i => i.Name == item.Name).GetValue(hack, null)));
            }


            foreach (var item in valuesDictionary)
            {
                Console.WriteLine("{0} = {1} *** {2}", item.Key, item.Value.Key.Name, item.Value.Value);
            }

            return new Test()
            {

                AddressVal = AddressVal,
                DoubleVal = DoubleVal,
                IntVal = IntVal,
                StringVal = StringVal
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

    #region Attempt to solve exception problem
    public class CustomTextMessageEncoder : MessageEncoder
    {
        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;
        private string contentType;

        public CustomTextMessageEncoder(CustomTextMessageEncoderFactory factory)
        {
            this.factory = factory;

            this.writerSettings = new XmlWriterSettings();
            this.writerSettings.Encoding = Encoding.GetEncoding(factory.CharSet);
            this.contentType = string.Format("{0}; charset={1}",
                this.factory.MediaType, this.writerSettings.Encoding.HeaderName);
        }

        public override string ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public override string MediaType
        {
            get
            {
                return factory.MediaType;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.factory.MessageVersion;
            }
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            byte[] msgContents = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
            bufferManager.ReturnBuffer(buffer.Array);

            MemoryStream stream = new MemoryStream(msgContents);
            Message mess = ReadMessage(stream, int.MaxValue); 
            return mess;
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            XmlReader reader = XmlReader.Create(stream);
            return Message.CreateMessage(reader, maxSizeOfHeaders, this.MessageVersion);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);
            message.WriteMessage(writer);
            writer.Close();

            byte[] messageBytes = stream.GetBuffer();
            int messageLength = (int)stream.Position;
            stream.Close();

            int totalLength = messageLength + messageOffset;
            byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
            Array.Copy(messageBytes, 0, totalBytes, messageOffset, messageLength);

            ArraySegment<byte> byteArray = new ArraySegment<byte>(totalBytes, messageOffset, messageLength);
            return byteArray;
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);
            message.WriteMessage(writer);
            writer.Close();
        }
    }
    public class CustomTextMessageEncoderFactory : MessageEncoderFactory
    {
        private MessageEncoder encoder;
        private MessageVersion version;
        private string mediaType;
        private string charSet;

        internal CustomTextMessageEncoderFactory(string mediaType, string charSet,
            MessageVersion version)
        {
            this.version = version;
            this.mediaType = mediaType;
            this.charSet = charSet;
            this.encoder = new CustomTextMessageEncoder(this);
        }

        public override MessageEncoder Encoder
        {
            get
            {
                return this.encoder;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.version;
            }
        }

        internal string MediaType
        {
            get
            {
                return this.mediaType;
            }
        }

        internal string CharSet
        {
            get
            {
                return this.charSet;
            }
        }
    }
    
    public class CustomTextMessageBindingElement : MessageEncodingBindingElement, IWsdlExportExtension
    {
        private MessageVersion msgVersion;
        private string mediaType;
        private string encoding;
        private XmlDictionaryReaderQuotas readerQuotas;

        CustomTextMessageBindingElement(CustomTextMessageBindingElement binding)
            : this(binding.Encoding, binding.MediaType, binding.MessageVersion)
        {
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            binding.ReaderQuotas.CopyTo(this.readerQuotas);
        }

        public CustomTextMessageBindingElement(string encoding, string mediaType,
            MessageVersion msgVersion)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            if (mediaType == null)
                throw new ArgumentNullException("mediaType");

            if (msgVersion == null)
                throw new ArgumentNullException("msgVersion");

            this.msgVersion = msgVersion;
            this.mediaType = mediaType;
            this.encoding = encoding;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
        }

        public CustomTextMessageBindingElement(string encoding, string mediaType)
            : this(encoding, mediaType, MessageVersion.Soap11WSAddressing10)
        {
        }

        public CustomTextMessageBindingElement(string encoding)
            : this(encoding, "text/xml")
        {

        }

        public CustomTextMessageBindingElement()
            : this("UTF-8")
        {
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.msgVersion;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.msgVersion = value;
            }
        }


        public string MediaType
        {
            get
            {
                return this.mediaType;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.mediaType = value;
            }
        }

        public string Encoding
        {
            get
            {
                return this.encoding;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.encoding = value;
            }
        }

        // This encoder does not enforces any quotas for the unsecure messages. The  
        // quotas are enforced for the secure portions of messages when this encoder 
        // is used in a binding that is configured with security.  
        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        #region IMessageEncodingBindingElement Members
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new CustomTextMessageEncoderFactory(this.MediaType,
                this.Encoding, this.MessageVersion);
        }

        #endregion


        public override BindingElement Clone()
        {
            return new CustomTextMessageBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return (T)(object)this.readerQuotas;
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        #region IWsdlExportExtension Members

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            // The MessageEncodingBindingElement is responsible for ensuring that the WSDL has the correct 
            // SOAP version. We can delegate to the WCF implementation of TextMessageEncodingBindingElement for this.
            TextMessageEncodingBindingElement mebe = new TextMessageEncodingBindingElement();
            mebe.MessageVersion = this.msgVersion;
            ((IWsdlExportExtension)mebe).ExportEndpoint(exporter, context);
        }

        #endregion
    }
    #endregion

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
            request.Headers.Clear();
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
        public static string testInn = "7329012644";
        public static string registrationInn = "3304005479"; //INN (ITN) - specially created for new management company registration //ТСЖ "Текстильщик" с Владимира "3304005479"
        public static int houseTestId = 7497097;
        public static int houseTestId2 = 8928081;

        /*static void Main(string[] args)
        {
            APIProvider provider = new APIProvider(new ApiSoapPortClient());
            Test t = provider.FillTest(provider.FillAddress("1", "2", "3", "4", "5", "6"), 10.5, 20, "Hello!");
            provider.Login();
            RequestState[] states = provider.GetRequestList();
            ReportingPeriod[] periods = provider.GetReportingPeriodList();
            provider.Logout();
        */

        //Вопросы
        //1. Missing Okopf SetCompanyProfile() - с какого фига, если установил 

        public static void PrintHousesList(string inn, HouseData[] housesData)
        {
            Console.WriteLine("******************House list for organization {0}***********************", inn);
            for (int i = 0; i < housesData.Count(); i++)
            {
                if (housesData[i].house_id.Equals(7497097))
                    throw new Exception();
                Console.WriteLine("{0}. - ", i);
                Console.WriteLine("HouseId = {0} |||, CityId = {1}, StreetId = {2} HouseNumber = {3}, Block = {4}, Building = {5}, Room = {6}", housesData[i].house_id, housesData[i].full_address.city1_guid, housesData[i].full_address.street_guid, housesData[i].full_address.house_number, housesData[i].full_address.block, housesData[i].full_address.building, "-");
            }
            Console.WriteLine("===========================================================");
        }

        public static void PrintFilesInfo(int id, ReformaAPI.FileInfo[] files)
        {
            Console.WriteLine("==================File info log========================");
            Console.WriteLine("The list is actual for house with id {0}", id);
            foreach (var item in files)
            {
                Console.WriteLine("{0} - {1}.{2}", item.file_id, item.name, item.extension);
            }
            Console.WriteLine("=======================================================");
        }

        static void Main(string[] args)
        {
            //Client creation
            ApiSoapPortClient client = new ApiSoapPortClient(new BasicHttpBinding("ApiSoapBinding"), new EndpointAddress(endpointAddress));
            LoginResponse response = new LoginResponse();
            //Particular token to store seesion key
            TokenProvider token = new TokenProvider("");

            //Custom behaviour for message interception and seesion key adding
            AuthHeaderBehavior behaviour = new AuthHeaderBehavior(token);
            //Activate this behaviour for our client connection
            client.Endpoint.Behaviors.Add(new AuthHeaderBehavior(token));
            
            // one way exception solution attempt
            /*ICollection<BindingElement> bindingElements = new List<BindingElement>();
            HttpTransportBindingElement httpBindingElement = new HttpTransportBindingElement();
            CustomTextMessageBindingElement textBindingElement = new CustomTextMessageBindingElement();
            bindingElements.Add(textBindingElement);
            bindingElements.Add(httpBindingElement);
            CustomBinding binding = new CustomBinding(bindingElements);
            client.Endpoint.Binding = binding;*/
            //
            
            //1. *************** Login() *************** (+)
            response.LoginResult = client.Login(LOGIN, PASSWORD);
            (client.Endpoint.Behaviors.First(i => i.GetType() == typeof(AuthHeaderBehavior)) as AuthHeaderBehavior).TokenProvider.LogKey = response.LoginResult;
            Console.WriteLine("My session key is {0}", response.LoginResult);

            //2. ***************SetRequestForSubmit() - set subscriptions for newcomers*********** (+)
            SetRequestForSubmitInnStatus[] registrationStatus = client.SetRequestForSubmit(new string[] { testInn });

            //3. ***************GetRequestList() - returns a list of all requests statuses of subscribed companies   (+)
            RequestState[] requestStates = client.GetRequestList();

            //4. *************** GetReportingPeriodList() - returns a list of system report periods
            ReportingPeriod[] periods = client.GetReportingPeriodList();
            //GetCompanyProfileSFResponse d = client.GetCompanyProfileSF("fee76045-fe22-43a4-ad58-ad99e903bd58", 1, periods.Last().id);
            //5. *************** SetCompanyProfile() - changes company data according to current/archived organization document with corresponded INN(ITN)individual tafor set report period (-) so much stuff to load
            try
            {
                client.SetCompanyProfile(testInn, periods[4].id, new CompanyProfileData()
                {
                    name_full = "Новое время",
                    name_short = "Новое время",
                    okopf = Okopf.Item28016,
                    surname = "Мышляев",
                    firstname = "Игорь",
                    middlename = "Александрович",
                    position = "Директор",
                    ogrn = "",
                    date_assignment_ogrn = DateTime.Now.AddYears(-3),
                    name_authority_assigning_ogrn = "Должностное лицо",
                    legal_address = new FiasAddress()
                    {
                        city_id = "73b29372-242c-42c5-89cd-8814bc2368af",
                        street_id = "e6087222-376a-43f6-8f0d-44bc9963daa3",
                        house_number = "110"
                    },
                    actual_address = new FiasAddress()
                    {
                        city_id = "73b29372-242c-42c5-89cd-8814bc2368af",
                        street_id = "e6087222-376a-43f6-8f0d-44bc9963daa3",
                        house_number = "110"
                    },
                    post_address = new FiasAddress()
                    {
                        city_id = "73b29372-242c-42c5-89cd-8814bc2368af",
                        street_id = "e6087222-376a-43f6-8f0d-44bc9963daa3",
                        house_number = "110"
                    },
                    work_time = "9:00 - 21:00",
                    phone = "2233322",
                    email = "organization@mail.ru",
                    site = "nowebsite.ru",
                    proportion_sf = 20.32f,
                    proportion_mo = 20.65f,
                    additional_info_freeform = "it is an additional information",
                    participation_in_associations = "we're really communicative and participate in everything",
                    srf_count = 1,
                    mo_count = 2,
                    offices_count = 2,
                    staff_regular_total = 100,
                    staff_regular_administrative = 10,
                    staff_regular_engineers = 60,
                    staff_regular_labor = 30,
                    count_dismissed = new CountDismissed()
                    {
                        count_dismissed = 10,
                        count_dismissed_admins = 1,
                        count_dismissed_engineers = 1,
                        count_dismissed_workers = 8
                    },
                    accidents_count = 6,
                    prosecute_count = 3,
                    prosecute_copies_of_documents = "some prosecute documents copies",
                    tsg_management_members = "management members",
                    audit_commision_members = "audit members",
                    residents_count = 3,
                    count_houses_under_mng_report_date = new CountHousesUnderMngReportDate()
                    {
                        count_houses_under_mng_report_date = 30,
                        serviced_by_competition = 5,
                        serviced_by_owner_uo = 5,
                        serviced_by_tsg = 10,
                        serviced_by_tsg_uo = 10
                    },
                    count_houses_under_mng_start_period = new CountHousesUnderMngStartPeriod()
                    {
                        count_houses_under_mng_start_period = 300,
                        serviced_by_competition = 50,
                        serviced_by_owner_uo = 50,
                        serviced_by_tsg = 100,
                        serviced_by_tsg_uo = 100
                    },
                    avg_time_service_mkd = new AvgTimeServiceMkd()
                    {
                        avg_time_service_mkd = 10000f,
                        by_houses_25 = 1000f,
                        by_houses_26_50 = 1000f,
                        by_houses_51_75 = 5000f,
                        by_houses_76 = 2000f,
                        by_houses_alarm = 1000f
                    },
                    income_of_mng = new IncomeOfMng()
                    {
                        by_houses_25 = 1000f,
                        by_houses_26_50 = 2000f,
                        by_houses_51_75 = 3000f,
                        by_houses_76 = 400f,
                        by_houses_alarm = 5000f,
                        income_of_mng = 10000f
                    },
                    net_assets = 10000000f,
                    annual_financial_statements = "Precisely estimated",
                    revenues_expenditures_estimates = "Estimations"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Method name is {0}, Exception type = {1}, Exception message = {2}", "SetNewCompany", e.GetType(), e.Message);
            }

            //5.1 *********************** GetCompanyProfileData()
            try
            {
                CompanyProfileData data = client.GetCompanyProfile(testInn, client.GetReportingPeriodList().Last().id);
            }
            catch (Exception e) { }

            //6. ************** SetNewCompany() - set a bid for a new company registration (-)
            try
            {
                client.SetNewCompany(registrationInn, new NewCompanyProfileData()
                {
                    firstname = "Иван",
                    surname = "Иванов",
                    middlename = "Иванович",
                    name_full = "Текстильщик ТСЖ",
                    name_short = "Текстильщик",
                    okopf = Okopf.Item28016,
                    position = "заместитель",
                    ogrn = "1033300201483",
                    date_assignment_ogrn = DateTime.Now.AddYears(-4),
                    name_authority_assigning_ogrn = "лицо власти",
                    actual_address = new FiasAddress()
                    {
                        city_id = "aa4aa0d7-f97f-4974-9291-c0a530a1ccb6", //Гусь-хрустальный
                        street_id = "81bfaf83-c26a-47cf-ae02-85eb193c4b6e", //Калинина
                        house_number = "50",
                        block = String.Empty,
                        building = String.Empty,
                        room_number = String.Empty,
                    },
                    legal_address = new FiasAddress()
                    {
                        city_id = "aa4aa0d7-f97f-4974-9291-c0a530a1ccb6", //Гусь-хрустальный
                        street_id = "81bfaf83-c26a-47cf-ae02-85eb193c4b6e", //Калинина
                        house_number = "50",
                        block = String.Empty,
                        building = String.Empty,
                        room_number = String.Empty,
                    },
                    post_address = new FiasAddress()
                    {
                        city_id = "aa4aa0d7-f97f-4974-9291-c0a530a1ccb6", //Гусь-хрустальный
                        street_id = "81bfaf83-c26a-47cf-ae02-85eb193c4b6e", //Калинина
                        house_number = "50",
                        block = String.Empty,
                        building = String.Empty,
                        room_number = String.Empty,
                    },
                    phone = "777888",
                    email = "emailaddress@mail.ru",
                    site = "www.nowebsite.ru",
                    proportion_mo = 0.0f,
                    proportion_sf = 0.0f
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Method name is {0}, Exception type = {1}, Exception message = {2}", "SetNewCompany", e.GetType(), e.Message);
            }

            //6.1 *************** SetCompanyProfile()


            //Односторонняя операция вернула ненулевое сообщение с Action=
            //7. ************** GetHouseList() - returns houses list which are under management of organization with corresponded INN
            HouseData[] housesData = client.GetHouseList(testInn);
            PrintHousesList(testInn, housesData);

            //Create new FiasAddres for further magic
            int houseId = housesData.First().house_id;
            FiasAddress address = new FiasAddress()
            {
                city_id = housesData.First().full_address.city1_guid,
                street_id = housesData.First().full_address.street_guid,
                house_number = housesData.First().full_address.house_number,
                block = housesData.First().full_address.block,
                building = housesData.First().full_address.building,
                room_number = ""
            };

            //8. ************** SetUnlinkFromOrganization / SetHouseLinkToOrganization - resets and sets house under organization management 
            try
            {
                //client.SetUnlinkFromOrganization(houseId, DateTime.Now, 1, "Just a test reason");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception type = {0}, Exception message = {1}", e.GetType(), e.Message);
            }
            housesData = client.GetHouseList(testInn);
            PrintHousesList(testInn, housesData);

            try
            {
                client.SetHouseLinkToOrganization(houseId, testInn, DateTime.Now, DateTime.Now.AddSeconds(5));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception type = {0}, Exception message = {1}", e.GetType(), e.Message);
            }
            housesData = client.GetHouseList(testInn);
            PrintHousesList(testInn, housesData);

            //9. ************** SetFileToHouseProfile() and GetFilesInfoFromHouseProfile()
            FileStream fs = new FileStream("crutch.jpg", FileMode.Open, FileAccess.Read);
            byte[] fileBytes = new byte[fs.Length];
            fs.Read(fileBytes, 0, Convert.ToInt32(fs.Length));
            string encodedData = Convert.ToBase64String(fileBytes, Base64FormattingOptions.InsertLineBreaks);

            GetHouseProfileResponse resp = client.GetHouseProfile(houseTestId2);
            //client.SetFileToHouseProfile(houseTestId2, 12, new FileObject() { name = "crutch.jpg", data = encodedData });
            ReformaAPI.FileInfo[] files = client.GetFilesInfoFromHouseProfile(houseTestId2, 12);
            PrintFilesInfo(houseTestId2, files);

            //10. ************* GetFileById() ****************
            FileObject file = client.GetFileByID(files.Last().file_id);
            //let's make attempt to save file to computer folder
            var bytes = Convert.FromBase64String(file.data);
            using (var imgFile = new FileStream("newfile" + file.name.Substring(file.name.IndexOf('.'), file.name.Length - file.name.IndexOf('.')), FileMode.Create))
            {
                imgFile.Write(bytes, 0, bytes.Length);
                imgFile.Flush();
            }

            //11. *************SetFileDelete() ***************
            Console.WriteLine("Before delete");
            files = client.GetFilesInfoFromHouseProfile(houseTestId2, 12);
            PrintFilesInfo(houseTestId2, files);

            if (files.Last() != null)
            {
                try
                {
                    client.SetFileDeleted(files.Last().file_id);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception type = {0}, Exception message = {1}", e.GetType(), e.Message);
                }
            }
            //checking... did the file really delete?
            files = client.GetFilesInfoFromHouseProfile(houseTestId2, 12);
            Console.WriteLine("After delete");
            PrintFilesInfo(houseTestId2, files);

            // ************** SetFileToCompanyProfile() - sets a new file in organization document for a corresponded report period
            //client.SetFileToCompanyProfile(periods.Last().id, "7329012644", 1, new FileObject() { data = "Some test data to upload", name = "testFile" }); 

            //   *************** Logout() ***************
            try
            {
                client.Logout();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception type = {0}, Exception message = {1}", e.GetType(), e.Message);
            }

            Console.WriteLine("Press any key to interrupt the program execution...");
            Console.ReadKey();
        }
    }
}

