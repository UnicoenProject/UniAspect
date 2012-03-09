using System;
using System.Linq;
using Unicoen;

namespace UniAspect {
	public class Program {
		private const string Usage =
				"Usage: aries <rootDirectoryPath> <aspectFilePath>\nyou should specify missing parameter(s).";

		/* 
		 * UniAspect targetDir aspect.apt
		 *  :  args[0] -> ウィーブ対象フォルダのパス
		 *  :  args[1] -> アスペクトファイルのパス
		 */
		private static void Main(string[] args) {
			// 引数が合わない場合はUsageを表示して終了する
			if(args.Length != 2) {
				Console.WriteLine(Usage);
				Environment.Exit(0);
			}

			// 配布用コード：コマンドラインからファイルのパスを取得する場合のコード
			var directoryPath = args[0];
			var aspectPath = args[1];

			// テスト用コード：以下にファイルのパスを直接指定する
//			const string directoryPath = "";
//			const string aspectPath = "../../fixture/Aspect/input/partial_aspect/before_execution.txt";

			// アスペクトファイルの解析
			Weaver.AnalizeAspect(aspectPath);
			// アスペクトの合成処理実行
			Weaver.Run(directoryPath);
		}
	}
}