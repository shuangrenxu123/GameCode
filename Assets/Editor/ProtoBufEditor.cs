using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;

public class ProtoBufTool
{
    private static string targetDir = Environment.CurrentDirectory + "Assets/HotfixScripts/Module/NetWork/Proto";
    private static string protobufFileDir = Environment.CurrentDirectory + "Assets/HotfixScripts/Module/NetWork/Proto";

    [MenuItem("打包/生成Protobuf类")]
    public static void AllProto2CS()
    {
        string rootDir = Environment.CurrentDirectory;

        string protoc;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            protoc = Path.Combine(protobufFileDir, "protoc.exe");
        }
        else
        {
            protoc = Path.Combine(protobufFileDir, "protoc");
        }
        //todo 判断目标文件夹是否存在

        string argument2 = $"--csharp_out=\"{targetDir}\" --proto_path=\"{protobufFileDir}\" P.proto";

        Run(protoc, argument2, waitExit: true);

        UnityEngine.Debug.Log("proto2cs succeed!");

        AssetDatabase.Refresh();
    }

    public static Process Run(string exe, string arguments, string workingDirectory = ".", bool waitExit = false)
    {
        try
        {
            bool redirectStandardOutput = true;
            bool redirectStandardError = true;
            bool useShellExecute = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                redirectStandardOutput = false;
                redirectStandardError = false;
                useShellExecute = true;
            }

            if (waitExit)
            {
                redirectStandardOutput = true;
                redirectStandardError = true;
                useShellExecute = false;
            }

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = useShellExecute,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = redirectStandardOutput,
                RedirectStandardError = redirectStandardError,
            };

            Process process = Process.Start(info);

            if (waitExit)
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception($"{process.StandardOutput.ReadToEnd()} {process.StandardError.ReadToEnd()}");
                }
            }

            return process;
        }
        catch (Exception e)
        {
            throw new Exception($"dir: {Path.GetFullPath(workingDirectory)}, command: {exe} {arguments}", e);
        }
    }

}
