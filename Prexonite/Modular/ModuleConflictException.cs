using System;
using System.Runtime.Serialization;

namespace Prexonite.Modular
{
    [Serializable]
    public class ModuleConflictException : Exception
    {
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