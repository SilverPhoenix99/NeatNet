/*
 * Created by: Silver Phoenix
 * Created: sábado, 7 de Julho de 2007
 */

using CenterSpace.Free;

namespace dasp.Neat.Operators
{
	public abstract class aMutationOperator
	{
		protected MersenneTwister mRandom;
		protected NeuronInnovation mInnovationManager;

		public aMutationOperator(MersenneTwister rand, NeuronInnovation innovationManager)
		{
			mRandom = rand;
			mInnovationManager = innovationManager;

			InitSettings();
		}

		protected abstract void InitSettings();

		public abstract void MutateInPlace(NeuralNetwork nn);

		public NeuralNetwork Mutate(NeuralNetwork nn)
		{
			NeuralNetwork clone = nn.Clone();
			MutateInPlace(clone);
			return clone;
		}
	}
}