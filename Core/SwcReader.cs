using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace SenseAction.Core
{
    public static class SwcReader
    {
        private const float SWC_TO_PNG_SX = 0.7875f;
        private const float SWC_TO_PNG_SY = 0.7494f;
        private const float SWC_TO_PNG_OX = 117f;
        private const float SWC_TO_PNG_OY = 101f;

        public static Point[] LoadPath(string filePath, float canvasSx, float canvasSy)
        {
            float SWC_TO_PNG_SX = 0.7875f;
            float SWC_TO_PNG_SY = 0.7494f;
            float SWC_TO_PNG_OX = 117f;
            float SWC_TO_PNG_OY = 101f;

            float finalSx = SWC_TO_PNG_SX * canvasSx;
            float finalSy = SWC_TO_PNG_SY * canvasSy;
            int finalOx = (int)Math.Round(SWC_TO_PNG_OX * canvasSx);
            int finalOy = (int)Math.Round(SWC_TO_PNG_OY * canvasSy);

            var points = new List<Point>();
            foreach (string line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 7) continue;

                float rawX = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                float rawY = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);

                points.Add(new Point(
                    (int)Math.Round((rawX * finalSx) + finalOx),
                    (int)Math.Round((rawY * finalSy) + finalOy)
                ));
            }

            if (points.Count > 0)
            {
                float cx = 0, cy = 0;
                foreach (var p in points)
                {
                    cx += p.X;
                    cy += p.Y;
                }
                cx /= points.Count;
                cy /= points.Count;

                float stretchX = 1.20f; 
                float stretchY = 1.00f;

                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = new Point(
                        (int)Math.Round(cx + (points[i].X - cx) * stretchX),
                        (int)Math.Round(cy + (points[i].Y - cy) * stretchY)
                    );
                }
            }

            return points.ToArray();
        }

        public static Point[] LoadPathRaw(string filePath)
        {
            var points = new List<Point>();

            foreach (string line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(
                    new[] { ' ', '\t' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 7) continue;

                points.Add(new Point(
                    (int)float.Parse(parts[2], CultureInfo.InvariantCulture),
                    (int)float.Parse(parts[3], CultureInfo.InvariantCulture)));
            }

            return points.ToArray();
        }
    }
}