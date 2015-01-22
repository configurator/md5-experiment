using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateData {
	class Program {
		private const string Usage = @"
Usage: {0} [method]
method can be one of:
- random: random data in the 
- real: generate random data, and for each block run MD5 and print its hash
- lines: for each line in stdin, calculate its MD5 hash and print.
";

		private static volatile int count;
		private static Stopwatch watch;

		private static MD5 MD5 = MD5.Create();
		private static Random random = new Random();

		static void Main(string[] args) {
			MethodInfo method = GetMethod(args);
			if (method == null) {
				Console.Error.WriteLine(Usage, typeof(Program).Assembly.GetName().Name);
				return;
			}

			int scale = 256;
			count = 0;
			watch = new Stopwatch();
			watch.Start();

			new Thread(PrintCountPeriodically) {
				IsBackground = true
			}.Start();

			while (true) {
				var cont = method.Invoke(null, new object[0]) as bool?;
				if (cont.HasValue && !cont.Value) {
					break;
				}

				count++;
				if (count % scale == 0) {
					PrintCount();
					if (count / scale >= 3) {
						scale *= 2;
					}
				}
			}
		}

		private static MethodInfo GetMethod(string[] args) {
			if (args.Length != 1) {
				return null;
			}

			return typeof(Program).GetMethod(args[0],
				BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
				null, Type.EmptyTypes, null);
		}

		public static void Random() {
			var hash = new byte[16];
			random.NextBytes(hash);
			Console.WriteLine(Convert.ToBase64String(hash));
		}

		public static void Real() {
			var buffer = new byte[4096];
			random.NextBytes(buffer);
			var hash = MD5.ComputeHash(buffer);
			Console.WriteLine(Convert.ToBase64String(hash));
		}

		public static bool Lines() {
			var line = Console.ReadLine();
			if (line == null) {
				return false;
			}
			var hash = MD5.ComputeHash(Encoding.UTF8.GetBytes(line));
			Console.WriteLine(Convert.ToBase64String(hash));
			return true;
		}

		private static void PrintCountPeriodically() {
			while (true) {
				Thread.Sleep(2000);
				PrintCount();
			}
		}

		private static void PrintCount() {
			Console.Error.WriteLine("{0} lines generated in {1}ms", count, watch.ElapsedMilliseconds);
		}
	}
}
