//------------------------------------------------------------------------------
// <copyright file="CSSqlClassFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Diagnostics;

namespace SQLAutoEnums.Compilers
{
    class CscCompiler : ICompiler
    {
        public CscCompiler()
        {
            //_cscPath = Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v3.5\\csc.exe";
            _cscPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
            _tempPath = Environment.GetEnvironmentVariable("TEMP");
            _codePath = _tempPath + "\\" + Guid.NewGuid() + ".cs";
            _dllPath = _tempPath + "\\" + Guid.NewGuid() + ".dll";            
        }




        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                Status = CompileStatus.NotCompiled;
                CompiledCode = null;
            } 
        }
        private string _code;

        public CompileStatus Status { get; private set; }

        public byte[] CompiledCode { get; private set; }

        public string[] CompilerMessages { get; private set; }


        private readonly string _cscPath;
        private readonly string _tempPath;
        private readonly string _codePath;
        private readonly string _dllPath;


        public int Compile()
        {
            CompilerMessages = new string[0];
            CompiledCode = new byte[0];
            Status = CompileStatus.InProgress;

            try
            {
                File.WriteAllText(_codePath, Code);

                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = _cscPath,
                        Arguments =
                            string.Format("/optimize+ /nologo /preferreduilang:en /target:library /out:\"{0}\" \"{1}\" ", _dllPath, _codePath),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = _tempPath
                    }
                };
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                

                CompilerMessages = new[]
                    {
                        "Compiler exit code = " + p.ExitCode,
                        output
                    };

                if (p.ExitCode == 0)
                {
                    CompiledCode = File.ReadAllBytes(_dllPath);
                    Status = CompileStatus.Success;
                }
                else
                {
                    Status = CompileStatus.Failed;
                }

                return p.ExitCode;
            }
            catch (Exception)
            {
                Status = CompileStatus.Failed;
                throw;
            }
            finally
            {
                File.Delete(_codePath);
                File.Delete(_dllPath);
            }
        }
    }
}
