using System.Collections.Generic;
using System.IO;
using Unity.MetallibGenertaor;
using UnityEditor;
using UnityEngine;

public class MetallibGenerator
{
    [MenuItem("Assets/Gen Metallib", false, 208)]
    private static void Metallib()
    {
        if (!(Selection.activeObject is Shader))
        {
            Debug.Log("Please select a shader!!");
            return;
        }
        Shader targetShader = Selection.activeObject as Shader;
        string metallibDir = "Temp/Compiled-" + targetShader.name.Replace('/', '-') + "-Metallib";
        if (!Directory.Exists(metallibDir))
            Directory.CreateDirectory(metallibDir);
        
        string compiledShader = UnityShaderCompiler.GetCompiledShader(targetShader, true);
        Dictionary<string, CompiledMetalShader> metalShaderTextMap = CompiledMetalShader.Parse(compiledShader);
        Debug.Log(metalShaderTextMap.Count);
        foreach (var pair in metalShaderTextMap)
        {
            string vertPath = "Temp/Compiled-" + targetShader.name.Replace('/', '-') + "-vert.metal";
            File.WriteAllText(vertPath, pair.Value.VertProgram);
            string fileName = pair.Key.Replace(":", "-").Replace(" ", "");
            string outputAirPath = metallibDir + "/" + fileName + "-vert.air";
            string outputLibPath = metallibDir + "/" + fileName + "-vert.metallib";
            string retString = null;
            bool success = CommandLine.Run("xcrun -sdk macosx metal -c " + vertPath + " -o " + outputAirPath, out retString);
            if (!success)
            {
                Debug.Log("Failed!!!" + retString + "\n" + vertPath + " -> " + outputAirPath);
                break;
            }
            
            success = CommandLine.Run("xcrun -sdk macosx metallib " + outputAirPath + " -o " + outputLibPath, out retString);
            if (!success)
            {
                Debug.Log("Failed!!!" + retString + "\n" + outputAirPath + " -> " + outputLibPath);
                break;
            }
            
            string fragPath = "Temp/Compiled-" + targetShader.name.Replace('/', '-') + "-frag.metal";
            File.WriteAllText(fragPath, pair.Value.FragProgram);
            outputAirPath = metallibDir + "/" + fileName + "-frag.air";
            outputLibPath = metallibDir + "/" + fileName + "-frag.metallib";
            retString = null;
            success = CommandLine.Run("xcrun -sdk macosx metal -c " + fragPath + " -o " + outputAirPath, out retString);
            if (!success)
            {
                Debug.Log("Failed!!!" + retString + "\n" + fragPath + " -> " + outputAirPath);
                break;
            }
            
            success = CommandLine.Run("xcrun -sdk macosx metallib " + outputAirPath + " -o " + outputLibPath, out retString);
            if (!success)
            {
                Debug.Log("Failed!!!" + retString + "\n" + outputAirPath + " -> " + outputLibPath);
                break;
            }
        }
        Debug.Log("Finished!!");
    }
}
