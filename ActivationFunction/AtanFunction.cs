/*
 * Created by: 
 * Created: domingo, 15 de Julho de 2007
 */

using System;

namespace dasp.Neat.ActivationFunction
{
	public class AtanFunction: aActivationFunction
	{
		#region FIELDS

		public double Slope;

	#endregion

		#region CONSTRUCTORS

		public AtanFunction(double slope, double min, double max)
			: base(min, max)
		{
			Slope = slope;
		}

		public AtanFunction(double min, double max)
			: this(1, min, max)
		{}

		public AtanFunction(double slope)
			: this(slope, 0, 1)
		{}

	#endregion

		#region METHODS

		protected override double NormalizedCalculation(double x)
		{
			return 0.5 + Math.Atan(Slope * x) / Math.PI;
		}

	#endregion
	}
}