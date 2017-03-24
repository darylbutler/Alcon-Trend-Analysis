using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlconTrendAnalysis
{
	public class TagValue
	{
		public long Time;
		public double Value;

		public TagValue(long time, double val)
		{
			Time = time;
			Value = val;
		}
	}
}
