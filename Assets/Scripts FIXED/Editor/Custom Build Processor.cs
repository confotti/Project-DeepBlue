using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class CustomBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get {return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        var db = Resources.Load<ItemDatabase>("Item Database");
        db.SetItemIDs();
        Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
    }
}
