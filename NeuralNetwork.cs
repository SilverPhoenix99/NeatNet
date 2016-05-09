/*
 * Created by: Silver Phoenix
 * Created: sábado, 30 de Junho de 2007
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace dasp.Neat
{
	[Serializable]
	[DebuggerDisplay("Inputs = {NumInputs}; Hidden = {hidden.Count}; Outputs = {outputs.Count}")]
	public class NeuralNetwork
	{
		#region FIELDS

		public readonly int NumInputs;

	#endregion

		#region PROPERTIES

		public SortedSet<Neuron> HiddenNeurons { get; }

		public SortedSet<Neuron> OutputNeurons { get; }

		public double[] Output
		{
			get
			{
				var outs = new double[OutputNeurons.Count];
				var i = 0;
				foreach(var n in OutputNeurons)
				{
					outs[i++] = n.Output;
				}

				return outs;
			}
		}

		public int Count
		{
			get { return HiddenNeurons.Count + OutputNeurons.Count; }
		}

	#endregion

		#region CONSTRUCTORS

		public NeuralNetwork(int numInputs)
		{
			NumInputs = numInputs;

            HiddenNeurons = new SortedSet<Neuron>();
            OutputNeurons = new SortedSet<Neuron>();
		}

	#endregion

		#region METHODS

		public void Execute(double[] inputs)
		{
			if(inputs.Length != NumInputs)
				throw new ArgumentException("The number of inputs in parameter doesn't match.");

			foreach(var neuron in HiddenNeurons)
				neuron.Calculate(inputs, this);

			foreach(var neuron in OutputNeurons)
				neuron.Calculate(inputs, this);

			foreach(Neuron neuron in GetNeurons())
				neuron.Update();
		}

		public Neuron GetNeuron(int id)
		{
			foreach(Neuron neuron in GetNeurons())
			{
				if(neuron.ID == id)
					return neuron;
			}

			return null;
		}

		public void Clean()
		{
			foreach(Neuron n in GetNeurons())
				n.Clean();
		}

		public NeuralNetwork Clone()
		{
			NeuralNetwork nn = new NeuralNetwork(NumInputs);

			foreach(var neuron in HiddenNeurons)
				nn.HiddenNeurons.Add(neuron.Clone());

			foreach(var neuron in OutputNeurons)
				nn.OutputNeurons.Add(neuron.Clone());

			return nn;
		}

		public IEnumerable<Neuron> GetNeurons()
		{
			foreach(var neuron in OutputNeurons)
			{
				yield return neuron;
			}

			foreach(var neuron in HiddenNeurons)
			{
				yield return neuron;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("net").AppendLine("{");

			// inputs
			sb.AppendFormat("\tinputs = {0}", NumInputs).AppendLine();

			// neurons and synapses
			sb.AppendLine("\toutputs").AppendLine("\t{");
			appendNeurons(OutputNeurons, sb);
			sb.AppendLine("\t}");

			if(HiddenNeurons.Count > 0)
			{
				sb.AppendLine("\thidden").AppendLine("\t{");
				appendNeurons(HiddenNeurons, sb);
				sb.AppendLine("\t}");
			}

			sb.Append("}");
			return sb.ToString();
		}

		private static void appendNeurons(IEnumerable<Neuron> neurons, StringBuilder sb)
		{
			foreach(var n in neurons)
			{
				sb.AppendFormat("\t\t[{0}] ->", n.ID).AppendLine().AppendLine("\t\t{");
				sb.AppendFormat("\t\t\tBias = {0}", n.Bias).AppendLine();
				foreach(var s in n.Synapses)
				{
					sb.Append("\t\t\t").AppendLine(s.Value.ToString());
				}
				sb.AppendLine("\t\t}");
			}
		}

	#endregion
	}
}