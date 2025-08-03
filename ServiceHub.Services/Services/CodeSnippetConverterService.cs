using Microsoft.Extensions.Logging;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services
{
    public class CodeSnippetConverterService : ICodeSnippetConverterService
    {
        private readonly ILogger<CodeSnippetConverterService> _logger;
        private static readonly HashSet<string> LockedLanguages = new HashSet<string> { "javascript", "php" };
        public CodeSnippetConverterService(ILogger<CodeSnippetConverterService> logger)
        {
            _logger = logger;
        }

        public Task<CodeSnippetConvertResponseModel> ConvertCodeAsync(CodeSnippetConvertRequestModel request, bool isBusinessUser)
        {
            _logger.LogInformation($"Attempting to convert code from {request.SourceLanguage} to {request.TargetLanguage}. IsBusinessUser: {isBusinessUser}");

            var response = new CodeSnippetConvertResponseModel();
            string convertedCode = "";
            string message = "";

            try
            {
                if (string.IsNullOrWhiteSpace(request.SourceCode))
                {
                    response.ConvertedCode = "";
                    response.Message = "Моля, въведете изходен код.";
                    _logger.LogWarning("Source code is empty.");
                    return Task.FromResult(response);
                }

                string sourceLang = request.SourceLanguage.ToLower();
                string targetLang = request.TargetLanguage.ToLower();

               
                if (!isBusinessUser && (LockedLanguages.Contains(sourceLang) || LockedLanguages.Contains(targetLang)))
                {
                    response.ConvertedCode = "// Достъпът до JavaScript и PHP конвертиране е само за Бизнес Потребители.";
                    response.Message = "Достъпът до JavaScript и PHP конвертиране е само за Бизнес Потребители. Моля, надстройте акаунта си.";
                    _logger.LogWarning("Unauthorized access attempt for locked language by non-business user. Source: {Source}, Target: {Target}", request.SourceLanguage, request.TargetLanguage);
                    return Task.FromResult(response);
                }


                if (sourceLang == targetLang)
                {
                    convertedCode = request.SourceCode;
                    message = "Изходният и целевият език не могат да бъдат еднакви. Върнат е оригиналният код.";
                    _logger.LogWarning("Source and target languages are the same. Returning original code.");
                }
                else if (sourceLang == "c#")
                {
                    if (targetLang == "python")
                    {
                        convertedCode = ConvertCSharpToPython(request.SourceCode);
                        message = "Кодът е 'конвертиран' от C# към Python (опростено).";
                    }
                    else if (targetLang == "javascript")
                    {
                        convertedCode = ConvertCSharpToJavaScript(request.SourceCode);
                        message = "Кодът е 'конвертиран' от C# към JavaScript (опростено).";
                    }
                    else if (targetLang == "php")
                    {
                        convertedCode = ConvertCSharpToPhp(request.SourceCode);
                        message = "Кодът е 'конвертиран' от C# към PHP (опростено).";
                    }
                    else
                    {
                        convertedCode = $"// Неподдържана комбинация за конвертиране или опростена логика.\n" +
                                        $"// Изходен език: {request.SourceLanguage}\n" +
                                        $"// Целеви език: {request.TargetLanguage}\n\n" +
                                        $"// Оригинален код:\n{request.SourceCode}";
                        message = "Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.";
                    }
                }
                else if (sourceLang == "python")
                {
                    if (targetLang == "c#")
                    {
                        convertedCode = ConvertPythonToCSharp(request.SourceCode);
                        message = "Кодът е 'конвертиран' от Python към C# (опростено).";
                    }
                    else if (targetLang == "javascript")
                    {
                        convertedCode = ConvertPythonToJavaScript(request.SourceCode);
                        message = "Кодът е 'конвертиран' от Python към JavaScript (опростено).";
                    }
                    else if (targetLang == "php")
                    {
                        convertedCode = ConvertPythonToPhp(request.SourceCode);
                        message = "Кодът е 'конвертиран' от Python към PHP (опростено).";
                    }
                    else
                    {
                        convertedCode = $"// Неподдържана комбинация за конвертиране или опростена логика.\n" +
                                        $"// Изходен език: {request.SourceLanguage}\n" +
                                        $"// Целеви език: {request.TargetLanguage}\n\n" +
                                        $"// Оригинален код:\n{request.SourceCode}";
                        message = "Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.";
                    }
                }
                else if (sourceLang == "javascript")
                {
                    if (targetLang == "c#")
                    {
                        convertedCode = ConvertJavaScriptToCSharp(request.SourceCode);
                        message = "Кодът е 'конвертиран' от JavaScript към C# (опростено).";
                    }
                    else if (targetLang == "python")
                    {
                        convertedCode = ConvertJavaScriptToPython(request.SourceCode);
                        message = "Кодът е 'конвертиран' от JavaScript към Python (опростено).";
                    }
                    else if (targetLang == "php")
                    {
                        convertedCode = ConvertJavaScriptToPhp(request.SourceCode);
                        message = "Кодът е 'конвертиран' от JavaScript към PHP (опростено).";
                    }
                    else
                    {
                        convertedCode = $"// Неподдържана комбинация за конвертиране или опростена логика.\n" +
                                        $"// Изходен език: {request.SourceLanguage}\n" +
                                        $"// Целеви език: {request.TargetLanguage}\n\n" +
                                        $"// Оригинален код:\n{request.SourceCode}";
                        message = "Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.";
                    }
                }
                else if (sourceLang == "php")
                {
                    if (targetLang == "c#")
                    {
                        convertedCode = ConvertPhpToCSharp(request.SourceCode);
                        message = "Кодът е 'конвертиран' от PHP към C# (опростено).";
                    }
                    else if (targetLang == "python")
                    {
                        convertedCode = ConvertPhpToPython(request.SourceCode);
                        message = "Кодът е 'конвертиран' от PHP към Python (опростено).";
                    }
                    else if (targetLang == "javascript")
                    {
                        convertedCode = ConvertPhpToJavaScript(request.SourceCode);
                        message = "Кодът е 'конвертиран' от PHP към JavaScript (опростено).";
                    }
                    else
                    {
                        convertedCode = $"// Неподдържана комбинация за конвертиране или опростена логика.\n" +
                                        $"// Изходен език: {request.SourceLanguage}\n" +
                                        $"// Целеви език: {request.TargetLanguage}\n\n" +
                                        $"// Оригинален код:\n{request.SourceCode}";
                        message = "Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.";
                    }
                }
                else
                {
                    convertedCode = $"// Неподдържана комбинация за конвертиране или опростена логика.\n" +
                                    $"// Изходен език: {request.SourceLanguage}\n" +
                                    $"// Целеви език: {request.TargetLanguage}\n\n" +
                                    $"// Оригинален код:\n{request.SourceCode}";
                    message = "Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.";
                }

                response.ConvertedCode = convertedCode;
                response.Message = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during code conversion in service.");
                response.ConvertedCode = $"// Грешка при конвертиране:\n// {ex.Message}\n\n// Оригинален код:\n{request.SourceCode}";
                response.Message = $"Възникна грешка при конвертиране на кода: {ex.Message}";
            }

            _logger.LogInformation($"Code conversion completed for {request.SourceLanguage} to {request.TargetLanguage}. Message: {response.Message}");

            return Task.FromResult(response);
        }
        private string ConvertJavaScriptToPhp(string jsCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?php"); 
            var lines = jsCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0; 

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

               
                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

               
                convertedLine = Regex.Replace(convertedLine, @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{", match =>
                {
                    indentLevel++; 
                    return $"class {match.Groups[1].Value} {{";
                });

               
                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*function\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    indentLevel++; 
                    string methodName = ConvertToCamelCase(match.Groups[2].Value); 
                    string parameters = Regex.Replace(match.Groups[3].Value, @"([a-zA-Z_][a-zA-Z0-9_]*)", "$$$1"); 
                    return $"public function {methodName}({parameters}) {{";
                });

               
                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*;", "$$$2 = new $3($4);");

             
                convertedLine = Regex.Replace(convertedLine, @"console\.log\((.*?)\);", "echo $1;");

               
                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$$$2 = $3;");

               
                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", match =>
                {
                    indentLevel++; 
                    return $"if ({match.Groups[1].Value}) {{";
                });

                
                convertedLine = Regex.Replace(convertedLine, @"for\s*\((let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", match =>
                {
                    indentLevel++; 
                    return $"for ($$$2 = {match.Groups[3].Value}; $$$2 < {match.Groups[4].Value}; $$$2++) {{";
                });

                 convertedLine = convertedLine.Replace("{", "").Replace("}", "");
             
                convertedLine = Regex.Replace(convertedLine, ";$", "");


                
                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, indentLevel) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine); 
                }
            }
            sb.AppendLine("?>"); 
            return sb.ToString().Trim();
        }
        private string ConvertCSharpToPython(string csharpCode)
        {
            var sb = new StringBuilder();
            var lines = csharpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(:\s*[a-zA-Z_][a-zA-Z0-9_]*)?\s*\{", match =>
                {
                    indentLevel++;
                    return $"class {match.Groups[2].Value}:";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*(static)?\s*(void|[a-zA-Z_][a-zA-Z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    indentLevel++;
                    string methodName = ConvertToSnakeCase(match.Groups[4].Value);
                    string parameters = Regex.Replace(match.Groups[5].Value, @"\b[a-zA-Z_][a-zA-Z0-9_]*\s+([a-zA-Z_][a-zA-Z0-9_]*)", "$1");
                    return $"def {methodName}({parameters}):";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+\1\s*\((.*?)\)\s*;", "$2 = $1($3)");

                convertedLine = Regex.Replace(convertedLine, @"Console\.WriteLine\s*\((.*?)\)\s*;", "print($1)");

                convertedLine = Regex.Replace(convertedLine, @"\b(int|string|bool|double|float|decimal|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?)\s*;", "$2 = $3");

                convertedLine = convertedLine.Replace("true", "True").Replace("false", "False");

                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", "if $1:");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\((int|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\d+);\s*\2\s*<\s*(\d+);\s*\2\+\+\)\s*\{", "for $2 in range($4):");

                convertedLine = convertedLine.Replace("{", "").Replace("}", "");
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, indentLevel) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }
            return sb.ToString().Trim();
        }

        private string ConvertPythonToCSharp(string pythonCode)
        {
            var sb = new StringBuilder();
            var lines = pythonCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>();

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                int currentPythonIndent = originalLine.TakeWhile(char.IsWhiteSpace).Count() / 4;
                int expectedCSharpIndent = openBlocks.Count * 4;

                while (expectedCSharpIndent > currentPythonIndent)
                {
                    if (openBlocks.Any())
                    {
                        sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                        openBlocks.Pop();
                        expectedCSharpIndent = openBlocks.Count * 4;
                    }
                    else
                    {
                        break;
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(?:\((.*?)\))?:\s*$", match =>
                {
                    openBlocks.Push("class");
                    string baseClass = match.Groups[2].Success ? $" : {match.Groups[2].Value}" : "";
                    return $"public class {match.Groups[1].Value}{baseClass} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^def\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\):\s*$", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToPascalCase(match.Groups[1].Value);
                    string parameters = Regex.Replace(match.Groups[2].Value, @"([a-zA-Z_][a-zA-Z0-9_]*)", "string $1");
                    return $"public void {methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"print\((.*?)\)", "Console.WriteLine($1);");

                convertedLine = Regex.Replace(convertedLine, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*)", "var $1 = $2;");

                convertedLine = convertedLine.Replace("True", "true").Replace("False", "false");

                convertedLine = Regex.Replace(convertedLine, @"if\s+(.*?):", match =>
                {
                    openBlocks.Push("if");
                    return $"if ({match.Groups[1].Value}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"for\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+in\s+range\((.*?)\):", match =>
                {
                    openBlocks.Push("for");
                    return $"for (int {match.Groups[1].Value} = 0; {match.Groups[1].Value} < {match.Groups[2].Value}; {match.Groups[1].Value}++) {{";
                });

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }

            while (openBlocks.Any())
            {
                sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                openBlocks.Pop();
            }

            return sb.ToString().Trim();
        }

        private string ConvertCSharpToJavaScript(string csharpCode)
        {
            var sb = new StringBuilder();
            var lines = csharpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(:\s*[a-zA-Z_][a-zA-Z0-9_]*)?\s*\{", match =>
                {
                    indentLevel++;
                    return $"class {match.Groups[2].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*(static)?\s*(void|[a-zA-Z_][a-zA-Z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    indentLevel++;
                    string methodName = ConvertToCamelCase(match.Groups[4].Value);
                    string parameters = Regex.Replace(match.Groups[5].Value, @"\b[a-zA-Z_][a-zA-Z0-9_]*\s+([a-zA-Z_][a-zA-Z0-9_]*)", "$1");
                    return $"{methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+\1\s*\((.*?)\)\s*;", "let $2 = new $1($3);");

                convertedLine = Regex.Replace(convertedLine, @"Console\.WriteLine\((.*?)\);", "console.log($1);");

                convertedLine = Regex.Replace(convertedLine, @"\b(int|string|bool|double|float|decimal|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "let $2 = $3;");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\((int|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", match =>
                {
                    indentLevel++;
                    return $"for (let {match.Groups[2].Value} = {match.Groups[3].Value}; {match.Groups[2].Value} < {match.Groups[4].Value}; {match.Groups[2].Value}++) {{";
                });

                convertedLine = convertedLine.Replace("{", "").Replace("}", "");
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, indentLevel) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }
            return sb.ToString().Trim();
        }

        private string ConvertJavaScriptToCSharp(string jsCode)
        {
            var sb = new StringBuilder();
            var lines = jsCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>();

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                int currentJsIndent = originalLine.TakeWhile(char.IsWhiteSpace).Count() / 4;
                int expectedCSharpIndent = openBlocks.Count * 4;

                if (trimmedLine.StartsWith("}"))
                {
                    if (openBlocks.Any())
                    {
                        sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                        openBlocks.Pop();
                        expectedCSharpIndent = openBlocks.Count * 4;
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{", match =>
                {
                    openBlocks.Push("class");
                    return $"public class {match.Groups[1].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToPascalCase(match.Groups[1].Value);
                    string parameters = Regex.Replace(match.Groups[2].Value, @"([a-zA-Z_][a-zA-Z0-9_]*)", "string $1");
                    return $"public void {methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*;", "$3 $2 = new $3($4);");

                convertedLine = Regex.Replace(convertedLine, @"console\.log\((.*?)\);", "Console.WriteLine($1);");

                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "var $2 = $3;");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\((let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", match =>
                {
                    openBlocks.Push("for");
                    return $"for (int {match.Groups[2].Value} = {match.Groups[3].Value}; {match.Groups[2].Value} < {match.Groups[4].Value}; {match.Groups[2].Value}++) {{";
                });

                if (!string.IsNullOrWhiteSpace(convertedLine) && !trimmedLine.StartsWith("}"))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else if (string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(convertedLine);
                }
            }

            while (openBlocks.Any())
            {
                sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                openBlocks.Pop();
            }
            return sb.ToString().Trim();
        }

        private string ConvertCSharpToPhp(string csharpCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?php");
            var lines = csharpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0;

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(:\s*[a-zA-Z_][a-zA-Z0-9_]*)?\s*\{", match =>
                {
                    indentLevel++;
                    return $"class {match.Groups[2].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*(static)?\s*(void|[a-zA-Z_][a-zA-Z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    indentLevel++;
                    string methodName = ConvertToCamelCase(match.Groups[4].Value);
                    string parameters = Regex.Replace(match.Groups[5].Value, @"\b[a-zA-Z_][a-zA-Z0-9_]*\s+([a-zA-Z_][a-zA-Z0-9_]*)", "$$$1");
                    return $"public function {methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+\1\s*\((.*?)\)\s*;", "$$$2 = new $1($3);");

                convertedLine = Regex.Replace(convertedLine, @"Console\.WriteLine\((.*?)\);", "echo $1;");

                convertedLine = Regex.Replace(convertedLine, @"\b(int|string|bool|double|float|decimal|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$$$2 = $3;");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\((int|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", match =>
                {
                    indentLevel++;
                    return $"for ($$$2 = {match.Groups[3].Value}; $$$2 < {match.Groups[4].Value}; $$$2++) {{";
                });

                convertedLine = convertedLine.Replace("{", "").Replace("}", "");
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, indentLevel) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }
            sb.AppendLine("?>");
            return sb.ToString().Trim();
        }

        private string ConvertPhpToCSharp(string phpCode)
        {
            var sb = new StringBuilder();
            string cleanPhpCode = Regex.Replace(phpCode, @"<\?php|\?>", "").Trim();
            var lines = cleanPhpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>();

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                int currentPhpIndent = originalLine.TakeWhile(char.IsWhiteSpace).Count() / 4;
                int expectedCSharpIndent = openBlocks.Count * 4;

                if (trimmedLine.StartsWith("}"))
                {
                    if (openBlocks.Any())
                    {
                        sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                        openBlocks.Pop();
                        expectedCSharpIndent = openBlocks.Count * 4;
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{", match =>
                {
                    openBlocks.Push("class");
                    return $"public class {match.Groups[1].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*function\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToPascalCase(match.Groups[2].Value);
                    string parameters = Regex.Replace(match.Groups[3].Value, @"\$([a-zA-Z_][a-zA-Z0-9_]*)", "string $1");
                    return $"public void {methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*;", "$2 $1 = new $2($3);");

                convertedLine = Regex.Replace(convertedLine, @"echo\s+(.*?);", "Console.WriteLine($1);");

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "var $1 = $2;");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\(\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\$1\s*<\s*(.*?);\s*\$1\+\+\)\s*\{", match =>
                {
                    openBlocks.Push("for");
                    return $"for (int {match.Groups[1].Value} = {match.Groups[2].Value}; {match.Groups[1].Value} < {match.Groups[3].Value}; {match.Groups[1].Value}++) {{";
                });

                if (!string.IsNullOrWhiteSpace(convertedLine) && !trimmedLine.StartsWith("}"))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else if (string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(convertedLine);
                }
            }

            while (openBlocks.Any())
            {
                sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                openBlocks.Pop();
            }
            return sb.ToString().Trim();
        }

        private string ConvertPythonToJavaScript(string pythonCode)
        {
            var sb = new StringBuilder();
            var lines = pythonCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>(); 

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                int currentPythonIndent = originalLine.TakeWhile(char.IsWhiteSpace).Count() / 4;
                int expectedJsIndent = openBlocks.Count * 4;

                while (expectedJsIndent > currentPythonIndent)
                {
                    if (openBlocks.Any())
                    {
                        sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                        openBlocks.Pop();
                        expectedJsIndent = openBlocks.Count * 4;
                    }
                    else
                    {
                        break;
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(?:\((.*?)\))?:\s*$", match =>
                {
                    openBlocks.Push("class");
                    return $"class {match.Groups[1].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^def\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\):\s*$", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToCamelCase(match.Groups[1].Value);
                    return $"{methodName}({match.Groups[2].Value}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*$", "let $1 = new $2($3);");

                convertedLine = Regex.Replace(convertedLine, @"print\((.*?)\)", "console.log($1)");

                convertedLine = Regex.Replace(convertedLine, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*)", "let $1 = $2;");

                convertedLine = convertedLine.Replace("True", "true").Replace("False", "false");

                convertedLine = Regex.Replace(convertedLine, @"if\s+(.*?):", match =>
                {
                    openBlocks.Push("if");
                    return $"if ({match.Groups[1].Value}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"for\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+in\s+range\((.*?)\):", match =>
                {
                    openBlocks.Push("for");
                    return $"for (let {match.Groups[1].Value} = 0; {match.Groups[1].Value} < {match.Groups[2].Value}; {match.Groups[1].Value}++) {{";
                });

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }

            while (openBlocks.Any())
            {
                sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                openBlocks.Pop();
            }
            return sb.ToString().Trim();
        }

        private string ConvertJavaScriptToPython(string jsCode)
        {
            var sb = new StringBuilder();
            var lines = jsCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>(); 

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                if (trimmedLine.StartsWith("}"))
                {
                    if (openBlocks.Any())
                    {
                        openBlocks.Pop();
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{", match =>
                {
                    openBlocks.Push("class");
                    return $"class {match.Groups[1].Value}:";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToSnakeCase(match.Groups[1].Value);
                    return $"def {methodName}({match.Groups[2].Value}):";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*;", "$2 = $3($4)");

                convertedLine = Regex.Replace(convertedLine, @"console\.log\((.*?)\);", "print($1)");

                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$2 = $3");

                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", match =>
                {
                    openBlocks.Push("if");
                    return $"if {match.Groups[1].Value}:";
                });

                convertedLine = Regex.Replace(convertedLine, @"for\s*\((let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\d+);\s*\2\s*<\s*(\d+);\s*\2\+\+\)\s*\{", match =>
                {
                    openBlocks.Push("for");
                    return $"for {match.Groups[2].Value} in range({match.Groups[4].Value}):";
                });

                convertedLine = convertedLine.Replace("{", "").Replace("}", "");
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }
            return sb.ToString().Trim();
        }

        private string ConvertPythonToPhp(string pythonCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?php");
            string cleanPythonCode = Regex.Replace(pythonCode, @"<\?php|\?>", "").Trim(); 
            var lines = cleanPythonCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>(); 

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                int currentPythonIndent = originalLine.TakeWhile(char.IsWhiteSpace).Count() / 4;
                int expectedPhpIndent = openBlocks.Count * 4;

                while (expectedPhpIndent > currentPythonIndent)
                {
                    if (openBlocks.Any())
                    {
                        sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                        openBlocks.Pop();
                        expectedPhpIndent = openBlocks.Count * 4;
                    }
                    else
                    {
                        break;
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(?:\((.*?)\))?:\s*$", match =>
                {
                    openBlocks.Push("class");
                    return $"class {match.Groups[1].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^def\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\):\s*$", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToCamelCase(match.Groups[1].Value);
                    string parameters = Regex.Replace(match.Groups[2].Value, @"([a-zA-Z_][a-zA-Z0-9_]*)", "$$$1");
                    return $"public function {methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*$", "$$$1 = new $2($3);");

                convertedLine = Regex.Replace(convertedLine, @"print\((.*?)\)", "echo $1;");

                convertedLine = Regex.Replace(convertedLine, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*)", "$$$1 = $2;");

                convertedLine = convertedLine.Replace("True", "true").Replace("False", "false");

                convertedLine = Regex.Replace(convertedLine, @"if\s+(.*?):", match =>
                {
                    openBlocks.Push("if");
                    return $"if ({match.Groups[1].Value}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"for\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+in\s+range\((.*?)\):", match =>
                {
                    openBlocks.Push("for");
                    return $"for ($$$1 = 0; $$$1 < {match.Groups[2].Value}; $$$1++) {{";
                });

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }

            while (openBlocks.Any())
            {
                sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count - 1) * 4) + "}");
                openBlocks.Pop();
            }
            sb.AppendLine("?>");
            return sb.ToString().Trim();
        }

        private string ConvertPhpToPython(string phpCode)
        {
            var sb = new StringBuilder();
            string cleanPhpCode = Regex.Replace(phpCode, @"<\?php|\?>", "").Trim();
            var lines = cleanPhpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Stack<string> openBlocks = new Stack<string>();

            foreach (var line in lines)
            {
                string originalLine = line;
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                if (trimmedLine.StartsWith("}"))
                {
                    if (openBlocks.Any())
                    {
                        openBlocks.Pop();
                    }
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{", match =>
                {
                    openBlocks.Push("class");
                    return $"class {match.Groups[1].Value}:";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*function\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    openBlocks.Push("method");
                    string methodName = ConvertToSnakeCase(match.Groups[2].Value);
                    string parameters = Regex.Replace(match.Groups[3].Value, @"\$([a-zA-Z_][a-zA-Z0-9_]*)", "$1");
                    return $"def {methodName}({parameters}):";
                });

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*;", "$1 = $2($3)");

                convertedLine = Regex.Replace(convertedLine, @"echo\s+(.*?);", "print($1)");

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$1 = $2");

                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", match =>
                {
                    openBlocks.Push("if");
                    return $"if {match.Groups[1].Value}:";
                });

                convertedLine = Regex.Replace(convertedLine, @"for\s*\(\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\$1\s*<\s*(.*?);\s*\$1\+\+\)\s*\{", match =>
                {
                    openBlocks.Push("for");
                    return $"for {match.Groups[1].Value} in range({match.Groups[2].Value}, {match.Groups[3].Value}):";
                });

                convertedLine = convertedLine.Replace("{", "").Replace("}", "");
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, openBlocks.Count) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }
            return sb.ToString().Trim();
        }

        private string ConvertPhpToJavaScript(string phpCode)
        {
            var sb = new StringBuilder();
            string cleanPhpCode = Regex.Replace(phpCode, @"<\?php|\?>", "").Trim();
            var lines = cleanPhpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int indentLevel = 0; 

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                string convertedLine = trimmedLine;

                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                convertedLine = Regex.Replace(convertedLine, @"^\s*class\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{", match =>
                {
                    indentLevel++;
                    return $"class {match.Groups[1].Value} {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"^\s*(public|private|protected)?\s*function\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*\{", match =>
                {
                    indentLevel++;
                    string methodName = ConvertToCamelCase(match.Groups[2].Value);
                    string parameters = Regex.Replace(match.Groups[3].Value, @"\$([a-zA-Z_][a-zA-Z0-9_]*)", "$1");
                    return $"{methodName}({parameters}) {{";
                });

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)\)\s*;", "let $1 = new $2($3);");

                convertedLine = Regex.Replace(convertedLine, @"echo\s+(.*?);", "console.log($1);");

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "let $1 = $2;");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\(\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\$1\s*<\s*(.*?);\s*\$1\+\+\)\s*\{", match =>
                {
                    indentLevel++;
                    return $"for (let {match.Groups[1].Value} = {match.Groups[2].Value}; {match.Groups[1].Value} < {match.Groups[3].Value}; {match.Groups[1].Value}++) {{";
                });

                convertedLine = convertedLine.Replace("{", "").Replace("}", "");
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                if (!string.IsNullOrWhiteSpace(convertedLine))
                {
                    sb.AppendLine(new string(' ', Math.Max(0, indentLevel) * 4) + convertedLine);
                }
                else
                {
                    sb.AppendLine(convertedLine);
                }
            }
            return sb.ToString().Trim();
        }

        private string ConvertToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private string ConvertToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        private string ConvertToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            return Regex.Replace(name, @"(?<=[a-z0-9])([A-Z])", "_$1").ToLowerInvariant();
        }
    }
}
