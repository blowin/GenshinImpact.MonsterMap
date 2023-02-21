using System;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.窗体;
using OpenCvSharp;

namespace GenshinImpact.MonsterMap
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
