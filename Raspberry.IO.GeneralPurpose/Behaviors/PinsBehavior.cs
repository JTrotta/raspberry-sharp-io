﻿#region References

using System.Collections.Generic;
using System.Linq;
using System.Threading;

#endregion

namespace Raspberry.IO.GeneralPurpose.Behaviors
{
    /// <summary>
    /// Represents the pins behavior base class.
    /// </summary>
    public abstract class PinsBehavior
    {
        #region Fields

        private readonly Timer timer;
        private int currentStep;

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="PinsBehavior"/> class.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        protected PinsBehavior(IEnumerable<PinConfiguration> configurations)
        {
            Configurations = configurations.ToArray();
            Interval = 250;

            timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        public PinConfiguration[] Configurations { get; private set; }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>
        /// The interval.
        /// </value>
        public int Interval { get; set; }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the connection.
        /// </summary>
        protected GpioConnection Connection { get; private set; }

        /// <summary>
        /// Gets the first step.
        /// </summary>
        /// <returns>The first step.</returns>
        protected abstract int GetFirstStep();

        /// <summary>
        /// Processes the step.
        /// </summary>
        /// <param name="step">The step.</param>
        protected abstract void ProcessStep(int step);

        /// <summary>
        /// Tries to get the next step.
        /// </summary>
        /// <param name="step">The step.</param>
        /// <returns><c>true</c> if the behavior may continue; otherwise behavior will be stopped.</returns>
        protected abstract bool TryGetNextStep(ref int step);

        #endregion

        #region Internal Methods

        internal void Start(GpioConnection connection)
        {
            Connection = connection;
            foreach (var pinConfiguration in Configurations)
                connection[pinConfiguration] = false;

            currentStep = GetFirstStep();
            timer.Change(0, Interval);
        }

        internal void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);

            foreach (var pinConfiguration in Configurations)
                Connection[pinConfiguration] = false;
        }

        #endregion

        #region Private Helpers

        private void OnTimer(object state)
        {
            ProcessStep(currentStep);
            if (!TryGetNextStep(ref currentStep))
            {
                Thread.Sleep(Interval);
                Stop();
            }
        }

        #endregion
    }
}