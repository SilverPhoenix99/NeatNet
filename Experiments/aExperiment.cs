/*
 * Created by: Silver Phoenix
 * Created: domingo, 8 de Julho de 2007
 */

using CenterSpace.Free;
using dasp.Utils;

namespace dasp.Neat.Experiments
{
	public abstract class aExperiment
	{
		protected MersenneTwister random;
		protected NeuronInnovation innovationManager;

		public aExperiment(MersenneTwister rand, NeuronInnovation innovationManager)
		{
			random = rand;
			this.innovationManager = innovationManager;

			InitSettings(Settings.Instance);
		}

		/// <summary>
		/// Initialize settings.
		/// </summary>
		protected abstract void InitSettings(Settings settings);

		/// <summary>
		/// Creates a "prototype" network.
		/// </summary>
		public abstract NeuralNetwork Build();

		/// <summary>
		/// Returns the fitness by evaluating the network.
		/// </summary>
		public abstract double Evaluate(NeuralNetwork nn);
	}
}