using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wox.Plugin.CopyFiles
{
    public class Main : IPlugin
    {
        #region Private Property
        private PluginInitContext context;
        private readonly string IcoPathStr = "Images/plugin.png";
        private readonly string TipStr = "Clipboard to copy the files to the specified directory";
        private readonly string TipCountStr = "This directory has been copied {0} times";
        #endregion

        public void Init(PluginInitContext context)
        {
            this.context = context;
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            List<Result> pushedResults = new List<Result>();
            string actionKeyword = context.CurrentPluginMetadata.ActionKeyword;
            if (query.RawQuery.Trim() == actionKeyword)
            {
                //加载所有的历史记录到列表中
                IEnumerable<Result> history = FileStorage.Instance.FileHistory.OrderByDescending(o => o.Value)
                    .Select(m => new Result
                    {
                        Title = m.Key,
                        SubTitle = string.Format(TipCountStr, m.Value),
                        IcoPath = IcoPathStr,
                        Action = (c) =>
                        {
                            ExcuteCopyFile(m.Key);
                            return true;
                        }
                    }).Take(5);

                if (history.Count() > 0)
                {
                    results.AddRange(history);
                }
                else
                {
                    results.Add(new Result()
                    {
                        Title = actionKeyword + " <Directory Path>",
                        SubTitle = TipStr,
                        IcoPath = IcoPathStr
                    });
                }
            }

            string path = query.GetAllRemainingParameter();
            if (query.RawQuery.StartsWith(actionKeyword) && !string.IsNullOrEmpty(path.Trim()))
            {
                //加载输入的路径到列表中
                Result result = new Result
                {
                    Title = path,
                    Score = 5000,
                    SubTitle = TipStr,
                    IcoPath = IcoPathStr,
                    Action = (c) =>
                    {
                        ExcuteCopyFile(path);
                        return true;
                    }
                };

                //根据插件的元信息将result集合加入到控件的列表中
                context.API.PushResults(query, context.CurrentPluginMetadata, new List<Result>() { result });
                pushedResults.Add(result);

                //加载匹配的历史记录到列表中
                IEnumerable<Result> history = FileStorage.Instance.FileHistory
                    .Where(o => o.Key.Contains(path))
                    .OrderByDescending(o => o.Value)
                    .Select(m =>
                    {
                        if (m.Key == path)
                        {
                            result.SubTitle = string.Format(TipCountStr, m.Value);
                            return null;
                        }

                        var ret = new Result
                        {
                            Title = m.Key,
                            SubTitle = string.Format(TipCountStr, m.Value),
                            IcoPath = IcoPathStr,
                            Action = (c) =>
                            {
                                ExcuteCopyFile(m.Key);
                                return true;
                            }
                        };
                        return ret;
                    }).Where(o => o != null).Take(4);

                context.API.PushResults(query, context.CurrentPluginMetadata, history.ToList());
                pushedResults.AddRange(history);

                //加载可能匹配的路径
                try
                {
                    string basedir = null;
                    string dir = null;
                    if (Directory.Exists(path) && (path.EndsWith("/") || path.EndsWith(@"\")))
                    {
                        basedir = Path.GetFullPath(path);
                        dir = Path.GetFullPath(path);
                    }
                    else if (Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        basedir = Path.GetDirectoryName(path);
                        dir = Path.GetFullPath(path);
                    }

                    if (basedir != null)
                    {
                        List<string> autocomplete = Directory.GetDirectories(basedir)
                            .Where(o =>
                                o.StartsWith(dir, StringComparison.OrdinalIgnoreCase)
                                && !results.Any(p => o.Equals(p.Title.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase))
                                && !pushedResults.Any(p => o.Equals(p.Title.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase))
                                ).ToList();
                        autocomplete.Sort();
                        results.AddRange(autocomplete.ConvertAll(m => new Result()
                        {
                            Title = m,
                            SubTitle = TipStr,
                            IcoPath = IcoPathStr,
                            Action = (c) =>
                            {
                                ExcuteCopyFile(m);
                                return true;
                            }
                        }));
                    }
                }
                catch (Exception) { }
            }

            return results;
        }

        /// <summary>
        /// 从剪贴板中复制文件到指定的目录中
        /// </summary>
        /// <param name="path"></param>
        private void ExcuteCopyFile(string path)
        {
            if (FileHelper.IsValidPath(path))
            {
                FileHelper.CopyFileFromClipbord(path);
                FileStorage.Instance.AddFileHistory(path);
            }
            else
            {
                context.API.ShowMsg("The path is not legitimate", "Please check the path!",string.Empty);
            }
        }
    }
}
