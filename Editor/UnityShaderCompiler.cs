using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Unity.MetallibGenertaor
{
    internal class DummyCodeEditorOpenScope : System.IDisposable
    {
        private string codeEditorString = null;

        public DummyCodeEditorOpenScope()
        {
            codeEditorString = CodeEditor.CodeEditor.CurrentEditorInstallation;

            if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
                CodeEditor.CodeEditor.SetExternalScriptEditor("/bin/bash");
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                CodeEditor.CodeEditor.SetExternalScriptEditor("cmd.exe");
        }

        public void Dispose()
        {
            CodeEditor.CodeEditor.SetExternalScriptEditor(codeEditorString);
        }
    }

    public class UnityShaderCompiler
    {
        private static void Compile(Shader shader, bool skipUnusedVariant)
        {
            using (DummyCodeEditorOpenScope dontOpenCodeEditor = new DummyCodeEditorOpenScope())
            {
                Type shaderUtilType = typeof(ShaderUtil);
                MethodInfo compileMethod = shaderUtilType.GetMethod("OpenCompiledShader", BindingFlags.Static | BindingFlags.NonPublic);
                int currentMode = 3;
                int externPlatformsMask = 0;
                externPlatformsMask = (1 << (int) ShaderCompilerPlatform.Metal);
                bool preprocessOnly = false;
                bool stripLineDirectives = false;
                ParameterInfo[] parameterInfos = compileMethod.GetParameters();
                // for (int i = 0; i < parameterInfos.Length; i++) 
                //     Debug.Log(i + ":" + parameterInfos[i].ParameterType + " : " + parameterInfos[i].Name);
                switch (parameterInfos.Length)
                {
                    case 4:
                        compileMethod.Invoke(null, new object[] {shader, currentMode, externPlatformsMask, !skipUnusedVariant});
                        break;
                    case 6:
                        compileMethod.Invoke(null, new object[] {shader, currentMode, externPlatformsMask, !skipUnusedVariant, preprocessOnly, stripLineDirectives});
                        break;
                }
            }
        }
        
        public static string GetCompiledShader(Shader shader, bool skipUnusedVariant)
        {
            if (ShaderUtil.ShaderHasError(shader))
                return null;
            Compile(shader, skipUnusedVariant);
            string path = "Temp/Compiled-" + shader.name.Replace('/', '-') + ".shader";
            if (File.Exists(path))
                return File.ReadAllText(path);
            return null;
        }
    }
}