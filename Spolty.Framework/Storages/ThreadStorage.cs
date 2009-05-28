using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Spolty.Framework.Storages
{
    public sealed class ThreadStorage
    {
        private const string TCSStorageName = "TCS";

        #region Private Fields

        private readonly Dictionary<string, object> _objects = new Dictionary<string, object>();

        #endregion Private Fields

        #region Public Methods

        public static ThreadStorage Current
        {
            get
            {
                ThreadStorage cur = (ThreadStorage) LoadData(TCSStorageName);
                if (cur == null)
                {
                    cur = new ThreadStorage();
                    SaveData(TCSStorageName, cur);
                }
                return cur;
            }
        }

        #endregion Public Methods

        #region Public Properties

        public object this[string index]
        {
            get
            {
                object result;
                _objects.TryGetValue(index, out result);
                return result;
            }
            set
            {
                _objects.Remove(index);
                _objects.Add(index, value);
            }
        }

        #endregion Public Properties

        #region Private Methods

        private static object LoadData(string storagename)
        {
            return HttpContext.Current != null
                       ? HttpContext.Current.Items[storagename]
                       : CallContext.GetData(storagename);
        }

        private static void SaveData(string storagename, object data)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[storagename] = data;
            }
            else
            {
                CallContext.SetData(storagename, data);
            }
        }

        #endregion Private Methods
    }
}