namespace Utils
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;


    /// Utility methods for Reflection namespace
    public class ReflectionUtils
    {
        /// Collection of search paths
        protected static StringCollection s_objAssemblySearchPaths;

        /// Load mode
        protected static bool s_bNormalLoad;

        /// SyncRoot
        protected static object s_oSyncRoot;

        /// Default Assembly PublicKeyToken
        protected static string s_sDefaultPublicKeyToken;

        /// Default Assembly Version
        protected static Version s_objDefaultAssemblyVersion;

        private static string m_sApplicationPath;


        //----------------------------------------------------------------------------
        /// <summary>
        /// Статический конструктор. 
        /// </summary>
        static ReflectionUtils()
        {
            s_bNormalLoad = true;
            s_objAssemblySearchPaths = new StringCollection();

            // application path
            m_sApplicationPath = "";
            if (Assembly.GetEntryAssembly() != null && !String.IsNullOrEmpty(Assembly.GetEntryAssembly().Location))
            {
                m_sApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else if (!String.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory))
            {
                m_sApplicationPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            }
            else if (Assembly.GetExecutingAssembly() != null && !String.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
            {
                m_sApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            s_objAssemblySearchPaths.Add(m_sApplicationPath);

            s_oSyncRoot = new object();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Загружать ассембли очным способом - напрямую с диска, 
        /// либо предварительно закачать файл в память (позволяет избежать блокирования файла на диске).
        /// </summary>
        public static bool NormalLoad
        {
            get
            {
                return s_bNormalLoad;
            }
            set
            {
                s_bNormalLoad = value;
            }
        }



        //----------------------------------------------------------------------------
        /// <summary>
        /// PublicKeyToken по умолчанию всех сборок для загрузки ассембли из GAC в случае её ненахождения локально в рабочих директориях
        /// </summary>
        public static string DefaultPublicKeyToken
        {
            get
            {
                return s_sDefaultPublicKeyToken;
            }
            set
            {
                s_sDefaultPublicKeyToken = value;
            }
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Версия ассембли по умолчанию
        /// </summary>
        public static Version DefaultAssemblyVersion
        {
            get
            {
                return s_objDefaultAssemblyVersion;
            }
            set
            {
                s_objDefaultAssemblyVersion = value;
            }
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Создаёт объект.
        /// </summary>
        /// <param name="p_sAssemblyPath">Путь до сборки.</param>
        /// <param name="p_sFullClassName">Полное имя класса.</param>
        /// <returns>Объект</returns>
        /// <remarks>Не прозводится повторной загрузки сборки если она уже загружена.</remarks>
        public static object CreateObject(string p_sAssemblyPath, string p_sFullClassName)
        {
            return ReflectionUtils.CreateObject(p_sAssemblyPath, p_sFullClassName, null);
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Создаёт объект.
        /// </summary>
        /// <param name="p_sAssemblyPath">Путь до сборки.</param>
        /// <param name="p_sFullClassName">Полное имя класса.</param>
        /// <param name="p_aoParams">Параметры для конструктора.</param>
        /// <returns>Объект</returns>
        /// <remarks>Не прозводится повторной загрузки сборки если она уже загружена.</remarks>
        public static object CreateObject(string p_sAssemblyPath, string p_sFullClassName, params object[] p_aoParams)
        {
            Assembly objAssembly = ReflectionUtils.LoadAssembly(p_sAssemblyPath);
            if (objAssembly == null)
            {
                throw new ExException("ReflectionUtils_CantLoadAssembly", "Can't load assembly {0}", p_sAssemblyPath);
            }

            object oObject = null;
            if (p_aoParams == null)
            {
                oObject = objAssembly.CreateInstance(p_sFullClassName);
            }
            else
            {
                oObject = objAssembly.CreateInstance(p_sFullClassName, false, BindingFlags.CreateInstance, null, p_aoParams, Thread.CurrentThread.CurrentCulture, null);
            }

            if (oObject == null)
            {
                throw new ExException("ReflectionUtils_ClassNotFound", "Class {0} not found in {1}", p_sFullClassName, p_sAssemblyPath);
            }

            return oObject;
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Загружает сборку.
        /// </summary>
        /// <param name="p_sPath">Путь до сборки</param>
        /// <returns>Загруженная сборка.</returns>
        /// <remarks>Если сборка уже загружена то возвращается загруженная копия.</remarks>
        public static Assembly LoadAssembly(string p_sPath)
        {
            return ReflectionUtils.LoadAssembly(p_sPath, true);
        }


        //----------------------------------------------------------------------------
        private static Assembly LoadAssemblyFromFile(string p_sPath, bool p_bCheckExisting)
        {
            string sPath = GetAssemblyName(p_sPath, true);
            Assembly objAssembly = null;
            if (p_bCheckExisting)
            {
                objAssembly = ReflectionUtils.CheckAssemblyLoaded(sPath);
            }

            if (!ReflectionUtils.NormalLoad)
            {
                if (objAssembly != null)
                {
                    AssemblyName objAssemblyName = null;
                    try
                    {
                        objAssemblyName = AssemblyName.GetAssemblyName(sPath);
                    }
                    catch
                    {
                    }

                    if (objAssemblyName != null)
                    {

                        bool bIsNeedLoad = ReflectionUtils.IsNeedRawLoad(objAssembly.GetName(), sPath);
                        if (bIsNeedLoad)
                        {
                            objAssembly = null; //force load new version
                        }
                    }
                    /*
					if(	objAssemblyName == null )
					{
						objAssembly = null; //force load new version
					}
					else
					{
						if( objAssemblyName.Version > objAssembly.GetName().Version ) 
						{
							objAssembly = null; //force load new version
						}					
					}
					*/
                }
            }

            if (objAssembly == null)
            {
                bool bNeedToSearch = true;
                if (File.Exists(sPath))
                {
                    try
                    {
                        if (ReflectionUtils.NormalLoad)
                        {
                            objAssembly = ReflectionUtils.NormalLoadAssembly(sPath);
                        }
                        else
                        {
                            objAssembly = ReflectionUtils.RawLoadAssembly(sPath);
                        }
                        bNeedToSearch = false;
                    }
                    catch
                    {
                    }
                }
                if (bNeedToSearch)
                {
                    objAssembly = ReflectionUtils.SearchAssembly(Path.GetFileName(sPath), true);
                }
            }
            return objAssembly;
        }


        //----------------------------------------------------------------------------
        private static Assembly LoadAssemblyFromGAC(string p_sFullAssemblyName, bool p_bCheckExisting)
        {
            string sFullAssemblyName = p_sFullAssemblyName;

            if (sFullAssemblyName.IndexOf(".dll") > 0)
            {
                sFullAssemblyName = ReflectionUtils.GetAssemblyName(sFullAssemblyName, false);
            }

            Assembly objAssembly = null;
            if (p_bCheckExisting)
            {
                objAssembly = ReflectionUtils.CheckAssemblyLoaded(sFullAssemblyName);
            }

            if (sFullAssemblyName.IndexOf("Version=") < 0 && ReflectionUtils.DefaultAssemblyVersion != null)
            {
                sFullAssemblyName += ", Version=" + ReflectionUtils.DefaultAssemblyVersion.ToString();
            }

            if (objAssembly != null)
            {
                string sVersion = sFullAssemblyName.Substring(sFullAssemblyName.IndexOf("Version="));
                sVersion = sVersion.Substring(0, sFullAssemblyName.IndexOf(","));
                Version objVersion = new Version(sVersion);
                if (objAssembly.GetName().Version.CompareTo(objVersion) == -1)
                {
                    objAssembly = null; //force load new version
                }
            }

            if (sFullAssemblyName.IndexOf("PublicKeyToken=") < 0 && ReflectionUtils.DefaultPublicKeyToken != null)
            {
                sFullAssemblyName += ", PublicKeyToken=" + ReflectionUtils.DefaultPublicKeyToken;
            }

            if (objAssembly == null)
            {
                try
                {
                    objAssembly = ReflectionUtils.NormalLoadAssembly(sFullAssemblyName);
                }
                catch
                {
                }
            }

            return objAssembly;
        }


        private static Hashtable s_htCircle = Hashtable.Synchronized(new Hashtable());
        //----------------------------------------------------------------------------
        /// <summary>
        /// LoadAssembly
        /// </summary>
        /// <param name="p_sAssembly">Путь или имя ассембли</param>
        /// <param name="p_bCheckExisting">Проверять если существует</param>
        /// <returns></returns>
        public static Assembly LoadAssembly(string p_sAssembly, bool p_bCheckExisting)
        {
            if (String.IsNullOrEmpty(p_sAssembly))
            {
                throw new ArgumentNullException("p_sAssembly");
            }

            lock (s_oSyncRoot)
            {
                Assembly objAssembly = null;
                if (!s_htCircle.Contains(p_sAssembly))
                {
                    try
                    {
                        s_htCircle.Add(p_sAssembly, 1);
                        objAssembly = ReflectionUtils.LoadAssemblyFromFile(p_sAssembly, p_bCheckExisting);
                        if (objAssembly == null)
                        {
                            objAssembly = ReflectionUtils.LoadAssemblyFromGAC(p_sAssembly, p_bCheckExisting);
                        }
                    }
                    finally
                    {
                        s_htCircle.Remove(p_sAssembly);
                    }
                }

                // throw exception if assembly not found
                if (objAssembly == null)
                {
                    StringBuilder objPathsBuilder = new StringBuilder();
                    objPathsBuilder.Append(Environment.NewLine);
                    int iDirectoryCounter = 0;
                    foreach (string sDirectory in s_objAssemblySearchPaths)
                    {
                        objPathsBuilder.Append('\t');
                        objPathsBuilder.Append(sDirectory);
                        if (iDirectoryCounter < s_objAssemblySearchPaths.Count - 1)
                        {
                            objPathsBuilder.Append(Environment.NewLine);
                        }
                        iDirectoryCounter++;
                    }
                    objPathsBuilder.Append(Environment.NewLine);
                    objPathsBuilder.Append('\t');
                    objPathsBuilder.Append("in GAC");
                    throw new Exception(string.Format("Assembly {0} not found (or not loaded) in search paths:{1}", p_sAssembly, objPathsBuilder.ToString()));
                }
                return objAssembly;
            }
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Загружает сборку обычным способом (<see cref="Assembly.LoadFrom( string )"/>)
        /// </summary>
        /// <param name="p_sPath">Путь до сборки.</param>
        /// <returns>Загруженная сборка.</returns>
        public static Assembly NormalLoadAssembly(string p_sPath)
        {
            Assembly objAssembly;
            AssemblyName objAssemblyName = new AssemblyName();
            objAssemblyName.CodeBase = p_sPath;
            objAssembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(p_sPath);

            //objAssembly = AppDomain.CurrentDomain.Load( objAssemblyName );
            string sSubKey = s_sKey + objAssembly.GetName().Name;
            AppDomain.CurrentDomain.SetData(sSubKey, p_sPath);
            return objAssembly;
        }


        protected static Hashtable s_htRawAssemblyTimeInfo = Hashtable.Synchronized(new Hashtable());
        protected static string s_sKey = "MOST_RAW_ASSEMBLY_LOAD";
        //----------------------------------------------------------------------------
        /// <summary>
        /// Возвращает путь к dll загруженной методом RawLoadAssembly.
        /// </summary>
        /// <param name="p_sAssemblyFullName"><see cref="AssemblyName.Name"/></param>
        /// <returns></returns>
        public static string GetRawAssemblyPathByName(string p_sAssemblyName)
        {
            return GetRawAssemblyPathByName(p_sAssemblyName, AppDomain.CurrentDomain);
        }

        public static string GetRawAssemblyPathByName(string p_sAssemblyName, AppDomain Domain)
        {
            string sSubKey = s_sKey + p_sAssemblyName;
            return (string)AppDomain.CurrentDomain.GetData(sSubKey);
        }

        public static Assembly RawLoadAssembly(string Path)
        {
            return RawLoadAssembly(Path, AppDomain.CurrentDomain);
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Загрузка сборки из памяти.
        /// </summary>
        /// <param name="p_sPath">Путь до сборки.</param>
        /// <returns>Загруженная сборка.</returns>
        /// <remarks>Файл со сборкой предварительно закачивается в память. Позволяет избежать блокировок.</remarks>
        public static Assembly RawLoadAssembly(string p_sPath, AppDomain Domain)
        {
            if (String.IsNullOrEmpty(p_sPath))
            {
                throw new ArgumentNullException("p_sPath");
            }

            byte[] abRawAssembly = Files.ReadData(p_sPath);
            string sSymbolsPath = p_sPath.Replace(".dll", ".pdb"); // Path.ChangeExtension( p_sPath, ".pdb" );
            byte[] abRawAssemblySymbols = null;
            if (File.Exists(sSymbolsPath))
            {
                abRawAssemblySymbols = Files.ReadData(sSymbolsPath);
            }

            Assembly objAssembly;
            if (abRawAssemblySymbols != null)
            {
                objAssembly = Domain.Load(abRawAssembly, abRawAssemblySymbols);
            }
            else
            {
                objAssembly = Domain.Load(abRawAssembly);
            }


            string sAssemblyName = objAssembly.GetName().Name;
            string sSubKey = s_sKey + sAssemblyName;
            Domain.SetData(sSubKey, p_sPath);


            FileInfo fifMyFile = new FileInfo(p_sPath);
            s_htRawAssemblyTimeInfo[sAssemblyName] = fifMyFile.LastWriteTime;

            return objAssembly;
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Транслирует отнсительный путь в абсолютный.
        /// </summary>
        /// <param name="p_sRelativePath">Относительный путь.</param>
        /// <returns>Абсолютный путь.</returns>
        public static string MapPath(string p_sRelativePath)
        {
            if (p_sRelativePath == null)
            {
                throw new ArgumentNullException("p_sRelativePath");
            }

            return Path.Combine(m_sApplicationPath, p_sRelativePath);
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Добавить поисковый путь.
        /// </summary>
        /// <param name="p_sPath">Путь до сборки.</param>
        /// <remarks>Используется при загрузке сборок.</remarks>
        public static void AddSearchPath(string p_sPath)
        {
            if (String.IsNullOrEmpty(p_sPath))
            {
                throw new ArgumentNullException("p_sPath");
            }

            if (s_objAssemblySearchPaths.Contains(p_sPath) == false)
            {
                if (Path.IsPathRooted(p_sPath))
                {
                    s_objAssemblySearchPaths.Add(p_sPath);
                }
                else
                {
                    s_objAssemblySearchPaths.Add(Path.Combine(m_sApplicationPath, p_sPath));
                }
            }
        }


        //----------------------------------------------------------------------------
        public static StringCollection SearchPaths
        {
            get
            {
                return s_objAssemblySearchPaths;
            }
            set
            {
                s_objAssemblySearchPaths = value;
            }
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Обработчик события о загрузке сборки. См. <see cref="AppDomain.AssemblyResolve "/>
        /// </summary>
        /// <param name="p_oSender">Источник события</param>
        /// <param name="p_objEventArgs">Аргументы.</param>
        /// <returns>Сборка.</returns>
        /// <remarks>перехватив это события можно управлять загрузкой сборок в системе.</remarks>
        public static Assembly SearchAssemblyEventHandler(object p_oSender, ResolveEventArgs p_objEventArgs)
        {
            //string sAssemblyName = p_objEventArgs.Name;
            Assembly objAssembly = ReflectionUtils.LoadAssembly(p_objEventArgs.Name, true);
            return objAssembly;
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Загружает сборку из указаных мест (<see cref="AddSearchPath"/>).
        /// </summary>
        /// <param name="p_sFileName">Путь до сборки.</param>
        /// <returns>Сборку</returns>
        public static Assembly SearchAssembly(string p_sFileName)
        {
            return ReflectionUtils.SearchAssembly(p_sFileName, false);
        }

        public static string GetAssemblyPath(string p_sFileName)
        {
            string path = "";
            string sAssemblyName = ReflectionUtils.GetAssemblyName(p_sFileName, true);

            foreach (string sPath in s_objAssemblySearchPaths)
            {
                string sDirectory = sPath;
                if (ReflectionUtils.IsPathRelative(sDirectory))
                {
                    sDirectory = Path.Combine(m_sApplicationPath, sDirectory);
                }
                string sRealPath = Path.Combine(sDirectory, sAssemblyName);
                if (!File.Exists(sRealPath))
                {
                    continue;
                }
                path = sRealPath;
                break;
            }
            return path;
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Загружает сборку из указаных мест (<see cref="AddSearchPath"/>).
        /// </summary>
        /// <param name="p_sAssemblyPathORFullName">Путь до сборки иди полное имя сборки</param>
        /// <param name="p_bFromLoadMethod"></param>
        /// <returns>Сборку</returns>
        protected static Assembly SearchAssembly(string p_sFileName, bool p_bFromLoadMethod)
        {
            if (String.IsNullOrEmpty(p_sFileName))
            {
                throw new ArgumentNullException("p_sFileName");
            }

            Assembly objAssembly = null;
            Assembly objLoadedAssembly = null;
            if (p_bFromLoadMethod == false)
            {
                Monitor.Enter(s_oSyncRoot);
            }
            try
            {
                /*
				Assembly objLoadedAssembly = ReflectionUtils.CheckAssemblyLoaded( p_sFileName );
				objAssembly = objLoadedAssembly;				
				if( !ReflectionUtils.NormalLoad && objAssembly != null )
				{
					objAssembly = null; //force search and load new version
				}
				*/
                objLoadedAssembly = ReflectionUtils.CheckAssemblyLoaded(p_sFileName);
                if (ReflectionUtils.NormalLoad)
                {
                    objAssembly = ReflectionUtils.CheckAssemblyLoaded(p_sFileName);
                }
                // search assembly in all paths
                if (objAssembly == null)
                {
                    string sAssemblyName = ReflectionUtils.GetAssemblyName(p_sFileName, true);
                    // application path
                    foreach (string sPath in s_objAssemblySearchPaths)
                    {
                        string sDirectory = sPath;
                        if (ReflectionUtils.IsPathRelative(sDirectory))
                        {
                            sDirectory = Path.Combine(m_sApplicationPath, sDirectory);
                        }
                        string sRealPath = Path.Combine(sDirectory, sAssemblyName);
                        if (!File.Exists(sRealPath))
                        {
                            continue;
                        }

                        try
                        {
                            if (ReflectionUtils.NormalLoad)
                            {
                                objAssembly = ReflectionUtils.NormalLoadAssembly(sRealPath);
                                break;
                            }

                            if (objLoadedAssembly == null)
                            {
                                objAssembly = ReflectionUtils.RawLoadAssembly(sRealPath);
                            }
                            else
                            {
                                AssemblyName objAssemblyNameLoaded = objLoadedAssembly.GetName();
                                bool bIsNeedRawLoad = ReflectionUtils.IsNeedRawLoad(objAssemblyNameLoaded, sRealPath);
                                if (bIsNeedRawLoad)
                                {
                                    objAssembly = ReflectionUtils.RawLoadAssembly(sRealPath);
                                }
                                else
                                {
                                    objAssembly = objLoadedAssembly;
                                }
                                /*
								AssemblyName objAssemblyName = AssemblyName.GetAssemblyName( sRealPath );
								if( objAssemblyName.Version > objAssemblyNameLoaded.Version )
								{
									objAssembly = ReflectionUtils.RawLoadAssembly( sRealPath );
								}
								*/
                            }
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            finally
            {
                if (p_bFromLoadMethod == false)
                {
                    Monitor.Exit(s_oSyncRoot);
                }
            }

            return objAssembly;
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Отдаёт сборку если она уже загружена.
        /// </summary>
        /// <param name="p_sAssemblyName">Имя ассембли или путь до неё.</param>
        /// <returns>null если сборка не загружена.</returns>
        public static Assembly CheckAssemblyLoaded(string p_sAssemblyName)
        {
            if (String.IsNullOrEmpty(p_sAssemblyName))
            {
                throw new ArgumentNullException("p_sAssemblyName");
            }
            string sAssemblyName = Path.GetFileName(p_sAssemblyName);

            Assembly objAssembly = null;
            AssemblyName objAssemblyName = null;

            sAssemblyName = GetAssemblyName(sAssemblyName, false);
            foreach (Assembly objLoadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string sName = objLoadedAssembly.FullName;
                int iComma = sName.IndexOf(',');
                if (iComma > 0)
                {
                    sName = sName.Substring(0, iComma);
                }
                bool bFullNameEquals = (string.Compare(sAssemblyName, sName, true) == 0);
                if (bFullNameEquals)
                {
                    if (objAssembly == null)
                    {
                        objAssembly = objLoadedAssembly;
                    }
                    else
                    {
                        // Находим самую последнюю версию загруженную в память
                        AssemblyName objAssemblyNameLoaded = objLoadedAssembly.GetName();
                        objAssemblyName = objAssembly.GetName();
                        if (objAssemblyName.Version < objAssemblyNameLoaded.Version)
                        {
                            objAssembly = objLoadedAssembly;
                        }
                    }
                }
            }
            return objAssembly;
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Check if need to load assembly
        /// </summary>
        /// <param name="p_objAssemblyNameLoaded">AssemblyName</param>
        /// <param name="p_sAssemblyFile">p_sAssemblyFile</param>
        /// <returns>Result</returns>
        protected static bool IsNeedRawLoad(AssemblyName p_objAssemblyNameLoaded, string p_sAssemblyFile)
        {
            AssemblyName objAssemblyName = AssemblyName.GetAssemblyName(p_sAssemblyFile);
            if (objAssemblyName.Version > p_objAssemblyNameLoaded.Version)
            {
                return true;
            }
            string sName = objAssemblyName.Name;
            // check by our hashtable times!
            if (s_htRawAssemblyTimeInfo.Contains(sName))
            {
                DateTime dtLastWriteTime = (DateTime)s_htRawAssemblyTimeInfo[sName];
                FileInfo fifMyFile = new FileInfo(p_sAssemblyFile);
                if (fifMyFile.LastWriteTime > dtLastWriteTime)
                {
                    return true;
                }
            }
            return false;
        }

        //----------------------------------------------------------------------------
        protected static string GetAssemblyName(string p_sAssemblyName, bool p_bWithExtenstion)
        {
            string sAssemblyName = Path.GetFileName(p_sAssemblyName);
            int iComma = sAssemblyName.IndexOf(',');
            if (iComma > 0)
            {
                sAssemblyName = sAssemblyName.Substring(0, iComma);
            }
            if (!p_bWithExtenstion && sAssemblyName.EndsWith(".dll"))
            {
                sAssemblyName = sAssemblyName.Substring(0, sAssemblyName.IndexOf(".dll"));
            }
            else if (p_bWithExtenstion && !sAssemblyName.EndsWith(".dll"))
            {
                sAssemblyName = sAssemblyName + ".dll";
            }
            return sAssemblyName;
        }


        //----------------------------------------------------------------------------
        /// <summary>
        /// Проверяет является ли переданный путь относительным.
        /// </summary>
        /// <param name="p_sPath">Путь.</param>
        /// <returns>true если путь относительный.</returns>
        protected static bool IsPathRelative(string p_sPath)
        {
            if (String.IsNullOrEmpty(p_sPath))
            {
                throw new ArgumentNullException("p_sPath");
            }

            if (p_sPath[0] != '/' && p_sPath[0] != '\\' && p_sPath.IndexOf(':') == -1)
            {
                return true;
            }

            return false;
        }
    }
}