/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.IO;
#if Compress
using System.IO.Compression;
#endif
using System.Text;
using Prexonite.Commands;
using Prexonite.Compiler.Ast;
using Prexonite.Helper;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    public class Loader : StackContext
    {
        #region Static

        #endregion

        #region Construction

        [NoDebug()]
        public Loader(Engine parentEngine, Application targetApplication)
            : this(new LoaderOptions(parentEngine, targetApplication))
        {
        }

        [NoDebug]
        public Loader(LoaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            _options = options;

            _symbols = new SymbolTable<SymbolEntry>();

            _functionTargets = new SymbolTable<CompilerTarget>();
            _functionTargetsIterator = new FunctionTargetsIterator(this);

            CreateFunctionTarget(ParentApplication._InitializationFunction, new AstBlock("~NoFile", -1, -1));

            if (options.RegisterCommands)
                RegisterCommands();
        }

        public void RegisterCommands()
        {
            foreach (KeyValuePair<string, PCommand> kvp in ParentEngine.Commands)
                Symbols.Add(kvp.Key, new SymbolEntry(SymbolInterpretations.Command, kvp.Key));
        }

        #endregion

        #region Options

        private LoaderOptions _options;

        public LoaderOptions Options
        {
            [NoDebug()]
            get { return _options; }
        }

        #endregion

        #region Symbol Table

        private SymbolTable<SymbolEntry> _symbols;

        public SymbolTable<SymbolEntry> Symbols
        {
            [NoDebug()]
            get { return _symbols; }
        }

        #endregion

        #region Function Symbol Tables

        private SymbolTable<CompilerTarget> _functionTargets;
        private FunctionTargetsIterator _functionTargetsIterator;

        public FunctionTargetsIterator FunctionTargets
        {
            [NoDebug()]
            get { return _functionTargetsIterator; }
        }

        [NoDebug()]
        public class FunctionTargetsIterator
        {
            private readonly Loader outer;

            internal FunctionTargetsIterator(Loader outer)
            {
                this.outer = outer;
            }

            public int Count
            {
                get { return outer._functionTargets.Count; }
            }

            public CompilerTarget this[string key]
            {
                get { return outer._functionTargets[key]; }
            }

            public CompilerTarget this[PFunction key]
            {
                get { return outer._functionTargets[key.Id]; }
            }
        }

        [NoDebug()]
        public CompilerTarget CreateFunctionTarget(PFunction func, AstBlock block)
        {
            if (func == null) throw new ArgumentNullException("func");
            if (_functionTargets.ContainsKey(func.Id))
                throw new ArgumentException("A symbol table for the function " + func.Id + " has already been created");
            CompilerTarget target = new CompilerTarget(this, func, block);
            _functionTargets.Add(func.Id, target);
            return target;
        }

        #endregion

        #region Compilation

        [NoDebug]
        private void LoadFromStream(Stream str, string filePath)
        {
#if Compression
            if(!str.CanSeek)
                goto noCompression;
            
            byte[] buffer = new byte[6];
            if(str.Read(buffer,0,6) != 6)
                goto noCompression;

            string header = System.Text.Encoding.ASCII.GetString(buffer);
            if(!header.Substring(0,5).Equals("//PXS",StringComparison.InvariantCultureIgnoreCase))
                goto noCompression;

            if(Char.ToUpperInvariant(header[5]) != 'C')
                goto noCompression;

            //Compressed
            GZipStream zip = new GZipStream(str, CompressionMode.Decompress, true);
            str = zip;

            goto compile;

            noCompression:
            str.Position -= 6;

            compile:
#endif
            Lexer lex = new Lexer(new StreamReader(str, Encoding.UTF8));
            if(filePath != null)
                lex._file = filePath;

            _load(lex);
        }

        [NoDebug]
        public void LoadFromStream(Stream str)
        {
            LoadFromStream(str,null);
        }

        [NoDebug()]
        public void LoadFromFile(string path)
        {
            string oldPath = _loadPath;
            string finalPath = CombineWithLoadPath(path);
            if(!File.Exists(finalPath))
            {
                IList<string> paths = ParentEngine.Paths;
                int i;
                for (i = 0; i < paths.Count; i++)
                {
                    finalPath = Path.Combine(paths[i], path);
                    if(File.Exists(finalPath))
                        break;
                }
                if(i >= paths.Count)
                    throw new FileNotFoundException("Cannot find script file \"" + path + "\".", path);
            }
            _loadedFiles.Add(Path.GetFullPath(finalPath));
            _loadPath = Path.GetDirectoryName(finalPath);
            using (Stream str = new FileStream(finalPath, FileMode.Open))
                LoadFromStream(str, Path.GetFileName(finalPath));

            _loadPath = oldPath;
        }

        [NoDebug()]
        public void LoadFromString(string code)
        {
            _load(new Lexer(new StringReader(code)));
        }

        [NoDebug]
        private void _load(IScanner lexer)
        {
            Parser parser = new Parser(lexer, this);
            LineCatcher lc = new LineCatcher();
            lc.LineCaught += new LineCaughtEventHandler(delegate (object sender, LineCaughtEventArgs o)
                                                        {
                                                            _errors.Add(o.Line);
                                                        });
            parser.errors.errorStream = lc;
            parser.Parse();

            //Compile initialization function
            CompilerTarget target = FunctionTargets[Application.InitializationId];
            target.Ast.EmitCode(target);
            target.Ast.Clear();
            target.FinishTarget();
        }

        
        public int ErrorCount
        {
            [NoDebug]
            get { return _errors.Count; }
        }

        public List<string> Errors
        {
            get { return _errors; }
        }
        private List<string> _errors = new List<string>();

        #endregion

        #region Load Path and file table

        private string _loadPath = Environment.CurrentDirectory;

        public string LoadPath
        {
            get { return _loadPath; }
            set { _loadPath = value; }
        }

        public string CombineWithLoadPath(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            return Path.Combine(_loadPath, path);
        }

        private SymbolCollection _loadedFiles = new SymbolCollection();

        public SymbolCollection LoadedFiles
        {
            get { return _loadedFiles; }
        }

        #endregion

        #region Build Block Commands

        private bool _buildCommandsEnabled = false;

        public bool BuildCommandsEnabled
        {
            get { return _buildCommandsEnabled; }
            set
            {
                if (value != _buildCommandsEnabled)
                    if (value)
                        _enableBuildCommands();
                    else
                        _disableBuildBlockCommands();

                _buildCommandsEnabled = value;
            }
        }

        /// <summary>
        /// The name of the add command in build blocks.
        /// </summary>
        public const string BuildAddCommand = @"\build\add";

        /// <summary>
        /// The name of the require command in build blocks
        /// </summary>
        public const string BuildRequireCommand = @"\build\require";

        /// <summary>
        /// The name of the default command in build blocks
        /// </summary>
        public const string BuildDefaultCommand = @"\build\default";

        /// <summary>
        /// The name of the default script file
        /// </summary>
        public const string DefaultScriptName = "_default.pxs";

        private void _enableBuildCommands()
        {
            ParentEngine.Commands.AddCompilerCommand(BuildAddCommand, 
                delegate(StackContext sctx, PValue[] args)
                  {
                      foreach (PValue arg in args)
                      {
                          string path = arg.CallToString(sctx);
                          LoadFromFile(path);
                      }
                      return null;
                  });

            ParentEngine.Commands.AddCompilerCommand(BuildRequireCommand, 
                delegate(StackContext sctx, PValue[] args)
                  {
                      bool allLoaded = true;
                      foreach (PValue arg in args)
                      {
                          string path =
                              Path.GetFullPath(
                                  CombineWithLoadPath(
                                      arg.CallToString(sctx)));
                          if (_loadedFiles.Contains(path))
                              allLoaded = false;
                          else
                              LoadFromFile(path);
                      }
                      return
                          PType.Bool.CreatePValue(allLoaded);
                  });
            ParentEngine.Commands.AddCompilerCommand(BuildDefaultCommand, 
                delegate {
                      return CombineWithLoadPath(DefaultScriptName);
                  });
        }

        private void _disableBuildBlockCommands()
        {
            ParentEngine.Commands.RemoveCompilerCommands();
        }

        #endregion

        #region Store

        public void StoreInFile(string path)
        {
#if Compress
            if(Options.Compress)
                using(FileStream fstr = new FileStream(path,FileMode.Create,FileAccess.Write,FileShare.None))
                    StoreCompressed(fstr);
            else
#endif
                using (StreamWriter writer = new StreamWriter(path, false))
                    Store(writer);
        }

        public string StoreInString()
        {
            StringWriter writer = new StringWriter();
            Store(writer);
            return writer.ToString();
        }

        public void Store(StringBuilder builder)
        {
            using (StringWriter writer = new StringWriter(builder))
                Store(writer);
        }

#if Compress

        public void StoreCompressed(Stream str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            using (GZipStream zip = new GZipStream(str, CompressionMode.Compress, true))
            using (StreamWriter writer = new StreamWriter(zip, Encoding.UTF8))
            {
                writer.WriteLine("//PXSC");
                Store(writer);
            }
        }

#endif

        public void Store(TextWriter writer)
        {
            Application app = ParentApplication;

            //Header
            writer.WriteLine("//PXS_");
            writer.WriteLine("//--GENERATED");

            //Meta information
            StoreMetaInformation(writer);

            //Global variables
            writer.WriteLine("\n//--GLOBAL VARIABLES");
            foreach (KeyValuePair<string, PVariable> kvp in app.Variables)
            {
                writer.Write("var {0}", kvp.Key);
                if (kvp.Value.Meta.Count > 1)
                {
                    writer.WriteLine("\n[");
                    kvp.Value.Meta.Store(writer);
                    writer.Write("]");
                }
                writer.WriteLine(";");
            }

            //Functions
            writer.WriteLine("\n//--FUNCTIONS");
            app.Functions.Store(writer);

            if(app._InitializationFunction.Code.Count > 0)
                app._InitializationFunction.Store(writer);

            //Symbols
            if (Options.StoreSymbols)
                StoreSymbols(writer);
        }

        /// <summary>
        /// Writes only the symbol declarations to the text writer (regardless of the <see cref="LoaderOptions.StoreSymbols"/> property.)
        /// </summary>
        /// <param name="writer">The writer to write the declarations to.</param>
        public void StoreSymbols(TextWriter writer)
        {
            writer.WriteLine("\n//--SYMBOLS");
            List<KeyValuePair<string, SymbolEntry>> functions = new List<KeyValuePair<string, SymbolEntry>>();
            List<KeyValuePair<string, SymbolEntry>> commands = new List<KeyValuePair<string, SymbolEntry>>();
            List<KeyValuePair<string, SymbolEntry>> objectVariables = new List<KeyValuePair<string, SymbolEntry>>();
            List<KeyValuePair<string, SymbolEntry>> referenceVariables =
                new List<KeyValuePair<string, SymbolEntry>>();

            foreach (KeyValuePair<string, SymbolEntry> kvp in Symbols)
                switch (kvp.Value.Interpretation)
                {
                    case SymbolInterpretations.Function:
                        functions.Add(kvp);
                        break;
                    case SymbolInterpretations.Command:
                        commands.Add(kvp);
                        break;
                    case SymbolInterpretations.GlobalObjectVariable:
                        objectVariables.Add(kvp);
                        break;
                    case SymbolInterpretations.GlobalReferenceVariable:
                        referenceVariables.Add(kvp);
                        break;
                }

            _writeSymbolKind(writer, "function", functions);
            _writeSymbolKind(writer, "command", commands);
            _writeSymbolKind(writer, "var", objectVariables);
            _writeSymbolKind(writer, "ref", referenceVariables);
        }

        /// <summary>
        /// Writes only meta information to the specified stream.
        /// </summary>
        /// <param name="writer">The writer to write meta information to.</param>
        public void StoreMetaInformation(TextWriter writer)
        {
            writer.WriteLine("\n//--META INFORMATION");
            ParentApplication.Meta.Store(writer);
        }

        private static void _writeSymbolKind(TextWriter writer, string kind,
                                             List<KeyValuePair<string, SymbolEntry>> entries)
        {
            if (entries.Count > 0)
            {
                writer.Write("declare ");
                writer.Write(kind);
                writer.Write(" ");
                int idx = 0;
                foreach (KeyValuePair<string, SymbolEntry> kvp in entries)
                {
                    writer.Write(kvp.Value.Id);
                    if (!Engine.StringsAreEqual(kvp.Value.Id, kvp.Key))
                    {
                        writer.Write(" as ");
                        writer.Write(kvp.Key);
                    }
                    if (++idx == entries.Count)
                        writer.WriteLine(";");
                    else
                        writer.Write(",");
                }
            }
        }

        #endregion

        #region Stack Context

        public override Engine ParentEngine
        {
            [NoDebug()]
            get { return _options.ParentEngine; }
        }

        public override PFunction Implementation
        {
            [NoDebug()]
            get { return Options.TargetApplication._InitializationFunction; }
        }

        [NoDebug()]
        protected override bool PerformNextCylce()
        {
            return false;
        }

        public override PValue ReturnValue
        {
            [NoDebug()]
            get { return Options.ParentEngine.CreateNativePValue(Options.TargetApplication); }
        }

        public override bool HandleException(Exception exc)
        {
            //Cannot handle exceptions.
            return false;
        }

        #endregion
    }
}