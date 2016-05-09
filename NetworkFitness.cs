/*
 * Created by: Silver Phoenix
 * Created: domingo, 1 de Julho de 2007
 */

using System;

namespace dasp.Neat
{
	public class NetworkFitness
	{
		#region FIELDS

		public readonly NeuralNetwork Network;
		public readonly double Fitness;

	#endregion

		#region PROPERTIES

	#endregion

		#region CONSTRUCTORS

		public NetworkFitness(NeuralNetwork network, double fitness)
		{
			if(fitness < 0)
				throw new ArgumentException("Fitness cannot be negative.");

			Network = network;
			Fitness = fitness;
		}

	#endregion

		#region METHODS

		public override string ToString()
		{
			return Fitness.ToString();
		}

	#endregion
	}
}