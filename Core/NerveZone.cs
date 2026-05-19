using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseAction.Core
{
    public class NerveZone : IDisposable
    {
        public string NerveName { get; private set; }
        public Color ZoneColor {  get; private set; }
        public GraphicsPath Path { get; private set; }

        public NerveZone(string nerveName, Color zoneColor, GraphicsPath path)
        {
            NerveName = nerveName;
            ZoneColor = zoneColor;
            Path = path;
        }

        public bool Contains(Point point)
        {
            return Path != null && Path.IsVisible(point);
        }

        public void Draw(Graphics g)
        {
            if(Path == null) return;
            using(SolidBrush brush = new SolidBrush(ZoneColor))
                g.FillPath(brush, Path);
        }

        public void Dispose()
        {
            Path?.Dispose();
        }
    }
}
