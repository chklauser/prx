// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using NUnit.Framework;
using Prexonite.Compiler.Cil;
using Prx.Tests;

// ReSharper restore RedundantUsingDirective

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming

namespace PrexoniteTests.Tests.Configurations
{
    [TestFixture]
    public class ast_Interpreted : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class ast_CilStatic : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class ast_CilIsolated : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {
                CompileToCil = true,
                Linking = FunctionLinking.FullyIsolated
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class ast_StoredInterpreted : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class ast_StoredCilStatic : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }


    [TestFixture]
    public class ast_StoredCilIsolated : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {
                CompileToCil = true,
                Linking = FunctionLinking.JustAvailableForLinking
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class lang_ext_Interpreted : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class lang_ext_CilStatic : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class lang_ext_CilIsolated : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {
                CompileToCil = true,
                Linking = FunctionLinking.FullyIsolated
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class lang_ext_StoredInterpreted : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class lang_ext_StoredCilStatic : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }


    [TestFixture]
    public class lang_ext_StoredCilIsolated : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {
                CompileToCil = true,
                Linking = FunctionLinking.JustAvailableForLinking
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class macro_Interpreted : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class macro_CilStatic : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class macro_CilIsolated : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {
                CompileToCil = true,
                Linking = FunctionLinking.FullyIsolated
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class macro_StoredInterpreted : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class macro_StoredCilStatic : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }


    [TestFixture]
    public class macro_StoredCilIsolated : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {
                CompileToCil = true,
                Linking = FunctionLinking.JustAvailableForLinking
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class misc_Interpreted : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class misc_CilStatic : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class misc_CilIsolated : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {
                CompileToCil = true,
                Linking = FunctionLinking.FullyIsolated
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class misc_StoredInterpreted : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class misc_StoredCilStatic : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }


    [TestFixture]
    public class misc_StoredCilIsolated : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {
                CompileToCil = true,
                Linking = FunctionLinking.JustAvailableForLinking
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class struct_Interpreted : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class struct_CilStatic : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class struct_CilIsolated : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory
            {
                CompileToCil = true,
                Linking = FunctionLinking.FullyIsolated
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class struct_StoredInterpreted : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }

    [TestFixture]
    public class struct_StoredCilStatic : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {CompileToCil = true};

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }


    [TestFixture]
    public class struct_StoredCilIsolated : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored
            {
                CompileToCil = true,
                Linking = FunctionLinking.JustAvailableForLinking
            };

        protected override UnitTestConfiguration Runner
        {
            get { return _runner; }
        }
    }


    [TestFixture]
    public class VMTests_Interpreted : VMTests
    {
        public VMTests_Interpreted()
        {
            CompileToCil = false;
        }
    }

    [TestFixture]
    public class VMTests_CilStatic : VMTests
    {
        public VMTests_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        }
    }

    [TestFixture]
    public class VMTests_CilIsolated : VMTests
    {
        public VMTests_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        }
    }


    [TestFixture]
    public class PartialApplication_Interpreted : PartialApplication
    {
        public PartialApplication_Interpreted()
        {
            CompileToCil = false;
        }
    }

    [TestFixture]
    public class PartialApplication_CilStatic : PartialApplication
    {
        public PartialApplication_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        }
    }

    [TestFixture]
    public class PartialApplication_CilIsolated : PartialApplication
    {
        public PartialApplication_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        }
    }


    [TestFixture]
    public class Lazy_Interpreted : Lazy
    {
        public Lazy_Interpreted()
        {
            CompileToCil = false;
        }
    }

    [TestFixture]
    public class Lazy_CilStatic : Lazy
    {
        public Lazy_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        }
    }

    [TestFixture]
    public class Lazy_CilIsolated : Lazy
    {
        public Lazy_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        }
    }


    [TestFixture]
    public class Translation_Interpreted : Translation
    {
        public Translation_Interpreted()
        {
            CompileToCil = false;
        }
    }

    [TestFixture]
    public class Translation_CilStatic : Translation
    {
        public Translation_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        }
    }

    [TestFixture]
    public class Translation_CilIsolated : Translation
    {
        public Translation_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        }
    }
}

// ReSharper enable RedundantExplicitArrayCreation
// ReSharper enable InconsistentNaming