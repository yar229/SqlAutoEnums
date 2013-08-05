//------------------------------------------------------------------------------
// <copyright file="CSSqlClassFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
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

        public CompileStatus Status
        {
            get { return _status; }
            private set { _status = value; }
        }
        private CompileStatus _status;

        public byte[] CompiledCode
        {
            get { return _compiledCode;  }
            private set
            {
                _compiledCode = value;
            }
        }
        private byte[] _compiledCode;

        public string[] CompilerMessages
        {
            get { return _compilerMessages; }
            private set
            {
                _compilerMessages = value;
            }
        }
        private string[] _compilerMessages;




        private string _cscPath;
        private string _tempPath;
        private string _codePath;
        private string _dllPath;


        public int Compile()
        {
            CompilerMessages = new string[0];
            CompiledCode = new byte[0];
            Status = CompileStatus.InProgress;

            try
            {
                File.WriteAllText(_codePath, Code);

                Process p = new Process();
                p.StartInfo.FileName = _cscPath;
                p.StartInfo.Arguments = string.Format("/optimize+ /nologo /preferreduilang:en /target:library /out:\"{0}\" \"{1}\" ", _dllPath, _codePath);
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WorkingDirectory = _tempPath;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                

                CompilerMessages = new string[2]
                    {
                        "Compiler exit code = " + p.ExitCode.ToString(),
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
            catch (System.Exception)
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
