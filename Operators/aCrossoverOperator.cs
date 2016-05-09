/*
 * Created by: Silver Phoenix
 * Created: sábado, 7 de Julho de 2007
 */

using CenterSpace.Free;

namespace dasp.Neat.Operators
{
	public abstract class aCrossoverOperator
	{
		protected MersenneTwister mRandom;

		public aCrossoverOperator(MersenneTwister rand)
		{
			mRandom = rand;

			InitSettings();
		}

		public abstract NeuralNetwork Crossover(NeuralNetwork mom, double momFitness,
												NeuralNetwork dad, double dadFitness);

		public NeuralNetwork Crossover(NetworkFitness mom, NetworkFitness dad)
		{
			return Crossover(mom.Network, mom.Fitness, dad.Network, dad.Fitness);
		}

		protected abstract void InitSettings();
	}
}