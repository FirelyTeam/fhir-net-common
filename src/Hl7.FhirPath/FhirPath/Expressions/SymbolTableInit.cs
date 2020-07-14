/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Utility;
using Hl7.FhirPath.FhirPath.Functions;
using Hl7.FhirPath.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hl7.FhirPath.Expressions
{
    public static class SymbolTableInit
    {
        public static SymbolTable AddStandardFP(this SymbolTable t)
        {
            // Functions that operate on the focus, without null propagation
            t.Add("empty", (IEnumerable<object> f) => !f.Any());
            t.Add("exists", (IEnumerable<object> f) => f.Any());

            t.Add("count", (IEnumerable<object> f) => f.Count());
            t.Add("trace", (IEnumerable<ITypedElement> f, string name, EvaluationContext ctx)
                    => f.Trace(name, ctx));

            t.Add("allTrue", (IEnumerable<ITypedElement> f) => f.All(e => e.Value as bool? == true));
            t.Add("anyTrue", (IEnumerable<ITypedElement> f) => f.Any(e => e.Value as bool? == true));
            t.Add("allFalse", (IEnumerable<ITypedElement> f) => f.All(e => e.Value as bool? == false));
            t.Add("anyFalse", (IEnumerable<ITypedElement> f) => f.Any(e => e.Value as bool? == false));
            t.Add("combine", (IEnumerable<ITypedElement> l, IEnumerable<ITypedElement> r) => l.Concat(r));
            t.Add("binary.|", (object f, IEnumerable<ITypedElement> l, IEnumerable<ITypedElement> r) => l.DistinctUnion(r));
            t.Add("union", (IEnumerable<ITypedElement> l, IEnumerable<ITypedElement> r) => l.DistinctUnion(r));
            t.Add("binary.contains", (object f, IEnumerable<ITypedElement> a, ITypedElement b) => a.Contains(b));
            t.Add("binary.in", (object f, ITypedElement a, IEnumerable<ITypedElement> b) => b.Contains(a));
            t.Add("distinct", (IEnumerable<ITypedElement> f) => f.Distinct());
            t.Add("isDistinct", (IEnumerable<ITypedElement> f) => f.IsDistinct());
            t.Add("subsetOf", (IEnumerable<ITypedElement> f, IEnumerable<ITypedElement> a) => f.SubsetOf(a));
            t.Add("supersetOf", (IEnumerable<ITypedElement> f, IEnumerable<ITypedElement> a) => a.SubsetOf(f));
            t.Add("intersect", (IEnumerable<ITypedElement> f, IEnumerable<ITypedElement> a) => f.Intersect(a));
            t.Add("exclude", (IEnumerable<ITypedElement> f, IEnumerable<ITypedElement> a) => f.Exclude(a));

            t.Add("today", (object f) => PartialDate.Today());
            t.Add("now", (object f) => PartialDateTime.Now());
            t.Add("timeOfDay", (object f) => PartialTime.Now());

            t.Add("binary.&", (object f, string a, string b) => (a ?? "") + (b ?? ""));

            t.Add(new CallSignature("iif", typeof(IEnumerable<ITypedElement>), typeof(object), typeof(bool?), typeof(Invokee), typeof(Invokee)), runIif);
            t.Add(new CallSignature("iif", typeof(IEnumerable<ITypedElement>), typeof(object), typeof(bool?), typeof(Invokee)), runIif);

            // Functions that use normal null propagation and work with the focus (buy may ignore it)
            t.Add("not", (IEnumerable<ITypedElement> f) => f.Not(), doNullProp: true);
            t.Add("builtin.children", (IEnumerable<ITypedElement> f, string a) => f.Navigate(a), doNullProp: true);

            t.Add("children", (IEnumerable<ITypedElement> f) => f.Children(), doNullProp: true);
            t.Add("descendants", (IEnumerable<ITypedElement> f) => f.Descendants(), doNullProp: true);

            t.Add("binary.=", (object f, IEnumerable<ITypedElement> a, IEnumerable<ITypedElement> b) => a.IsEqualTo(b), doNullProp: true);
            t.Add("binary.!=", (object f, IEnumerable<ITypedElement> a, IEnumerable<ITypedElement> b) => !a.IsEqualTo(b), doNullProp: true);
            t.Add("binary.~", (object f, IEnumerable<ITypedElement> a, IEnumerable<ITypedElement> b) => a.IsEquivalentTo(b), doNullProp: true);
            t.Add("binary.!~", (object f, IEnumerable<ITypedElement> a, IEnumerable<ITypedElement> b) => !a.IsEquivalentTo(b), doNullProp: true);

            t.Add("unary.-", (object f, long a) => -a, doNullProp: true);
            t.Add("unary.-", (object f, decimal a) => -a, doNullProp: true);
            t.Add("unary.-", (object f, Quantity a) => new Quantity(-a.Value, a.Unit), doNullProp: true);
            t.Add("unary.+", (object f, long a) => a, doNullProp: true);
            t.Add("unary.+", (object f, decimal a) => a, doNullProp: true);
            t.Add("unary.+", (object f, Quantity a) => a, doNullProp: true);

            t.Add("binary.*", (object f, long a, long b) => a * b, doNullProp: true);
            t.Add("binary.*", (object f, decimal a, decimal b) => a * b, doNullProp: true);
            t.Add("binary.*", (object f, Quantity a, Quantity b) => a * b, doNullProp: true);

            t.Add("binary./", (object f, decimal a, decimal b) => a / b, doNullProp: true);
            t.Add("binary./", (object f, Quantity a, Quantity b) => a / b, doNullProp: true);

            t.Add("binary.+", (object f, long a, long b) => a + b, doNullProp: true);
            t.Add("binary.+", (object f, decimal a, decimal b) => a + b, doNullProp: true);
            t.Add("binary.+", (object f, string a, string b) => a + b, doNullProp: true);
            t.Add("binary.+", (object f, Quantity a, Quantity b) => a + b, doNullProp: true);

            t.Add("binary.-", (object f, long a, long b) => a - b, doNullProp: true);
            t.Add("binary.-", (object f, decimal a, decimal b) => a - b, doNullProp: true);
            t.Add("binary.-", (object f, Quantity a, Quantity b) => a - b, doNullProp: true);

            t.Add("binary.div", (object f, long a, long b) => a / b, doNullProp: true);
            t.Add("binary.div", (object f, decimal a, decimal b) => (long)Math.Truncate(a / b), doNullProp: true);

            t.Add("binary.mod", (object f, long a, long b) => a % b, doNullProp: true);
            t.Add("binary.mod", (object f, decimal a, decimal b) => a % b, doNullProp: true);

            t.Add("binary.>", (object f, long a, long b) => a > b, doNullProp: true);
            t.Add("binary.>", (object f, decimal a, decimal b) => a > b, doNullProp: true);
            t.Add("binary.>", (object f, string a, string b) => string.CompareOrdinal(a, b) > 0, doNullProp: true);
            t.Add("binary.>", (object f, PartialDateTime a, PartialDateTime b) => a > b, doNullProp: true);
            t.Add("binary.>", (object f, PartialTime a, PartialTime b) => a > b, doNullProp: true);
            t.Add("binary.>", (object f, PartialDate a, PartialDate b) => a > b, doNullProp: true);
            t.Add("binary.>", (object f, Quantity a, Quantity b) => a > b, doNullProp: true);

            t.Add("binary.<", (object f, long a, long b) => a < b, doNullProp: true);
            t.Add("binary.<", (object f, decimal a, decimal b) => a < b, doNullProp: true);
            t.Add("binary.<", (object f, string a, string b) => string.CompareOrdinal(a, b) < 0, doNullProp: true);
            t.Add("binary.<", (object f, PartialDateTime a, PartialDateTime b) => a < b, doNullProp: true);
            t.Add("binary.<", (object f, PartialTime a, PartialTime b) => a < b, doNullProp: true);
            t.Add("binary.<", (object f, PartialDate a, PartialDate b) => a < b, doNullProp: true);
            t.Add("binary.<", (object f, Quantity a, Quantity b) => a < b, doNullProp: true);

            t.Add("binary.<=", (object f, long a, long b) => a <= b, doNullProp: true);
            t.Add("binary.<=", (object f, decimal a, decimal b) => a <= b, doNullProp: true);
            t.Add("binary.<=", (object f, string a, string b) => string.CompareOrdinal(a, b) <= 0, doNullProp: true);
            t.Add("binary.<=", (object f, PartialDateTime a, PartialDateTime b) => a <= b, doNullProp: true);
            t.Add("binary.<=", (object f, PartialTime a, PartialTime b) => a <= b, doNullProp: true);
            t.Add("binary.<=", (object f, PartialDate a, PartialDate b) => a <= b, doNullProp: true);
            t.Add("binary.<=", (object f, Quantity a, Quantity b) => a <= b, doNullProp: true);

            t.Add("binary.>=", (object f, long a, long b) => a >= b, doNullProp: true);
            t.Add("binary.>=", (object f, decimal a, decimal b) => a >= b, doNullProp: true);
            t.Add("binary.>=", (object f, string a, string b) => string.CompareOrdinal(a, b) >= 0, doNullProp: true);
            t.Add("binary.>=", (object f, PartialDateTime a, PartialDateTime b) => a >= b, doNullProp: true);
            t.Add("binary.>=", (object f, PartialTime a, PartialTime b) => a >= b, doNullProp: true);
            t.Add("binary.>=", (object f, PartialDate a, PartialDate b) => a >= b, doNullProp: true);
            t.Add("binary.>=", (object f, Quantity a, Quantity b) => a >= b, doNullProp: true);

            t.Add("single", (IEnumerable<ITypedElement> f) => f.Single(), doNullProp: true);
            t.Add("skip", (IEnumerable<ITypedElement> f, long a) => f.Skip((int)a), doNullProp: true);
            t.Add("first", (IEnumerable<ITypedElement> f) => f.First(), doNullProp: true);
            t.Add("last", (IEnumerable<ITypedElement> f) => f.Last(), doNullProp: true);
            t.Add("tail", (IEnumerable<ITypedElement> f) => f.Tail(), doNullProp: true);
            t.Add("take", (IEnumerable<ITypedElement> f, long a) => f.Take((int)a), doNullProp: true);
            t.Add("builtin.item", (IEnumerable<ITypedElement> f, long a) => f.Item((int)a), doNullProp: true);

            t.Add("toBoolean", (ITypedElement f) => f.ToBoolean(), doNullProp: true);
            t.Add("convertsToBoolean", (ITypedElement f) => f.ConvertsToBoolean(), doNullProp: true);
            t.Add("toInteger", (ITypedElement f) => f.ToInteger(), doNullProp: true);
            t.Add("convertsToInteger", (ITypedElement f) => f.ConvertsToInteger(), doNullProp: true);
            t.Add("toDecimal", (ITypedElement f) => f.ToDecimal(), doNullProp: true);
            t.Add("convertsToDecimal", (ITypedElement f) => f.ConvertsToDecimal(), doNullProp: true);
            t.Add("toDateTime", (ITypedElement f) => f.ToDateTime(), doNullProp: true);
            t.Add("convertsToDateTime", (ITypedElement f) => f.ConvertsToDateTime(), doNullProp: true);
            t.Add("toTime", (ITypedElement f) => f.ToTime(), doNullProp: true);
            t.Add("convertsToTime", (ITypedElement f) => f.ConvertsToTime(), doNullProp: true);
            t.Add("toDate", (ITypedElement f) => f.ToDate(), doNullProp: true);
            t.Add("convertsToDate", (ITypedElement f) => f.ConvertsToDate(), doNullProp: true);
            t.Add("toString", (ITypedElement f) => f.ToStringRepresentation(), doNullProp: true);
            t.Add("convertsToString", (ITypedElement f) => f.ConvertsToString(), doNullProp: true);
            t.Add("toQuantity", (ITypedElement f) => f.ToQuantity(), doNullProp: true);
            t.Add("convertsToQuantity", (ITypedElement f) => f.ConvertsToQuantity(), doNullProp: true);

            t.Add("upper", (string f) => f.ToUpper(), doNullProp: true);
            t.Add("lower", (string f) => f.ToLower(), doNullProp: true);
            t.Add("toChars", (string f) => f.ToChars(), doNullProp: true);
            t.Add("substring", (string f, long a) => f.FpSubstring((int)a, null), doNullProp: true);
            //special case: only focus should be Null propagated:
            t.Add(new CallSignature("substring", typeof(string), typeof(string), typeof(long), typeof(long?)),
                InvokeeFactory.WrapWithPropNullForFocus((string f, long a, long? b) => f.FpSubstring((int)a, (int?)b)));
            t.Add("startsWith", (string f, string fragment) => f.StartsWith(fragment), doNullProp: true);
            t.Add("endsWith", (string f, string fragment) => f.EndsWith(fragment), doNullProp: true);
            t.Add("matches", (string f, string regex) => Regex.IsMatch(f, regex), doNullProp: true);
            t.Add("indexOf", (string f, string fragment) => f.FpIndexOf(fragment), doNullProp: true);
            t.Add("contains", (string f, string fragment) => f.Contains(fragment), doNullProp: true);
            t.Add("replaceMatches", (string f, string regex, string subst) => Regex.Replace(f, regex, subst), doNullProp: true);
            t.Add("replace", (string f, string regex, string subst) => f.FpReplace(regex, subst), doNullProp: true);
            t.Add("length", (string f) => f.Length, doNullProp: true);
            t.Add("split", (string f, string seperator) => f.FpSplit(seperator), doNullProp: true);

            // Math functions
            t.Add("abs", (decimal f) => Math.Abs(f), doNullProp: true);
            t.Add("abs", (Quantity f) => new Quantity(Math.Abs(f.Value), f.Unit), doNullProp: true);
            t.Add("ceiling", (decimal f) => Math.Ceiling(f), doNullProp: true);
            t.Add("exp", (decimal f) => Math.Exp(Convert.ToDouble(f)), doNullProp: true);
            t.Add("floor", (decimal f) => Math.Floor(f), doNullProp: true);
            t.Add("ln", (decimal f) => Math.Log(Convert.ToDouble(f)), doNullProp: true);
            t.Add("log", (decimal f, decimal @base) => Math.Log(Convert.ToDouble(f), Convert.ToDouble(@base)), doNullProp: true);
            t.Add("power", (decimal f, decimal exponent) => f.Power(exponent), doNullProp: true);
            t.Add("round", (decimal f, long precision) => Math.Round(f, Convert.ToInt32(precision)), doNullProp: true);
            t.Add("round", (decimal f) => Math.Round(f), doNullProp: true);
            t.Add("sqrt", (decimal f) => f.Sqrt(), doNullProp: true);
            t.Add("truncate", (decimal f) => Math.Truncate(Convert.ToDouble(f)), doNullProp: true);

            // The next two functions existed pre-normative, so we have kept them.
            t.Add("is", (ITypedElement f, string name) => f.Is(name), doNullProp: true);
            t.Add("as", (IEnumerable<ITypedElement> f, string name) => f.FilterType(name), doNullProp: true);

            t.Add("ofType", (IEnumerable<ITypedElement> f, string name) => f.FilterType(name), doNullProp: true);
            t.Add("binary.is", (object f, ITypedElement left, string name) => left.Is(name), doNullProp: true);
            t.Add("binary.as", (object f, ITypedElement left, string name) => left.CastAs(name), doNullProp: true);

            // Kept for backwards compatibility, but no longer part of the spec
            t.Add("binary.as", (object f, IEnumerable<ITypedElement> left, string name) => left.FilterType(name), doNullProp: true);

            t.Add("extension", (IEnumerable<ITypedElement> f, string url) => f.Extension(url), doNullProp: true);

            // Logic operators do not use null propagation and may do short-cut eval
            t.AddLogic("binary.and", (a, b) => a.And(b));
            t.AddLogic("binary.or", (a, b) => a.Or(b));
            t.AddLogic("binary.xor", (a, b) => a.XOr(b));
            t.AddLogic("binary.implies", (a, b) => a.Implies(b));

            // Special late-bound functions
            t.Add(new CallSignature("where", typeof(IEnumerable<ITypedElement>), typeof(object), typeof(Invokee)), runWhere);
            t.Add(new CallSignature("select", typeof(IEnumerable<ITypedElement>), typeof(object), typeof(Invokee)), runSelect);
            t.Add(new CallSignature("all", typeof(bool), typeof(object), typeof(Invokee)), runAll);
            t.Add(new CallSignature("any", typeof(bool), typeof(object), typeof(Invokee)), runAny);
            t.Add(new CallSignature("exists", typeof(bool), typeof(object), typeof(Invokee)), runAny);
            t.Add(new CallSignature("repeat", typeof(IEnumerable<ITypedElement>), typeof(object), typeof(Invokee)), runRepeat);
            t.Add(new CallSignature("trace", typeof(IEnumerable<ITypedElement>), typeof(string), typeof(object), typeof(Invokee)), Trace);

            t.AddVar("sct", "http://snomed.info/sct");
            t.AddVar("loinc", "http://loinc.org");
            t.AddVar("ucum", "http://unitsofmeasure.org");

            t.Add("builtin.coreexturl", (object f, string id) => getCoreExtensionUrl(id));
            t.Add("builtin.corevsurl", (object f, string id) => getCoreValueSetUrl(id));

            return t;
        }


        private static string getCoreExtensionUrl(string id)
        {
            return "http://hl7.org/fhir/StructureDefinition/" + id;
        }

        private static string getCoreValueSetUrl(string id)
        {
            return "http://hl7.org/fhir/ValueSet/" + id;
        }

        private static IEnumerable<ITypedElement> Trace(Closure ctx, IEnumerable<Invokee> arguments)
        {
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);
            string name = arguments.Skip(1).First()(ctx, InvokeeFactory.EmptyArgs).FirstOrDefault()?.Value as string;

            List<Invokee> selectArgs = new List<Invokee> { arguments.First() };
            selectArgs.AddRange(arguments.Skip(2));
            var selectResults = runSelect(ctx, selectArgs);
            ctx?.EvaluationContext?.Tracer?.Invoke(name, selectResults);

            return focus;
        }

        private static IEnumerable<ITypedElement> runIif(Closure ctx, IEnumerable<Invokee> arguments)
        {
            // iif(criterion: expression, true-result: collection [, otherwise-result: collection]) : collection
            // note: short-circuit behavior is expected in this function
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);

            var expression = arguments.Skip(1).First()(ctx, InvokeeFactory.EmptyArgs);
            var trueResult = arguments.Skip(2).First();
            var otherResult = arguments.Skip(3).FirstOrDefault();

            if (expression.Count() > 1)
                throw Error.InvalidOperation($"Result of {nameof(expression)} is not of type boolean");

            return (expression.BooleanEval() ?? false)
                ? trueResult(ctx, InvokeeFactory.EmptyArgs)
                : otherResult == null ? ElementNode.EmptyList : otherResult(ctx, InvokeeFactory.EmptyArgs);
        }

        private static IEnumerable<ITypedElement> runWhere(Closure ctx, IEnumerable<Invokee> arguments)
        {
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);
            var lambda = arguments.Skip(1).First();

            foreach (ITypedElement element in focus)
            {
                var newFocus = ElementNode.CreateList(element);
                var newContext = ctx.Nest(newFocus);
                newContext.SetThis(newFocus);

                if (lambda(newContext, InvokeeFactory.EmptyArgs).BooleanEval() == true)
                    yield return element;
            }
        }

        private static IEnumerable<ITypedElement> runSelect(Closure ctx, IEnumerable<Invokee> arguments)
        {
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);
            var lambda = arguments.Skip(1).First();

            foreach (ITypedElement element in focus)
            {
                var newFocus = ElementNode.CreateList(element);
                var newContext = ctx.Nest(newFocus);
                newContext.SetThis(newFocus);

                var result = lambda(newContext, InvokeeFactory.EmptyArgs);
                foreach (var resultElement in result)       // implement SelectMany()
                    yield return resultElement;
            }
        }

        private static IEnumerable<ITypedElement> runRepeat(Closure ctx, IEnumerable<Invokee> arguments)
        {
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);
            var lambda = arguments.Skip(1).First();

            var fullResult = new List<ITypedElement>();
            List<ITypedElement> newNodes = new List<ITypedElement>(focus);

            while (newNodes.Any())
            {
                var current = newNodes;
                newNodes = new List<ITypedElement>();

                foreach (ITypedElement element in current)
                {
                    var newFocus = ElementNode.CreateList(element);
                    var newContext = ctx.Nest(newFocus);
                    newContext.SetThis(newFocus);


                    newNodes.AddRange(lambda(newContext, InvokeeFactory.EmptyArgs));
                }

                fullResult.AddRange(newNodes);
            }
            return fullResult;
        }

        private static IEnumerable<ITypedElement> runAll(Closure ctx, IEnumerable<Invokee> arguments)
        {
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);
            var lambda = arguments.Skip(1).First();

            foreach (ITypedElement element in focus)
            {
                var newFocus = ElementNode.CreateList(element);
                var newContext = ctx.Nest(newFocus);
                newContext.SetThis(newFocus);

                var result = lambda(newContext, InvokeeFactory.EmptyArgs).BooleanEval();
                if (result == null) return ElementNode.EmptyList;
                if (result == false) return ElementNode.CreateList(false);
            }

            return ElementNode.CreateList(true);
        }

        private static IEnumerable<ITypedElement> runAny(Closure ctx, IEnumerable<Invokee> arguments)
        {
            var focus = arguments.First()(ctx, InvokeeFactory.EmptyArgs);
            var lambda = arguments.Skip(1).First();

            foreach (ITypedElement element in focus)
            {
                var newFocus = ElementNode.CreateList(element);
                var newContext = ctx.Nest(newFocus);
                newContext.SetThis(newFocus);


                var result = lambda(newContext, InvokeeFactory.EmptyArgs).BooleanEval();

                //if (result == null) return ElementNode.EmptyList; -> otherwise this would not be where().exists()
                //Patient.identifier.any(use = 'official') would return {} if ANY identifier has no 'use' element. Unexpected behaviour, I think
                if (result == true) return ElementNode.CreateList(true);
            }

            return ElementNode.CreateList(false);
        }
    }
}
