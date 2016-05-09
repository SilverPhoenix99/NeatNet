/*
 * Created by: Silver Phoenix
 * Created: domingo, 8 de Julho de 2007
 */

using CenterSpace.Free;
using dasp.Neat.ActivationFunction;
using dasp.Utils;

namespace dasp.Neat.Experiments
{
	public class XORExperiment: aExperiment
	{
		#region SETTINGS

		public const string GROUP = "dasp.Neat.Experiment.XOR";
		public const string PROP_TRIALS = "Trials";

		private int Trials = 10;

	#endregion

		#region FIELDS

		private readonly int outputId;
		private readonly aActivationFunction function;

	#endregion

		#region CONSTRUCTORS

		public XORExperiment(MersenneTwister rand, NeuronInnovation innovationManager)
			: base(rand, innovationManager)
		{
			outputId = base.innovationManager.NextInnovation();
			//function = new InverseAbsFunction(0.18);
			function = new SigmoidFunction(5);
		}

		protected override void InitSettings(Settings settings)
		{
			settings.TryGetValue(GROUP, PROP_TRIALS, ref Trials);
		}

	#endregion

		#region METHODS

		public override NeuralNetwork Build()
		{
			NeuralNetwork nn = new NeuralNetwork(2);

			Neuron n = new Neuron(outputId, function, 1);

			for(int i = 0; i < 2; i++)
			{
				n.Synapses.Add(i - 2, new Synapse(i - 2, random.NextDouble(-1, 1)));
			}

			nn.OutputNeurons.Add(n);

			return nn;
		}

		public override double Evaluate(NeuralNetwork nn)
		{
			double fitness = 0.0;
			double[] inputs = new double[2];

			nn.Clean();

			for(int i = 0; i < 4; i++)
			{
				inputs[0] = i % 2;
				inputs[1] = (i % 4) / 2;

				for(int j = 0; j < Trials; j++)
				{
					nn.Execute(inputs);
				}

				fitness += inputs[0] == inputs[1] ? 1 - nn.Output[0] : nn.Output[0];
			}

			return fitness / 4.0;
		}

	#endregion
	}
}