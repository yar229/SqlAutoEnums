//------------------------------------------------------------------------------
// <copyright file="CSSqlClassFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using SQLAutoEnums.Compilers;

namespace SQLAutoEnums
{
    interface ICompiler
    {
        string Code { get; set; }
        int Compile();
        CompileStatus Status { get; }
        byte[] CompiledCode { get; }
    
    }
}
