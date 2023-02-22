namespace GenshinImpact.MonsterMap.Script
{
    public class InfoModel
    {
        public struct RECT
        {
            public int Left;                             //leftmost coordinate
            public int Top;                             //top coordinate
            public int Right;                           //rightmost coordinate
            public int Bottom;                        //lowest coordinate
        }
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
    }
}
