namespace Kernel.Utils;

public static class DirectoryParser
{
    public static string ExpandTildePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        // 处理以 ~ 开头的路径
        if (path.StartsWith("~/") || path == "~")
        {
            // 获取用户主目录（跨平台方法）
            var homeDirectory = DirectoryParser.GetHomeDirectory();

            if (path == "~")
                return homeDirectory;

            // 组合路径并规范化
            return Path.Combine(homeDirectory, path.Substring(2)).Replace('\\', '/');
        }

        // 处理以 ~username 形式的路径（可选实现）
        if (path.StartsWith("~") && path.Length > 1 && !path[1].Equals('/'))
            throw new NotImplementedException("User-specific paths (~username) not implemented");

        return path;
    }

    private static string GetHomeDirectory() =>
        // 跨平台获取主目录的优先顺序
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) ?? // 标准 .NET 方法
        Environment.GetEnvironmentVariable("HOME") ?? // Linux/macOS
        Environment.GetEnvironmentVariable("USERPROFILE") ?? throw new NotSupportedException(); // Windows
}