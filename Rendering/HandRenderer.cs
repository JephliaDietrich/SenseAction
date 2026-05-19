using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using SenseAction.Core;

namespace SenseAction.Rendering
{
    public class HandRenderer : IDisposable
    {
        private readonly HandFrameAnimator _frameAnimator;
        private List<NerveZone> _zones;

        public HandRenderer(HandFrameAnimator frameAnimator)
        {
            _frameAnimator = frameAnimator;
            _zones = BuildZones();
        }
        public void RebuildZones()
        {
            if (_zones != null)
            {
                foreach (NerveZone z in _zones)
                    z.Dispose();
            }
            _zones = BuildZones();
        }

        private List<NerveZone> BuildZones()
        {
            return new List<NerveZone>
            {
                new NerveZone(
                    "Supraclavicular nerves (C3-4)",
                    Color.FromArgb(80, 240, 230, 100),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.SupraClavicular)),

                new NerveZone(
                    "Axillary nerve — upper lateral cutaneous (C5-6)",
                    Color.FromArgb(80, 100, 160, 220),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.AxillarySupLat)),

                new NerveZone(
                    "Intercostobrachial nerve (T2)",
                    Color.FromArgb(80, 244, 155, 175),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.IntercostBrachial)),

                new NerveZone(
                    "Medial brachial cutaneous nerve (T1-2)",
                    Color.FromArgb(80, 180, 140, 100),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.MedialBrachialCut)),

                new NerveZone(
                    "Lateral antebrachial cutaneous nerve (C5-6)",
                    Color.FromArgb(80, 150, 200, 150),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.LateralAntebrachialCut)),

                new NerveZone(
                    "Medial antebrachial cutaneous nerve (C8-T1)",
                    Color.FromArgb(80, 244, 155, 175),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.MedialAntebrachialCut)),

                new NerveZone(
                    "Radial nerve — dorsal antebrachial cutaneous (C5-6)",
                    Color.FromArgb(80, 244, 160, 80),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.RadialDorsAntebrachCut)),

                new NerveZone(
                    "Radial nerve — superficial branch (C6-8)",
                    Color.FromArgb(80, 80, 200, 200),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.RadialSuperficial)),

                new NerveZone(
                    "Median nerve — palmar branch",
                    Color.FromArgb(80, 251, 191, 138),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.Palmar)),

                new NerveZone(
                    "Ulnar nerve — palmar branch",
                    Color.FromArgb(80, 100, 160, 220),
                    NerveZoneBuilder.BuildPath(NerveZoneCoordinates.PalmarBlue))
            };
        }

        public void Draw(Graphics g, int canvasWidth, int canvasHeight)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(Color.FromArgb(255, 224, 224, 224));

            Bitmap currentFrame = _frameAnimator?.CurrentFrame;
            if (currentFrame != null)
                g.DrawImage(currentFrame, 0, 0, canvasWidth, canvasHeight);

            if (_frameAnimator != null && _frameAnimator.IsBaseFrame)
            {
                foreach (NerveZone zone in _zones)
                    zone.Draw(g);
            }
        }

        public string GetClickedZone(Point p)
        {
            foreach (NerveZone zone in _zones)
                if (zone.Contains(p))
                    return zone.NerveName;
            return null;
        }

        public void Dispose()
        {
            if (_zones != null)
            {
                foreach (NerveZone z in _zones)
                    z.Dispose();
            }
        }
    } 
}
