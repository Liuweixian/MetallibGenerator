using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MetallibGenertaor
{
    public class CompiledMetalShader
    {
        private enum ParseState
        {
            eInvalid,
            eReadSubShader,
            eReadPassInfo,
            eReadVertProgram,
            eReadFragProgram,
            
        }
        private string subShaderInfo = null;
        private int passIndex = 0;
        private string passInfo = null;
        private string keywords = null;
        private string vertProgram = null;
        public string VertProgram
        {
            get
            {
                return vertProgram;
            }
        }
        private string fragProgram = null;

        public string FragProgram
        {
            get
            {
                return fragProgram;
            }
        }
        private string identify = null;
        public string Identify
        {
            get
            {
                return identify;
            }
        }

        private CompiledMetalShader(string subShaderInfo, int passIndex, string passInfo, string keywords, string vertProgram, string fragProgram)
        {
            this.subShaderInfo = subShaderInfo;
            this.passIndex = passIndex;
            this.passInfo = passInfo;
            this.keywords = keywords;
            this.vertProgram = vertProgram;
            this.fragProgram = fragProgram;
            this.identify = "P: " + passIndex + " " + keywords;
        }

        public static Dictionary<string, CompiledMetalShader> Parse(string compiledShader)
        {
            Dictionary<string, CompiledMetalShader> retMapping = new Dictionary<string, CompiledMetalShader>();
            
            string subShaderInfo = null;
            string passInfo = null;
            string keywords = null;
            string vertProgram = null;
            string fragProgram = null;
            int passCounter = 0;
            bool readingDisassembly = false;
            bool readingFragBlock = false;
            ParseState parseState = ParseState.eInvalid;
            
            string[] lines = compiledShader.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Contains("SubShader {"))
                {
                    subShaderInfo = "";
                    parseState = ParseState.eReadSubShader;
                    continue;
                }

                if (line.Contains("Pass {"))
                {
                    passCounter++;
                    passInfo = "";
                    parseState = ParseState.eReadPassInfo;
                    continue;
                }

                if (line.Contains("//////////////////////////////////"))
                {
                    parseState = ParseState.eInvalid;
                    continue;
                }

                if (line.Contains("Global Keywords"))
                {
                    line = line.Replace("Global Keywords", "G");
                    line = line.TrimEnd();
                    keywords = line;
                    continue;
                }
                
                if (line.Contains("Local Keywords"))
                {
                    line = line.Replace("Local Keywords", "L");
                    line = line.Replace("<none>", "");
                    line = line.TrimEnd();
                    keywords += " " + line;
                    continue;
                }

                if (line.StartsWith("-- Hardware tier variant"))
                {
                    readingDisassembly = false;
                    continue;
                }
                
                if (line.StartsWith("-- Vertex shader for"))
                {
                    Debug.Assert(!readingDisassembly);
                    vertProgram = "";
                    parseState = ParseState.eReadVertProgram;
                    continue;
                }
                
                if (line.StartsWith("-- Fragment shader for"))
                {
                    Debug.Assert(!readingDisassembly);
                    fragProgram = "";
                    parseState = ParseState.eReadFragProgram;
                    continue;
                }

                if (line.StartsWith("Shader Disassembly"))
                {
                    readingDisassembly = true;
                    continue;
                }
                
                switch (parseState)
                {
                    case ParseState.eReadSubShader:
                        subShaderInfo += line;
                        break;
                    case ParseState.eReadPassInfo:
                        passInfo += line;
                        break;
                    case ParseState.eReadVertProgram:
                        if (readingDisassembly)
                            vertProgram += line + "\n";
                        break;
                    case ParseState.eReadFragProgram:
                        if (readingDisassembly)
                        {
                            fragProgram += line + "\n";
                            if (line.StartsWith("fragment Mtl_FragmentOut xlatMtlMain"))
                            {
                                readingFragBlock = true;
                            }

                            if (readingFragBlock && line == "}")
                            {
                                readingFragBlock = false;
                                readingDisassembly = false;
                                parseState = ParseState.eInvalid;
                                
                                CompiledMetalShader compiledMetalShader = new CompiledMetalShader(subShaderInfo, passCounter, passInfo, keywords, vertProgram, fragProgram);
                                Debug.Assert(!retMapping.ContainsKey(compiledMetalShader.identify), "Identify exist!!" + compiledMetalShader.identify);
                                retMapping[compiledMetalShader.identify] = compiledMetalShader;
                            }
                        }
                        break;
                }
            }
            return retMapping;
        }
    }
}

