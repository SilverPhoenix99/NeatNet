/*
 * Created by: 
 * Created: domingo, 15 de Julho de 2007
 */

using System;

namespace dasp.Neat.ActivationFunction
{
	public class InverseAbsFunction: aActivationFunction
	{
		#region FIELDS

		public double Slope;

	#endregion

		#region CONSTRUCTORS

		public InverseAbsFunction(double slope, double min, double max)
			: base(min, max)
		{
			Slope = slope;
		}

		public InverseAbsFunction(double min, double max)
			: this(1, min, max)
		{}

		public InverseAbsFunction(double slope)
			: this(slope, 0, 1)
		{}

	#endregion

		#region METHODS

		protected override double NormalizedCalculation(double x)
		{
			return (1 + x / (Slope + Math.Abs(x))) * 0.5;
		}

	#endregion
	}
}