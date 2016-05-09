/*
 * Created by: Silver Phoenix
 * Created: sábado, 30 de Junho de 2007
 */

using System;
using System.Diagnostics;

namespace dasp.Neat
{
	[Serializable]
	[DebuggerDisplay("[{Source} Enabled = {Enabled}]: Weight = {Weight}")]
	public class Synapse: IComparable<Synapse>
	{
		#region FIELDS

		public bool Enabled;
		public readonly int Source;
		public double Weight;

	#endregion

		#region PROPERTIES

	#endregion

		#region CONSTRUCTORS

		public Synapse(int source, double weight)
		{
			Source = source;
			Weight = weight;
			Enabled = true;
		}

	#endregion

		#region METHODS

		public Synapse Clone()
		{
			Synapse s = new Synapse(Source, Weight) {Enabled = Enabled};
			return s;
		}

		public override string ToString()
		{
			return string.Format("[{0} {2}]: Weight = {1}", Source, Weight,
				Enabled ? 'E' : 'D');
		}

        public int CompareTo(Synapse other) => Source.CompareTo(other?.Source);

        #endregion
    }
}