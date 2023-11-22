using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

public class UnpackAssetBundle : MonoBehaviour
{
    // See example for selected and selection validation:
    // https://discussions.unity.com/t/how-to-add-context-menu-on-assets-and-asset-folder/136341/2
    // Verify you get an asset (multiple solutions):
    // https://forum.unity.com/threads/how-to-get-currently-selected-folder-for-putting-new-asset-into.81359/

    [MenuItem("Assets/Yoni - List Asset Bundle", validate = true)]
    private static bool ListAssets_Validate ()
    {
        return AssetDatabase.GetAssetPath(Selection.activeObject) != "";
    }

    [MenuItem("Assets/Yoni - List Asset Bundle")]
    private static void ListAssets()
    {
        object selected = Selection.activeObject;

        // Clear 
        AssetBundle.UnloadAllAssetBundles(true);

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            Debug.LogError("Yoni: can't select file");
            return;
        }
        else 
        {
            string filename = Path.GetFileName(path);

            AssetBundle _bundle = AssetBundle.LoadFromFile(path);

            Debug.Log("Bundle Name: " + _bundle.name);

            foreach(string s in _bundle.GetAllAssetNames())
            {
                Debug.Log("[ASSET] " + filename + ": " + s);
            }
        }

        // Clear 
        AssetBundle.UnloadAllAssetBundles(true);
    }


    [MenuItem("Assets/Yoni - Unpack Asset Bundle", validate = true)]
    private static bool UnpackAssets_Validate()
    {
        return AssetDatabase.GetAssetPath(Selection.activeObject) != "";
    }

    [MenuItem("Assets/Yoni - Unpack Asset PNGs")]
    private static void UnpackAssets()
    {
        object selected = Selection.activeObject;

        // Clear 
        AssetBundle.UnloadAllAssetBundles(true);

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            Debug.LogError("Yoni: can't select file");
            return;
        }
        else
        {
            string filename = Path.GetFileName(path);
            string folderName = path.Replace(filename, "") + "unpack_" +  filename  ;

            AssetBundle _bundle = AssetBundle.LoadFromFile(path);
            string[] _assets = _bundle.GetAllAssetNames();

            Directory.CreateDirectory(folderName);

            File.WriteAllText(folderName + "\\_name.txt", _bundle.name);
            File.WriteAllLines(folderName + "\\_files.txt", _assets);

            AssetDatabase.StartAssetEditing();
            try
            {
                int i = 0;
                foreach (string s in _assets)
                {
                    Debug.Log("[EXPORT] " + filename + ": " + s);

                    UnityEngine.Object _o = _bundle.LoadAsset(s);

                    Debug.Log("[Type] " + filename + ": " + _o.GetType().ToString());

                    
                    AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(_o), folderName + "\\" + i + ".asset");

                    //DestroyImmediate(_o, allowDestroyingAssets: true);

                    i++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            
        }

        // Clear 
        //AssetBundle.UnloadAllAssetBundles(true);
    }

    private static void GenerateAssetBundle(UnityEngine.Object[] selection, string bundleName, string dir_path)
    {
        string[] assetNameList = new string[selection.Length];
        int i = 0;
        foreach (UnityEngine.Object o in selection)
        {
            assetNameList[i++] = AssetDatabase.GetAssetPath(o);
        }

        GenerateAssetBundle(assetNameList, bundleName, dir_path);
    }

    private static void GenerateAssetBundle(string[] paths, string bundleName, string dir_path)
    {
        // https://forum.unity.com/threads/how-to-add-assets-created-procedurally-to-an-assetbundle.354052/

        
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0] = new AssetBundleBuild();
        buildMap[0].assetBundleName = bundleName;
        buildMap[0].assetNames = paths;


        //BuildPipeline.BuildAssetBundleExplicitAssetNames(
        //    new Object[] { },
        //    new string[] { }, "",
        //    0,
        //    BuildTarget.NoTarget);


        BuildPipeline.BuildAssetBundles(dir_path, buildMap, 
            BuildAssetBundleOptions.ChunkBasedCompression, 
            BuildTarget.StandaloneWindows);
    }

    // https://docs.unity3d.com/ScriptReference/ImageConversion.html



    [MenuItem("Assets/Yoni - Pack bad")]
    private static void Pack1()
    {
        AssetBundle.UnloadAllAssetBundles(true);

        GameObject _g = new GameObject();
        YonixwDepsTrick _t = _g.AddComponent<YonixwDepsTrick>();
        _t.myRefs = new Object[]
        {
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AAA/01.png"),
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AAA/fire.png")
        };
        GameObject _p = PrefabUtility.SaveAsPrefabAsset(_g, "Assets/AAA/Copy.prefab"); // must end with .prefab
        GenerateAssetBundle(
            new string[] { "Assets/AAA/Copy.prefab" },
            "bad_png_example",
            "Assets/AAA/");
    }


    [MenuItem("Assets/Yoni - [dec-bg023] Rebundle")]
    private static void CreateExampleRef()
    {
        AssetBundle.UnloadAllAssetBundles(true);

        AssetBundle _bundle = AssetBundle.LoadFromFile("Assets/bg023/dec-bg023.unity3d");
        string[] _assets = _bundle.GetAllAssetNames();
        List<string> _newNames = new List<string>();
        List<Object> _objs = new List<Object>();
        int i = 0;
        foreach (string s in _assets)
        {
            Object[] _all = _bundle.LoadAssetWithSubAssets(s);
            for (int j = 0; j < _all.Length; j++)
            {
                Debug.Log(s + " i=" + i + " j=" + j + " " + _all[j].GetType().ToString());

                if (_all[j] is Texture2D)
                {
                    string replacePath = "Assets/bg023/" + i + "." + j + ".png";
                    if (File.Exists(replacePath))
                    {
                        byte[] replacePng = AssetDatabase.LoadAssetAtPath<Texture2D>(replacePath).EncodeToPNG();
                        Debug.Log("Loading for " + s + ": " + replacePath);
                        ((Texture2D)_all[j]).LoadImage(replacePng);
                    }
                    else
                    {
                        // Force read image?
                        ((Texture2D)_all[j]).LoadImage(((Texture2D)_all[j]).EncodeToPNG());
                    }
                }

                _newNames.Add(s); // "Container name" - shared for stuff under same (prefabs, images etc.)
                _objs.Add(_all[j]);

            }
            i++;
        }

        //GameObject _go = new GameObject();
        //YonixwDepsTrick _tricks = _go.AddComponent<YonixwDepsTrick>();
        //_tricks.myRefs = new Object[] { Instantiate(_objs[0]) };

        BuildPipeline.BuildAssetBundleExplicitAssetNames(
            _objs.ToArray(),
            _newNames.ToArray(),
            "Assets/bg023/dec-bg023-rebundle.unity3d",
            BuildAssetBundleOptions.CollectDependencies,
            BuildTarget.StandaloneWindows
        );

        _bundle.Unload(true);


        // Result: should be with deps with their "full" path
    }
}

#endif