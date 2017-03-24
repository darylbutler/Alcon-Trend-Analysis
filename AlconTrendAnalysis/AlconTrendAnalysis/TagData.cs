using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlconTrendAnalysis
{
	public class TagData
	{
		public string Name, Description;
		public long ID;
		public List<TagValue> Values = new List<TagValue>();
	}
}
