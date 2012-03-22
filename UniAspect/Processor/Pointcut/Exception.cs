using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Unicoen.Model;

namespace UniAspect.Processor.Pointcut {
	[Export(typeof(CodeProcessor))]
	public class Exception : CodeProcessor{
		public override string PointcutName {
			get { return "exception"; }
		}
		
		public override void Before(UnifiedElement model, AspectElement.Pointcut target, UnifiedBlock advice) {
			var exceptions = model.Descendants<UnifiedCatch>();
			foreach (var e in exceptions) {
				var regex = new Regex("^" + target.GetTargetName().ElementAt(1) + "$");
				var type = e.Types[0].BasicTypeName as UnifiedIdentifier;
				if(type == null)
					continue;
				var m = regex.Match(type.Name);
				if (m.Success) {
					//アドバイスを対象関数に合成する
					e.Body.Insert(0, advice.DeepCopy());
				}
			}
		}

		public override void After(UnifiedElement model, AspectElement.Pointcut target, UnifiedBlock advice) {
			throw new NotImplementedException();
		}

		public override void Around(UnifiedElement model) {
			throw new NotImplementedException();
		}
	}
}
