﻿using System;
using System.Collections.Generic;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Validation;
using System.Linq;
using System.Runtime.Serialization;

/*
  Copyright (c) 2011-2013, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  

*/

//
// Generated on Tue, Apr 15, 2014 17:48+0200 for FHIR v0.80
//
namespace Hl7.Fhir.Model
{
    /// <summary>
    /// A series of measurements taken by a device
    /// </summary>
    [FhirType("SampledData")]
    [DataContract]
    public partial class SampledData : Hl7.Fhir.Model.Element, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Zero value and units
        /// </summary>
        [FhirElement("origin", InSummary=true, Order=40)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Hl7.Fhir.Model.Quantity Origin
        {
            get { return _Origin; }
            set { _Origin = value; OnPropertyChanged("Origin"); }
        }
        private Hl7.Fhir.Model.Quantity _Origin;
        
        /// <summary>
        /// Number of milliseconds between samples
        /// </summary>
        [FhirElement("period", InSummary=true, Order=50)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Hl7.Fhir.Model.FhirDecimal PeriodElement
        {
            get { return _PeriodElement; }
            set { _PeriodElement = value; OnPropertyChanged("PeriodElement"); }
        }
        private Hl7.Fhir.Model.FhirDecimal _PeriodElement;
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public decimal? Period
        {
            get { return PeriodElement != null ? PeriodElement.Value : null; }
            set
            {
                if(value == null)
                  PeriodElement = null; 
                else
                  PeriodElement = new Hl7.Fhir.Model.FhirDecimal(value);
                OnPropertyChanged("Period");
            }
        }
        
        /// <summary>
        /// Multiply data by this before adding to origin
        /// </summary>
        [FhirElement("factor", InSummary=true, Order=60)]
        [DataMember]
        public Hl7.Fhir.Model.FhirDecimal FactorElement
        {
            get { return _FactorElement; }
            set { _FactorElement = value; OnPropertyChanged("FactorElement"); }
        }
        private Hl7.Fhir.Model.FhirDecimal _FactorElement;
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public decimal? Factor
        {
            get { return FactorElement != null ? FactorElement.Value : null; }
            set
            {
                if(value == null)
                  FactorElement = null; 
                else
                  FactorElement = new Hl7.Fhir.Model.FhirDecimal(value);
                OnPropertyChanged("Factor");
            }
        }
        
        /// <summary>
        /// Lower limit of detection
        /// </summary>
        [FhirElement("lowerLimit", InSummary=true, Order=70)]
        [DataMember]
        public Hl7.Fhir.Model.FhirDecimal LowerLimitElement
        {
            get { return _LowerLimitElement; }
            set { _LowerLimitElement = value; OnPropertyChanged("LowerLimitElement"); }
        }
        private Hl7.Fhir.Model.FhirDecimal _LowerLimitElement;
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public decimal? LowerLimit
        {
            get { return LowerLimitElement != null ? LowerLimitElement.Value : null; }
            set
            {
                if(value == null)
                  LowerLimitElement = null; 
                else
                  LowerLimitElement = new Hl7.Fhir.Model.FhirDecimal(value);
                OnPropertyChanged("LowerLimit");
            }
        }
        
        /// <summary>
        /// Upper limit of detection
        /// </summary>
        [FhirElement("upperLimit", InSummary=true, Order=80)]
        [DataMember]
        public Hl7.Fhir.Model.FhirDecimal UpperLimitElement
        {
            get { return _UpperLimitElement; }
            set { _UpperLimitElement = value; OnPropertyChanged("UpperLimitElement"); }
        }
        private Hl7.Fhir.Model.FhirDecimal _UpperLimitElement;
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public decimal? UpperLimit
        {
            get { return UpperLimitElement != null ? UpperLimitElement.Value : null; }
            set
            {
                if(value == null)
                  UpperLimitElement = null; 
                else
                  UpperLimitElement = new Hl7.Fhir.Model.FhirDecimal(value);
                OnPropertyChanged("UpperLimit");
            }
        }
        
        /// <summary>
        /// Number of sample points at each time point
        /// </summary>
        [FhirElement("dimensions", InSummary=true, Order=90)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Hl7.Fhir.Model.Integer DimensionsElement
        {
            get { return _DimensionsElement; }
            set { _DimensionsElement = value; OnPropertyChanged("DimensionsElement"); }
        }
        private Hl7.Fhir.Model.Integer _DimensionsElement;
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public int? Dimensions
        {
            get { return DimensionsElement != null ? DimensionsElement.Value : null; }
            set
            {
                if(value == null)
                  DimensionsElement = null; 
                else
                  DimensionsElement = new Hl7.Fhir.Model.Integer(value);
                OnPropertyChanged("Dimensions");
            }
        }
        
        /// <summary>
        /// Decimal values with spaces, or "E" | "U" | "L"
        /// </summary>
        [FhirElement("data", InSummary=true, Order=100)]
        [Cardinality(Min=1,Max=1)]
        [DataMember]
        public Hl7.Fhir.Model.FhirString DataElement
        {
            get { return _DataElement; }
            set { _DataElement = value; OnPropertyChanged("DataElement"); }
        }
        private Hl7.Fhir.Model.FhirString _DataElement;
        
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public string Data
        {
            get { return DataElement != null ? DataElement.Value : null; }
            set
            {
                if(value == null)
                  DataElement = null; 
                else
                  DataElement = new Hl7.Fhir.Model.FhirString(value);
                OnPropertyChanged("Data");
            }
        }
        
    }
    
}
