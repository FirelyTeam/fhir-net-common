﻿using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace Hl7.Fhir.Validation.Impl
{
    public enum MinMax
    {
        [EnumLiteral("MinValue"), Description("Minimum Value")]
        MinValue,
        [EnumLiteral("MaxValue"), Description("Maximum Value")]
        MaxValue
    }

    public class MinMaxValue : SimpleAssertion
    {
        private readonly ITypedElement _minMaxValue;
        private readonly MinMax _minMaxType;
        private readonly string _key;

        public MinMaxValue(ITypedElement minMaxValue, MinMax minMaxType)
        {
            _minMaxValue = minMaxValue ?? throw new IncorrectElementDefinitionException($"{nameof(minMaxValue)} cannot be null");
            _minMaxType = minMaxType;

            _key = $"{_minMaxType.GetLiteral().Uncapitalize()}[x]";

            // Min/max are only defined for ordered types
            if (!IsOrderedType(_minMaxValue.InstanceType))
            {
                throw new IncorrectElementDefinitionException($"{_minMaxValue.Name} was given in ElementDefinition, but type '{_minMaxValue.InstanceType}' is not an ordered type");
            }
        }

        public MinMaxValue(int minMaxValue, MinMax minMaxType) : this(ElementNode.ForPrimitive(minMaxValue), minMaxType) { }

        protected override string Key => _key;

        protected override object Value => _minMaxValue;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            var comparisonOutcome = _minMaxType == MinMax.MinValue ? -1 : 1;

            // TODO : what to do if Value is not IComparable?
            if (input.Value is IComparable instanceValue)
            {
                if (instanceValue.CompareTo(_minMaxValue.Value) == comparisonOutcome)
                {
                    var label = comparisonOutcome == -1 ? "smaller than" :
                                    comparisonOutcome == 0 ? "equal to" :
                                        "larger than";

                    return new Assertions(Assertions.Failure + new TraceText($"Value '{instanceValue}' is {label} {_minMaxValue.Value})"));
                }
            }

            return Assertions.Success;
        }

        public override JToken ToJson()
        {
            return new JProperty(Key, (Value as ITypedElement)?.Value.ToString());
        }

        /// <summary>
        /// TODO Validation: this should be altered and moved to a more generic place, and should be more sophisticated
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsOrderedType(string type)
        {
            switch (type)
            {
                case "date":
                case "dateTime":
                case "instant":
                case "time":
                case "decimal":
                case "integer":
                case "positiveInt":
                case "unsignedInt":
                case "Quantity": return true;
                default:
                    return false;
            }
        }
    }
}