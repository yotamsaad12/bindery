using System;
namespace bindecy.Models
{
	public class OperationsByFile
	{
		public int NumberOfReadCalls { get; set; }
        public int NumberOfWriteCalls { get; set; }

		public OperationsByFile()
		{
			NumberOfReadCalls = 0;
			NumberOfWriteCalls = 0;
		}
    }
}

