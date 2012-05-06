using System;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    internal sealed class VersionConflictException : Exception
    {
        private readonly ModuleName _existingModule;
        private readonly ModuleName _newModule;
        private readonly ModuleName _offendingModule;

        public VersionConflictException(ModuleName existingModule, ModuleName newModule, ModuleName offendingModule)
        {
            _existingModule = existingModule;
            _newModule = newModule;
            _offendingModule = offendingModule;
            Data["ExistingModule"] = existingModule;
            Data["NewModule"] = newModule;
            Data["OffendingModule"] = offendingModule;
        }

        public ModuleName ExistingModule
        {
            get { return _existingModule; }
        }

        public ModuleName NewModule
        {
            get { return _newModule; }
        }

        public ModuleName OffendingModule
        {
            get { return _offendingModule; }
        }

        public override string Message
        {
            get
            {
                return
                    String.Format(
                        "Version conflict detected in dependencies of module {0}, concerning module {1}. Existing version {2}, new version {3}.",
                        OffendingModule, ExistingModule.Id, ExistingModule.Version, NewModule.Version);
            }
        }
    }
}