namespace Utils
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    //=================================================================================================
    /// <summary>
    /// Создаёт сообщение об ошибке на основании данных из ресурсов (по ключу) и строки форматирования.
    /// </summary>
    [Serializable]
    public class ExException : Exception
    {
        /// <summary>Текст для показа кода ошибки.</summary>
        protected static readonly string s_sErrorCodeText = "ErrorCode: ";

        /// <summary>Разделитель для показа вложенного исключения.</summary>
        protected static readonly string s_sEndOfInnerException = "   --------- End of Inner Exception ---------------";

        //---------------------------------------------------------------------------------------------

        /// <summary>Ключ ресурсов.</summary>
        protected string m_sResourceKey;

        /// <summary>Критическое ли исключение.</summary>
        protected bool m_bIsCritical;

        /// <summary>Текст по-умолчанию.</summary>
        protected string m_sDefaultText;

        /// <summary>Код ошибки.</summary>
        protected int m_iErrorCode;

        /// <summary>Параметры.</summary>
        protected object[] m_aoParams;

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конструктор для десериализации.
        /// </summary>
        public ExException(SerializationInfo p_objInfo, StreamingContext p_objContext)
            : base(p_objInfo, p_objContext)
        {
            m_sResourceKey = (string)p_objInfo.GetValue("m_sResourceKey", typeof(string));
            m_bIsCritical = (bool)p_objInfo.GetValue("m_bIsCritical", typeof(bool));
            m_sDefaultText = (string)p_objInfo.GetValue("m_sDefaultText", typeof(string));
            m_iErrorCode = (int)p_objInfo.GetValue("m_iErrorCode", typeof(int));
            m_aoParams = (object[])p_objInfo.GetValue("m_aoParams", typeof(object[]));
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Сериализация.
        /// </summary>
        public override void GetObjectData(SerializationInfo p_objInfo, StreamingContext p_objContext)
        {
            base.GetObjectData(p_objInfo, p_objContext);

            p_objInfo.AddValue("m_sResourceKey", m_sResourceKey);
            p_objInfo.AddValue("m_bIsCritical", m_bIsCritical);
            p_objInfo.AddValue("m_sDefaultText", m_sDefaultText);
            p_objInfo.AddValue("m_iErrorCode", m_iErrorCode);
            p_objInfo.AddValue("m_aoParams", m_aoParams);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="p_sResourceKey">Ключ ресурса с описанием ошибки.</param>
        /// <param name="p_sDefaultText">Форматная строка с текстом ошибки по-умолчанию. Используется если не найдено описание ошибки в ресурсах.</param>
        /// <param name="p_aoParams">Параметры форматной строки стекстом ошибки по умолчанию.</param>
        public ExException(string p_sResourceKey, string p_sDefaultText, params object[] p_aoParams)
            : this(null, p_sResourceKey, 0, true, p_sDefaultText, p_aoParams)
        {
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="p_sResourceKey">Ключ ресурса с описанием ошибки.</param>
        /// <param name="p_iErrorCode">Код ошибки.</param>
        /// <param name="p_bIsCritical">Является ли ошибка критической.</param>
        /// <param name="p_sDefaultText">Форматная строка с текстом ошибки по-умолчанию. Используется если не найдено описание ошибки в ресурсах.</param>
        /// <param name="p_aoParams">Параметры форматной строки стекстом ошибки по умолчанию.</param>
        public ExException(string p_sResourceKey, int p_iErrorCode, bool p_bIsCritical, string p_sDefaultText, params object[] p_aoParams)
            : this(null, p_sResourceKey, p_iErrorCode, p_bIsCritical, p_sDefaultText, p_aoParams)
        {
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="p_objInnerException">Перехваченное исключение.</param>
        /// <param name="p_sResourceKey">Ключ ресурса с описанием ошибки.</param>
        /// <param name="p_sDefaultText">Форматная строка с текстом ошибки по-умолчанию. Используется если не найдено описание ошибки в ресурсах.</param>
        /// <param name="p_aoParams">Параметры форматной строки стекстом ошибки по умолчанию.</param>
        public ExException(Exception p_objInnerException, string p_sResourceKey, string p_sDefaultText, params object[] p_aoParams)
            : this(p_objInnerException, p_sResourceKey, 0, true, p_sDefaultText, p_aoParams)
        {
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="p_objInnerException">Перехваченное исключение.</param>
        /// <param name="p_sResourceKey">Ключ ресурса с описанием ошибки.</param>
        /// <param name="p_iErrorCode">Код ошибки.</param>
        /// <param name="p_bIsCritical">Является ли ошибка критической.</param>
        /// <param name="p_sDefaultText">Форматная строка с текстом ошибки по-умолчанию. Используется если не найдено описание ошибки в ресурсах.</param>
        /// <param name="p_aoParams">Параметры форматной строки стекстом ошибки по умолчанию.</param>
        public ExException(Exception p_objInnerException, string p_sResourceKey, int p_iErrorCode, bool p_bIsCritical, string p_sDefaultText, params object[] p_aoParams)
            : base(p_sResourceKey, p_objInnerException)
        {
            m_sResourceKey = p_sResourceKey;
            m_bIsCritical = p_bIsCritical;
            m_sDefaultText = p_sDefaultText;
            m_iErrorCode = p_iErrorCode;
            m_aoParams = p_aoParams;
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Ключ ресурса с описанием ошибки.
        /// </summary>
        public string ResourceKey
        {
            get { return m_sResourceKey; }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Является ли ошибка критической.
        /// </summary>
        /// <remarks>true по умолчанию.</remarks>
        public bool IsCritical
        {
            get { return m_bIsCritical; }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Форматная строка с текстом ошибки по-умолчанию. Используется если не найдено описание ошибки в ресурсах.
        /// </summary>
        public string DefaultText
        {
            get { return m_sDefaultText; }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Код ошибки.
        /// </summary>
        /// <remarks>0 по умолчанию.</remarks>
        public int ErrorCode
        {
            get { return m_iErrorCode; }
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Параметры форматной строки стекстом ошибки по умолчанию.
        /// </summary>
        public object[] Parameters
        {
            get { return m_aoParams; }
        }

        /*
		//---------------------------------------------------------------------------------------------
		/// <summary>
		/// Создать сообщение об ошибке на основании ресурсов и параметров.
		/// </summary>
		public string MakeMessage( ExResource p_objResource )
		{
			string sExMessage = null;
			string sParamsException = null;

			// process error code
			string sErrorCodeText = null;
			if( m_iErrorCode != 0 )
			{
				if( p_objResource != null )
				{
					sErrorCodeText = p_objResource.ExGetString( "ExException_ErrorCodeText" );
				}

				if( sErrorCodeText == null || sErrorCodeText.Length == 0 )
				{
					sErrorCodeText = s_sErrorCodeText;
				}

				sErrorCodeText += m_iErrorCode.ToString() + Environment.NewLine;
			}

			// use resource key
			if( p_objResource != null )
			{
				sExMessage = p_objResource.ExGetString( m_sResourceKey );
				if( sExMessage != null && sExMessage.Length > 0 )
				{
					if( m_aoParams == null || m_aoParams.Length == 0 )
					{
						return ( sErrorCodeText + sExMessage );
					}

					try
					{
						return ( sErrorCodeText + String.Format( sExMessage, m_aoParams ) );
					}
					catch( Exception exc )
					{
						sParamsException = exc.Message;
					}
				}
			}

			// use default text
			if( m_sDefaultText != null &&
				m_sDefaultText.Length > 0 )
			{
				if( m_aoParams == null || m_aoParams.Length == 0 )
				{
					return ( sErrorCodeText + m_sDefaultText );
				}

				try
				{
					return ( sErrorCodeText + String.Format( m_sDefaultText, m_aoParams ) );
				}
				catch( Exception exc )
				{
					sParamsException = exc.Message;
				}
			}

			// if parameter exception occured
			string sMessage = sErrorCodeText;
			if( m_sResourceKey != null && m_sResourceKey.Length > 0 )
			{
				sMessage += "ResourceKey: " + m_sResourceKey + "; ";
			}
			if( sExMessage != null && sExMessage.Length > 0 )
			{
				sMessage += "ExMessage: " + sExMessage + "; ";
			}
			if( sParamsException != null && sParamsException.Length > 0 )
			{
				sMessage += "ParamsException: " + sParamsException + "; ";
			}
			if( m_aoParams != null )
			{
				for( int i = 0; i < m_aoParams.Length; i++ )
				{
					string sParam = null;
					if( m_aoParams[ i ] is string )
					{
						sParam = (string)m_aoParams[ i ];
					}
					else if( m_aoParams[ i ] == null )
					{
						sParam = "<null>";
					}
					else
					{
						try
						{
							sParam = m_aoParams[ i ].ToString();
						}
						catch( Exception exc )
						{
							sParam = exc.Message;
						}
					}
					sMessage += "Param" + i + ": " + sParam + "; ";
				}
			}

			return sMessage;
		}


		//---------------------------------------------------------------------------------------------
		/// <summary>
		/// Строка сообщения об ошибке.
		/// </summary>
		/// <remarks>Используется <see cref="ExException.MakeMessage"/>. В качестве ресурсов используются <see cref="ExResource.MainResource"/>.</remarks>
		public override string Message
		{
			get
			{
				return this.MakeMessage( ExResource.MainResource );
			}
		}
		*/

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Печатает exception
        /// </summary>
        /// <param name="p_objException">exception</param>
        /// <param name="p_bMain">Основной exception</param>
        /// <returns>Строка с exception</returns>
        protected static string PrintException(Exception p_objException, bool p_bMain)
        {
            if (p_objException == null)
            {
                return "";
            }

            string[] asTrace = new String[13];

            if (p_objException.InnerException != null)
            {
                asTrace[0] = ExException.PrintException(p_objException.InnerException, false);
            }

            asTrace[1] = p_objException.GetType().FullName;

            ExException objExException = p_objException as ExException;
            if (objExException != null)
            {
                asTrace[2] = " (";
                asTrace[3] = objExException.ResourceKey;
                asTrace[4] = ")";
            }

            asTrace[5] = ": ";
            asTrace[6] = p_objException.Message;
            asTrace[7] = Environment.NewLine;
            asTrace[8] = p_objException.StackTrace;
            asTrace[9] = Environment.NewLine;

            asTrace[10] = AppendAssemblyVersions(p_objException);

            if (p_bMain == false)
            {
                asTrace[11] = s_sEndOfInnerException;
                asTrace[12] = Environment.NewLine;
            }

            return String.Concat(asTrace);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Печатает exception
        /// </summary>
        /// <param name="p_objException">exception</param>
        /// <returns>Строка с exception</returns>
        public static string PrintException(Exception p_objException)
        {
            return ExException.PrintException(p_objException, true);
        }

        //---------------------------------------------------------------------------------------------
        /// Returns Message, StackTrace and all Inner Exceptions in revers order
        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            return ExException.PrintException(this, true);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Достаёт ExException из Exception
        /// </summary>
        /// <param name="p_objException">Exception в котором лежит ExException</param>
        /// <returns>ExException или null если нет ExException в Exception</returns>
        public static ExException GetExException(Exception p_objException)
        {
            return ExException.GetExException(p_objException, null, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Есть ли ExException в Exception
        /// </summary>
        /// <param name="p_objException">Exception</param>
        /// <returns>true если есть</returns>
        public static bool IsExException(Exception p_objException)
        {
            return ExException.IsExException(p_objException, null, false);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Достаёт ExException из Exception
        /// </summary>
        /// <param name="p_objException">Exception в котором лежит ExException</param>
        /// <param name="p_sResourceKey">Ключ ExException, если null то первый ExException который встретится</param>
        /// <returns>если есть ExException, удовлетворяющий условиям, то его, в обратном случае null</returns>
        public static ExException GetExException(Exception p_objException, string p_sResourceKey)
        {
            return ExException.GetExException(p_objException, p_sResourceKey, true);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Есть ли ExException в Exception, удовлетворяющий условиям
        /// </summary>
        /// <param name="p_objException">Exception</param>
        /// <param name="p_sResourceKey">Ключ ExException, если null то первый ExException который встретится</param>
        /// <returns>true если есть</returns>
        public static bool IsExException(Exception p_objException, string p_sResourceKey)
        {
            return ExException.IsExException(p_objException, p_sResourceKey, true);
        }


        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Достаёт ExException из Exception
        /// </summary>
        /// <param name="p_objException">Exception в котором лежит ExException</param>
        /// <param name="p_sResourceKey">Ключ ExException, если null то первый ExException который встретится</param>
        /// <param name="p_bExact">Искать точно по ключу (true) или по его началу (false) </param>
        /// <returns>если есть ExException, удовлетворяющий условиям, то его, в обратном случае null</returns>
        public static ExException GetExException(Exception p_objException, string p_sResourceKey, bool p_bExact)
        {
            if (p_objException == null)
            {
                return null;
            }

            Exception objTempExeption = p_objException;
            ExException objExException = null;
            do
            {
                ExException objTempExException = objTempExeption as ExException;
                if (objTempExException != null)
                {
                    if (String.IsNullOrEmpty(p_sResourceKey))
                    {
                        objExException = objTempExException;
                        break;
                    }

                    if (!String.IsNullOrEmpty(objTempExException.ResourceKey))
                    {
                        if ((p_bExact && objTempExException.ResourceKey == p_sResourceKey) ||
                            (p_bExact == false && objTempExException.ResourceKey.StartsWith(p_sResourceKey)))
                        {
                            objExException = objTempExException;
                        }
                    }
                }

                objTempExeption = objTempExeption.InnerException;
            }
            while (objTempExeption != null);

            return objExException;
        }


        public static bool IsHaveInSignature(ExException p_objException, string p_sSignature)
        {
            if (p_objException.ResourceKey.StartsWith(p_sSignature))
            {
                return true;
            }
            return false;
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Есть ли ExException в Exception, удовлетворяющий условиям
        /// </summary>
        /// <param name="p_objException">Exception</param>
        /// <param name="p_sResourceKey">Ключ ExException, если null то первый ExException который встретится</param>
        /// <param name="p_bExact">Искать точно по ключу (true) или по его началу (false) </param>
        /// <returns>true если есть</returns>
        public static bool IsExException(Exception p_objException, string p_sResourceKey, bool p_bExact)
        {
            Exception objExExeption = ExException.GetExException(p_objException, p_sResourceKey, p_bExact);
            return (objExExeption != null);
        }


        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get first exception in stack trace
        /// </summary>
        /// <param name="p_objException"></param>
        /// <returns>First exception</returns>
        public static Exception GetFirstException(Exception p_objException)
        {
            if (p_objException == null)
            {
                throw new ArgumentNullException("p_objException");
            }

            Exception objTempExeption = p_objException;
            while (objTempExeption.InnerException != null)
            {
                objTempExeption = objTempExeption.InnerException;
            }

            return objTempExeption;
        }


        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get first ExException in stack trace
        /// </summary>
        /// <param name="p_objException"></param>
        /// <returns>First ExException</returns>
        public static ExException GetFirstExException(Exception p_objException)
        {
            if (p_objException == null)
            {
                throw new ArgumentNullException("p_objException");
            }

            Exception objTempExeption = p_objException;
            ExException objExException = null;
            do
            {
                ExException objTempExException = objTempExeption as ExException;
                if (objTempExException != null)
                {
                    objExException = objTempExException;
                }

                objTempExeption = objTempExeption.InnerException;
            }
            while (objTempExeption != null);

            return objExException;
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Получение AssemblyVersion и AssemblyFileVersion для идентификации версии сборки, в которой произошло исключение
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>Строка с версиями</returns>
        private static string AppendAssemblyVersions(Exception exception)
        {
            StringBuilder assemblyVersions = new StringBuilder();
            try
            {
                if (exception != null)
                {
                    if (exception.TargetSite != null)
                    {
                        // Get AssemblyVersion
                        AssemblyName assemblyName = exception.TargetSite.Module.Assembly.GetName();
                        if (assemblyName != null)
                        {
                            assemblyVersions.AppendLine(assemblyName.Name + ": AssemblyVersion: " + assemblyName.Version.ToString());
                        }
                        // Get AssemblyFileVersion
                        object[] attributes = Assembly.LoadFrom(exception.TargetSite.Module.FullyQualifiedName).GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                        if (attributes.Length > 0)
                        {
                            assemblyVersions.AppendLine(assemblyName.Name + ": AssemblyFileVersion: " + ((AssemblyFileVersionAttribute)attributes[0]).Version);
                        }
                        exception.Data["assemblyVersions"] = assemblyVersions;
                    }
                    else
                    {
                        if (exception.Data.Contains("assemblyVersions"))
                        {
                            assemblyVersions = (StringBuilder)exception.Data["assemblyVersions"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in ExExeption.PrintException: GetAssemblyVersions: " + e.Message);
            }
            finally
            {
                Debug.WriteLine("finally: " + assemblyVersions.ToString());
            }
            return assemblyVersions.ToString();
        }
    }
}
