using System;
using System.Drawing;

namespace SenseAction.Rendering
{
    public sealed class HandFrameSet : IDisposable
    {
        public Bitmap BaseFrame { get; }
        public Bitmap TransitionFrame { get; }
        public Bitmap FinalFrame { get; }

        public HandFrameSet(Bitmap baseFrame, Bitmap transitionFrame, Bitmap finalFrame)
        {
            BaseFrame = baseFrame;
            TransitionFrame = transitionFrame;
            FinalFrame = finalFrame;
        }

        public void Dispose()
        {
            TransitionFrame?.Dispose();
            FinalFrame?.Dispose();
        }
    }
}