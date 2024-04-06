using Microsoft.Extensions.Logging;
using MyLoggerNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Workers
{
    public abstract class WorkerFilter
    {
        private bool _IsSingleton = true;
        private MyLoggerNamespace.Logger _Logger = null;
        //private List<RewriteRule> _RequestRules = new List<RewriteRule>();
        //public List<RewriteRule> RequestRules { get { return _RequestRules; } }
        //public void InitFilter(List<RewriteRule> RequestRules)
        //{
        //    this._RequestRules = RequestRules;
        //}
        public virtual MyLoggerNamespace.Logger Logger
        {
            set { _Logger = value; }
            get { return _Logger; }
        }
        public virtual void Init(XmlNode p_xndConfig) { }
        public virtual bool IsSingleton
        {
            get { return _IsSingleton; }
            set { _IsSingleton = value; }
        }
        public virtual void Start() { }
        public virtual void Stop() { }
    }
}
