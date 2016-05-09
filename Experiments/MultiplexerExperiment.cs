/*
 * Created by: Silver Phoenix
 * Created: domingo, 8 de Julho de 2007
 */

using CenterSpace.Free;
using dasp.Neat.ActivationFunction;
using dasp.Utils;

namespace dasp.Neat.Experiments
{
	public class MultiplexerExperiment: aExperiment
	{
		#region SETTINGS

		public const string GROUP = "dasp.Neat.Experiment.Multiplexer";
		public const string PROP_TRIALS = "Trials";
		public const string PROP_ENTRIES = "Entries";

		private int Trials = 10;
		private int Entries = 1;

	#endregion

		#region FIELDS

		private readonly int outputId;
		private readonly aActivationFunction function;
		private readonly int inputs;

	#endregion

		#region CONSTRUCTORS

		public MultiplexerExperiment(MersenneTwister rand, NeuronInnovation innovationManager)
			: base(rand, innovationManager)
		{
			outputId = base.innovationManager.NextInnovation();
			function = new SigmoidFunction(5);
			inputs = 1 << Entries;
		}

		protected override void InitSettings(Settings settings)
		{
			settings.TryGetValue(GROUP, PROP_TRIALS, ref Trials);
			settings.TryGetValue(GROUP, PROP_ENTRIES, ref Entries);
		}

	#endregion

		#region METHODS

		public override NeuralNetwork Build()
		{
			NeuralNetwork nn = new NeuralNetwork(Entries + inputs);

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
			double[] inputValues = new double[Entries + inputs];

			nn.Clean();

			for(int i = 0; i < inputs; i++)
			{
				for(int j = 0; j < Entries; j++)
				{
					inputValues[j] = 0x01 & i >> j;
				}

				for(int j = 0; j < inputs; j++)
				{
					inputValues[j + Entries] = random.NextBool() ? 1 : 0;
				}

				for(int j = 0; j < Trials; j++)
				{
					nn.Execute(inputValues);
				}

				fitness += inputValues[i + Entries] == 0 ? 1 - nn.Output[0] : nn.Output[0];
			}

			return fitness < 1 ? 0.1 : fitness * fitness;
		}

	#endregion
	}
}