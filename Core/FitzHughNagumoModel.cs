using System;

namespace SenseAction.Core
{
    public class FitzHughNagumoModel
    {
        // Безрозмірні змінні моделі
        private double _v;
        private double _w;

        // Константи Фітцх'ю-Нагумо
        private readonly double _a = 0.7;
        private readonly double _b = 0.8;
        private readonly double _tau = 12.5;

        // Коефіцієнти для перетворення у реальні мілівольти (мВ)
        private readonly double _k = 35.0;
        private readonly double _vShift = -28.0;

        public FitzHughNagumoModel()
        {
            Reset();
        }

        // Властивість, яка повертає готове значення для графіка (у мВ)
        public double MembranePotential_mV => (_k * _v) + _vShift;

        public void Step(double iExt)
        {
            double dt = 0.1;

            int stepsPerTick = 15;

            for (int i = 0; i < stepsPerTick; i++)
            {
                double dv = _v - (Math.Pow(_v, 3) / 3.0) - _w + iExt;
                double dw = (_v + _a - _b * _w) / _tau;

                _v += dv * dt;
                _w += dw * dt;
            }
        }

        public void Reset()
        {
            // Повернення до базового стану спокою (~ -70 мВ)
            _v = -1.199;
            _w = -0.624;
        }
    }
}