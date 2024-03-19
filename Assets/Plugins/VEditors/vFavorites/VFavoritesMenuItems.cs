#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static VFavorites.Libs.VUtils;
using static VFavorites.Libs.VGUI;


namespace VFavorites
{
    class VFavoritesMenuItems
    {
        const string menuDir = "Tools/vFavorites/";


        [MenuItem(menuDir + "Upgrade to vFavorites 2", false, 101)]
        static void dadsadsas() => Application.OpenURL("https://assetstore.unity.com/packages/slug/263643?aid=1100lGLBn&pubref=menuupgrade");

        [MenuItem(menuDir + "Join our Discord", false, 102)]
        static void dadsas() => Application.OpenURL("https://discord.gg/4dG9KsbspG");


        [MenuItem(menuDir + "Disable vFavorites", false, 1001)]
        static void das() => ToggleDefineDisabledInScript(typeof(VFavorites));
        [MenuItem(menuDir + "Disable vFavorites", true, 1001)]
        static bool dassadc() { UnityEditor.Menu.SetChecked(menuDir + "Disable vFavorites", ScriptHasDefineDisabled(typeof(VFavorites))); return true; }

    }
}
#endif