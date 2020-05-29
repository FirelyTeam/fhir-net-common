﻿/* 
 * Copyright (c) 2018, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TS = Hl7.Fhir.Language.TypeSpecifier;

namespace Hl7.Fhir.ElementModel
{
    public class ElementNode : DomNode<ElementNode>, ITypedElement, IAnnotated, IAnnotatable, IShortPathGenerator
    {
        /// <summary>
        /// Creates an implementation of ITypedElement that represents a primitive value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // HACK: For now, allow a Quantity (which is NOT a primitive) in the .Value property
        // of ITypedElement. This is a temporary situation to make a quick & dirty upgrade of
        // FP to Normative (with Quantity support) possible.
        public static ITypedElement ForPrimitive(object value)
        {
            return value switch
            {
                Model.Primitives.Quantity q => PrimitiveElement.ForQuantity(q),
                _ => new PrimitiveElement(value, useFullTypeName:true)
            };
        }

        /// <summary>
        /// Create a fixed length set of values (but also support variable number of parameter values)
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<ITypedElement> CreateList(params object[] values) => 
            values != null
                ? values.Select(value => value == null 
                    ? null 
                    : value is ITypedElement element
                        ? element 
                        : ForPrimitive(value))
                : EmptyList;

        /// <summary>
        /// Create a variable list of values using an enumeration
        /// - so doesn't have to be converted to an array in memory (issue with larger dynamic lists)
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<ITypedElement> CreateList(IEnumerable<object> values) => values != null
                ? values.Select(value => value == null ? null : value is ITypedElement element ? element : ForPrimitive(value))
                : EmptyList;

        public static readonly IEnumerable<ITypedElement> EmptyList = Enumerable.Empty<ITypedElement>();
        public IEnumerable<ITypedElement> Children(string name = null) => ChildrenInternal(name);

        internal ElementNode(string name, object value, string instanceType, IElementDefinitionSummary definition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            InstanceType = instanceType;
            Value = value;
            Definition = definition;
        }

        private IReadOnlyCollection<IElementDefinitionSummary> _childDefinitions = null;

        private IReadOnlyCollection<IElementDefinitionSummary> getChildDefinitions(IStructureDefinitionSummaryProvider provider)
        {
            LazyInitializer.EnsureInitialized(ref _childDefinitions, () => this.ChildDefinitions(provider));

            return _childDefinitions;
        }

        public ElementNode Add(IStructureDefinitionSummaryProvider provider, ElementNode child, string name = null)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (child == null) throw new ArgumentNullException(nameof(child));

            importChild(provider, child, name);
            return child;
        }

        /// <summary>
        /// Adds a child to this instance. Be careful: this will not add type information to the child. 
        /// Use <see cref="Add(IStructureDefinitionSummaryProvider, ElementNode, string)"/> instead
        /// </summary>
        /// <param name="child"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal ElementNode Add(ElementNode child, string name = null)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            importChild(null, child, name);
            return child;
        }

        public ElementNode Add(IStructureDefinitionSummaryProvider provider, string name, object value = null, string instanceType = null)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var child = new ElementNode(name, value, instanceType, null);

            // Add() will supply the definition and the instanceType (if necessary)
            return Add(provider, child);
        }

        /// <summary>
        /// Adds a child to this instance. Be careful: this will not add type information to the child. 
        /// Use <see cref="Add(IStructureDefinitionSummaryProvider, string, object, string)"/> instead
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        internal ElementNode Add(string name, object value = null, string instanceType = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var child = new ElementNode(name, value, instanceType, null);

            return Add(child);
        }

        public void ReplaceWith(IStructureDefinitionSummaryProvider provider, ElementNode node)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (Parent == null) throw Error.Argument("Current node is a root node and cannot be replaced.");
            Parent.Replace(provider, this, node);
        }

        public void Replace(IStructureDefinitionSummaryProvider provider, ElementNode oldChild, ElementNode newChild)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (oldChild == null) throw new ArgumentNullException(nameof(oldChild));
            if (newChild == null) throw new ArgumentNullException(nameof(newChild));

            int childIndex = ChildList.IndexOf(oldChild);
            if (childIndex == -1) throw Error.Argument("Node to be replaced is not one of the children of this node");
            importChild(provider, newChild, oldChild.Name, childIndex);
            Remove(oldChild);
        }

        /// <summary>
        /// Will update the child to reflect it being a child of this element, but will not yet add the child at any position within this element
        /// </summary>
        private void importChild(IStructureDefinitionSummaryProvider provider, ElementNode child, string name, int? position = null)
        {
            child.Name = name ?? child.Name;
            if (child.Name == null) throw Error.Argument($"The ElementNode given should have its Name property set or the '{nameof(name)}' parameter should be given.");

            // Remove this child from the current parent (if any), then reassign to me
            if (child.Parent != null) Parent.Remove(child);
            child.Parent = this;

            if (provider != null)
            {
                // If we add a child, we better overwrite it's definition with what
                // we think it should be - this way you can safely first create a node representing
                // an independently created root for a resource of datatype, and then add it to the tree.
                var childDefs = getChildDefinitions(provider);
                var childDef = childDefs.Where(cd => cd.ElementName == child.Name).SingleOrDefault();

                child.Definition = childDef ?? child.Definition;    // if we don't know about the definition, stick with the old one (if any)
            }

            if (child.InstanceType == null && child.Definition != null)
            {
                if (child.Definition.IsResource || child.Definition.Type.Length > 1)
                {
                    // [EK20190822] This functionality has been removed since it heavily depends on knowledge about
                    // FHIR types, it would automatically try to derive a *FHIR* type from the given child.Value,
                    // however, this would not work correctly if the model used is something else than FHIR, 
                    // so this cannot be expected to work correctly in general, and I have chosen to remove
                    // this.
                    //// We are in a situation where we are on an polymorphic element, but the caller did not specify
                    //// the instance type.  We can try to auto-set it by deriving it from the instance's type, if it is a primitive
                    //if (child.Value != null && IsSupportedValue(child.Value))
                    //    child.InstanceType = TypeSpecifier.ForNativeType(child.Value.GetType()).Name;
                    //else
                        throw Error.Argument("The ElementNode given should have its InstanceType property set, since the element is a choice or resource.");
                }
                else
                    child.InstanceType = child.Definition.Type.Single().GetTypeName();
            }

            if (position == null || position >= ChildList.Count)
                ChildList.Add(child);
            else
                ChildList.Insert(position.Value, child);

        }

        /// <summary>
        /// Creates a Root element. Be careful: This method will not provide type information to the node. 
        /// Use <see cref="Root(IStructureDefinitionSummaryProvider, string, string, object)"/> instead.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static ElementNode Root(string type, string name = null, object value = null)
        {
            if (type == null) throw Error.ArgumentNull(nameof(type));
            return new ElementNode(name ?? type, value, type, null);
        }

        public static ElementNode Root(IStructureDefinitionSummaryProvider provider, string type, string name = null, object value = null)
        {
            if (provider == null) throw Error.ArgumentNull(nameof(provider));
            if (type == null) throw Error.ArgumentNull(nameof(type));

            var sd = provider.Provide(type);
            IElementDefinitionSummary definition = null;

            // Should we throw if type is not found?
            if (sd != null)
                definition = ElementDefinitionSummary.ForRoot(sd);

            return new ElementNode(name ?? type, value, type, definition);
        }

        public static ElementNode FromElement(ITypedElement node, bool recursive = true, IEnumerable<Type> annotationsToCopy = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return buildNode(node, recursive, annotationsToCopy, null);
        }

        private static ElementNode buildNode(ITypedElement node, bool recursive, IEnumerable<Type> annotationsToCopy, ElementNode parent)
        {
            var me = new ElementNode(node.Name, node.Value, node.InstanceType, node.Definition)
            {
                Parent = parent
            };

            foreach (var t in annotationsToCopy ?? Enumerable.Empty<Type>())
                foreach (var ann in node.Annotations(t))
                    me.AddAnnotation(ann);

            if (recursive)
                me.ChildList.AddRange(node.Children().Select(c => buildNode(c, recursive: true, annotationsToCopy: annotationsToCopy, me)));

            return me;
        }

        public bool Remove(ElementNode child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            var success = ChildList.Remove(child);
            if (success) child.Parent = null;

            return success;
        }

        [Obsolete("The Clone() function actually only performs a shallow copy, so this function has been renamed to ShallowCopy()")]
        public ElementNode Clone() => ShallowCopy();

        public ElementNode ShallowCopy()
        {
            var copy = new ElementNode(Name, Value, InstanceType, Definition)
            {
                Parent = Parent,
                ChildList = ChildList
            };

            if (HasAnnotations)
                copy.AnnotationsInternal.AddRange(AnnotationsInternal);

            return copy;
        }

        public IElementDefinitionSummary Definition { get; private set; }

        public string InstanceType { get; private set; }

        public object Value { get; set; }

        public IEnumerable<object> Annotations(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return (type == typeof(ElementNode) || type == typeof(ITypedElement) || type == typeof(IShortPathGenerator))
                ? (new[] { this })
                : AnnotationsInternal.OfType(type);
        }

        public string Location
        {
            get
            {
                if (Parent != null)
                {
                    //TODO: Slow - but since we'll change the use of this property to informational 
                    //(i.e. for error messages), it may not be necessary to improve it.
                    var basePath = Parent.Location;
                    var myIndex = Parent.ChildList.Where(c => c.Name == Name).ToList().IndexOf(this);
                    return $"{basePath}.{Name}[{myIndex}]";

                }
                else
                    return Name;
            }
        }

        public string ShortPath
        {
            get
            {
                if (Parent != null)
                {
                    //TODO: Slow - but since we'll change the use of this property to informational 
                    //(i.e. for error messages), it may not be necessary to improve it.
                    var basePath = Parent.ShortPath;

                    if (Definition?.IsCollection == false)
                        return $"{basePath}.{Name}";
                    else
                    {
                        var myIndex = Parent.ChildList.Where(c => c.Name == Name).ToList().IndexOf(this);
                        return $"{basePath}.{Name}[{myIndex}]";
                    }
                }
                else
                    return Name;
            }
        }
    }
}
