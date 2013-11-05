 
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantNameQualifier
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections.Generic;
using Prexonite.Types;
using Prexonite.Compiler.Cil;
using NUnit.Framework;
// ReSharper restore RedundantUsingDirective

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming

namespace PrexoniteTests.Tests.Configurations
{

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class ast_Interpreted : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class ast_CilStatic : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class ast_CilIsolated : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{
            CompileToCil=true,
            Linking = FunctionLinking.FullyIsolated
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class ast_StoredInterpreted : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class ast_StoredCilStatic : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    
    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class ast_StoredCilIsolated : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
            CompileToCil=true,
            Linking = FunctionLinking.JustAvailableForLinking
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class lang_ext_Interpreted : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class lang_ext_CilStatic : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class lang_ext_CilIsolated : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{
            CompileToCil=true,
            Linking = FunctionLinking.FullyIsolated
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class lang_ext_StoredInterpreted : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class lang_ext_StoredCilStatic : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    
    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class lang_ext_StoredCilIsolated : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
            CompileToCil=true,
            Linking = FunctionLinking.JustAvailableForLinking
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class macro_Interpreted : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class macro_CilStatic : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class macro_CilIsolated : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{
            CompileToCil=true,
            Linking = FunctionLinking.FullyIsolated
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class macro_StoredInterpreted : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class macro_StoredCilStatic : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    
    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class macro_StoredCilIsolated : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
            CompileToCil=true,
            Linking = FunctionLinking.JustAvailableForLinking
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class misc_Interpreted : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class misc_CilStatic : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class misc_CilIsolated : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{
            CompileToCil=true,
            Linking = FunctionLinking.FullyIsolated
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class misc_StoredInterpreted : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class misc_StoredCilStatic : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    
    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class misc_StoredCilIsolated : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
            CompileToCil=true,
            Linking = FunctionLinking.JustAvailableForLinking
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class struct_Interpreted : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class struct_CilStatic : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class struct_CilIsolated : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.InMemory{
            CompileToCil=true,
            Linking = FunctionLinking.FullyIsolated
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class struct_StoredInterpreted : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored();
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class struct_StoredCilStatic : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{CompileToCil=true};
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }

    
    [TestFixture]
    [Explicit]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class struct_StoredCilIsolated : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
            CompileToCil=true,
            Linking = FunctionLinking.JustAvailableForLinking
        };
        protected override UnitTestConfiguration Runner
        {
            get 
            {
                return _runner;
            }
        }
    }


    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class VMTests_Interpreted : Prx.Tests.VMTests
    {
        public VMTests_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class VMTests_CilStatic : Prx.Tests.VMTests
    {
        public VMTests_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class VMTests_CilIsolated : Prx.Tests.VMTests
    {
        public VMTests_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        } 
    }


    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class PartialApplication_Interpreted : PrexoniteTests.Tests.PartialApplication
    {
        public PartialApplication_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class PartialApplication_CilStatic : PrexoniteTests.Tests.PartialApplication
    {
        public PartialApplication_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class PartialApplication_CilIsolated : PrexoniteTests.Tests.PartialApplication
    {
        public PartialApplication_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        } 
    }


    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class Lazy_Interpreted : PrexoniteTests.Tests.Lazy
    {
        public Lazy_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class Lazy_CilStatic : PrexoniteTests.Tests.Lazy
    {
        public Lazy_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class Lazy_CilIsolated : PrexoniteTests.Tests.Lazy
    {
        public Lazy_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        } 
    }


    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class Translation_Interpreted : PrexoniteTests.Tests.Translation
    {
        public Translation_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class Translation_CilStatic : PrexoniteTests.Tests.Translation
    {
        public Translation_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    [GeneratedCode("VMTestConfiguration.tt","0.0")]
    internal class Translation_CilIsolated : PrexoniteTests.Tests.Translation
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



