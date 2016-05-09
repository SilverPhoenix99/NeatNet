/*
 * Created by: Silver Phoenix
 * Created: domingo, 1 de Julho de 2007
 */

using System;
using System.Collections.Generic;
using CenterSpace.Free;
using dasp.Utils;

namespace dasp.Neat.Operators
{
	public class CrossoverOperator: aCrossoverOperator
	{
		#region SETTINGS

		public const string GROUP = "dasp.Neat.Crossover";
		public const string PROP_MEAN_FITNESS = "MeanFitness";
		public const string PROP_REENABLE = "Synapse.ReEnable";

		public bool MeanFitness = false;
		public double ReEnableSynapse = 0.5;

	#endregion

		#region CONSTRUCTORS

		public CrossoverOperator(MersenneTwister rand)
			: base(rand)
		{}

		protected override void InitSettings()
		{
			Settings settings = Settings.Instance;

			settings.TryGetValue(GROUP, PROP_MEAN_FITNESS, ref MeanFitness);
			settings.TryGetValue(GROUP, PROP_REENABLE, ref ReEnableSynapse);
		}

	#endregion

		#region METHODS

		public override NeuralNetwork Crossover(NeuralNetwork mom, double momFitness,
												NeuralNetwork dad, double dadFitness)
		{
			int inputs  = mom.NumInputs;
			NeuralNetwork child = new NeuralNetwork(inputs);

			if(chooseBest(mom, momFitness, dad, dadFitness) == 1)
			{
				NeuralNetwork tempnet = mom;
				mom = dad;
				dad = tempnet;

				double tempfit = momFitness;
				momFitness = dadFitness;
				dadFitness = tempfit;
			}

			double Pb = momFitness / (momFitness + dadFitness);

			IEnumerator<Neuron> best  = mom.OutputNeurons.GetEnumerator();
			IEnumerator<Neuron> worst = dad.OutputNeurons.GetEnumerator();
			addNeurons(best, worst, child.OutputNeurons, Pb);

			best  = mom.HiddenNeurons.GetEnumerator();
			worst = dad.HiddenNeurons.GetEnumerator();
			addNeurons(best, worst, child.HiddenNeurons, Pb);

			return child;
		}

		private int chooseBest(NeuralNetwork mom, double momFitness,
							   NeuralNetwork dad, double dadFitness)
		{
			double mF = momFitness;
			double dF = dadFitness;
			int mC = mom.OutputNeurons.Count;
			int dC = dad.OutputNeurons.Count;

			if(mC != dC)
				throw new ArgumentException("mom and dad have different number of outputs");

			mC += mom.HiddenNeurons.Count;
			dC += dad.HiddenNeurons.Count;

			return (mF > dF || (mF == dF && (mC < dC || (mC == dC && mRandom.NextBool()))))
				? 0 : 1;
		}

		private void addNeurons(IEnumerator<Neuron> bestIt, IEnumerator<Neuron> worstIt,
			ICollection<Neuron> child, double Pb)
		{
			bool bestHasNext  = bestIt.MoveNext();
			bool worstHasNext = worstIt.MoveNext();
			while(bestHasNext)
			{
				Neuron best = bestIt.Current;

				if(!worstHasNext)
				{	// excess at best. add all synapses
					child.Add(best.Clone());
					bestHasNext = bestIt.MoveNext();
					continue;
				}

				Neuron worst = worstIt.Current;

				if(best.ID < worst.ID)
				{	// disjoint at best. add all synapses
					child.Add(best.Clone());
					bestHasNext = bestIt.MoveNext();
				}
				else if(best.ID == worst.ID)
				{	// matching neurons. select synapses
					selectSynapses(best, worst, child, Pb);
					bestHasNext  = bestIt.MoveNext();
					worstHasNext = worstIt.MoveNext();
				}
				else // best.ID > worst.ID
				{	// disjoint at worst. ignore
					worstHasNext = worstIt.MoveNext();
				}
			}
		}

		private void selectSynapses(Neuron bestNeuron, Neuron worstNeuron,
			ICollection<Neuron> neurons, double Pb)
		{
			// select random neuron
			Neuron n = mRandom.NextBool() ? bestNeuron : worstNeuron;
			Neuron clone = new Neuron(n.ID, n.Function, n.Bias);

			IEnumerator<KeyValuePair<int, Synapse>> bestIt = bestNeuron.Synapses.GetEnumerator();
			IEnumerator<KeyValuePair<int, Synapse>> worstIt = worstNeuron.Synapses.GetEnumerator();
			bool bestHasNext  = bestIt.MoveNext();
			bool worstHasNext = worstIt.MoveNext();
			while(bestHasNext)
			{
				var best = bestIt.Current.Value;

				if(!worstHasNext)
				{	// excess at best. add the synapse
					clone.Synapses.Add(best.Source, best.Clone());
					bestHasNext = bestIt.MoveNext();
					continue;
				}

				var worst = worstIt.Current.Value;

				if(best.Source < worst.Source)
				{	// disjoint at best. add the synapse
					clone.Synapses.Add(best.Source, best.Clone());
					bestHasNext = bestIt.MoveNext();
				}
				else if(best.Source == worst.Source)
				{	// matching synapses. select random synapse
					Synapse s;
					if(MeanFitness)
					{
						s = best.Clone();
						s.Weight = Pb * best.Weight + (1.0 - Pb) * worst.Weight;
					}
					else
					{
						s = mRandom.NextBool() ? best.Clone() : worst.Clone();
					}

					if(!best.Enabled && !worst.Enabled)
					{
						s.Enabled = mRandom.NextDouble() < ReEnableSynapse;
					}

					clone.Synapses.Add(s.Source, s);

					bestHasNext  = bestIt.MoveNext();
					worstHasNext = worstIt.MoveNext();
				}
				else // best.Source > worst.Source
				{	// disjoint at worst. ignore
					worstHasNext = worstIt.MoveNext();
				}
			}

			neurons.Add(clone);
		}

	#endregion
	}
}