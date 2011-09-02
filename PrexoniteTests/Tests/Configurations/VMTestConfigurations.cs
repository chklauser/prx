 
// ReSharper disable RedundantUsingDirective
using System;
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
    public class ast_Interpreted : Unit_ast
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
    public class ast_CilStatic : Unit_ast
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
    public class ast_CilIsolated : Unit_ast
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
    public class ast_StoredInterpreted : Unit_ast
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
    public class ast_StoredCilStatic : Unit_ast
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

    /*
    [TestFixture]
    public class ast_StoredCilIsolated : Unit_ast
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
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
    } */

    [TestFixture]
    public class lang_ext_Interpreted : Unit_lang_ext
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
    public class lang_ext_CilStatic : Unit_lang_ext
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
    public class lang_ext_CilIsolated : Unit_lang_ext
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
    public class lang_ext_StoredInterpreted : Unit_lang_ext
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
    public class lang_ext_StoredCilStatic : Unit_lang_ext
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

    /*
    [TestFixture]
    public class lang_ext_StoredCilIsolated : Unit_lang_ext
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
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
    } */

    [TestFixture]
    public class macro_Interpreted : Unit_macro
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
    public class macro_CilStatic : Unit_macro
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
    public class macro_CilIsolated : Unit_macro
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
    public class macro_StoredInterpreted : Unit_macro
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
    public class macro_StoredCilStatic : Unit_macro
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

    /*
    [TestFixture]
    public class macro_StoredCilIsolated : Unit_macro
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
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
    } */

    [TestFixture]
    public class misc_Interpreted : Unit_misc
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
    public class misc_CilStatic : Unit_misc
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
    public class misc_CilIsolated : Unit_misc
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
    public class misc_StoredInterpreted : Unit_misc
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
    public class misc_StoredCilStatic : Unit_misc
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

    /*
    [TestFixture]
    public class misc_StoredCilIsolated : Unit_misc
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
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
    } */

    [TestFixture]
    public class struct_Interpreted : Unit_struct
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
    public class struct_CilStatic : Unit_struct
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
    public class struct_CilIsolated : Unit_struct
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
    public class struct_StoredInterpreted : Unit_struct
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
    public class struct_StoredCilStatic : Unit_struct
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

    /*
    [TestFixture]
    public class struct_StoredCilIsolated : Unit_struct
    {
        private readonly UnitTestConfiguration _runner = new UnitTestConfiguration.FromStored{
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
    } */


    [TestFixture]
    public class VMTests_Interpreted : Prx.Tests.VMTests
    {
        public VMTests_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    public class VMTests_CilStatic : Prx.Tests.VMTests
    {
        public VMTests_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    public class VMTests_CilIsolated : Prx.Tests.VMTests
    {
        public VMTests_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        } 
    }


    [TestFixture]
    public class PartialApplication_Interpreted : PrexoniteTests.Tests.PartialApplication
    {
        public PartialApplication_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    public class PartialApplication_CilStatic : PrexoniteTests.Tests.PartialApplication
    {
        public PartialApplication_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    public class PartialApplication_CilIsolated : PrexoniteTests.Tests.PartialApplication
    {
        public PartialApplication_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        } 
    }


    [TestFixture]
    public class Lazy_Interpreted : PrexoniteTests.Tests.Lazy
    {
        public Lazy_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    public class Lazy_CilStatic : PrexoniteTests.Tests.Lazy
    {
        public Lazy_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    public class Lazy_CilIsolated : PrexoniteTests.Tests.Lazy
    {
        public Lazy_CilIsolated()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        } 
    }


    [TestFixture]
    public class Translation_Interpreted : PrexoniteTests.Tests.Translation
    {
        public Translation_Interpreted()
        {
            CompileToCil = false;
        } 
    }

    [TestFixture]
    public class Translation_CilStatic : PrexoniteTests.Tests.Translation
    {
        public Translation_CilStatic()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        } 
    }

    [TestFixture]
    public class Translation_CilIsolated : PrexoniteTests.Tests.Translation
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


