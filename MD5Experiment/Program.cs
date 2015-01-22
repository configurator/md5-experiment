using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MD5Experiment {
	class Program {
		private class Hash {
			private int part1;
			private int part2;
			private int part3;
			private int part4;

			public override bool Equals(object obj) {
				if (!(obj is Hash)) {
					return false;
				}
				var hash = (Hash)obj;
				return this.part1 == hash.part1
					&& this.part2 == hash.part2
					&& this.part3 == hash.part3
					&& this.part4 == hash.part4;
			}

			public override int GetHashCode() {
				return part1 ^ part2 ^ part3 ^ part4;
			}

			public override string ToString() {
				var bytes = BitConverter.GetBytes(part1)
					.Concat(BitConverter.GetBytes(part2))
					.Concat(BitConverter.GetBytes(part3))
					.Concat(BitConverter.GetBytes(part4));
				return Convert.ToBase64String(bytes.ToArray());
			}

			public static implicit operator Hash(string s) {
				var bytes = Convert.FromBase64String(s);
				return new Hash {
					part1 = BitConverter.ToInt32(bytes, 0),
					part2 = BitConverter.ToInt32(bytes, 4),
					part3 = BitConverter.ToInt32(bytes, 8),
					part4 = BitConverter.ToInt32(bytes, 12)
				};
			}

			public static implicit operator string (Hash s) {
				return s.ToString();
			}
		}

		private static volatile int count;
		private static Stopwatch watch;

		private static void Main() {
			var linesSeen = new HashSet<Hash>();

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
			var mem = Process.GetCurrentProcess().WorkingSet64;
			Console.WriteLine("{0:#,##0} unique lines stored in {1:#,##0}ms. Current memory usage: {2:#,##0}, average {3:0.0} bytes per line.",
				count,
				watch.ElapsedMilliseconds,
				mem,
				(double)mem / count);
		}
	}
}
