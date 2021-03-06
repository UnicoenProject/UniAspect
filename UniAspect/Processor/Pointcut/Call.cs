﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using UniAspect.Processor.Pointcut;
using Unicoen.Model;

namespace UniAspect.Processor.Pointcut {
	[Export(typeof(CodeProcessor))]
	public class Call : CodeProcessor{
		/// <summary>
		///   指定された関数呼び出しの直前に、指定されたコードを共通コードモデルとして挿入します。
		/// </summary>
		/// <param name = "root">コードを追加するモデルのルートノード</param>
		/// <param name = "regex">対象関数を指定する正規表現</param>
		/// <param name = "advice">挿入するコード断片</param>
		public static void InsertAtBeforeCall(
				UnifiedElement root, Regex regex, UnifiedBlock advice) {
			//get cass list
			var calls = root.Descendants<UnifiedCall>().ToList();

			//親要素がUnifiedBlockの場合に、その関数呼び出しは単項式であると判断する。
			foreach (var call in calls) {
				//プロパティでない関数呼び出しのみを扱う
				//e.g. write()はOK. Math.max()はNG.
				var functionName = call.Function as UnifiedIdentifier;
				if (functionName == null)
					continue;

				// 現状ではToString()とのマッチングを行う。
				var m = regex.Match(functionName.Name);
				if (!m.Success)
					continue;

				//(Javaにおいて)関数呼び出しの親ノードがブロックの場合、それは単独である
				//(JavaScriptにおいて)関数呼び出しの親ノードがブロックの場合、それは単独である
				var block = call.Parent as UnifiedBlock;
				if (block != null)
					block.Insert(block.IndexOf(call, 0), advice.DeepCopy());
			}
		}

		/// <summary>
		///   指定された関数呼び出しの後に、指定されたコードを共通コードモデルとして挿入します。
		/// </summary>
		/// <param name = "root">コードを追加するモデルのルートノード</param>
		/// <param name = "regex">対象関数を指定する正規表現</param>
		/// <param name = "advice">挿入するコード断片</param>
		public static void InsertAtAfterCall(
				UnifiedElement root, Regex regex, UnifiedBlock advice) {
			//get cass list
			var calls = root.Descendants<UnifiedCall>().ToList();

			//親要素がUnifiedBlockの場合に、その関数呼び出しは単項式であると判断する。
			foreach (var call in calls) {
				//プロパティでない関数呼び出しのみを扱う
				//e.g. write()はOK. Math.max()はNG.
				var functionName = call.Function as UnifiedIdentifier;
				if (functionName == null)
					continue;

				var m = regex.Match(functionName.Name);
				if (!m.Success)
					continue;

				//(Javaにおいて)関数呼び出しの親ノードがブロックの場合、それは単独である
				//(JavaScriptにおいて)関数呼び出しの親ノードがブロックの場合、それは単独である
				var block = call.Parent as UnifiedBlock;
				if (block != null)
					block.Insert(block.IndexOf(call, 0) + 1, advice.DeepCopy());
			}
		}

		/// <summary>
		///   すべての関数呼び出しの直前に、指定されたコードを共通コードモデルとして挿入します。
		/// </summary>
		/// <param name = "root">コードを追加するモデルのルートノード</param>
		/// <param name = "advice">挿入するコード断片</param>
		public static void InsertAtBeforeCallAll(
				UnifiedElement root, UnifiedBlock advice) {
			InsertAtBeforeCall(root, new Regex(".*"), advice);
		}

		/// <summary>
		///   すべての関数呼び出しの後に、指定されたコードを共通コードモデルとして挿入します。
		/// </summary>
		/// <param name = "root">コードを追加するモデルのルードノード</param>
		/// <param name = "advice">挿入するコード断片</param>
		public static void InsertAtAfterCallAll(
				UnifiedElement root, UnifiedBlock advice) {
			InsertAtAfterCall(root, new Regex(".*"), advice);
		}

		/// <summary>
		///   指定された関数呼び出しの直前に、指定されたコードを共通コードモデルとして挿入します。
		/// </summary>
		/// <param name = "root">コードを追加するモデルのルートノード</param>
		/// <param name = "name">対象関数の名前</param>
		/// <param name = "advice">挿入するコード断片</param>
		public static void InsertAtBeforeCallByName(
				UnifiedElement root, string name, UnifiedBlock advice) {
			InsertAtBeforeCall(root, new Regex("^" + name + "$"), advice);
		}

		/// <summary>
		///   指定された関数呼び出しの後に、指定されたコードを共通コードモデルとして挿入します。
		/// </summary>
		/// <param name = "root">コードを追加するモデルのルードノード</param>
		/// <param name = "name">対象関数の名前</param>
		/// <param name = "advice">挿入するコード断片</param>
		public static void InsertAtAfterCallByName(
				UnifiedElement root, string name, UnifiedBlock advice) {
			InsertAtAfterCall(root, new Regex("^" + name + "$"), advice);
		}

		public override string PointcutName {
			get { return "call"; }
		}

		public override void Before(UnifiedElement model, AspectElement.Pointcut target, UnifiedBlock advice) {
			InsertAtBeforeCallByName(model, target.GetTargetName().ElementAt(1), advice);
		}

		public override void After(UnifiedElement model, AspectElement.Pointcut target, UnifiedBlock advice) {
			InsertAtAfterCallByName(model, target.GetTargetName().ElementAt(1), advice);
		}

		public override void Around(UnifiedElement model) {
			throw new NotImplementedException();
		}
	}
}
