//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StylizedWater2
{
    public class HelpWindow : EditorWindow
    {
        //Window properties
        private static int width = 440;
        private static int height = 350;

        //Tabs
        private bool isTabInstallation = true;
        private bool isTabDocumentation = true;
        private bool isTabSupport = false;

        public ShaderConfigurator.FogConfiguration fogConfig;

        [MenuItem("Help/Stylized Water 2", false, 0)]
        public static void ShowWindow()
        {
            HelpWindow editorWindow = EditorWindow.GetWindow<HelpWindow>(true, "Help", true);
            editorWindow.titleContent = new GUIContent("Help");
            editorWindow.autoRepaintOnSceneChange = true;

            //Open somewhat in the center of the screen
            editorWindow.position = new Rect((Screen.width) / 2f + width, (Screen.height) / 2f, (width * 2), height);

            //Fixed size
            editorWindow.maxSize = new Vector2(width, height);
            editorWindow.minSize = new Vector2(width, 200);

            //Init
            editorWindow.Init();

            editorWindow.Show();
        }

        public void Init()
        {
            AssetInfo.VersionChecking.CheckForUpdate(false);
            AssetInfo.VersionChecking.CheckUnityVersion();
            StylizedWaterEditor.DWP2.CheckInstallation();
            
            ShaderConfigurator.RefreshShaderFilePaths();
            ShaderConfigurator.GetCurrentFogConfiguration();
            fogConfig = ShaderConfigurator.CurrentFogConfiguration;
        }

        private void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(5);
            DrawTabs();
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (isTabInstallation) DrawInstallation();

            if (isTabDocumentation) DrawDocumentation();

            if (isTabSupport) DrawSupport();

            //DrawActionButtons();

            EditorGUILayout.EndVertical();

            DrawFooter();
        }

        void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=24>" + AssetInfo.ASSET_NAME + "</size>", UI.Styles.Header);

            GUILayout.Label("Version: " + AssetInfo.INSTALLED_VERSION, UI.Styles.Footer);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();

            Texture2D dot = null;
            if (AssetInfo.compatibleVersion && !AssetInfo.untestedVersion) dot = UI.Styles.SmallGreenDot;
            else if (AssetInfo.untestedVersion || !AssetInfo.IS_UPDATED) dot = UI.Styles.SmallOrangeDot;
            else if (!AssetInfo.compatibleVersion) dot = UI.Styles.SmallRedDot;

            if (GUILayout.Toggle(isTabInstallation, new GUIContent("Installation", dot), UI.Styles.Tab))
            {
                isTabInstallation = true;
                isTabDocumentation = false;
                isTabSupport = false;
            }

            if (GUILayout.Toggle(isTabDocumentation, "Documentation", UI.Styles.Tab))
            {
                isTabInstallation = false;
                isTabDocumentation = true;
                isTabSupport = false;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Toggle(isTabSupport, "Support", UI.Styles.Tab))
            {
                isTabInstallation = false;
                isTabDocumentation = false;
                isTabSupport = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawInstallation()
        {
            //SetWindowHeight(320f);

            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent(" Compiling scripts...", EditorGUIUtility.FindTexture("cs Script Icon")), UI.Styles.Header);

                EditorGUILayout.Space();
                return;
            }

            if (AssetInfo.compatibleVersion == false && AssetInfo.untestedVersion == false)
            {
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("This version of Unity is not supported.", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Please upgrade to at least Unity " + AssetInfo.MIN_UNITY_VERSION);
                return;
            }

            //Folder
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Unity version");

            Color defaultColor = GUI.contentColor;
            if (AssetInfo.compatibleVersion)
            {
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField("Compatible");
                GUI.contentColor = defaultColor;
            }
            else if (AssetInfo.untestedVersion)
            {
                GUI.contentColor = new Color(1f, 0.65f, 0f);
                EditorGUILayout.LabelField("Untested", EditorStyles.boldLabel);
                GUI.contentColor = defaultColor;
            }

            EditorGUILayout.EndHorizontal();
            if (AssetInfo.untestedVersion)
            {
                EditorGUILayout.LabelField("The current Unity version has not been tested yet, or compatibility is being worked on. You may run into issues.", EditorStyles.helpBox);
                EditorGUILayout.Space();
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                //PPSv2
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Target graphics API");

                if (PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)[0] != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 || PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS)[0] != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2)
                {
                    GUI.contentColor = Color.green;
                    EditorGUILayout.LabelField("OpenGL ES 3.0 or better");
                    GUI.contentColor = defaultColor;
                }
                else
                {
                    GUI.contentColor = Color.red;
                    EditorGUILayout.LabelField("OpenGL ES 2.0", EditorStyles.boldLabel);
                    GUI.contentColor = defaultColor;
                }
                EditorGUILayout.EndHorizontal();
            }

            //Version
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Package version");

            defaultColor = GUI.contentColor;
            if (AssetInfo.IS_UPDATED)
            {
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField("Up-to-date");
                GUI.contentColor = defaultColor;
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUI.contentColor = new Color(1f, 0.65f, 0f);
                EditorGUILayout.LabelField("Outdated", EditorStyles.boldLabel);

                GUI.contentColor = defaultColor;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (!AssetInfo.IS_UPDATED)
            {
                EditorGUILayout.LabelField("");
                if (GUILayout.Button(new GUIContent("Update package"), UI.Styles.UpdateText))
                {
                    AssetInfo.OpenStorePage();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Dynamic Water Physics 2");
            EditorGUILayout.LabelField(StylizedWaterEditor.DWP2.isInstalled ? "Installed" : "Not Installed", EditorStyles.boldLabel, GUILayout.MaxWidth((75f)));
            
            if (StylizedWaterEditor.DWP2.isInstalled && StylizedWaterEditor.DWP2.dataProviderUnlocked == false)
            {
                if (GUILayout.Button("Install integration"))
                {
                    StylizedWaterEditor.DWP2.UnlockDataProvider();
                    StylizedWaterEditor.DWP2.CheckInstallation();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Fog rendering");
            fogConfig = (ShaderConfigurator.FogConfiguration)EditorGUILayout.EnumPopup(fogConfig);
            if (GUILayout.Button("Change"))
            {
                ShaderConfigurator.SetFogConfiguration(fogConfig);
                fogConfig = ShaderConfigurator.CurrentFogConfiguration;
            }

            EditorGUILayout.EndHorizontal();

        }

        void DrawDocumentation()
        {
            //SetWindowHeight(335);

            EditorGUILayout.HelpBox("Please view the documentation for further details about this package and its workings.", MessageType.Info);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("<b><size=12>Documentation</size></b>\n<i>Usage instructions</i>", UI.Styles.Button))
            {
                Application.OpenURL(AssetInfo.DOC_URL);
            }
            if (GUILayout.Button("<b><size=12>Troubleshooting</size></b>\n<i>Common issues and solutions</i>", UI.Styles.Button))
            {
                Application.OpenURL(AssetInfo.DOC_URL + "?section=troubleshooting-10");
            }
            EditorGUILayout.EndHorizontal();

        }

        void DrawSupport()
        {
            //SetWindowHeight(350f);

            EditorGUILayout.BeginVertical(); //Support box

            EditorGUILayout.HelpBox("If you have any questions, or ran into issues, please get in touch.", MessageType.Info);

            EditorGUILayout.Space();

            //Buttons box
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<b><size=12>Email</size></b>\n<i>Contact</i>", UI.Styles.Button))
            {
                Application.OpenURL(AssetInfo.EMAIL_URL);
            }
            if (GUILayout.Button("<b><size=12>Twitter</size></b>\n<i>Follow developments</i>", UI.Styles.Button))
            {
                Application.OpenURL("https://twitter.com/search?q=staggart%20creations&f=user");
            }
            if (GUILayout.Button("<b><size=12>Forum</size></b>\n<i>Join the discussion</i>", UI.Styles.Button))
            {
                Application.OpenURL(AssetInfo.FORUM_URL);
            }
            EditorGUILayout.EndHorizontal();//Buttons box

            EditorGUILayout.EndVertical(); //Support box
        }

        private void DrawFooter()
        {
            //EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            GUILayout.Label("- Staggart Creations -", UI.Styles.Footer);
        }





    }
}