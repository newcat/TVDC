using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
//Imports Helpers.BitOperation

namespace System.ComponentModel
{
    //NotifyPropertyChanged
    public class NotifyPropertyChanged : INotifyPropertyChanged, ICloneable
    {

        private static bool? _IsInDesignMode = null;
        private static List<string> _HostProcesses = new List<string>(new string[] { "XDesProc", "denev" });
        public static bool IsInDesignMode()
        {
            if (!_IsInDesignMode.HasValue) _IsInDesignMode = _HostProcesses.Contains(Process.GetCurrentProcess().ProcessName);
            return _IsInDesignMode.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool ChangePropIfDifferent<T>(T value, ref T backingField, [CallerMemberName()]string name = "")
        {
            if (object.Equals(backingField, value)) return false;
            backingField = value;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
            return true;
        }

        protected bool ChangePropIfDifferent<T>(T value, ref T backingField, params string[] propNames)
        {
            return ChangePropIfDifferent(value, ref backingField, Array.ConvertAll(propNames, s => new PropertyChangedEventArgs(s)));
        }
        protected bool ChangePropIfDifferent<T>(T value, ref T backingField, params PropertyChangedEventArgs[] props)
        {
            if (object.Equals(backingField, value)) return false;
            backingField = value;
            RaisePropChanged(props);
            return true;
        }
        public void RaisePropChanged(params string[] names)
        {
            RaisePropChanged(Array.ConvertAll(names, name => new PropertyChangedEventArgs(name)));
        }
        public void RaisePropChanged(params PropertyChangedEventArgs[] eventArgses)
        {
            RaisePropChanged((IEnumerable<PropertyChangedEventArgs>)eventArgses);
        }
        public void RaisePropChanged(IEnumerable<PropertyChangedEventArgs> eventArgses)
        {
            VerifyPropertyNames(eventArgses);
            foreach (var e in eventArgses)
            {
                if (PropertyChanged != null) PropertyChanged(this, e);
            }
        }


        private static Dictionary<Type, HashSet<string>> _AllProperties = new Dictionary<Type, HashSet<string>>();
        [Conditional("DEBUG"), DebuggerStepThrough()]
        public void VerifyPropertyNames(IEnumerable<PropertyChangedEventArgs> eventArgses)
        {
            HashSet<string> propNames = null;
            var tp = this.GetType();
            if (!_AllProperties.TryGetValue(tp, out propNames))
            {
                propNames = new HashSet<string>(from pd in this.GetType().GetProperties() select pd.Name);
                _AllProperties.Add(tp, propNames);
            }
            foreach (var pe in eventArgses)
            {
                if (!propNames.Contains(pe.PropertyName))
                    throw new Exception("trying to raise an unknown PropertyChanged");
            }
        }

        /// <summary>Überschreibungen werden auch von ICloneable.Clone() aufgerufen</summary>
        public virtual void CopyTo(object other)
        {
        }
        public object Clone()
        {
            var cl = MemberwiseClone();
            CopyTo(cl);
            return cl;
        }
    }

}
