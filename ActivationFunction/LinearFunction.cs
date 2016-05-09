/*
 * Created by: 
 * Created: sábado, 14 de Julho de 2007
 */

using System;

namespace dasp.Neat.ActivationFunction
{
	public class LinearFunction: aActivationFunction
	{
		#region FIELDS

		public double Left;
		public double Right;

	#endregion

		#region CONSTRUCTORS

		public LinearFunction(double left, double right, double min, double max)
			: base(min, max)
		{
			if(left > right)
				throw new ArgumentException("Left cannot be bigger than right.");

			Left = left;
			Right = right;
		}

		public LinearFunction(double left, double right)
			: this(left, right, 0, 1)
		{}

		public LinearFunction()
			: this(-2, 2, 0, 1)
		{}

	#endregion

		#region METHODS

		protected override double NormalizedCalculation(double x)
		{
			if(x < Left)
				return 0;
			if(x > Right)
				return 1;

			return (x - Left) / (Right - Left);
		}

	#endregion
	}
}