/*
 * Created by: Silver Phoenix
 * Created: domingo, 1 de Julho de 2007
 */

using System;
using System.Collections.Generic;

namespace dasp.Neat
{
	public class NeuronInnovation
	{
		#region FIELDS

		private readonly Dictionary<Tuple<int, int>, int> neuronIDs;

	#endregion

		#region PROPERTIES

		public int CurrentInnovation { get; private set; }

	#endregion

		#region CONSTRUCTORS

		public NeuronInnovation(int baseInnovation)
		{
			CurrentInnovation = baseInnovation;
			neuronIDs = new Dictionary<Tuple<int, int>, int>();
		}

		public NeuronInnovation()
			: this(0)
		{}

	#endregion

		#region METHODS

		/// <summary>
		/// Searches if a given connection already has a neuron associated with it.
		/// </summary>
		public bool HasInnovation(int source, int destination)
		{
			return neuronIDs.ContainsKey(new Tuple<int, int>(source, destination));
		}

		/// <summary>
		/// Reuses an id or returns a new id, for a neuron in a connection.
		/// </summary>
		public int GetInnovation(int source, int destination)
		{
            var key = new Tuple<int, int>(source, destination);
			if(neuronIDs.ContainsKey(key))
			{
				return neuronIDs[key];
			}

			int innovation = NextInnovation();
			neuronIDs.Add(key, innovation);
			return innovation;
		}

		/// <summary>
		/// Gives a new innovation id for neurons that aren't connected.
		/// </summary>
		public int NextInnovation()
		{
			int innovation = CurrentInnovation;
			CurrentInnovation++;
			return innovation;
		}

	#endregion
	}
}