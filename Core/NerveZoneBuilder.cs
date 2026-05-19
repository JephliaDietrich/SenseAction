using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SenseAction.Core
{
    public static class NerveZoneBuilder
    {
        private const float PNG_WIDTH = 1920f;
        private const float PNG_HEIGHT = 1080f;

        public static float ScaleX { get; private set; } = 1f;
        public static float ScaleY { get; private set; } = 1f;

        public static void SetCanvasSize(int width, int height)
        {
            ScaleX = (width > 0) ? width / PNG_WIDTH : 1f;
            ScaleY = (height > 0) ? height / PNG_HEIGHT : 1f;
        }

        public static GraphicsPath BuildPath(int[,] rawCoords)
        {
            int count = rawCoords.GetLength(0);
            if (count < 3)
                return new GraphicsPath();

            PointF[] points = new PointF[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = new PointF(
                    rawCoords[i, 0] * ScaleX,
                    rawCoords[i, 1] * ScaleY
                );
            }

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(points);
            return path;
        }

        public static GraphicsPath BuildCombinedHandZone()
        {
            GraphicsPath combined =
                new GraphicsPath();

            combined.AddPath(
                BuildPath(NerveZoneCoordinates.Palmar),
                false);

            combined.AddPath(
                BuildPath(NerveZoneCoordinates.PalmarBlue),
                false);

            combined.AddPath(
                BuildPath(
                    NerveZoneCoordinates.LateralAntebrachialCut),
                false);

            combined.AddPath(
                BuildPath(
                    NerveZoneCoordinates.MedialAntebrachialCut),
                false);

            combined.AddPath(
                BuildPath(
                    NerveZoneCoordinates.RadialDorsAntebrachCut),
                false);

            return combined;
        }
        public static Point GetCNSPoint()
        {
            const float CNS_PNG_X = 1850f;
            const float CNS_PNG_Y = 350f;
            return new Point(
                (int)(CNS_PNG_X * ScaleX),
                (int)(CNS_PNG_Y * ScaleY)
            );
        }
        public static PointF GetZoneCentroid(int[,] rawCoords)
        {
            float sumX = 0, sumY = 0;
            int n = rawCoords.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                sumX += rawCoords[i, 0];
                sumY += rawCoords[i, 1];
            }
            return new PointF(
                (sumX / n) * ScaleX,
                (sumY / n) * ScaleY
            );
        }

        
    }
}