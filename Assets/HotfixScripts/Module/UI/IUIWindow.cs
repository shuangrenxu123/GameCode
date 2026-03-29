using UnityEngine;

namespace UIWindow
{
    public interface IUIWindow
    {
        string WindowName { get; }
        Canvas CanvasComponent { get; }
        CanvasGroup CanvasGroup { get; }
        UIWindowGroup UIGroup { get; }
        bool PauseCoveredWindow { get; }
        int DepthInUIGroup { get; }
        int SortingOrderOffset { get; }

        void OnInit(object userData);
        void OnOpen(object userData);
        void OnClose(object userData);
        void OnPause();
        void OnResume();
        void OnCover();
        void OnReveal();
        void OnRefocus(object userData);
        void OnUpdate();
        void OnDepthChanged(int depthInUIGroup);
    }
}
