/*
 * Created by: Silver Phoenix
 * Created: sexta-feira, 29 de Junho de 2007
 */

using System;
using System.Diagnostics;
using System.Linq;
using dasp.Neat.ActivationFunction;
using System.Collections.Generic;

namespace dasp.Neat
{
	[Serializable]
	[DebuggerDisplay("[{ID}]: Synapses = {synapses.Count}, Bias = {Bias}")]
	public class Neuron : IComparable<Neuron>
	{
		#region FIELDS

		public readonly int ID;
		public aActivationFunction Function;
		public double Bias;

		private double nextOutput;

	#endregion

		#region PROPERTIES

		public SortedDictionary<int, Synapse> Synapses { get; }

		public double Output { get; private set; }

	#endregion

		#region CONSTRUCTORS

		public Neuron(int id, aActivationFunction function, double bias)
		{
			ID = id;
			Bias = bias;
			Function = function;
			Output = 0;
			nextOutput = 0;

            Synapses = new SortedDictionary<int, Synapse>();
		}

		public Neuron(int id, aActivationFunction function)
			: this(id, function, 0)
		{}

		protected Neuron(Neuron other)
		{
			ID = other.ID;
			Bias = other.Bias;
			Function = other.Function;
			Output = other.Output;
			nextOutput = other.nextOutput;

            Synapses = new SortedDictionary<int, Synapse>();

			foreach(var s in other.Synapses)
			{
                Synapses.Add(s.Key, s.Value.Clone());
			}
		}

	#endregion

		#region METHODS

		public double Calculate(double[] inputs, NeuralNetwork net)
		{
			double signal = 0;

            if ((from s in Synapses where s.Value.Enabled select 1).Count() == 0)
            {
                return nextOutput = Function.Calculate(Bias);
            }

			foreach(var s in Synapses)
			{
				if(!s.Value.Enabled)
					continue;

				signal += s.Value.Weight * (s.Value.Source < 0 ?
					inputs[s.Value.Source + net.NumInputs] : net.GetNeuron(s.Value.Source).Output);
			}

			return nextOutput = Function.Calculate(signal + Bias);
		}

		public void Update()
		{
			Output = nextOutput;
		}

		public void Clean()
		{
			Output = 0;
			nextOutput = 0;
		}

		public int CompareTo(Neuron other) => ID.CompareTo(other?.ID);

		public Neuron Clone() => new Neuron(this);

		public override string ToString() =>
			string.Format("[{0}]: Synapses = {1}, Bias = {2}", ID, Synapses.Count, Bias);

	#endregion
	}
}