using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class LoaderTests
    {
        private string _tempDirectory;
        private Engine _eng;
        private Application _app;
        private Loader _ldr;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            void mkdir(string subDirectory1)
            {
                Trace.TraceInformation("Create sub-directory {0}", subDirectory1);
                Directory.CreateDirectory(subDirectory1);
            }

            void touch(string s)
            {
                Trace.TraceInformation("Create file {0}", s);
                using (File.Create(s))
                {
                }
            }

            _tempDirectory = Path.Combine(Path.GetTempPath(), $"PrexoniteTests_LoaderTests_{Guid.NewGuid()}");
            Trace.TraceInformation("Create test directory {0}", _tempDirectory);
            Directory.CreateDirectory(_tempDirectory);

            var rootFile = Path.Combine(_tempDirectory, "root.pxs");
            touch(rootFile);

            var subDirectory = Path.Combine(_tempDirectory, "sub1");
            mkdir(subDirectory);
            touch(Path.Combine(subDirectory, "sub1.pxs"));
            
            var subSubDirectory = Path.Combine(subDirectory, "sub2");
            mkdir(subSubDirectory);
            touch(Path.Combine(subSubDirectory, "sub2.pxs"));
            
            // Creates a directory structure that looks like this:
            //
            // /
            // |- root.pxs
            // `- sub1/
            //    |- sub1.pxs
            //    `- sub2/
            //       `- sub2.pxs
            
            _eng = new Engine();
            _app = new Application();
            _ldr = new Loader(_eng, _app);
            _ldr.LoadPaths.Push(_tempDirectory);
        }

        [OneTimeTearDown]
        public void TearDownFixture()
        {
            if (_tempDirectory == null) 
                return;
            
            Trace.TraceInformation("Deleting test directory {0}", _tempDirectory);
            Directory.Delete(_tempDirectory, true);
        }

        [Test]
        public void Root()
        {
            _assertFindsFile("root.pxs", "root.pxs");
        }
        
        [Test]
        public void RootRelative()
        {
            _assertFindsFile("./root.pxs", "root.pxs");
        }
        
        [Test]
        public void Sub1ForwardSlash()
        {
            _assertFindsFile("sub1/sub1.pxs", "sub1", "sub1.pxs");
        }

        [Test]
        public void Sub1ForwardSlashRelative()
        {
            _assertFindsFile("./sub1/sub1.pxs", "sub1", "sub1.pxs");
        }
        
        [Test]
        public void Sub2ForwardSlashRelative()
        {
            _assertFindsFile("./sub1/sub2/sub2.pxs", "sub1", "sub2", "sub2.pxs");
        }

        private void _assertFindsFile(string logicalPath, params string[] physicalPathComponents)
        {
            var effectivePathComponents = _tempDirectory.Singleton().Append(physicalPathComponents).ToArray();
            var fileInfo = new FileInfo(Path.Combine(effectivePathComponents));
            var appliedPath = _ldr.ApplyLoadPaths(logicalPath);
            Assert.That(appliedPath, Is.Not.Null, "Loader.ApplyLoadPaths(\"{0}\") should not be null.", logicalPath);
            Assert.That(appliedPath.FullName, Is.EqualTo(fileInfo.FullName));
        }
        
        
    }
}