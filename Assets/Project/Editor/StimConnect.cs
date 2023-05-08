using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;


public class StimConnect : EditorWindow
{
    [MenuItem("Tactility/Stimulator Connector")]
    public static void createMenu()
    {
        var window = GetWindow<StimConnect>("Stimulator Connector");
        window.titleContent = new GUIContent("Stimulator Connector");
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Project/Editor/StimConnectWindow.uxml");
        VisualElement tree = visualTree.Instantiate();
        root.Add(tree);
    }
}
