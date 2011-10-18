// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
            SetUpLoader();
            Runner.SetupTestFile(this, new string[]
                {
                    @"psr\ast.pxs",
                });
            LoadUnitTestingFramework();
            Runner.PrepareTestCompilation(this);
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
            SetUpLoader();
            Runner.SetupTestFile(this, new string[]
                {
                    @"psr\pattern.pxs",
                    @"psr\prop.pxs",
                    @"psr\test\meta_macro.pxs",
                    @"psr\macro.pxs",
                });
            LoadUnitTestingFramework();
            Runner.PrepareTestCompilation(this);
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
            SetUpLoader();
            Runner.SetupTestFile(this, new string[]
                {
                    @"psr\macro.pxs",
                    @"psr\test\meta_macro.pxs",
                });
            LoadUnitTestingFramework();
            Runner.PrepareTestCompilation(this);
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
        public void test_macro_id_static()
        {
            RunUnitTest(@"test_macro_id_static");
        }

        [Test]
        public void test_macro_id()
        {
            RunUnitTest(@"test_macro_id");
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
    }

    public abstract class Unit_misc : ScriptedUnitTestContainer
    {
        [TestFixtureSetUp]
        public void SetupTestFile()
        {
            SetUpLoader();
            Runner.SetupTestFile(this, new string[]
                {
                    @"psr\misc.pxs",
                });
            LoadUnitTestingFramework();
            Runner.PrepareTestCompilation(this);
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
            SetUpLoader();
            Runner.SetupTestFile(this, new string[]
                {
                    @"psr\struct.pxs",
                    @"psr\set.pxs",
                    @"psr\queue.pxs",
                    @"psr\stack.pxs",
                });
            LoadUnitTestingFramework();
            Runner.PrepareTestCompilation(this);
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