//------------------------------------------------------------------------------
// <copyright file="CSSqlFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using SQLAutoEnums.Compilers;
using SQLAutoEnums.Generators;


public partial class UserDefinedFunctions
{
    [return: SqlFacet(MaxSize = -1)]
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlChars SqlAutoEnumsTryCompile([SqlFacet(MaxSize = -1)]SqlString code)
    {
        CscCompiler csc = CompileIt(code.ToString());

        string msg = string.Empty;
        foreach (string s in csc.CompilerMessages)
        {
            msg += s + "\r\n";
        }

        //msg += code.ToString();
        return new SqlChars(msg.ToCharArray());
    }

    [return: SqlFacet(MaxSize = -1)]
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBinary SqlAutoEnumsCompile([SqlFacet(MaxSize = -1)]SqlString code)
    {
        CscCompiler csc = CompileIt(code.ToString());

        if (csc.Status == CompileStatus.Success)
            return csc.CompiledCode;

        return null;
    }

    [return: SqlFacet(MaxSize = -1)]
    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlChars SqlAutoEnumsGenerate(SqlString tableName, SqlString columnPrefix, SqlString columnName, SqlString columnMember, SqlString columnValue)
    {
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            string qry =
                //                             0              1            2                   3
                string.Format("select {1} as Prefix, {2} as Name, {3} as MemberName, {4} as MemberValue from {0}",
                              tableName, columnPrefix, columnName, columnMember, columnValue);


            var command = new SqlCommand(qry, conn) { CommandType = CommandType.Text };

            var reader = command.ExecuteReader();

            var result = new List<EnumDescriptor>();
            while (reader.Read())
            {
                string prefix = reader[0].ToString();
                string name = reader[1].ToString();
                string memberName = reader[2].ToString();
                int memberValue = reader.GetInt32(3);
                EnumDescriptor ed = Search(result,prefix, name);
                if (null == ed)
                {
                    ed = new EnumDescriptor
                        {
                            Prefix = prefix,
                            Name = name
                        };
                    result.Add(ed);
                }

                ed.Values.Add(new KeyValuePair<string, int>(memberName, memberValue));
            }

            return SqlAutoEnumsGenerateFromList(result);
        }
    }

    private static EnumDescriptor Search(List<EnumDescriptor> list, string prefix, string name )
    {
        foreach (var data in list)
        {
            if (data.Name == name && data.Prefix == prefix) return data;
        }
        return null;
    }

    private static SqlChars SqlAutoEnumsGenerateFromList(List<EnumDescriptor> list)
    {
        var gen = new SimpleGenerator();
        string res = gen.Generate(list);

        return new SqlChars(res.ToCharArray());
    }



    private static CscCompiler CompileIt(string code)
    {
        CscCompiler csc = new CscCompiler()
        {
            Code = code // "public class FooClass { public string Execute() { return \"output!\";}}"
        };
        csc.Compile();

        return csc;
    }


    class EnumMember
    {
        public int ID;
        public string Name;
    }

    [SqlFunction(DataAccess = DataAccessKind.Read, Name = "SqlAutoEnums.EnumMembers_Current", FillRowMethodName = "EnumMembersCurrentFillRow", TableDefinition = "ID INT, Name nvarchar(4000)")]
    public static IEnumerable EnumMembersCurrent(string enumName) 
    {
        using (SqlConnection conn = new SqlConnection("context connection=true"))
        {
            conn.Open();
            string qry = string.Format("select ID, Name from dbo.[{0}.ToList]()", enumName);


            var command = new SqlCommand(qry, conn) { CommandType = CommandType.Text };

            var reader = command.ExecuteReader();

            var result = new List<EnumMember>();
            while (reader.Read())
            {
                EnumMember em = new EnumMember { ID = reader.GetInt32(0), Name = reader[1].ToString() };
                result.Add(em);
            }
            return result;
        }
    }
    public static void EnumMembersCurrentFillRow(Object obj, out int id, out string name) 
    {{
        var data = (EnumMember)obj;
        id = data.ID;
        name = data.Name; 
    }} 


}






//[Microsoft.SqlServer.Server.SqlFunction]
//public static SqlString SQLAutoEnumsGenerateTest()
//{
//    string msg = "";
//    using (Microsoft.CSharp.CSharpCodeProvider foo = new Microsoft.CSharp.CSharpCodeProvider())
//    {

//        var cr = foo.CompileAssemblyFromSource(
//            new System.CodeDom.Compiler.CompilerParameters()
//                {
//                    GenerateInMemory = true
//                },

//            );

//        // If errors occurred during compilation, output the compiler output and errors. 
//        if (cr.Errors.Count > 0)
//        {
//            for (int i = 0; i < cr.Output.Count; i++)
//                Console.WriteLine(cr.Output[i]);
//            for (int i = 0; i < cr.Errors.Count; i++)
//                Console.WriteLine(i.ToString() + ": " + cr.Errors[i].ToString());

//        }
//        else
//        {
//            msg += "\r\n" + "Compiler returned with result code: " + cr.NativeCompilerReturnValue.ToString();
//            msg += "\r\n" + "Generated assembly name: " + cr.CompiledAssembly.FullName;
//            if (cr.PathToAssembly == null) msg += "\r\n" + "The assembly has been generated in memory.";
//            else msg += "\r\n" + "Path to assembly: " + cr.PathToAssembly;

//            if (!cr.TempFiles.KeepFiles) msg += "\r\n" + "Temporary build files were deleted.";
//            else
//            {
//                msg += "\r\n" + "Temporary build files were not deleted.";
//                // Display a list of the temporary build files
//                IEnumerator enu = cr.TempFiles.GetEnumerator();
//                for (int i = 0; enu.MoveNext(); i++)
//                    msg += "\r\n" + "TempFile " + i.ToString() + ": " + (string) enu.Current;
//            }
//        }
//        return msg;

//        //var type = res.CompiledAssembly.GetType("FooClass");
//        //var obj = Activator.CreateInstance(type);
//        //var output = type.GetMethod("Execute").Invoke(obj, new object[] { });
//    }
//}

