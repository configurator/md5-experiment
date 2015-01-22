using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MD5Experiment {
	class Program {
		private static volatile int count;
		private static Stopwatch watch;

		private static void Main() {
			HashSet<string> linesSeen = new HashSet<string>();

			int scale = 256;
			count = linesSeen.Count;
			watch = new Stopwatch();
			watch.Start();

			new Thread(PrintCountPeriodically) {
				IsBackground = true
			}.Start();

			try {
				while (true) {
					var line = Console.ReadLine();
					if (line == null) {
						break;
					}
					if (!linesSeen.Add(line.Trim())) {
						Console.Write("Collision!");
					}
					count = linesSeen.Count;
					if (count % scale == 0) {
						PrintCount();
						if (count / scale >= 3) {
							scale *= 2;
						}
					}
				}
				Console.Write("Exiting successfully ");
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
			} finally {
				PrintCount();
			}
		}

		public static void PrintCountPeriodically() {
			while (true) {
				Thread.Sleep(2000);
				PrintCount();
			}
		}

		private static void PrintCount() {
			Console.WriteLine("{0} unique lines stored in {1}ms", count, watch.ElapsedMilliseconds);
		}
	}
}
