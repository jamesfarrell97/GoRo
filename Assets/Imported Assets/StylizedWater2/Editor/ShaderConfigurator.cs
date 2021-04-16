//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StylizedWater2
{
    public class ShaderConfigurator
    {
        private const string ShaderGUID = "f04c9486b297dd848909db26983c9ddb";
        private static string ShaderFilePath;
        private const string FogLibraryGUID = "8427feba489ff354ab7b22c82a15ba03";
        private static string FogLibraryFilePath;

        public enum FogConfiguration
        {
            UnityFog,
            Enviro,
            Azure,
            AtmosphericHeightFog
        }

        public static FogConfiguration CurrentFogConfiguration
        {
            get { return (FogConfiguration)SessionState.GetInt("SWS2_FOG_INTEGRATION", (int)FogConfiguration.UnityFog); }
            set { SessionState.SetInt("SWS2_FOG_INTEGRATION", (int)value); }
        }

        public static void GetCurrentFogConfiguration()
        {
            FogConfiguration config;
            FogConfiguration.TryParse(GetConfiguration(FogLibraryFilePath), out config);
            
            CurrentFogConfiguration = config;
        }

        public static void SetFogConfiguration(FogConfiguration config)
        {
            if(config == FogConfiguration.UnityFog) ConfigureUnityFog();
            if(config == FogConfiguration.Enviro) ConfigureEnviroFog();
            if(config == FogConfiguration.Azure) ConfigureAzureFog();
            if(config == FogConfiguration.AtmosphericHeightFog) ConfigureAtmosphericHeightFog();
        }

        private struct CodeBlock
        {
            public int startLine;
            public int endLine;
        }

        public static void RefreshShaderFilePaths()
        {
            ShaderFilePath = AssetDatabase.GUIDToAssetPath(ShaderGUID);
            FogLibraryFilePath = AssetDatabase.GUIDToAssetPath(FogLibraryGUID);
        }
        
        public static void ConfigureUnityFog()
        {
            RefreshShaderFilePaths();

            EditorUtility.DisplayProgressBar("Stylized Water 2", "Modifying shader...", 1f);
            {
                ToggleCodeBlock(FogLibraryFilePath, "UnityFog", true);
                ToggleCodeBlock(FogLibraryFilePath, "Enviro", false);
                ToggleCodeBlock(FogLibraryFilePath, "Azure", false);
                ToggleCodeBlock(FogLibraryFilePath, "AtmosphericHeightFog", false);

                //multi_compile keyword
                ToggleCodeBlock(ShaderFilePath, "UnityFog", true);
                ToggleCodeBlock(ShaderFilePath, "AtmosphericHeightFog", false);

            }
            EditorUtility.ClearProgressBar();

            CurrentFogConfiguration = FogConfiguration.UnityFog;
            Debug.Log("Shader file modified to use " + CurrentFogConfiguration + " rendering");
        }
        
        public static void ConfigureEnviroFog()
        {
            RefreshShaderFilePaths();

            EditorUtility.DisplayProgressBar("Stylized Water 2", "Modifying shader...", 1f);
            {
                ToggleCodeBlock(FogLibraryFilePath, "UnityFog", false);
                ToggleCodeBlock(FogLibraryFilePath, "Enviro", true);
                ToggleCodeBlock(FogLibraryFilePath, "Azure", false);
                ToggleCodeBlock(FogLibraryFilePath, "AtmosphericHeightFog", false);

                //multi_compile keyword
                ToggleCodeBlock(ShaderFilePath, "UnityFog", false);
                ToggleCodeBlock(ShaderFilePath, "AtmosphericHeightFog", false);

            }
            EditorUtility.ClearProgressBar();

            CurrentFogConfiguration = FogConfiguration.Enviro;
            Debug.Log("Shader file modified to use " + CurrentFogConfiguration + " rendering");
        }
        
        public static void ConfigureAzureFog()
        {
            RefreshShaderFilePaths();

            EditorUtility.DisplayProgressBar("Stylized Water 2", "Modifying shader...", 1f);
            {
                ToggleCodeBlock(FogLibraryFilePath, "UnityFog", false);
                ToggleCodeBlock(FogLibraryFilePath, "Enviro", false);
                ToggleCodeBlock(FogLibraryFilePath, "Azure", true);
                ToggleCodeBlock(FogLibraryFilePath, "AtmosphericHeightFog", false);

                //multi_compile keyword
                ToggleCodeBlock(ShaderFilePath, "UnityFog", false);
                ToggleCodeBlock(ShaderFilePath, "AtmosphericHeightFog", false);


            }
            EditorUtility.ClearProgressBar();

            CurrentFogConfiguration = FogConfiguration.Azure;
            Debug.Log("Shader file modified to use " + CurrentFogConfiguration + " rendering");
        }
        
        public static void ConfigureAtmosphericHeightFog()
        {
            RefreshShaderFilePaths();

            EditorUtility.DisplayProgressBar("Stylized Water 2", "Modifying shader...", 1f);
            {
                ToggleCodeBlock(FogLibraryFilePath, "UnityFog", false);
                ToggleCodeBlock(FogLibraryFilePath, "Enviro", false);
                ToggleCodeBlock(FogLibraryFilePath, "Azure", false);
                ToggleCodeBlock(FogLibraryFilePath, "AtmosphericHeightFog", true);
                
                //multi_compile keyword
                ToggleCodeBlock(ShaderFilePath, "UnityFog", false);
                ToggleCodeBlock(ShaderFilePath, "AtmosphericHeightFog", true);

            }
            EditorUtility.ClearProgressBar();

            CurrentFogConfiguration = FogConfiguration.AtmosphericHeightFog;
            Debug.Log("Shader file modified to use " + CurrentFogConfiguration + " rendering");
        }

        //TODO: Process multiple keywords in one read/write pass
        private static void ToggleCodeBlock(string filePath, string id, bool enable)
        {
            string[] lines = File.ReadAllLines(filePath);

            List<CodeBlock> codeBlocks = new List<CodeBlock>();

            //Find start and end line indices
            for (int i = 0; i < lines.Length; i++)
            {
                bool blockEndReached = false;

                if (lines[i].Contains("/* Configuration: ") && enable)
                {
                    lines[i] = lines[i].Replace(lines[i], "/* Configuration: " + id + " */");
                }

                if (lines[i].Contains("start " + id))
                {
                    CodeBlock codeBlock = new CodeBlock();

                    codeBlock.startLine = i;

                    //Find related end point
                    for (int l = codeBlock.startLine; l < lines.Length; l++)
                    {
                        if (blockEndReached == false)
                        {
                            if (lines[l].Contains("end " + id))
                            {
                                codeBlock.endLine = l;

                                blockEndReached = true;
                            }
                        }
                    }

                    codeBlocks.Add(codeBlock);
                    blockEndReached = false;
                }

            }

            if (codeBlocks.Count == 0)
            {
                //Debug.Log("No code blocks with the marker \"" + id + "\" were found in file");

                return;
            }

            foreach (CodeBlock codeBlock in codeBlocks)
            {
                if (codeBlock.startLine == codeBlock.endLine) continue;

                //Debug.Log((enable ? "Enabled" : "Disabled") + " \"" + id + "\" code block. Lines " + (codeBlock.startLine + 1) + " through " + (codeBlock.endLine + 1));

                for (int i = codeBlock.startLine + 1; i < codeBlock.endLine; i++)
                {
                    //Uncomment lines
                    if (enable == true)
                    {
                        if (lines[i].StartsWith("//") == true) lines[i] = lines[i].Remove(0, 2);
                    }
                    //Comment out lines
                    else
                    {
                        if (lines[i].StartsWith("//") == false) lines[i] = "//" + lines[i];
                    }
                }
            }

            File.WriteAllLines(filePath, lines);

            AssetDatabase.ImportAsset(filePath);
        }

        private static string GetConfiguration(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            string configStr = lines[0].Replace("/* Configuration: ", string.Empty);
            configStr = configStr.Replace(" */", string.Empty);

            return configStr;
        }
    }
}