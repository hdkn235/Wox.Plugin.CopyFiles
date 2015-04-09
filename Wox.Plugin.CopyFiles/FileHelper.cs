using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Wox.Plugin.CopyFiles
{
    public class FileHelper
    {
        #region 从剪切板中复制文件
        /// <summary>
        /// 从剪切板中复制文件
        /// </summary>
        /// <param name="desPath">目标文件夹路径</param>
        private static void CopyFileFromClipbordThread(object desDirPath)
        {
            string strDesDirPath = desDirPath.ToString();
            foreach (var srcPath in Clipboard.GetFileDropList())
            {
                string desFilePath = Path.Combine(strDesDirPath, Path.GetFileName(srcPath));
                File.Copy(srcPath, desFilePath, true);
            }
        }



        /// <summary>
        /// 从剪切板中复制文件（在非winform中使用）
        /// </summary>
        /// <param name="desPath">目标文件夹路径</param>
        public static void CopyFileFromClipbord(string desDirPath)
        {
            Thread th = new Thread(new ParameterizedThreadStart(CopyFileFromClipbordThread));
            th.ApartmentState = ApartmentState.STA;//坑啊
            th.Start(desDirPath);
        }

        #endregion

        #region 判断路径是否合法
        /// <summary>
        /// 判断路径是否合法
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            try
            {
                if (!Directory.Exists(Path.GetFullPath(path)))//如果不存在就创建file文件夹　　             　　                
                    Directory.CreateDirectory(path);//创建该文件夹　　              
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
