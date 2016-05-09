/*
 * Created by: Silver Phoenix
 * Created: terça-feira, 3 de Julho de 2007
 */

namespace dasp.Neat.ActivationFunction
{
	public class StepFunction: aActivationFunction
	{
		#region CONSTRUCTORS

		public StepFunction(double min, double max)
			: base(min, max)
		{}

		public StepFunction()
			: this(0, 1)
		{}

	#endregion

		#region METHODS

		protected override double NormalizedCalculation(double x)
		{
			return x < 0 ? Minimum : Maximum;
		}

	#endregion
	}
}