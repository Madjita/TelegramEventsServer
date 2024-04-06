using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoggerNamespace.Enums
{
    //================================================================================
    /// <summary>
    /// Тип сообщения выводимого в логгер.
    /// </summary>
    [Flags]
    public enum MessageType
    {
        /// <summary>
        /// никакого вывода
        /// </summary>
        Nothing = 0,
        /// <summary>
        /// просто информация например сообщение что из кэша удаляется всё что зависит от guid'a
        /// </summary>
        Info = 1,
        /// <summary>
        /// расширенная информация. например, результаты каких команд затронуло удаления информации из кэша
        /// </summary>
        Verbose = 2,
        /// <summary>
        /// Предупреждения.
        /// </summary>
        Warning = 4,
        /// <summary>
        ///  Сообщения об ошибках
        /// </summary>
        Error = 8,
        /// <summary>
        ///  Отладочная информация
        /// </summary>
        Debug = 16,
        /// <summary>
        ///  Все сообщения
        /// </summary>
        Everything = 31
    }

}
