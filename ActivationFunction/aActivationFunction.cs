/*
 * Created by: Silver Phoenix
 * Created: sábado, 30 de Junho de 2007
 */

using System;

namespace dasp.Neat.ActivationFunction
{
	public abstract class aActivationFunction
	{
		#region FIELDS

		public double Minimum;
		public double Maximum;

	#endregion

		#region CONSTRUCTORS

		public aActivationFunction(double min, double max)
		{
			if(min > max)
				throw new MethodAccessException("Min cannot be bigger than max");

			Minimum = min;
			Maximum = max;
		}

		public aActivationFunction()
			: this(0, 1)
		{}

	#endregion

		#region METHODS

		public double Calculate(double x)
		{
			return (Maximum - Minimum) * NormalizedCalculation(x) + Minimum;
		}

		protected abstract double NormalizedCalculation(double x);

	#endregion
	}
}