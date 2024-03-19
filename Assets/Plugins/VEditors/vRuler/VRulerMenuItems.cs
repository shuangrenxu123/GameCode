#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static VRuler.Libs.VUtils;
using static VRuler.Libs.VGUI;


namespace VRuler
{
    class VRulerMenuItems
    {
        public static bool rulerEnabled { get => EditorPrefs.GetBool("vRuler-rulerEnabled", true); set => EditorPrefs.SetBool("vRuler-rulerEnabled", value); }
        public static bool boundsEnabled { get => EditorPrefs.GetBool("vRuler-boundsEnabled", true); set => EditorPrefs.SetBool("vRuler-boundsEnabled", value); }
        public static bool objectsForScaleEnabled { get => EditorPrefs.GetBool("vRuler-objectsForScaleEnabled", true); set => EditorPrefs.SetBool("vRuler-objectsForScaleEnabled", value); }

        public static bool imperialSystemEnabled { get => EditorPrefs.GetBool("vRuler-imperialSystemEnabled", false); set => EditorPrefs.SetBool("vRuler-imperialSystemEnabled", value); }

        public static int decimalPoints { get => EditorPrefs.GetInt("vRuler-decimalPoints", 1); set => EditorPrefs.SetInt("vRuler-decimalPoints", value); }



        const string menuDir = "Tools/vRuler/";


        const string ruler = menuDir + "Hold Shift-R and Move Mouse to show ruler";
        const string bounds = menuDir + "Hold Shift-R and Click to measure bounds";
        const string objectsForScale = menuDir + "Hold Shift-R and Scroll to draw objects for scale";

        const string metricSystem = menuDir + "Use metric system";
        const string imperialSystem = menuDir + "Use imperial system";




        [MenuItem(ruler, false, 1)] static void dadsadadsas() => rulerEnabled = !rulerEnabled;
        [MenuItem(ruler, true, 1)] static bool dadsaddasadsas() { UnityEditor.Menu.SetChecked(ruler, rulerEnabled); return true; }

        [MenuItem(bounds, false, 2)] static void dadsaadsdadsas() => boundsEnabled = !boundsEnabled;
        [MenuItem(bounds, true, 2)] static bool dadsadadsdasadsas() { UnityEditor.Menu.SetChecked(bounds, boundsEnabled); return true; }

        [MenuItem(objectsForScale, false, 3)] static void dadsaadsdadsdasas() => objectsForScaleEnabled = !objectsForScaleEnabled;
        [MenuItem(objectsForScale, true, 3)] static bool dadsadadsddsaasadsas() { UnityEditor.Menu.SetChecked(objectsForScale, objectsForScaleEnabled); return true; }



        [MenuItem(metricSystem, false, 101)] static void dadadssaadsdadsdasas() => imperialSystemEnabled = !imperialSystemEnabled;
        [MenuItem(metricSystem, true, 101)] static bool dadsadsadadsddsaasadsas() { UnityEditor.Menu.SetChecked(metricSystem, !imperialSystemEnabled); return true; }

        [MenuItem(imperialSystem, false, 102)] static void dadadsasdsaadsdadsdasas() => imperialSystemEnabled = !imperialSystemEnabled;
        [MenuItem(imperialSystem, true, 102)] static bool dadsadssadadadsddsaasadsas() { UnityEditor.Menu.SetChecked(imperialSystem, imperialSystemEnabled); return true; }



        [MenuItem(menuDir + "Show more decimal points", false, 1001)] static void qdadadsssa() { decimalPoints++; Debug.Log("vRuler: now showing " + decimalPoints + " decimal points"); }
        [MenuItem(menuDir + "Show less decimal points", false, 1002)] static void qdadadadssssa() { if (decimalPoints > 0) decimalPoints--; Debug.Log("vRuler: now showing " + decimalPoints + " decimal points"); }



        [MenuItem(menuDir + "Join our Discord", false, 10001)]
        static void dadsas() => Application.OpenURL("https://discord.gg/4dG9KsbspG");

        [MenuItem(menuDir + "Get the rest of our Editor Ehnancers with a discount", false, 10002)]
        static void dadsadsas() => Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/editor-enhancers-bundle-251318?aid=1100lGLBn&pubref=menu");





        [MenuItem(menuDir + "Disable vRuler", false, 100001)]
        static void das() => ToggleDefineDisabledInScript(typeof(VRuler));
        [MenuItem(menuDir + "Disable vRuler", true, 100001)]
        static bool dassadc() { UnityEditor.Menu.SetChecked(menuDir + "Disable vRuler", ScriptHasDefineDisabled(typeof(VRuler))); return true; }





    }
}
#endif