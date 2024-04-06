using Logger.Helpers;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using System.Xml;

namespace Workers
{
    public partial class Worker : WorkerFilter
    {
        private ManualResetEvent _eventStopWork = new ManualResetEvent(false);
        private ManualResetEvent _eventStopBatchCheck = new ManualResetEvent(false);

        private DateTime _timeStartWorker = DateTime.Now;

        private static MyLoggerNamespace.Logger _logger;
        public static new MyLoggerNamespace.Logger Logger
        {
            get
            {
                return _logger;
            }
        }

        static Worker()
        {
            _logger = InitLogger("Worker");
        }

        private static MyLoggerNamespace.Logger InitLogger(string loggerName)
        {

            try
            {
                return new MyLoggerNamespace.Logger(loggerName);
            }
            catch (Exception exc)
            {
                File.WriteAllText(
                    Path.Combine("log", $"{loggerName}_{DateTime.Now.Ticks}.log"), exc.Message);
                return null;
            }
        }

        public XmlDocument Config { get; private set; }
        public void InitCfg(string config = "conf.cfg")
        {
            string sBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string ConfigFileName = Path.Combine(sBaseDirectory, config);


            if (!File.Exists(ConfigFileName))
            {
                Console.WriteLine("file config not found in " + sBaseDirectory);
                return;
            }
            Config = new XmlDocument();
            Config.Load(ConfigFileName);
            XmlNode p_xndConfig = Config.FirstChild;

            XmlNode xndLogger = p_xndConfig.SelectSingleNode("Logger");
            Init(p_xndConfig);
        }

        public override void Init(XmlNode p_xndConfig)
        {
            _timeStartWorker = DateTime.Now.AddSeconds(-10);

            #region init connections
            XmlNode xndConnections = p_xndConfig.SelectSingleNode("Connections");
            if (xndConnections != null)
            {
                foreach (XmlNode node in xndConnections.SelectNodes("Connection"))
                {
                    string connectionName = XmlUtils.GetString(node.Attributes["Name"]);
                    string connectionString = XmlUtils.GetString(node.Attributes["ConnectionString"]);
                    string assembly = XmlUtils.GetString(node.Attributes["Assembly"]);
                    string className = XmlUtils.GetString(node.Attributes["ClassName"]);
                    string logLevel = XmlUtils.GetString(node.Attributes["LogLevel"], "All");
                    int timingMinMs = XmlUtils.GetInt(node.Attributes["TimingMinMs"], 15);
                    //ADBConnection.LogLevel level = (ADBConnection.LogLevel)Enum.Parse(typeof(ADBConnection.LogLevel), logLevel);

                    //ADBConnection connection = (ADBConnection)ADBConnection.Create(connectionName, connectionString, assembly, className, level, timingMinMs);
                }
            }
            #endregion

            #region BankRegistryConverter
            var workerMailParser = p_xndConfig.SelectSingleNode("BankRegistryConverter");
            if (workerMailParser != null)
            {
                //ConfigureWorkerMailParserParameters(workerMailParser);
            }
            else
            {
                Logger.WriteLine(MessageType.Warning, "[Web Api Worker] Node <BankRegistryConverter> not found");
            }
            #endregion
        }

        public override void Start()
        {

        }
    }
}