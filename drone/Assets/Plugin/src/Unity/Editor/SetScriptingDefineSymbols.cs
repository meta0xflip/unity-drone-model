using UnityEditor;

[InitializeOnLoad]
public class SetScriptingDefineSymbols
{
    static SetScriptingDefineSymbols()
    {
        // Android��NO_USE_GRPC��ǉ�
        SetSymbolForGroup(BuildTargetGroup.Android, "NO_USE_GRPC");
        SetSymbolForGroup(BuildTargetGroup.Standalone, "NO_USE_GRPC");
        SetSymbolForGroup(BuildTargetGroup.WebGL, "NO_USE_GRPC");
        SetSymbolForGroup(BuildTargetGroup.iOS, "NO_USE_GRPC");

        // ���̃O���[�v�̐ݒ�������ɋL�q
    }

    static void SetSymbolForGroup(BuildTargetGroup group, string symbol)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (!defines.Contains(symbol))
        {
            defines += ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }
}
