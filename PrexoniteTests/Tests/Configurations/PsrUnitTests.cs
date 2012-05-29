 
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
        public abstract class Unit_ast : ScriptedUnitTestContainer
        {
            [TestFixtureSetUp]
            public void SetupTestFile()
            {
                Initialize();
                // Units under test
                Runner.SetupUnitsUnderTest(this, new string[] {
                        @"psr\ast.pxs",
                });
                // Testing framework
                LoadUnitTestingFramework();
                // Test dependencies
                Runner.SetupDependencies(this, new string[] {
                                            
                });
                // Potentially isolate test code from rest
                Runner.PrepareTestCompilation(this);
                // Actual test code
                RequireFile(@".\ast.test.pxs");                
                Runner.PrepareExecution(this);
            }

            [Test]
            public void compiler_is_loaded()
            {
                RunUnitTest(@"compiler_is_loaded");
            } 
            [Test]
            public void test_ast_withpos_null()
            {
                RunUnitTest(@"test_ast_withpos_null");
            } 
            [Test]
            public void test_ast_withpos_memcall()
            {
                RunUnitTest(@"test_ast_withpos_memcall");
            } 
            [Test]
            public void test_ast_simple_memcall()
            {
                RunUnitTest(@"test_ast_simple_memcall");
            } 
            [Test]
            public void test_ast_memcall()
            {
                RunUnitTest(@"test_ast_memcall");
            } 
            [Test]
            public void test_unique_id_counter()
            {
                RunUnitTest(@"test_unique_id_counter");
            } 
            [Test]
            public void test_is_function_call()
            {
                RunUnitTest(@"test_is_function_call");
            } 
            [Test]
            public void test_is_member_access()
            {
                RunUnitTest(@"test_is_member_access");
            } 
            [Test]
            public void test_local_meta()
            {
                RunUnitTest(@"test_local_meta");
            } 
            [Test]
            public void test_si_fields()
            {
                RunUnitTest(@"test_si_fields");
            } 
            [Test]
            public void test_si_is_star()
            {
                RunUnitTest(@"test_si_is_star");
            } 
            [Test]
            public void test_si_make_star()
            {
                RunUnitTest(@"test_si_make_star");
            } 
            [Test]
            public void test_si_m_is_star()
            {
                RunUnitTest(@"test_si_m_is_star");
            } 
            [Test]
            public void test_sub_blocks()
            {
                RunUnitTest(@"test_sub_blocks");
            } 
        }
        public abstract class Unit_lang_ext : ScriptedUnitTestContainer
        {
            [TestFixtureSetUp]
            public void SetupTestFile()
            {
                Initialize();
                // Units under test
                Runner.SetupUnitsUnderTest(this, new string[] {
                        @"psr\pattern.pxs",
                        @"psr\prop.pxs",
                        @"psr\macro.pxs",
                });
                // Testing framework
                LoadUnitTestingFramework();
                // Test dependencies
                Runner.SetupDependencies(this, new string[] {
                        @"psr\test\meta_macro.pxs",
                                            
                });
                // Potentially isolate test code from rest
                Runner.PrepareTestCompilation(this);
                // Actual test code
                RequireFile(@".\lang-ext.test.pxs");                
                Runner.PrepareExecution(this);
            }

            [Test]
            public void test_con()
            {
                RunUnitTest(@"test_con");
            } 
            [Test]
            public void test_dcon()
            {
                RunUnitTest(@"test_dcon");
            } 
            [Test]
            public void test_prop_simple()
            {
                RunUnitTest(@"test_prop_simple");
            } 
            [Test]
            public void test_prop_proxy()
            {
                RunUnitTest(@"test_prop_proxy");
            } 
            [Test]
            public void test_prop_complex()
            {
                RunUnitTest(@"test_prop_complex");
            } 
            [Test]
            public void test_prop_simple_glob()
            {
                RunUnitTest(@"test_prop_simple_glob");
            } 
        }
        public abstract class Unit_macro : ScriptedUnitTestContainer
        {
            [TestFixtureSetUp]
            public void SetupTestFile()
            {
                Initialize();
                // Units under test
                Runner.SetupUnitsUnderTest(this, new string[] {
                        @"psr\macro.pxs",
                });
                // Testing framework
                LoadUnitTestingFramework();
                // Test dependencies
                Runner.SetupDependencies(this, new string[] {
                        @"psr\test\meta_macro.pxs",
                                            
                });
                // Potentially isolate test code from rest
                Runner.PrepareTestCompilation(this);
                // Actual test code
                RequireFile(@".\macro.test.pxs");                
                Runner.PrepareExecution(this);
            }

            [Test]
            public void test_file()
            {
                RunUnitTest(@"test_file");
            } 
            [Test]
            public void test_pos()
            {
                RunUnitTest(@"test_pos");
            } 
            [Test]
            public void test_is_in_macro()
            {
                RunUnitTest(@"test_is_in_macro");
            } 
            [Test]
            public void test_establish_macro_context()
            {
                RunUnitTest(@"test_establish_macro_context");
            } 
            [Test]
            public void test_reports()
            {
                RunUnitTest(@"test_reports");
            } 
            [Test]
            public void test_ast_is_expression()
            {
                RunUnitTest(@"test_ast_is_expression");
            } 
            [Test]
            public void test_ast_is_effect()
            {
                RunUnitTest(@"test_ast_is_effect");
            } 
            [Test]
            public void test_ast_is_partially_applicable()
            {
                RunUnitTest(@"test_ast_is_partially_applicable");
            } 
            [Test]
            public void test_ast_is_partial_application()
            {
                RunUnitTest(@"test_ast_is_partial_application");
            } 
            [Test]
            public void test_ast_is_CreateClosure()
            {
                RunUnitTest(@"test_ast_is_CreateClosure");
            } 
            [Test]
            public void test_ast_is_node()
            {
                RunUnitTest(@"test_ast_is_node");
            } 
            [Test]
            public void test_temp()
            {
                RunUnitTest(@"test_temp");
            } 
            [Test]
            public void test_optimize()
            {
                RunUnitTest(@"test_optimize");
            } 
            [Test]
            public void test_read()
            {
                RunUnitTest(@"test_read");
            } 
            [Test]
            public void test_macro_internal_id_static()
            {
                RunUnitTest(@"test_macro_internal_id_static");
            } 
            [Test]
            public void test_macro_internal_id()
            {
                RunUnitTest(@"test_macro_internal_id");
            } 
            [Test]
            public void test_macro_interpretation()
            {
                RunUnitTest(@"test_macro_interpretation");
            } 
            [Test]
            public void test_macro_interpretation_static()
            {
                RunUnitTest(@"test_macro_interpretation_static");
            } 
            [Test]
            public void test_invoke_macro()
            {
                RunUnitTest(@"test_invoke_macro");
            } 
            [Test]
            public void test_ast_symbol()
            {
                RunUnitTest(@"test_ast_symbol");
            } 
            [Test]
            public void test_ast_member()
            {
                RunUnitTest(@"test_ast_member");
            } 
            [Test]
            public void test_ast_const()
            {
                RunUnitTest(@"test_ast_const");
            } 
            [Test]
            public void test_ast_ret()
            {
                RunUnitTest(@"test_ast_ret");
            } 
            [Test]
            public void test_ast_with_arguments()
            {
                RunUnitTest(@"test_ast\with_arguments");
            } 
            [Test]
            public void test_ast_new()
            {
                RunUnitTest(@"test_ast\new");
            } 
            [Test]
            public void test_ast_null()
            {
                RunUnitTest(@"test_ast\null");
            } 
        }
        public abstract class Unit_misc : ScriptedUnitTestContainer
        {
            [TestFixtureSetUp]
            public void SetupTestFile()
            {
                Initialize();
                // Units under test
                Runner.SetupUnitsUnderTest(this, new string[] {
                        @"psr\misc.pxs",
                });
                // Testing framework
                LoadUnitTestingFramework();
                // Test dependencies
                Runner.SetupDependencies(this, new string[] {
                                            
                });
                // Potentially isolate test code from rest
                Runner.PrepareTestCompilation(this);
                // Actual test code
                RequireFile(@".\misc.test.pxs");                
                Runner.PrepareExecution(this);
            }

            [Test]
            public void test_cmp()
            {
                RunUnitTest(@"test_cmp");
            } 
            [Test]
            public void test_cmp_values()
            {
                RunUnitTest(@"test_cmp_values");
            } 
            [Test]
            public void test_cmp_keys()
            {
                RunUnitTest(@"test_cmp_keys");
            } 
            [Test]
            public void test_cmp_with()
            {
                RunUnitTest(@"test_cmp_with");
            } 
            [Test]
            public void test_cmp_then()
            {
                RunUnitTest(@"test_cmp_then");
            } 
            [Test]
            public void test_cmpr()
            {
                RunUnitTest(@"test_cmpr");
            } 
            [Test]
            public void test_ieq()
            {
                RunUnitTest(@"test_ieq");
            } 
            [Test]
            public void test_ieq_any()
            {
                RunUnitTest(@"test_ieq_any");
            } 
            [Test]
            public void test_ieq_all()
            {
                RunUnitTest(@"test_ieq_all");
            } 
            [Test]
            public void test_refeq()
            {
                RunUnitTest(@"test_refeq");
            } 
            [Test]
            public void test_nrefeq()
            {
                RunUnitTest(@"test_nrefeq");
            } 
            [Test]
            public void test_create_terminator()
            {
                RunUnitTest(@"test_create_terminator");
            } 
            [Test]
            public void test_swap()
            {
                RunUnitTest(@"test_swap");
            } 
        }
        public abstract class Unit_struct : ScriptedUnitTestContainer
        {
            [TestFixtureSetUp]
            public void SetupTestFile()
            {
                Initialize();
                // Units under test
                Runner.SetupUnitsUnderTest(this, new string[] {
                        @"psr\struct.pxs",
                        @"psr\set.pxs",
                        @"psr\queue.pxs",
                        @"psr\stack.pxs",
                });
                // Testing framework
                LoadUnitTestingFramework();
                // Test dependencies
                Runner.SetupDependencies(this, new string[] {
                                            
                });
                // Potentially isolate test code from rest
                Runner.PrepareTestCompilation(this);
                // Actual test code
                RequireFile(@".\struct.test.pxs");                
                Runner.PrepareExecution(this);
            }

            [Test]
            public void test_struct()
            {
                RunUnitTest(@"test_struct");
            } 
            [Test]
            public void tsm_create()
            {
                RunUnitTest(@"tsm_create");
            } 
            [Test]
            public void tsm_add_remove()
            {
                RunUnitTest(@"tsm_add_remove");
            } 
            [Test]
            public void tsi_create()
            {
                RunUnitTest(@"tsi_create");
            } 
            [Test]
            public void tsi_add_remove()
            {
                RunUnitTest(@"tsi_add_remove");
            } 
            [Test]
            public void tqm_count()
            {
                RunUnitTest(@"tqm_count");
            } 
            [Test]
            public void tqm_peek()
            {
                RunUnitTest(@"tqm_peek");
            } 
            [Test]
            public void tqm_dequeue()
            {
                RunUnitTest(@"tqm_dequeue");
            } 
            [Test]
            public void tqm_enumarte_dequeues()
            {
                RunUnitTest(@"tqm_enumarte_dequeues");
            } 
            [Test]
            public void tqi_create()
            {
                RunUnitTest(@"tqi_create");
            } 
            [Test]
            public void tqi_enqueuedequeue()
            {
                RunUnitTest(@"tqi_enqueuedequeue");
            } 
            [Test]
            public void tqi_nonserial()
            {
                RunUnitTest(@"tqi_nonserial");
            } 
            [Test]
            public void tm_count()
            {
                RunUnitTest(@"tm_count");
            } 
            [Test]
            public void tm_peek()
            {
                RunUnitTest(@"tm_peek");
            } 
            [Test]
            public void tm_pop()
            {
                RunUnitTest(@"tm_pop");
            } 
            [Test]
            public void tm_enumarte_pops()
            {
                RunUnitTest(@"tm_enumarte_pops");
            } 
            [Test]
            public void ti_create()
            {
                RunUnitTest(@"ti_create");
            } 
            [Test]
            public void ti_pushpop()
            {
                RunUnitTest(@"ti_pushpop");
            } 
            [Test]
            public void ti_nonserial()
            {
                RunUnitTest(@"ti_nonserial");
            } 
        }
}

// ReSharper restore RedundantExplicitArrayCreation
// ReSharper restore InconsistentNaming

