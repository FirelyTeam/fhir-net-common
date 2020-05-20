﻿/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */


using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Introspection;

namespace Hl7.Fhir.Serialization
{
    internal class PocoBuilder : IExceptionSource
    {
        public PocoBuilder(IStructureDefinitionSummaryProvider inspector, PocoBuilderSettings settings = null)
        {
            _settings = settings?.Clone() ?? new PocoBuilderSettings();

            // EK, 20200519 - Obviously, this code should be replaced when we have a better solution for how
            // to work with multiple types of IStructureDefinitionSummaryProvider.
            _inspector = inspector as VersionAwarePocoStructureDefinitionSummaryProvider ??
                    throw new ArgumentException($"PocoBuilder can only work with a {nameof(IStructureDefinitionSummaryProvider)} " +
                    $"that are instances of {nameof(VersionAwarePocoStructureDefinitionSummaryProvider)}");
        }

        private readonly PocoBuilderSettings _settings;
        private readonly VersionAwarePocoStructureDefinitionSummaryProvider _inspector;

        public ExceptionNotificationHandler ExceptionHandler { get; set; }

        /// <summary>
        /// Build a POCO from an ISourceNode.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dataType">Optional. Type of POCO to build. Should be one of the generated POCO classes.</param>
        /// <returns></returns>
        /// <remarks>If <paramref name="dataType"/> is not supplied, or is <code>Resource</code> or <code>DomainResource</code>, 
        /// the builder will try to determine the actual type to create from the <paramref name="source"/>. </remarks>
        public Base BuildFrom(ISourceNode source, Type dataType = null)
        {
            if (source == null) throw Error.ArgumentNull(nameof(source));
            
            return BuildFrom(source, dataType != null ?
                    findMappingOrReport(dataType) : null);

            ClassMapping findMappingOrReport(Type t)
            {
                var typeFound = _inspector.FindClassMappingByType(dataType);

                if (typeFound == null)
                {
                    ExceptionHandler.NotifyOrThrow(this,
                        ExceptionNotification.Error(
                            new StructuralTypeException($"While building a POCO: The .NET type '{dataType.Name}' does not represent a FHIR type.")));

                    return null;
                }

                return typeFound;
            }
        }

        /// <summary>
        /// Build a POCO from an ISourceNode.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mapping">Optional. The <see cref="ClassMapping" /> for the POCO to build. </param>
        /// <returns></returns>
        /// <remarks>If <paramref name="mapping"/> is not supplied, or is <code>Resource</code> or <code>DomainResource</code>, 
        /// the builder will try to determine the actual type to create from the <paramref name="source"/>. </remarks>
        public Base BuildFrom(ISourceNode source, ClassMapping mapping = null)
        {
            if (source == null) throw Error.ArgumentNull(nameof(source));
            TypedElementSettings typedSettings = new TypedElementSettings
            {
                ErrorMode = _settings.IgnoreUnknownMembers ?
                    TypedElementSettings.TypeErrorMode.Ignore
                    : TypedElementSettings.TypeErrorMode.Report
            };

            string dataType;
            // If dataType is an abstract resource superclass -> ToTypedElement(with type=null) will figure it out;
            if (mapping == null)
                dataType = null;
            else if (mapping.NativeType.IsAbstract)
                dataType = null;
            else
                dataType = mapping.Name;

            var typedSource = source.ToTypedElement(_inspector, dataType, typedSettings);

            return BuildFrom(typedSource);
        }

        /// <summary>
        /// Build a POCO from an ITypedElement.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Base BuildFrom(ITypedElement source)
        {
            if (source == null) throw Error.ArgumentNull(nameof(source));

            if (source is IExceptionSource)
            {
                using (source.Catch((o, a) => ExceptionHandler.NotifyOrThrow(o, a)))
                {
                    return build();
                }
            }
            else
                return build();

            Base build()
            {
                var settings = new ParserSettings
                {
                    AcceptUnknownMembers = _settings.IgnoreUnknownMembers,
                    AllowUnrecognizedEnums = _settings.AllowUnrecognizedEnums
                };

                return new ComplexTypeReader(_inspector, source, settings).Deserialize(null);
            }
        }
    }
}

