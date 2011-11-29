using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using Prexonite.Modular;

namespace Prexonite
{
    class ApplicationCompoundImpl : ApplicationCompound
    {
        private class AppTable : KeyedCollection<ModuleName, Application>
        {
            protected override ModuleName GetKeyForItem(Application item)
            {
                return item.Module.Name;
            }
        }

        private readonly KeyedCollection<ModuleName,Application> _table = new AppTable();

        public override IEnumerator<Application> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        public override Application this[ModuleName name]
        {
            get { return _table[name]; }
        }

        internal override void _Unlink(Application application)
        {
            var r = _table.Remove(application);
            Debug.Assert(r,
                "Tried to _Unlink an application that wasn't part of the compound. Probable cause of bugs");
        }

        internal override void _Link(Application application)
        {
            Application current;
            if (TryGetApplication(application.Module.Name, out current))
            {
                if (Equals(current, application))
                {
                    return;  //merging
                }
                else
                {
                    throw new ModuleConflictException(
                        "Attempted to link two instantiations of the same module (or modules wth the same name/version).",
                        application.Module,
                        current.Module);
                }
            }
            _table.Add(application);
        }

        internal override void _Clear()
        {
            _table.Clear();
        }

        public override bool Contains(ModuleName item)
        {
            return _table.Contains(item);
        }

        public override bool TryGetApplication(ModuleName moduleName, out Application application)
        {
            if(_table.Contains(moduleName))
            {
                application = _table[moduleName];
                return true;
            }
            else
            {
                application = null;
                return false;
            }
        }

        public override void CopyTo(Application[] array, int arrayIndex)
        {
            _table.CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get { return _table.Count; }
        }
    }

    [Serializable]
    public class ModuleConflictException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ModuleConflictException()
        {
        }

        public ModuleConflictException(string message, Module module1 = null, Module module2 = null) : base(_appendModules(message,module1, module2))
        {
            Module1 = module1;
            Module2 = module2;
        }

        public ModuleConflictException(string message, Exception inner, Module module1 = null, Module module2 = null)
            : base(_appendModules(message, module1, module2), inner)
        {
            Module1 = module1;
            Module2 = module2;
        }

        protected ModuleConflictException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        private  static string _appendModules(string message, Module m1, Module m2)
        {
            if(m1 == null && m2 == null)
                return message;
            return string.Format("{1}{0}Conflict over module {2}.", 
                Environment.NewLine, 
                message,
                (m1 ?? m2).Name);
        }

        public Module Module1 { get; set; }
        public Module Module2 { get; set; }
    }
}