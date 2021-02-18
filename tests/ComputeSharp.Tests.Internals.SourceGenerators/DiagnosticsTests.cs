﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ComputeSharp.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputeSharp.Tests.Internals
{
    [TestClass]
    [TestCategory("Diagnostics")]
    public class DiagnosticsTests
    {
        [TestMethod]
        public void InvalidShaderField()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;
                    public string text;

                    public void Execute() { }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0001");
        }

        [TestMethod]
        public void InvalidGroupSharedFieldType()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
                public class GroupSharedAttribute : Attribute { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    [GroupShared]
                    public static string text;

                    public void Execute() { }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0002");
        }

        [TestMethod]
        public void InvalidGroupSharedFieldElementType()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
                public class GroupSharedAttribute : Attribute { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    [GroupShared]
                    public static string[] text;

                    public void Execute() { }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0003");
        }

        [TestMethod]
        public void InvalidGroupSharedFieldDeclaration()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
                public class GroupSharedAttribute : Attribute { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    [GroupShared]
                    public ReadWriteBuffer<float> buffer;

                    public void Execute() { }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0004");
        }

        [TestMethod]
        public void MissingShaderResources()
        {
            string source = @"
            using ComputeSharp;

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public void Execute() { }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0005");
        }

        [TestMethod]
        [DataRow(nameof(ThreadIds), "CMPS0006")]
        [DataRow(nameof(GroupIds), "CMPS0007")]
        [DataRow(nameof(GroupSize), "CMPS0008")]
        [DataRow(nameof(WarpIds), "CMPS0009")]
        public void InvalidDispatchInfoUsage_LocalFunction(string typeName, string diagnosticsId)
        {
            string source = $@"
            using ComputeSharp;

            namespace ComputeSharp
            {{
                public class ReadWriteBuffer<T> {{ }}
                public static class {typeName} {{ public static int X => 0; }}
            }}

            namespace MyFancyApp.Sample
            {{
                public struct MyShader : IComputeShader
                {{
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {{
                        static int Fail() => {typeName}.X;

                        Fail();
                    }}
                }}
            }}";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, diagnosticsId);
        }

        [TestMethod]
        [DataRow(nameof(ThreadIds), "CMPS0006")]
        [DataRow(nameof(GroupIds), "CMPS0007")]
        [DataRow(nameof(GroupSize), "CMPS0008")]
        [DataRow(nameof(WarpIds), "CMPS0009")]
        public void InvalidDispatchInfoUsage_StaticMethod(string typeName, string diagnosticsId)
        {
            string source = $@"
            using ComputeSharp;

            namespace ComputeSharp
            {{
                public class ReadWriteBuffer<T> {{ }}
                public static class {typeName} {{ public static int X => 0; }}
            }}

            namespace MyFancyApp.Sample
            {{
                public struct MyShader : IComputeShader
                {{
                    public ReadWriteBuffer<float> buffer;

                    public static int Fail() => {typeName}.X;

                    public void Execute() => Fail();
                }}
            }}";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, diagnosticsId);
        }

        [TestMethod]
        public void InvalidObjectCreationExpression_Explicit()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        object o = new object();
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0010");
        }

        [TestMethod]
        public void InvalidObjectCreationExpression_Implicit()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        object o = new();
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0010");
        }

        [TestMethod]
        public void AnonymousObjectCreationExpression()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        var o = new { Age = 12 };
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0011");
        }

        [TestMethod]
        public void AsyncModifierOnMethodOrFunction_LocalFunction()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        static async void Fail() { }

                        Fail();
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0012");
        }

        [TestMethod]
        public void AsyncModifierOnMethodOrFunction_StaticMethod()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public static async void Fail() { }

                    public void Execute() => Fail();
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0012");
        }

        [TestMethod]
        public void AwaitExpression()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public async void Execute()
                    {
                        await null;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0012", "CMPS0013");
        }

        [TestMethod]
        [DataRow("checked")]
        [DataRow("unchecked")]
        public void CheckedExpression(string text)
        {
            string source = $@"
            using ComputeSharp;

            namespace ComputeSharp
            {{
                public class ReadWriteBuffer<T> {{ }}
            }}

            namespace MyFancyApp.Sample
            {{
                public struct MyShader : IComputeShader
                {{
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {{
                        int a = {text}(1 + 1);
                    }}
                }}
            }}";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0014");
        }

        [TestMethod]
        [DataRow("checked")]
        [DataRow("unchecked")]
        public void CheckedStatement(string text)
        {
            string source = $@"
            using ComputeSharp;

            namespace ComputeSharp
            {{
                public class ReadWriteBuffer<T> {{ }}
            }}

            namespace MyFancyApp.Sample
            {{
                public struct MyShader : IComputeShader
                {{
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {{
                        {text}
                        {{
                            int a = 1 + 1;
                        }}
                    }}
                }}
            }}";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0015");
        }

        [TestMethod]
        public void FixedStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public int[] Data;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {                        
                        fixed (void* p = buffer.Data)
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0016", "CMPS0032");
        }

        [TestMethod]
        public void ForEachStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public int[] Data;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {                        
                        foreach (int i in buffer.Data)
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0017");
        }

        [TestMethod]
        public void LockStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {                        
                        lock (buffer)
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0018");
        }

        [TestMethod]
        public void QueryExpression()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public int[] Data;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        _ = from x in buffer.Data select x;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0019");
        }

        [TestMethod]
        public void RangeExpression()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public int[] Data;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        _ = buffer.Data[1..];
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0020");
        }

        [TestMethod]
        public void RecursivePattern()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public object Data;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        if (buffer.Data is int[] { Length: 50 } foo)
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0021");
        }

        [TestMethod]
        public void RefType()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public ref T this[int x] => throw null;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        ref float x = ref buffer[0];
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0022");
        }

        [TestMethod]
        public void RelationalPattern()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T>
                {
                    public int Data;
                }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        if (buffer.Data is > 0 and < 10)
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0023");
        }

        [TestMethod]
        public void SizeOfExpression()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        _ = sizeof(float);
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0024");
        }

        [TestMethod]
        public void StackAllocArrayCreationExpression()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        int* p = stackalloc int[10];
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0032", "CMPS0025");
        }

        [TestMethod]
        public void ThrowExpressionOrStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {               
                        throw null;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0026");
        }

        [TestMethod]
        public void TryStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        try
                        {
                        }
                        catch
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0027");
        }

        [TestMethod]
        public void TupleType()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        (int, float) foo;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0028");
        }

        [TestMethod]
        public void UsingDeclaration()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        using IDisposable foo = null
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0029");
        }

        [TestMethod]
        public void UsingStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        using (IDisposable foo = null)
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0029");
        }

        [TestMethod]
        public void YieldStatement()
        {
            string source = @"
            using System.Collections.Generic;
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public static IEnumerable<float> Fail()
                    {
                        yield return 3.14f;
                    }

                    public void Execute()
                    {
                        Fail();
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0030");
        }

        [TestMethod]
        public void InvalidObjectDeclaration()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        object bar;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0031");
        }

        [TestMethod]
        public void PointerType()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        int* p;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0032");
        }

        [TestMethod]
        public void FunctionPointer()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        delegate*<int, void> f;
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0033");
        }

        [TestMethod]
        public void UnsafeStatement()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        unsafe
                        {
                        }
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0034");
        }

        [TestMethod]
        public void UnsafeModifier_LocalFunction()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        static unsafe void Fail() { }

                        Fail();
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0035");
        }

        [TestMethod]
        public void UnsafeModifier_StaticMethod()
        {
            string source = @"
            using ComputeSharp;

            namespace ComputeSharp
            {
                public class ReadWriteBuffer<T> { }
            }

            namespace MyFancyApp.Sample
            {
                public struct MyShader : IComputeShader
                {
                    public static unsafe void Fail() { }

                    public ReadWriteBuffer<float> buffer;

                    public void Execute()
                    {
                        Fail();
                    }
                }
            }";

            VerifyGeneratedDiagnostics<IComputeShaderSourceGenerator>(source, "CMPS0035");
        }

        /// <summary>
        /// Verifies the output of a source generator.
        /// </summary>
        /// <typeparam name="TGenerator">The generator type to use.</typeparam>
        /// <param name="source">The input source to process.</param>
        /// <param name="index">The target index to check in the resulting output.</param>
        /// <param name="expectedBody">The expected body to compare with the generated code.</param>
        private void VerifyGeneratedDiagnostics<TGenerator>(string source, params string[] diagnosticsIds)
            where TGenerator : class, ISourceGenerator, new()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

            IEnumerable<MetadataReference> references =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                where !assembly.IsDynamic
                let reference = MetadataReference.CreateFromFile(assembly.Location)
                select reference;

            CSharpCompilation compilation = CSharpCompilation.Create("original", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            ISourceGenerator generator = new TGenerator();

            CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

            HashSet<string> resultingIds = diagnostics.Select(diagnostic => diagnostic.Id).ToHashSet();

            Assert.IsTrue(resultingIds.SetEquals(diagnosticsIds));
        }
    }
}