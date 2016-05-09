/*
 * Created by: Silver Phoenix
 * Created: sábado, 30 de Junho de 2007
 */

using System;

namespace dasp.Neat.ActivationFunction
{
	public class SigmoidFunction: aActivationFunction
	{
		#region FIELDS

		// negative slope inverts the function: - inf -> 1, + inf -> 0
		public double Slope;

	#endregion

		#region CONSTRUCTORS

		public SigmoidFunction(double min, double max, double slope)
			: base(min, max)
		{
			Slope = slope;
		}

		public SigmoidFunction(double min, double max)
			: this(min, max, 1)
		{}

		public SigmoidFunction(double slope)
			: this(0, 1, slope)
		{}

		public SigmoidFunction()
			: this(0, 1, 1)
		{}

	#endregion

		#region METHODS

		protected override double NormalizedCalculation(double signal)
		{
			return 1.0 / (1.0 + Math.Exp(-Slope*signal));
		}

	#endregion
	}
}