﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LanguageService;
using Xunit;

namespace LanguageModel.Tests
{
    public class TestGenerator
    {
        internal IndentingTextWriter IndentingWriter { get; private set; }
        public StringBuilder sb { get; private set; }

        private static readonly string BasePath = Environment.ExpandEnvironmentVariables(@"%UserProfile%\\Documents\\LuaTests");
        private static readonly string GenPath = Path.Combine(BasePath, "Generated Test Files");
        private static readonly string GenFileFormat = "{0}_Generated.cs";
        private static readonly string GenFileName = "Generated_{0}";

        private string GetGenFilePath(string fileName)
        {
            return Path.Combine(GenPath, string.Format(GenFileFormat, fileName));
        }

        public List<SyntaxTree> GenerateTestsForAllTestFiles()
        {
            if (!Directory.Exists(GenPath))
                Directory.CreateDirectory(GenPath);

            var treeList = new List<SyntaxTree>();

            int fileNumber = 0;

            foreach (string file in Directory.EnumerateFiles(Path.Combine(BasePath, "Lua Files for Testing"), "*.lua"))
            {
                SyntaxTree tree = SyntaxTree.Create(file);
                File.WriteAllText(GetGenFilePath(fileNumber.ToString()), string.Format(@"//{0}{1}", file, GenerateTest(tree, string.Format(GenFileName, fileNumber.ToString()))));

                Assert.Equal(0, tree.ErrorList.Count);

                treeList.Add(tree);
                
                fileNumber++;
            }

            return treeList;
        }

        public void GenerateTestFromFile(string filePath, string name)
        {
            SyntaxTree tree = SyntaxTree.Create(filePath);
            File.WriteAllText(GetGenFilePath(name), GenerateTest(tree, name + "_Generated"));
        }

        public void GenerateTestFromString(string program, string name)
        {
            SyntaxTree tree = SyntaxTree.CreateFromString(program);
            File.WriteAllText(GetGenFilePath(name), GenerateTest(tree, name + "_Generated"));
        }

        public string GenerateTest(SyntaxTree tree, string name)
        {
            IndentingWriter = IndentingTextWriter.Get(new StringWriter());
            sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("using LanguageModel.Tests.TestGeneration;");
            sb.AppendLine("using LanguageService;");
            sb.AppendLine("using Xunit;");
            sb.AppendLine("namespace LanguageModel.Tests.GeneratedTestFiles");
            sb.AppendLine("{");
            sb.AppendLine(string.Format("    class {0}", name));
            sb.AppendLine("    {");
            sb.AppendLine("        [Fact]");
            sb.AppendLine("        public void Test(Tester t)");
            sb.AppendLine("        {");

            using (IndentingWriter.Indent())
            {
                using (IndentingWriter.Indent())
                {
                    using (IndentingWriter.Indent())
                    {
                        GenerateTestStructure(tree.Root);
                    }
                }
            }

            sb.Append(IndentingWriter.ToString());
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void GenerateTestStructure(SyntaxNodeOrToken syntaxNodeOrToken)
        {
            if (syntaxNodeOrToken == null)
            {
                return;
            }

            //TODO remove is-check once Immutable graph object bug is fixed. 
            if (syntaxNodeOrToken is SyntaxNode)
            {
                IndentingWriter.WriteLine("t.N(SyntaxKind." + ((SyntaxNode)syntaxNodeOrToken).Kind + ");");
            }
            else
            {
                IndentingWriter.WriteLine("t.N(SyntaxKind." + ((Token)syntaxNodeOrToken).Kind + ");");
            }

            if (!syntaxNodeOrToken.IsLeafNode)
            {
                IndentingWriter.WriteLine("{");
                foreach (var node in ((SyntaxNode)syntaxNodeOrToken).Children)
                {
                    using (IndentingWriter.Indent())
                    {
                        GenerateTestStructure(node);
                    }
                }
                IndentingWriter.WriteLine("}");
            }
        }
    }
}
