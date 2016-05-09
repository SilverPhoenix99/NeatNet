/*
 * Created by: Silver Phoenix
 * Created: domingo, 1 de Julho de 2007
 */

using System.Collections.Generic;
using CenterSpace.Free;
using dasp.Neat.ActivationFunction;
using dasp.Utils;

namespace dasp.Neat.Operators
{
	public class MutationOperator: aMutationOperator
	{
		#region SETTINGS

		public const string GROUP = "dasp.Neat.Mutation";

		public const string PROP_NEURON_MAX     = "Neuron.Max";
		public const string PROP_NEURON_ADD     = "Neuron.Add";

		public const string PROP_SYNAPSE_ADD    = "Synapse.Add";
		public const string PROP_SYNAPSE_REC    = "Synapse.Recursive";

		public const string PROP_BIAS_CHANGE    = "Bias.Change";

		public const string PROP_WEIGHT_CHANGE  = "Weight.Change";
		public const string PROP_WEIGHT_REPLACE = "Weight.Replace";
		public const string PROP_WEIGHT_PERTURB = "Weight.Perturb";
		public const string PROP_WEIGHT_MIN     = "Weight.Min";
		public const string PROP_WEIGHT_MAX     = "Weight.Max";

		public int MaxNeurons    = 0;
		public double AddNeuron  = 0.5;

		public double AddSynapse = 0.5;
		public bool RecursiveSynapse = false;

		public bool ChangeBias = true;

		public double ChangeWeights = 0.5;
		public double ReplaceWeight = 0.5;
		public double PerturbWeight = 0.5;
		public double MinWeight = -1.0;
		public double MaxWeight = 1.0;

	#endregion

		#region CONSTRUCTORS

		public MutationOperator(MersenneTwister random, NeuronInnovation innovationManager)
			: base(random, innovationManager)
		{}

		protected override void InitSettings()
		{
			Settings settings = Settings.Instance;

			settings.TryGetValue(GROUP, PROP_NEURON_MAX, ref MaxNeurons);
			settings.TryGetValue(GROUP, PROP_NEURON_ADD, ref AddNeuron);
			settings.TryGetValue(GROUP, PROP_SYNAPSE_ADD, ref AddSynapse);
			settings.TryGetValue(GROUP, PROP_SYNAPSE_REC, ref RecursiveSynapse);
			settings.TryGetValue(GROUP, PROP_BIAS_CHANGE, ref ChangeBias);
			settings.TryGetValue(GROUP, PROP_WEIGHT_CHANGE, ref ChangeWeights);
			settings.TryGetValue(GROUP, PROP_WEIGHT_REPLACE, ref ReplaceWeight);
			settings.TryGetValue(GROUP, PROP_WEIGHT_PERTURB, ref PerturbWeight);
			settings.TryGetValue(GROUP, PROP_WEIGHT_MIN, ref MinWeight);
			settings.TryGetValue(GROUP, PROP_WEIGHT_MAX, ref MaxWeight);
		}

	#endregion

		#region METHODS

		public override void MutateInPlace(NeuralNetwork nn)
		{
			if(ChangeWeights > 0 && mRandom.NextDouble() < ChangeWeights)
			{
				changeWeights(nn);
			}

			int count = nn.NumInputs + nn.Count;
			List<int> ids = new List<int>(count);
			buildNeuronList(nn, ids);

			if(AddNeuron > 0 && nn.HiddenNeurons.Count < MaxNeurons && mRandom.NextDouble() < AddNeuron)
			{
				addNeuron(nn, ids);
			}

			if(AddSynapse > 0 && mRandom.NextDouble() < AddSynapse)
			{
				addSynapse(nn, ids);
			}
		}

		private void changeWeights(NeuralNetwork nn)
		{
			if(ReplaceWeight > 0 && mRandom.NextDouble() < ReplaceWeight)
			{
				foreach(Neuron n in nn.GetNeurons())
				{
					if(ChangeBias)
						n.Bias = mRandom.NextDouble(MinWeight, MaxWeight);
					foreach(var s in n.Synapses)
					{
						s.Value.Weight = mRandom.NextDouble(MinWeight, MaxWeight);
					}
				}

				return;
			}
			
			if(PerturbWeight <= 0)
				return;

			foreach(Neuron n in nn.GetNeurons())
			{
				if(ChangeBias)
					perturbWeight(ref n.Bias);
				foreach(var s in n.Synapses)
				{
					perturbWeight(ref s.Value.Weight);
				}
			}
		}

		private void perturbWeight(ref double weight)
		{
			weight *= 1.0 + mRandom.NextDouble(-PerturbWeight, PerturbWeight);
			if(weight > MaxWeight)
				weight = MaxWeight;
			else if(weight < MinWeight)
				weight = MinWeight;
		}

		private static void buildNeuronList(NeuralNetwork nn, ICollection<int> ids)
		{
			for(int i = 0; i < nn.NumInputs; i++)
				ids.Add(i - nn.NumInputs);
			foreach(Neuron n in nn.GetNeurons())
				ids.Add(n.ID);
		}

		private void addNeuron(NeuralNetwork nn, IList<int> ids)
		{
			int to = ids[mRandom.Next(nn.NumInputs, ids.Count)];
			Neuron dest = nn.GetNeuron(to);

			List<int> sources = new List<int>(dest.Synapses.Count);
			foreach(var s in dest.Synapses)
			{
				if(s.Value.Enabled)
					sources.Add(s.Key);
			}

			if(sources.Count == 0)
				return;

			int from = sources[mRandom.Next(sources.Count)];

			int newId = mInnovationManager.GetInnovation(from, to);

			if(nn.GetNeuron(newId) != null)
				return;

			dest.Synapses[from].Enabled = false;

			aActivationFunction f = from < 0 ? dest.Function : nn.GetNeuron(from).Function;
			Neuron n = new Neuron(newId, f);

			double w = mRandom.NextDouble(MinWeight, MaxWeight);
			dest.Synapses.Add(newId, new Synapse(newId, w));

			w = mRandom.NextDouble(MinWeight, MaxWeight);
			n.Synapses.Add(newId, new Synapse(from, w));

			nn.HiddenNeurons.Add(n);
		}

		private void addSynapse(NeuralNetwork nn, IList<int> ids)
		{
			// heavy: O(n^2), n = # neurons + # inputs

			// build list of non existing synapses
			List<int[]> synapseList = new List<int[]>();

			foreach(Neuron n in nn.GetNeurons())
			{
				for(int i = 0; i < ids.Count; i++)
				{
					int id = ids[i];
					if(n.ID == id && !RecursiveSynapse)
						continue;

					if(!n.Synapses.ContainsKey(id))
					{
						// add synapse [to <- from]
						synapseList.Add(new int[] { n.ID, id });
					}
				}
			}

			if(synapseList.Count > 0)
			{
				int pos = mRandom.Next(synapseList.Count);
				int[] s = synapseList[pos];

				double w = mRandom.NextDouble(MinWeight, MaxWeight);
				nn.GetNeuron(s[0]).Synapses.Add(s[1], new Synapse(s[1], w));
			}
		}

	#endregion
	}
}