using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using System.Text;
using System.Text.RegularExpressions;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class CodeSnippetConverterController : ControllerBase
    {
        private readonly ILogger<CodeSnippetConverterController> _logger;

        public CodeSnippetConverterController(ILogger<CodeSnippetConverterController> logger)
        {
            _logger = logger;
        }

        [HttpPost("convert")] 
        public IActionResult ConvertCode([FromBody] CodeSnippetConvertRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CodeSnippetConvertRequestModel: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            string convertedCode = "";
            string message = "";

            if (string.IsNullOrWhiteSpace(request.SourceCode))
            {
                return BadRequest(new { message = "Моля, въведете изходен код." });
            }

            string sourceLang = request.SourceLanguage.ToLower();
            string targetLang = request.TargetLanguage.ToLower();

            if (sourceLang == targetLang)
            {
                convertedCode = request.SourceCode; 
                message = "Изходният и целевият език не могат да бъдат еднакви. Върнат е оригиналният код.";
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
            }
         
            else
            {
                convertedCode = $"// Неподдържана комбинация за конвертиране или опростена логика.\n" +
                                $"// Изходен език: {request.SourceLanguage}\n" +
                                $"// Целеви език: {request.TargetLanguage}\n\n" +
                                $"// Оригинален код:\n{request.SourceCode}";
                message = "Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.";
            }

            return Ok(new CodeSnippetConvertResponseModel { ConvertedCode = convertedCode, Message = message });
        }


        private string ConvertCSharpToPython(string csharpCode)
        {
            var sb = new StringBuilder();
            var lines = csharpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

               
                convertedLine = Regex.Replace(convertedLine, @"Console\.WriteLine\s*\((.*?)\)\s*;", "print($1)");

           
                convertedLine = Regex.Replace(convertedLine, @"\b(int|string|bool|double|float|decimal|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?)\s*;", "$2 = $3");

               
                convertedLine = convertedLine.Replace("true", "True").Replace("false", "False");

              
                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", "if $1:");

                
                convertedLine = Regex.Replace(convertedLine, @"for\s*\((int|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\d+);\s*\2\s*<\s*(\d+);\s*\2\+\+\)\s*\{", "for $2 in range($4):");

                
                convertedLine = convertedLine.Replace("{", "").Replace("}", "");

                convertedLine = Regex.Replace(convertedLine, ";$", "");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }

        private string ConvertPythonToCSharp(string pythonCode)
        {
            var sb = new StringBuilder();
            var lines = pythonCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

            
                convertedLine = Regex.Replace(convertedLine, @"print\((.*?)\)", "Console.WriteLine($1);");

          
                convertedLine = Regex.Replace(convertedLine, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*)", "var $1 = $2;");

               
                convertedLine = convertedLine.Replace("True", "true").Replace("False", "false");

            
                convertedLine = Regex.Replace(convertedLine, @"if\s+(.*?):", "if ($1) {");

               
                convertedLine = Regex.Replace(convertedLine, @"for\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+in\s+range\((.*?)\):", "for (int $1 = 0; $1 < $2; $1++) {");

                sb.AppendLine(convertedLine);
            }
         
            if (lines.Any(l => l.Trim().StartsWith("if") || l.Trim().StartsWith("for")))
            {
                sb.AppendLine("}");
            }
            return sb.ToString().Trim();
        }

        private string ConvertCSharpToJavaScript(string csharpCode)
        {
            var sb = new StringBuilder();
            var lines = csharpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

               
                convertedLine = Regex.Replace(convertedLine, @"Console\.WriteLine\((.*?)\);", "console.log($1);");

                
                convertedLine = Regex.Replace(convertedLine, @"\b(int|string|bool|double|float|decimal|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "let $2 = $3;");

             
                convertedLine = convertedLine.Replace("true", "true").Replace("false", "false");

             
                convertedLine = Regex.Replace(convertedLine, @"for\s*\((int|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", "for (let $2 = $3; $2 < $4; $2++) {");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }

        private string ConvertJavaScriptToCSharp(string jsCode)
        {
            var sb = new StringBuilder();
            var lines = jsCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

               
                convertedLine = Regex.Replace(convertedLine, @"console\.log\((.*?)\);", "Console.WriteLine($1);");

              
                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "var $2 = $3;");

              
                convertedLine = Regex.Replace(convertedLine, @"for\s*\((let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", "for (int $2 = $3; $2 < $4; $2++) {");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }

        private string ConvertCSharpToPhp(string csharpCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?php");
            var lines = csharpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

            
                convertedLine = Regex.Replace(convertedLine, @"Console\.WriteLine\((.*?)\);", "echo $1;");

                convertedLine = Regex.Replace(convertedLine, @"\b(int|string|bool|double|float|decimal|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$$$2 = $3;");

             
                convertedLine = convertedLine.Replace("true", "true").Replace("false", "false");

                convertedLine = Regex.Replace(convertedLine, @"for\s*\((int|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", "for ($$$2 = $3; $$$2 < $4; $$$2++) {");

                sb.AppendLine(convertedLine);
            }
            sb.AppendLine("?>");
            return sb.ToString().Trim();
        }

        private string ConvertPhpToCSharp(string phpCode)
        {
            var sb = new StringBuilder();
          
            string cleanPhpCode = Regex.Replace(phpCode, @"<\?php|\?>", "").Trim();
            var lines = cleanPhpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

            
                convertedLine = Regex.Replace(convertedLine, @"echo\s+(.*?);", "Console.WriteLine($1);");

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "var $1 = $2;");

           
                convertedLine = Regex.Replace(convertedLine, @"for\s*\(\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\$1\s*<\s*(.*?);\s*\$1\+\+\)\s*\{", "for (int $1 = $2; $1 < $3; $1++) {");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }

        private string ConvertPythonToJavaScript(string pythonCode)
        {
            var sb = new StringBuilder();
            var lines = pythonCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

             
                convertedLine = Regex.Replace(convertedLine, @"print\((.*?)\)", "console.log($1);");

              
                convertedLine = Regex.Replace(convertedLine, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*)", "let $1 = $2;");

                convertedLine = convertedLine.Replace("True", "true").Replace("False", "false");

              
                convertedLine = Regex.Replace(convertedLine, @"if\s+(.*?):", "if ($1) {");

               
                convertedLine = Regex.Replace(convertedLine, @"for\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+in\s+range\((.*?)\):", "for (let $1 = 0; $1 < $2; $1++) {");

                sb.AppendLine(convertedLine);
            }
           
            if (lines.Any(l => l.Trim().StartsWith("if") || l.Trim().StartsWith("for")))
            {
                sb.AppendLine("}");
            }
            return sb.ToString().Trim();
        }

        private string ConvertJavaScriptToPython(string jsCode)
        {
            var sb = new StringBuilder();
            var lines = jsCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

                convertedLine = Regex.Replace(convertedLine, @"console\.log\((.*?)\);", "print($1)");

                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$2 = $3");

             
                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", "if $1:");

                
                convertedLine = Regex.Replace(convertedLine, @"for\s*\((let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\d+);\s*\2\s*<\s*(\d+);\s*\2\+\+\)\s*\{", "for $2 in range($4):");

              
                convertedLine = convertedLine.Replace("{", "").Replace("}", "");

               
                convertedLine = Regex.Replace(convertedLine, ";$", "");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }

        private string ConvertPythonToPhp(string pythonCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?php");
            var lines = pythonCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

               
                convertedLine = Regex.Replace(convertedLine, @"print\((.*?)\)", "echo $1;");

                
                convertedLine = Regex.Replace(convertedLine, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*)", "$$$1 = $2;");

                
                convertedLine = convertedLine.Replace("True", "true").Replace("False", "false");

              
                convertedLine = Regex.Replace(convertedLine, @"if\s+(.*?):", "if ($1) {");

               
                convertedLine = Regex.Replace(convertedLine, @"for\s+([a-zA-Z_][a-zA-Z0-9_]*)\s+in\s+range\((.*?)\):", "for ($$$1 = $2; $$$1 < $3; $$$1++) {");

                sb.AppendLine(convertedLine);
            }
            sb.AppendLine("?>");
            return sb.ToString().Trim();
        }

        private string ConvertPhpToPython(string phpCode)
        {
            var sb = new StringBuilder();
        
            string cleanPhpCode = Regex.Replace(phpCode, @"<\?php|\?>", "").Trim();
            var lines = cleanPhpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

              
                convertedLine = Regex.Replace(convertedLine, @"echo\s+(.*?);", "print($1)");

                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$1 = $2");

               
                convertedLine = Regex.Replace(convertedLine, @"if\s*\((.*?)\)\s*\{", "if $1:");

               
                convertedLine = Regex.Replace(convertedLine, @"for\s*\(\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(\d+);\s*\$1\s*<\s*(\d+);\s*\$1\+\+\)\s*\{", "for $1 in range($2, $3):");

                
                convertedLine = convertedLine.Replace("{", "").Replace("}", "");

                convertedLine = Regex.Replace(convertedLine, ";$", "");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }

        private string ConvertJavaScriptToPhp(string jsCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?php");
            var lines = jsCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

               
                convertedLine = Regex.Replace(convertedLine, @"console\.log\((.*?)\);", "echo $1;");

                
                convertedLine = Regex.Replace(convertedLine, @"\b(let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "$$$2 = $3;");

              
                convertedLine = Regex.Replace(convertedLine, @"for\s*\((let|const|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\2\s*<\s*(.*?);\s*\2\+\+\)\s*\{", "for ($$$2 = $3; $$$2 < $4; $$$2++) {");

                sb.AppendLine(convertedLine);
            }
            sb.AppendLine("?>");
            return sb.ToString().Trim();
        }

        private string ConvertPhpToJavaScript(string phpCode)
        {
            var sb = new StringBuilder();
           
            string cleanPhpCode = Regex.Replace(phpCode, @"<\?php|\?>", "").Trim();
            var lines = cleanPhpCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string convertedLine = line.Trim();

               
                convertedLine = Regex.Replace(convertedLine, @"echo\s+(.*?);", "console.log($1);");

               
                convertedLine = Regex.Replace(convertedLine, @"\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);", "let $1 = $2;");

               
                convertedLine = Regex.Replace(convertedLine, @"for\s*\(\$([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.*?);\s*\$1\s*<\s*(.*?);\s*\$1\+\+\)\s*\{", "for (let $1 = $2; $1 < $3; $1++) {");

                sb.AppendLine(convertedLine);
            }
            return sb.ToString().Trim();
        }
    }
}
